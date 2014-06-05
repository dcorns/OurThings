using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OurThings.Classes
{
    public class ITEMDATA
    {
        public int ItemID { get; set; }
        public string Name { get; set; }
        public int ItemTypeID { get; set; }
        public int ManufacturerID { get; set; }
        public string UPC { get; set; }
        public string Description { get; set; }
        public decimal Quantity { get; set; }
        public bool ItemNew { get; set; }
        public bool ItemTested { get; set; }
        public int LocID { get; set; }
        public int OwnerID { get; set; }
        public decimal Cost { get; set; }
        public decimal Price { get; set; }
        public decimal Appraisal { get; set; }
        public int LineID { get; set; }
        public bool Serialized { get; set; }
        public int RevAcct { get; set; }
        public int ExpAcct { get; set; }
        public int InvAcct { get; set; }
    }
}