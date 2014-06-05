using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OurThings.Classes
{
    public class DepositItem
    {
        public int ItemTransID { get; set; }
        public int ItemAcctID { get; set; }
        public string Reference { get; set; }
        public decimal Amount { get; set; }
        public int DepositType { get; set; }
        public string DepTypeName { get; set; }
        public string AcctName { get; set; }
    }
}