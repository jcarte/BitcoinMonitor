using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BitcoinWalletWatcher.Data
{
    public class WalletRepository
    {
        MonitorContext _context;
        public WalletRepository(MonitorContext context)
        {
            _context = context;
            _context.Database.Migrate();
            if (!_context.Wallets.Any())
                PopulateSeedData();
        }

        private void PopulateSeedData()
        {
            string json = File.ReadAllText("Data/InitialWalletBalances.json");
            var wallets = JsonConvert.DeserializeObject<IEnumerable<Wallet>>(json);
            _context.Wallets.AddRange(wallets);
            _context.SaveChanges();
        }

        public event Action<IEnumerable<WalletDiff>> DataUpdated;

        private void OnDataUpdated(IEnumerable<WalletDiff> diffs)
        {
            DataUpdated?.Invoke(diffs);
        }

        public async void UpdateWalletsAsync(IEnumerable<WalletInfo> wals)
        {
            var diffs = new List<WalletDiff>();
            foreach (var wal in wals)
            {
                if(string.IsNullOrEmpty(wal.Address))
                    throw new ArgumentException($"Wallet Address is empty");

                //get from db
                var ent = _context.Wallets.FirstOrDefault(w => w.Address == wal.Address);
                if (ent == null)
                    throw new ArgumentException($"Wallet {wal.Address} could not be found in database");

                if(ent.CurrentBalanceBTC!=wal.CurrentBalanceBTC)//there's a difference, run a diff
                {
                    var diff = new WalletDiff()
                    {
                        Old = new WalletInfo()
                        {
                            Address = ent.Address,
                            CurrentBalanceBTC = ent.CurrentBalanceBTC,
                            MaxBalanceBTC = ent.MaxBalanceBTC,
                            LastBalanceScrapedAt = ent.LastBalanceScrapedAt,
                            Notes = ent.Notes
                        },
                        New = new WalletInfo()
                        {
                            Address = wal.Address,
                            CurrentBalanceBTC = wal.CurrentBalanceBTC,
                            MaxBalanceBTC = Math.Max(ent.MaxBalanceBTC, ent.CurrentBalanceBTC),
                            LastBalanceScrapedAt = wal.LastBalanceScrapedAt,
                            Notes = ent.Notes
                        }
                    };
                    diffs.Add(diff);

                    //Add a new balance record
                    WalletBalance bal = new WalletBalance()
                    {
                        WalletId = ent.WalletId,
                        BalanceBTC = wal.CurrentBalanceBTC,
                        ScrapedAt = wal.LastBalanceScrapedAt
                    };
                    _context.WalletBalances.Add(bal);

                    //Update wallet values
                    ent.CurrentBalanceBTC = wal.CurrentBalanceBTC;
                    ent.MaxBalanceBTC = Math.Max(ent.MaxBalanceBTC, ent.CurrentBalanceBTC);
                    ent.LastBalanceScrapedAt = wal.LastBalanceScrapedAt;
                }
            }

            await _context.SaveChangesAsync();//commit all

            if (diffs.Any())
                OnDataUpdated(diffs);
        }

        public IEnumerable<WalletInfo> GetWallets()
        {
            return _context.Wallets.Select(e => new WalletInfo()
            {
                Address = e.Address,
                CurrentBalanceBTC = e.CurrentBalanceBTC,
                LastBalanceScrapedAt = e.LastBalanceScrapedAt,
                MaxBalanceBTC = e.MaxBalanceBTC,
                Notes = e.Notes
            });
        }

    }

    public class WalletDiff
    {
        public WalletInfo Old { get; set; }
        public WalletInfo New { get; set; }
    }
}
