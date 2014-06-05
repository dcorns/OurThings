using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OurThings.Classes
{
    public class DepLine
    {
        public int DepLineID { get; set; }
        public string TransType { get; set; }
        public decimal AMT { get; set; }
        public string PayerName { get; set; }
    }
}