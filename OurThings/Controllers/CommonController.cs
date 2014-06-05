using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OurThings.Models;
using OurThings.Classes;

namespace OurThings.Controllers
{
    
    public class CommonController : Controller
    {
        //
        // GET: /Common/
        MyJava J = new MyJava();
        MyMethods M = new MyMethods();
        
        public ActionResult Index()
        {
            return View();
        }
        public JavaScriptResult PageSelectTable(int FirstRec, string DataType, string Element, string TBLID, int RecsPerPage,string filter )
        {
            string result = @" $("+TBLID+@").remove(); ";
            result = result + J.MakeSelectionTable(Element, TBLID,"mobiletable","mobilecell","mobilerow","ledgbtn", FirstRec, RecsPerPage, DataType,filter,"");
            return JavaScript(result);
        }
        

        public JavaScriptResult TableRecordSelected(int RecID, string DataType,string RecType)
        {
            string result = "";
            switch (DataType)
            {
                case "ACCOUNTS":
                    switch (RecType)
	{
                        case "Checking":
            result = result + J.MakeCheckingDetails(RecID);
            break;
                        case "Payable":
            result = result + J.MakePayableReceivableDetails(RecID);
            break;
                        case "Expense":
            result = result + J.MakeCheckingDetails(RecID);
            break;
                        case "Savings":
            result = result + J.MakeCheckingDetails(RecID);
            break;
                        case "CreditCard":
            result = result + J.MakeCheckingDetails(RecID);
            break;
                        case "Loan":
            result = result + J.MakeCheckingDetails(RecID);
            break;
                        case "LineOfCredit":
            result = result + J.MakeCheckingDetails(RecID);
            break;
                        case "Receivable":
            result = result + J.MakeCheckingDetails(RecID);
            break;
                        case "RecurringExpense":
            result = result + J.MakeCheckingDetails(RecID);
            break;
                        case "Capital":
            result = result + J.MakeCheckingDetails(RecID);
            break;
                        case "Payroll":
            result = result + J.MakeCheckingDetails(RecID);
            break;
                        case "Cash":
            result = result + J.MakeCheckingDetails(RecID);
            break;
                        case "Equity":
            result = result + J.MakeCheckingDetails(RecID);
            break;
                        case "Inventory":
            result = result + J.MakeCheckingDetails(RecID);
            break;
                        case "Account":
            result = result + J.MakeCheckingDetails(RecID);
            break;
	}
                 //   result = result + J.MakeCheckingDetails(RecID);
                    break;
                case "ACCTLEDGER":
                    int DocID = M.GetDocumentIDFromTransactionID(RecID);
                    result = result + J.MakeDocument(DocID);
                    break;
                case "GENERALLEDGER":

                    switch (RecType)
                    {
                        case "Invoice":
                            result = result + "window.open('/Accounting/ShowInvoice/?DocID=" + RecID + @"');";
                            break;
                        
                        default:
                            result = result + J.MakeDocument(RecID);
                            break;
                    }
                    
                    break;
            }
            return JavaScript(result);
        }
        public JavaScriptResult StartMain()
        {

            string result = M.UserIsLoggedIn();

            if (result != "")
            {
                int MobilePreference = M.getUserPrefMobile().Preference;
                result = J.MakeMain(MobilePreference);
            }
            else result=J.MakeMain(1);
            return JavaScript(result);
        }
        public JavaScriptResult SetMobilePreference(int MobilePreference)
        {
            string result = M.UserIsLoggedIn();
            if (result != "")
            {
                result = M.SetUserPrefMobile(MobilePreference);
            }
            return JavaScript(result);
        }
        public JavaScriptResult SaveNewManufacturer(string Name)
        {
            string result = "";

            if (M.GetManufacturerDuplicateCount(Name) < 1 && Name != null && Name != "")
            {
                int ManID = M.SaveManufacturer(Name);
                if (ManID > 0) result = @" $(ddlManufacturerID).append('<option value=" + ManID + @">" + Name + @"</option>'); selvalManufacturerID=" + ManID + @"; ";
                else result = @" alert('New Manufacturer Save Failed'); ";
            }
            else result = @" alert('An Manufacturer by that name already exists'); ";


            return JavaScript(result);
        }
        public JavaScriptResult SaveNewOwner(string Name)
        {
            string result = "";

            if (M.GetOwnerDuplicateCount(Name) < 1 && Name != null && Name != "")
            {
                int OwnerID = M.SaveOwner(Name);
                if (OwnerID > 0) result = @" $(ddlOwner).append('<option value=" + OwnerID + @">" + Name + @"</option>'); selvalOwner=" + OwnerID + @"; ";
                else result = @" alert('New Owner Save Failed'); ";
            }
            else result = @" alert('An Owner by that name already exists'); ";


            return JavaScript(result);
        }
        public JavaScriptResult SaveItemType(string NewItemType, int RevAcct,int ExpAcct,int InvAcct,int COGAcct)
        {
            string result = "";

            if ((M.GetItemTypeDuplicateCount(NewItemType) < 1) && NewItemType != null && NewItemType != "")
            {
                int ITID = M.SaveItType(NewItemType,RevAcct,ExpAcct,InvAcct,COGAcct);
                if (ITID > 0) result = @" $(ddlItemTypeID).append('<option value=" + ITID + @">" + NewItemType + @"</option>'); selvalItemTypeID=" + ITID + @"; ";
                else result = @" alert('New Item Type Save Failed'); ";
            }
            else result = @" alert('An Item Type by that name already exists, or attempted to add a blank'); ";


            return JavaScript(result);
        }
        public JavaScriptResult GetVenderItems(int VendID)
        {
            string result = J.MakeVenderItems(VendID);
            return JavaScript(result);
        }
        [HttpPost]
        public JavaScriptResult AddContactList(int AcctID, string Element)
        {
            string result = @"$("+Element+@").append('"+J.MakeDropDown("ddlcontacts", "", "CONTACTS", "", AcctID.ToString(), "contactselected()", "*")+@"'); ";

            return JavaScript(result);
        }
    }
}
