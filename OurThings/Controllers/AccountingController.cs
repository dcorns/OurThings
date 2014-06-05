using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OurThings.Models;
using OurThings.Classes;
using System.Web.Security;

namespace OurThings.Controllers
{
    public class AccountingController : Controller
    {
        //
        // GET: /Accounting/
        MyJava J = new MyJava();
        MyMethods M = new MyMethods();
        [Authorize(Roles = "PowerUser")]
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public JavaScriptResult StartAccounting()
        {
            string result = J.MakeAccountingMain();
            return JavaScript(result);
        }
        [HttpPost]
        public JavaScriptResult NewAccount()
        {
            string result = J.MakeNewAccount();
            return JavaScript(result);
        }
        [HttpPost]
        public JavaScriptResult SaveNewAccount(string AcctName, string AcctNumber, int AcctType)
        {
            
            string result = "";
            string saveresult="";
            if (AcctType > 0)
            {
                try
                {
                    saveresult = M.SaveNewAccount(AcctName, AcctNumber, AcctType);
                    int ACCTID = Convert.ToInt32(saveresult);
                        result = J.MakeContact("TopDiv", 0, ACCTID);
                }
                catch
                {
                    result = J.SaveFailed(saveresult);
                }
            }
            else result = J.SaveFailed("Select a valid Account Type");
            
            return JavaScript(result);
        }

        [HttpPost]
        public JavaScriptResult NewPurchase()
        {
            string result = J.MakePurchase();
            return JavaScript(result);
        }
        [HttpPost]
        public JavaScriptResult TransferFunds()
        {
            string result = J.MakeTransferFunds();
            return JavaScript(result);
        }
        [HttpPost]
        public JavaScriptResult SaveTransChanges(int TransactionID, string DocPath)
        {
            string result = "";// M.SaveTransActionDetails(TransactionID, DocPath);
            return JavaScript(result);
        }
        [HttpPost]
        public JavaScriptResult SaveFundsTransfer(string Tdate, int Cact, int Dact, int Tmethod, string Desc, string Tamt, string Reference, int PayAct, string FEEAMT)
        {
            string result = @" $(TopDiv).empty(); $(PitemDiv).empty(); $(LocDivMain).empty();";
            int FeeAct = 2;//Fixed and Required Transaction Fee account Location
            
                try
                {
                    decimal TAMT = Convert.ToDecimal(Tamt.Substring(1));
                    if (FEEAMT.Length < 4) FEEAMT = "$0.00";
                    decimal FeeAmt = Convert.ToDecimal(FEEAMT.Substring(1));
                    DateTime DATE = Convert.ToDateTime(Tdate);
                    result = M.SaveFundsTransfer(DATE, Cact, Dact, Tmethod, Reference, Desc, TAMT, FeeAmt,  PayAct,FeeAct);
                    
                        result = result + J.MakeAccountingMain();
                   
                }
                catch
                {
                    result = @" alert('A problem occurred. Please check fields for errors.'); ";
                }
            
            return JavaScript(result);
        }
       
