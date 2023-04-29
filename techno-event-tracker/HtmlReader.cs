using HtmlAgilityPack;
using System.Text;

namespace techno_event_tracker;

public class HtmlReader
{
    public async Task<ICollection<EventModel>> ReadAsync(string url)
    {
        var htmlCode = await GetHtmlCodeAsync(url);
        var list = ReadRadion(htmlCode, url);

        for (var i = 0; i < list.Count; i++)
        {
            var node = await GetHtmlCodeAsync(list[i].Url);
            list[i] = ReadDescription(list[i], node);
        }

        return list;
    }

    private static IList<EventModel> ReadRadion(HtmlNode bodyNode, string url)
    {
        var mainDiv = bodyNode.SelectSingleNode("//div[@class='programs_list-wrapper']");
        var childNodes = mainDiv.ChildNodes.Where(x => x.Name == "div");
        var list = new List<EventModel>();
        url = url[..(url.IndexOf("/", url.IndexOf("//", StringComparison.Ordinal) + 2, StringComparison.Ordinal) + 1)];

        foreach (var node in childNodes)
        {
            var month = node.Attributes["data-month-slug"].Value;
            var eventListContainer = node.ChildNodes[1].ChildNodes[0];
            if (eventListContainer == null || eventListContainer.HasClass("w-dyn-empty")) continue;

            foreach (var listItemDiv in eventListContainer.ChildNodes)
            {
                var aTag = listItemDiv.FirstChild;
                var tags = aTag.ChildNodes.FirstOrDefault(x => x.Name == "div" && x.HasClass("programs_tag-wrapper"));
                var title = aTag.ChildNodes.FirstOrDefault(x => x.Name == "h2")?.InnerText;
                var date = aTag.ChildNodes.FirstOrDefault(x => x.Name == "div" && x.HasClass("programs_date"))?.InnerText;
                
                ArgumentNullException.ThrowIfNull(title);
                ArgumentNullException.ThrowIfNull(date);

                var location = tags?.ChildNodes.FirstOrDefault(x => x.Name == "div" && x.Attributes.Contains("fs-cmsfilter-field"))?.InnerText;
                if (location != "Club") continue;

                // Remove the first character from this
                var programUrl = aTag.Attributes["href"].Value.Remove(0, 1);

                list.Add(new EventModel
                {
                    Title = title,
                    Date = DateTime.Parse(date),
                    Url = $"{url}{programUrl}"
                });
            }
        }
        
        return list;
    }

    private static EventModel ReadDescription(EventModel model, HtmlNode fromNode)
    {
        var programDescription = fromNode.Descendants("div")
            .FirstOrDefault(x => x.HasClass("program_description"))
            ?.InnerHtml;

        var ticketUrl = fromNode.Descendants("a")
            .FirstOrDefault(x => x.HasClass("event_ticket-link"))
            ?.Attributes["href"].Value;

        var programTime = fromNode.Descendants("div")
            .FirstOrDefault(x => x.HasClass("event_time-info"))
            ?.SelectSingleNode("h1").InnerText;

        model.Description = RemoveHtmlTagsAndKeepFormatting(programDescription!);
        model.TicketUrl = ticketUrl!;
        
        if (programTime == null) return model;

        var startTime = TimeSpan.Parse(programTime[..programTime.IndexOf("-", StringComparison.Ordinal)].Trim());
        var endTime = TimeSpan.Parse(programTime[(programTime.IndexOf("-", StringComparison.Ordinal) + 1)..].Trim());
        if (endTime < startTime) endTime += TimeSpan.FromDays(1);
        
        model.StartTime = model.Date.Add(startTime);
        model.EndTime = model.Date.Add(endTime);


        return model;
    }

    private static string RemoveHtmlTagsAndKeepFormatting(string htmlText)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(htmlText);

        var sb = new StringBuilder();

        foreach (var node in doc.DocumentNode.DescendantsAndSelf())
        {
            if (node.NodeType == HtmlNodeType.Text)
            {
                sb.Append(node.InnerText);
            }
            else if (node.Name == "br" || node.Name == "p")
            {
                sb.AppendLine();
            }
        }

        return sb.ToString();
    }

    private static async Task<HtmlNode> GetHtmlCodeAsync(string url)
    {
        var web = new HtmlWeb();
        var htmlDoc = await web.LoadFromWebAsync(url);
        var node = htmlDoc.DocumentNode.SelectSingleNode("//body");
        return node;
    }
}