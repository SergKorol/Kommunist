using System.Globalization;
using FluentAssertions;
using Kommunist.Application.Models;
using Kommunist.Application.ViewModels;
using Kommunist.Core.Services.Interfaces;
using Moq;

namespace Kommunist.Tests.ViewModels;

public class CalConfigViewModelTests
{
    private static CalEvent CreateEvent(string title, DateTime startLocal, TimeSpan duration, string? location = null, string? url = "http://test")
    {
        var start = new DateTimeOffset(DateTime.SpecifyKind(startLocal, DateTimeKind.Local)).ToUnixTimeSeconds();
        var end = new DateTimeOffset(DateTime.SpecifyKind(startLocal.Add(duration), DateTimeKind.Local)).ToUnixTimeSeconds();

        return new CalEvent
        {
            Title = title,
            Start = start,
            End = end,
            DateTime = startLocal,
            Location = location,
            Url = url
        };
    }

    private static CalConfigViewModel CreateSut(
        out Mock<IFileHostingService> fileHosting,
        out Mock<IEmailService> email,
        out Mock<ICoordinatesService> coords,
        out Mock<IToastService> toast,
        out Mock<IFileSaverService> saver,
        out Mock<IFileSystemService> fileSystem,
        out Mock<ILauncherService> launcher)
    {
        fileHosting = new Mock<IFileHostingService>(MockBehavior.Strict);
        email = new Mock<IEmailService>(MockBehavior.Strict);
        coords = new Mock<ICoordinatesService>(MockBehavior.Strict);
        var android = new Mock<IAndroidCalendarService>(MockBehavior.Strict);
        toast = new Mock<IToastService>(MockBehavior.Strict);
        saver = new Mock<IFileSaverService>(MockBehavior.Strict);
        fileSystem = new Mock<IFileSystemService>(MockBehavior.Strict);
        launcher = new Mock<ILauncherService>(MockBehavior.Strict);
        var pageDialog = new Mock<IPageDialogService>(MockBehavior.Strict);

        fileSystem.SetupGet(fs => fs.AppDataDirectory).Returns("/tmp");
        pageDialog.Setup(p => p.DisplayActionSheet(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>()))
            .ReturnsAsync("Cancel");

        return new CalConfigViewModel(
            fileHosting.Object,
            email.Object,
            coords.Object,
            android.Object,
            toast.Object,
            saver.Object,
            fileSystem.Object,
            launcher.Object,
            pageDialog.Object);
    }

    [Fact]
    public void ApplyQueryAttributes_SetsEventsAndDateRange()
    {
        var sut = CreateSut(out _, out _, out _, out _, out _, out _, out _);

        var ev1 = CreateEvent("A", new DateTime(2025, 1, 10, 9, 0, 0), TimeSpan.FromHours(1));
        var ev2 = CreateEvent("B", new DateTime(2025, 1, 12, 9, 0, 0), TimeSpan.FromHours(1));
        var events = new List<CalEvent> { ev2, ev1 };

        sut.ApplyQueryAttributes(new Dictionary<string, object>
        {
            ["SelectedEvents"] = events
        });

        sut.Events.Should().HaveCount(2);
        sut.FirstEventDateTime.Should().Be(new DateTime(2025, 1, 10).ToString("d MMM yyyy", CultureInfo.CurrentCulture));
        sut.LastEventDateTime.Should().Be(new DateTime(2025, 1, 12).ToString("d MMM yyyy", CultureInfo.CurrentCulture));
    }

    [Fact]
    public void AlarmCommands_IncrementAndDecrementWithBounds()
    {
        var sut = CreateSut(out _, out _, out _, out _, out _, out _, out _);

        sut.AlarmMinutes.Should().Be(10);
        sut.IncrementAlarmCommand.Execute(null);
        sut.AlarmMinutes.Should().Be(15);

        sut.DecrementAlarmCommand.Execute(null);
        sut.AlarmMinutes.Should().Be(10);

        sut.AlarmMinutes = 119;
        sut.IncrementAlarmCommand.Execute(null);
        sut.AlarmMinutes.Should().Be(120);
        sut.IncrementAlarmCommand.Execute(null);
        sut.AlarmMinutes.Should().Be(120);

        sut.AlarmMinutes = 0;
        sut.DecrementAlarmCommand.Execute(null);
        sut.AlarmMinutes.Should().Be(0);
    }

    [Fact]
    public async Task SaveAndValidateIcalFileAsync_ShowsError_WhenInviteesMissing()
    {
        var sut = CreateSut(out _, out _, out _, out var toast, out _, out _, out _);
        sut.Events = [CreateEvent("A", DateTime.Now, TimeSpan.FromHours(1))];
        sut.Invitees = null;

        toast.Setup(t => t.ShowAsync("Email is required.")).Returns(Task.CompletedTask);

        await sut.SaveAndValidateIcalFileAsync();

        toast.Verify(t => t.ShowAsync("Email is required."), Times.Once);
    }

