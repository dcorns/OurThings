using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OurThings.Classes
{
    public class TransDocument
    {
        public int DocID { get; set; }
        public decimal Total { get; set; }
        public int CustVendAcctID { get; set; }
        public string CustVendName { get; set; }
        public int TaxLocID { get; set; }
        public string TaxLocName { get; set; }
        public string TaxRate { get; set; }
        public decimal Tax { get; set; }
        public List<LineItem> items { get; set; }
        public string PayMeth { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Phone { get; set; }
        public string Reference { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public string EmailTo { get; set; }
        public string DocumentNumber { get; set; }
        public string Balance { get; set; }
    }
}