        [HttpPost]
        public JavaScriptResult NewSale()
        {
            string result = J.MakeNewSale();

            return JavaScript(result);
        }
        [HttpPost]
        public JavaScriptResult SaveSale(string IDate,int CAcct,string LineItems, string tax, int PayMethod, bool Paid, string TaxRateName, decimal TaxRate, string Sns)
        {
            DateTime Date = new DateTime(); Decimal TAX = 0;
            string result = "";
            
            try
            {
                Date = Convert.ToDateTime(IDate);
                
                if (PayMethod == 6)
                {
                    int JobID = 0;//This will have to come from input eventually
                    result = M.RecordServiceCreditDeduction(Date, Convert.ToInt32(CAcct), LineItems, Convert.ToInt32(PayMethod),JobID) + J.MakeAccountingMain();
                }
                else
                {
                    TAX = Convert.ToDecimal(tax);
                    result = M.RecordSale(Date, Convert.ToInt32(CAcct), LineItems, TAX, Convert.ToInt32(PayMethod), Paid, TaxRateName, TaxRate) + J.MakeAccountingMain();//Change to resetting new sale, but also updating account summary data
                }
            }
            catch (Exception e)
            {

                result = @" alert('Save Sale Input invalid, Please check the input fields for errors: " + e.Message + @"');  ";
            }
            
            return JavaScript(result);
        }
        [HttpPost]
        public JavaScriptResult NewCustomer()
        {
            string result = J.MakeQuickAddAccount(15);

            return JavaScript(result);
        }
        [HttpPost]
        public JavaScriptResult getItemStatus(int ItemID)
        {
            string result = "";

            return JavaScript(result);
        }
        [HttpPost]
        public JavaScriptResult ReceivePayment()
        {
            string result = J.MakeReceivePayment();
            return JavaScript(result);
        }
        [HttpPost]
        public JavaScriptResult ApplyPayments()
        {
            string result = J.MakeApplyPayments();
            return JavaScript(result);
        }
        [HttpPost]
        public JavaScriptResult AllocatePayment(string DOCS)
        {
            //receives payment docID, invoice doc ID
            //subtract invoice amount from payment if there is enough, otherwise deductuct amount of payment from invoice
            //allocate reveneue, taxes etc.
            string result = @" alert(Payment Allocation Failed!'); ";
            int PayDoc = Convert.ToInt32(DOCS.Substring(0, DOCS.IndexOf(",")));
            int InvDoc=Convert.ToInt32(DOCS.Substring(DOCS.IndexOf(",")+1));
            bool success=M.AllocatePayment(PayDoc, InvDoc);
            if (success) result = J.MakeApplyPayments();

            return JavaScript(result);
        }
        [HttpPost]
        public JavaScriptResult SaveReceivedPayment(string Pdate, int Pact, string DepRef,bool DI, int Pmethod, string PayRef, string Desc, string Pamt, int BankID, string AppliedInvoices)
        {
            bool success = false;
            string result = @" alert('Payment Saved'); ";
            DateTime DATE=new DateTime();
            decimal AMT=0;
            try
            {
                AMT = Convert.ToDecimal(Pamt.Substring(1));
            }
            catch
            {
                return JavaScript(@" alert('Failed-Check for valid amount'); ");
            }
            try
            {
                DATE = Convert.ToDateTime(Pdate);
            }
            catch
            {
                return JavaScript(@" alert('Failed-Check for valid date'); ");
            }
            try
            {

                int DepLineID = M.SavePaymentReceived(DATE, Pact, Pmethod, PayRef, Desc, AMT, AppliedInvoices);
                    if (DepLineID>0)
                    {
                    if (DI)//Deposit Immediately
                    {
                        
                        success=SaveSingleItemDeposit(Pdate, Pmethod, BankID, "Immediate Deposit", AMT, DepRef,DepLineID);
                        if (success)
                        {
                            result = @" alert('Payment with Imediate Deposit Saved'); ";
                            result=result + @" $(TopDiv).empty(); " + J.MakeAccountingMain();
                        }
                    }
                    else
                    {

                    result = result + @" $(TopDiv).empty(); " + J.MakeAccountingMain();
                    
                    }
                    }
                
            }
            catch (Exception e)
            {
                result = @" alert('Payment Save Failed--"+e.Data+@"'); ";
            }
            return JavaScript(result);
        }
        [HttpPost]
        public JavaScriptResult GetPayAgainstDocList(int AcctID)
        {
            string result = J.MakeOutStandingDocumentList(AcctID);

            return JavaScript(result);
        }
        //[HttpPost]
        //public JavaScriptResult ApplyPayment(int AcctID, DateTime Date,int PayMeth,string PayRef,string InvRef, string Description, decimal PAMT,decimal InvAMT)
        //{
        //    Description = "Payment Applied-"+Description;
        //    M.SaveAppliedPaymentReceived(Date, AcctID, PayMeth, PayRef, Description, InvAMT);
        //    string result = "";

