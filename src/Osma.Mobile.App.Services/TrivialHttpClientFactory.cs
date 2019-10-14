using System.Net.Http;

namespace Osma.Mobile.App.Services
{
    public class TrivialHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name)
        {
            return new HttpClient();
        }
    }
}