using Newtonsoft.Json;
using SPKTruckUsage.Controllers;
using SPKTruckUsage.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace SPKTruckUsage.TestApp
{
    public partial class Form1 : Form
    {
        private string _token = string.Empty;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            HttpWebRequest httpRequest = WebRequest.Create(@"http://truckcenter.ctibkk.com:8999/user/auth") as HttpWebRequest;
            httpRequest.ContentType = "application/json";
            httpRequest.Method = "POST";

            using (var swr = new StreamWriter(httpRequest.GetRequestStream()))
            {
                var authData = new AuthData
                {
                    userId = "",
                    password = ""
                };

                string json = JsonConvert.SerializeObject(authData);

                swr.Write(json);
            }

            var httpResponse = (HttpWebResponse)httpRequest.GetResponse();

            using (var reader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = reader.ReadToEnd();
                var authResponse = JsonConvert.DeserializeObject<RestAuth>(result);
                _token = authResponse.token;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (_token == "") return;

            MessageBox.Show(_token);

            string invoiceNo = "CHU00170            ";
            string uri = string.Format(@"http://truckcenter.ctibkk.com:8999/booking/invoice/{0}?token={1}", invoiceNo, _token);

            HttpWebRequest httpRequest = WebRequest.Create(uri) as HttpWebRequest;
            httpRequest.ContentType = "application/json";
            httpRequest.Method = "GET";

            var httpResponse = (HttpWebResponse)httpRequest.GetResponse();

            using (var reader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = reader.ReadToEnd();
                var bookingResponse = JsonConvert.DeserializeObject<RestBooking>(result);
                MessageBox.Show(result);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var ctrl = new BookingController("http://truckcenter.ctibkk.com:8999");
            var authResp = ctrl.Auth("", "");

            // MessageBox.Show(authResp.token);
            var bookingResp = ctrl.GetBooking("", authResp);

            MessageBox.Show("OK");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var connection = @"";
            var truckCtrl = new TruckUsageController(connection);

            var queue = truckCtrl.GetQueue();

            for (int i = 0; i < queue.Count; i++)
            {
                try
                {
                    var billno = queue[i];

                    var billings = truckCtrl.FindBilling(billno);

                    var truckBillings = new List<TruckUsage>();
                    for (int j = 0; j < billings.Count; j++)
                    {
                        var truckUsages = truckCtrl.GetCommercialInvoiceAttributeOnTruckSystem(
                            billings[j], @"http://truckcenter.ctibkk.com:8999",
                            "", "");

                        truckBillings.AddRange(truckUsages);
                    }

                    truckBillings = truckCtrl.AverageTransportCost(truckBillings);
                    truckCtrl.InsertBatch(truckBillings.ToArray());
                    truckCtrl.Delete(truckBillings);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

            MessageBox.Show("OK");
        }
    }

    public class AuthData
    {
        public string userId { get; set; }
        public string password { get; set; }
    }

    public class RestAuth
    {
        public string message { get; set; }
        public string token { get; set; }
    }

    public class RestBooking
    {
        public string invoiceno { get; set; }
        public List<Booking> bookings { get; set; }
    }
}