using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BitcoinWalletWatcher.Utilities
{
    public class HttpHelper:IHttpHelper
    {
        public async Task PostJsonAsync(string url, string body, IDictionary<string,string> headers = null)
        {
            using (HttpClient http = new HttpClient())
            {
                StringContent cont = new StringContent(body, Encoding.UTF8, "application/json");

                if (headers != null)
                {
                    foreach (var head in headers)
                    {
                        http.DefaultRequestHeaders.Add(head.Key, head.Value);
                    }
                }

                var res = await http.PostAsync(url, cont);
                if (!res.IsSuccessStatusCode)
                    throw new WebApiException(res);
            }
        }

        public async Task<string> GetJsonAsync(string url, IDictionary<string, string> headers = null)
        {
            using (HttpClient http = new HttpClient())
            {
                if (headers != null)
                {
                    foreach (var head in headers)
                    {
                        http.DefaultRequestHeaders.Add(head.Key, head.Value);
                    }
                }

                var res = await http.GetAsync(url);
                if (!res.IsSuccessStatusCode)
                    throw new WebApiException(res);
                return await res.Content.ReadAsStringAsync();
            }
        }

    }

    public class WebApiException: Exception
    {
        public HttpStatusCode StatusCode { get; }
        public string ResponseContent { get; }
        public string StatusMessage { get; }
        public WebApiException(HttpStatusCode statusCode, string statusMessage, string responseContent)
        {
            this.StatusCode = statusCode;
            this.ResponseContent = responseContent;
            this.StatusMessage = statusMessage;
        }
        public WebApiException(HttpResponseMessage msg)
        {
            this.StatusCode = msg.StatusCode;
            this.ResponseContent = msg.Content.ReadAsStringAsync().Result;
            this.StatusMessage = msg.ReasonPhrase;
        }
        public override string ToString() => $"{StatusCode} {StatusMessage} - {ResponseContent}";
    }
}