        //    return JavaScript(result);
        //}
        [HttpPost]
        public JavaScriptResult YesDeposit(string Pdate, int Cashact, int Pmethod, string DocRef, decimal AMT)
        {
            string result = "";
            result = result + J.MakeQuickDeposit(Pdate, Cashact, Pmethod, AMT);
            return JavaScript(result);
        }
        [HttpPost]
        public bool SaveSingleItemDeposit(string Ddate, int Dmethod, int BankAct, string Desc, decimal AMT, string DocRef, int DepLineID)
        {
            bool result = true;
            if (DocRef.Length < 0) DocRef = "*";
            if (Desc.Length < 0) Desc = "*";
            List<DepositLine> receipt = new List<DepositLine>();
            receipt.Add(M.getDepositLineFromDepLineID(DepLineID));
            try
            {
                DateTime D = Convert.ToDateTime(Ddate);
                M.SaveDeposit(D,  Dmethod, BankAct, Desc, DocRef, receipt);
                
            }
            catch (Exception e)
            {
                result = false;
            }
            return result;
            
        }
        [HttpPost]
        public JavaScriptResult MakePurchase()
        {
            string result = J.MakePurchase();
               return JavaScript(result);
        }
        [HttpPost]
        public JavaScriptResult SavePurchase(string PDate, int VendAcct, int PAcct, string TransFee, string LineItems, string tax, string Ref, string CashBack, string DocPath, int PayMethod, string TaxLocation,string Sns,string COGShip, string EXPShip, string InvRec,bool ReceiveItems)
        {
            //Entries to Transaction, TransactionLine, Document and ItemLoc
            string result = "";
            DateTime Date = new DateTime(); Decimal Fee = 0; Decimal TAX = 0; Decimal Cash = 0; decimal shipCOG = 0; decimal shipEXP = 0; 

            if (DocPath.Length < 1) DocPath = "*";
            if (Ref.Length < 1) Ref = "*";
            try
            {
                Date = Convert.ToDateTime(PDate);
                Fee = Convert.ToDecimal(TransFee);
                TAX = Convert.ToDecimal(tax);
                Cash = Convert.ToDecimal(CashBack);
                shipCOG = Convert.ToDecimal(COGShip);
                shipEXP = Convert.ToDecimal(EXPShip);

            }
            catch (Exception e)
            {

                result = @" alert('Purchase Save Input invalid, Please check the input fields for errors: " + e.Message + @"');  ";
            }
            result = M.RecordPurchase(Date, Convert.ToInt32(PAcct), Convert.ToInt32(VendAcct), LineItems, Fee, TAX, Ref, Cash, DocPath, Convert.ToInt32(PayMethod), TaxLocation, Sns, shipCOG, shipEXP, InvRec,ReceiveItems) + J.MakeAccountingMain();
               
            
            return JavaScript(result);
        }
        [HttpPost]
        public JavaScriptResult AddTaxLocation()
        {
            string result = "";
            result = result + J.MakeAddTaxLoc();
            return JavaScript(result);
        }
        [HttpPost]
        public JavaScriptResult SaveTaxLoc(string TaxLoc, string TaxCode, decimal TaxRate)
        {
            string result = M.SaveNewTaxLoc(TaxLoc, TaxCode, TaxRate);

            return JavaScript(result);
        }
        [HttpPost]
        public JavaScriptResult AllocateNewItems()
        {

            List<ITEMDATA> NewItems = M.GetUnAllocatedLineItems();
            string result = J.MakeAllocateItems(NewItems);

            return JavaScript(result);
        }
        [HttpPost]
        public JavaScriptResult SaveItemAllocations(string DATA)
        {
            string result = M.AllocateItems(DATA);

            return JavaScript(result);
        }
        [HttpPost]
        public JavaScriptResult SaveNewItem(string Name, int ItemTypeID, int ManufacturerID, string UPC, string Description, bool ItemNew, bool ItemTested, int OwnerID, string Cost, string Price, string Appraisal, bool Serialized)
        {
            string result = "";
            
            if (M.GetItemNameDuplicateCount(Name) < 1)
            {
                ITEMDATA newitem = new ITEMDATA { Name = Name, ItemTypeID = ItemTypeID, ManufacturerID = ManufacturerID,  OwnerID = OwnerID, Description = Description, ItemNew = ItemNew, ItemTested = ItemTested, UPC = UPC,Serialized=Serialized};

                newitem.LocID = 0; newitem.Quantity = 0;
                try { newitem.Cost = Convert.ToDecimal(Cost); }
                catch { newitem.Cost = 0; }
                try { newitem.Price = Convert.ToDecimal(Price); }
                catch { newitem.Price = 0; }
                try { newitem.Appraisal = Convert.ToDecimal(Appraisal); }
                catch { newitem.Appraisal = 0; }
                int ItemID = M.SaveItem(newitem);

                if (ItemID > 0) result = @" alert('New Item Saved');  $(PitemDiv).hide(); $(ddlItemList).append('<option value=" + ItemID + @">" + Name + @"</option>'); selvalItemList=" + ItemID + @"; ";

                else result = @" alert('New Item Save Failed!'); ";
            }
            else result = @" alert('An item by that name already exists'); ";
            return JavaScript(result);
        }
        public ActionResult OpenDoc(int? DocID)
        {
            ViewData["DocID"] = DocID;
            return View("Document");
        }
        [HttpPost]
        public JavaScriptResult LoadDocument(int DocID)
        {
            string result = J.BuildDocument(DocID);

            return JavaScript(result);
        }
        [HttpPost]
        public JavaScriptResult SaveDocPath(int DocID,string DocPath)
        {
            string result = "";

            result = result + M.saveDocPath(DocID, DocPath);

            return JavaScript(result);

        }
        [HttpPost]
        public JavaScriptResult Maintanance()
        {
            string result = J.MakeAccountingMaintanance();

            return JavaScript(result);
        }
        [HttpPost]
        public JavaScriptResult DelDocument(int DocID)
        {
            string result = M.DeleteDocument(DocID);

            return JavaScript(result);
        }
        [HttpPost]
        public JavaScriptResult UpdateAcctSummary(int AcctType, bool Showit)
        {
            string result = "";
            bool success=M.ToggleUserSumView(AcctType, Showit);
            if (!(success)) result = @" alert('Failed to save view preference!'); ";
            result=result+ J.MakeAccountingMain();
            return JavaScript(result);
        }
        [HttpPost]
        public JavaScriptResult SetDateRange(string StartDate, string EndDate)
        {
            string result = "";
            bool success = M.ChangeDateRange(StartDate, EndDate);
                if(!success)
                {
                result = @" alert('Failed set Date, Check Date Fields and try again'); ";
                }
            
            result = result + J.MakeAccountingMain();
            return JavaScript(result);
        }
        [HttpPost]
        public JavaScriptResult AcctSummaryRecSelected(int RecID, string ElementID,string BTN)
        {
            string result ="";
            result=result+ J.MakeAccountDetails(RecID, ElementID,BTN);

            return JavaScript(result);
        }
        [HttpPost]
        public JavaScriptResult LoadInvoice(int DocID)
        {
            string result = J.MakeInvoice(DocID);

            return JavaScript(result);
        }
        public ActionResult ShowInvoice(int? DocID)
        {
            ViewData["DocID"] = DocID;
            return View("Invoice");
        }
        [HttpPost]
        public JavaScriptResult LoadSCDeduction(int DocID)
        {
            string result = "";
            //Check if DocID represents deduction or increase in Service credits and display accordingly
            int DocType = M.getdoctypeIDFromDocID(DocID);
            switch (DocType)
            {
                case 7:
                    result = J.MakeInvoice(DocID);
                    break;
                case 15:
                    result = J.MakeSCDeduction(DocID);
                    break;
                default:
                    break;
            }
           

            return JavaScript(result);
        }
        public ActionResult ShowSCDeduction(int? DocID)
        {
            ViewData["DocID"] = DocID;
            return View("SCDeduction");
        }
        public ActionResult ShowSCStatement(int ACCTID)
        {
            ViewData["ACCTID"] = ACCTID;
            return View("SCStatement");
        }
        [HttpPost]
        public JavaScriptResult LoadSCStatement(int ACCTID)
        {
            string result = J.MakeSCStatement(ACCTID,"","",0);

            return JavaScript(result);
        }
        [HttpPost]
        public JavaScriptResult LoadReceipt(int DocID)
        {
            string result = J.MakeReceipt(DocID);

            return JavaScript(result);
        }
        public ActionResult ShowReceipt(int? DocID)
        {
            ViewData["DocID"] = DocID;
            return View("Receipt");
        }
        [HttpPost]
        public JavaScriptResult EmailInvoice(int DocID, int ContactID)
        {
            string result = "";

            return JavaScript(result);
        }
        [HttpPost]
        public JavaScriptResult Deposit()
        {
            string result = J.MakeDeposit();

            return JavaScript(result);
        }
        [HttpPost]
        public JavaScriptResult SaveDeposit(string DDate, string DItems,int BAcct,string DepRef)
        {
            string result = "";
            DateTime DepDate = new DateTime();
            try
            {
                DepDate = Convert.ToDateTime(DDate);
                result = M.SaveDeposit2(DepDate, BAcct, DepRef, DItems);
            }
            catch
            {
                result = @" alert('Deposit save failed'); $(DivDeposit).empty(); ";
            }
            return JavaScript(result);
        }
        [HttpPost]
        public JavaScriptResult ShowContactList(int AcctID,string ElementID,int ContactID)
        {
            string result = J.MakeContactList(AcctID, ElementID,ContactID);

            return JavaScript(result);
        }
        public ActionResult OpenContact(int ContactID,int AcctID)
        {
            ViewData["ContactID"] = ContactID;
            ViewData["AcctID"]=AcctID;
            return View("Contact");
        }
        [HttpPost]
        public JavaScriptResult ShowContact(int ContactID,int AcctID)
        {
            string result = J.MakeContact("ContactDiv",ContactID,AcctID);

            return JavaScript(result);
        }
        [HttpPost]
        public JavaScriptResult SaveContact(int CID,int CTID,string FName, string LName, string Address, string City, string State, string Country, string PostalCode, string Phone, string Email,int AcctID)//requires ContactDiv on requesting page
        {
            string result = "Contact Saved Successfully";
            Contact C = new Contact { AcctID = AcctID, Address = Address, City = City, ContactID = CID, ContactTypeID = CTID, Country = Country, Email = Email, FirstName = FName, LastName = LName, Phone = Phone, PostalCode = PostalCode, State = State };
            bool subresult=M.SaveContact(C);

            if (!(subresult)) result = "Contact Save Failed!";

            return JavaScript(@" alert('"+result+@"'); $(ContactDiv).empty(); ");
        }
    }
}
