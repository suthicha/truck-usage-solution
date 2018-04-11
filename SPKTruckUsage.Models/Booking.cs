using System;

namespace SPKTruckUsage.Models
{
    public class Booking
    {
        public string DELIVERY_JOB_NO { get; set; }
        public DateTime START_DATE { get; set; }
        public DateTime END_DATE { get; set; }

        public string BOOKING_TYPE_CODE { get; set; }
        public string TRUCK_TYPE_CODE { get; set; }
        public string TRUCK_TYPE_NAME { get; set; }
        public string OWNER_TRUCK_REGISTRATION_NO { get; set; }
        public string SERVICE_TYPE_CODE { get; set; }
        public int ROUTE_FROM_ID { get; set; }
        public string ROUTE_FROM_CODE { get; set; }
        public string ROUTE_FROM_NAME { get; set; }
        public int ROUTE_TO_ID { get; set; }
        public string ROUTE_TO_CODE { get; set; }
        public string ROUTE_TO_NAME { get; set; }

        public Decimal TOTAL_WEIGHT { get; set; }
        public string BOOKING_NO { get; set; }
        public int SEQUENCE_NO { get; set; }
    }
}