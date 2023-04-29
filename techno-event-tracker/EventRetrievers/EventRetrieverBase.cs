using HtmlAgilityPack;
using System.Text;

namespace techno_event_tracker.EventRetrievers;

internal abstract class EventRetrieverBase : IEventRetriever
{
    public async Task<ICollection<EventModel>> RetrieveEventsAsync(string fromUrl)
    {
        var htmlCode = await GetHtmlBodyAsync(fromUrl);
        var list = GetEventList(htmlCode, fromUrl);

        for (var i = 0; i < list.Count; i++)
        {
            var node = await GetHtmlBodyAsync(list[i].Url);
            list[i] = ReadDescription(list[i], node);
        }

        return list;
    }

    protected abstract IList<EventModel> GetEventList(HtmlNode bodyNode, string url);
    protected abstract EventModel ReadDescription(EventModel model, HtmlNode fromNode);

    protected static string RemoveHtmlTagsAndKeepFormatting(string htmlText)
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
            else if (node.Name is "br" or "p")
            {
                sb.AppendLine();
            }
        }

        return sb.ToString();
    }

    private static async Task<HtmlNode> GetHtmlBodyAsync(string url)
    {
        var web = new HtmlWeb();
        var htmlDoc = await web.LoadFromWebAsync(url);
        var node = htmlDoc.DocumentNode.SelectSingleNode("//body");
        return node;
    }
}