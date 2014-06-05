using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OurThings.Models;
using OurThings.Classes;
namespace OurThings.Controllers
{
    [HandleError]
    public class HomeController : Controller
    {
        MyJava J = new MyJava();
        MyMethods M = new MyMethods();
        public ActionResult Index()
        {
            ViewData["Message"] = "Welcome to Our Things!";

            return View();
        }

        public ActionResult About()
        {
            return View();
        }
        [Authorize(Roles = "PowerUser")]
        public ActionResult Books()
        {
            return View();
        }
        [HttpPost]
        public JavaScriptResult StartBooks()
        {
            string result = J.MakeBooks();

            return JavaScript(result);
        }
        public JavaScriptResult SaveBook(string Title, int BookAuthorID, int PublisherID, string Pages, string ISBN, bool HardBack, int LocationID, int OwnerID, string Cost, string Price, string Appraisal)
        {
            string result = "";
            if (M.GetBookTitleDuplicateCount(Title) < 1)
            {
                BOOKDATA newbook = new BOOKDATA { Title = Title, BookAuthorID = BookAuthorID, PublisherID = PublisherID, HardBack = HardBack, LocID = LocationID, OwnerID = OwnerID };
                try { newbook.Pages = Convert.ToInt32(Pages); }
                catch { newbook.Pages = 0; }
                try { newbook.ISBN = Convert.ToInt32(ISBN); }
                catch { newbook.ISBN = 0; }
                try { newbook.Cost = Convert.ToDecimal(Cost); }
                catch { newbook.Cost = 0; }
                try { newbook.Price = Convert.ToDecimal(Price); }
                catch { newbook.Price = 0; }
                try { newbook.Appraisal = Convert.ToDecimal(Appraisal); }
                catch { newbook.Appraisal = 0; }
                int BookID = M.SaveBook(newbook);
                if (BookID > 0) result = @" alert('New Book Save Succeeded'); $(Here).empty(); $(TDIV).empty(); $.post('/Home/StartBooks');";
                else result = @" alert('New Book Save Failed'); ";
            }
            else result = @" alert('A title by that name already exists'); ";
            return JavaScript(result);
        }
        public JavaScriptResult AddNewBookAuthors(string Title)//string Title,string Publisher,int Pages, int ISBN, bool HardBack, string Location, string Owner, decimal Cost, decimal Price, decimal Apprasial)
        {
            string result = "";
            //int BookID = 0;
            //int PublisherID = 0;
            //int LocationID = 0;
            ////check if new publisher
            //if (Publisher != null && Publisher != "")
            //{
            //    try { PublisherID = Convert.ToInt32(Publisher); }
            //    catch { }//new Publisher
            //    if (PublisherID < 1)
            //    {
            //        //Get or create publisher id
            //    }
            //}
            //try { LocationID = Convert.ToInt32(Location); }
            //catch { }//no location
            
            result = @" alert('Feature Not Available'); ";

            return JavaScript(result);
        }
        [HttpPost]
        public JavaScriptResult PageBookTable(int FirstRec)
        {
            string result = @" $(TDIV).empty(); " + J.MakeBookTable("TDIV", "tblBooks", FirstRec, 5);
            return JavaScript(result);
        }
        [Authorize(Roles = "PowerUser")]
        public ActionResult Items()
        {
            return View();
        }
        #region Code Fellows Challenge*****************************************************************************************************************************************************************************
        public ActionResult CFChallenge()
        {
            return View();
        }
        #endregion

