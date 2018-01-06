using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace BitcoinWalletWatcher.Data
{
    public class MonitorContext: DbContext
    {

        public static MonitorContext GetContext(string connectionStr)
        {
            var connection = new SqliteConnection(connectionStr);
            connection.Open();

            var options = new DbContextOptionsBuilder<MonitorContext>()
                    .UseSqlite(connection)
                    .Options;

            var ent = new MonitorContext(options);
            return ent;
        }

        public MonitorContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<WalletBalance> WalletBalances { get; set; }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    //optionsBuilder.UseSqlite(connectionStr);
        //}
    }
}
