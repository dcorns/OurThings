using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OurThings.Classes
{
    public class BOOKDATA
    {
        public int BookID { get; set; }
        public string Title { get; set; }
        public int BookAuthorID { get; set; }
        public int PublisherID { get; set; }
        public int Pages { get; set; }
        public int ISBN { get; set; }
        public bool HardBack { get; set; }
        public int LocID { get; set; }
        public int OwnerID { get; set; }
        public decimal Cost { get; set; }
        public decimal Price { get; set; }
        public decimal Appraisal { get; set; }
    }
}