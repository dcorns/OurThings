using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OurThings.Classes
{
    public class SOFTDATA
    {
        public int SoftID { get; set; }
        public string Title { get; set; }
        public int MediaTypeID { get; set; }
        public int PublisherID { get; set; }
        public int ISBN { get; set; }
        public int LocID { get; set; }
        public int OwnerID { get; set; }
        public decimal Cost { get; set; }
        public decimal Price { get; set; }
        public decimal Appraisal { get; set; }
    }
}