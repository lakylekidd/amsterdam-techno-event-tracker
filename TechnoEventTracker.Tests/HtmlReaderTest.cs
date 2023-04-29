using techno_event_tracker;

namespace TechnoEventTracker.Tests;

public class HtmlReaderTest
{
    [Fact]
    public async Task Test1()
    {
        var reader = new HtmlReader();
        var list = await reader.ReadAsync("https://www.radion.amsterdam/program");
        Assert.NotEmpty(list);
    }
}