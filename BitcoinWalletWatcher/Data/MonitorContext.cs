using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace BitcoinWalletWatcher.Data
{
    public class MonitorContext: DbContext
    {
        private string connectionStr;

        public MonitorContext(string connectionStr)
        {
            this.connectionStr = connectionStr;
        }

        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<WalletBalance> WalletBalances { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(connectionStr);
        }
    }
}
