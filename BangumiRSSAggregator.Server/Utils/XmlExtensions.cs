using System.Xml.Linq;

namespace BangumiRSSAggregator.Server.Utils;

public static class XmlExtensions
{
    public static string ToXml(this XElement element)
    {
        var textWritter = new StringWriter();
        element.Save(textWritter);
        return textWritter.ToString();
    }
}
