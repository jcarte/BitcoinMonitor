using BitcoinWalletWatcher.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BitcoinWalletWatcher.UnitTests
{
    [TestClass]
    public class WalletRepositoryTests
    {

        private MonitorContext GetDbContext()
        {
            var ent = MonitorContext.GetContext("DataSource=:memory:");
            ent.Database.EnsureCreated();
            return ent;
        }

        [TestMethod]
        public void WalletRepository_BlankDatabase_LoadsInitialWallets()
        {
            var db = GetDbContext();
            var wallets = new Wallet[]
            {
                new Wallet(){Address = "ADD1"},
                new Wallet(){Address = "ADD2"},
                new Wallet(){Address = "ADD3"}
            };

            var repo = new WalletRepository(db,wallets);

            Assert.AreEqual(3,db.Wallets.Count());
        }

        [TestMethod]
        public void GetWallets_WalletsInDb_LoadsInitialWallets()
        {
            var db = GetDbContext();
            var wallets = new Wallet[]
            {
                new Wallet(){
                    Address = "ADD1",
                    CurrentBalanceBTC = 10,
                    MaxBalanceBTC = 20,
                    Notes = "notes",
                    LastBalanceScrapedAt = new DateTime(2010,01,01)
                }
            };
            db.Wallets.AddRange(wallets);
            db.SaveChanges();
            var repo = new WalletRepository(db, null);

            var walls = repo.GetWallets();

            Assert.AreEqual(1, walls.Count());
            Assert.AreEqual("ADD1", walls.First().Address);
            Assert.AreEqual(10, walls.First().CurrentBalanceBTC);
            Assert.AreEqual(20, walls.First().MaxBalanceBTC);
            Assert.AreEqual("notes", walls.First().Notes);
            Assert.AreEqual(new DateTime(2010, 01, 01), walls.First().LastBalanceScrapedAt);
        }


        [TestMethod]
        public void GetWallets_NoWalletsInDb_ReturnsNone()
        {
            var db = GetDbContext();
            var repo = new WalletRepository(db, null);

            var walls = repo.GetWallets();

            Assert.AreEqual(0, walls.Count());
        }

        [TestMethod]
        public void UpdateWallets_WalletToUpdate_FiresEvent()
        {
            var db = GetDbContext();
            var wallets = new Wallet[]
            {
                new Wallet(){
                    Address = "ADD1",
                    CurrentBalanceBTC = 10,
                    MaxBalanceBTC = 20,
                    Notes = "notes",
                    LastBalanceScrapedAt = new DateTime(2010,01,01)
                }
            };
            db.Wallets.AddRange(wallets);
            db.SaveChanges();
            var repo = new WalletRepository(db, null);
            int eventCount = 0; 
            repo.DataUpdated += (w) => eventCount++;


            var walInfo = new List<WalletInfo> {new WalletInfo() {
                Address = "ADD1",
                CurrentBalanceBTC = 20,
                MaxBalanceBTC = 40
            } };
            repo.UpdateWalletsAsync(walInfo);

            Assert.AreEqual(1, eventCount);
        }

        [TestMethod]
        public async void UpdateWallets_WalletToUpdate_Updates()
        {
            var db = GetDbContext();
            var wallets = new Wallet[]
            {
                new Wallet(){
                    Address = "ADD1",
                    CurrentBalanceBTC = 10,
                    MaxBalanceBTC = 20,
                    Notes = "notes",
                    LastBalanceScrapedAt = new DateTime(2010,01,01)
                }
            };
            db.Wallets.AddRange(wallets);
            db.SaveChanges();
            var repo = new WalletRepository(db, null);


            var walInfo = new List<WalletInfo> {new WalletInfo() {
                Address = "ADD1",
                CurrentBalanceBTC = 20,
                MaxBalanceBTC = 40
            } };
            await repo.UpdateWalletsAsync(walInfo);

            var wal = db.Wallets.First();
            Assert.AreEqual(20, wal.CurrentBalanceBTC);
            Assert.AreEqual(40, wal.MaxBalanceBTC);
        }


    }
}
