using SPKTruckUsage.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;

namespace SPKTruckUsage.Controllers
{
    public class TruckUsageController
    {
        private readonly string _sqlConnectionString;

        public TruckUsageController(string sqlConnectionString)
        {
            _sqlConnectionString = sqlConnectionString;
        }

        public List<string> GetQueue()
        {
            List<string> queueObj = new List<string>();

            string sqlText = @"SELECT TOP 10 BILLNO, BILLDATE FROM TRUCK_USAGETRG GROUP BY BILLNO, BILLDATE";
            DataSet dsResult = DsAdapter(sqlText, CommandType.Text);

            for (int i = 0; i < dsResult.Tables[0].Rows.Count; i++)
            {
                queueObj.Add(dsResult.Tables[0].Rows[i][0].ToString());
            }

            return queueObj;
        }

        public List<TruckUsage> AverageTransportCost(List<TruckUsage> trucks)
        {
            var totalCost = trucks[0].TransCostTHB;
            var totalTransWeight = trucks.Sum(q => q.TotalWeight);
            var sumTotalCost = 0;

            for (int i = 0; i < trucks.Count; i++)
            {
                var truckItem = trucks[i];

                if (i == (trucks.Count - 1))
                {
                    truckItem.TransCostTHB = totalCost - sumTotalCost;
                }
                else
                {
                    truckItem.TransCostTHB = (truckItem.TotalWeight * totalCost) / totalTransWeight;
                }
            }

            return trucks;
        }

        public List<TruckUsage> GetCommercialInvoiceAttributeOnTruckSystem(TruckUsage truck, string baseUrl, string userId, string password)
        {
            var bookingCtrl = new BookingController(baseUrl);
            var authResp = bookingCtrl.Auth(userId, password);

            if (authResp.token == "") return null;

            var bookingResp = bookingCtrl.GetBooking(truck.InvNo, authResp);
            if (bookingResp == null) return null;

            var truckUsages = new List<TruckUsage>();

            if (bookingResp.bookings.Count == 0)
            {
                truckUsages.Add(truck);
                return truckUsages;
            }

            for (int i = 0; i < bookingResp.bookings.Count; i++)
            {
                var copyTruck = (TruckUsage)truck.Clone();
                var booking = bookingResp.bookings[i];
                copyTruck.DeliveryNo = booking.DELIVERY_JOB_NO;
                copyTruck.DeliveryDate = booking.START_DATE;
                copyTruck.StartDate = booking.START_DATE;
                copyTruck.EndDate = booking.END_DATE;
                copyTruck.ServiceTypeCode = booking.SERVICE_TYPE_CODE;
                copyTruck.RouteFromId = booking.ROUTE_FROM_ID;
                copyTruck.RouteFromCode = booking.ROUTE_FROM_CODE;
                copyTruck.RouteFromName = booking.ROUTE_FROM_NAME;
                copyTruck.RouteToId = booking.ROUTE_TO_ID;
                copyTruck.RouteToCode = booking.ROUTE_TO_CODE;
                copyTruck.RouteToName = booking.ROUTE_TO_NAME;
                copyTruck.TruckRegistrationNo = booking.OWNER_TRUCK_REGISTRATION_NO;
                copyTruck.TruckTypeCode = booking.TRUCK_TYPE_CODE;
                copyTruck.TruckTypeName = booking.TRUCK_TYPE_NAME;
                copyTruck.TotalWeight = booking.TOTAL_WEIGHT;
                copyTruck.BookingNo = booking.BOOKING_NO;
                copyTruck.BookingSeq = booking.SEQUENCE_NO;
                truckUsages.Add(copyTruck);
            }

            return truckUsages;
        }

        public List<TruckUsage> FindBilling(string billNo)
        {
            var dsResult = DsAdapter("sp_fcas150_find_billno",
                    CommandType.StoredProcedure,
                    new SqlParameter[] { new SqlParameter("@billno", billNo) });

            if (dsResult == null || dsResult.Tables[0].Rows.Count == 0) return null;

            List<TruckUsage> truckUsages = new List<TruckUsage>();

            for (int i = 0; i < dsResult.Tables[0].Rows.Count; i++)
            {
                TruckUsage truckUsage = new TruckUsage();
                DataRow dr = dsResult.Tables[0].Rows[i];
                truckUsage.BillNo = dr["ABIVNO"].ToString();
                truckUsage.BillDate = DateTime.ParseExact(dr["ABIVYMD"].ToString(), "yyyyMMdd", CultureInfo.InvariantCulture);
                truckUsage.InvNo = dr["ABYREF"].ToString();
                truckUsage.ShipmentType = dr["SHPTYP"].ToString();
                truckUsage.GrossWeight = Convert.ToDecimal(dr["GWT"]);
                truckUsage.WeightUnit = "KGS";
                truckUsage.CargoValue = Convert.ToDecimal(dr["CARGOVAL"]);
                truckUsage.Currency = dr["CURRENCY"].ToString();
                truckUsage.TransCostTHB = Convert.ToDecimal(dr["ABTKAM"]);
                truckUsages.Add(truckUsage);
            }

            return truckUsages;
        }

        public void InsertBatch(params TruckUsage[] trucks)
        {
            for (int i = 0; i < trucks.Length; i++)
            {
                Insert(trucks[i]);
            }
        }

