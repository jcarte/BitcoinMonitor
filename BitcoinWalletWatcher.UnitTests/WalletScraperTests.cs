using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using BitcoinWalletWatcher.Scraper;
using BitcoinWalletWatcher.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BitcoinWalletWatcher.UnitTests
{
    [TestClass]
    public class WalletScraperTests
    {

        [TestMethod]
        public void ScrapeWallet_WalletExists_ReturnsInfo()
        {
            var http = new Mock<IHttpHelper>();
            var resp = "{\"16rCmCmbuWDhPjWTrpQGaU3EPdZF7MTdUk\":{\"final_balance\":11920302747185,\"n_tx\":104,\"total_received\":16370502780885}}";
            http.Setup(h => h.GetJsonAsync("http://api.com/16rCmCmbuWDhPjWTrpQGaU3EPdZF7MTdUk", null)).ReturnsAsync(resp);
            http.Setup(h => h.GetJsonAsync(It.IsNotIn("http://api.com/16rCmCmbuWDhPjWTrpQGaU3EPdZF7MTdUk"), null)).ThrowsAsync(new WebApiException(System.Net.HttpStatusCode.NotFound,"Not Found",""));


            WalletScraper scrape = new WalletScraper(http.Object, "http://api.com/");

            //Act
            var wals = scrape.ScrapeWalletsAsync(new string[] { "16rCmCmbuWDhPjWTrpQGaU3EPdZF7MTdUk" }).Result;

            //Assert
            Assert.AreEqual(119203.02747185m, wals.First().CurrentBalanceBTC);
        }

        [TestMethod]
        public void ScrapeWallet_WalletDoesntExists_Exception()//check it passes exception up, don't need to test more combinations otherwise testing mock
        {
            //Arrange
            var http = new Mock<IHttpHelper>();
            var resp = "{\"16rCmCmbuWDhPjWTrpQGaU3EPdZF7MTdUk\":{\"final_balance\":11920302747185,\"n_tx\":104,\"total_received\":16370502780885}}";
            http.Setup(h => h.GetJsonAsync("http://api.com/16rCmCmbuWDhPjWTrpQGaU3EPdZF7MTdUk", null)).ReturnsAsync(resp);
            http.Setup(h => h.GetJsonAsync(It.IsNotIn("http://api.com/16rCmCmbuWDhPjWTrpQGaU3EPdZF7MTdUk"), null)).ThrowsAsync(new WebApiException(System.Net.HttpStatusCode.NotFound, "Not Found", ""));

            WalletScraper scrape = new WalletScraper(http.Object, "http://api.com/");

            //Act/Assert
            var e = Assert.ThrowsExceptionAsync<WebApiException>(()=> scrape.ScrapeWalletsAsync(new string[] { "26rCmCmbuWDhPjWTrpQGaU3EPdZF7MTdUk" }));
        }

        [TestMethod]
        public void ScrapeWallet_MultiWalletsExist_ReturnsInfo()
        {
            //Arrange
            var http = new Mock<IHttpHelper>();
            var resp = "{\"a\":{\"final_balance\":51920302747185,\"n_tx\":104,\"total_received\":26370502780885},\"b\":{\"final_balance\":21920302747185,\"n_tx\":104,\"total_received\":16370502780885}}";
            http.Setup(h => h.GetJsonAsync("http://api.com/a|b", null)).ReturnsAsync(resp);
            http.Setup(h => h.GetJsonAsync(It.IsNotIn("http://api.com/a|b"), null)).ThrowsAsync(new WebApiException(System.Net.HttpStatusCode.NotFound, "Not Found", ""));

            WalletScraper scrape = new WalletScraper(http.Object, "http://api.com/");

            //Act
            var wals = scrape.ScrapeWalletsAsync(new string[] { "a","b" }).Result;

            //Assert
            Assert.AreEqual(2, wals.Count());
            Assert.AreEqual(519203.02747185m, wals.First(a => a.Address == "a").CurrentBalanceBTC);
            Assert.AreEqual(219203.02747185m, wals.First(a => a.Address == "b").CurrentBalanceBTC);
        }


    }
}
