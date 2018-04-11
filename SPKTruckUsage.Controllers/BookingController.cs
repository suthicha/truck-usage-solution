using Newtonsoft.Json;
using SPKTruckUsage.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace SPKTruckUsage.Controllers
{
    public class BookingController
    {
        private readonly string _baseUrl;

        public BookingController(string baseUrl)
        {
            _baseUrl = baseUrl;
        }

        public AuthResponse Auth(string userId, string password)
        {
            try
            {
                string uri = string.Format("{0}/user/auth", _baseUrl);
                HttpWebRequest httpRequest = WebRequest.Create(uri) as HttpWebRequest;
                httpRequest.ContentType = "application/json";
                httpRequest.Method = "POST";

                using (var swr = new StreamWriter(httpRequest.GetRequestStream()))
                {
                    var authData = new AuthData
                    {
                        userId = userId,
                        password = password
                    };

                    string json = JsonConvert.SerializeObject(authData);

                    swr.Write(json);
                }

                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();

                using (var reader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = reader.ReadToEnd();
                    return JsonConvert.DeserializeObject<AuthResponse>(result);
                }
            }
            catch
            {
            }

            return null;
        }

        public BookingResponse GetBooking(string invoiceno, AuthResponse auth)
        {
            try
            {
                invoiceno = invoiceno.Replace("/", "&sol;");
                string uri = string.Format(@"{0}/booking/invoice/{1}?token={2}", _baseUrl, invoiceno, auth.token);

                HttpWebRequest httpRequest = WebRequest.Create(uri) as HttpWebRequest;
                httpRequest.ContentType = "application/json";
                httpRequest.Method = "GET";

                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();

                using (var reader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = reader.ReadToEnd();
                    return JsonConvert.DeserializeObject<BookingResponse>(result);
                }
            }
            catch { }

            return null;
        }
    }

    public class AuthData
    {
        public string userId { get; set; }
        public string password { get; set; }
    }

    public class AuthResponse
    {
        public string message { get; set; }
        public string token { get; set; }
    }

    public class BookingResponse
    {
        public string invoiceno { get; set; }
        public List<Booking> bookings { get; set; }

        public string GetCombineDeliveryNo()
        {
            if (bookings == null || bookings.Count == 0) return string.Empty;

            List<string> arry = new List<string>();
            bookings.ForEach((item) =>
            {
                arry.Add(item.DELIVERY_JOB_NO);
            });

            return arry.Aggregate((a, b) => a + "," + b);
        }

        public string GetCombineTruckType()
        {
            if (bookings == null || bookings.Count == 0) return string.Empty;

            List<string> arry = new List<string>();
            bookings.ForEach((item) =>
            {
                arry.Add(item.TRUCK_TYPE_NAME);
            });

            return arry.Aggregate((a, b) => a + "," + b);
        }

        public string GetCombineRegistrationNo()
        {
            if (bookings == null || bookings.Count == 0) return string.Empty;

            List<string> arry = new List<string>();
            bookings.ForEach((item) =>
            {
                arry.Add(item.OWNER_TRUCK_REGISTRATION_NO);
            });

            return arry.Aggregate((a, b) => a + "," + b);
        }
    }
}