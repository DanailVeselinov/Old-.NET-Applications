using System;

namespace DentistDB
{
    class AccountTask
    {
        public int Id { get; set; }
        public string ClientId { get; set; }
        public int CId { get; set; }
        public string DoctorId { get; set; }
        public byte ToothCode { get; set; }
        public System.DateTime Date { get; set; }
        public string Status { get; set; }
        public string Diagnose { get; set; }
        public Nullable<double> Price { get; set; }
        public Nullable<double> PriceNOI { get; set; }
        public Nullable<double> PriceDT { get; set; }
        public bool PaidByCard { get; set; }
        public bool PaidCash { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsPaid { get; set; }


    }
}
