using techno_event_tracker;

namespace TechnoEventTracker.Tests;

public class HtmlReaderTest
{
    [Fact]
    public async Task Test1()
    {
        var reader = new RadionEventRetriever();
        var list = await reader.RetrieveEventsAsync("https://www.radion.amsterdam/program");
        Assert.NotEmpty(list);
    }
}