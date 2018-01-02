
using BitcoinWalletWatcher.Data;
using BitcoinWalletWatcher.Reporting.Email;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BitcoinWalletWatcher.Reporting
{
    public class ReportEngine
    { 
        WalletRepository _repo;
        decimal _failThreshold;
        SendGrid _email;

        public ReportEngine(WalletRepository repo, decimal failThreshold, SendGrid emailer)
        {
            _repo = repo;
            _failThreshold = failThreshold;
            _email = emailer;
            _repo.DataUpdated += ProcessNewWalletData;
        }

        private void ProcessNewWalletData(IEnumerable<WalletDiff> diffs)
        {
            foreach (var diff in diffs)
            {
                var oldR = GetWalletReport(diff.Old);
                var newR = GetWalletReport(diff.New);

                if (oldR.IsFailing != newR.IsFailing)
                    OnSingleWalletStatusChanged(newR);
            }

            var port = GetPortfolioReport();//get portfolio now

            //how much has the max bal increased
            decimal maxDiff = (decimal)diffs.Sum(d => d.New.MaxBalanceBTC - (decimal)d.Old.MaxBalanceBTC);

            //how much has the balance changed
            decimal currDiff = (decimal)diffs.Sum(d => d.New.CurrentBalanceBTC - (decimal)d.Old.CurrentBalanceBTC);

            //reconstruct old portfolio status to comapre to new
            decimal oldMax = port.MaxTotalBalanceBTC - maxDiff;
            decimal oldCurr = port.CurrentTotalBalanceBTC - currDiff;
            bool wasOldFailing = (oldCurr / oldMax) < _failThreshold;

            if (port.IsFailing != wasOldFailing)
                OnPortfolioStatusChanged(port);
        }

        public void SendBalanceReport()
        {
            var port = GetPortfolioReport();
            _email.SendBalanceReport(port);
        }


        private void OnSingleWalletStatusChanged(WalletReport rep) => _email.SendSingleWalletAlert(rep);
        private void OnPortfolioStatusChanged(PortfolioReport rep) => _email.SendPortfolioAlert(rep);

        private PortfolioReport GetPortfolioReport()
        {
            IEnumerable<WalletInfo> wals = _repo.GetWallets();
            PortfolioReport port = new PortfolioReport()
            {
                CurrentTotalBalanceBTC = wals.Sum(w => w.CurrentBalanceBTC),
                MaxTotalBalanceBTC = wals.Sum(w => (decimal)w.MaxBalanceBTC),
                LastScrapedAt = wals.Max(w => w.LastBalanceScrapedAt)
            };
            port.IsFailing = port.PercentOfMax < _failThreshold;

            port.WalletReports = wals.Select(wal => GetWalletReport(wal));

            return port;
        }

        private WalletReport GetWalletReport(WalletInfo wal)
        {
            WalletReport rep = new WalletReport()
            {
                CurrentBalanceBTC = wal.CurrentBalanceBTC,
                MaxBalanceBTC = (decimal)wal.MaxBalanceBTC,
                Address = wal.Address
            };
            rep.IsFailing = rep.PercentOfMax < _failThreshold;
            return rep;
        }
    }
}
