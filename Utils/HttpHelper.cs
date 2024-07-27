using System.Net;

namespace BangumiRSSAggregator.Server.Utils;

public static class HttpHelper
{
    public static HttpClient GetHttpClient(string? proxyAddr = null)
    {
        var httpClientHandler = new HttpClientHandler();
        if (!string.IsNullOrEmpty(proxyAddr))
        {
            httpClientHandler.Proxy = new WebProxy(proxyAddr)
            {
                BypassProxyOnLocal = false,
                UseDefaultCredentials = false,
            };
        }
        return new HttpClient(httpClientHandler, true);
    }

    public static async Task<string> GetResponseText(this HttpClient httpClient, string url)
    {
        var result = string.Empty;
        using (var resp = await httpClient.GetAsync(url))
        {
            if (resp.IsSuccessStatusCode)
            {
                result = await resp.Content.ReadAsStringAsync();
            }
        }

        return result;
    }
}
