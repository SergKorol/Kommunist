using Kommunist.Core.Entities;
using Kommunist.Core.Entities.BaseType;
using Kommunist.Core.Entities.PageProperties.Agenda;

namespace Kommunist.Core.Services.Interfaces;

public interface IEventService
{
    Task<IEnumerable<Event>> LoadEvents(DateTime startDate, DateTime endDate);
    Task<IEnumerable<EventPage>> GetEventPages(int eventId);
    Task<AgendaPage> GetAgenda(int eventId);
}