        public bool Insert(TruckUsage truck)
        {
            return Execute("sp_truck_usageinfo_insert", CommandType.StoredProcedure, new SqlParameter[] {
                new SqlParameter("@BillNo", truck.BillNo),
                new SqlParameter("@BillDate", truck.BillDate),
                new SqlParameter("@InvNo", truck.InvNo),
                new SqlParameter("@ShipmentType", truck.ShipmentType),
                new SqlParameter("@GrossWeight", truck.GrossWeight),
                new SqlParameter("@WeightUnit", truck.WeightUnit),
                new SqlParameter("@CargoValue", truck.CargoValue),
                new SqlParameter("@Currency", truck.Currency),
                new SqlParameter("@DeliveryNo", truck.DeliveryNo),
                new SqlParameter("@DeliveryDate", truck.DeliveryDate),
                new SqlParameter("@StartDate", truck.StartDate),
                new SqlParameter("@EndDate", truck.EndDate),
                new SqlParameter("@TruckTypeCode", truck.TruckTypeCode),
                new SqlParameter("@TruckTypeName", truck.TruckTypeName),
                new SqlParameter("@TruckRegistrationNo", truck.TruckRegistrationNo),
                new SqlParameter("@ServiceTypeCode", truck.ServiceTypeCode),
                new SqlParameter("@RouteFromId", truck.RouteFromId),
                new SqlParameter("@RouteFromCode", truck.RouteFromCode),
                new SqlParameter("@RouteFromName", truck.RouteFromName),
                new SqlParameter("@RouteToId", truck.RouteToId),
                new SqlParameter("@RouteToCode", truck.RouteToCode),
                new SqlParameter("@RouteToName", truck.RouteToName),
                new SqlParameter("@TransCostTHB", truck.TransCostTHB),
                new SqlParameter("@TransWeight", truck.TotalWeight),
                new SqlParameter("@BookingNo", truck.BookingNo),
                new SqlParameter("@BookingSeq", truck.BookingSeq)
            });
        }

        public bool Update(TruckUsage truck)
        {
            return Execute("sp_truck_usageinfo_update", CommandType.StoredProcedure, new SqlParameter[] {
                new SqlParameter("@TrxNo", truck.TrxNo),
                new SqlParameter("@BillNo", truck.BillNo),
                new SqlParameter("@BillDate", truck.BillDate),
                new SqlParameter("@InvNo", truck.InvNo),
                new SqlParameter("@ShipmentType", truck.ShipmentType),
                new SqlParameter("@GrossWeight", truck.GrossWeight),
                new SqlParameter("@WeightUnit", truck.WeightUnit),
                new SqlParameter("@CargoValue", truck.CargoValue),
                new SqlParameter("@Currency", truck.Currency),
                new SqlParameter("@DeliveryNo", truck.DeliveryNo),
                new SqlParameter("@DeliveryDate", truck.DeliveryDate),
                new SqlParameter("@StartDate", truck.StartDate),
                new SqlParameter("@EndDate", truck.EndDate),
                new SqlParameter("@TruckTypeCode", truck.TruckTypeCode),
                new SqlParameter("@TruckTypeName", truck.TruckTypeName),
                new SqlParameter("@TruckRegistrationNo", truck.TruckRegistrationNo),
                new SqlParameter("@ServiceTypeCode", truck.ServiceTypeCode),
                new SqlParameter("@RouteFromId", truck.RouteFromId),
                new SqlParameter("@RouteFromCode", truck.RouteFromCode),
                new SqlParameter("@RouteFromName", truck.RouteFromName),
                new SqlParameter("@RouteToId", truck.RouteToId),
                new SqlParameter("@RouteToCode", truck.RouteToCode),
                new SqlParameter("@RouteToName", truck.RouteToName),
                new SqlParameter("@TransCostTHB", truck.TransCostTHB),
                new SqlParameter("@TrxState", truck.TrxState)
            });
        }

        public void Delete(List<TruckUsage> trucks)
        {
            for (int i = 0; i < trucks.Count; i++)
            {
                Execute(@"DELETE FROM TRUCK_USAGETRG WHERE BILLNO=@billno AND INVNO=@invno", CommandType.Text, new SqlParameter[] {
                new SqlParameter("@billno", trucks[i].BillNo),
                new SqlParameter("@invno", trucks[i].InvNo)
            });
            }
        }

        private DataSet DsAdapter(string commandText, CommandType commandType, params SqlParameter[] parameters)
        {
            DataSet dsResult = null;
            using (SqlConnection conn = new SqlConnection(_sqlConnectionString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = commandText;
                cmd.CommandType = commandType;

                if (parameters.Length > 0)
                {
                    cmd.Parameters.AddRange(parameters);
                }

                var adapter = new SqlDataAdapter(cmd);
                dsResult = new DataSet();
                adapter.Fill(dsResult);
            }
            return dsResult;
        }

        private bool Execute(string commandText, CommandType commandType, params SqlParameter[] parameters)
        {
            bool result = false;
            using (SqlConnection conn = new SqlConnection(_sqlConnectionString))
            {
                try
                {
                    conn.Open();
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = commandText;
                    cmd.CommandType = commandType;

                    if (parameters.Length > 0)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }

                    cmd.ExecuteNonQuery();
                    result = true;
                }
                catch { }
            }

            return result;
        }
    }
}