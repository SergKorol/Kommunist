using Kommunist.Core.Entities.BaseType;
using Kommunist.Core.Entities.PageProperties.Agenda;
using Kommunist.Core.Models;
using Event = Kommunist.Core.Entities.Event;

namespace Kommunist.Core.Services.Interfaces;

public interface IEventService
{
    Task<IEnumerable<Event>> LoadEvents(DateTime startDate, DateTime endDate);
    Task<IEnumerable<PageItem>> GetHomePage(int eventId);
    Task<AgendaPage> GetAgenda(int eventId);
}