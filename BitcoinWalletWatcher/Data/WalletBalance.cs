using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using SQLitePCL;

namespace BitcoinWalletWatcher.Data
{
    public class WalletBalance
    {
        [Key]
        public int BalanceId { get; set; }
        public int WalletId { get; set; }
        public decimal BalanceBTC { get; set; }
        public DateTime ScrapedAt { get; set; }
    }
}
