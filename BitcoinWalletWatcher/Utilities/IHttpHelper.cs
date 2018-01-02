using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace BitcoinWalletWatcher.Utilities
{
    public interface IHttpHelper
    {
        Task<HttpResponseMessage> PostJsonAsync(string url, string body, IDictionary<string, string> headers = null);
        Task<HttpResponseMessage> GetJsonAsync(string url, IDictionary<string, string> headers = null);
    }
    public class WebApiException : Exception
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
