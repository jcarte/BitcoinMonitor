using System;
using System.Collections.Generic;
using System.Text;

namespace BitcoinWalletWatcher
{
    public class WalletInfo
    {
        public string Address { get; set; }
        public string Notes { get; set; }
        public decimal CurrentBalanceBTC { get; set; }
        public decimal? MaxBalanceBTC { get; set; }//TODO two separate objects?
        public DateTime LastBalanceScrapedAt { get; set; }
    }
}
