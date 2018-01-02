using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BitcoinWalletWatcher.Data
{
    public class Wallet
    {
        [Key]
        public int WalletId { get; set; }
        public string Address { get; set; }
        public string Notes { get; set; }
        public decimal CurrentBalanceBTC { get; set; }
        public decimal MaxBalanceBTC { get; set; }
        public DateTime LastBalanceScrapedAt { get; set; }
    }
}
