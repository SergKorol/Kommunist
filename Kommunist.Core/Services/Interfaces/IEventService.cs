using Kommunist.Core.Entities;

namespace Kommunist.Core.Services.Interfaces;

public interface IEventService
{
    Task<IEnumerable<Event>> LoadEvents(DateTime startDate, DateTime endDate);
    // IEnumerable<Event> LoadEvents(DateTime startDate, DateTime endDate);
}