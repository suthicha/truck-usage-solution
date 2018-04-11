using System;

namespace SPKTruckUsage.Models
{
    public class TruckUsage : ICloneable
    {
        public int TrxNo { get; set; }
        public String BillNo { get; set; }
        public DateTime? BillDate { get; set; }
        public String InvNo { get; set; }
        public String ShipmentType { get; set; }
        public decimal? GrossWeight { get; set; }
        public String WeightUnit { get; set; }
        public decimal? CargoValue { get; set; }
        public String Currency { get; set; }
        public String DeliveryNo { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public String TruckTypeCode { get; set; }
        public String TruckTypeName { get; set; }
        public String TruckRegistrationNo { get; set; }
        public String ServiceTypeCode { get; set; }
        public int? RouteFromId { get; set; }
        public String RouteFromCode { get; set; }
        public String RouteFromName { get; set; }
        public int? RouteToId { get; set; }
        public String RouteToCode { get; set; }
        public String RouteToName { get; set; }
        public decimal? TransCostTHB { get; set; }
        public String TrxState { get; set; }
        public DateTime? CreateDateTime { get; set; }
        public DateTime? UpdateDateTime { get; set; }

        public decimal? TotalWeight { get; set; }

        public string BookingNo { get; set; }
        public int BookingSeq { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}