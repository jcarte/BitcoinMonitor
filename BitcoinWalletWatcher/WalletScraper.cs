using BitcoinWalletWatcher.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BitcoinWalletWatcher
{
    /// <summary>
    /// Get the updated balances of bitcoing wallets
    /// </summary>
    public class WalletScraper
    {
        HttpHelper _http;
        string _baseUrl;//starting url for api call
        const string ADDRESS_SEPERATOR = "|";//how list of wallet addresses are delimited in query url
        public WalletScraper(HttpHelper http, string baseUrl)
        {
            _http = http;
            _baseUrl = baseUrl;
        }

        /// <summary>
        /// Get current balances for each wallet address
        /// </summary>
        /// <param name="walletAddresses"></param>
        /// <returns></returns>
        public async Task<IEnumerable<WalletInfo>> ScrapeWalletsAsync(string[] walletAddresses)
        {
            string addys = string.Join(ADDRESS_SEPERATOR, walletAddresses);//construct param part of url

            var resp = await _http.GetJsonAsync(_baseUrl + addys);//get balances

            var sentTime = DateTime.Now;
            string json = await resp.Content.ReadAsStringAsync();//extract balances from response and return

            ////Example JSON
            //{"16rCmCmbuWDhPjWTrpQGaU3EPdZF7MTdUk": {
            //    "final_balance": 11920302747185,
            //    "n_tx": 104,
            //    "total_received": 16370502780885
            //}}
            var dic = JsonConvert.DeserializeObject<Dictionary<string, WalletScrape>>(json);

            var wallets = dic.Select(k => new WalletInfo()
            {
                Address = k.Key,
                CurrentBalanceBTC = k.Value.BalanceBTC,
                LastBalanceScrapedAt = sentTime
            });

            return wallets;
        }

        /// <summary>
        /// Strongly typed version of response object
        /// </summary>
        private class WalletScrape
        {
            [JsonProperty("final_balance")]
            public long BalanceSatoshi { get; set; }

            /// <summary>
            /// Convert balance in Satoshi to bitcoin
            /// </summary>
            public decimal BalanceBTC {
                get
                {
                    return BalanceSatoshi / 100000000m;
                }
            }
        }
    }

    
    
}

