using Kommunist.Core.Entities.BaseType;
using Kommunist.Core.Entities.PageProperties.Agenda;
using Event = Kommunist.Core.Entities.Event;
using Item = Kommunist.Core.Models.Item;

namespace Kommunist.Core.Services.Interfaces;

public interface IEventService
{
    Task<IEnumerable<Event>> LoadEvents(DateTime startDate, DateTime endDate);
    Task<IEnumerable<Item>> GetHomePage(int eventId);
    Task<AgendaPage> GetAgenda(int eventId);
}