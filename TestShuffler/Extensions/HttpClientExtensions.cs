using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace TestShuffler
{
    public static class HttpClientExtensions
    {
        public static async Task<HttpResponseMessage> HeadAsync(this HttpClient httpClient, Uri uri) => 
            await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, uri));
    }
}