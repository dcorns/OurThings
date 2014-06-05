using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OurThings.Classes
{
    public class PaymentDetail
    {
        public Document PayDoc { get; set; }
        public DateTime PDate { get; set; }
        public TransactionTable PayTrans { get; set; }
        public int PaymentCreditID { get; set; }
        public string PaymentCreditName { get; set; }
        public int PaymentDebitID { get; set; }
        public string PaymentDebitName { get; set; }
        public List<TransactionTable> PayAllocations { get; set; }
        public Decimal UnallocatedPayment { get; set; }
    }
}