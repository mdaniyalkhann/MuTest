using System.IO;
using HtmlAgilityPack;

namespace MuTest.Core.Common
{
    public static class HtmlUtilities
    {
        public static string ConvertToPlainText(this string html)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return null;
            }

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            using (var stringWriter = new StringWriter())
            {
                ConvertTo(doc.DocumentNode, stringWriter);
                stringWriter.Flush();

                return stringWriter.ToString();
            }
        }

        private static void ConvertContentTo(HtmlNode node, TextWriter outText)
        {
            foreach (var htmlNode in node.ChildNodes)
            {
                ConvertTo(htmlNode, outText);
            }
        }

        private static void ConvertTo(HtmlNode node, TextWriter outText)
        {
            switch (node.NodeType)
            {
                case HtmlNodeType.Comment:
                    break;

                case HtmlNodeType.Document:
                    ConvertContentTo(node, outText);
                    break;

                case HtmlNodeType.Text:
                    var parentName = node.ParentNode.Name;
                    if (parentName == "script" ||
                        parentName == "style")
                        break;

                    var html = ((HtmlTextNode)node).Text;

                    if (HtmlNode.IsOverlappedClosingElement(html))
                        break;

                    if (html.Trim().Length > 0)
                    {
                        outText.Write(HtmlEntity.DeEntitize(html));
                    }

                    break;

                default:
                    if (node.Name == "p" ||
                        node.Name == "br" ||
                        node.Name == "legend" ||
                        node.Name == "pre")
                    {
                        outText.Write("\r\n");
                    }

                    if (node.HasChildNodes)
                    {
                        ConvertContentTo(node, outText);
                    }

                    break;
            }
        }
    }
}