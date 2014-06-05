using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OurThings.Classes
{
    public class InvoiceDetail
    {
        public string InvoiceNumber { get; set; }
        public int InvoiceDocID { get; set; }
        public decimal InvoiceAmount { get; set; }
        public decimal UnpaidAmount { get; set; }
        public List<TransactionTable> Payments { get; set; }
        public int DebitAcctID { get; set; }
    }
}