using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BitcoinWalletWatcher.Utilities
{
    public class HttpHelper:IHttpHelper
    {
        public async Task<HttpResponseMessage> PostJsonAsync(string url, string body, IDictionary<string,string> headers = null)
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
                return res;
            }
        }

        public async Task<HttpResponseMessage> GetJsonAsync(string url, IDictionary<string, string> headers = null)
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
                return res;
            }
        }

    }


}
