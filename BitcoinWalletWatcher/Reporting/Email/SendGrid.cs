using BitcoinWalletWatcher.Utilities;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Dynamic;

namespace BitcoinWalletWatcher.Reporting.Email
{
    public class SendGrid
    {
        HttpHelper _http;
        EmailSetting _setting;
        public SendGrid(HttpHelper http, EmailSetting setting)
        {
            _http = http;
            _setting = setting;
        }

        /// <summary>
        /// Send an email warning that a single wallet has fallen below the critical level
        /// </summary>
        /// <param name="wal">Details of wallet that needs to be alerted about</param>
        public void SendSingleWalletAlert(WalletReport wal)
        {
            dynamic o = GetBaseSendGridObject();

            var dic = new Dictionary<string, string>()//replacements to be made in template
            {
                {"<%address%>",wal.Address},
                {"<%maxbalance%>",wal.MaxBalanceBTC.ToString("N0")},
                {"<%currentbalance%>",wal.CurrentBalanceBTC.ToString("N0")},
                {"<%percentage%>",wal.PercentOfMax.ToString("P1")}
            };
            o.personalizations[0].substitutions = dic;
            o.template_id = _setting.WalletAlertTemplateId;

            string body = JsonConvert.SerializeObject(o);

            SendMessageAsync(body);
        }

        /// <summary>
        /// Send an email warning that all wallets have collectively fallen below a critical level
        /// </summary>
        /// <param name="rep">Report of all data required to produce the alert</param>
        public void SendPortfolioAlert(PortfolioReport port)
        {
            dynamic o = GetBaseSendGridObject();

            var dic = new Dictionary<string, string>()//replacements
            {
                {"<%numfailingwallets%>", $"{port.TotalNumberOfFailingWallets} of {port.TotalNumberOfMonitoredWallets}"},
                {"<%maxbalance%>",port.MaxTotalBalanceBTC.ToString("N0")},
                {"<%currentbalance%>",port.CurrentTotalBalanceBTC.ToString("N0")},
                {"<%percentage%>",port.PercentOfMax.ToString("P1")}
            };
            o.personalizations[0].substitutions = dic;
            o.template_id = _setting.PortfolioAlertTemplateId;

            string body = JsonConvert.SerializeObject(o);

            SendMessageAsync(body);
        }

        /// <summary>
        /// Send report showing current state of wallets being monitored
        /// </summary>
        /// <param name="port">Report containing details to put in email</param>
        public void SendBalanceReport(PortfolioReport port)
        {
            dynamic o = GetBaseSendGridObject();

            var dic = new Dictionary<string, string>()//replacements
            {
                {"<%numfailingwallets%>", $"{port.TotalNumberOfFailingWallets} of {port.TotalNumberOfMonitoredWallets}"},
                {"<%maxbalance%>",port.MaxTotalBalanceBTC.ToString("N0")},
                {"<%currentbalance%>",port.CurrentTotalBalanceBTC.ToString("N0")},
                {"<%percentage%>",port.PercentOfMax.ToString("P1")}
            };
            o.personalizations[0].substitutions = dic;
            o.template_id = _setting.BalanceReportTemplateId;

            string body = JsonConvert.SerializeObject(o);

            SendMessageAsync(body);
        }

        /// <summary>
        /// Get the base request object for sending to sendgrid
        /// </summary>
        /// <returns></returns>
        private ExpandoObject GetBaseSendGridObject()
        {
            dynamic o = new ExpandoObject();

            o.personalizations = new object[] { new ExpandoObject() };//array with single
            o.personalizations[0].to = new List<object>();

            for (int i = 0; i < _setting.RecipientsEmails.Length; i++)//create dyanmic to object for all recipients
            {
                dynamic r = new ExpandoObject();//recipient
                r.email = _setting.RecipientsEmails[i];
                r.name = _setting.RecipientsEmails[i];
                o.personalizations[0].to.Add(r);
            }

            o.from = new ExpandoObject();//setup single from object
            o.from.email = _setting.SenderEmail;
            o.from.name = _setting.SenderEmail;

            return o;
            ////Example JSON
            //{
            //	"personalizations": [{
            //		"to": [{
            //			"email": "jcarter1987@gmail.com",
            //			"name": "james"
            //		}],
            //		"substitutions": {
            //			"<%address%>": "abcdefghi234",
            //			"<%maxbalance%>": "1,258",
            //			"<%currentbalance%>": "126",
            //			"<%percentage%>": "10.1"
            //		}
            //	}],
            //	"from": {
            //		"email": "bitcoinalert@jicola.co.uk",
            //		"name": "james"
            //	},
            //	"template_id": "6c94afbc-3518-441e-806b-54a36784d45d"
            //}
        }

        //TODO fix sendgrid spam issue, possible to unsubscribe to transactional emails?
        //maybe setup as marketing emails, add users to list and just sent to all with substituions
        
        /// <summary>
        /// Send the request to the sendgrid api
        /// </summary>
        /// <param name="bodyContent">body to send with request</param>
        private async void SendMessageAsync(string bodyContent)
        {
            string url = _setting.ApiUrl;
            var head = new Dictionary<string, string>()
            {
                {"Authorization",$"Bearer {_setting.ApiSecretKey}"}
            };

            await _http.PostJsonAsync(url, bodyContent, head);
        }
    }
}
