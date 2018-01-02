using BitcoinWalletWatcher.Data;
using BitcoinWalletWatcher.Reporting;
using BitcoinWalletWatcher.Reporting.Email;
using BitcoinWalletWatcher.Utilities;
using Microsoft.Extensions.Configuration;
using Quartz;
using Quartz.Impl;
using Quartz.Logging;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BitcoinWalletWatcher
{
    public class Program
    {
        private static void Main(string[] args)
        {
            LogProvider.SetCurrentLogProvider(new ConsoleLogProvider());

            RunProgramRunExample().GetAwaiter().GetResult();

            Console.WriteLine("Press any key to close the application");
            Console.ReadKey();
        }

        private static async Task RunProgramRunExample()
        {
            try
            {
                //setup config
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

                IConfigurationRoot config = builder.Build();

                //setup http connection helper
                HttpHelper http = new HttpHelper();

                //setup web wallet balance scraper
                WalletScraper scrape = new WalletScraper(http);

                //setup database
                string connectionStr = config["DatabaseSetting:ConnectionString"];
                MonitorContext context = new MonitorContext(connectionStr);//entity context
                WalletRepository repo = new WalletRepository(context);

                //setup emailer and reporting
                EmailSetting emailSet = new EmailSetting();
                config.Bind("EmailSetting", emailSet);
                SendGrid email = new SendGrid(http, emailSet);

                //setup reporter
                decimal failThreshold = Convert.ToDecimal(config["ReportingSetting:FailThreshold"]);
                ReportEngine report = new ReportEngine(repo, failThreshold, email);

                string scrapeCron = config["ReportingSetting:ScrapeScheduleCron"];
                string balanceReportCron = config["ReportingSetting:BalanceReportScheduleCron"];


                // Setup scheduler
                NameValueCollection props = new NameValueCollection
                {
                    { "quartz.serializer.type", "binary" }
                };
                StdSchedulerFactory factory = new StdSchedulerFactory(props);
                IScheduler scheduler = await factory.GetScheduler();
                await scheduler.Start();


                //Setup scrape job & needed data
                JobDataMap scrapeMap = new JobDataMap();
                scrapeMap.Add("scrape", scrape);
                scrapeMap.Add("repo", repo);
                scrapeMap.Add("report", report);

                IJobDetail scrapeJob = JobBuilder.Create<WalletScrapeJob>()
                    .WithIdentity("scrapeJob", "group1")
                    .UsingJobData(scrapeMap)
                    .Build();

                ITrigger scrapeTrigger = TriggerBuilder.Create()
                    .WithIdentity("scrapeTrigger", "group1")
                    .StartNow()
                    .WithCronSchedule(scrapeCron)
                    .Build();



                //Setup balance report job & needed data
                JobDataMap reportMap = new JobDataMap();
                reportMap.Add("report", report);

                IJobDetail reportJob = JobBuilder.Create<BalanceReportJob>()
                    .WithIdentity("reportJob", "group1")
                    .UsingJobData(reportMap)
                    .Build();

                ITrigger reportTrigger = TriggerBuilder.Create()
                    .WithIdentity("reportTrigger", "group1")
                    .StartNow()
                    .WithCronSchedule(balanceReportCron)
                    .Build();



                await scheduler.ScheduleJob(scrapeJob, scrapeTrigger);
                await scheduler.ScheduleJob(reportJob, reportTrigger);
            }
            catch (SchedulerException se)
            {
                Console.WriteLine(se);
            }
        }
    }
    

    public class WalletScrapeJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            await Console.Out.WriteLineAsync("Scrape Job Started");
            var data = context.JobDetail.JobDataMap;

            var scrape = (WalletScraper)data.Get("scrape");
            var repo = (WalletRepository)data.Get("repo");
            var report = (ReportEngine)data.Get("report");

            await Console.Out.WriteLineAsync("Getting Wallets");
            string[] walAddys = repo.GetWallets().Select(w => w.Address).ToArray();

            await Console.Out.WriteLineAsync("Getting Wallet Balances");
            var newBals = await scrape.ScrapeWalletsAsync(walAddys);

            await Console.Out.WriteLineAsync("Updating Database");
            repo.UpdateWalletsAsync(newBals);
            await Console.Out.WriteLineAsync("Scrape Job Complete");
        }
    }

    public class BalanceReportJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            await Console.Out.WriteLineAsync("Report Job Started");
            var report = (ReportEngine)context.JobDetail.JobDataMap.Get("report");
            report.SendBalanceReport();
            await Console.Out.WriteLineAsync("Report Job Completed");
        }
    }

    public class ConsoleLogProvider : ILogProvider
    {
        public Logger GetLogger(string name)
        {
            return (level, func, exception, parameters) =>
            {
                if (level >= LogLevel.Info && func != null)
                {
                    Console.WriteLine("[" + DateTime.Now.ToLongTimeString() + "] [" + level + "] " + func(), parameters);
                }
                return true;
            };
        }

        public IDisposable OpenNestedContext(string message) => throw new NotImplementedException();
        public IDisposable OpenMappedContext(string key, string value) => throw new NotImplementedException();
    }

}