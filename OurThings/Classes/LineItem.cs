using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OurThings.Classes
{
    public class LineItem
    {
        public decimal Quantity { get; set; }
        public int ItemID { get; set; }
        public string ItemName { get; set; }
        public decimal Price { get; set; }
        public bool Taxable { get; set; }
        public bool Allocated { get; set; }
        public int LineItemStatus { get; set; }
        public string SerialNumber { get; set; }
        public int TransactionLineID { get; set; }
        public int DocID { get; set; }
        public int ExpenseAcct { get; set; }
        public int LocationID { get; set; }
        public int? JobLineID { get; set; }
        public string Description { get; set; }
        public DateTime TimeIn { get; set; }
        public DateTime TimeOut { get; set; }
        public int JobID { get; set; }
        public int EmployeeID { get; set; }
        public int ServiceCreditTypeID { get; set; }
        public string ServiceCreditTypeName { get; set; }
        public DateTime DateOfService { get; set; }
        public decimal Balance { get; set; }
       
    }
}