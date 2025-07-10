using HtmlAgilityPack;
using Microsoft.Extensions.Options;

namespace Adapter_HTML
{
    public class HtmlToJsonConverter
    {
        private readonly HtmlAdapterSettings settings;
        private readonly HttpClient client;

        public HtmlToJsonConverter(IOptions<HtmlAdapterSettings> options, IHttpClientFactory factory)
        {
            settings = options.Value;
            client = factory.CreateClient();
        }

        public async Task<IEnumerable<TagModel>> ConvertAsync()
        {
            string url = $"{settings.BaseURL} {settings.Endpoint ?? ""}";
            string html = await client.GetStringAsync(url);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            IEnumerable<HtmlNode> tags;
            if (!string.IsNullOrEmpty(settings.XPath))
            {
                tags = doc.DocumentNode.SelectNodes(settings.XPath) ?? Enumerable.Empty<HtmlNode>();
            }
            else 
            {
                tags = doc.DocumentNode.Descendants();
            }

            var jsonElements = tags.Where(x => !x.Name.StartsWith("#")).Select(tag => new TagModel
                {
                    TagName = tag.Name,
                    InnerText = tag.InnerText.Trim(),
                    Attributes = tag.Attributes.Select(attr => new AttributeModel
                    {
                        Name = attr.Name,
                        Value = attr.Value
                    })
                 });
            return jsonElements;
        }
    }
}
