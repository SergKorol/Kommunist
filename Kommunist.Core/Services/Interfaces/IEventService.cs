using Kommunist.Core.Entities;
using Kommunist.Core.Entities.PageProperties.Agenda;
using Kommunist.Core.Models;

namespace Kommunist.Core.Services.Interfaces;

public interface IEventService
{
    Task<IEnumerable<ServiceEvent>?> LoadEvents(DateTime startDate, DateTime endDate);
    Task<IEnumerable<PageItem>?> GetHomePage(int eventId);
    Task<AgendaPage?> GetAgenda(int eventId);
}