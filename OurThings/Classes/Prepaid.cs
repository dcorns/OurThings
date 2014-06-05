using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OurThings.Classes
{
    public class Prepaid
    {
        public int AcctID { get; set; }
        public int ServiceTypeID { get; set; }
        public int ItemID { get; set; }
        public decimal RemoteRate { get; set; }
        public decimal InhouseRate { get; set; }
        public decimal OnsiteRate { get; set; }
        public decimal PhoneRate { get; set; }
        public decimal Balance { get; set; }
        public decimal Amount { get; set; }
        public int? JobID { get; set; }
        public DateTime LineDate { get; set; }
        public string Description{get;set;}
        public string Reference { get; set; }
        public int DocID { get; set; }
    }
}