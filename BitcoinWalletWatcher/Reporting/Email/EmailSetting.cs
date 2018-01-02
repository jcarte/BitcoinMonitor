using System;
using System.Collections.Generic;
using System.Text;

namespace BitcoinWalletWatcher.Reporting.Email
{
    public class EmailSetting
    {
        public string ApiSecretKey { get; set; }
        public string ApiUrl { get; set; }
        public string[] RecipientsEmails { get; set; }
        public string SenderEmail { get; set; }
        public string WalletAlertTemplateId { get; set; }
        public string PortfolioAlertTemplateId { get; set; }
        public string BalanceReportTemplateId { get; set; }
    }
}
