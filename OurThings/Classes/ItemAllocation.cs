using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OurThings.Classes
{
    public class ItemAllocation
    {
        public int LineID { get; set; }
        public int ItemID{get;set;}
        public decimal Quantity { get; set; }
        public int AllocationType { get; set; }
        public int AllocationID { get; set; }
    }
}