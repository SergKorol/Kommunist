namespace Kommunist.Core.Services.Interfaces;

public interface IAndroidCalendarService
{
    Task AddEvents(string icsPath, string targetCalendarName = null);
    Task<string[]> GetCalendarNames();

}