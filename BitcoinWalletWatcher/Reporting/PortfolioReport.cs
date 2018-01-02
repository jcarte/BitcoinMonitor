using System;
using System.Collections.Generic;
using System.Linq;

namespace BitcoinWalletWatcher.Reporting
{
    public class PortfolioReport
    {
        public decimal CurrentTotalBalanceBTC { get; set; }
        public decimal MaxTotalBalanceBTC { get; set; }
        public DateTime LastScrapedAt { get; set; }
        public decimal PercentOfMax {
            get
            {
                return CurrentTotalBalanceBTC / MaxTotalBalanceBTC;
            }
        }
        public bool IsFailing { get; set; }
        public IEnumerable<WalletReport> WalletReports { get; set; }
        public int TotalNumberOfMonitoredWallets {
            get
            {
                return WalletReports.Count();
            }
        }
        public int TotalNumberOfFailingWallets
        {
            get
            {
                return WalletReports.Count(w=>w.IsFailing);
            }
        }
    }

    public class WalletReport
    {
        public string Address { get; set; }
        public decimal CurrentBalanceBTC { get; set; }
        public decimal MaxBalanceBTC { get; set; }
        public decimal PercentOfMax
        {
            get
            {
                return CurrentBalanceBTC / MaxBalanceBTC;
            }
        }
        public bool IsFailing { get; set; }
    }
}
