using SPKHelperPackage.Logs;
using SPKTruckUsage.Controllers;
using SPKTruckUsage.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.ServiceProcess;

namespace SPKTruckUsage.Service
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            timer1.Interval = 3000;
            try
            {
                timer1.Interval = Convert.ToInt32(ConfigurationManager.AppSettings["interval"]);
            }
            catch { }
            timer1.Start();
            Logged.Event("Start Service");
        }

        protected override void OnStop()
        {
            timer1.Stop();
            Logged.Event("Stop Service");
        }

        private void timer1_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            timer1.Enabled = false;
            try
            {
                string sqlConnectionString = ConfigurationManager.AppSettings["ConnectionString"];
                string apiBaseUrl = ConfigurationManager.AppSettings["api_baseUrl"];
                string apiUserId = ConfigurationManager.AppSettings["api_userid"];
                string apiPassword = ConfigurationManager.AppSettings["api_password"];

                var truckCtrl = new TruckUsageController(sqlConnectionString);
                var queue = truckCtrl.GetQueue();

                for (int i = 0; i < queue.Count; i++)
                {
                    try
                    {
                        var billno = queue[i];
                        var truckBillings = new List<TruckUsage>();
                        var billings = truckCtrl.FindBilling(billno);

                        Logged.Event("Find billing no " + billno);

                        for (int j = 0; j < billings.Count; j++)
                        {
                            var truckUsages = truckCtrl.GetCommercialInvoiceAttributeOnTruckSystem(
                                billings[j], apiBaseUrl, apiUserId, apiPassword);

                            truckBillings.AddRange(truckUsages);
                        }

                        truckBillings = truckCtrl.AverageTransportCost(truckBillings);

                        truckCtrl.InsertBatch(truckBillings.ToArray());
                        Logged.Event("Insert billing no " + billno);

                        truckCtrl.Delete(truckBillings);
                        Logged.Event("Delete billing no " + billno);
                    }
                    catch (Exception ex)
                    {
                        Logged.Error("PROCESS", ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Logged.Error("Timer", ex.Message);
            }
            timer1.Enabled = true;
        }
    }
}