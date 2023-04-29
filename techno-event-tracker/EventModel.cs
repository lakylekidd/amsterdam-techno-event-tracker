namespace techno_event_tracker;

public class EventModel
{
    public string Title { get; set; }
    public DateTime Date { get; set; }
    public string Url { get; set; }
    public string? TicketUrl { get; set; }
    public string? Description { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}