        [HttpPost]
        public JavaScriptResult StartItems()
        {
            string result = J.MakeItems("Unknown", "Unknown", "Unassigned", "None");
            return JavaScript(result);
        }
        public JavaScriptResult PageItemTable(int FirstRec, string Skey, int recPerPage)
        {
            string result = J.MakeItemTable("TDIV", "tblItems", FirstRec, recPerPage, Skey);
            return JavaScript(result);
        }
        public JavaScriptResult SaveItem(string Name,int ItemTypeID,int ManufacturerID,string UPC,string Description,string Quantity,bool ItemNew,bool ItemTested,int LocationID,int OwnerID,string Cost,string Price,string Appraisal)
        {
            string result = "";
            int ItemLocID=0;
            if (M.GetItemNameDuplicateCount(Name) < 1)
            {
                ITEMDATA newitem = new ITEMDATA { Name = Name, ItemTypeID = ItemTypeID, ManufacturerID = ManufacturerID,  LocID = LocationID, OwnerID = OwnerID,Description=Description, ItemNew=ItemNew,ItemTested=ItemTested,UPC=UPC };
                
                try { newitem.Quantity = Convert.ToDecimal(Quantity); }
                catch { newitem.Quantity = 0; }
                try { newitem.Cost = Convert.ToDecimal(Cost); }
                catch { newitem.Cost = 0; }
                try { newitem.Price = Convert.ToDecimal(Price); }
                catch { newitem.Price = 0; }
                try { newitem.Appraisal = Convert.ToDecimal(Appraisal); }
                catch { newitem.Appraisal = 0; }
                int ItemID = M.SaveItem(newitem);
                if(ItemID>0) ItemLocID = M.SaveQuantityLoc(ItemID, LocationID, newitem.Quantity);
                if (ItemID > 0 && ItemLocID > 0) result = J.TableRecordPreppend("tblItemsbody", "newtblrecord", Name, M.GetItemTypeFromID(ItemTypeID), M.GetManufacturerFromID(ManufacturerID), UPC, Description, Quantity.ToString(), ItemNew.ToString(), ItemTested.ToString(), M.GetOwnerNameFromID(OwnerID), newitem.Cost.ToString(), newitem.Price.ToString(), newitem.Appraisal.ToString());
                else result = @" alert('New Item Save Failed'); ";
            }
            else result = @" alert('An item by that name already exists'); ";
            return JavaScript(result);
        }
        public JavaScriptResult DeleteItem(int ItemID)
        {
            string result = "";
            string ItemName = M.GetItemNameFromID(ItemID);


            if (M.ItemDelete(ItemID))
            {
                result = @" $(TDIV).empty();" + J.MakeItemTable("TDIV","tblitems",0,5,"ID");
                
            }
            else result = @" alert('An error occured trying to delete "+ItemName+@"!'); ";


            return JavaScript(result);
        }
        public JavaScriptResult SaveItemType(string NewItemType,int RevAcct,int ExpAcct, int InvAcct, int COGAcct)
        {
            string result = "";

            if ((M.GetItemTypeDuplicateCount(NewItemType) < 1) && NewItemType!=null && NewItemType!="")
            {
               int ITID= M.SaveItType(NewItemType,RevAcct,ExpAcct,InvAcct,COGAcct);
               if (ITID > 0) result = @" $(ddlItemTypeID).append('<option value=" + ITID + @">" + NewItemType + @"</option>'); selvalItemTypeID=" + ITID + @"; $(SavLocDiv).hide();";
                else result = @" alert('New Item Type Save Failed'); ";
            }
            else result = @" alert('An Item Type by that name already exists, or attempted to add a blank'); ";
            

            return JavaScript(result);
        }
        [HttpPost]
        public JavaScriptResult TableItemEdit(int ItemID)
        {
            string result = " $(btnSaveEdit" + ItemID + @").show(); $(btnCancelEdit" + ItemID + @").show(); $(Name" + ItemID + @").show(); $(btnDEL" + ItemID + @").hide(); $(btnEdit" + ItemID + @").hide(); ";
            return JavaScript(result);
        }
        [HttpPost]
        public JavaScriptResult CancelItemEdit(int ItemID)
        {
            string result = " $(btnSaveEdit" + ItemID + @").hide(); $(btnCancelEdit" + ItemID + @").hide(); $(Name" + ItemID + @").hide(); $(btnDEL" + ItemID + @").show(); $(btnEdit" + ItemID + @").show(); ";
            return JavaScript(result);
        }
        [HttpPost]
        public JavaScriptResult SaveItemEdit(int ItemID, string Name)
        {
            ITEMDATA edits=new ITEMDATA{Name=Name,ItemID=ItemID};
            string result = "";
            if (M.ItemEditSave(edits))
            {
                result = @" alert('" + Name + @" Succeeded');";
            }
            else
            {
                result = @" alert('" + Name + @" Failed');";
            }
            return JavaScript(result);
        }
        [HttpPost]
        public JavaScriptResult ItemQ(int ItemID, string tblcell)//(string tblcell, int ItemID)
        {
            string result = J.MakeItemQuan(tblcell, ItemID);
            return JavaScript(result);
        }
        [HttpPost]
        public JavaScriptResult ChangeItemQ(int ItemLocID,string tblcell,int ItemID)
        {
            string result = J.MakeItemQchange(ItemLocID, tblcell, ItemID);
            return JavaScript(result);
        }
        [HttpPost]
        public JavaScriptResult AddItemQ(string tblcell,int ItemID)
        {
            string result = J.MakeAddQuan(tblcell, ItemID);
            return JavaScript(result);
        }
        [HttpPost]
        public JavaScriptResult CloseItemQ(string tblcell, int ItemID)
        {
            string result = J.MakeItemQCell(ItemID, tblcell);
            return JavaScript(result);
        }
        [HttpPost]
        public JavaScriptResult SaveItemQuan(string tblcell, int ItemID, int LocID, string Q)
        {
            string result = "";
            decimal Qty = 1;
            try { Qty = Convert.ToDecimal(Q); }
            catch { }//just stay at 0
            ItemLoc newIL = new ItemLoc { ItemID = ItemID, LocID = LocID, Quantity = Qty };
            int ItemLocID = M.SaveItemQuantity(newIL);
            if (ItemLocID < 1) { result = @" alert('New Item Quantity Save Failed'); "; }
            result = result + J.MakeItemQCell(ItemID, tblcell);
            return JavaScript(result);
        }
        [HttpPost]
        public JavaScriptResult SortItemTable(string Skey)
        {
            string result = J.MakeItemTable("TDIV", "tblItems", 0, 5,Skey);
            return JavaScript(result);
        }
        public JavaScriptResult SaveNewOwner(string Name)
        {
            string result = "";

            if (M.GetOwnerDuplicateCount(Name) < 1 && Name!=null && Name!="")
            {
                int OwnerID = M.SaveOwner(Name);
                if (OwnerID > 0) result = @" $(ddlOwner).append('<option value=" + OwnerID + @">" + Name + @"</option>'); selvalOwner=" + OwnerID + @"; $(SavLocDiv).hide();";
                else result = @" alert('New Owner Save Failed'); ";
            }
            else result = @" alert('An Owner by that name already exists'); ";


            return JavaScript(result);
        }
        public JavaScriptResult SaveNewManufacturer(string Name)
        {
            string result = "";

            if (M.GetManufacturerDuplicateCount(Name) < 1 && Name!=null && Name!="")
            {
                int ManID = M.SaveManufacturer(Name);
                if (ManID > 0) result = @" $(ddlManufacturerID).append('<option value=" + ManID + @">" + Name + @"</option>'); selvalManufacturerID=" + ManID + @";  $(SavLocDiv).hide();";
                else result = @" alert('New Manufacturer Save Failed'); ";
            }
            else result = @" alert('An Manufacturer by that name already exists'); ";

            
            return JavaScript(result);
        }
        public JavaScriptResult SaveNewLocType(string Name)
        {
            string result = "";

            if (M.GetLocTypeDuplicateCount(Name) < 1 && Name!=null && Name!="")
            {
                int LocTypeID = M.SaveLocType(Name);
                if (LocTypeID > 0) result = @" LocTypeID=" + LocTypeID + @";  $(ddlLocation).html('" + J.GetOptions("LOCTYPE") + @"');";
                else result = @" alert('New Location Type Save Failed'); ";
            }
            else result = @" alert('An Location Type by that name already exists'); ";


            return JavaScript(result);
        }
        public JavaScriptResult SaveNewLocation(string Name, int LocType)
        {
            string result = "";

            if (M.GetLocationDuplicateCount(Name,LocType) < 1 && Name!=null && Name!="" && LocType>0)
            {
                int LocID = M.SaveLocation(Name,LocType);
                if (LocID > 0) result = @" $(ddlLocation).html('" + J.GetOptions("LOCATION") + @"'); $(tbLocation).val('"+Name+@"'); selvalLocation="+LocID+@"; ";
                else result = @" alert('New Location Save Failed'); ";
            }
            else result = @" alert('A Location by that name and type already exists'); ";


            return JavaScript(result);
        }
        public JavaScriptResult CancelNewLocation(string Name)
        {
            string result = "";

             result = @" $(ddlLocation).html('" + J.GetOptions("LOCATION") + @"'); $(tbLocation).val('" + Name + @"');  ";
               


            return JavaScript(result);
        }
        public JavaScriptResult CancelNewQ(int ItemLocID, string tblcell, int ItemID)
        {
            string result = J.MakeItemQuan(tblcell, ItemID);

            return JavaScript(result);
        }
        public JavaScriptResult SaveNewQuan(int ItemLocID, string tblcell, string Q, int ItemID)
        {
            string result = "";
            
            if (M.UpdateItemLocQ(ItemLocID, Convert.ToDecimal(Q)))
            {
                result = J.MakeItemQuan(tblcell, ItemID);
            }
            else result = @" alert('Update Quantity Failed!'); ";
            return JavaScript(result);
        }
        [Authorize(Roles = "PowerUser")]
        public ActionResult Audio()
        {
            return View();
        }
        [HttpPost]
        public JavaScriptResult StartAudio()
        {
            string result = J.MakeGroupedInputBox("Here", "Title", "Title", "Title", "", "Relative") + J.MakeGroupedInputBox("Here", "Artist", "Artist", "Artist", "", "Relative") + J.MakeGroupedInputBox("Here", "Publisher", "Publisher", "Publisher", "", "Relative") + J.MakeGroupedInputBox("Here", "Tracks", "Tracks", "Tracks", "", "Relative") + J.MakeGroupedInputBox("Here", "UPC", "UPC", "UPC", "", "Relative") + J.MakeGroupedInputBox("Here", "Location", "Location", "Location", "", "Relative") + J.MakeGroupedInputBox("Here", "Owner", "Owner", "Owner", "", "Relative") + J.MakeGroupedInputBox("Here", "Cost", "Cost", "Cost", "", "Relative") + J.MakeGroupedInputBox("Here", "Price", "Price", "Price", "", "Relative") + J.MakeGroupedInputBox("Here", "Appraisal", "Appraisal", "Appraisal", "", "Relative") + J.MakeButton("btnID", "Here", "", "ADD");
            return JavaScript(result);
        }
        [Authorize(Roles = "PowerUser")]
        public ActionResult Video()
        {
            return View();
        }
        [HttpPost]
        public JavaScriptResult StartVideo()
        {
            string result = J.MakeGroupedInputBox("Here", "Title", "Title", "Title", "", "Relative") + J.MakeGroupedInputBox("Here", "Director", "Director", "Director", "", "Relative") + J.MakeGroupedInputBox("Here", "Publisher", "Publisher", "Publisher", "", "Relative") + J.MakeGroupedInputBox("Here", "UPC", "UPC", "UPC", "", "Relative") + J.MakeGroupedInputBox("Here", "Location", "Location", "Location", "", "Relative") + J.MakeGroupedInputBox("Here", "Owner", "Owner", "Owner", "", "Relative") + J.MakeGroupedInputBox("Here", "Cost", "Cost", "Cost", "", "Relative") + J.MakeGroupedInputBox("Here", "Price", "Price", "Price", "", "Relative") + J.MakeGroupedInputBox("Here", "Appraisal", "Appraisal", "Appraisal", "", "Relative") + J.MakeButton("btnID", "Here", "", "ADD");
            return JavaScript(result);
        }
        [Authorize(Roles = "PowerUser")]
        public ActionResult Keys()
        {
            return View();
        }
        [HttpPost]
        public JavaScriptResult StartKeys()
        {
            string result = @"function SaveNewKey()
            {
                $.post('/Home/SaveNewKey',{KeyType:$(KeyType).val(),KeyName:$(Name).val()});
            } "+J.MakeGroupedInputBox("Here", "KeyType", "KeyType", "KeyType", "", "Relative") + J.MakeGroupedInputBox("Here", "Name", "Name", "Name", "", "Relative") + J.MakeButton("btnID", "Here", "SaveNewKey", "ADD");
            //$.post('/Home/SaveNewKey',{KeyType:$KeyType.val(),KeyName:$(Name).val()})
            return JavaScript(result);
        }
        public JavaScriptResult SaveNewKey(string KeyType, string KeyName)
        {
            string result = "";
            int KeyID = 0;
            //Check if type exist
            int KeyTypeID = M.GetKeyTypeIDFromTypeName(KeyType);
            if (KeyTypeID < 1)//Does not exist so add it.
            {
                KeyTypeID = M.SaveNewKeyType(KeyType);
            }
            else//(If key type is new key type is new, the key cant already exist)
            {
                //Check if key exists
                KeyID = M.GetKeyIDFromTypeNamePair(KeyTypeID, KeyName);

            }
            if (KeyTypeID > 0)//All is well move to key part
            {
                if (KeyID < 1)//Save new key
                {
                    KeyID = M.SaveNewKey(KeyTypeID, KeyName);

                    if (KeyID > 0)
                    {
                        result = @" alert('New Key Save Succeeded'); ";
                    }
                    else result = @" alert('New Key Save Failed'); ";
                }
                else result = @" alert('Key already exists'); ";
            }
            else result = @" alert('New Key Type Save Failed'); ";

            return JavaScript(result);
        }
        [Authorize(Roles = "PowerUser")]
        public ActionResult Locations()
        {
            return View();
        }
        [HttpPost]
        public JavaScriptResult StartLocations()
        {
            string result = J.MakeLocations();
            return JavaScript(result);
        }
        [HttpPost]
        public JavaScriptResult LocToLocType()
        {
            LocationType Loca = M.GetOneLocType(1);
            string result = @" $(ddlLocation).html('" + J.GetOptions("LOCTYPE") + @"'); $(tbLocation).val('"+Loca.Name+@"'); selvalLocation="+Loca.LocTypeID+@"; LocTypeID="+Loca.LocTypeID+@"; ";

            return JavaScript(result);
        }
        [Authorize(Roles = "PowerUser")]
        public ActionResult Owners()
        {
            return View();
        }
        [HttpPost]
        public JavaScriptResult StartOwners()
        {
            string result = J.MakeGroupedInputBox("Here", "Name", "Name", "Name", "", "Relative") + J.MakeButton("btnID", "Here", "", "ADD");
            return JavaScript(result);
        }
        [Authorize(Roles = "PowerUser")]
        public ActionResult Authors()
        {
            return View();
        }
        [HttpPost]
        public JavaScriptResult StartAuthors()
        {
            string result = J.MakeGroupedInputBox("Here", "Name", "Name", "Name", "", "Relative") + J.MakeButton("btnID", "Here", "", "ADD");
            return JavaScript(result);
        }
        [Authorize(Roles = "PowerUser")]
        public ActionResult Publishers()
        {
            return View();
        }
        [HttpPost]
        public JavaScriptResult StartPublishers()
        {
            string result = J.MakeGroupedInputBox("Here", "Name", "Name", "Name", "", "Relative") + J.MakeButton("btnID", "Here", "", "ADD");
            return JavaScript(result);
        }
        [Authorize(Roles = "PowerUser")]
        public ActionResult Software()
        {
            return View();
        }
        [HttpPost]
        public JavaScriptResult StartSoftware()
        {
            string result = J.MakeSoftware();
            return JavaScript(result);
        }
        public JavaScriptResult SaveSoftware(string Title, int MediaTypeID, int PublisherID, string ISBN, int LocationID, int OwnerID, string Cost, string Price, string Appraisal)
        {
            string result = "";
            if (M.GetSoftTitleDuplicateCount(Title) < 1)
            {
                SOFTDATA newsoft = new SOFTDATA { Title = Title, MediaTypeID = MediaTypeID, PublisherID = PublisherID,  LocID = LocationID, OwnerID = OwnerID };
                
                try { newsoft.ISBN = Convert.ToInt32(ISBN); }
                catch { newsoft.ISBN = 0; }
                try { newsoft.Cost = Convert.ToDecimal(Cost); }
                catch { newsoft.Cost = 0; }
                try { newsoft.Price = Convert.ToDecimal(Price); }
                catch { newsoft.Price = 0; }
                try { newsoft.Appraisal = Convert.ToDecimal(Appraisal); }
                catch { newsoft.Appraisal = 0; }
                int SoftID = M.SaveSoft(newsoft);
                if (SoftID > 0) result = @" alert('New Software Save Succeeded'); $(Here).empty(); $(TDIV).empty(); $.post('/Home/StartSoftware');";
                else result = @" alert('New Software Save Failed'); ";
            }
            else result = @" alert('A title by that name already exists'); ";
            return JavaScript(result);
        }
        [HttpPost]
        public JavaScriptResult LoadSoftwareContents(int SID)
        {
            string SoftwareName=M.GetSoftNameFromID(SID);
            string result = J.MakeSoftwareContents(SID,SoftwareName);

            return JavaScript(result);
        }
        [HttpPost]
        public JavaScriptResult SaveSoftContent(int SoftID, string Content)
        {
            string result = "";
            if (M.GetSoftContentDuplicateCount(SoftID, Content) < 1)
            {
                SoftwareContent newsoftcontent = new SoftwareContent { SoftwareID = SoftID, Title = Content };
                int ContentID = M.SaveSoftContent(newsoftcontent);
                if (ContentID > 0) result = @" $(Here).empty(); $(TDIV).empty(); $.post('/Home/LoadSoftwareContents',{SID:" + SoftID + @"}); ";
                else result = @" alert('New Content Save Failed'); ";
            }
            else result = @" alert('A title by that name already exists'); ";
            return JavaScript(result);
        }
        [HttpPost]
        public JavaScriptResult PageSoftTable(int FirstRec)
        {
            string result = @" $(TDIV).empty(); " + J.MakeSoftwareTable("TDIV", "tblSoftware", FirstRec, 4);
            return JavaScript(result);
        }
        [HttpPost]
        public JavaScriptResult LocDetails(int LocID)
        {
            string result = @" $(TDIV).empty(); " + J.MakeLocDetails(LocID);
            return JavaScript(result);
        }
        [HttpPost]
        public JavaScriptResult ReceiveItems()
        {
            string result = J.MakeReceiveItems();

            return JavaScript(result);
        }

    }
}
