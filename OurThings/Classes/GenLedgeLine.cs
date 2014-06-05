using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OurThings.Classes
{
    public class GenLedgeLine
    {
        public int DocID { get; set; }
        public DateTime DocTime { get; set; }
        public string DocRef { get; set; }
        public string DocType { get; set; }
        public int DocTypeID { get; set; }
        public decimal DocAmt { get; set; }
        public decimal DocCredits { get; set; }
        public decimal DocDebits { get; set; }
    }
}