    [Fact]
    public async Task SaveAndValidateIcalFileAsync_ShowsError_WhenInviteesInvalidEmail()
    {
        var sut = CreateSut(out _, out _, out _, out var toast, out _, out _, out _);
        sut.Events = [CreateEvent("A", DateTime.Now, TimeSpan.FromHours(1))];
        sut.Invitees = "not-an-email";

        toast.Setup(t => t.ShowAsync("Invalid email format.")).Returns(Task.CompletedTask);

        await sut.SaveAndValidateIcalFileAsync();

        toast.Verify(t => t.ShowAsync("Invalid email format."), Times.Once);
    }

    [Fact]
    public async Task SaveAndValidateIcalFileAsync_SaveFileTrue_EmailFlow_Success()
    {
        var sut = CreateSut(out var hosting, out var email, out var coords, out var toast, out var saver, out _, out var launcher);
        sut.Events = [CreateEvent("Meeting", DateTime.Now, TimeSpan.FromHours(1), "Some place")];
        sut.Invitees = "user@example.com";
        sut.Email = "sender@example.com";
        sut.SaveFile = true;
        sut.SendEmail = true;

        coords.Setup(c => c.GetCoordinatesAsync("Some place")).ReturnsAsync((1.23, 4.56));
        saver.Setup(s => s.SaveAsync("events.ics", It.IsAny<Stream>()))
            .ReturnsAsync(new FileSaveResult(true, "/tmp/events.ics", null));
        email.Setup(e => e.SendEmailAsync("user@example.com", "Your iCal Event", It.IsAny<string>(), "/tmp/events.ics", "sender@example.com"))
            .Returns(Task.CompletedTask);
        toast.Setup(t => t.ShowAsync("The file was sent successfully")).Returns(Task.CompletedTask);

        await sut.SaveAndValidateIcalFileAsync();

        email.VerifyAll();
        toast.VerifyAll();
        coords.Verify(c => c.GetCoordinatesAsync("Some place"), Times.Once);
        hosting.VerifyNoOtherCalls();
        launcher.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task SaveAndValidateIcalFileAsync_SaveFileTrue_SaveFails_ShowsError()
    {
        var sut = CreateSut(out var hosting, out var email, out _, out var toast, out var saver, out _, out var launcher);
        sut.Events = [CreateEvent("Meeting", DateTime.Now, TimeSpan.FromHours(1))];
        sut.Invitees = "user@example.com";
        sut.SaveFile = true;
        sut.SendEmail = true;

        saver.Setup(s => s.SaveAsync("events.ics", It.IsAny<Stream>()))
            .ReturnsAsync(new FileSaveResult(false, "", new Exception("boom")));
        toast.Setup(t => t.ShowAsync(It.Is<string>(m => m.Contains("boom")))).Returns(Task.CompletedTask);

        await sut.SaveAndValidateIcalFileAsync();

        toast.VerifyAll();
        hosting.VerifyNoOtherCalls();
        email.VerifyNoOtherCalls();
        launcher.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task SaveAndValidateIcalFileAsync_SaveFileFalse_UploadFlow_OpensUrlAndShowsToast()
    {
        var sut = CreateSut(out var hosting, out var email, out var coords, out var toast, out _, out var fileSystem, out var launcher);
        var ev1 = CreateEvent("A", DateTime.Now, TimeSpan.FromHours(1));
        var ev2 = CreateEvent("B", DateTime.Now.AddHours(2), TimeSpan.FromHours(1), "Venue");
        sut.Events = [ev1, ev2];
        sut.Invitees = "user@example.com";
        sut.Email = "sender@example.com";
        sut.SaveFile = false;
        sut.SendEmail = false;

        fileSystem.Setup(fs => fs.SaveTextAsync("events.ics", It.IsAny<string>()))
            .ReturnsAsync("/app/data/events.ics");
        coords.Setup(c => c.GetCoordinatesAsync("Venue")).ReturnsAsync((0.0, 0.0));
        hosting.Setup(h => h.UploadFileAsync("/app/data/events.ics", "sender@example.com"))
            .ReturnsAsync(" https://example.com/ics ");
        toast.Setup(t => t.ShowAsync("The file was uploaded successfully")).Returns(Task.CompletedTask);
        launcher.Setup(l => l.OpenAsync("https://example.com/ics")).Returns(Task.CompletedTask);

        await sut.SaveAndValidateIcalFileAsync();

        coords.Verify(c => c.GetCoordinatesAsync("Venue"), Times.Once);
        hosting.VerifyAll();
        launcher.VerifyAll();
        email.VerifyNoOtherCalls();
        toast.VerifyAll();
    }
}
