namespace techno_event_tracker;

public interface IEventRetriever
{
    Task<ICollection<EventModel>> RetrieveEventsAsync(string fromUrl);
}