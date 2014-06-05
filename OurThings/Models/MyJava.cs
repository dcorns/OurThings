using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OurThings.Models
{
    public class MyJava
    {
       
          MyMethods M=new MyMethods();
          TheDataDataContext db = new TheDataDataContext();
          public string SuccessfulSave(string ItemSaved)
          {
              return @" alert('"+ItemSaved+@" Saved Successfully!'); ";
          }
          public string SaveFailed(string ErrorData)
          {
              return @" alert('Saved Failed!**********" + ErrorData + "'); ";
          }

          #region HTML Elements
          public string MakeSelectionTable(string div, string id, string tblClass,string CellClass,string RowClass,string btnclass, int startRec, int recPerPage, string DataType, string filter,string FromBTNID)
          {
              //Collect Date Range info
              DateTime SDate = Convert.ToDateTime(M.getAPstartDate());
              DateTime EDate = Convert.ToDateTime(M.getAPendDate());
              //***********************************************************
              string theader = "";
              string thecaption = "";
              int recordcount = 0;
             // if (filter.Length < 1) filter = "*";
              
              List<Classes.TableRecord> Records = new List<Classes.TableRecord>();
              #region SelectionTable DATATYPES*******************************************************************************************
              switch (DataType)
              {
                  case "ACCOUNTS":
                      theader = @"<th title=AccountID>ID</th><th title=AccountName>NAME</th><th title=AccountNumber>ACCOUNT NUMBER</th><th title=Balance>BALANCE</th>";
                      int accountype = 0;
                      if (filter != "") accountype = Convert.ToInt32(filter);
                      List<AccountTable> Accounts = M.getAccounts(accountype);
                      foreach (AccountTable ar in Accounts)
                      {
                          string ANum = "*";
                          string BAL = M.getBalance(ar.AcctID).ToString();
                          if (BAL.Length > 1)//There is a balance
                          {
                              BAL = BAL.Substring(0, BAL.Length - 2);//remove extra 0s
                              if (BAL.Substring(0, 1) == "-") BAL = "(" + BAL.Substring(1) + ")";
                          }
                          else { BAL = "0.00"; }//there is no balance
                          if (ar.AcctNumber.Length > 0) ANum = ar.AcctNumber;
                          Records.Add(new Classes.TableRecord { Field1 = ar.AcctID.ToString(), Field2 = ar.AcctName, Field3 = ANum,Field4=BAL, RecordType=M.getAccountTypeFromID(ar.AcctTypeID)});
                      }
                      recordcount = Accounts.Count;
                      
                      break;
                  case "ACCTLEDGER":
                    theader = @"<th title=DocumentID>ID</th><th title=DateTime>DATETIME</th><th title=DocumentType>TYPE</th><th title=ReferenceNumber>REFERENCE</th><th title=Amount>AMOUNT</th><th title=Balance>BALANCE</th>";
                      int AcctID = Convert.ToInt32(filter);
                      int AcctTypeID = M.GetAccountTypeIDFromAccountID(AcctID);
                      int DebitMultiplier = 1; int CreditMultiplier=-1;//default for asset accounts like checking other accounts may require multiplier setting changes
                      if (AcctTypeID == 1 || AcctTypeID == 2 || AcctTypeID == 12 || AcctTypeID == 7 || AcctTypeID == 9||AcctTypeID==15)
                      {
                          DebitMultiplier = 1; CreditMultiplier = -1;
                      }
                      else { DebitMultiplier = -1; CreditMultiplier = 1; }
                      List<TransactionTable> Transactions= M.SortTransactionsByDate(M.getTransactionsByAcctID(AcctID));
                      decimal Balance = 0;
                      List<Classes.GenLedgeLine> Lines = new List<Classes.GenLedgeLine>();
                      List<int> Docs = new List<int>();
                      string Amt = "";
                      
                      foreach (TransactionTable T in Transactions)
                      {
                          Document doc = M.GetDocumentFromID(T.DocumentID);
                          if (T.DebitID == AcctID)
                          {
                              Amt = "(" + T.Amount.ToString() + ")";
                              Balance = Math.Round(Balance - T.Amount,2);
                          }
                          else
                          {
                              Amt = T.Amount.ToString();
                              Balance = Math.Round(Balance + T.Amount,2);
                          }
                          Records.Add(new Classes.TableRecord { Field1 = T.TransactionID.ToString(), Field2 = doc.DocumentTime.ToShortDateString(), Field3 = M.GetDocTypeFromTypeID(doc.DocumentTypeID), Field4 = doc.DocumentReference, Field5 =Amt, Field6 = Balance.ToString(), RecordType = M.getAccountTypeFromID(AcctTypeID) });

                      }
                     
                      recordcount = Transactions.Count;
                      break;

                  case "GENERALLEDGER":
                      theader = @"<th title=DocumentID>ID</th><th title=DateTime>DATETIME</th><th title=DocumentType>TYPE</th><th title=ReferenceNumber>REFERENCE</th><th title=Amount>AMOUNT</th><th title=Balance>BALANCE</th>";
                      
                      int GAcctID = Convert.ToInt32(filter);
                      thecaption = M.getAccountFromID(GAcctID).AcctName + @" Details";
                      int GAcctTypeID = M.GetAccountTypeIDFromAccountID(GAcctID);
                      int GDebitMultiplier = 1; int GCreditMultiplier=-1;//default for asset accounts like checking other accounts may require multiplier setting changes
                      if (GAcctTypeID == 1 || GAcctTypeID == 2 || GAcctTypeID == 12 || GAcctTypeID == 7 || GAcctTypeID == 9 || GAcctTypeID == 15 || GAcctTypeID==20)
                      {
                          GDebitMultiplier = 1; GCreditMultiplier = -1;
                      }
                      else { GDebitMultiplier = -1; GCreditMultiplier = 1; }
                      List<TransactionTable> GTransactions= M.SortTransactionsByDate(M.getTransactionsByAcctID(GAcctID));
                      decimal GBalance = 0;
                      List<Classes.GenLedgeLine> GLines = new List<Classes.GenLedgeLine>();
                      List<int> GDocs = new List<int>();
                      decimal GDocAmt = 0;
                      foreach (TransactionTable GT in GTransactions)
                      {

                          if (!(GDocs.Contains(GT.DocumentID))) GDocs.Add(GT.DocumentID);

                      }
                      foreach (int d in GDocs)
                      {
                          
                          Document doc = M.GetDocumentFromID(d);

                          //Check for Date range and apply if it exists

                          if (doc.DocumentTime.Date >= SDate.Date && doc.DocumentTime.Date <= EDate.Date)
                          {
                              //proccess as valid 

                              if (GAcctTypeID == 15)//Customer acct type 15
                              {

                                  GDocAmt = doc.Amount;
                                  //If payment, make negative
                                  if (doc.DocumentTypeID == 3) GDocAmt = GDocAmt * -1;
                                  GBalance = GBalance + GDocAmt;
                              }
                              else
                              {
                                  decimal Gcredit = M.getTransactionDocAcctCredits(d, GAcctID) * GCreditMultiplier;
                                  decimal Gdebit = M.getTransactionDocAcctDebits(d, GAcctID) * GDebitMultiplier;
                                  GDocAmt = Gcredit + Gdebit;
                                  GBalance = GBalance + GDocAmt;
                              }
                              GLines.Add(new Classes.GenLedgeLine { DocID = d, DocAmt = GDocAmt, DocRef = doc.DocumentReference, DocTime = doc.DocumentTime, DocTypeID = doc.DocumentTypeID, DocType = M.GetDocTypeFromTypeID(doc.DocumentTypeID) });
                              string GAMT = GDocAmt.ToString(); GAMT = GAMT.Substring(0, GAMT.Length - 2);
                              if (GAMT.Substring(0, 1) == "-") GAMT = "(" + GAMT.Substring(1) + ")";
                              string GBAL = GBalance.ToString(); GBAL = GBAL.Substring(0, GBAL.Length - 2);
                              if (GBAL.Substring(0, 1) == "-") GBAL = "(" + GBAL.Substring(1) + ")";


                              Records.Add(new Classes.TableRecord { Field1 = d.ToString(), Field2 = doc.DocumentTime.ToShortDateString(), Field3 = M.GetDocTypeFromTypeID(doc.DocumentTypeID), Field4 = doc.DocumentReference, Field5 = GAMT, Field6 = GBAL, RecordType = M.getAccountTypeFromID(GAcctTypeID) });
                          }
                          
                      }
                      recordcount = Records.Count;
                     // recordcount = GDocs.Count;
                     
                      break;
                  case "PAYRECEIVE":
                      theader = @"<th title=DocumentID>ID</th><th title=DateTime>DATETIME</th><th title=DocumentType>TYPE</th><th title=ReferenceNumber>REFERENCE</th><th title=Amount>AMOUNT</th><th title=Balance>BALANCE</th>";
                      int PAcctID = Convert.ToInt32(filter);
                      int PAcctTypeID = M.GetAccountTypeIDFromAccountID(PAcctID);
                      int PDebitMultiplier = 1; int PCreditMultiplier=-1;//default for asset accounts like checking other accounts may require multiplier setting changes
                      if (PAcctTypeID == 1 || PAcctTypeID == 2 || PAcctTypeID == 12 || PAcctTypeID == 7 || PAcctTypeID == 9)
                      {
                          PDebitMultiplier = 1; PCreditMultiplier = -1;
                      }
                      else { PDebitMultiplier = -1; PCreditMultiplier = 1; }
                      List<TransactionTable> PTransactions = M.SortTransactionsByDate(M.getTransactionsByAcctID(PAcctID));
                      decimal PBalance = 0;
                      List<Classes.GenLedgeLine> PLines = new List<Classes.GenLedgeLine>();
                      List<int> PDocs = new List<int>();
                      decimal PDocAmt = 0;
                      foreach (TransactionTable PT in PTransactions)
                      {

                          if (!(PDocs.Contains(PT.DocumentID))) PDocs.Add(PT.DocumentID);

                      }
                      foreach (int d in PDocs)
                      {
                          Document doc = M.GetDocumentFromID(d);
                          decimal Pcredit = M.getTransactionDocAcctCredits(d, PAcctID) * PCreditMultiplier;
                          decimal Pdebit = M.getTransactionDocAcctDebits(d, PAcctID) * PDebitMultiplier;
                          PDocAmt = Pcredit + Pdebit;
                          PBalance = PBalance + PDocAmt;
                          PLines.Add(new Classes.GenLedgeLine { DocID = d ,DocAmt=PDocAmt,DocRef=doc.DocumentReference,DocTime=doc.DocumentTime,DocTypeID=doc.DocumentTypeID,DocType=M.GetDocTypeFromTypeID(doc.DocumentTypeID)});
                          string PAMT = PDocAmt.ToString(); PAMT = PAMT.Substring(0, PAMT.Length - 2);
                          if (PAMT.Substring(0,1) == "-") PAMT = "(" + PAMT.Substring(1) + ")";
                          string PBAL = PBalance.ToString(); PBAL = PBAL.Substring(0, PBAL.Length - 2);
                          if (PBAL.Substring(0,1) == "-") PBAL = "(" + PBAL.Substring(1) + ")";


                          Records.Add(new Classes.TableRecord { Field1 = d.ToString(), Field2 = doc.DocumentTime.ToShortDateString(), Field3 = M.GetDocTypeFromTypeID(doc.DocumentTypeID), Field4 = doc.DocumentReference, Field5 = PAMT, Field6 = PBAL, RecordType = M.getAccountTypeFromID(PAcctTypeID) });
                          
                      }
                      recordcount = PDocs.Count;
                      break;

              }
              #endregion
              string result = "";
              if (FromBTNID != "")
              {
                  result = result + @" $(" + FromBTNID + @").hide(); ";
              }
              else
              {
                  //For close button to work, the following is added
                  FromBTNID = "NoBTN";
              }
              result=result+ @" var StartRec=" + startRec + @"; var RecPerPage=" + recPerPage + @";
                        $(" + div + @").append('<table id=" + id + @" class="+tblClass+@"><caption class=SelTblCaption>"+thecaption+@"</caption><thead><tr>"+theader+@"</tr></thead><tbody id=" + id + @"body>";

              
              List<Classes.TableRecord> recordspage = new List<Classes.TableRecord>();
              string actions = "";
              int NextPage = startRec + recPerPage;
              if (NextPage + recPerPage > recordcount) NextPage = recordcount - recPerPage;
              int PrevPage = startRec - recPerPage;
              if (PrevPage < 0) PrevPage = 0;
              int LastPage = recordcount - recPerPage;
              if (recordcount - recPerPage < 0) LastPage = 0;

              if (recordcount > 0)
              {
                  if (recordcount > recPerPage)
                  {
                      if (startRec + recPerPage > recordcount) recPerPage = recPerPage - (startRec + recPerPage - recordcount);
                      for (int r = startRec; r < (startRec + recPerPage); r++)
                      {
                          recordspage.Add(Records[r]);
                      }
                  }
                  else
                  {
                      foreach (Classes.TableRecord item in Records)
                      {
                          recordspage.Add(item);
                      }
                  }

                  foreach (var pr in recordspage)
                  {
                      
                      result = result + @"<tr class="+RowClass+@">";
                      if (pr.Field1 != "" && pr.Field1 != null)
                      {
                          result = result + @"<td id=td"+pr.Field1+@" class="+CellClass+@"></td>";
                          actions = actions + MakeButton2("btn"+pr.Field1, "td"+pr.Field1, btnclass, pr.Field1, @" $.post('/Common/TableRecordSelected',{RecID:"+pr.Field1+@", DataType:'" + DataType + @"', RecType:'"+pr.Field3+@"'}); ");
                      }
                      if (pr.Field2 != "" && pr.Field2 != null) result = result + @"<td class="+CellClass+@">" + M.fixstring(pr.Field2) + @"</td>";
                      if (pr.Field3 != "" && pr.Field3 != null) result = result + @"<td class=" + CellClass + @">" + M.fixstring(pr.Field3) + @"</td>";
                      if (pr.Field4 != "" && pr.Field4 != null) result = result + @"<td class=" + CellClass + @">" + M.fixstring(pr.Field4) + @"</td>";
                      if (pr.Field5 != "" && pr.Field5 != null) result = result + @"<td class=" + CellClass + @">" + M.fixstring(pr.Field5) + @"</td>";
                      if (pr.Field6 != "" && pr.Field6 != null) result = result + @"<td class=" + CellClass + @">" + M.fixstring(pr.Field6) + @"</td>";
                      if (pr.Field7 != "" && pr.Field7 != null) result = result + @"<td class=" + CellClass + @">" + M.fixstring(pr.Field7) + @"</td>";
                      if (pr.Field8 != "" && pr.Field8 != null) result = result + @"<td class=" + CellClass + @">" + M.fixstring(pr.Field8) + @"</td>";
                      if (pr.Field9 != "" && pr.Field9 != null) result = result + @"<td class=" + CellClass + @">" + M.fixstring(pr.Field9) + @"</td>";
                      if (pr.Field10 != "" && pr.Field10 != null) result = result + @"<td class=" + CellClass + @">" + M.fixstring(pr.Field10) + @"</td>";
                      if (pr.Field11 != "" && pr.Field11 != null) result = result + @"<td class=" + CellClass + @">" + M.fixstring(pr.Field11) + @"</td>";
                      if (pr.Field12 != "" && pr.Field12 != null) result = result + @"<td class=" + CellClass + @">" + M.fixstring(pr.Field12) + @"</td>";
                      if (pr.Field13 != "" && pr.Field13 != null) result = result + @"<td class=" + CellClass + @">" + M.fixstring(pr.Field13) + @"</td>";
                      if (pr.Field14 != "" && pr.Field14 != null) result = result + @"<td class=" + CellClass + @">" + M.fixstring(pr.Field14) + @"</td>";
                      if (pr.Field15 != "" && pr.Field15 != null) result = result + @"<td class=" + CellClass + @">" + M.fixstring(pr.Field15) + @"</td>";
                      if (pr.Field16 != "" && pr.Field16 != null) result = result + @"<td class=" + CellClass + @">" + M.fixstring(pr.Field16) + @"</td>";

                      result = result + @"</tr>";
                      
                  }
                  result = result + @"<tr><td id=tdHome></td><td id=tdPrev></td><td id=tdNext><td id=tdEnd></td><td id=tblClose"+FromBTNID+@"></td></tr>";



                 
                  actions = actions + MakeButton2("btnHome", "tdHome", "", "<--", @" $.post('/Common/PageSelectTable',{FirstRec:0, DataType:'" + DataType + @"',Element:'"+div+@"',TBLID:'"+id+@"',RecsPerPage:"+recPerPage+@",filter:'"+filter+@"'}); ");
                  actions = actions + MakeButton2("btnPrev", "tdPrev", "", "<", @" $.post('/Common/PageSelectTable',{FirstRec:" + PrevPage + @",DataType:'" + DataType + @"',Element:'" + div + @"',TBLID:'" + id + @"',RecsPerPage:" + recPerPage + @",filter:'" + filter + @"'}); ");
                  actions = actions + MakeButton2("btnNext", "tdNext", "", ">", @" $.post('/Common/PageSelectTable',{FirstRec:" + NextPage + @",DataType:'" + DataType + @"',Element:'" + div + @"',TBLID:'" + id + @"',RecsPerPage:" + recPerPage + @",filter:'" + filter + @"'}); ");
                  actions = actions + MakeButton2("btnEnd", "tdEnd", "", "-->", @" $.post('/Common/PageSelectTable',{FirstRec:" + LastPage + @",DataType:'" + DataType + @"',Element:'" + div + @"',TBLID:'" + id + @"',RecsPerPage:" + recPerPage + @",filter:'" + filter + @"'}); ");
                  actions = actions + MakeButton2("btnClose" + FromBTNID, "tblClose" + FromBTNID, "", "CLOSE", @" SCloseBTN("+id+@","+FromBTNID+@"); ");
              }
              else
              {
                  result = result + @"<tr><td id=tblClose"+ FromBTNID+@"></td></tr>";
                  actions = actions + MakeButton2("btnClose" + FromBTNID, "tblClose" + FromBTNID, "", "CLOSE", @" SCloseBTN(" + id + @"," + FromBTNID + @"); ");
              }
              if (FromBTNID != "")
              {
                  actions = actions + @" function SCloseBTN(tbid,btn)
                                        {
                                            $(tbid).remove();
                                            $(btn).show();
                                        }
";
              }
              else//this should never get used change to use only what is in the if part of statement FromBTNID is never "" at this point.
              {
//                  actions = actions + @" function SCloseBTN(tbid,btn)
//                                        {
//                                            $(tbid).remove();
//                                            if(!(btn=='NoBTN'))
//                                                {
//                                                    $(btn).show();
//                                                }
//                                        }
//";
              }
              
              result = result + @"</tbody></table>'); ";
              result = result + actions;
              return result;
          }
          public string MakeEmptyTable(string Element, string id, string tblClass, string CellClass, string RowClass, List<string> Headers)
          {
              string theader = @"";
              foreach (string h in Headers)
              {
                  theader = theader + @"<th>" + h + @"</th>";
              }
              string result = "$(" + Element + @").append('<table id=" + id + @" class=" + tblClass + @"><thead><tr>" + theader + @"</tr></thead><tbody id=" + id + @"body></tbody></table>');";

              return result;
          }
          public string MakeDateField(string div, string id, string title,string onchange, string value)//value=YYYY-MM-DD onchange default=id+"change()"
          {
              if (onchange == "") onchange = id + "change()";

              string result = @"$(" + div + @").append('<input type=date id=" + id + @" title=" + title + @" onchange="+onchange+@" value="+value+@"></input>'); ";
              return result;
          }
          public string MakeDateBox(string div, string id, string title, string lbltext, string postype)
          {

              string result = @" $(" + div + @").append('<label id=lbl" + id + @" style=position:" + postype + @";display:block>" + lbltext + @"</label>');";
              result = result + @"$(" + div + @").append('<input type=date id=" + id + @" title=" + title + @" style=position:" + postype + @";display:block></input>'); ";
              return result;
          }
          public string MakeDateTimeField(string div, string id, string title, string onchange, string value)//value=YYYY-MM-DDTHH:MM:SS onchange default=id+"change()"
          {
              if (onchange == "") onchange = id + "change()";
              string result = @"$(" + div + @").append('<input type=datetime id=" + id + @" title=" + title + @" onchange=" + onchange + @" value=" + value + @"></input>'); ";
              return result;
          }
          public string MakeDateTimeBox(string element, string id, string title, string lbltext, string posttype)
          {
              string result = @" $(" + element + @").append('<label id=lbl" + id + @" style=position:" + posttype + @";display:block>" + lbltext + @"</label>');";
              result = result + @"$(" + element + @").append('<input type=datetime id=" + id + @" title=" + title + @" style=position:" + posttype + @";display:block></input>'); ";
              return result;
          }
          public string MakeDateTimeLocalField(string div, string id, string title, string onchange, string value,int step)//value=YYYY-MM-DD onchange default=id+"change()" default value=*
          {
              if (onchange == "") onchange = id + "change()";
              if (value == "") value = "*";
              string result = @"$(" + div + @").append('<input type=datetime-local id=" + id + @" title=" + title + @" onchange=" + onchange + @" value=" + value + @" step="+step+@"></input>'); ";
              return result;
          }
          public string MaketimeField(string element, string id, string title, string onchange, string value)//HH:MM:SS onchange default=id+"change()"
          {
              if (onchange == "") onchange = id + "change()";
              string result = @"$(" + element + @").append('<input type=time id=" + id + @" title=" + title + @" onchange=" + onchange + @" value=" + value + @"></input>'); ";
              return result;
          }
        public string MakeNumberBox(string element, string id, string title, string lbltext, string defval, string postype, double step)
          {

              string result = @"";
              result = result + @"$(" + element + @").append('<input type=number step=" + step + @" id=" + id + @" title=" + title + @"></input>'); $(" + id + @").val('" + defval + @"'); ";
              return result;
          }
          public string makeddlInputPair(string lblclass, string ddlclass, string tbclass, string type, string div, string selected, string name, string filter, string lbl, string title, int autotype, int subsize)//autotype 0 for first characters, 1 for characters in any position
          {

              string tbval = ">";
              string thelabel = @"<label id=lblddl" + name + @" class=" + lblclass + @">" + lbl + @"</label>";
              string Lists = @" $(divLists).append('<select class=SUB" + ddlclass + @" id=sddl" + name + @"></select>'); ";
              string ddl = @"<select class=" + ddlclass + @" id=ddl" + name + @">";
              ddl = ddl + @"<option value=0>NEW ENTRY</option>";
              string tb = @"<input type=text class=" + tbclass + @" id=tb" + name + @" ";

              #region Selection List Types$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$
              switch (type)
              {
                  case "PUBLISHER":
                      List<BookPub> Pub = (from PU in db.BookPubs orderby PU.Name select PU).ToList();
                      foreach (BookPub BP in Pub)
                      {
                          ddl = ddl + @"<option value=" + BP.BookPubID + @">" + BP.Name + @"</option>";
                      }
                      if (Pub.Count > 0) tbval = @"value=" + Pub[0].Name + tbval;
                      break;
                  case "LOCATION":
                      List<Location> Loc = (from LO in db.Locations orderby LO.LocLabel select LO).ToList();
                      foreach (Location L in Loc)
                      {
                          ddl = ddl + @"<option value=" + L.LocID + @">" + L.LocLabel + @"</option>";
                      }
                      if (Loc.Count > 0) tbval = @"value=" + Loc[0].LocLabel + tbval;
                      break;
                  case "OWNER":
                      List<Owner> Own = (from OW in db.Owners orderby OW.Name select OW).ToList();
                      foreach (Owner O in Own)
                      {
                          ddl = ddl + @"<option value=" + O.OwnerID + @">" + O.Name + @"</option>";
                      }
                      if (Own.Count > 0) tbval = @"value=" + Own[0].Name + tbval;
                      break;
                  case "SOFTWAREPUB":
                      List<SofwarePub> SPub = (from SPU in db.SofwarePubs orderby SPU.Name select SPU).ToList();
                      foreach (SofwarePub SP in SPub)
                      {
                          ddl = ddl + @"<option value=" + SP.SoftPubID + @">" + SP.Name + @"</option>";
                      }
                      if (SPub.Count > 0) tbval = @"value=" + SPub[0].Name + tbval;
                      break;
                  case "MEDIA":
                      List<MediaType> Med = (from ME in db.MediaTypes orderby ME.MediaTypeName select ME).ToList();
                      foreach (MediaType media in Med)
                      {
                          ddl = ddl + @"<option value=" + media.MediaTypeID + @">" + media.MediaTypeName + @"</option>";
                      }
                      if (Med.Count > 0) tbval = @"value=" + Med[0].MediaTypeName + tbval;
                      break;
                  case "ITEMTYPE":
                      List<ItemType> It = (from Ity in db.ItemTypes orderby Ity.Name select Ity).ToList();
                      foreach (ItemType IT in It)
                      {
                          ddl = ddl + @"<option value=" + IT.ItemTypeID + @">" + IT.Name + @"</option>";
                      }
                      if (It.Count > 0) tbval = @"value=" + It[0].Name + tbval;
                      break;
                  case "MANUFACTURERID":
                      List<ItemManufacturer> MAN = (from mn in db.ItemManufacturers orderby mn.Name select mn).ToList();
                      foreach (ItemManufacturer man in MAN)
                      {
                          ddl = ddl + @"<option value=" + man.MaunufacturerID + @">" + man.Name + @"</option>";
                      }
                      if (MAN.Count > 0) tbval = @"value=" + MAN[0].Name + tbval;
                      break;
                  case "LOCTYPE":
                      List<LocationType> LT = (from lt in db.LocationTypes orderby lt.Name select lt).ToList();
                      foreach (LocationType loct in LT)
                      {
                          ddl = ddl + @"<option value=" + loct.LocTypeID + @">" + loct.Name + @"</option>";
                      }
                      if (LT.Count > 0) tbval = @"value=" + LT[0].Name + tbval;
                      break;
                  case "EXPENCES":
                      List<AccountTable> AT = (from at in db.AccountTables where at.AcctTypeID == 9 orderby at.AcctName select at).ToList();
                      foreach (AccountTable acct in AT)
                      {
                          ddl = ddl + @"<option value=" + acct.AcctID + @">" + acct.AcctName + @"</option>";
                      }
                      if (AT.Count > 0) tbval = @"value=" + AT[0].AcctName + tbval;
                      break;
                  case "PAYMETH":
                      List<PayMethod> PM = (from p in db.PayMethods orderby p.PayMeth select p).ToList();
                      foreach (PayMethod pmeth in PM)
                      {
                          ddl = ddl + @"<option value=" + pmeth.PayMethID + @">" + pmeth.PayMeth + @"</option>";
                      }
                      if (PM.Count > 0) tbval = @"value=" + PM[0].PayMeth + tbval;
                      break;
                  #region Accounts DDL Options
                  case "ACCOUNTS":
                      if (filter == "")
                      {
                          List<AccountTable> A = (from a in db.AccountTables orderby a.AcctName select a).ToList();
                          foreach (AccountTable act in A)
                          {
                              ddl = ddl + @"<option value=" + act.AcctID + @">" + act.AcctName + @"</option>";
                          }

                          if (A.Count > 0) tbval = @"value=" + A[0].AcctName + tbval;
                      }
                      else
                      {
                          switch (filter)
                          {
                              case "1-2-12"://checking(1) savings(2) and cash(12)
                                  List<AccountTable> A = (from a in db.AccountTables where a.AcctTypeID == 1 || a.AcctTypeID == 2 || a.AcctTypeID == 12 orderby a.AcctName select a).ToList();
                                  foreach (AccountTable act in A)
                                  {
                                      ddl = ddl + @"<option value=" + act.AcctID + @">" + act.AcctName + @"</option>";
                                  }

                                  if (A.Count > 0) tbval = @"value=" + A[0].AcctName + tbval;
                                  break;
                              case "1-2"://checking(1) savings(2)
                                  List<AccountTable> BA = (from a in db.AccountTables where a.AcctTypeID == 1 || a.AcctTypeID == 2 orderby a.AcctName select a).ToList();
                                  foreach (AccountTable act in BA)
                                  {
                                      ddl = ddl + @"<option value=" + act.AcctID + @">" + act.AcctName + @"</option>";
                                  }

                                  if (BA.Count > 0) tbval = @"value=" + BA[0].AcctName + tbval;
                                  break;

                              case "6"://payable like a vender
                                  List<AccountTable> PA = (from a in db.AccountTables where a.AcctTypeID == 6 orderby a.AcctName select a).ToList();
                                  foreach (AccountTable act in PA)
                                  {
                                      ddl = ddl + @"<option value=" + act.AcctID + @">" + act.AcctName + @"</option>";
                                  }
                                  if (PA.Count > 0) tbval = @"value=" + PA[0].AcctName + tbval;
                                  break;
                              case "7-15"://Receivable with customers(7-15)
                                  List<AccountTable> REAS = (from a in db.AccountTables where a.AcctTypeID == 7 || a.AcctTypeID == 15 orderby a.AcctName select a).ToList();
                                  foreach (AccountTable act in REAS)
                                  {
                                      ddl = ddl + @"<option value=" + act.AcctID + @">" + act.AcctName + @"</option>";
                                  }

                                  if (REAS.Count > 0) tbval = @"value=" + REAS[0].AcctName + tbval;
                                  break;
                              case "9"://Expense Account
                                  List<AccountTable> EA = (from a in db.AccountTables where a.AcctTypeID == 9 orderby a.AcctName select a).ToList();
                                  foreach (AccountTable act in EA)
                                  {
                                      ddl = ddl + @"<option value=" + act.AcctID + @">" + act.AcctName + @"</option>";
                                  }
                                  if (EA.Count > 0) tbval = @"value=" + EA[0].AcctName + tbval;
                                  break;
                              case "7-13"://Receivable(7) Equity(13)
                                  List<AccountTable> REA = (from a in db.AccountTables where a.AcctTypeID == 7 || a.AcctTypeID == 13 orderby a.AcctName select a).ToList();
                                  foreach (AccountTable act in REA)
                                  {
                                      ddl = ddl + @"<option value=" + act.AcctID + @">" + act.AcctName + @"</option>";
                                  }

                                  if (REA.Count > 0) tbval = @"value=" + REA[0].AcctName + tbval;
                                  break;

                              case "12"://Cash(12)
                                  List<AccountTable> CA = (from a in db.AccountTables where a.AcctTypeID == 12 orderby a.AcctName select a).ToList();
                                  foreach (AccountTable act in CA)
                                  {
                                      ddl = ddl + @"<option value=" + act.AcctID + @">" + act.AcctName + @"</option>";
                                  }

                                  if (CA.Count > 0) tbval = @"value=" + CA[0].AcctName + tbval;
                                  break;
                              case "1-2-3-12"://checking(1) savings(2) CreditCard(3) and cash(12)
                                  List<AccountTable> CrA = (from a in db.AccountTables where a.AcctTypeID == 1 || a.AcctTypeID == 2 || a.AcctTypeID == 12 || a.AcctTypeID == 3 orderby a.AcctName select a).ToList();
                                  foreach (AccountTable act in CrA)
                                  {
                                      ddl = ddl + @"<option value=" + act.AcctID + @">" + act.AcctName + @"</option>";
                                  }

                                  if (CrA.Count > 0) tbval = @"value=" + CrA[0].AcctName + tbval;
                                  break;
                              case "15"://Customer Account
                                  List<AccountTable> CustA = (from a in db.AccountTables where a.AcctTypeID == 15 orderby a.AcctName select a).ToList();
                                  foreach (AccountTable act in CustA)
                                  {
                                      ddl = ddl + @"<option value=" + act.AcctID + @">" + act.AcctName + @"</option>";
                                  }
                                  if (CustA.Count > 0) tbval = @"value=" + CustA[0].AcctName + tbval;
                                  break;
                              case "16"://Revenue Account (Equity)
                                  List<AccountTable> RevA = (from a in db.AccountTables where a.AcctTypeID == 16 orderby a.AcctName select a).ToList();
                                  foreach (AccountTable act in RevA)
                                  {
                                      ddl = ddl + @"<option value=" + act.AcctID + @">" + act.AcctName + @"</option>";
                                  }
                                  if (RevA.Count > 0) tbval = @"value=" + RevA[0].AcctName + tbval;
                                  break;
                          }
                      }
                      break;
                  #endregion
              }
              #endregion
              tb = tb + tbval;
              ddl = ddl + @"</select>";

              string F0 = @" var selval" + name + @"=0; var downarrowpress=false;";
              if (selected == "") F0 = F0 + @" selval" + name + @"=document.getElementById('ddl" + name + @"').options[1].value;
            document.getElementById('ddl" + name + @"').options[1].selected=true;
            $(tb" + name + @").val(document.getElementById('ddl" + name + @"').options[1].text);
            ";
              else
              {

                  F0 = F0 + @"  $(tb" + name + @").val('" + selected + @"');
                            for (var i=0;i<document.getElementById('ddl" + name + @"').length;i++)
                            {
                                if(document.getElementById('ddl" + name + @"').options[i].text=='" + selected + @"')
                                    {
                                       selval" + name + @"= document.getElementById('ddl" + name + @"').options[i].value;
                                       document.getElementById('ddl" + name + @"').options[i].selected=true;
                                       
                                    }
                            } 

";
              }


              string F1 = @"var l" + name + @"= 0; var idx" + name + @"=0; $(sddl" + name + @").hide(); var sidx" + name + @"=0; var  ddlval" + name + @"=0;
              $(tb" + name + @").keyup(function(event)
                {
                    l" + name + @"=$(tb" + name + @").val().length;
               

                    try //error happens when tabbing but does not affect intended proccess so put in try group
                    {
                    
                        if(l" + name + @">0)
                            {
                                $(ddl" + name + @").hide();
                                $(sddl" + name + @").empty();
                                $(sddl" + name + @").append('<option value=0>NEW ENTRY</option>');
                                for (var i=1;i<document.getElementById('ddl" + name + @"').length;i++)
                                    {
                                        ddlval" + name + @"=document.getElementById('ddl" + name + @"').options[i].value;
                                        if($(tb" + name + @").val()==document.getElementById('ddl" + name + @"').options[i].text)
                                            {   
                                                document.getElementById('ddl" + name + @"').options[i].selected=true;
                                                selval" + name + @"= document.getElementById('ddl" + name + @"').options[i].value;
                                            }
                                        else{
                                                document.getElementById('ddl" + name + @"').selectedIndex=-1;
                                                selval" + name + @"=0;
                                            }
                        
";

              switch (autotype)
              {
                  case 0:
                      F1 = F1 + @"
                                        
                            
                                        if(document.getElementById('ddl" + name + @"').options[i].text.substr(0,l" + name + @").toUpperCase()==$(tb" + name + @").val().toUpperCase())
                                            {
                                    
                                    
                                                $(sddl" + name + @").append('<option value='+ddlval" + name + @"+'>'+document.getElementById('ddl" + name + @"').options[i].text+'</option>');
                                                sidx" + name + @"++;
                                            }

                                        
";


                      break;
                  case 1:

                      F1 = F1 + @"
                                        if(document.getElementById('ddl" + name + @"').options[i].text.toUpperCase().lastIndexOf($(tb" + name + @").val().toUpperCase())>-1)
                                            {
                                    
                                                $(sddl" + name + @").show();
                                                $(sddl" + name + @").append('<option value='+ddlval" + name + @"+'>'+document.getElementById('ddl" + name + @"').options[i].text+'</option>');
                                                sidx" + name + @"++;
                                            
}";

                      break;

                  default:
                      break;
              }


              F1 = F1 + @"
                                        }//This is the end of the for loop
                               var sddlLENGTH=document.getElementById('sddl" + name + @"').length;
                                document.getElementById('sddl" + name + @"').selectedIndex=0;
                                if (sddlLENGTH>0)
                                            {
                                                $(ddl" + name + @").hide();
                                                $(sddl" + name + @").show();
                                                if(event.keyCode==40)
                                                {$(sddl" + name + @").focus(); downarrowpress=true;}
                                                else
                                                {downarrowpress=false; }
                                                if(sddlLENGTH<" + subsize + @")
                                                    {
                                                        $(sddl" + name + @").attr('size',sddlLENGTH);
                                                    }
                                                    else
                                                    {
                                                        $(sddl" + name + @").attr('size','" + subsize + @"');
                                                    }
                                            }
                                        else
                                            {
                                                alert('else 1');
                                                $(ddl" + name + @").show();
                                                $(sddl" + name + @").hide();
                                                
                                            }
                                
                            }
                        else
                            {
                                
                                $(ddl" + name + @").show();
                                $(sddl" + name + @").hide();
                            }
                
                    }
                 catch(err)
                    {
                        alert(err);
                    }   
            




});";

              string F2 = @"  $(ddl" + name + @").change(function(event){$(tb" + name + @").val(document.getElementById('ddl" + name + @"').options[document.getElementById('ddl" + name + @"').selectedIndex].text); selval" + name + @"=document.getElementById('ddl" + name + @"').options[document.getElementById('ddl" + name + @"').selectedIndex].value; $(ddl" + name + @").hide();
//showDetailsButton('" + name + @"'); 
});";
              string F3 = @" $(sddl" + name + @").change(function(event){$(tb" + name + @").val(document.getElementById('sddl" + name + @"').options[document.getElementById('sddl" + name + @"').selectedIndex].text); selval" + name + @"=document.getElementById('sddl" + name + @"').options[document.getElementById('sddl" + name + @"').selectedIndex].value; alert('sddl changed');

//showDetailsButton('" + name + @"');  
}); ";




              string result = @" $(" + div + @").append('" + thelabel + ddl + tb + @"'); " + Lists + F0 + F1 + F2 + F3;

              //final ddl and sddl presentation handling
              // result = result + @" $(ddl" + name + @").hide();  ";
              string docreadyfunction = @"$(document).ready(function(){P=$(tb" + name + @").position();    $(sddl" + name + @").css('position','fixed');  $(sddl" + name + @").css('z-index','20'); $(ddl" + name + @").css('position','fixed'); $(ddl" + name + @").css('z-index','20'); ";

              if (ddlclass == "offset")//temp fix where some divs need adjustment of position relationships to parent
              {
                  docreadyfunction = docreadyfunction + @"  $(sddl" + name + @").css('left',(P.left+$(" + div + @").position().left)); $(sddl" + name + @").css('top',(P.top+($(" + div + @").position().top)+$(tb" + name + @").height()+5)); $(ddl" + name + @").css('left',(P.left+$(" + div + @").position().left)); $(ddl" + name + @").css('top',(P.top+($(" + div + @").position().top)+$(tb" + name + @").height()+5));



});";
              }
              else
              {
                  docreadyfunction = docreadyfunction + @"  $(sddl" + name + @").css('left',P.left); $(sddl" + name + @").css('top',(P.top+$(tb" + name + @").height()+5)); $(ddl" + name + @").css('left',P.left); $(ddl" + name + @").css('top',(P.top+$(tb" + name + @").height()+5));

             

});";
              }
              return result + docreadyfunction;
          }
          public string MakeDropDown(string name, string ddlclass, string type, string selected, string filter, string changefunction, string blankoptiontext)
          {


              string result;
              if (ddlclass.Length > 0)
                  result = @"<select id=" + name + @" class=" + ddlclass + @" onchange=" + changefunction + @" ><option value=0 >" + blankoptiontext + @"</option>";
              else result = @"<select id=" + name + @" onchange=" + changefunction + @" ><option value=0 >" + blankoptiontext + @"</option>";




              switch (type)
              {
                  case "ITEMLOCATIONS":
                      List<Location> I = (from i in db.Locations orderby i.LocLabel select i).ToList();
                      foreach (Location L in I)
                      {
                          result = result + @"<option value=" + L.LocID + @">" + L.LocLabel + @"</option>";
                      }
                      break;
                  case "EXPENCES":
                      List<AccountTable> AT = (from at in db.AccountTables where at.AcctTypeID == 9 orderby at.AcctName select at).ToList();
                      foreach (AccountTable acct in AT)
                      {
                          result = result + @"<option value=" + acct.AcctID + @">" + acct.AcctName + @"</option>";
                      }

                      break;
                  case "PAYMETH":
                      List<PayMethod> PM = (from p in db.PayMethods orderby p.PayMeth select p).ToList();
                      foreach (PayMethod pmeth in PM)
                      {
                          result = result + @"<option value=" + pmeth.PayMethID + @">" + pmeth.PayMeth + @"</option>";
                      }

                      break;
                  case "CONTACTS":
                      List<Contact> C = (from c in db.Contacts where c.AcctID == Convert.ToInt32(filter) orderby c.LastName select c).ToList();
                      foreach (Contact con in C)
                      {
                          result = result + @"<option value=" + con.ContactID.ToString() + @">" + con.FirstName + @" " + con.LastName + @"</option>";
                      }
                      break;
                  case "ACCOUNTS":
                      switch (filter)
                      {
                          case "12-20"://Accounts that can be deposited from

                              List<AccountTable> DA = (from a in db.AccountTables where a.AcctTypeID == 12 || a.AcctTypeID==20 orderby a.AcctName select a).ToList();
                              foreach (AccountTable act in DA)
                              {
                                  result = result + @"<option value=" + act.AcctID + @">" + act.AcctName + @"</option>";
                              }
                              break;
                          case "12-20-BAL"://Accounts that can be deposited from and currently have a positive balance
                              //Get Cash Accounts
                              List<Classes.AccountDetails> ADB = M.getAcctsWithPlusBalance(12);
                              //Get Bank Note Accounts
                              List<Classes.AccountDetails> ADNB = M.getAcctsWithPlusBalance(20);
                              //Add Bank Note Accounts to Cash Accounts
                              foreach (Classes.AccountDetails NAD in ADNB)
                              {
                                  ADB.Add(NAD);
                              }

                              //Make option list
                              foreach (Classes.AccountDetails act in ADB)
                              {
                                  result = result + @"<option value=" + act.AcctID + @">" + act.Name + @", "+act.Balance+@"</option>";
                              }
                              break;
                          case "1-2"://Checking and Savings Account

                              List<AccountTable> BA = (from a in db.AccountTables where a.AcctTypeID == 1 || a.AcctTypeID == 2 orderby a.AcctName select a).ToList();
                              foreach (AccountTable act in BA)
                              {
                                  result = result + @"<option value=" + act.AcctID + @">" + act.AcctName + @"</option>";
                              }
                              break;
                          default:
                              List<AccountTable> A = (from a in db.AccountTables where a.AcctTypeID == Convert.ToInt32(filter) orderby a.AcctName select a).ToList();
                              foreach (AccountTable act in A)
                              {
                                  result = result + @"<option value=" + act.AcctID + @">" + act.AcctName + @"</option>";
                              }
                              break;
                      }
                      break;
              }


              result = result + @"</select> ";




              return result;
          }
          public string makeGroupedddlInputPair(string div, string name, string title, string lbltext, string selected, string postype, string type, string filter, string changefunction)
          {

              string tbval = ">";
              string thelabel = @"<label id=lblddl" + name + @" style=position:" + postype + @";display:block>" + lbltext + @"</label>";

              string ddl = @"<select style=position:" + postype + @":fixed;display:block id=ddl" + name + @">";

              string tb = @"<input type=text style=position:" + postype + @";display:block id=tb" + name + @" ";
              #region DDL Types ###############################################################################
              switch (type)
              {
                  case "PUBLISHER":
                      List<BookPub> Pub = (from PU in db.BookPubs orderby PU.Name select PU).ToList();
                      foreach (BookPub BP in Pub)
                      {
                          ddl = ddl + @"<option value=" + BP.BookPubID + @">" + BP.Name + @"</option>";
                      }
                      if (Pub.Count > 0) tbval = @"value=" + Pub[0].Name + tbval;
                      break;
                  case "LOCATION":
                      List<Location> Loc = (from LO in db.Locations orderby LO.LocLabel select LO).ToList();
                      foreach (Location L in Loc)
                      {
                          ddl = ddl + @"<option value=" + L.LocID + @">" + L.LocLabel + @"</option>";
                      }
                      if (Loc.Count > 0) tbval = @"value=" + Loc[0].LocLabel + tbval;
                      break;
                  case "OWNER":
                      List<Owner> Own = (from OW in db.Owners orderby OW.Name select OW).ToList();
                      foreach (Owner O in Own)
                      {
                          ddl = ddl + @"<option value=" + O.OwnerID + @">" + O.Name + @"</option>";
                      }
                      if (Own.Count > 0) tbval = @"value=" + Own[0].Name + tbval;
                      break;
                  case "SOFTWAREPUB":
                      List<SofwarePub> SPub = (from SPU in db.SofwarePubs orderby SPU.Name select SPU).ToList();
                      foreach (SofwarePub SP in SPub)
                      {
                          ddl = ddl + @"<option value=" + SP.SoftPubID + @">" + SP.Name + @"</option>";
                      }
                      if (SPub.Count > 0) tbval = @"value=" + SPub[0].Name + tbval;
                      break;
                  case "MEDIA":
                      List<MediaType> Med = (from ME in db.MediaTypes orderby ME.MediaTypeName select ME).ToList();
                      foreach (MediaType media in Med)
                      {
                          ddl = ddl + @"<option value=" + media.MediaTypeID + @">" + media.MediaTypeName + @"</option>";
                      }
                      if (Med.Count > 0) tbval = @"value=" + Med[0].MediaTypeName + tbval;
                      break;
                  case "ITEMTYPE":
                      List<ItemType> It = (from Ity in db.ItemTypes orderby Ity.Name select Ity).ToList();
                      foreach (ItemType IT in It)
                      {
                          ddl = ddl + @"<option value=" + IT.ItemTypeID + @">" + IT.Name + @"</option>";
                      }
                      if (It.Count > 0) tbval = @"value=" + It[0].Name + tbval;
                      break;
                  case "MANUFACTURERID":
                      List<ItemManufacturer> MAN = (from mn in db.ItemManufacturers orderby mn.Name select mn).ToList();
                      foreach (ItemManufacturer man in MAN)
                      {
                          ddl = ddl + @"<option value=" + man.MaunufacturerID + @">" + man.Name + @"</option>";
                      }
                      if (MAN.Count > 0) tbval = @"value=" + MAN[0].Name + tbval;
                      break;
                  case "LOCTYPE":
                      List<LocationType> LT = (from lt in db.LocationTypes orderby lt.Name select lt).ToList();
                      foreach (LocationType loct in LT)
                      {
                          ddl = ddl + @"<option value=" + loct.LocTypeID + @">" + loct.Name + @"</option>";
                      }
                      if (LT.Count > 0) tbval = @"value=" + LT[0].Name + tbval;
                      break;
                  case "ACCTTYPE":
                      List<AccountTypeTable> AT = (from at in db.AccountTypeTables orderby at.AcctType select at).ToList();
                      foreach (AccountTypeTable atype in AT)
                      {
                          ddl = ddl + @"<option value=" + atype.AcctTypeID + @">" + atype.AcctType + @"</option>";
                      }
                      if (AT.Count > 0) tbval = @"value=" + AT[0].AcctType + tbval;
                      break;
                  case "TRANSACTIONTYPE":
                      List<TransactionTypeTable> TT = (from t in db.TransactionTypeTables orderby t.TransactionType select t).ToList();
                      foreach (TransactionTypeTable ttype in TT)
                      {
                          ddl = ddl + @"<option value=" + ttype.TransactionTypeID + @">" + ttype.TransactionType + @"</option>";
                      }
                      if (TT.Count > 0) tbval = @"value=" + TT[0].TransactionType + tbval;
                      break;
                  case "PAYMETH":
                      List<PayMethod> PM = (from p in db.PayMethods orderby p.PayMeth select p).ToList();
                      foreach (PayMethod pmeth in PM)
                      {
                          ddl = ddl + @"<option value=" + pmeth.PayMethID + @">" + pmeth.PayMeth + @"</option>";
                      }
                      if (PM.Count > 0) tbval = @"value=" + PM[0].PayMeth + tbval;
                      break;
                  case "JOB":
                      List<Job> J = new List<Job>();
                      if (filter == "P")//Pending Jobs
                      {
                          J = (from j in db.Jobs where j.Completed==false orderby j.JID select j).ToList();
                      }
                      else//All Jobs
                      {
                          J = (from j in db.Jobs orderby j.JID select j).ToList();
                      }
                      foreach (Job jb in J)
                      {
                          ddl = ddl + @"<option value=" + jb.JID  + @">" + jb.JID+@"-"+jb.JobDescription + @"</option>";
                      }
                      if (J.Count > 0) tbval = @"value=" + J[0].JID + tbval;
                      break;
                  case "CONTACTS":
                      int AcctID = 0;
                      try { AcctID = Convert.ToInt32(filter); }
                      catch { }//Do nothing, no contacts will be returned
                      List<Contact> C = (from c in db.Contacts where c.AcctID==AcctID orderby c.LastName select c).ToList();
                      foreach (Contact c in C)
                      {
                          ddl = ddl + @"<option value=" + c.ContactID + @">" + c.LastName + @"," + c.FirstName + @"</option>";
                      }
                      if (C.Count > 0) tbval = @"value=" + C[0].ContactID + tbval;
                      else { return ""; }
                      break;
                  #region Accounts DDL Options
                  case "ACCOUNTS":
                      if (filter == "")
                      {
                          List<AccountTable> A = (from a in db.AccountTables orderby a.AcctName select a).ToList();
                          foreach (AccountTable act in A)
                          {
                              ddl = ddl + @"<option value=" + act.AcctID + @">" + act.AcctName + @"</option>";
                          }

                          if (A.Count > 0) tbval = @"value=" + A[0].AcctName + tbval;
                      }
                      else
                      {
                          switch (filter)
                          {
                              case "1-2-12"://checking(1) savings(2) and cash(12)
                                  List<AccountTable> A = (from a in db.AccountTables where a.AcctTypeID == 1 || a.AcctTypeID == 2 || a.AcctTypeID == 12 orderby a.AcctName select a).ToList();
                                  foreach (AccountTable act in A)
                                  {
                                      ddl = ddl + @"<option value=" + act.AcctID + @">" + act.AcctName + @"</option>";
                                  }

                                  if (A.Count > 0) tbval = @"value=" + A[0].AcctName + tbval;
                                  break;
                              case "1-2"://checking(1) savings(2)
                                  List<AccountTable> BA = (from a in db.AccountTables where a.AcctTypeID == 1 || a.AcctTypeID == 2 orderby a.AcctName select a).ToList();
                                  foreach (AccountTable act in BA)
                                  {
                                      ddl = ddl + @"<option value=" + act.AcctID + @">" + act.AcctName + @"</option>";
                                  }

                                  if (BA.Count > 0) tbval = @"value=" + BA[0].AcctName + tbval;
                                  break;

                              case "6"://payable like a vender
                                  List<AccountTable> PA = (from a in db.AccountTables where a.AcctTypeID == 6 orderby a.AcctName select a).ToList();
                                  foreach (AccountTable act in PA)
                                  {
                                      ddl = ddl + @"<option value=" + act.AcctID + @">" + act.AcctName + @"</option>";
                                  }
                                  if (PA.Count > 0) tbval = @"value=" + PA[0].AcctName + tbval;
                                  break;
                              case "7-15"://Receivable with customers(7-15)
                                  List<AccountTable> REAS = (from a in db.AccountTables where a.AcctTypeID == 7 || a.AcctTypeID == 15 orderby a.AcctName select a).ToList();
                                  foreach (AccountTable act in REAS)
                                  {
                                      ddl = ddl + @"<option value=" + act.AcctID + @">" + act.AcctName + @"</option>";
                                  }

                                  if (REAS.Count > 0) tbval = @"value=" + REAS[0].AcctName + tbval;
                                  break;
                              case "9"://Expense Account
                                  List<AccountTable> EA = (from a in db.AccountTables where a.AcctTypeID == 9 orderby a.AcctName select a).ToList();
                                  foreach (AccountTable act in EA)
                                  {
                                      ddl = ddl + @"<option value=" + act.AcctID + @">" + act.AcctName + @"</option>";
                                  }
                                  if (EA.Count > 0) tbval = @"value=" + EA[0].AcctName + tbval;
                                  break;
                              case "7-13"://Receivable(7) Equity(13)
                                  List<AccountTable> REA = (from a in db.AccountTables where a.AcctTypeID == 7 || a.AcctTypeID == 13 orderby a.AcctName select a).ToList();
                                  foreach (AccountTable act in REA)
                                  {
                                      ddl = ddl + @"<option value=" + act.AcctID + @">" + act.AcctName + @"</option>";
                                  }

                                  if (REA.Count > 0) tbval = @"value=" + REA[0].AcctName + tbval;
                                  break;

                              case "12"://Cash(12)
                                  List<AccountTable> CA = (from a in db.AccountTables where a.AcctTypeID == 12 orderby a.AcctName select a).ToList();
                                  foreach (AccountTable act in CA)
                                  {
                                      ddl = ddl + @"<option value=" + act.AcctID + @">" + act.AcctName + @"</option>";
                                  }

                                  if (CA.Count > 0) tbval = @"value=" + CA[0].AcctName + tbval;
                                  break;
                              case "13"://Revenue Account (Equity)
                                  List<AccountTable> EqA = (from a in db.AccountTables where a.AcctTypeID == 16 orderby a.AcctName select a).ToList();
                                  foreach (AccountTable act in EqA)
                                  {
                                      ddl = ddl + @"<option value=" + act.AcctID + @">" + act.AcctName + @"</option>";
                                  }
                                  if (EqA.Count > 0) tbval = @"value=" + EqA[0].AcctName + tbval;
                                  break;
                              case "14"://Inventory Account
                                  List<AccountTable> InvA = (from a in db.AccountTables where a.AcctTypeID == 14 orderby a.AcctName select a).ToList();
                                  foreach (AccountTable act in InvA)
                                  {
                                      ddl = ddl + @"<option value=" + act.AcctID + @">" + act.AcctName + @"</option>";
                                  }
                                  if (InvA.Count > 0) tbval = @"value=" + InvA[0].AcctName + tbval;
                                  break;
                              case "1-2-3-12"://checking(1) savings(2) CreditCard(3) and cash(12)
                                  List<AccountTable> CrA = (from a in db.AccountTables where a.AcctTypeID == 1 || a.AcctTypeID == 2 || a.AcctTypeID == 12 || a.AcctTypeID == 3 orderby a.AcctName select a).ToList();
                                  foreach (AccountTable act in CrA)
                                  {
                                      ddl = ddl + @"<option value=" + act.AcctID + @">" + act.AcctName + @"</option>";
                                  }

                                  if (CrA.Count > 0) tbval = @"value=" + CrA[0].AcctName + tbval;
                                  break;
                              case "15"://Customer Account
                                  List<AccountTable> CustA = (from a in db.AccountTables where a.AcctTypeID == 15 orderby a.AcctName select a).ToList();
                                  foreach (AccountTable act in CustA)
                                  {
                                      ddl = ddl + @"<option value=" + act.AcctID + @">" + act.AcctName + @"</option>";
                                  }
                                  if (CustA.Count > 0) tbval = @"value=" + CustA[0].AcctName + tbval;
                                  break;
                              case "16"://Equity
                                  List<AccountTable> RevA = (from a in db.AccountTables where a.AcctTypeID == 16 orderby a.AcctName select a).ToList();
                                  foreach (AccountTable act in RevA)
                                  {
                                      ddl = ddl + @"<option value=" + act.AcctID + @">" + act.AcctName + @"</option>";
                                  }
                                  if (RevA.Count > 0) tbval = @"value=" + RevA[0].AcctName + tbval;
                                  break;
                              case "17"://CostOfGoodsSold
                                  List<AccountTable> CogA = (from a in db.AccountTables where a.AcctTypeID == 17 orderby a.AcctName select a).ToList();
                                  foreach (AccountTable act in CogA)
                                  {
                                      ddl = ddl + @"<option value=" + act.AcctID + @">" + act.AcctName + @"</option>";
                                  }
                                  if (CogA.Count > 0) tbval = @"value=" + CogA[0].AcctName + tbval;
                                  break;
                              case "18"://Borrower
                                  List<AccountTable> BorA = (from a in db.AccountTables where a.AcctTypeID == 18 orderby a.AcctName select a).ToList();
                                  foreach (AccountTable act in BorA)
                                  {
                                      ddl = ddl + @"<option value=" + act.AcctID + @">" + act.AcctName + @"</option>";
                                  }
                                  if (BorA.Count > 0) tbval = @"value=" + BorA[0].AcctName + tbval;
                                  break;
                              case "19"://Rebate
                                  List<AccountTable> RebA = (from a in db.AccountTables where a.AcctTypeID == 19 orderby a.AcctName select a).ToList();
                                  foreach (AccountTable act in RebA)
                                  {
                                      ddl = ddl + @"<option value=" + act.AcctID + @">" + act.AcctName + @"</option>";
                                  }
                                  if (RebA.Count > 0) tbval = @"value=" + RebA[0].AcctName + tbval;
                                  break;
                              case "13-15-18-19"://Receive Payment From
                                  List<AccountTable> RpfA = (from a in db.AccountTables where a.AcctTypeID == 13 || a.AcctTypeID == 15 || a.AcctTypeID == 18 || a.AcctTypeID == 19 orderby a.AcctName select a).ToList();
                                  foreach (AccountTable act in RpfA)
                                  {
                                      ddl = ddl + @"<option value=" + act.AcctID + @">" + act.AcctName + @"</option>";
                                  }
                                  if (RpfA.Count > 0) tbval = @"value=" + RpfA[0].AcctName + tbval;
                                  break;
                              case "21"://Employee
                                  List<AccountTable> EmpA = (from a in db.AccountTables where a.AcctTypeID == 21 orderby a.AcctName select a).ToList();
                                  foreach (AccountTable act in EmpA)
                                  {
                                      ddl = ddl + @"<option value=" + act.AcctID + @">" + act.AcctName + @"</option>";
                                  }
                                  if (EmpA.Count > 0) tbval = @"value=" + EmpA[0].AcctName + tbval;
                                  break;
                              case "SC"://Accounts that have had service credits
                                  
                                  //Find Accounts with entries in service credits table
                                  List<AccountTable> SCA = M.getPrepayCustomers();
                                  ddl = ddl + @"<option value=0>CHOOSE ACCT</option>";
                                  foreach (AccountTable act in SCA)
                                  {
                                      ddl = ddl + @"<option value=" + act.AcctID + @">" + act.AcctName + @"</option>";
                                  }
                                  if (SCA.Count > 0) tbval = @"value=" + SCA[0].AcctName + tbval;
                                  break;
                          }
                      }
                      break;
                  #endregion

                  case "ITEMS":
                      List<Item> I = new List<Item>();
                      if (filter == "")
                      {
                          I = (from item in db.Items orderby item.ItemName select item).ToList();
                      }
                      else
                      {
                          int T = Convert.ToInt32(filter);
                          I = (from item in db.Items where item.ItemTypeID==T orderby item.ItemName select item).ToList();
                      }
                      foreach (Item ITEM in I)
                      {
                          ddl = ddl + @"<option value=" + ITEM.ItemID + @">" + M.fixstring(ITEM.ItemName) + @"</option>";
                      }
                      if (I.Count > 0) tbval = @"value=" + M.fixstring(I[0].ItemName) + tbval;
                      break;

                  case "SALEITEMS":

                      //items determine by item type InvDesignator 1=inventory type and 2=labor type and 3=resellable service
                      List<Item> SI = M.getSaleItems();
                      foreach (Item ITEM in SI)
                      {
                          ddl = ddl + @"<option value=" + ITEM.ItemID + @">" + M.fixstring(ITEM.ItemName) + @"</option>";
                      }
                      if (SI.Count > 0) tbval = @"value=" + M.fixstring(SI[0].ItemName) + tbval;
                      break;

                  case "TAXRATES":
                      List<TaxRate> TR = (from tr in db.TaxRates orderby tr.LocationName select tr).ToList();
                      foreach (TaxRate T in TR)
                      {
                          ddl = ddl + @"<option value=" + T.Rate + @">" + M.fixstring(T.LocationName) + @"</option>";
                      }
                      if (TR.Count > 0) tbval = @"value=" + M.fixstring(TR[0].LocationName) + tbval;
                      break;

              }
              #endregion
              tb = tb + tbval;
              ddl = ddl + @"</select>";

              string F0 = @"var selval" + name + @"=0; ";
              if (selected == "") F0 = F0 + @" selval" + name + @"=document.getElementById('ddl" + name + @"').options[0].value; 
                                            $(tb" + name + @").val(document.getElementById('ddl" + name + @"').options[0].text);

";
              else
              {
                  F0 = F0 + @" for (var i=0;i<document.getElementById('ddl" + name + @"').length;i++)
                            {
                                if(document.getElementById('ddl" + name + @"').options[i].text=='" + selected + @"')
                                    {
                                       selval" + name + @"= document.getElementById('ddl" + name + @"').options[i].value;
                                       document.getElementById('ddl" + name + @"').options[i].selected=true;
                                       $(tb" + name + @").val('" + selected + @"');
                                    }
                            } 

";
              }


              string F1 = @"var l" + name + @"= 0; var idx" + name + @"=0; 
              $(tb" + name + @").keyup(function(event)
                {
                
                l" + name + @"=$(tb" + name + @").val().length;
                //hideDetailsButton('" + name + @"');//Function must be in startup, but only used in inventory.
                
                try //error happens when tabbing but does not affect intended proccess so put in try group
                {
                    if(l" + name + @">0)
                    {
                        for (var i=0;i<document.getElementById('ddl" + name + @"').length;i++)
                            {
                               if(document.getElementById('ddl" + name + @"').options[i].text.substr(0,l" + name + @")==$(tb" + name + @").val())
                                {
                                    
                                    document.getElementById('ddl" + name + @"').options[i].style.backgroundColor='#00FF00';
                                    if(document.getElementById('ddl" + name + @"').options[i].text.length==$(tb" + name + @").val().length)
                                    {    
                                        idx" + name + @"=i;
                                        document.getElementById('ddl" + name + @"').selectedIndex=i;
                                        selval" + name + @"=$(':selected').val();
                                        $('option').css('background-Color','white');
                                        document.getElementById('ddl" + name + @"').options[i].style.backgroundColor='#00FF00';
                                       // showDetailsButton('" + name + @"');
                                        break;
                                    }
                                    
                                }
                            else{
                                    document.getElementById('ddl" + name + @"').options[i].style.backgroundColor='white';
                                    selval" + name + @"=0;
                                }
                            }
                    }
                    else{document.getElementById('ddl" + name + @"').options[idx" + name + @"].style.backgroundColor='white';}
                 }
                 catch(err)
                 {
                    
                 }   
            
                    



});";
              string F2 = @"  $(ddl" + name + @").change(function(event){$(tb" + name + @").val(document.getElementById('ddl" + name + @"').options[document.getElementById('ddl" + name + @"').selectedIndex].text); selval" + name + @"=document.getElementById('ddl" + name + @"').options[document.getElementById('ddl" + name + @"').selectedIndex].value; 
//showDetailsButton('" + name + @"');
});";
              if (changefunction != "") F2 = @"  $(ddl" + name + @").change(function(event){$(tb" + name + @").val(document.getElementById('ddl" + name + @"').options[document.getElementById('ddl" + name + @"').selectedIndex].text); selval" + name + @"=document.getElementById('ddl" + name + @"').options[document.getElementById('ddl" + name + @"').selectedIndex].value; 
" + changefunction + @" 
});";//@"  $(ddl" + name + @").change(function(event){"+changefunction+@"
              //});";


              string result = @" $(" + div + @").append('" + thelabel + ddl + tb + @"'); " + F0 + F1 + F2;
              return result;
          }
          public string makeddlInputPair(string div, string name, string title, string lbltext, string selected, string type, string filter, string changefunction,string blankoptiontext)
          {

              string tbval = ">";


              string ddl = @"<select id=ddl" + name + @"><option value=0 >" + blankoptiontext + @"</option>";

              string tb = @"<input id=tb" + name + @" ";
              #region DDL Types ###############################################################################
              switch (type)
              {
                  case "PUBLISHER":
                      List<BookPub> Pub = (from PU in db.BookPubs orderby PU.Name select PU).ToList();
                      foreach (BookPub BP in Pub)
                      {
                          ddl = ddl + @"<option value=" + BP.BookPubID + @">" + BP.Name + @"</option>";
                      }
                      if (Pub.Count > 0) tbval = @"value=" + Pub[0].Name + tbval;
                      break;
                  case "LOCATION":
                      List<Location> Loc = (from LO in db.Locations orderby LO.LocLabel select LO).ToList();
                      foreach (Location L in Loc)
                      {
                          ddl = ddl + @"<option value=" + L.LocID + @">" + L.LocLabel + @"</option>";
                      }
                      if (Loc.Count > 0) tbval = @"value=" + Loc[0].LocLabel + tbval;
                      break;
                  case "OWNER":
                      List<Owner> Own = (from OW in db.Owners orderby OW.Name select OW).ToList();
                      foreach (Owner O in Own)
                      {
                          ddl = ddl + @"<option value=" + O.OwnerID + @">" + O.Name + @"</option>";
                      }
                      if (Own.Count > 0) tbval = @"value=" + Own[0].Name + tbval;
                      break;
                  case "SOFTWAREPUB":
                      List<SofwarePub> SPub = (from SPU in db.SofwarePubs orderby SPU.Name select SPU).ToList();
                      foreach (SofwarePub SP in SPub)
                      {
                          ddl = ddl + @"<option value=" + SP.SoftPubID + @">" + SP.Name + @"</option>";
                      }
                      if (SPub.Count > 0) tbval = @"value=" + SPub[0].Name + tbval;
                      break;
                  case "MEDIA":
                      List<MediaType> Med = (from ME in db.MediaTypes orderby ME.MediaTypeName select ME).ToList();
                      foreach (MediaType media in Med)
                      {
                          ddl = ddl + @"<option value=" + media.MediaTypeID + @">" + media.MediaTypeName + @"</option>";
                      }
                      if (Med.Count > 0) tbval = @"value=" + Med[0].MediaTypeName + tbval;
                      break;
                  case "ITEMTYPE":
                      List<ItemType> It = (from Ity in db.ItemTypes orderby Ity.Name select Ity).ToList();
                      foreach (ItemType IT in It)
                      {
                          ddl = ddl + @"<option value=" + IT.ItemTypeID + @">" + IT.Name + @"</option>";
                      }
                      if (It.Count > 0) tbval = @"value=" + It[0].Name + tbval;
                      break;
                  case "MANUFACTURERID":
                      List<ItemManufacturer> MAN = (from mn in db.ItemManufacturers orderby mn.Name select mn).ToList();
                      foreach (ItemManufacturer man in MAN)
                      {
                          ddl = ddl + @"<option value=" + man.MaunufacturerID + @">" + man.Name + @"</option>";
                      }
                      if (MAN.Count > 0) tbval = @"value=" + MAN[0].Name + tbval;
                      break;
                  case "LOCTYPE":
                      List<LocationType> LT = (from lt in db.LocationTypes orderby lt.Name select lt).ToList();
                      foreach (LocationType loct in LT)
                      {
                          ddl = ddl + @"<option value=" + loct.LocTypeID + @">" + loct.Name + @"</option>";
                      }
                      if (LT.Count > 0) tbval = @"value=" + LT[0].Name + tbval;
                      break;
                  case "ACCTTYPE":
                      List<AccountTypeTable> AT = (from at in db.AccountTypeTables orderby at.AcctType select at).ToList();
                      foreach (AccountTypeTable atype in AT)
                      {
                          ddl = ddl + @"<option value=" + atype.AcctTypeID + @">" + atype.AcctType + @"</option>";
                      }
                      if (AT.Count > 0) tbval = @"value=" + AT[0].AcctType + tbval;
                      break;
                  case "TRANSACTIONTYPE":
                      List<TransactionTypeTable> TT = (from t in db.TransactionTypeTables orderby t.TransactionType select t).ToList();
                      foreach (TransactionTypeTable ttype in TT)
                      {
                          ddl = ddl + @"<option value=" + ttype.TransactionTypeID + @">" + ttype.TransactionType + @"</option>";
                      }
                      if (TT.Count > 0) tbval = @"value=" + TT[0].TransactionType + tbval;
                      break;
                  case "PAYMETH":
                      List<PayMethod> PM = (from p in db.PayMethods orderby p.PayMeth select p).ToList();
                      foreach (PayMethod pmeth in PM)
                      {
                          ddl = ddl + @"<option value=" + pmeth.PayMethID + @">" + pmeth.PayMeth + @"</option>";
                      }
                      if (PM.Count > 0) tbval = @"value=" + PM[0].PayMeth + tbval;
                      break;
                  #region Accounts DDL Options
                  case "ACCOUNTS":
                      if (filter == "")
                      {
                          List<AccountTable> A = (from a in db.AccountTables orderby a.AcctName select a).ToList();
                          foreach (AccountTable act in A)
                          {
                              ddl = ddl + @"<option value=" + act.AcctID + @">" + act.AcctName + @"</option>";
                          }

                          if (A.Count > 0) tbval = @"value=" + A[0].AcctName + tbval;
                      }
                      else
                      {
                          switch (filter)
                          {
                              case "1-2-12"://checking(1) savings(2) and cash(12)
                                  List<AccountTable> A = (from a in db.AccountTables where a.AcctTypeID == 1 || a.AcctTypeID == 2 || a.AcctTypeID == 12 orderby a.AcctName select a).ToList();
                                  foreach (AccountTable act in A)
                                  {
                                      ddl = ddl + @"<option value=" + act.AcctID + @">" + act.AcctName + @"</option>";
                                  }

                                  if (A.Count > 0) tbval = @"value=" + A[0].AcctName + tbval;
                                  break;
                              case "1-2"://checking(1) savings(2)
                                  List<AccountTable> BA = (from a in db.AccountTables where a.AcctTypeID == 1 || a.AcctTypeID == 2 orderby a.AcctName select a).ToList();
                                  foreach (AccountTable act in BA)
                                  {
                                      ddl = ddl + @"<option value=" + act.AcctID + @">" + act.AcctName + @"</option>";
                                  }

                                  if (BA.Count > 0) tbval = @"value=" + BA[0].AcctName + tbval;
                                  break;

                              case "6"://payable like a vender
                                  List<AccountTable> PA = (from a in db.AccountTables where a.AcctTypeID == 6 orderby a.AcctName select a).ToList();
                                  foreach (AccountTable act in PA)
                                  {
                                      ddl = ddl + @"<option value=" + act.AcctID + @">" + act.AcctName + @"</option>";
                                  }
                                  if (PA.Count > 0) tbval = @"value=" + PA[0].AcctName + tbval;
                                  break;
                              case "7-15"://Receivable with customers(7-15)
                                  List<AccountTable> REAS = (from a in db.AccountTables where a.AcctTypeID == 7 || a.AcctTypeID == 15 orderby a.AcctName select a).ToList();
                                  foreach (AccountTable act in REAS)
                                  {
                                      ddl = ddl + @"<option value=" + act.AcctID + @">" + act.AcctName + @"</option>";
                                  }

                                  if (REAS.Count > 0) tbval = @"value=" + REAS[0].AcctName + tbval;
                                  break;
                              case "9"://Expense Account
                                  List<AccountTable> EA = (from a in db.AccountTables where a.AcctTypeID == 9 orderby a.AcctName select a).ToList();
                                  foreach (AccountTable act in EA)
                                  {
                                      ddl = ddl + @"<option value=" + act.AcctID + @">" + act.AcctName + @"</option>";
                                  }
                                  if (EA.Count > 0) tbval = @"value=" + EA[0].AcctName + tbval;
                                  break;
                              case "7-13"://Receivable(7) Equity(13)
                                  List<AccountTable> REA = (from a in db.AccountTables where a.AcctTypeID == 7 || a.AcctTypeID == 13 orderby a.AcctName select a).ToList();
                                  foreach (AccountTable act in REA)
                                  {
                                      ddl = ddl + @"<option value=" + act.AcctID + @">" + act.AcctName + @"</option>";
                                  }

                                  if (REA.Count > 0) tbval = @"value=" + REA[0].AcctName + tbval;
                                  break;

                              case "12"://Cash(12)
                                  List<AccountTable> CA = (from a in db.AccountTables where a.AcctTypeID == 12 orderby a.AcctName select a).ToList();
                                  foreach (AccountTable act in CA)
                                  {
                                      ddl = ddl + @"<option value=" + act.AcctID + @">" + act.AcctName + @"</option>";
                                  }

                                  if (CA.Count > 0) tbval = @"value=" + CA[0].AcctName + tbval;
                                  break;
                              case "1-2-3-12"://checking(1) savings(2) CreditCard(3) and cash(12)
                                  List<AccountTable> CrA = (from a in db.AccountTables where a.AcctTypeID == 1 || a.AcctTypeID == 2 || a.AcctTypeID == 12 || a.AcctTypeID == 3 orderby a.AcctName select a).ToList();
                                  foreach (AccountTable act in CrA)
                                  {
                                      ddl = ddl + @"<option value=" + act.AcctID + @">" + act.AcctName + @"</option>";
                                  }

                                  if (CrA.Count > 0) tbval = @"value=" + CrA[0].AcctName + tbval;
                                  break;
                              case "15"://Customer Account
                                  List<AccountTable> CustA = (from a in db.AccountTables where a.AcctTypeID == 15 orderby a.AcctName select a).ToList();
                                  foreach (AccountTable act in CustA)
                                  {
                                      ddl = ddl + @"<option value=" + act.AcctID + @">" + act.AcctName + @"</option>";
                                  }
                                  if (CustA.Count > 0) tbval = @"value=" + CustA[0].AcctName + tbval;
                                  break;
                              case "16"://Revenue Account (Equity)
                                  List<AccountTable> RevA = (from a in db.AccountTables where a.AcctTypeID == 16 orderby a.AcctName select a).ToList();
                                  foreach (AccountTable act in RevA)
                                  {
                                      ddl = ddl + @"<option value=" + act.AcctID + @">" + act.AcctName + @"</option>";
                                  }
                                  if (RevA.Count > 0) tbval = @"value=" + RevA[0].AcctName + tbval;
                                  break;
                          }
                      }
                      break;
                  #endregion

                  case "ITEMS":
                      List<Item> I = (from item in db.Items orderby item.ItemName select item).ToList();
                      foreach (Item ITEM in I)
                      {
                          ddl = ddl + @"<option value=" + ITEM.ItemID + @">" + M.fixstring(ITEM.ItemName) + @"</option>";
                      }
                      if (I.Count > 0) tbval = @"value=" + M.fixstring(I[0].ItemName) + tbval;
                      break;

                  case "SALEITEMS":

                      //items determine by item type InvDesignator 1=inventory type and 2=labor type and 3=resellable service
                      List<Item> SI = M.getSaleItems();
                      foreach (Item ITEM in SI)
                      {
                          ddl = ddl + @"<option value=" + ITEM.ItemID + @">" + M.fixstring(ITEM.ItemName) + @"</option>";
                      }
                      if (SI.Count > 0) tbval = @"value=" + M.fixstring(SI[0].ItemName) + tbval;
                      break;

                  case "TAXRATES":
                      List<TaxRate> TR = (from tr in db.TaxRates orderby tr.LocationName select tr).ToList();
                      foreach (TaxRate T in TR)
                      {
                          ddl = ddl + @"<option value=" + T.Rate + @">" + M.fixstring(T.LocationName) + @"</option>";
                      }
                      if (TR.Count > 0) tbval = @"value=" + M.fixstring(TR[0].LocationName) + tbval;
                      break;
                  case "CONTACTTYPE":
                      List<ContactTypeTable> CT = (from ct in db.ContactTypeTables orderby ct.ContactType select ct).ToList();
                      foreach (ContactTypeTable ct in CT)
                      {
                          ddl = ddl + @"<option value=" + ct.ContactTypeID + @">" + M.fixstring(ct.ContactType) + @"</option>";
                      }
                      if (CT.Count > 0) tbval = @"value=" + M.fixstring(CT[0].ContactType) + tbval;
                      break;

              }
              #endregion
              tb = tb + tbval;
              ddl = ddl + @"</select>";

              string F0 = @"var selval" + name + @"=0; ";
              if (selected == "") F0 = F0 + @" selval" + name + @"=document.getElementById('ddl" + name + @"').options[0].value; 
                                            $(tb" + name + @").val(document.getElementById('ddl" + name + @"').options[0].text);

";
              else
              {
                  F0 = F0 + @" for (var i=0;i<document.getElementById('ddl" + name + @"').length;i++)
                            {
                                if(document.getElementById('ddl" + name + @"').options[i].text=='" + selected + @"')
                                    {
                                       selval" + name + @"= document.getElementById('ddl" + name + @"').options[i].value;
                                       document.getElementById('ddl" + name + @"').options[i].selected=true;
                                       $(tb" + name + @").val('" + selected + @"');
                                    }
                            } 

";
              }


              string F1 = @"var l" + name + @"= 0; var idx" + name + @"=0; 
              $(tb" + name + @").keyup(function(event)
                {
                
                l" + name + @"=$(tb" + name + @").val().length;
                //hideDetailsButton('" + name + @"');//Function must be in startup, but only used in inventory.
                
                try //error happens when tabbing but does not affect intended proccess so put in try group
                {
                    if(l" + name + @">0)
                    {
                        for (var i=0;i<document.getElementById('ddl" + name + @"').length;i++)
                            {
                               if(document.getElementById('ddl" + name + @"').options[i].text.substr(0,l" + name + @")==$(tb" + name + @").val())
                                {
                                    
                                    document.getElementById('ddl" + name + @"').options[i].style.backgroundColor='#00FF00';
                                    if(document.getElementById('ddl" + name + @"').options[i].text.length==$(tb" + name + @").val().length)
                                    {    
                                        idx" + name + @"=i;
                                        document.getElementById('ddl" + name + @"').selectedIndex=i;
                                        selval" + name + @"=$(':selected').val();
                                        $('option').css('background-Color','white');
                                        document.getElementById('ddl" + name + @"').options[i].style.backgroundColor='#00FF00';
                                       // showDetailsButton('" + name + @"');
                                        break;
                                    }
                                    
                                }
                            else{
                                    document.getElementById('ddl" + name + @"').options[i].style.backgroundColor='white';
                                    selval" + name + @"=0;
                                }
                            }
                    }
                    else{document.getElementById('ddl" + name + @"').options[idx" + name + @"].style.backgroundColor='white';}
                 }
                 catch(err)
                 {
                    
                 }   
            
                    



});";
              string F2 = @"  $(ddl" + name + @").change(function(event){$(tb" + name + @").val(document.getElementById('ddl" + name + @"').options[document.getElementById('ddl" + name + @"').selectedIndex].text); selval" + name + @"=document.getElementById('ddl" + name + @"').options[document.getElementById('ddl" + name + @"').selectedIndex].value; 
//showDetailsButton('" + name + @"');
});";
              if (changefunction != "") F2 = @"  $(ddl" + name + @").change(function(event){$(tb" + name + @").val(document.getElementById('ddl" + name + @"').options[document.getElementById('ddl" + name + @"').selectedIndex].text); selval" + name + @"=document.getElementById('ddl" + name + @"').options[document.getElementById('ddl" + name + @"').selectedIndex].value; 
" + changefunction + @" 
});";//@"  $(ddl" + name + @").change(function(event){"+changefunction+@"
              //});";


              string result = @" $(" + div + @").append('" + ddl + tb + @"'); " + F0 + F1 + F2;
              return result;
          }
          public string MakeDataList(string element, string id, string theclass, string type, string changefunction)
          {

              string result = @" $(" + element + @").append('<input list=" + id + @"><datalist id=" + id + @">";
              switch (type)
              {
                  case "ITEMLOCATIONS":
                      List<Location> I = (from i in db.Locations orderby i.LocLabel select i).ToList();
                      foreach (Location L in I)
                      {
                          result = result + @"<option value=" + L.LocID + @">" + L.LocLabel + @"</option>";
                      }
                      break;
                  case "EXPENCES":
                      List<AccountTable> AT = (from at in db.AccountTables where at.AcctTypeID == 9 orderby at.AcctName select at).ToList();
                      foreach (AccountTable acct in AT)
                      {
                          result = result + @"<option value=" + acct.AcctID + @">" + acct.AcctName + @"</option>";
                      }

                      break;
                  case "PAYMETH":
                      List<PayMethod> PM = (from p in db.PayMethods orderby p.PayMeth select p).ToList();
                      foreach (PayMethod pmeth in PM)
                      {
                          result = result + @"<option value=" + pmeth.PayMeth + @">";
                      }

                      break;
              }
              result = result + @"<datalist>'); ";
              return result;
          }
          public string MakeAutoDDL(string element, string id, string ddlclass, string type, string ChangeFunction)
          {
              string result = "";

              List<Classes.OptionList> options = M.GetOptionList(type);
              if (element.Length > 0) //element provided append to element
              {
                  result = result + @" $(" + element + @").append('";
              }

              result = result + @"<select id=" + id + @" class=" + ddlclass + @" onchange=" + ChangeFunction + @">";
              foreach (Classes.OptionList O in options)
              {
                  result = result + @"<option id=" + id + O.Ovalue + @" value=" + O.Ovalue + @">" + O.Otext + @"</option>";
              }
              result = result + @"</select>";

              //if appended add functions
              if (element.Length > 0)
              {
                  result = result + @"'); ";


              }

              return result;
          }
          public string MakeGroupedInputBox(string div, string id, string title, string lbltext, string defval, string postype)
          {

              string result = @" $(" + div + @").append('<label id=lbl" + id + @" style=position:" + postype + @";display:block>" + lbltext + @"</label>');";
              result = result + @"$(" + div + @").append('<input id=" + id + @" title=" + title + @" style=position:" + postype + @";display:block></input>'); $(" + id + @").val('" + defval + @"'); ";
              return result;
          }
          public string MakeInputBox(string div, string id, string txtclass, string title, string holdtxt, string defval)
          {

              string result = "";
              result = result + @"$(" + div + @").append('<input class=" + txtclass + @" placeholder=" + holdtxt + @" id=" + id + @" title=" + title + @" ></input>'); $(" + id + @").val('" + defval + @"'); ";
              return result;
          }
          public string MakeInputBox(string div, string id, string txtclass, string title, string holdtxt, string defval, bool jreadonly)
          {
              string ro = "";
              if (jreadonly) ro = "readonly";
              string result = "";
              result = result + @"$(" + div + @").append('<input class=" + txtclass + @" placeholder=" + holdtxt + @" id=" + id + @" title=" + title + @" " + ro + @"></input>'); $(" + id + @").val('" + defval + @"'); ";
              return result;
          }
          public string MakeButton(string btnID, string DIV, string clickfunction, string txt)
          {
              return @" var data" + btnID + @"='" + btnID + @"'; $(" + DIV + @").append('<button id=" + btnID + @" onclick=" + clickfunction + @"(data" + btnID + @") style=width:" + txt.Length + @">" + txt + @"</button>');
            ";
          }
          public string MakeLabel(string lblclass, string div, string name, string lbltext)
          {
              return @" $(" + div + @").append('<label id=" + name + @" class=" + lblclass + @">" + lbltext + @"</label>'); ";
          }
          public string MakeOutput(string outclass, string div, string name, string lbltext)
          {
              return @" $(" + div + @").append('<output id=" + name + @" class=" + outclass + @">" + lbltext + @"</output>'); ";
          }
          public string MakeCheckbox(string cblblclass, string cbclass, string div, string id, string lbl, string title, string clkfunc, bool check)
          {
              string action = @"var " + id + @"var=false; ";
              string result = "";
              if (!(div == ""))
              {
                  result = @" $(" + div + @").append('<label id=lbl" + id + @" class=" + cblblclass + @">" + lbl + @"</label>');";
                  result = result + @" $(" + div + @").append('<input id=" + id + @" title=" + title + @" type=checkbox ";
                  if (check)
                  {
                      result = result + @" checked=checked ";
                      action = @" " + id + @"var=true; ";
                  }
    result=result+@" class=" + cbclass + @">'); ";
                  if (check)
                  //if (clkfunc != "")
                  //{ result = result + @"onclick=" + clkfunc + @">');"; }
                  //else { result = result + @">');"; }
                  result = result + @">');";
              }
              else
              {
                  result = @"<input id=" + id + @" title=" + title + @" type=checkbox ";
                  if (check)
                  {
                      result = result + @" checked=checked ";
                      action = @" " + id + @"var=true; ";
                  }
                  result = result + @" class=" + cbclass + @"> ";
              }
              action = action + @" $(" + id + @").click(function(){" + id + @"var=document.getElementById('" + id + @"').checked; " + clkfunc + @"}); ";
              result = result + action;
              return result;
          }
          public string MakeInlineCheckbox(string cbclass, string id, string title, bool check)
          {

              string result = "";


              result = result + @" <input id=" + id + @" title=" + title + @" type=checkbox class=" + cbclass + @" ";
              if (check)
              {
                  result = result + @" checked=true ";

              }

              result = result + @"> ";




              return result;
          }
          public string MakeInnerCheckbox(string cbclass, string id, string title, string clkfunc, bool check)
          {
              string result = @"<input id=" + id + @" title=" + title + @" type=checkbox class=" + cbclass + @" ";
              if (check)
              {
                  result = result + @" checked=checked ";
              }
              result = result + @" onclick=" + clkfunc + @">";
              return result;
          }
          public string MakeCheckBoxList(string element, string cbclass, string id, string clkfuncname, string listsource)
          {
              //Makes a list of name and value based on name and ids of source. If element is provided it is appended, otherwise it is return raw. 
              string result = "";
              if (cbclass == "") cbclass = "*";
              if (element != "") result = result + @" $(" + element + @").append('";
              switch (listsource)
              {
                  case "AcctType":
                      List<AccountTypeTable> AT = M.getAccountTypes();
                      foreach (AccountTypeTable at in AT)
                      {
                          string chk = "";
                          if (M.getOneUserAcctSumView(at.AcctTypeID)) chk = "checked=checked";
                          result = result + @"<input type=checkbox name=" + id + at.AcctTypeID.ToString() + @" value=" + at.AcctTypeID + @" title=" + at.AcctType + @" class="+cbclass+@" onclick=" + clkfuncname + @" " + chk + @">" + at.AcctType + @"</input>";
                      }
                      break;
                  default:
                      break;
              }
              if (element != "") result = result + @"'); ";
              return result;
          }
          public string MakeMoneyInput(string div, string id, string tbclass, string lblclass, string lbltxt)
          {
              string result = "";
              result = @"$(" + div + @").append('<label id=lbl" + id + @" class=" + lblclass + @">" + lbltxt + @"</label><input id=" + id + @" class=" + tbclass + @" value=$0>'); var " + id + @"keypress = 0;
            var " + id + @"Pamt = String('$0');
            $(" + id + @").keyup(function (event) {
                var " + id + @"strout = String('');
                var " + id + @"code = event.keyCode;
                
                  switch (" + id + @"code) {
                    case 8:
                        " + id + @"keypress--;
                        if (" + id + @"keypress < 1) {
                            " + id + @"strout = '$0';
                            " + id + @"keypress = 0;
                        }
                        else {
                            switch (" + id + @"keypress) {
                                case 1:
                                    " + id + @"Pamt = '$0.0' + " + id + @"Pamt.substr(3, 1);
                                    break;
                                case 2:
                                    " + id + @"Pamt = '$0.' + " + id + @"Pamt.substr(1, 1) + " + id + @"Pamt.substr(3, 1);
                                    break;
                                default:
                                    " + id + @"Pamt = " + id + @"Pamt.substr(0, " + id + @"Pamt.length - 4) + '.' + " + id + @"Pamt.substr(" + id + @"Pamt.length - 4, 1) + " + id + @"Pamt.substr(" + id + @"Pamt.length - 2, 1);
                                    break;

                            }
                            " + id + @"strout = " + id + @"Pamt;
                        }

                        break;
                    
                    case 46:
                        " + id + @"strout = '$0';
                        " + id + @"keypress = 0;
                        break;

                    case 16:    //shift key

                        break;
                    case 17:    //CTRL key
                        break;
                    case 18:    //ALT key
                        break;

                    default:

                        if (!((" + id + @"code > 47 & " + id + @"code < 58) | (" + id + @"code > 95 & " + id + @"code < 106))) {
                            
                            " + id + @"strout = " + id + @"Pamt;
                        }
                        else {
                            " + id + @"keypress++;
                            if (" + id + @"code > 95) {
                            " + id + @"code = " + id + @"code - 48;
                            }
                            switch (" + id + @"keypress) {
                                case 1:
                                    if (" + id + @"code != 48) {
                                        " + id + @"strout = '$0.0' + String.fromCharCode(" + id + @"code);
                                    }
                                    else {
                                        " + id + @"keypress = 0;
                                        " + id + @"strout = '$0';
                                    }
                                    break;
                                case 2:
                                    " + id + @"strout = '$0.' + " + id + @"Pamt.substr(4, 1) + String.fromCharCode(" + id + @"code);
                                    break;
                                case 3:
                                    " + id + @"strout = '$' + " + id + @"Pamt.substr(3, 1) + '.' + " + id + @"Pamt.substr(4, 1) + String.fromCharCode(" + id + @"code);

                                    break;
                                default:

                                    " + id + @"strout = " + id + @"Pamt.substr(0, " + id + @"Pamt.length - 3) + " + id + @"Pamt.substr(" + id + @"Pamt.length - 2, 1) + '.' + " + id + @"Pamt.substr(" + id + @"Pamt.length - 1, 1) + String.fromCharCode(" + id + @"code);
                                    break;

                            }

                        " + id + @"Pamt = " + id + @"strout;
                    }

                        break;

                }
            $(" + id + @").val(" + id + @"strout);
        });";


              return result;
          }
          public string MakeMoney(string div, string id, string tbclass, string blur)
          {
              string result = "";
              result = @"$(" + div + @").append('<input id=" + id + @" class=" + tbclass + @" onblur=" + blur + @" value=$0>'); var " + id + @"keypress = 0;
            var " + id + @"Pamt = String('$0');
            $(" + id + @").keyup(function (event) {
                var " + id + @"strout = String('');
                var " + id + @"code = event.keyCode;
                
                  switch (" + id + @"code) {
                    case 8:
                        " + id + @"keypress--;
                        if (" + id + @"keypress < 1) {
                            " + id + @"strout = '$0';
                            " + id + @"keypress = 0;
                        }
                        else {
                            switch (" + id + @"keypress) {
                                case 1:
                                    " + id + @"Pamt = '$0.0' + " + id + @"Pamt.substr(3, 1);
                                    break;
                                case 2:
                                    " + id + @"Pamt = '$0.' + " + id + @"Pamt.substr(1, 1) + " + id + @"Pamt.substr(3, 1);
                                    break;
                                default:
                                    " + id + @"Pamt = " + id + @"Pamt.substr(0, " + id + @"Pamt.length - 4) + '.' + " + id + @"Pamt.substr(" + id + @"Pamt.length - 4, 1) + " + id + @"Pamt.substr(" + id + @"Pamt.length - 2, 1);
                                    break;

                            }
                            " + id + @"strout = " + id + @"Pamt;
                        }

                        break;
                    
                    case 46:
                        " + id + @"strout = '$0';
                        " + id + @"keypress = 0;
                        break;

                    case 16:    //shift key

                        break;
                    case 17:    //CTRL key
                        break;
                    case 18:    //ALT key
                        break;

                    default:

                        if (!((" + id + @"code > 47 & " + id + @"code < 58) | (" + id + @"code > 95 & " + id + @"code < 106))) {
                            
                            " + id + @"strout = " + id + @"Pamt;
                        }
                        else {
                            " + id + @"keypress++;
                            if (" + id + @"code > 95) {
                            " + id + @"code = " + id + @"code - 48;
                            }
                            switch (" + id + @"keypress) {
                                case 1:
                                    if (" + id + @"code != 48) {
                                        " + id + @"strout = '$0.0' + String.fromCharCode(" + id + @"code);
                                    }
                                    else {
                                        " + id + @"keypress = 0;
                                        " + id + @"strout = '$0';
                                    }
                                    break;
                                case 2:
                                    " + id + @"strout = '$0.' + " + id + @"Pamt.substr(4, 1) + String.fromCharCode(" + id + @"code);
                                    break;
                                case 3:
                                    " + id + @"strout = '$' + " + id + @"Pamt.substr(3, 1) + '.' + " + id + @"Pamt.substr(4, 1) + String.fromCharCode(" + id + @"code);

                                    break;
                                default:

                                    " + id + @"strout = " + id + @"Pamt.substr(0, " + id + @"Pamt.length - 3) + " + id + @"Pamt.substr(" + id + @"Pamt.length - 2, 1) + '.' + " + id + @"Pamt.substr(" + id + @"Pamt.length - 1, 1) + String.fromCharCode(" + id + @"code);
                                    break;

                            }

                        " + id + @"Pamt = " + id + @"strout;
                    }

                        break;

                }
            $(" + id + @").val(" + id + @"strout);
        });";


              return result;
          }
          public string MakeTextArea(string element, string taclass, string id,int rows,int cols, string value)
          {
              return @"
                        $("+element+@").append('<textarea rows="+rows+@" cols="+cols+@" id="+id+@" class="+taclass+@" >"+value+@"</textarea>'); 

";
          }
          public string MakeButton2(string btnID, string ElementID, string btnclass, string txt, string clickfunction)
          {
              string result = "";

              result = @" var data" + btnID + @"='" + btnID + @"'; $(" + ElementID + @").append('<button id=" + btnID + @" style=width:" + txt.Length + @" class=" + btnclass + @">" + txt + @"</button>');
                        $(" + btnID + @").click(function() {" + clickfunction + @"});
                    ";


              return result;
          }
          #endregion


          public string UpdateAcctSumTable(string tblname,List<int> accttypeIDs, string recClickFunction)
        {
            string result = "";
            
            foreach (int at in accttypeIDs)
            {
                result=result+@" <tr><td class=tablesection>"+ M.getAccountTypeFromID(at)+@"</td></tr>" ;
                List<AccountTable> viewaccts = M.getAccounts(at);
                foreach (AccountTable A in viewaccts)
                {
                    result = result + @" <tr><td id=btnfield" + A.AcctID + @"><button id=btn" + A.AcctID + @" style=width:" + A.AcctID.ToString().Length + @" onclick="+recClickFunction+@"("+A.AcctID+",btnfield"+A.AcctID+@",this)>" + A.AcctID + @"</button>" + @"</td><td>" + A.AcctName + "</td><td>" + A.AcctNumber + "</td><td>" + M.getBalance(A.AcctID) + "</td></tr> ";
                }
            }
            result = @" $( " + tblname + @"body).empty(); $(" + tblname + @"body).append('" + result + @"'); ";
            return result;
        }
          public string UpdateAcctSumTable(string tblname, List<int> accttypeIDs, string recClickFunction,DateTime SDate,DateTime EDate)//OverLoaded with DateRange
          {
              string result = "";
              foreach (int at in accttypeIDs)
              {
                  result = result + @" <tr><td class=tablesection>" + M.getAccountTypeFromID(at) + @"</td></tr>";
                  List<AccountTable> viewaccts = M.getAccounts(at);
                  foreach (AccountTable A in viewaccts)
                  {
                      result = result + @" <tr><td id=btnfield" + A.AcctID + @"><button id=btn" + A.AcctID + @" style=width:" + A.AcctID.ToString().Length + @" onclick=" + recClickFunction + @"(" + A.AcctID + ",btnfield" + A.AcctID + @",this)>" + A.AcctID + @"</button>" + @"</td><td>" + A.AcctName + "</td><td>" + A.AcctNumber + "</td><td>" + M.getBalance(A.AcctID,SDate,EDate) + "</td></tr> ";
                  }
              }
              result = @" $( " + tblname + @"body).empty(); $(" + tblname + @"body).append('" + result + @"'); ";
              return result;
          }
//        public string makeautoddl(string type, string selected, string filter, string name, string div, string lbl, int h, int w, int top, int left, int fntsz)
//        {
//            MyMethods DRCM = new MyMethods();
//            //string filter = "";
//            string result;
//            //if (name.IndexOf(':') > -1)
//            //{
//            //    int idx = name.IndexOf(':');
//            //    result = @"$(" + div + @").append('<select id=" + name.Substring(0, idx) + @"  style=position:fixed;display:block;height:" + h + @"px;width:" + w + @"px;top:" + top + @"px;left:" + left + @"px >";
//            //    filter = name.Substring(idx + 1);
//            //}
//            //else
//            //{
//            result = @"$(" + div + @").append('<label id=lbl" + name + @" style=position:fixed;display:block;font-size:" + fntsz + @"px;height:" + h + @"px;width:" + w + @"px;top:" + (top - h - 7) + @"px;left:" + left + @"px>" + lbl + @"</label><div id=div" + name + @" style=position:fixed;font-size:" + (fntsz - 4) + @"px;display:block;height:" + h + @"px;width:" + w + @"px;top:" + (top - 7) + @"px;left:" + left + @"px><select id=" + name + @"  >";
//            // }
//            bool match = false;

//            switch (type)
//            {
//                case "LOCATION":
//                    var pri =
//                                    from p in db.Locations
                                    
//                                    select p;
//                    foreach (var pr in pri)
//                    {
//                         result = result + @"<option value=" + pr.LocID + @">" + pr.LocLabel + @"</option>";
//                    }
//                    break;
//                //case "STATUS":
//                //    var sta =
//                //                    from s in db.JobCodes
//                //                    where s.CodeType == 1
//                //                    select s;
//                //    foreach (var st in sta)
//                //    {
//                //        if (selected == st.CodeID.ToString()) result = result + @"<option selected=selected value=" + st.CodeID + @">" + st.Code + @"</option>";
//                //        else result = result + @"<option value=" + st.CodeID + @">" + st.Code + @"</option>";
//                //    }

//                //    break;
//                //case "CSR":
//                //    var csr =
//                //                    from s in db.JobCodes
//                //                    where s.CodeType == 5
//                //                    select s;
//                //    foreach (var cs in csr)
//                //    {
//                //        if (selected == cs.CodeID.ToString()) result = result + @"<option selected=selected value=" + cs.CodeID + @">" + cs.Code + @"</option>";
//                //        else result = result + @"<option value=" + cs.CodeID + @">" + cs.Code + @"</option>";
//                //    }

//                //    break;
//                //case "CONTACTS":
//                //    int CI = Convert.ToInt32(filter);
//                //    var con =
//                //                    from c in db.DRC_Contacts
//                //                    where c.AccountID == CI
//                //                    select c;
//                //    foreach (var co in con)
//                //    {
//                //        if (selected == co.ContactID.ToString())
//                //        {
//                //            result = result + @"<option selected=selected value=" + co.ContactID + @">" + co.FirstName + @" " + co.LastName + @" Mbl:" + co.MobilePhone1 + @"</option>";
//                //            match = true;
//                //        }
//                //        else
//                //        {
//                //            result = result + @"<option value=" + co.ContactID + @">" + co.FirstName + @" " + co.LastName + @" Mbl:" + co.MobilePhone1 + @"</option>";
//                //        }
//                //    }
//                //    if (!(match)) result = result + @"<option selected=selected value=0>UNKNOWN</option>";

//                //    break;
//                //case "SERVICES":
//                //    var serv =
//                //                    from se in db.JobCodes
//                //                    where se.CodeType == 6
//                //                    select se;
//                //    foreach (var sv in serv)
//                //    {
//                //        if (selected == sv.CodeID.ToString()) result = result + @"<option selected=selected value=" + sv.CodeID + @">" + sv.Code + @"</option>";
//                //        else result = result + @"<option value=" + sv.CodeID + @">" + sv.Code + @"</option>";
//                //    }

//                //    break;
//                //case "ACCOUNTS":
//                //    List<Account> al = DRCrepos.AccountList(Convert.ToInt32(filter));
//                //    foreach (var a in al)
//                //    {
//                //        result = result + @"<option value=" + a.AccountID + @">" + a.Name + @"</option>";
//                //    }
//                //    break;
//                //case "TWOACCOUNTS":
//                //    DRCMethods DM = new DRCMethods();
//                //    List<string> selections = DM.MakeStringList(selected, '♫');
//                //    List<Account> twoal = DRCrepos.AccountList(Convert.ToInt32(selections[0]), Convert.ToInt32(selections[1]));
//                //    foreach (var a in twoal)
//                //    {
//                //        result = result + @"<option value=" + a.AccountID + @"♫" + a.AccountType + @">" + a.Name + @"</option>";
//                //    }
//                //    break;
//                //case "PARTTYPE":
//                //    List<DRC_PNCode> PNtype = DRCrepos.getPNCodes();
//                //    foreach (DRC_PNCode PN in PNtype)
//                //    {
//                //        result = result + @"<option value=" + PN.Code + @">" + DRCM.fixstring(PN.ProductType) + @"</option>";
//                //    }

//                //    break;
//                //case "PARTMAN":
//                //    List<DRC_ManCode> Man = DRCrepos.getManCodes();
//                //    foreach (DRC_ManCode M in Man)
//                //    {
//                //        result = result + @"<option value=" + M.Code + @">" + DRCM.fixstring(M.Manufacturer) + @"</option>";
//                //    }
//                //    break;
//                //case "CUSTTERMS":
//                //    List<DRC_Term> CustTerms = DRCrepos.getTerms();
//                //    foreach (DRC_Term CT in CustTerms)
//                //    {
//                //        result = result + @"<option value=" + CT.TermsID + @">" + DRCM.fixstring(CT.TermsDescription) + @"</option>";
//                //    }
//                //    break;
//                //case "STATELIST":
//                //    List<DRC_StateProvenceList> States = DRCrepos.getStates();
//                //    foreach (DRC_StateProvenceList ST in States)
//                //    {
//                //        result = result + @"<option value=" + ST.StateID + @">" + DRCM.fixstring(ST.StateName) + @"</option>";
//                //    }
//                //    break;
//                //case "TAXCODE":
//                //    List<DRC_TaxCode> Tax = DRCrepos.getTaxCodes();
//                //    foreach (DRC_TaxCode T in Tax)
//                //    {
//                //        result = result + @"<option value=" + T.TaxCodeID + @">" + T.Name + @"|" + DRCM.fixstring(T.Description) + @"|" + T.CurrentRate.ToString() + @"</option>";
//                //    }
//                //    break;
//                //case "STKBIN":
//                //    List<DRC_ItemBin> Bin = DRCrepos.getItemBins(Convert.ToInt32(filter));
//                //    foreach (DRC_ItemBin B in Bin)
//                //    {
//                //        result = result + @"<option value=" + B.BinID + @">" + B.Name + @"</option>";
//                //    }
//                //    break;
//                //case "STKBUILDING":
//                //    List<DRC_ItemBuilding> Bldg = DRCrepos.getItemBuildings(Convert.ToInt32(filter));
//                //    foreach (DRC_ItemBuilding B in Bldg)
//                //    {
//                //        result = result + @"<option value=" + B.BuildingID + @">" + B.Name + @"</option>";
//                //    }
//                //    break;
//                //case "STKROOM":
//                //    List<DRC_ItemRoom> Room = DRCrepos.getItemRooms(Convert.ToInt32(filter));
//                //    foreach (DRC_ItemRoom R in Room)
//                //    {
//                //        result = result + @"<option value=" + R.RoomID + @">" + R.Name + @"</option>";
//                //    }
//                //    break;

//            }

//            //complete select and add autolist functionality
//            result = result + @"</select></div>'); (function($) {
//		$.widget('ui." + name + @"', {
//			_create: function() {
//				var self = this;
//				var select = this.element.hide();
//				var input = $('<input>')
//					.insertAfter(select)
//					.autocomplete({
//						source: function(request, response) {
//							var matcher = new RegExp(request.term, 'i');
//							response(select.children('option').map(function() {
//								var text = $(this).text();
//                                    
//								if (this.value && (!request.term || matcher.test(text)))
//									return {
//										id: this.value,
//										label: text.replace(new RegExp('(?![^&;]+;)(?!<[^<>]*)(' + $.ui.autocomplete.escapeRegex(request.term) + ')(?![^<>]*>)(?![^&;]+;)', 'gi'), '<strong>$1</strong>'),
//										value: text
//									}
//							}));
//						},
//						delay: 0,
//						change: function(event, ui) {
//							if (!ui.item) {
//								// remove invalid value, as it didn't match anything
//								$(this).val('');
//                                
//								return false;
//							}
//							select.val(ui.item.id);
//
//							self._trigger('selected', event, {
//								item: select.find('[value=' + ui.item.id + ']')
//							});
//						
//						},
//						minLength: 0,
//                        select: function(event,ui){autoitem" + name + @"(ui.item);
//
//
//                        selitem" + name + @"=ui.item;
//
//                        sel" + name + @"=ui.item.id;   
//
//                        }
//                        
//                        
//					})
//					.addClass('ui-widget ui-widget-content ui-corner-left');
//
//$('<button>&nbsp;</button>')
//				.attr('tabIndex', -1)
//				.attr('title', 'Show All Items')
//				.insertAfter(input)
//				.button({
//					icons: { 
//						primary: 'ui-icon-triangle-1-s'
//					},
//					text: false
//				}).removeClass('ui-corner-all')
//				.addClass('ui-corner-right ui-button-icon')
//				.click(function() {
//					// close if already visible
//					if (input.autocomplete('widget').is(':visible')) {
//						input.autocomplete('close');
//						return;
//					}
//					// pass empty string as value to search for, displaying all results
//					input.autocomplete('search', '');
//					input.focus();
//
//				});
//			}
//		});
//
//	})(jQuery);
//
//		
//	$(function() {
//		$('#" + name + @"')." + name + @"();
//		$('#toggle').click(function() {
//			$('#" + name + @"').toggle();
//		});
//	});";
//            result = result + @" var selitem" + name + @"=''; sel" + name + @"=''; function autoitem" + name + @"(data) {selecteditem=data;} ";

//            return result;
//        }
        
        
        public string MakeBookTable(string div, string id, int startRec, int recPerPage)
        {
            string result = @" var StartRec="+startRec+@"; var RecPerPage="+recPerPage+@";
                        $(" + div + @").append('<table id=" + id + @"><thead><tr><th>TITLE</th><th title=Author>Author</th><th title=Publisher>Publisher</th><th>Pages</th><th>ISBN</th><th title=HardBack>HardBack</th><th title=Location>Location</th><th title=Owner>Owner</th><th title=Book_Cost>Cost</th><th title=Book_Price>PRICE</th><th title=Book_Appraised_value>Appraisal</th></tr></thead><tbody id=" + id + @"body>";

            List<Book> books = M.getBooks();
            string Publisher = "";
            string Location = "";
            string Owner = "";
            List<Book> bookpage = new List<Book>();
            string actions="";
            int NextPage = startRec + recPerPage;
            if (NextPage + recPerPage > books.Count) NextPage = books.Count - recPerPage;
            int PrevPage = startRec - recPerPage;
            if (PrevPage < 0) PrevPage = 0;
            int LastPage = books.Count - recPerPage;
            if (books.Count - recPerPage < 0) LastPage = 0;

            if (books.Count() > 0)
            {
                if (books.Count > recPerPage)
                {
                    if (startRec + recPerPage > books.Count) recPerPage = recPerPage - (startRec + recPerPage - books.Count);
                    for (int r = startRec; r < (startRec+recPerPage); r++)
                    {
                        bookpage.Add(books[r]);
                    }
                }

                foreach (var b in bookpage)
                {
                    if (b.PublisherID > 0) Publisher = M.GetBookPubNameFromID(b.PublisherID);
                    if (b.LocID > 0) Location = M.GetLocationLabelFromID(b.LocID);
                    if (b.OwnerID > 0) Owner = M.GetOwnerNameFromID(b.OwnerID);

                    result = result + @"<tr><td>" +M.fixstring( b.Title)+@"</td><td>" + "ButtonSoon" + @"</td><td>" + Publisher + @"</td><td>" + b.Pages + @"</td><td>" + b.ISBN + @"</td>";
                    result = result + @"<td>" + b.HardBack + @"</td><td>" + Location + @"</td>";
                    result = result + @"<td>" + Owner + @"</td>";

                    result = result + @"<td>" + b.BookCost + @"</td>";

                    result = result + @"<td>" + b.BookPrice + @"</td>";


                    result = result + @"<td>" + b.BookApprasial + @"</td>";

                    result = result + @"</tr>";
                    Publisher = "";
                    Location = "";
                    Owner = "";
                }
                result = result + @"<tr><td id=tdHome></td><td id=tdPrev></td><td id=tdNext><td id=tdEnd></td></tr>";
                
                
                
                actions = MakeButton2("btnHome", "tdHome", "", "<--", @" $.post('/Home/PageBookTable',{FirstRec:0}); ");
                actions = actions + MakeButton2("btnPrev", "tdHome", "", "<", @" $.post('/Home/PageBookTable',{FirstRec:" + PrevPage + @"}); ");
                actions = actions + MakeButton2("btnNext", "tdHome", "", ">", @" $.post('/Home/PageBookTable',{FirstRec:" + NextPage + @"}); ");
                actions = actions + MakeButton2("btnEnd", "tdHome", "", "-->", @" $.post('/Home/PageBookTable',{FirstRec:" + LastPage + @"}); ");
            }

            

            result = result + @"</tbody></table>'); ";
            result = result + actions;
            return result;
        }
        public string MakeBooks()
        {
            string result = @"function AddBookAuthors()
            {
                $.post('/Home/AddNewBookAuthors',{Title:$(Title).val()});
            }
            function AddBookContents()
            {
                $.post('/Home/AddContents',{Title:$(Title).val()});
            }
            function SaveBook()
            {
                $.post('/Home/SaveBook',{Title:$(Title).val(),BookAuthorID:0,PublisherID:selvalPublisher,Pages:$(Pages).val(),ISBN:$(ISBN).val(),HardBack:document.getElementById('cbHardBack').checked,LocationID:selvalLocation,OwnerID:selvalOwner,Cost:$(Cost).val(),Price:$(Price).val(),Appraisal:$(Appraisal).val()});
            } " + MakeGroupedInputBox("Here", "Title", "Title", "Title", "", "Relative") + MakeButton("btnAddContents", "Here", "AddContents", "CONTENTS") + MakeButton("btnAddAuthors", "Here", "AddBookAuthors", "AUTHORS") + makeGroupedddlInputPair("Here", "Publisher", "Publisher", "Publisher", "", "Relative", "PUBLISHER", "","") + MakeGroupedInputBox("Here", "Pages", "Pages", "Pages", "", "Relative") + MakeGroupedInputBox("Here", "ISBN", "ISBN", "ISBN", "", "Relative") + MakeCheckbox("", "", "Here", "cbHardBack", "HardBack", "Hardback", "", false) + makeGroupedddlInputPair("Here", "Location", "Location", "Location", "", "Relative", "LOCATION", "","") + makeGroupedddlInputPair("Here", "Owner", "Owner", "Owner", "", "Relative", "OWNER", "","") + MakeGroupedInputBox("Here", "Cost", "Cost", "Book Cost", "", "Relative") + MakeGroupedInputBox("Here", "Price", "Price", "Book Price", "", "Relative") + MakeGroupedInputBox("Here", "Appraisal", "Appraisal", "Appraised Value", "", "Relative") + MakeButton("btnSaveBook", "Here", "SaveBook", "ADD BOOK") + MakeBookTable("TDIV", "tblBooks",5,5);

            result = result + @" $.post('/Common/StartMain'); ";

            return result;
        }
        public string MakeSoftware()
        {
            string result = @"
            
            function SaveSoftware()
            {
                $.post('/Home/SaveSoftware',{Title:$(Title).val(),MediaTypeID:selvalMedia,PublisherID:selvalPublisher,ISBN:$(ISBN).val(),LocationID:selvalLocation,OwnerID:selvalOwner,Cost:$(Cost).val(),Price:$(Price).val(),Appraisal:$(Appraisal).val()});
            } " + MakeGroupedInputBox("Here", "Title", "Title", "Title", "", "Relative") + makeGroupedddlInputPair("Here", "Publisher", "Publisher", "Publisher", "", "Relative", "SOFTWAREPUB", "","") + makeGroupedddlInputPair("Here", "Media", "Media", "Media", "", "Relative", "MEDIA", "","") + MakeGroupedInputBox("Here", "ISBN", "ISBN", "ISBN", "", "Relative") + makeGroupedddlInputPair("Here", "Location", "Location", "Location", "", "Relative", "LOCATION", "","") + makeGroupedddlInputPair("Here", "Owner", "Owner", "Owner", "", "Relative", "OWNER", "","") + MakeGroupedInputBox("Here", "Cost", "Cost", "Cost", "", "Relative") + MakeGroupedInputBox("Here", "Price", "Price", "Price", "", "Relative") + MakeGroupedInputBox("Here", "Appraisal", "Appraisal", "Appraised Value", "", "Relative") + MakeButton("btnSaveSoftware", "Here", "SaveSoftware", "ADD") +MakeSoftwareTable("TDIV", "tblSoftware",0,4);
            result = result + @" $.post('/Common/StartMain'); ";
            return result;
        }
        public string MakeSoftwareContents(int SoftwareID,string Title)
        {
            string result =  @" $(Here).empty(); $(TDIV).empty();
            function AddSoftwareContent()
            {
               $.post('/Home/SaveSoftContent',{SoftID:"+SoftwareID+@",Content:$(Content).val()});
            }


 $(Here).append('<label>" + Title + @"</label>');

                " + MakeGroupedInputBox("Here", "Content", "Content", "Enter Content", "", "Relative") + MakeButton("btnAddContent", "Here", "AddSoftwareContent", "ADD")+ MakeContentTable("Here", "tblBooks","SOFTWARE",SoftwareID);




            return result;
        }

        public string MakeContentTable(string div, string id, string type, int ParentID)
        {
            List<string> Content = M.GetContents(type, ParentID);
            string result = @" $(" + div + @").append('<table id=" + id + @"><thead><tr><th>TITLE</th></tr></thead><tbody id=" + id + @"body>";

            if (Content.Count() > 0)
            {

                foreach (string c in Content)
                {

                    result = result + @"<tr><td>" + M.fixstring(c) + @"</td></tr>";

                }
            }



            result = result + @"</tbody></table>'); ";

            return result;
        }
        public string MakeSoftwareTable(string div, string id,int startRec,int recPerPage)
        {
            List<Software> Soft = M.GetSoftwareData();
            List<Software> SoftPage = new List<Software>();
            string Publisher = "";
            string Location = "";
            string Owner = "";
            string Media = "";
            string actions = "";
            string result = @"
$(" + div + @").append('<table id=" + id + @"><thead><tr><th>TITLE</th><th>PUBLISHER</th><th>MEDIATYPE</th><th>LOCATION</th><th>OWNER</th><th>CONTENTS</th></tr></thead><tbody id=" + id + @"body>";

            
            int NextPage = startRec + recPerPage;
            if (NextPage + recPerPage > Soft.Count) NextPage = Soft.Count - recPerPage;
            int PrevPage = startRec - recPerPage;
            if (PrevPage < 0) PrevPage = 0;
            int LastPage = Soft.Count - recPerPage;
            if (Soft.Count - recPerPage < 0) LastPage = 0;

            if (Soft.Count() > 0)
            {
                if (Soft.Count > recPerPage)
                {
                    if (startRec + recPerPage > Soft.Count) recPerPage = recPerPage - (startRec + recPerPage - Soft.Count);
                    for (int r = startRec; r < (startRec + recPerPage); r++)
                    {
                        SoftPage.Add(Soft[r]);
                    }
                }
                else
                {
                    foreach (Software sft in Soft)
                    {
                        SoftPage.Add(sft);
                    }
                }
                foreach (var s in SoftPage)
                {
                    if (s.PublisherID > 0) Publisher = M.GetBookPubNameFromID(s.PublisherID);
                    if (s.LocID > 0) Location = M.GetLocationLabelFromID(s.LocID);
                    if (s.OwnerID > 0) Owner = M.GetOwnerNameFromID(s.OwnerID);
                    if (s.MediaTypeID > 0) Media = M.GetMediaTypeFromID(s.MediaTypeID);
                    result = result + @"<tr><td>" + M.fixstring(s.Title) + @"</td><td>" + Publisher + @"</td><td>" + Media + @"</td><td>" + Location + @"</td><td>" + Owner + @"</td><td id=Content" + s.SoftwareID + @"></td></tr>";
                    actions = actions + MakeButton2("btn" + s.SoftwareID, "Content" + s.SoftwareID, "", "CONTENTS", @" $.post('/Home/LoadSoftwareContents',{SID:" + s.SoftwareID + @"}); ");
                }
                result = result + @"<tr><td id=tdHome></td></tr>";



                actions = actions+MakeButton2("btnHome", "tdHome", "", "<--", @" $.post('/Home/PageSoftTable',{FirstRec:0}); ");
                actions = actions + MakeButton2("btnPrev", "tdHome", "", "<", @" $.post('/Home/PageSoftTable',{FirstRec:" + PrevPage + @"}); ");
                actions = actions + MakeButton2("btnNext", "tdHome", "", ">", @" $.post('/Home/PageSoftTable',{FirstRec:" + NextPage + @"}); ");
                actions = actions + MakeButton2("btnEnd", "tdHome", "", "-->", @" $.post('/Home/PageSoftTable',{FirstRec:" + LastPage + @"}); ");
            }



            result = result + @"</tbody></table>'); "+actions;

            return result;
        }
        public string MakeHTMLDate(DateTime D)
        {
            string result = D.Year.ToString();
            string M = D.Month.ToString();
            string Day = D.Day.ToString();
            if (M.Length < 2) M = "0" + M;
            if (Day.Length < 2) Day = "0" + Day;
            result = result + "-" + M + "-" + Day;
            return result;
        }
        public string MakeHTMLDateTime(DateTime D)
        {
            
            string result = D.Year.ToString();
            string M = D.Month.ToString();
            string Day = D.Day.ToString();
            if (M.Length < 2) M = "0" + M;
            if (Day.Length < 2) Day = "0" + Day;
            result = result + "-" + M + "-" + Day;
            result = result + "T";
            string Hour = D.Hour.ToString();
            if (Hour.Length < 2) Hour = "0" + Hour;
            string Minute = D.Minute.ToString();
            if (Minute.Length < 2) Minute = "0" + Minute;
            result = result + Hour + ":" + Minute;
            return result;
        }
        //add a div tag
        public string OpenDiv(string divID, string ElementID, string divclass)
        {
            string result = "";

            result = @" $(" + ElementID + @").append('<DIV id=" + divID + @" class=" + divclass + @">');
                       
                    ";


            return result;
        }
        //remove a div tag
        public string CloseDiv(string ElementID)
        {
            string result = "";

            result = @" $(" + ElementID + @").append('</DIV>');
                       
                    ";


            return result;
        }
        public string GetOptions(string Options)
        {
            string result = "";
            int defaultvalue = 0;
            string defaultoption="";
            switch (Options)
            {
                case "PUBLISHER":
                    List<BookPub> Pub = (from PU in db.BookPubs orderby PU.Name select PU).ToList();
                    foreach (BookPub BP in Pub)
                    {
                        result=result + @"<option value=" + BP.BookPubID + @">" + BP.Name + @"</option>";
                    }
                    defaultvalue = Pub[0].BookPubID;
                    defaultoption = Pub[0].Name;
                    break;
                case "LOCATION":
                    List<Location> Loc = (from LO in db.Locations orderby LO.LocLabel select LO).ToList();
                    foreach (Location L in Loc)
                    {
                        result = result + @"<option value=" + L.LocID + @">" + L.LocLabel + @"</option>";
                    }
                    defaultvalue = Loc[0].LocID;
                    defaultoption = Loc[0].LocLabel;
                    break;
                case "OWNER":
                    List<Owner> Own = (from OW in db.Owners orderby OW.Name select OW).ToList();
                    foreach (Owner O in Own)
                    {
                        result = result + @"<option value=" + O.OwnerID + @">" + O.Name + @"</option>";
                    }
                    defaultvalue = Own[0].OwnerID;
                    defaultoption = Own[0].Name;
                    break;
                case "SOFTWAREPUB":
                    List<SofwarePub> SPub = (from SPU in db.SofwarePubs orderby SPU.Name select SPU).ToList();
                    foreach (SofwarePub SP in SPub)
                    {
                        result = result + @"<option value=" + SP.SoftPubID + @">" + SP.Name + @"</option>";
                    }
                    defaultvalue = SPub[0].SoftPubID;
                    defaultoption = SPub[0].Name;
                    break;
                case "MEDIA":
                    List<MediaType> Med = (from ME in db.MediaTypes orderby ME.MediaTypeName select ME).ToList();
                    foreach (MediaType media in Med)
                    {
                        result = result + @"<option value=" + media.MediaTypeID + @">" + media.MediaTypeName + @"</option>";
                    }
                    defaultvalue = Med[0].MediaTypeID;
                    defaultoption = Med[0].MediaTypeName;
                    break;
                case "ITEMTYPE":
                    List<ItemType> It = (from Ity in db.ItemTypes orderby Ity.Name select Ity).ToList();
                    foreach (ItemType IT in It)
                    {
                        result = result + @"<option value=" + IT.ItemTypeID + @">" + IT.Name + @"</option>";
                    }
                    defaultvalue = It[0].ItemTypeID;
                    defaultoption = It[0].Name;
                    break;
                case "MANUFACTURERID":
                    List<ItemManufacturer> MAN = (from mn in db.ItemManufacturers orderby mn.Name select mn).ToList();
                    foreach (ItemManufacturer man in MAN)
                    {
                        result = result + @"<option value=" + man.MaunufacturerID + @">" + man.Name + @"</option>";
                    }
                    defaultvalue = MAN[0].MaunufacturerID;
                    defaultoption = MAN[0].Name;
                    break;
                case "LOCTYPE":
                    List<LocationType> LT = (from lt in db.LocationTypes orderby lt.Name select lt).ToList();
                    foreach (LocationType loct in LT)
                    {
                        result = result + @"<option value=" + loct.LocTypeID + @">" + loct.Name + @"</option>";
                    }
                    defaultvalue = LT[0].LocTypeID;
                    defaultoption = LT[0].Name;
                    break;
               
            }

            result = result + @" dvalue=" + defaultvalue + @"; doption=" + defaultoption + @"; ";

            return result;
        }

        #region Item Stuff
        public string MakeNewItemElements(string div)
        {
            return MakeGroupedInputBox(div, "Name", "Name", "Name", "", "Relative") + makeGroupedddlInputPair(div, "ItemTypeID", "ItemTypeID", "ItemTypeID", "", "Relative", "ITEMTYPE", "", "") + MakeButton("btnAddItemType", div, "AddItemType", "+")
                
                + makeGroupedddlInputPair(div, "ManufacturerID", "ManufacturerID", "ManufacturerID", "", "Relative", "MANUFACTURERID", "", "") + MakeButton("btnAddManufacturer",div, "AddManufacturer", "+")
                + MakeGroupedInputBox(div, "UPC", "UPC", "UPC", "", "Relative")
                + MakeGroupedInputBox(div, "Description", "Description", "Description", "", "Relative")
                + MakeCheckbox("", "", div, "cbNew", "NEW", "NEW", "", false)
                + MakeCheckbox("", "", div, "cbTested", "TESTED", "TESTED", "", false)
                + MakeCheckbox("", "", div, "cbSerialized", "Serialized", "Serialized", "", false)
                + makeGroupedddlInputPair(div, "Owner", "Owner", "Owner", "0", "Relative", "OWNER", "", "") + MakeButton("btnAddOwner", div, "AddOwner", "+")
                + MakeGroupedInputBox(div, "Cost", "Cost", "Cost", "", "Relative")
                + MakeGroupedInputBox(div, "Price", "Price", "Price", "", "Relative")
                + MakeGroupedInputBox(div, "Appraisal", "Appraisal", "Appraised Value", "", "Relative")
                + MakeButton("btnSaveItem", div, "SaveItem", "SaveNewItem")
                +MakeButton("btnCancelItem", div, "CancelItem", "Cancel") +
               @" function SaveItem()
            {
                $.post('/Accounting/SaveNewItem',{Name:$(Name).val(),ItemTypeID:selvalItemTypeID,ManufacturerID:selvalManufacturerID,UPC:$(UPC).val(),Description:$(Description).val(),ItemNew:document.getElementById('cbNew').checked,ItemTested:document.getElementById('cbTested').checked,OwnerID:selvalOwner,Cost:$(Cost).val(),Price:$(Price).val(),Appraisal:$(Appraisal).val(),Serialized:document.getElementById('cbSerialized').checked});
            } 

               function AddItemType()
            {
                "

               + makeGroupedddlInputPair("DivQuickAdd", "RevAcct", "RevenueAccount", "Revenue Account", "", "Relative", "ACCOUNTS", "16", "")
                + makeGroupedddlInputPair("DivQuickAdd", "ExpAcct", "ExpenseAccount", "Expense Account", "", "Relative", "ACCOUNTS", "9", "") + makeGroupedddlInputPair("DivQuickAdd", "InvAcct", "InventoryAccount", "Inventory Account", "", "Relative", "ACCOUNTS", "14", "") + makeGroupedddlInputPair("DivQuickAdd", "COGAcct", "CostOfGoodsAccount", "Cost Of Goods Account", "", "Relative", "ACCOUNTS", "17", "") + MakeButton2("btnSaveItemType", "DivQuickAdd", "", "SaveItem", " SaveItemType(); ")
                 + @"
                
            }  
function SaveItemType()
            {
                
                $.post('/Common/SaveItemType',{NewItemType:$(tbItemTypeID).val(),RevAcct:$(ddlRevAcct).val(),ExpAcct:$(ddlExpAcct).val(),InvAcct:$(ddlInvAcct).val(),COGAcct:$(ddlCOGAcct).val()});
                $(DivQuickAdd).empty();
                
            }

function AddManufacturer()
            {
                $.post('/Common/SaveNewManufacturer',{Name:$(tbManufacturerID).val()});
            } 

function AddOwner()
            {
                $.post('/Home/SaveNewOwner',{Name:$(tbOwner).val()});
            } 
function CancelItem()
            {
                $(" + div+@").hide();
            }


                ";
        }
        public string MakeItems(string Owner, string Manufacturer, string LocationLabel, string ItemType)
        {
            string result = @" var LocTypeID=0; var NewLocName=''; $(SavLocDiv).hide();
            
            function SaveItem()
            {
                $.post('/Home/SaveItem',{Name:$(Name).val(),ItemTypeID:selvalItemTypeID,ManufacturerID:selvalManufacturerID,UPC:$(UPC).val(),Description:$(Description).val(),Quantity:$(Quantity).val(),ItemNew:document.getElementById('cbNew').checked,ItemTested:document.getElementById('cbTested').checked,LocationID:selvalLocation,OwnerID:selvalOwner,Cost:$(Cost).val(),Price:$(Price).val(),Appraisal:$(Appraisal).val()});
            } 

function AddItemType()
            {
                $(SavLocDiv).show();
                " + makeGroupedddlInputPair("TopDiv", "RevAcct", "RevenueAccount", "Revenue Account", "", "Relative", "ACCOUNTS", "16", "")
                + makeGroupedddlInputPair("TopDiv", "ExpAcct", "ExpenseAccount", "Expense Account", "", "Relative", "ACCOUNTS", "9", "") + MakeButton("btnSaveItemType", "TopDiv", "SaveItem", "SaveItemType") + @"
                
            }  

               function SaveItemType()
            {
                $.post('/Home/SaveItemType',{NewItemType:$(tbItemTypeID).val(),RevAcct:$(ddlRevAcct).val(),ExpAcct:$(ddlExpAcct).val()});
                
            }  


function AddManufacturer()
            {
                $.post('/Home/SaveNewManufacturer',{Name:$(tbManufacturerID).val()});
            } 

function AddLocation()
            {
                $(SavLocDiv).hide();
                $(btnShowLocType).show();
                $(tbLocation).val(NewLocName);
                $.post('/Home/SaveNewLocation',{Name:$(tbLocation).val(),LocType:LocTypeID});
            }
 
function CancelAddLocation()
            {
                $(SavLocDiv).hide();
                $(btnShowLocType).show();
                $(tbLocation).val(NewLocName);
                $.post('/Home/CancelNewLocation',{Name:$(tbLocation).val()});
            } 


function AddOwner()
            {
                $.post('/Home/SaveNewOwner',{Name:$(tbOwner).val()});
            } 

function AddLocType()
            {
                $.post('/Home/SaveNewLocType',{Name:$(tbLocation).val()});
            } 

function ShowLocType()
            {
                $(lblddlLocation).html('Choose or add location type for new location');
                NewLocName=$(tbLocation).val();
                $(SavLocDiv).show();
                $(btnShowLocType).hide();
                $.post('/Home/LocToLocType');
                
            } 

function ReceiveItems()
            {
                $.post('/Home/ReceiveItems');
                
            } 

                
            "

                + MakeButton("btnReceiveItems", "TopDiv", "ReceiveItems", "ReceiveItems")
                + MakeGroupedInputBox("TopDiv", "Name", "Name", "Name", "", "Relative")
                + makeGroupedddlInputPair("TopDiv", "ItemTypeID", "ItemTypeID", "ItemTypeID", ItemType, "Relative", "ITEMTYPE", "","") + MakeButton("btnAddItemType", "TopDiv", "AddItemType", "+")
                + makeGroupedddlInputPair("TopDiv", "ManufacturerID", "ManufacturerID", "ManufacturerID", Manufacturer, "Relative", "MANUFACTURERID", "","") + MakeButton("btnAddManufacturer", "TopDiv", "AddManufacturer", "+")
                + MakeGroupedInputBox("TopDiv", "UPC", "UPC", "UPC", "", "Relative")
                + MakeGroupedInputBox("TopDiv", "Description", "Description", "Description", "", "Relative")
                + MakeGroupedInputBox("TopDiv", "Quantity", "Quantity", "Quantity", "1", "Relative")
                + MakeCheckbox("","","TopDiv","cbNew","NEW","NEW","",false)
                + MakeCheckbox("", "", "TopDiv", "cbTested", "TESTED", "TESTED", "", false)
                + makeGroupedddlInputPair("LocDivMain", "Location", "Location", "Location", LocationLabel, "Relative", "LOCATION", "","") + MakeButton("btnShowLocType", "LocDivMain", "ShowLocType", "+") + MakeButton("btnSaveLocType", "SavLocDiv", "AddLocType", "+") + MakeButton("btnAddLocation", "SavLocDiv", "AddLocation", "SAVE") + MakeButton("btnCancelAddLocation", "SavLocDiv", "CancelAddLocation", "CANCEL")

                + makeGroupedddlInputPair("BottomDiv", "Owner", "Owner", "Owner", Owner, "Relative", "OWNER", "","") + MakeButton("btnAddOwner", "BottomDiv", "AddOwner", "+")
                + MakeGroupedInputBox("BottomDiv", "Cost", "Cost", "Cost", "", "Relative")
                + MakeGroupedInputBox("BottomDiv", "Price", "Price", "Price", "", "Relative")
                + MakeGroupedInputBox("BottomDiv", "Appraisal", "Appraisal", "Appraised Value", "", "Relative")
                + MakeButton("btnSaveItem", "BottomDiv", "SaveItem", "ADD") + MakeItemTable("TDIV", "tblItems", 0, 7,"ID")
                ;
            result = result + @" $.post('/Common/StartMain'); ";
            return result;
        }
        public string MakeItemTable(string div, string id, int startRec, int recPerPage, string SortKey)
        {
            List<Item> It = M.GetItemData(SortKey);
            List<Item> ItemPage = new List<Item>();
            string ItemType = "";
            string Manufacturer = "";
            string Owner = "";
            string actions = "";
            string result = @"

                            $(BTNSDIV).empty(); $(TDIV).empty();

function EditItem()
            {
                alert('EDIT ITEM');
            }  


$(" + div + @").append('<table id=" + id + @"><thead><tr><th id=thname></th><th>TYPE</th><th>MANUFACTURER</th><th>UPC</th><th>DESCRIPTION</th><th>QUANTITY</th><th>NEW</th><th>TESTED</th><th>OWNER</th><th>COST</th><th>PRICE</th><th>APPRAISAL</th></tr></thead><tbody id=" + id + @"body>";


            int NextPage = startRec + recPerPage;
            if (NextPage + recPerPage > It.Count) NextPage = It.Count - recPerPage;
            int PrevPage = startRec - recPerPage;
            if (PrevPage < 0) PrevPage = 0;
            int LastPage = It.Count - recPerPage;
            if (It.Count - recPerPage < 0) LastPage = 0;

            if (It.Count() > 0)
            {
                if (It.Count > recPerPage)
                {
                    if (startRec + recPerPage > It.Count) recPerPage = recPerPage - (startRec + recPerPage - It.Count);
                    for (int r = startRec; r < (startRec + recPerPage); r++)
                    {
                        ItemPage.Add(It[r]);
                    }
                }
                else
                {
                    foreach (Item it in It)
                    {
                        ItemPage.Add(it);
                    }
                }
                foreach (var i in ItemPage)
                {
                    if (i.ItemTypeID > 0) ItemType = M.GetItemTypeFromID(i.ItemTypeID);
                    if (i.OwnerID > 0) Owner = M.GetOwnerNameFromID(i.OwnerID);
                    if (i.ItemManufacturerID > 0) Manufacturer = M.GetManufacturerFromID(i.ItemManufacturerID);
                    result = result + @"<tr><td>" + M.fixstring(i.ItemName) + @"<Input id=Name" + i.ItemID + @"></Input></td>"+
@"<td>" + ItemType + @"</td>"+
@"<td>" + Manufacturer + @"</td>"+
@"<td>" + i.UPC + @"</td>"+
"<td>" + i.ItemDescription + @"</td>"+
@"<td id=tdQ" + i.ItemID + @"></td>"+
@"<td>" + MakeInnerCheckbox("", "icbNew", "New", "", Convert.ToBoolean(i.ItemNew)) + @"</td>"+
@"<td>" + MakeInnerCheckbox("", "icbNew", "New", "", Convert.ToBoolean(i.ItemTested)) + @"</td>"+
@"<td>" + Owner + @"</td>"+
@"<td>" + i.ItemCost + @"</td>"+
@"<td>" + i.ItemPrice + @"</td>"+
@"<td>" + i.ItemApprasial + @"</td>"+
@"<td id=BTNS" + i.ItemID + @"></td></tr>";
                    
                    actions = actions + 
                        MakeButton2("btnDEL" + i.ItemID, "BTNS" + i.ItemID, "", "X", @" $.post('/Home/DeleteItem',{ItemID:" + i.ItemID + @"}); ") + 
                        MakeButton2("btnEdit" + i.ItemID, "BTNS" + i.ItemID, "", "#", @" $.post('/Home/TableItemEdit',{ItemID:" + i.ItemID + @"}); ") + 
                        MakeButton2("btnSaveEdit" + i.ItemID, "BTNS" + i.ItemID, "", "*", @" $.post('/Home/SaveItemEdits',{ItemID:" + i.ItemID + @"}); ") + 
                        MakeButton2("btnCancelEdit" + i.ItemID, "BTNS" + i.ItemID, "", "X", @" $.post('/Home/CancelItemEdit',{ItemID:" + i.ItemID + @"}); ") +
                        MakeButton2("btnQ" + i.ItemID, "tdQ" + i.ItemID, "MainButtons", M.CalcItemQuantity(i.ItemID).ToString(), @" $.post('/Home/ItemQ',{ItemID:" + i.ItemID + @",tblcell:'tdQ"+i.ItemID+@"'}); ")
                        
                        
                        + @" $(Name" + i.ItemID + @").hide(); " + @" $(btnSaveEdit" + i.ItemID + @").hide(); " + @" $(btnCancelEdit" + i.ItemID + @").hide(); ";
                    ItemType = "";
                    Owner = "";
                    Manufacturer = "";
                }
               // result = result + @"<tr><td id=tdHome></td></tr>";



                actions = actions + MakeButton2("btnHome", "BTNSDIV", "PageButton", "<--", @" $.post('/Home/PageItemTable',{FirstRec:0,SKey:'" + SortKey + @"',recPerPage:"+recPerPage+@"}); ");
                actions = actions + MakeButton2("btnPrev", "BTNSDIV", "PageButton", "<", @" $.post('/Home/PageItemTable',{FirstRec:" + PrevPage + @",Skey:'" + SortKey + @"',recPerPage:" + recPerPage + @"}); ");
                actions = actions + MakeButton2("btnNext", "BTNSDIV", "PageButton", ">", @" $.post('/Home/PageItemTable',{FirstRec:" + NextPage + @",Skey:'" + SortKey + @"',recPerPage:" + recPerPage + @"}); ");
                actions = actions + MakeButton2("btnEnd", "BTNSDIV", "PageButton", "-->", @" $.post('/Home/PageItemTable',{FirstRec:" + LastPage + @",Skey:'" + SortKey + @"',recPerPage:" + recPerPage + @"}); ");
                actions = actions + MakeButton2("btnNameSort", "thname", "MainButton", "NAME", @" $.post('/Home/SortItemTable',{Skey:'NAME'}); ");
            }



            result = result + @"</tbody></table>'); " + actions;

            return result;
        }
        public string MakeItemQuan(string div, int ItemID)
        {
            string result = @" $(" + div + @").empty(); ";

            List<ItemLoc> iql = M.GetItemQuantities(ItemID);
            foreach (ItemLoc iq in iql)
            {
                result = result + @" $(" + div + @").append('<label id=lblQL" + iq.ItemLocID + @" class= >" + M.GetLocationLabelFromID(iq.LocID) + @"</label>--<label id=lblQQ" + iq.ItemLocID + @" class= >" + iq.Quantity + @"</label>'); " + MakeButton2("ChangeQ" + iq.ItemLocID, div, "", "±", @" $.post('/Home/ChangeItemQ',{ItemLocID:" + iq.ItemLocID + @",tblcell:'"+div+"',ItemID:"+ItemID+@"}); ");
            }
            result = result + MakeButton2("AddQ" + ItemID, div, "", "+", @" $.post('/Home/AddItemQ',{ItemID:" + ItemID + @",tblcell:'" + div + @"'}); ") + MakeButton2("CloseQ" + ItemID, div, "", "X", @" $.post('/Home/CloseItemQ',{ItemID:" + ItemID + @",tblcell:'" + div + @"'}); ");
            return result;
        }
        public string MakeAddQuan(string div, int ItemID)
        {
            string result = @" $(" + div + @").empty(); " + makeGroupedddlInputPair(div, "QLoc"+ItemID, "Select_Location", "Location", "", "Relative", "LOCATION", "","") + MakeGroupedInputBox(div, "QQuan"+ItemID, "Enter_Quantitiy", "Quantity", "1", "Relative") + MakeButton2("CloseQ" + ItemID, div, "", "X", @" $.post('/Home/CloseItemQ',{ItemID:" + ItemID + @",tblcell:'" + div + @"'}); ") + MakeButton2("SaveQ" + ItemID, div, "", "S", @" $.post('/Home/SaveItemQuan',{ItemID:" + ItemID + @",tblcell:'" + div + @"',LocID:selvalQLoc"+ItemID+@",Q:$(QQuan"+ItemID+@").val()}); ");
            return result;
        }
        public string MakeItemQCell(int ItemID, string div)
        {
            string result = @" $(" + div + @").empty(); " + MakeButton2("btnQ" + ItemID, div, "MainButtons", M.CalcItemQuantity(ItemID).ToString(), @" $.post('/Home/ItemQ',{ItemID:" + ItemID + @",tblcell:'" + div + @"'}); ");
            return result;
        }
        public string MakeItemQchange(int ItemLocID, string div, int ItemID)
        {
            

            string result = @" $(" + div + @").empty(); " + MakeGroupedInputBox(div, "NewQuan"+ItemLocID, "Enter_Quantitiy", "Quantity", "1", "Relative") + MakeButton2("CancelNewQuan" + ItemLocID, div, "", "X", @" $.post('/Home/CancelNewQ',{ItemLocID:" + ItemLocID + @",tblcell:'" + div + @"',ItemID:"+ItemID+@"}); ") + MakeButton2("SaveNewQ" + ItemLocID, div, "", "S", @" $.post('/Home/SaveNewQuan',{ItemLocID:" + ItemLocID + @",tblcell:'" + div + @"',Q:$(NewQuan" + ItemLocID + @").val(),ItemID:"+ItemID+@"}); ");

            return result;
        }
        public string MakeReceiveItems()
        {
            string result = "";
            string div = "TopDiv";
            result = result + MakeLabel("", div, "test", "TESTTESTTESTTEST");
            return result;
        }
        public string MakeAllocateItems(List<Classes.ITEMDATA> Items)
        {
            string result = @" 
            
            function saveAllocations()
            {
                var records=document.getElementById('tblItemAllocationbody').rows.length;
                var result='';
                var count=0;
                    while(count<records)
                    {
                        var LineID=document.getElementById('tblItemAllocationbody').rows[count].cells[0].childNodes[0].nodeValue;
                        
                        var ReceivedQuantity=document.getElementById('tblItemAllocationbody').rows[count].cells[2].childNodes[0].nodeValue;
                        var QuantityToAllocate=$(document.getElementById('QtoA'+count)).val();
                        var AllocationType=$(document.getElementById('ItemAllType'+count)).val();
                        var ExpenseID=$(document.getElementById('ddlExpChoice'+count)).val();
                        var LocationID=$(document.getElementById('ddlLocChoice'+count)).val();
                        
                        if(QuantityToAllocate!='')
                            {
                                if(!(QuantityToAllocate>ReceivedQuantity))
                                    {
                                        if(QuantityToAllocate>0 && AllocationType>0)
                                            {
                                                switch (AllocationType)
                                                    {
                                                        case '1':
                                                            if(ExpenseID>0)
                                                                {
                                                                    result=result+'{'+LineID+','+QuantityToAllocate+','+AllocationType+','+ExpenseID+'}';
                                                                }
                                                        break;
                                                        case '3':
                                                            if(LocationID>0)
                                                                {
                                                                    result=result+'{'+LineID+','+QuantityToAllocate+','+AllocationType+','+LocationID+'}';
                                                                }
                                                        break;
                                                        default:
                                                    }
                                            }
                                    }
                            }

                        
                        
                        count++;
                    }

                
                if(result!='')
                    {
                        $.post('Accounting/SaveItemAllocations',{DATA:result});
                    }
                
            }
            function AllTypeChange(L,C,Loc)
            {
                
                switch ($(L).val())
                    {
                      case '1':
                        $(C).show();
                        $(Loc).hide();
                        break;
                      case '3':
                        $(Loc).show();
                        $(C).hide();
                        break;
                        default:
                        $(C).hide();
                        $(Loc).hide();
                    }
            }
            function EXListChange(E)
            {
                
                
            }
            
            function LocListChange(L)
            {
                
            }

                


            
            
            ";
            string div = "ActDiv";
            string docfunction = @"$(document).ready(function()
            {";
            List<string> Headers = new List<string>();
            Headers.Add("LineID"); Headers.Add("ItemName"); Headers.Add("QuantityReceived"); Headers.Add("QuantityToAllocate"); Headers.Add("AllocateAs");
            result = result + MakeEmptyTable(div, "tblItemAllocation", "", "", "", Headers);
            int count = 0;
            foreach (Classes.ITEMDATA I in Items)
            {
                result = result + @" $(tblItemAllocationbody).append('<tr><td>" + I.LineID + @"</td><td>" + I.Name + @"</td><td >" + I.Quantity + "</td><td class=QuanClass ><input id=QtoA" + count + @" ></input></td><td id=choice" + count + @">" + MakeDropDown("ddlExpChoice" + count, "", "EXPENCES", "", "", "EXListChange(ddlExpChoice" + count + @") ", "CHOOSE EXPENSE") + MakeDropDown("ddlLocChoice" + count, "", "ITEMLOCATIONS", "", "", "LocListChange(ddlLocChoice" + count + @") ", "CHOOSE LOCATION") + @"<select id=ItemAllType" + count + @" onchange=AllTypeChange(ItemAllType" + count + @",ddlExpChoice" + count + @",ddlLocChoice" + count + @") ><option value=0>CHOOSE</option><option value=1>Expense</option><option value=2>Capitalize</option><option value=3>Inventory</option><option value=4>Customer</option></select></td></tr>'); ";
                docfunction = docfunction + @" $(ddlExpChoice" + count + @").hide(); $(ddlLocChoice" + count + @").hide(); ";
                count++;
            }
            result = result + MakeButton2("btnSubmit", div, "", "SAVE", "saveAllocations()");
            docfunction = docfunction + @"});";
            return result + docfunction;
        }
        public string MakeVenderItems(int VendID)//requires var Vitems=new Array(); and at page variable initialazation
        {
            string result = " ";
            List<VendItem> Vlist = M.getVenderItemList(VendID);
            int RecCount = 0;
            if (Vlist.Count > 0)//Make code java script object
            {
                foreach (VendItem vi in Vlist)
                {
                    result = result + @" vi={ItemID:'" + vi.ItemID + @"',VendPN:'" + vi.VendPN + @"',LastCost:'" + vi.LastCost + @"'};";
                    result = result + @" Vitems[" + RecCount + @"]=vi; ";
                    RecCount++;
                }
            }
            else
            {

            }

            return result;
        }
        public string MakeSerialedItemList()
        {
            string result = @" var serialedItems=[";
            List<int> SIs = M.getSerialedItems();
            bool firstitem = true;
            foreach (int i in SIs)
            {
                if (firstitem)
                {
                    result = result + i.ToString();
                    firstitem = false;
                }
                else result = result + @"," + i;
            }
            result = result + @"]; ";
            return result;
        }
        #endregion

        public string TableRecordPreppend(string tblbodyID,string classname,string Col1, string Col2, string Col3, string Col4, string Col5, string Col6, string Col7, string Col8, string Col9, string Col10, string Col11, string Col12)
        {
            string result = @" $(" + tblbodyID + @").prepend('<tr class=" + classname + @"><td>" + Col1 + @"</td><td>" + Col2 + @"</td><td>" + Col3+ @"</td><td>" + Col4+ @"</td><td>" + Col5+ @"</td><td>" + Col6+ @"</td><td>" + Col7+ @"</td><td>" + Col8+ @"</td><td>" + Col9+ @"</td><td>" + Col10+ @"</td><td>" + Col11+ @"</td><td>" + Col12+ @"</td></tr>'); ";
            return result;
        }
        public string MakeLocations()
        {
            string result = makeGroupedddlInputPair("LocDIV", "Lo", "Location", "Location", "", "Relative", "LOCATION", "","") + MakeButton2("ViewContents","LocDIV","","View"," $.post('/Home/LocDetails',{LocID:selvalLo()}); ")+@"











";

            result = result + @" $.post('/Common/StartMain'); ";


            return result;
        }
        public string MakeLocDetails(int locID)
        {
            string result = @"";

            return result;
        }
        


        #region Accounting  Accounting  Accounting **************************************************************
        public string MakeAccountingMain()
        {
            DateTime SD = Convert.ToDateTime(M.getAPstartDate()); DateTime ED = Convert.ToDateTime(M.getAPendDate());
            string Sdate = MakeHTMLDate(SD); string Edate = MakeHTMLDate(ED);
            string result = @" $(TopDiv).empty(); $(header).empty(); $(MainDiv).empty(); $(TotalsDiv).hide(); $(AcctSelDiv).empty(); 
            
            function cbAcctTypeClick(data)
                    {
                        //Add Date Range to this
                        $.post('/Accounting/UpdateAcctSummary',{AcctType:$(data).val(),Showit:$(data).attr('checked')});
                    }
            function AccountSelected(data,elementID,btn)
                    {   
                        $.post('/Accounting/AcctSummaryRecSelected',{RecID:data, ElementID:$(elementID).attr('id'),BTN:$(btn).attr('id')});
                    }


            
            ";
            result = result + JSetDateRange();
            result = result + MakeButton2("btnNewAcct", "TopDiv", "", "New Account", @" $.post('/Accounting/NewAccount'); ");
            result = result + MakeButton2("btnPurchase", "TopDiv", "", "New Purchase", @" $.post('/Accounting/NewPurchase'); ");
            result = result + MakeButton2("btnMakePayment", "TopDiv", "", "Make Payment", @" $.post('/Accounting/MakePayment'); ");
            result = result + MakeButton2("btnReceivePayment", "TopDiv", "", "Receive Payment", @" $.post('/Accounting/ReceivePayment'); ");
            result = result + MakeButton2("btnApplyPayments", "TopDiv", "", "Apply Payments", @" $.post('/Accounting/ApplyPayments'); ");
            result = result + MakeButton2("btnSell", "TopDiv", "", "New Sale", @" $.post('/Accounting/NewSale'); ");
            result = result + MakeButton2("btnTransferFunds", "TopDiv", "", "TransferFunds", @" $.post('/Accounting/TransferFunds'); ");
            result = result + MakeButton2("btnAddTaxLocation", "TopDiv", "", "AddTaxLocation", @" $.post('/Accounting/AddTaxLocation'); ");
            result = result + MakeButton2("btnAllocatePurchase", "TopDiv", "", "AllocateNewItems", @" $.post('/Accounting/AllocateNewItems'); ");
            result = result + MakeButton2("btnDeposit", "TopDiv", "", "Deposit", @" $.post('/Accounting/Deposit'); ");
            result = result + MakeButton2("btnMaintanance", "TopDiv", "", "Maintanance", @" $.post('/Accounting/Maintanance'); ");
            result = result + MakeOutput("outclass", "AcctSelDiv", "AcctTypeBtnsLable", "Display Account Types");
            
            result = result + MakeCheckBoxList("AcctSelDiv", "", "cbAcctTypes", "cbAcctTypeClick(this)", "AcctType");
            //Date Filter
            result = result + MakeLabel("", "MainDiv", "lblFilterDate", "Date Range");
            result = result + MakeDateField("MainDiv", "SDate", "StartDate", "", Sdate);
            result = result + MakeDateField("MainDiv", "EDate", "EndDate", "", Edate);
            result = result + MakeButton2("btnApplyDates", "MainDiv", "", "SetDateRange", "SetDateRange($(SDate).val(),$(EDate).val());");
            result = result + MakeSelectionTable("MainDiv", "legetable", "", "", "", "ledgbtn", 0, 10, "ACCOUNTS", "12", "");
            result = result + UpdateAcctSumTable("legetable", M.getUserAcctSumView(), "AccountSelected");
            result = result + @" $.post('/Common/StartMain'); ";
            result = result + @" $(legetable).append('<caption id=ledgetblcaption></caption>'); ";
            result = result + @" $(ledgetblcaption).append('Summary of Accounts'); ";

            return result;
        }
        public string MakePrepaidList()
        {
            
            List<Classes.Prepaid> P = M.getPrepays();//all prepays with a greater than 0 balance
            string result = @" 
                                var Prepays=new Array();
                                //pp=new Object();

";
            int count = 0;
            foreach (Classes.Prepaid p in P)
            {
                result = result + @"var pp={AcctID:" + p.AcctID + @",PType:" +p.ServiceTypeID+ @",PName:'"+M.getPrepayTypeName(p.ServiceTypeID)+@"',Bal:"+p.Balance+@"}; ";
                result = result + @" Prepays[" + count + @"]=pp; ";
                count++;
            }
            return result;
        }
        public string MakePrePaidValues()
        {
            List < ServiceCreditValue > SCV= (from scv in db.ServiceCreditValues select scv).ToList();
            string result = @"
                                var PrePayValue=new Array();
";
            int count=0;
            foreach (ServiceCreditValue v in SCV)
	{
        result = result + @" var ppv={SCTID:" + v.SCTID + @",ItemID:" + v.ItemID + @",Value:" + v.Value + @"}; ";
        result = result + @" PrePayValue[" + count + @"]=ppv; ";
        count++;
	}
            

            return result;
        }
        
        public string MakeNewAccount()
        {
            string result = " $(TopDiv).empty(); ";
            result = result + MakeGroupedInputBox("TopDiv", "tbAcctName", "AccountName", "Account Name", "", "Relative");
            result = result + MakeGroupedInputBox("TopDiv", "tbAcctNum", "AccountNumber", "Account Number", "", "Relative");
            result = result + makeGroupedddlInputPair("TopDiv", "AcctType", "AccountType", "Account Type", "Checking", "Relative", "ACCTTYPE", "","");
            result = result + MakeButton2("btnSaveNewAcct", "TopDiv", "", "Save New Account", @" $.post('/Accounting/SaveNewAccount',{AcctName:$(tbAcctName).val(), AcctNumber:$(tbAcctNum).val(), AcctType:selvalAcctType}); ");
            
            return result;

        }
        public string MakeCheckingDetails(int AcctID)
        {
            AccountTable Acct=M.getAccountFromID(AcctID);
            string result = @" $(ActDiv).empty(); $(TDIV).empty(); ";
            result = result + MakeButton2("btnBack", "TopDiv", "SpecialButton", "BACK", @" $.post('/Accounting/StartAccounting'); ");
            result=result+MakeLabel("titlelabel", "FeeDiv", "lblAcctName", Acct.AcctID + @"  " + Acct.AcctName + @"   " + Acct.AcctNumber);

            result = result + MakeSelectionTable("ActDiv", "tblLedger", "ledgtable", "ledgcell", "ledgrow", "ledgbtn", 0, 10, "GENERALLEDGER", AcctID.ToString(),"");
            return result;
        }
        public string MakeAccountDetails(int AcctID,string ElementID,string FromBTN)
        {
            AccountTable Acct = M.getAccountFromID(AcctID);
            string result = "";
            
            result = result + MakeSelectionTable(ElementID, "tblLedger"+AcctID.ToString(), "ledgtable", "ledgcell", "ledgrow", "ledgbtn", 0, 10, "GENERALLEDGER", AcctID.ToString(),FromBTN);
            
            return result;
        }
        public string MakePayableReceivableDetails(int AcctID)
        {
            AccountTable Acct = M.getAccountFromID(AcctID);
            string result = @" $(ActDiv).empty(); $(TDIV).empty(); ";
            result = result + MakeButton2("btnBack", "TopDiv", "SpecialButton", "BACK", @" $.post('/Accounting/StartAccounting'); ");
            result = result + MakeLabel("titlelabel", "FeeDiv", "lblAcctName", Acct.AcctID + @"  " + Acct.AcctName + @"   " + Acct.AcctNumber);

            result = result + MakeSelectionTable("ActDiv", "tblLedger", "ledgtable", "ledgcell", "ledgrow", "ledgbtn", 0, 10, "ACCTLEDGER", AcctID.ToString(),"");
            return result;
        }
       
        public string MakeTransferFunds()
        {
            string result = @" 
$(LocDivMain).hide(); $(TopDiv).empty(); $(PitemDiv).empty(); $(LocDivMain).empty();


 function SaveTransfer()
            {
               
                $.post('/Accounting/SaveFundsTransfer',{Tdate:$(Tdate).val(),Cact:$(ddlCreditAccount).val(),Dact:                          $(ddlDebitAccount).val(),Tmethod:$(ddlTransferMethod).val(),Reference:$(tbReference).val(),Desc:                           $(tbDescription).val(),Tamt:$(transamt).val(), PayAct:$(ddlVendAcct).val(), FEEAMT:$(FeeAmt).val()});
               
            }

function cbFeeClicked()
            {
                if(cbFee.checked)
                    {
 
                        $(LocDivMain).show();
                        
                    }
                else
                    {
                        $(FeeAmt).val(0);
                        $(LocDivMain).hide();
                    }
            }





";
            result = result + MakeCheckbox("", "", "ActDiv", "cbFee", "TransAction Fee?", "TransactionFee?", " cbFeeClicked() ", true);
            result = result + makeGroupedddlInputPair("LocDivMain", "VendAcct", "PayeeAcct", "Payee Account", "", "Relative", "ACCOUNTS", "6", "");
            result = result + MakeMoneyInput("LocDivMain", "FeeAmt", "", "", "Fee Amount");
            result = result + MakeGroupedInputBox("PitemDiv", "Tdate", "TransferDate", "Transfer Date", DateTime.Now.ToShortDateString(), "Relative");
            result = result + makeGroupedddlInputPair("PitemDiv", "CreditAccount", "FromAccount", "From Account", "", "Relative", "ACCOUNTS", "1-2-12", "");
            result = result + makeGroupedddlInputPair("PitemDiv", "DebitAccount", "ToAccount", "To Account", "", "Relative", "ACCOUNTS", "1-2-12", "");
            result = result + makeGroupedddlInputPair("PitemDiv", "TransferMethod", "TransferMethod", "Transfer Method", "", "Relative", "TRANSACTIONTYPE", "", "");
            result = result + MakeGroupedInputBox("PitemDiv", "tbReference", "Reference", "Reference", "", "Relative");
            result = result + MakeGroupedInputBox("PitemDiv", "tbDescription", "Description", "Description", "", "Relative");
            result = result + MakeMoneyInput("PitemDiv", "transamt", "", "", "Amount");
            result = result + MakeButton2("btnSaveTransfer","PitemDiv", "", "MakeTransfer", @" SaveTransfer() ");
            return result;
        }
        public string MakeTransactionDetails(int TransactionID)
        {
            string result = "";
            //result = result + MakeButton2("btnBack", "TopDiv", "SpecialButton", "BACK", @" $.post('/Accounting/StartAccounting'); ");
            result = result + MakeButton2("btnSaveTransChanges", "TopDiv", "mobilebtn", "SaveChanges", @" $.post('/Accounting/SaveTransChanges',{TransactionID:"+TransactionID+@",DocPath:$(DocumentPath).val()}); ");
            result = result + MakeGroupedInputBox("ActDiv", "DocumentPath", "DocumentPath", "Document Path",M.fixstring(M.GetDocumentPathFromID(M.GetDocumentIDFromTransactionID(TransactionID))), "Relative");
            return result;
        }
        public string MakeNewSaleStartup()
        {
            return @" $(TopDiv).empty(); $(ActDiv).empty(); var lineitems=''; var itemcount=0;
                                var itemarray=new Array(); $(PitemDiv).hide(); $(PitemDiv).empty();
                                $(BottomDiv).empty(); var Total=0; var subtotal=0; var TaxTotal=0; var Taxable=0; var Paid=false;

var NonTaxable=0; var snlist=new Array(); var presnlist=new Array(); var serialized=false; 


var IsInvoice=true; var CurrentPayMeth=1; var HasServiceCredits=false; var ServiceCreditsBalance=0; var CustID=0; var SCTypeID=0; var ItemVerified=true; var ItemServiceCredits=0;


";
        }
        public string MakeNewSale()
        {
             #region RaW Javascript ***********************************************************************************
            string result = MakeNewSaleStartup()+JSaleLineClick()+JSaleAddItem()+JSaleDelLine()+JSaveSale()+JSaveSalePaid()+JSaleItemAdd()+JSaleItemChange()+
@"

            



function LineAllocationChange(Aline,rcount)
            {
               
                var idx=0;
                
                
               
                while(idx<itemarray.length)
                {
                    if(itemarray[idx].LineID==rcount)
                    {
                        itemarray[idx].LineAllocation=$(Aline).val();
                    }
                    idx++;
                }
                
            }










function NewItem()
            {
                $(PitemDiv).show();
            }

function NewCustomer()
            {
                $.post('/Accounting/NewCustomer');
            }

function TaxChange()
            {
                $(lblddlTaxRateList).text('Tax Rate:'+($(ddlTaxRateList).val()*100).toFixed(2)+'%');
                var TT=Number($(TotalTaxableSale).val())
                $(Tax).val(($(ddlTaxRateList).val()*TT).toFixed(2));
                $(SaleTotal).val((TT+Number($(Tax).val())+Number($(TotalExemptSale).val())).toFixed(2));
            }

function BackBtn()
            {
                $(ActDiv).empty(); $(BottomDiv).empty();
                $.post('/Accounting/StartAccounting');
            }
function MakeSnList(itemid,quantity)
            {
                
                $(DivQuickAdd).empty();
                $(DivQuickAdd).append('<label>Enter Serial Numbers</label><br></br>');
                var lcount=0;
                while(lcount<quantity)
                    {
                        $(DivQuickAdd).append('<input onblur=SerialFieldBlur(this) type=text id=SN'+lcount+'></input>');
                        lcount++;
                    }
                $(DivQuickAdd).append('<Button onclick=SaveSns('+itemid+','+quantity+')>SAVE</Button>');
                $(DivQuickAdd).append('<Button onclick=CancelSns()>CANCEL</Button>');
            }
function SaveSns(itemID,quan)
            {
                var alldatagood=true;
                if(presnlist.length==quan)
                    {
                        for(var z=0;z<presnlist.length;z++) //make sure all serial number fields contain data and if not exit
                            {
                    
                                if(presnlist[z].sn.length<1)
                                    {
                                        alldatagood=false;
                                        
                                    }
                            }
                    }
                else
                    {
                        alldatagood=false;
                    }
                if(alldatagood)
                    {
                        
                        var transfersns=new Array();
                        
                        snlist.push({Prow:itemcount,ItemID:itemID,sns:transfersns});
                            var precount=0;
                            while(precount<presnlist.length)
                                {
                                    snlist[snlist.length-1].sns[precount]=presnlist[precount].sn;
                                    precount++;
                                    
                                }
                        $(DivQuickAdd).empty();
                        presnlist.splice(0,presnlist.length); //empty the array
                        ItemAdd();
                    }
                else
                    {
                        alert('missing serial number');
                    }
                
            }
function CancelSns()
            {
                alert('This item must inclued serial numbers. Item Add Canceled.');
                $(DivQuickAdd).empty();
                presnlist.splice(0,presnlist.length); //empty the array
            }
function SerialFieldBlur(event)
            {
                var count=0;
                var notinlist=true;
                while(count<presnlist.length)
                    {
                        if($(event).data().snid==presnlist[count].ifield)
                                {
                                    presnlist[count].sn=$(event).val();
                                    notinlist=false;
                                }
                        count++;
                    }
                if(notinlist)
                    {
                        
                        $(event).data('snid',count);
                        presnlist.push({ifield:count,sn:$(event).val()});
                    }
                
            }

            function SetSerialized(itemID)
            {
                serialized=false;
                for(var i=0;i<serialedItems.length;i++)
                    {
                        if(serialedItems[i]==itemID)
                            {
                                serialized=true;
                                return;
                            }
                    }
            }



";

            #endregion
            result = result + MakeSerialedItemList() + JCustomerSelected() + MakePrepaidList() + MakePrePaidValues() + JPrepaySelected() + JSetServiceCredits() + JPayMethSelected() + JVerifyItem() + JAddLaborToInvoice();
            result =result + MakeButton2("btnBack", "TopDiv", "SpecialButton", "BACK",@" BackBtn();  ");
            result = result + MakeButton2("btnNewCustomer", "TopDiv", "SpecialButton", "NewCustomer", " NewCustomer();");
            result = result + MakeButton2("btnNewItem", "TopDiv", "SpecialButton", "NewItem", " NewItem() ");
            //Form Labels
            result = result + MakeLabel("NewSaleLabel", "ActDiv", "lblIDate", "Invoice Date");
            result = result + MakeLabel("NewSaleLabel", "ActDiv", "lblPayMeth", "Payment Method");
            
            result = result + MakeLabel("NewSaleLabel", "ActDiv", "lblCustAcct", "Customer Name");
            
            result = result + @" $(ActDiv).append('<br/>'); ";


            
           
            

            result = result + makeddlInputPair("ActDiv", "Cust", "CustomerName", "Customer Name", "", "ACCOUNTS", "15", " CustomerSelected(); ","SelectCustomer");
            
            
            result = result + MakeDateField("ActDiv", "SDate", "Sale Date","","");
            result = result + MakeAutoDDL("ActDiv", "PayMeth", "any", "PAYMETH","PayMethSelected()");
            
            result = result + makeGroupedddlInputPair("ActDiv", "ItemList", "ItemList", "", "", "Relative", "SALEITEMS", "", " ItemChange(); ");
            result = result + MakeButton2("btnAddLabor", "ActDiv", "*", "AddLabor", "AddLaborToInvoice()");
            result = result + MakeLabel("NewSaleLabel", "ActDiv", "lblQuantity", "Quantity");
            result = result + MakeLabel("NewSaleLabel", "ActDiv", "lblItemAmount", "Item Amount");
            result = result + @" $(ActDiv).append('<br/>'); ";
            result = result + MakeNumberBox("ActDiv", "SQ", "ItemQuantity", "Quantity", "1", "Relative",1);
          //  result = result + MakeInputBox("ActDiv", "PAMT", "NewSaleData", "ItemAmount", "Amount", "");
          //  result = result + MakeMoneyInput("ActDiv", "SAMT", "NewSaleData", "NewSaleData", "Amount");
            result = result + MakeInputBox("ActDiv", "ItemDescription", "NewSaleData", "ItemDescription", "Item Description","");
            result = result + MakeMoney("ActDiv", "SAMT", "NewSaleData","");
           
            result = result + MakeButton2("btnAddItem", "ActDiv", "", "AddItem", " AddItem();");
            //If item is serialized require the serial number when chosen
            result = result + @" $(ActDiv).append('<br/>'); ";
            result = result + MakeLabel("NewSaleLabel", "ActDiv", "lblTotalExempt", "Total Tax Exempt");
            result = result + MakeLabel("NewSaleLabel", "ActDiv", "lblTotalTaxed", "Total Taxed");
            result = result + MakeLabel("", "ActDiv", "lblTax", "Tax");
            result = result + MakeLabel("", "ActDiv", "lblTotal", "Total");
            result = result + @" $(ActDiv).append('<br/>'); ";
            result = result + MakeInputBox("ActDiv", "TotalExemptSale", "NewSaleData", "TotalExemptAmount", "Total Exempt", "");
            result = result + MakeInputBox("ActDiv", "TotalTaxableSale", "NewSaleData", "TotalTaxableAmount", "Total Taxable", "");
            result = result + MakeInputBox("ActDiv", "Tax", "NewSaleData", "Tax", "Tax", "");
            result = result + MakeInputBox("ActDiv", "SaleTotal", "NewSaleData", "PurchaseTotal", "Total Purchase", "");
            result = result + makeGroupedddlInputPair("ActDiv", "TaxRateList", "TaxRateList", "Tax Rates", "", "Relative", "TAXRATES", "", " TaxChange(); ");
            
            
            
            
            
            
            result = result + MakeNewItemElements("PitemDiv");
            

            result = result + MakeSaleTable();
            
            result = result + MakeButton2("btnSaveUnpaid", "BottomDiv", "", "SaveAsUnpaid", " SaveSale();");
            result = result + MakeButton2("btnSavePaid", "BottomDiv", "", "SaveAsPaid", " SaveSalePaid();");
            
            
            return result;
        }
        
        
        public string MakeEnterFeeOffer(string Tdate, int Cact, int Tmethod, string Reference, string FeeRequestText)
        {
            string result = "";// @" $(TopDiv).empty(); ";
            result = result + MakeLabel("Requestlbl", "TopDiv", "lblAskForFee", FeeRequestText);
            result = result + MakeButton2("btnYestoFee", "TopDiv", "YESbtn", "YES", @" $.post('/Accounting/YesTransFee'); ");
            result = result + MakeButton2("btnNotoFee", "TopDiv", "NObtn", "NO", @" $.post('/Accounting/NoTransFee',{Tdate:$(Tdate).val(),Cact:$(ddlCreditAccount).val(),Dact:$(ddlDebitAccount).val(),Tmethod:$(ddlTransferMethod).val(),DocID:$(tbDocID).val(),Desc:$(tbDescription).val(),Tamt:$(transamt).val()}); ");
            
            return result;
        }
        public string MakeTransFee()
        {
            string result = "";// @" $(TopDiv).empty(); ";
            result = result + makeGroupedddlInputPair("TopDiv", "VendAcct", "PayeeAcct", "Payee Account", "", "Relative", "ACCOUNTS","6", "");
            result = result + makeGroupedddlInputPair("TopDiv", "FeeAcct", "FeeAccount", "Fee Account", "", "Relative", "ACCOUNTS", "9", "");
            result = result + MakeMoneyInput("TopDiv", "FeeAmt", "", "", "Fee Amount");
            result = result + MakeButton2("btnSaveTransFee", "TopDiv", "", "SAVE FEE", @" $.post('/Accounting/SaveTransWithFee',{Tdate:$(Tdate).val(),Cact:$(ddlCreditAccount).val(),Dact:$(ddlDebitAccount).val(),Tmethod:$(ddlTransferMethod).val(),DocID:$(tbDocID).val(),Desc:$(tbDescription).val(),Tamt:$(transamt).val(), PayAct:$(ddlVendAcct).val(), FeeAct:$(ddlFeeAcct).val(), AMT:$(FeeAmt).val()}); ");

            return result;
        }
        public string MakeReceivePayment()
        {
            string result = MakeRecPayStartup() + JDepNowClicked() + JRecAcctChange() + JApplyPaymentClicked()+JSavePaymentReceived();
            //Immediate Deposit

            result = result + MakeCheckbox("", "", "TopDiv", "ApplyPayment", "</br>Apply Payment", "ApplyPayment", "ApplyPaymentClicked()", false);
            result = result + MakeLabel("", "FeeDiv", "IDWarning", @" Payment received will be recorded as a single item deposit. Use this only if the received payment was already deposited without any other payments.");
            result = result + MakeGroupedInputBox("FeeDiv", "DepRef", "DepositReference", "Deposit Reference", "", "Relative");
            result = result + makeGroupedddlInputPair("FeeDiv", "BankAcct", "BankAcct", "Bank Account", "", "Relative", "ACCOUNTS", "1-2", "");
            result = result + MakeCheckbox("", "", "TopDiv", "depnow", "</br>Immediate Deposit", "ImediateDeposit", "DepNowClicked()", false);
            //Payment
            result = result + MakeDateField("ActDiv", "Pdate", "PaymentDate", "", "");
            result = result + makeGroupedddlInputPair("ActDiv", "ReceivableAcct", "ReceiveAcct", "Receivable Account", "", "Relative", "ACCOUNTS", "13-15-18-19", "RecAcctChange()");
            result = result + makeGroupedddlInputPair("ActDiv", "PaymentMethod", "PaymentMethod", "Payment Method", "", "Relative", "PAYMETH", "", "");
            result = result + MakeGroupedInputBox("ActDiv", "tbPayRef", "PaymentReference", "Payment Reference", "", "Relative");
            result = result + MakeGroupedInputBox("ActDiv", "tbDescription", "Description", "Description", "", "Relative");
            result = result + MakeMoneyInput("ActDiv", "receivedamt", "", "", "Amount");
            
            result = result + MakeButton2("btnSaveReceived", "ActDiv", "", "SAVE", @" SavePaymentReceived() ");
            return result;
        }
        public string MakeApplyPayments()
        {
            string result = @"

                                function PaymentSelected(data)
                                    {
                                        if(!($(data).val()==0))
                                        {
                                            $.post('/Accounting/AllocatePayment',{DOCS:$(data).val()});
                                        }
                                    }


$(ActDiv).empty(); $(ActDiv).append('<Table id=tblPayments><tr><th>CreditAccount</th><th>PaymentDate</th><th>Reference</th><th>Amount</th><th>Unallocated</th><th>Available</th></tr>";//<select id=ddlPaymentsFrom onchange=PaymentSelected(this)>";
            //Within parameters of master date range
            //Display Customers with upaid invoices when selected unpaid invoices are displayed and amount of unapplied payments. When invoices are selected deduct amount from unapplied payments.
            //Find Customers unapplied payments using reconciled field of payment documents
            
            List<Document> D = M.getIncompletPayments();
            
            //Build a list of payment details
            List<Classes.PaymentDetail> IncompletePayments = new List<Classes.PaymentDetail>();
            foreach (Document Doc in D)
            {
                IncompletePayments.Add(M.getPaymentDetails(Doc.DocumentID));
            }
            //sort by account name
            IncompletePayments = (from IP in IncompletePayments orderby IP.PaymentCreditName select IP).ToList();
            foreach (Classes.PaymentDetail PD in IncompletePayments)
            {
                result = result + @"<tr><td>" + PD.PaymentCreditName + @"</td><td>" + PD.PDate.ToShortDateString() + @"</td><td>" + PD.PayDoc.DocumentID + "</td><td>" + Math.Round(PD.PayDoc.Amount,2) + @"</td><td id=UP" + PD.PayDoc.DocumentID + @">" +Math.Round( PD.UnallocatedPayment,2) + @"</td><td>";
                // get all unpaid items with amount left due per customer and place in select in this field
                //get incomplete invoice documents per customer
                List<Classes.InvoiceDetail> IDocs = M.getIncompleteInvoicesByAcctID(PD.PaymentCreditID);
                //get totals left to pay on each invoice per customer
                result = result + @"<select id=II" + PD.PayDoc.DocumentID + @" onchange=PaymentSelected(this)>";
                result = result + @"<option value=0>Select Unpaid Item</option>";
                foreach (Classes.InvoiceDetail IvD in IDocs)
                {
                    result = result + @"<option value="+PD.PayDoc.DocumentID+@","+IvD.InvoiceDocID+@">" + IvD.InvoiceNumber + @"--$"+Math.Round(IvD.UnpaidAmount,2)+@"</option>";
                }
                result=result+@"</select></td></tr>";
 
                

            }
           
            result = result + @"</table>'); ";

            return result;
        }
        
        public string MakeDepositOffer(string Pdate, int Cashact, int Pmethod, string DocRef, decimal AMT, string DepositRequestText)
        {
            string result = @" $(TopDiv).empty(); ";
            result = result + MakeLabel("Requestlbl", "TopDiv", "lblAskForDeposit", DepositRequestText);
            result = result + MakeButton2("btnYestoFee", "TopDiv", "YESbtn", "YES", @" $.post('/Accounting/YesDeposit',{Pdate:'" + Pdate + @"' ,Cashact:" + Cashact + @", Pmethod:" + Pmethod + @", DocID:'" + DocRef + @"', AMT:"+AMT+@"}); ");
            result = result + MakeButton2("btnNotoFee", "TopDiv", "NObtn", "NO", @" $.post('/Accounting/NoTransFee');");//No TransFee does the same thing as a no deposit would do

            return result;
        }
        public string MakeQuickDeposit(string Ddate, int Cashact, int Dmethod, decimal AMT)
        {
            string result = @" $(TopDiv).empty(); ";
            result = result + makeGroupedddlInputPair("TopDiv", "BankAcct", "BankAccount", "Bank Account", "", "Relative", "ACCOUNTS", "1-2", "");
            result = result + MakeGroupedInputBox("TopDiv", "DepRef", "DepositReference", "Deposit Receipt Reference", "", "Relative");
            
            result = result + MakeButton2("btnSaveQuickDeposit", "TopDiv", "", "SAVE DEPOSIT", @" $.post('/Accounting/SaveQuickDeposit',{Ddate:'" + Ddate + @"' ,Cashact:" + Cashact + @", Dmethod:" + Dmethod + @", BankAct:$(ddlBankAcct).val(), Desc:' Deposit from "+M.getAccountFromID(Cashact).AcctName+@"', AMT:"+AMT+@",DocRef:$(DepRef).val()}); ");

            return result;
        }
        public string MakeDeposit()
        {
            //Accounts 12 and 20 are depositalble accounts
            
            List<Classes.AccountDetails> AD = M.getAcctsWithPlusBalance(12);
            List<Classes.AccountDetails> Notes = M.getAcctsWithPlusBalance(20);
            foreach (Classes.AccountDetails act in Notes)
            {
                AD.Add(act);
            }
            List<TransactionTable> NoteItems = M.getNonDepositedBankNotes();
            string result = @"

$(ActDiv).empty(); ";
           

            result = result+@" Current=new Object(); var TotalDeposit=0; $(DivDeposit).empty(); ";
            result =  result + MakeDateField("DivDeposit", "DDate", "Deposit Date","","");
            result = result + MakeInputBox("DivDeposit", "DepRef", "", "Deposit Reference", "", "");
            result = result + JDepositAcctSelected() + MakeAcctObjectArray(AD) + MakeTransactionTableObjectArray(NoteItems) + JDepositCash() + JDepositChecks() + JAddDepositItems() + JSaveDeposit2() + JNoteItemSelected()+JRemoveFromDeposit();
            List<string> DepItemHeads=new List<string>{"TransID","AcctID","AcctName","Item","Amount","Remove"};
            result = result + MakeEmptyTable("DivDeposit", "tblDepItems", "", "", "", DepItemHeads);
            result = result + @"$(DivDeposit).append('<Table id=tblNotes><tr><th>DocID</th><th>TransID</th><th>Date</th><th>Description</th><th>Amount</th></tr></table>'); ";
            result = result + MakeLabel("", "DivDeposit", "lblDepositLable", "Total Deposit");
            result = result + MakeLabel("", "DivDeposit", "lblTotalDeposit", "$0.00");
            result = result + MakeLabel("", "DivDeposit", "lblError", "ERROR");
            result = result + @" $(DivDeposit).append('" + MakeDropDown("DepositAccounts", "", "ACCOUNTS", "Cash", "12-20-BAL", "DepositAcctSelected()", "SelectItemAccount") + @"');";
            result = result + @" $(DivDeposit).append('" + MakeDropDown("BankAccounts", "", "ACCOUNTS", "Cash", "1-2", "", "SelectBankAccount") + @"');";
            result = result + MakeMoneyInput("DivDeposit", "CashIn", "", "", "Enter Cash Amount");
            result = result + MakeButton2("AddItem", "DivDeposit", "", "ADD", "AddDepositItems()");
            result = result + MakeButton2("SaveDeposit", "DivDeposit", "", "SAVE", "SaveDeposit2()");
            result = result + MakeButton2("CancelDepost", "DivDeposit", "", "CANCEL", "$(DivDeposit).empty()");
            result = result + JDocumentReady(" $(lblCashIn).hide(); $(CashIn).hide(); $(lblError).hide(); $(tblNotes).hide(); ");

            
            return result;
        }
        public string MakeAcctObjectArray(List<Classes.AccountDetails> Accts)
        {
            string result = @"
                                var Accts=new Array();
                                
                                

";
            int count = 0;
            foreach (Classes.AccountDetails ad in Accts)
            {
                result = result + @" Accts[" + count + @"]=new Object();
                                    Accts["+count+@"].AcctID="+ad.AcctID+@";
                                    Accts["+count+@"].AcctType="+ad.AcctType+@";
                                    Accts["+count+@"].AcctName='"+ad.Name+@"';
                                    Accts["+count+@"].AcctBal="+ad.Balance+@";

";
                count++;
            }
            return result;
        }
        public string MakeTransactionTableObjectArray(List<TransactionTable> BNS)
        {
            string result = @"
                                var TransactionTable=new Array();
";
            int count = 0;
            foreach (TransactionTable TD in BNS)
            {
                result = result + @" TransactionTable[" + count + @"]=new Object();
                                    TransactionTable[" + count + @"].DebitID=" + TD.DebitID + @";
                                    TransactionTable[" + count + @"].CreditID=" + TD.CreditID + @";
                                    TransactionTable[" + count + @"].Amount=" + TD.Amount + @";
                                    TransactionTable[" + count + @"].TransactionTypeID=" + TD.TransactionTypeID + @";
                                   TransactionTable[" + count + @"].TransactionID=" + TD.TransactionID + @";
                                   TransactionTable[" + count + @"].EnteredBy='" + TD.EnteredBy + @"';
                                    TransactionTable[" + count + @"].DocumentID=" + TD.DocumentID + @";
                                    TransactionTable[" + count + @"].Description='" + TD.Description + @"';
                                    TransactionTable[" + count + @"].EnterDate='" + MakeHTMLDateTime(TD.EnterDate) + @"';
                                    TransactionTable[" + count + @"].USED=new Boolean(false);
";
                count++;
            }
            return result;
        }
        
        
        
        
        public string MakeMain(int MobilePreference)
        {
            string result = " $(header).empty(); ";
            result = MakeButton2("btnMobile", "header", "mobilebtn", "MOBILE", @" if ($(btnMobile).text()=='MOBILE') { $(btnMobile).text('DESKTOP'); $('*').addClass('Mobile'); $.post('/Common/SetMobilePreference',{MobilePreference:2}); } else {$(btnMobile).text('MOBILE'); $('*').removeClass('Mobile'); $.post('/Common/SetMobilePreference',{MobilePreference:1});} ");
            if (MobilePreference == 2) //Site Preference is Mobile
            {
                result = result + @" $('*').addClass('Mobile'); $(btnMobile).text('DESKTOP'); ";
            }
            
            return result;
        }
        public string MakeQuickAddAccount(int AccountTypeID)
        {
            string result = @" $(DivQuickAdd).empty(); 

";
            
            switch (AccountTypeID)
	{
                case 15:
            result = result + MakeGroupedInputBox("DivQuickAdd", "tbNewAcctName", "CustomerName", "Customer Name", "", "Relative");
            break;
		
	}
            result = result + MakeGroupedInputBox("DivQuickAdd", "tbNewAcctNumber", "AccountNumber", "Account Number", "", "Relative");
            result=result+MakeButton2("btnSaveNewAcct","DivQuickAdd","SaveButton","SAVE"," $.post('/Accounting/SaveNewAccount',{AcctName:$(tbNewAcctName).val(),AcctNumber:$(tbNewAcctNumber).val(),AcctType:"+AccountTypeID+@"}); $(DivQuickAdd).empty(); ");
            result = result + MakeButton2("btnCancelNewAcct", "DivQuickAdd", "CancelButton", "CANCEL", " $(DivQuickAdd).empty(); ");
            return result;
        }

        public string MakeRecPayStartup()
        {
            string result = @"
                                var DI=false; $(FeeDiv).empty(); $(FeeDiv).hide(); $(ActDiv).empty(); var ApplyPay=false; var InvoiceArray=new Array(); var AppliedAmount=0; var AppliedPayList=new Array(); var AppliedInvoices='';


                                


";
            return result;
        }

        public string MakePurchase()
        {

            #region MakePurchase Functions ***********************************************************************************
            string result = MakePurchaseVariables() + JCreateExpenseOptionsVariable() +
 JAddItem() + JLineAllocationChange() + JDelLine() + JSavePurchase() + JSaveAs() + JFeeBoxClicked() + JCashBackBoxClicked() + JLineClick() + JTaxChange() + JNewItem() + JItemAdd() + JMakeSnList() + JSaveSns() + JCancelSns() + JSerialFieldBlur() + JItemChange() + JSetSerialized() + JRemoveDollarSign() + JFeeAmtBlur() + JCashBackAmtBlur() + JShippingCOGBlur() + JShippingExpBlur() + JExpenseOverideSelected() + JPTaxBlur() + JVenderListChange() + JDocumentReady("");

            #endregion

            #region MakePurchase Presentation
            result = result + MakeSerialedItemList();
            result = result + MakeButton2("btnBack", "TopDiv", "SpecialButton", "BACK", @" $.post('/Accounting/StartAccounting'); ");

            //Transaction Fee
            result = result + MakeCheckbox("", "", "ActDiv", "chkFee", "Transaction Fee", "TransactionFee", "FeeBoxClicked()", false);
            result = result + MakeLabel("NewSaleLabel", "FeeDiv", "lblTransFee", "Transaction Fee Amount");
            result = result + MakeMoney("FeeDiv", "FeeAmt", "NewSaleData", "FeeAmtBlur()");


            //Cash Back
            result = result + MakeCheckbox("", "", "ActDiv", "chkCashBack", "Cash Back", "CashBack", "CashBackBoxClicked()", false);
            result = result + MakeLabel("NewSaleLabel", "CashBackDiv", "lblCashBack", "Cash Back Amount");
            result = result + MakeMoney("CashBackDiv", "CashBackAmt", "NewSaleData", "CashBackAmtBlur()");

            //Uncalculated
            result = result + MakeLabel("", "ActDiv", "lblReference", "Reference/CheckNumber");
            result = result + MakeLabel("", "ActDiv", "lblInvoice", "Invoice/Receipt");
            result = result + MakeLabel("", "ActDiv", "lblPayMeth", "Payment Method");
            result = result + MakeCheckbox("", "", "ActDiv", "ReceiveItems", "Receive Items With Purchase Save", "Receive_Items_With_Purchase_Save", "", false);
            result = result + @" $(ActDiv).append('<br/>'); ";

            result = result + MakeDateField("ActDiv", "PDate", "Purchase Date", "", "");
            result = result + MakeInputBox("ActDiv", "PRef", "", "Reference_ChkNumber", "Reference/CheckNumber", "");
            result = result + MakeInputBox("ActDiv", "PInv", "", "Invoice_Receipt", "Invoice/Receipt", "");
            result = result + MakeAutoDDL("ActDiv", "PaymentMethod", "", "PAYMETH", "");

            result = result + @" $(ActDiv).append('<br/>'); ";

            result = result + MakeLabel("", "ActDiv", "lblDocPath", "Document Path");

            result = result + @" $(ActDiv).append('<br/>'); ";

            result = result + MakeInputBox("ActDiv", "PDocPath", "", "DocumentPath", "Document Path", "");

            //Account Selections
            result = result + makeGroupedddlInputPair("ActDiv", "PurchAcctList", "PurchaseAccount", "Purchase Account", "", "Relative", "ACCOUNTS", "1-2-3-12", "");
            result = result + makeGroupedddlInputPair("ActDiv", "VenderList", "VenderAccount", "Vender Account", "", "Relative", "ACCOUNTS", "6", "VenderListChange()");

            //Item Stuff
            result = result + makeGroupedddlInputPair("ActDiv", "ItemList", "ItemList", "Item", "Item", "Relative", "ITEMS", "", " ItemChange(); ");
            result = result + MakeLabel("", "ActDiv", "lblQuantity", "PQ");
            result = result + MakeLabel("", "ActDiv", "lblPAMT", "PAMT");
            result = result + @" $(ActDiv).append('<br/>'); ";
            result = result + MakeNumberBox("ActDiv", "PQ", "ItemQuantity", "PQ", "1", "Relative", 1);
            result = result + MakeMoney("ActDiv", "PAMT", "NewSaleData", "");


            result = result + MakeButton2("btnAddItem", "ActDiv", "", "AddItem", " AddItem();");
            result = result + MakeButton2("btnNewItem", "ActDiv", "", "NewItem", " NewItem() ");


            result = result + MakeNewItemElements("PitemDiv");
            //Purchase List
            result = result + MakePurchaseTable();

            //Shipping
            result = result + MakeLabel("", "ActDiv", "lblShipExpense", "Shipping(Expense)");
            result = result + MakeLabel("", "ActDiv", "lblShipCOG", "Shipping(COG)");
            result = result + @" $(ActDiv).append('<br/>'); ";
            result = result + MakeMoney("ActDiv", "ShipExpense", "noclass", "ShippingExpBlur()");
            result = result + MakeMoney("ActDiv", "ShipCOG", "noclass", "ShippingCOGBlur()");

            //Tax Rate
            result = result + makeGroupedddlInputPair("ActDiv", "TaxRateList", "TaxRateList", "Tax Rates", "", "Relative", "TAXRATES", "", " TaxChange(); ");


            result = result + @" $(ActDiv).append('<br/>'); ";



            //Calculated Fields
            result = result + MakeLabel("lblPTotals", "TotalsDiv", "lblTotalExemptPurchase", "TotalExemptPurchase");
            result = result + MakeInputBox("TotalsDiv", "TotalExemptPurchase", "PTotals", "TotalExemptPurchase", "a", "$0", true);
            result = result + MakeLabel("lblPTotals", "TotalsDiv", "lblTotalTaxablePurchase", "TotalTaxablePurchase");
            result = result + MakeInputBox("TotalsDiv", "TotalTaxablePurchase", "PTotals", "TotalTaxablePurchase", "a", "$0", true);
            result = result + MakeLabel("lblPTotals", "TotalsDiv", "lblPTax", "PTax");
            result = result + MakeMoney("TotalsDiv", "PTax", "PTotals", "PTaxBlur()");//Tax", "a", "$0");
            result = result + MakeLabel("lblPTotals", "TotalsDiv", "lblPurchaseTotal", "PurchaseTotal");
            result = result + MakeInputBox("TotalsDiv", "PurchaseTotal", "PTotals", "PurchaseTotal", "a", "$0", true);


            //  result = result + MakeGroupedInputBox("BottomDiv", "Tax", "Tax", "Tax", "", "Relative");


            //   result = result + MakeGroupedInputBox("BottomDiv", "PurchaseTotal", "PurchaseTotal", "Total Purchase", "", "Relative");
            result = result + MakeButton2("btnRecordPurchase", "BottomDiv", "", "SavePurchase", " SavePurchase();");
            #endregion
            return result;
        }
        public string MakePurchaseVariables()
        {
            return @" $(TopDiv).empty(); $(ActDiv).empty(); $(TotalsDiv).show(); var lineitems=''; var FEE=false; var itemcount=0;
                                var itemarray=new Array(); $(FeeDiv).hide(); $(PitemDiv).hide(); $(PitemDiv).empty();
                                $(BottomDiv).empty(); var CASHBACK=false; $(CashBackDiv).hide(); var Total=0; var subtotal=0; var TaxTotal=0; var Taxable=0; var NonTaxable=0; var Vitems=new Array(); var SelectedVenderID=0; //used to cause item add change to trigger venderlist change if SelectedVender=0, meaning that the initial vender has not changed so the vender item data that loads on the change event has not been loaded.

var serialized=false; var snlist=new Array(); var presnlist=new Array(); var SaveAsThis=1; 
//used with Blur Functions
var shipexp=0; var shipcog=0; ";

        }

        public string MakePurchaseTable()
        {
            string result = "";
            List<string> headers = new List<string> { "Remove","Quantity", "ItemNumber", "Name", "Amount", "SubTotal","Taxable","ExpenseLine","Expense Overide" };
            result = result + MakeEmptyTable("ActDiv", "tblPurchase", "", "", "", headers);
            return result;
        }
        public string MakeSaleTable()
        {
            string result = "";
            List<string> headers = new List<string> { "Remove", "Quantity", "ItemNumber", "Name", "Amount", "SubTotal", "Taxable"};
            result = result + MakeEmptyTable("BottomDiv", "tblSale", "", "", "", headers);
            return result;
        }
        public string MakeAddTaxLoc()
        {
            string result = @" $(ActDiv).empty(); ";
            string div = "ActDiv";
            result = result + MakeButton2("btnBack", "TopDiv", "SpecialButton", "BACK", @" $.post('/Accounting/StartAccounting'); ");
            result = result + MakeGroupedInputBox(div, "Loc", "LocationName", "Location Name", "", "Relative");
            result = result + MakeGroupedInputBox(div, "Code", "LocationCode", "Location Code", "", "Relative");
            result = result + MakeGroupedInputBox(div, "Rate", "LocationRate", "Location Rate", "", "Relative");
            result = result + MakeButton2("btnSaveTaxLoc", div, "", "SAVE", @" $.post('/Accounting/SaveTaxLoc',{TaxLoc:$(Loc).val(), TaxCode:$(Code).val(),TaxRate:$(Rate).val()}); ");
            
            return result;
        }
        public string JVenderListChange()//requires var SelectedVender=0 to already exist at the page
        {
            string result = @"
                                function VenderListChange()
                                {
                                    SelectedVenderID=$(ddlVenderList).val();
                                    $.post('/Common/GetVenderItems',{VendID:SelectedVenderID});
                                    
                                }


";
            return result;
        }
        public string MakeAccountingMaintanance()
        {
            string result = " $(ActDiv).empty(); ";
            string div = "ActDiv";

            result = result + MakeGroupedInputBox(div, "DocID", "DocumentID", "DocumentID", "", "Relative");
            result = result + MakeButton("btnDelDoc", div, @" $.post('/Accounting/DelDocument',{DocID:$(DocID).val()}); ","DELETE");

            return result;
        }
        public string MakeOutStandingDocumentList(int AcctID)
        {
            string result = @" $(DivQuickAdd).append('No Documents apply to selected account'); ";
            int AcctType = M.GetAccountTypeIDFromAccountID(AcctID);
            List<Document> OD = M.getOutstandingInvoices(AcctID);
            if (OD.Count > 0)
            {
                result = @" $(DivQuickAdd).empty(); "+JApplyPayment();
                result = result+@" $(DivQuickAdd).append('<table id=tblRecDocs>";
                result = result + @"<tr><th>DATE</th><th>REFERENCE</th><th>AMOUNT</th></tr>";
                string rows = "";
                int count = 0;
                foreach (Document D in OD)
                {

                    rows = rows + @"<tr id=tblRecDocsRow"+count+@" onclick=ApplyPayment(tblRecDocsRow" + count + @")><td>" + D.DocumentTime + @"K</td><td>" + D.DocumentReference + @"</td><td> $" + D.Amount + @"</td></tr>";
                    count++;
                }
                result = result + rows + @"</table>');";
                result = result + MakeInputBox("DivQuickAdd", "InvAmount", "", "InvoicedAmount", "InvoicedAmount", "");
            }
            return result;
        }
        public string MakeInvoice(int DocID)
        {
            Classes.TransDocument T = M.getTransDoc(DocID);
            AccountTable CoAcct = M.getCompanyAcct();
            Contact COInfo = M.getPrimaryContact(CoAcct.AcctID);

            //Functions
            //Startup Functions



            //Called Functions
            string result = @" function printinvoice()
                                    {
                                        //$(btnprint).hide();
                                        //$(btnemail).hide();
                                        window.print();
                                    }

                               function emailinvoice()
                                    {
                                        $.post('/Common/AddContactList',{AcctID:" + T.CustVendAcctID + @", Element:'divFooter'});
                                    }
                                
                               function contactselected()
                                    {
                                        $.post('/Accounting/EmailInvoice',{DocID:" + DocID + @",ContactID:$(ddlcontacts).val()});
                                    }
";



            //Company Info
            result = result + @" $(divLogo).append('<img src=/Content/Images/Logo3WithText.png>'); ";

            result = result + @" $(divCoInfo).addClass('InvCoInfo'); ";
            result = result + @" $(divCoInfo).append('<label>" + CoAcct.AcctName + "</label><label>" + COInfo.Address + "</label><label>" + COInfo.City + "," + COInfo.State + @" &#160 " + COInfo.PostalCode + "</label><label>" + COInfo.Phone + "</label><label>" + COInfo.Email + "</label>'); ";

            //Customer Info
            result = result + @" $(divCustInfo).addClass('InvCustInfo'); ";
            result = result + @" $(divCustInfo).append('<label>" + T.CustVendName + "</label><label>" + T.Address + "</label><label>" + T.City + @"," + T.State + " &#160 " + T.Zip + "</label>'); ";

            //Header stuff
            result = result + @" $(headerone).addClass('Invheaderone'); ";
            result = result + @" $(headerone).append('<label>INVOICE:" + T.Reference + @"</label><label>" + T.Date + @"</label><label>" + T.PayMeth + @"</label> '); ";

            //Items
            result = result + @" $(divLineItems).addClass('InvLineItems'); ";
            result = result + @" $(divLineItems).append('<table id=ItemTable><tr><th>Quantity</th><th>ItemID</th><th>Description</th><th>Price</th><th>Subtotal</th><th>Taxable</th><tr></table>'); ";

            decimal Tax = T.Tax;
            decimal SubTotal = 0;
            foreach (Classes.LineItem I in T.items)
            {
                decimal ST = I.Quantity * I.Price;
                SubTotal = SubTotal + ST;
                
                if (I.JobLineID > 0)//There is a log associated with item, enclose line in log info
                {
                    JobLine JL = M.getJobLineFromID(I.JobLineID);
                    result = result + @" $(ItemTable).append(' <tr class=InvJobLogTimeRow><td colspan=6>" + JL.TimeIn + @"-" + JL.TimeOut + @"</td></tr> '); ";
                    result = result + @" $(ItemTable).append(' <tr><td>" + Math.Round(I.Quantity, 2) + @"</td><td>" + I.ItemID + @"</td><td>" + I.ItemName + @"</td><td>$" + Math.Round(I.Price, 2) + @"</td><td>$" + Math.Round(ST, 2) + @"</td><td>" + I.Taxable + @"</td></tr> '); ";
                    result = result + @" $(ItemTable).append(' <tr class=InvJobLogRow><td colspan=6>" + JL.Description + @"</td></tr> '); ";
                }
                else //not labor
                {
                    result = result + @" $(ItemTable).append(' <tr><td>" + Math.Round(I.Quantity, 2) + @"</td><td>" + I.ItemID + @"</td><td>" + I.ItemName + @"</td><td>$" + Math.Round(I.Price, 2) + @"</td><td>$" + Math.Round(ST, 2) + @"</td><td>" + I.Taxable + @"</td></tr> '); ";
                    try
                    {
                        //If there is an item note, display it otherwise just fail through
                        
                            result = result + @" $(ItemTable).append(' <tr class=InvJobLogRow><td colspan=6>" + I.Description + @"</td></tr> '); ";
                       
                    }
                    catch
                    {
                        //Do nothing row stays as is
                    }
                }
            }
            //Totals
            decimal Total = T.Total;
            result = result + @" $(divTotals).addClass('InvTotals'); ";
            result = result + @" $(divTotals).append('<label id=ItemTableST>SUBTOTAL: $" + Math.Round(SubTotal, 2) + @"</label><label id=ItemTableTax>TAX: $" + Math.Round(Tax, 2) + @"</label><label id=ItemTableTotal>TOTAL: $" + Math.Round(Total, 2) + @"</label>'); ";

            //Footer Stuff
            result = result + @" $(divFooter).addClass('InvFooter'); ";
            result = result + @" $(divFooter).append('<button type=button id=btnprint onclick=printinvoice()>PRINT</button>'); ";
            result = result + @" $(divFooter).append('<button type=button id=btnemail onclick=emailinvoice()>EMAIL</button>'); ";
            return result;
        }
        public string MakeReceipt(int DocID)
        {
            string result = @"<div> 'Receipt'</div>";


            return result;
        }
        public string MakeSCDeduction(int DocID)
        {
            Classes.TransDocument T = M.getSCDeductionDoc(DocID);
            AccountTable CoAcct = M.getCompanyAcct();
            Contact COInfo = M.getPrimaryContact(CoAcct.AcctID);

            //Functions
            //Startup Functions



            //Called Functions
            string result = @" function printinvoice()
                                    {
                                        $(btnprint).hide(); $(btnemail).hide();
                                        window.print();
                                    }

                               function emailinvoice()
                                    {
                                        $.post('/Common/AddContactList',{AcctID:" + T.CustVendAcctID + @", Element:'divFooter'});
                                    }
                                
                               function contactselected()
                                    {
                                        $.post('/Accounting/EmailInvoice',{DocID:" + DocID + @",ContactID:$(ddlcontacts).val()});
                                    }
";



            //Company Info
            result = result + @" $(divLogo).append('<img src=/Content/Images/Logo3WithText.png>'); ";


            result = result + @" $(divCoInfo).append('<label>" + CoAcct.AcctName + "</label><label>" + COInfo.Address + "</label><label>" + COInfo.City + "," + COInfo.State + @" &#160 " + COInfo.PostalCode + "</label><label>" + COInfo.Phone + "</label><label>" + COInfo.Email + "</label>'); ";

            //Customer Info

            result = result + @" $(divCustInfo).append('<label>" + T.CustVendName + "</label><label>" + T.Address + "</label><label>" + T.City + @"," + T.State + " &#160 " + T.Zip + "</label>'); ";

            //Header stuff
            result = result + @" $(headerone).append('<label>SC Deduction:" + T.Reference + @"</label><label>" + T.Date + @"</label> '); ";

            //Items
            result = result + @" $(divLineItems).append('<table id=ItemTable><tr><th>Hours</th><th>Job#</th><th>Labor</th><th>Description</th><th>Plan</th><th>Rate</th><th>Credits</th><tr></table>'); ";

            
            decimal SubTotal = 0;
            List<string> SCNames = new List<string>();
            List<decimal> SCBalance = new List<decimal>();
            List<decimal> SCUsed = new List<decimal>();
            foreach (Classes.LineItem I in T.items)
            {
                decimal ST = I.Quantity * I.Price;
                SubTotal = SubTotal + ST;
                //Fix Apostophes
                if (I.Description.Contains("'")) I.Description = M.fixapost(I.Description);
                result = result + @" $(ItemTable).append(' <tr><td>" + Math.Round(I.Quantity, 2) + @"</td><td>"+I.JobID+@"</td><td>" + I.ItemName + @"</td><td>" + I.Description + @"</td><td>" + I.ServiceCreditTypeName + @"</td><td>" + Math.Round(I.Price, 2) + @"</td><td>" + Math.Round(ST, 2) + @"</td><tr> '); ";
                if (!(SCNames.Contains(I.ServiceCreditTypeName)))
                {
                    SCNames.Add(I.ServiceCreditTypeName);
                    SCBalance.Add(I.Balance);
                }
            }
            //Totals
            decimal Total = SubTotal;
            result = result + @" $(divTotals).append('<label id=ItemTableTotal>TOTAL SERVICE CREDITS USED: " + Math.Round(Total, 2) + @"</label>'); ";
            int count = 0;
            foreach (string scn in SCNames)
            {
                result=result + @" $(divTotals).append('<label id=lbl"+scn+@"bal>"+scn+@" Balance: " + Math.Round(SCBalance[count], 2) + @"</label>'); ";
            }
            //Footer Stuff
            result = result + @" $(divFooter).append('<button type=button id=btnprint onclick=printinvoice()>PRINT</button>'); ";
            result = result + @" $(divFooter).append('<button type=button id=btnemail onclick=emailinvoice()>EMAIL</button>'); ";
            return result;
        }
        public string MakeSCStatement(int AcctID, string sd, string ed, int STID)
        {
            List<Classes.Prepaid> PP = M.getPrepaidStatementData(AcctID, sd, ed, STID);
            AccountTable CoAcct = M.getCompanyAcct();
            Contact COInfo = M.getPrimaryContact(CoAcct.AcctID);
            Contact C = M.getPrimaryContact(AcctID);
            string CustomerName = M.getAccountFromID(AcctID).AcctName;
            //Called Functions
            string result = @" function printinvoice()
                                    {
                                        
                                        window.print();
                                    }

                               function emailinvoice()
                                    {
                                        $.post('/Common/AddContactList',{AcctID:" + AcctID + @", Element:'divFooter'});
                                    }
                                
                               function contactselected()
                                    {
                                        $.post('/Accounting/EmailInvoice',{DocID:" + 0 + @",ContactID:$(ddlcontacts).val()});
                                    }
";

            //Company Info
            result = result + @" $(divLogo).append('<img src=/Content/Images/Logo3WithText.png>'); ";
            result = result + @" $(divLogo).append('<label class=coinfo>" + CoAcct.AcctName + "</label>'); ";

            result = result + @" $(headerone).append('<label class=coinfo>" + COInfo.Address + "</label><label class=coinfo>" + COInfo.City + "," + COInfo.State + @" &#160 " + COInfo.PostalCode + "</label><label class=coinfo>" + COInfo.Phone + "</label><label class=coinfo>" + COInfo.Email + "</label>'); ";

            //Customer Info

            result = result + @" $(divCustInfo).append('<label>" + CustomerName + "</label><label>" + C.Address + "</label><label>" + C.City + @"," + C.State + " &#160 " + C.PostalCode + "</label>'); ";

            //Header stuff
            result = result + @" $(headerone).append('<label>" + DateTime.Now.ToLongDateString() + @"</label><label>SC Statement:" + PP[0].LineDate.ToShortDateString() + @" - " + PP[PP.Count - 1].LineDate.ToShortDateString() + @"</label> '); ";

            //Items
            result = result + @" $(divLineItems).append('<table id=ItemTable><tr><th>Date</th><th>Description</th><th>Credits</th><th>Balance</th><tr></table>'); ";

            
            
            string SCTypeName = "";
            int SCTypeID = 0;
            int count = 0;
            foreach (Classes.Prepaid I in PP)
            {
                if (!(SCTypeID == I.ServiceTypeID))
                {
                    SCTypeID = I.ServiceTypeID; SCTypeName = M.getServiceTypeNameFromID(I.ServiceTypeID);
                }
                //Fix Apostophes
                if (I.Description.Contains("'")) I.Description = M.fixapost(I.Description);
                if (I.Amount < 0)//is deduction from job get job line info
                {
                    
                    List<JobLine> JobLines = M.getJobLinesFromJobID(Convert.ToInt32(I.JobID));
                    result = result + @" $(ItemTable).append(' <tr><td>" + I.LineDate.ToShortDateString() + @"</td><td>" + I.Description + @"</td><td>" + Math.Round(I.Amount, 2) + @"</td><td>" + Math.Round(I.Balance, 2) + @"</td></tr><tr><td></td><td><table><tr><th>Service</th><th>Hours</th><th>Rate</th><th>CreditsUsed</th></tr>";

                    foreach (JobLine JL in JobLines)
                    {
                        //Fix Apostophes
                        if (JL.Description.Contains("'")) JL.Description = M.fixapost(JL.Description);
                        
                        result = result + @"<tr><td>"+M.getItemFromID(Convert.ToInt32(JL.ServiceItemID)).ItemName+@"</td><td> "+JL.TotalTime+@"</td><td>"+Math.Round(JL.HourRate,2)+@"</td><td>"+Math.Round(JL.HourRate*JL.TotalTime,2)+@"</td></tr><tr><td>"+JL.Description+@"</td><tr>";
                    }


                        result=result+@"</table></td><td></td><td></td><tr> '); ";
                }
                else
                {//is an invoice
                    result = result + @" $(ItemTable).append(' <tr><td>" + I.LineDate.ToShortDateString() + @"</td><td>" + I.Description + @"</td><td>" + Math.Round(I.Amount, 2) + @"</td><td>" + Math.Round(I.Balance, 2) + @"</td><tr> '); ";
                }
                
                count++;
            }

            //Footer Stuff
            result = result + @" $(divFooter).append('<button type=button id=btnprint onclick=printinvoice()>PRINT</button>'); ";
            result = result + @" $(divFooter).append('<button type=button id=btnemail onclick=emailinvoice()>EMAIL</button>'); ";
            
            return result;
        }
        #endregion

        #region JOBS ************************************************************************************
        public string MakeJobsMain()
        {
            string result = JJobChanged() + JSCChanged() + JServiceCreditAcctsChanged();
            result = result+makeGroupedddlInputPair("TopDiv", "AllJobsList", "AllJobs", "AllJobs", "", "Relative", "JOB", "", "JobChanged($(ddlAllJobsList).val())");
            result = result + makeGroupedddlInputPair("TopDiv", "PendingJobsList", "PendingJobs", "PendingJobs", "", "Relative", "JOB", "P", "JobChanged($(ddlPendingJobsList).val())");//P for pending
            result = result + makeGroupedddlInputPair("TopDiv", "ServiceCreditAccts", "ServiceCreditAccts", "ServiceCreditAccts", "", "Relative", "ACCOUNTS", "SC", "ServiceCreditAcctsChanged()");
            result = result + MakeAutoDDL("TopDiv", "SCDeductions", "noclass", "SCDEDUCTIONS", "SCChanged()");//change to GroupedDDLInputPair
            result = result + MakeButton2("btnNewJob", "TopDiv", "", "NewJob", "window.open('/Jobs/OneJob/?JOBID=0'); ");
            return result;
        }
        
        public string MakeSingleJob(int JobID)
        {
            string result = @" var LogID=0; ";

            string RDate = "";
            string DDate = "";
            string CDate = "";
            bool chk = false;
            string Cust = "";
            string description = "";
           
            int OrderedByID = 0;
            
            result = result + JSaveJob() + JJobCustomerSelected() + JCancelJobSave() + JRequestDateChange() + JDueDateChange() + JNewJobLog() + JSaveJobLog();
            if (JobID > 0)//Existing Job
            {
                Job J = M.getJobFromID(JobID);
                RDate = MakeHTMLDate(J.EnterDate);
                DDate = MakeHTMLDate(J.DueDate);
                try
                {
                    CDate = MakeHTMLDate(Convert.ToDateTime(J.CompleteDate));
                }
                catch
                {
                    //Leave = to "";
                }
                chk = J.Completed;
                Cust = M.getAccountFromID(J.CustID).AcctName;
                result = result + MakeLabel("", "TopDiv", "lblJobID", JobID.ToString());
                description = J.JobDescription;

                OrderedByID = M.getContactFromID(J.OrderedBy).ContactID;
                result = result + MakeJobLogs("MidDiv",JobID);
                
            }
            else//new Job
            {
                

            }
            result = result + MakeLabel("", "TopDiv", "lblReqDate", "RequestDate");
            result = result + MakeDateField("TopDiv", "ReqDate", "RequestDate", "RequestDateChange()", RDate);
            result = result + MakeLabel("", "TopDiv", "lblDueDate", "DueDate");
            result = result + MakeDateField("TopDiv", "DueDate", "DueDate","DueDateChange()",DDate);
            result = result + MakeLabel("", "TopDiv", "lblDueDate", "CompletionDate");
            if (CDate.Length > 0)
            {
                result = result + MakeDateField("TopDiv", "CompletionDate", "CompletionDate", "DueDateChange()", CDate);
            }
            else
            {
                result = result + MakeDateField("TopDiv", "CompletionDate", "CompletionDate", "DueDateChange()", "");
            }
            result = result + MakeCheckbox("None", "None", "TopDiv", "chkComplete", "Complete", "Complete", "", chk);
            result = result + makeGroupedddlInputPair("TopDiv", "CustomerAcct", "Customer", "Customer", Cust, "Relative", "ACCOUNTS", "15", " JobCustomerSelected() ");
            result = result + MakeTextArea("TopDiv","*","Description",6,50,description);
           // result = result + MakeGroupedInputBox("TopDiv", "Description", "Description", "Description", description, "Relative");
            result = result + MakeButton2("btnSaveJob", "TopDiv", "", @"SAVE JOB", " SaveJob(" + JobID + @") ");
            result = result + MakeButton2("btnCancelJob", "TopDiv", "", @"Cancel", " CancelJobSave() ");
           // result = result + @" $(MidDiv).append('<table id=tblJobLogs></table>'); ";
           // result = result + @" $(tblJobLogs).append('<tr id=tblJobLogsHeader><th>LogID</th><th>StartTime</th><th>EndTime</th><th>TotalTime</th><th>Description</th>'); ";

            if (JobID > 0)
            {
                result = result + @"

                                $(ContactDiv).empty();
                                
                                $.post('/Accounting/ShowContactList',{AcctID:$(ddlCustomerAcct).val(),ElementID:'ContactDiv',ContactID:"+OrderedByID+@"});
";
            }

            return result;
        }
        #endregion
        
        public string MakeContactList(int AcctID, string ElementID,int ContactID)
        {
            string result = @" ";
            
                result=result+makeGroupedddlInputPair(ElementID, "Contacts", "Contacts", "Contacts", "", "Relative", "CONTACTS", AcctID.ToString(), "");
            result=result+MakeButton2("btnNewContact",ElementID,"","NewContact"," $.post('/Accounting/ShowContact/',{ContactID:"+ContactID+@",AcctID:"+AcctID+@"}) ");// window.open('/Accounting/OpenContact/?ContactID=0') ");
            return result;
        }
        
        public string MakeContact(string Element, int ContactID,int AcctID)//ContactDiv must exist on requesting page
        {
            string result = "";
            result = result + JSaveContact()+JCancelSaveContact();
            
            if (ContactID > 0)//Existing Contact
            {
                result = result + MakeLabel("", Element, "lblContactID", ContactID.ToString());
            }
            else//new Contact
            {

                
            }
            result = result + makeddlInputPair(Element, "ContactType", "ContactType", "Contact Type", "", "CONTACTTYPE", "", "", "");
            result = result + MakeGroupedInputBox(Element, "tbFirstName", "FirstName", "First Name", "", "Relative");
            result = result + MakeGroupedInputBox(Element, "tbLastName", "LastName", "Last Name", "", "Relative");
            result = result + MakeGroupedInputBox(Element, "tbAddress", "Address", "Address", "", "Relative");
            result = result + MakeGroupedInputBox(Element, "tbCity", "City", "City", "", "Relative");
            result = result + MakeGroupedInputBox(Element, "tbState", "State", "State", "", "Relative");
            result = result + MakeGroupedInputBox(Element, "tbCountry", "Country", "Country", "", "Relative");
            result = result + MakeGroupedInputBox(Element, "tbPostalCode", "PostalCode", "PostalCode", "", "Relative");
            result = result + MakeGroupedInputBox(Element, "tbPhone", "Phone", "Phone", "", "Relative");
            result = result + MakeGroupedInputBox(Element, "tbEmail", "Email", "Email", "", "Relative");
                result = result + MakeButton2("btnSaveContact", Element, "", "SAVECONTACT", " SaveContact(" + ContactID + @"," + AcctID + @") ");
                result = result + MakeButton2("btnCancelSaveContact", Element, "", "CANCELSAVECONTACT", " CancelSaveContact() ");
            
            return result;
        }
        public string MakeDocument(int DocID)
        {
            string result = "window.open('/Accounting/OpenDoc/?DocID=" + DocID + @"'); ";
            //result = result + MakeButton2("btnBack", "TopDiv", "SpecialButton", "BACK", @" $.post('/Accounting/StartAccounting'); ");
            //result = result + MakeButton2("btnSaveTransChanges", "TopDiv", "", "SaveChanges", @" $.post('/Accounting/SaveTransChanges',{DocID:" + DocID + @",DocPath:$(DocumentPath).val()}); ");
            //result = result + MakeGroupedInputBox("ActDiv", "DocumentPath", "DocumentPath", "Document Path", M.fixstring(M.GetDocumentPathFromID(DocID)), "Relative");

            return result;
        }
        public string BuildDocument(int DocID)
        {
            Document D = M.GetDoc(DocID);
            List<TransactionTable> Transactions = M.getTransactionsFromDocID(DocID);
            decimal Ptax = 0; decimal CashBack = 0; decimal TransFee = 0;
            string result = @"";
            result = result + MakeLabel("titlelabel", "TopDiv", "DocID", DocID.ToString() + "&nbsp;&nbsp;&nbsp;&nbsp;");
            result = result + MakeLabel("titlelabel", "TopDiv", "DocType", M.GetDocTypeFromTypeID(D.DocumentTypeID) + "<BR/>");
            result = result + MakeLabel("titlelabel", "TopDiv", "DocTime", D.DocumentTime.ToString() + "&nbsp;&nbsp;&nbsp;&nbsp;");
            result = result + MakeLabel("titlelabel", "TopDiv", "DocRef", D.DocumentReference + "<BR/>");
            if (D.PaymentMethodID > 0) //added if because deposit doc fails having a D.PaymentMethodID of 0
            {
                result = result + MakeLabel("titlelabel", "TopDiv", "DocTransType", M.getTransactionTypeFromID(D.PaymentMethodID) + "&nbsp;&nbsp;&nbsp;&nbsp;");
            }
            result = result + MakeLabel("titlelabel", "TopDiv", "DocAmt", "$" + Math.Round(D.Amount, 2).ToString() + "<BR/>");
            
            
            result = result + MakeGroupedInputBox("TopDiv", "DocPath", "DocumentPath", "Document Path", M.fixslash(D.DocumentPath), "Relative");
            result = result + MakeButton2("btnSaveDocPath", "TopDiv", "", "SaveDocumentPath", @" $.post('/Accounting/SaveDocPath',{DocID:"+DocID+@",DocPath:$(DocPath).val()}); ");


            result = result + MakeLabel("titlelabel", "TopDiv", "DocRecLabel", "<BR/>Reconciled? ");
            result = result + MakeLabel("titlelabel", "TopDiv", "DocRec", D.Reconciled.ToString() + "<BR/><BR/>");
            result = result + MakeButton2("btnShowLedger", "TopDiv", "MainButtons", "ShowLedger", @" $(MidDiv).show(); $(btnHideLedger).show(); $(btnShowLedger).hide(); ");
            result = result + MakeButton2("btnHideLedger", "TopDiv", "MainButtons", "HideLedger", @" $(MidDiv).hide(); $(btnHideLedger).hide(); $(btnShowLedger).show(); ");
            int TransCount = 0;
            foreach (TransactionTable T in Transactions)
            {
                if (T.TransactionTypeID == 20) Ptax = T.Amount;
                if (T.TransactionTypeID == 21) TransFee = T.Amount;
                if (T.TransactionTypeID == 22) CashBack = T.Amount;
                result = result + MakeLabel("titlelabel", "MidDiv", "DocTransType" + TransCount, M.getTransactionTypeFromID(T.TransactionTypeID) + "&nbsp;&nbsp;&nbsp;&nbsp;");
                result = result + MakeLabel("titlelabel", "MidDiv", "DocTransDescript" + TransCount, T.Description + "<BR/>");
                result = result + MakeLabel("titlelabel", "MidDiv", "DocTransCreditAcct" + TransCount, M.getAccountFromID(T.CreditID).AcctName + "&nbsp;&nbsp;&nbsp;&nbsp;");
                result = result + MakeLabel("titlelabel", "MidDiv", "DocTransAmt" + TransCount, "$" + Math.Round(T.Amount, 2).ToString() + "&nbsp;&nbsp;&nbsp;&nbsp;");
                result = result + MakeLabel("titlelabel", "MidDiv", "DocTransDebitAcct" + TransCount, M.getAccountFromID(T.DebitID).AcctName + "&nbsp;&nbsp;&nbsp;&nbsp;");
                result = result + @" $(btnHideLedger).hide(); $(btnShowLedger).hide(); ";
                //  result = result + "$(TopDiv).append('</BR></BR>')";

            }
            switch (D.DocumentTypeID)
            {
                case 1://Funds Transfer
                    break;
                case 2://Deposit
                    List<string> dephead=new List<string>{"From","Payment Method","Reference","Amount"};
                    result = result + MakeEmptyTable("BottomDiv", "deplinestbl", "", "", "", dephead);
                        List<Classes.DepLine> dlines = M.getDepositLinesFromDocID(DocID);
                        foreach (Classes.DepLine DL in dlines)
                        {
                            DepositLine dl=M.getDepositLineFromDepLineID(DL.DepLineID);
                            string dlineref=M.getTransaction(dl.TransID).Description;
                            result = result + @" $(deplinestblbody).append('<tr><td>"+DL.PayerName+@"</td><td>"+DL.TransType+@"</td><td>"+dlineref+@"</td><td>" + Math.Round(DL.AMT,2) + @"</td></tr>'); ";
                        }

                    break;
                case 3://PaymentReceived
                    break;
                case 4://Purchase
                    result = result + MakeLabel("purchasetblclass", "TopDiv", "lblVendor", M.getPayableNameFromPurchaseDocument(DocID));
                    
                    List<string> Purchhead=new List<string>{"Quantity","Item","Price","SubTotal"};
                    result = result + MakeEmptyTable("BottomDiv", "plinestbl", "purchasetblclass","purchasecellclass", "purchaserowclass", Purchhead);
                    List<TransLineItem> plines = M.getTransLinesByDocID(DocID);
                    foreach (TransLineItem LI in plines)
                    {
                        string ItemName = M.GetItemNameFromID(LI.ItemID);
                        result = result + @" $(plinestblbody).append('<tr><td>" + LI.Quantity + @"</td><td>" + ItemName + @"</td><td>$" + Math.Round(LI.Price,2) + @"</td><td>$" + Math.Round(LI.Price * LI.Quantity, 2) + @"</td></tr>'); ";
                    }
                    if(Ptax>0)
                    result = result + @" $(plinestblbody).append('<tr><td>*</td><td>TAX</td><td>-</td><td>$" + Math.Round(Ptax, 2) + @"</td></tr>'); ";
                    if (TransFee > 0) result = result + @" $(plinestblbody).append('<tr><td>*</td><td>Transaction Fee</td><td>-</td><td>$" + Math.Round(TransFee, 2) + @"</td></tr>'); ";
                    if (CashBack > 0) result = result + @" $(plinestblbody).append('<tr><td>*</td><td>Cash Back</td><td>-</td><td>$" + Math.Round(CashBack, 2) + @"</td></tr>'); ";
                    result = result + @" $(btnShowLedger).show(); $(MidDiv).hide(); ";
                    break;
                case 5://PurchaseOrder
                    break;
                case 6://Quote
                    break;
                case 7://Invoice
                    break;
                case 8://PaymentMade
                    break;
                case 9://Bill
                    break;
                default:
                    break;
            }

            return result;
        }
        public string MakeJobLogs(string element,int JobID)
        {
            string result = @"";
            result=result+JNextLog() + JPrevLog() + JCheckIfLogInvoiced();
            List<JobLine> JLs = M.getJobLinesFromJobID(JobID);
            string LDate = ""; string SDT = ""; string EDT = ""; string TotalTime = ""; string Employee = ""; string ServiceType = ""; string Description = ""; string LogID = "0";
            if (JLs.Count > 0)
            {
                result = result + @" var currentlog=0; var logcount=" + JLs.Count+@"; ";
                result=result+MakeJobLogsArrayObjects(JLs, "JL");
                LDate = MakeHTMLDateTime(JLs[0].EnterTime);
                SDT = MakeHTMLDateTime(JLs[0].TimeIn);
                EDT = MakeHTMLDateTime(JLs[0].TimeOut);
                TotalTime = JLs[0].TotalTime.ToString();
                Employee = M.getAccountFromID(JLs[0].EmployeeID).AcctName;
                ServiceType = M.GetItemNameFromID(JLs[0].ServiceItemID);
                Description = JLs[0].Description;
                LogID = JLs[0].JobLineID.ToString();
            }
            result = result + @" var LogID="+LogID+@"; ";
            result = result + MakeLabel("*", element, "lblLogID", LogID);
            result = result + MakeDateTimeLocalField(element, "LogDate", "LogDate", "LogDateChange()", LDate, 60);
            result = result + MakeDateTimeLocalField(element, "StartTime", "StartTime", "", SDT, 900);
            result = result + MakeDateTimeLocalField(element, "EndTime", "EndTime", "", EDT, 900);
            result = result + MakeNumberBox(element, "TotalTime", "TotalTime", "lblTotalTime", TotalTime, "relative", .25);
            result = result + makeGroupedddlInputPair(element, "Employee", "Employee", "Work Performed By:", Employee, "relative", "ACCOUNTS", "21", "");
            result = result + makeGroupedddlInputPair(element, "ServiceType", "ServiceType", "ServiceType", ServiceType, "relative", "ITEMS", "51", "");
            result = result + @" $(" + element + @").append('<p></p>'); ";
            result = result + MakeTextArea(element, "*", "taLog", 10, 80, M.fixstring(Description));
            if (JLs.Count > 1)
            {
                result = result + MakeButton2("btnPrevLog", element, "*", "PrevLog", "PrevLog()");
                result = result + MakeButton2("btnNextLog", element, "*", "NextLog", "NextLog()");
            }
            result = result + MakeButton2("btnNewLog", element, "*", "NewLog", "NewJobLog()");
            result = result + MakeButton2("btnSaveLog", element, "SaveBtnClass", "SaveLog", "SaveJobLog(" + JobID + @",LogID)");

            return result;
        }
        public string MakeJobLogsArrayObjects(List<JobLine> JL,string ArrayName)//JL[]
        {

            string result = @" "+ArrayName+@"=new Array(); ";
            int count = 0;

            foreach (JobLine jl in JL)
            {

                result = result + ArrayName + @"[" + count + @"]={LogDateTime:'" + MakeHTMLDateTime(jl.EnterTime) + @"',LogID:'" + jl.JobLineID + @"',TimeIn:'" + MakeHTMLDateTime(jl.TimeIn) + @"',TimeOut:'" + MakeHTMLDateTime(jl.TimeOut) + @"',TotalTime:'" + jl.TotalTime + @"',ServiceItemID:'" + jl.ServiceItemID + @"',EmployeeID:'" + jl.EmployeeID + @"',Description:'" + M.fixstring(jl.Description) + @"',Rate:'" + jl.HourRate + @"',JobID:'" + jl.JobID + @"',InDoc:'"+jl.InDocument+@"'};

                

";
                count++;
            }
            
            

            return result;
        }
        public string MakeLaborInvoiceItems(List<JobLine> JL, string Element)
        {
            string result = @" var LaborLine=new Array(); "+JLaborRowSelected();
            result = result + @" $(" + Element + @").append('<table id=tblLaborItems><tr><th>JOBID</th><th>LOGID</th><th>SERVICE</th><th>DESCRIPTION</th><th>HOURS</th></tr>";
            int count = 0;
            foreach (JobLine jl in JL)
            {
                result = result + @"<tr id=LaborRow"+count+@" onclick=LaborRowSelected(LaborRow"+count+@")><td>" + jl.JobID + @"</td><td>" + jl.JobLineID + @"</td><td>"+M.GetItemNameFromID(jl.ServiceItemID) + @"</td><td>" + M.fixstring(jl.Description) + @"</td><td>" + jl.TotalTime + @"</td></tr>";
                count++;
            }
                
                
                
                result=result+@"</table>'); ";


            return result;
        }
        #region The "J" Stuff (javascript functions)


        public string JLaborRowSelected()
        {
            string result = @"
                                function LaborRowSelected(rowid)
                                    {
                                        
                                        var JLineID=$(rowid).find('td:eq(1)').text();
                                        var sitem=$(rowid).find('td:eq(2)').text();
                                        var itemdescription=$(rowid).find('td:eq(3)').text();
                                        var Quan=$(rowid).find('td:eq(4)').text();
                                        //var test=$('#ddlItemList option:selected').text();
                                       // var test2=document.getElementById('ddlItemList');
                                        var ItemID=0;
                                        
                                        $(ddlItemList).children().each(function(index)
                                                            {
                                                                if($(this).text()==sitem)
                                                                    {
                                                                        ItemID=$(this).val();
                                                                    }
                                                            });                                        


                                        if(!(IsInvoice))
                                            {

                                //Assign amount of SCs based on chosen SC plan
                                var count=0;
                                while(count<PrePayValue.length)

                                    {
                                        if(PrePayValue[count].SCTID==SCTypeID && PrePayValue[count].ItemID==ItemID)
                                                {
                                                    SaleAmount=PrePayValue[count].Value;
                                                }
                                        count++;
                                    }

                                                lineitem={LineID:itemcount,Quantity:Number(Quan),Item:ItemID,JobLineID:JLineID,Description:itemdescription,Amount:SaleAmount,Taxable:false,SN:SCTypeID};//Put service credit type in SN field
                                            }


                                        else
                                            {
                                                SaleAmount=$(SAMT).val().substr(1);
                                                lineitem={LineID:itemcount,Quantity:Number(Quan),Item:ItemID,JobLineID:JLineID,Description:itemdescription,Amount:SaleAmount,Taxable:true,SN:0};
                                            }
                                        subtotal=(Number(Quan*SaleAmount)).toFixed(2);
                                
                                var cbid='line'+itemcount;
                                var ddlLE='LE'+itemcount;
                                var Prow='PROW'+itemcount;
                                var btnDelLine='DelL'+itemcount;
                
                                Total=(Number($(TotalTaxableSale).val())+Number(subtotal)).toFixed(2);

                                itemarray[itemcount]=lineitem;
                
               
                                lineitems=lineitems+'{'+Quan+','+ItemID+','+$(SAMT).val()+'}';
                                

                                if(!(IsInvoice))
                                {
                                    $(tblSalebody).append('<tr id='+Prow+'><td><button id=' + btnDelLine +' onclick= DelLine('+Prow+','+itemcount+')>REMOVE</button></td><td>'+Quan+'</td><td>'+ItemID+'</td><td>'+sitem+'-'+itemdescription+'</td><td>'+$(SAMT).val()+'</td><td>'+subtotal+'</td></tr>');
                                    $(Tax).val(0);
                                    $(TotalExemptSale).val(Total);
                                    ServiceCreditsBalance=ServiceCreditsBalance-subtotal;
                                    $(lblCreditsBalanceAmt).text(ServiceCreditsBalance);
                                }
                                else
                                {
                                    $(tblSalebody).append('<tr id='+Prow+'><td><button id=' + btnDelLine +' onclick= DelLine('+Prow+','+itemcount+')>REMOVE</button></td><td>'+Quan+'</td><td>'+ItemID+'</td><td>'+sitem+'-'+itemdescription+'</td><td>'+$(SAMT).val()+'</td><td>'+subtotal+'</td><td><input id='+cbid+' type=checkbox checked=checked onclick=LineClick('+subtotal+','+cbid+'); ></input></td></tr>');
                                    $(Tax).val(($(ddlTaxRateList).val()*Total).toFixed(2));
                                    $(TotalTaxableSale).val(Total);
                                }
                                
                                itemcount++;
                
                                var ptax=Number($(Tax).val()).toFixed(2);
                
                                $(SaleTotal).val((Number(Number(ptax)+Number(Total))).toFixed(2));




                                    }

                               


";
            

            return result;
        }
        public string JAddLaborToInvoice()// Requires $(ddlCust) and <div>ActDiv</div>
        {
            return @"
                        function AddLaborToInvoice()
                            {
                                if($(ddlCust).val()==0)
                                    {
                                        alert('Customer must be selected before adding labor');
                                    }
                                else
                                    {
                                        $.post('/Jobs/GetLaborToInvoice',{CID:$(ddlCust).val(),Element:'ActDiv'});
                                    }
                            }



";
        }
        public string JCheckIfLogInvoiced()// Requires var currentlog from MakeJobLogs()
        {
            return @"
                        function CheckIfLogInvoiced()
                            {
                                var crap=JL[currentlog].InDoc.toString();
                                if(crap=='True')
                                    {
                                        $(StartTime).attr('disabled',true); $(EndTime).attr('disabled',true); $(TotalTime).attr('disabled',true);
                                        
                                    }
                                else
                                    {
                                        $(StartTime).attr('disabled',false); $(EndTime).attr('disabled',false); $(TotalTime).attr('disabled',false);
                                        
                                    }
                                
                            }




";
        }
        public string JNextLog()//requires var currentlog, var logcount, var LogID, array JL and elements from MakeJobLogs()
        {
            return @"
                        function NextLog()
                            {
                               currentlog++;
                                
                                
                                if(currentlog==logcount)
                                    {
                                        currentlog=0;
                                    }
                                LogID=JL[currentlog].LogID;
                                $(lblLogID).text(JL[currentlog].LogID);
                                $(LogDate).val(JL[currentlog].LogDateTime);
                                $(StartTime).val(JL[currentlog].TimeIn);
                                $(EndTime).val(JL[currentlog].TimeOut);
                                $(TotalTime).val(JL[currentlog].TotalTime);
                                $(ddlEmployee).val(JL[currentlog].EmployeeID);
                                $(ddlServiceType).val(JL[currentlog].ServiceItemID);

                                $(taLog).val(JL[currentlog].Description);
                                
                                CheckIfLogInvoiced();
                               
                            }


";
           
        }
        public string JPrevLog()//requires var currentlog, var logcount, var LogID, array JL and elements from MakeJobLogs()
        {
            return @"
                        function PrevLog()
                            {
                                
                                currentlog--;
                                
                                
                                if(currentlog<0)
                                    {
                                        currentlog=logcount-1;
                                    }
                                LogID=JL[currentlog].LogID;
                                $(lblLogID).text(JL[currentlog].LogID);
                                $(LogDate).val(JL[currentlog].LogDateTime);
                                $(StartTime).val(JL[currentlog].TimeIn);
                                $(EndTime).val(JL[currentlog].TimeOut);
                                $(TotalTime).val(JL[currentlog].TotalTime);
                                $(ddlEmployee).val(JL[currentlog].EmployeeID);
                                $(ddlServiceType).val(JL[currentlog].ServiceItemID);

                                $(taLog).val(JL[currentlog].Description);
                                
                                CheckIfLogInvoiced();
                            }


";

        }
        public string JNewJobLog()
        {
            return @"
                        function NewJobLog()
                            {
                                $(lblLogID).text('');
                                $(LogDate).val('');
                                $(StartTime).val('');
                                $(EndTime).val('');
                                $(TotalTime).val('');
                                $(taLog).val('');
                                LogID=0;
                            }

";
        }
        public string JSaveJobLog()//requires $(taLog),$(LogDate),$(StartTime),$(EndTime),$(TotalTime),$(ddlEmployee),$(ddlServiceType)
        {
            return @"

                        function SaveJobLog(JobID,LogID)
                            {
                                
                                if($(taLog).val().length<1)
                                    {
                                        alert('No text in Log');
                                        return;
                                    }
                                if($(TotalTime).val().length<1)
                                    {
                                        alert('Please Enter total time');
                                        return;
                                    }
                                
                                $.post('/Jobs/SaveJobLog',{JobID:JobID,LogID:LogID,LogTime:$(LogDate).val(),StartTime:$(StartTime).val(),EndTime:$(EndTime).val(),TotalTime:$(TotalTime).val(),EmployeeID:$(ddlEmployee).val(),ItemID:$(ddlServiceType).val(),Description:$(taLog).val()});
                            }


";
        }
        
        public string JRequestDateChange()
        {
            return @"
                        function RequestDateChange()
                            {
                                $(tbReqDate).val($(ReqDate).val());
                            }

";
        }
        public string JDueDateChange()
        {
            return @"
                        function DueDateChange()
                            {
                                $(tbDueDate).val($(DueDate).val());
                            }

";
        }
        
        public string JSaveContact()
        {
            return @"
                            function SaveContact(CID,AcctID)
                                {
                                    $.post('/Accounting/SaveContact',{CID:CID, CTID:$(ddlContactType).val(),FName:$(tbFirstName).val(),LName:$(tbLastName).val(),Address:$(tbAddress).val(),City:$(tbCity).val(),State:$(tbState).val(),Country:$(tbCountry).val(),PostalCode:$(tbPostalCode).val(),Phone:$(tbPhone).val(),Email:$(tbEmail).val(),AcctID:AcctID});
                                }

";
        }
        public string JCancelSaveContact()
        {
            return @"
                        function CancelSaveContact()
                            {
                                $(ContactDiv).empty();
                            }


";
        }
        
        public string JSaveJob()
        {
            return @"
                        function SaveJob(JobID)
                            {
                                
                                $.post('/Jobs/SaveJob',{ReqDate:$(ReqDate).val(), Description:$(Description).val(),DueDate:$(DueDate).val(),RequestedBy:$(ddlContacts).val(),CompletionStatus:document.getElementById('chkComplete').checked,AcctID:$(ddlCustomerAcct).val(),JobID:JobID,CompleteDate:$(CompletionDate).val()});
                            }

";
        }
        public string JCancelJobSave()
        {
            return @"
                        function CancelJobSave()
                            {
                                window.close();
                            }



";
        }
        public string JJobCustomerSelected()
        {
            return @"
                      function JobCustomerSelected()
                            {
                                $(ContactDiv).empty();
                                
                                $.post('/Accounting/ShowContactList',{AcctID:$(ddlCustomerAcct).val(),ElementID:'ContactDiv',ContactID:0});
                            }  


";
        }
        public string JJobChanged()
        {
            return @"
                        function JobChanged(JobID)
                            {
                                
                                window.open('/Jobs/OneJob/?JOBID='+JobID); 
                            }

";
        }
        
        public string JSCChanged()
        {
            return @"
                        function SCChanged()
                            {
                                window.open('/Accounting/ShowSCDeduction/?DocID='+$(SCDeductions).val());
                                
                            }


";
        }
        public string JServiceCreditAcctsChanged()
        {
            return @"
                        function ServiceCreditAcctsChanged()
                            {
                                window.open('/Accounting/ShowSCStatement/?ACCTID='+$(ddlServiceCreditAccts).val());
                            }


";
        }
        
        public string JCustomerSelected()
        {
            string result = @"
                                function CustomerSelected()
                                    {
                                        CustID=$(ddlCust).val();
                                        
                                        $(TopDiv).append('<select id=ddlCredits onchange=PrepaySelected()><option value=0>Use Credits</option></select><label id=lblCreditsBalance>Balance:</label><label id=lblCreditsBalanceAmt>0</label>');
                                        var count=0;
                                        
                                        for(var c=0,len=Prepays.length; c<len; c++)
                                            {
                                                if(Prepays[c].AcctID==CustID)
                                                {
                                                    $(ddlCredits).append('<option value='+Prepays[c].PType+'>'+Prepays[c].PName+'-'+Prepays[c].Bal+'</option>');
                                                    count++;
                                                }
                                                
                                            }
                                        if(count<1)
                                                {
                                                    $(ddlCredits).remove();
                                                    $(lblCreditsBalance).remove();
                                                    $(lblCresitsBalanceAmt).remove();
                                                    HasServiceCredits=false;
                                                }
                                        else
                                            {
                                                HasServiceCredits=true;
                                            }
                                    }

";
            

            return result;
        }
        
        public string JPrepaySelected()
        {
            return @"
                        function PrepaySelected()
                            {
                                
                                if((itemarray.length>0)&&(IsInvoice))
                                    {
                                        alert('Service Credit Deductions can not be combined with invoice items');
                                    }
                                else
                                    {
                                        
                                        SCTypeID=$(ddlCredits).val();
                                        
                                        for(var c=0;c<Prepays.length;c++)
                                                {
                                                   
                                                    if(Prepays[c].PType==SCTypeID&&Prepays[c].AcctID==CustID)
                                                        {
                                                            ServiceCreditsBalance=Prepays[c].Bal;
                                                            
                                                        }
                                                }
                                        $(PayMeth).val(6);
                                        IsInvoice=false;
                                        $(lblCreditsBalanceAmt).text(ServiceCreditsBalance);
                                    }
                            }

";
        }
        
        public string JSetDateRange()
        {
            string result = @"
                                function SetDateRange(SD,ED)
                                    {
                                        
                                        $.post('/Accounting/SetDateRange',{StartDate:SD,EndDate:ED});
                                        
                                    }

";
            

            return result;
        }
        
        public string JDepositAcctSelected()
        {
            return @"
                        function DepositAcctSelected()
                                {
                                    $(lblError).hide();
                                    $(lblCashIn).hide();
                                    $(CashIn).hide();
                                    
                                    Current.AcctID=$(DepositAccounts).val();
                                    
                                    for (var c=0;c<Accts.length;c++)
                                        {
                                            
                                            if($(DepositAccounts).val()==Accts[c].AcctID)
                                                {
                                                    Current.AcctType=Accts[c].AcctType;
                                                    Current.AcctName=Accts[c].AcctName;
                                                    Current.AcctBal=Accts[c].AcctBal;
                                                    if(Accts[c].AcctType==12)//Cash acct
                                                        {
                                                            //Show balance and input for amount to deposit
                                                            DepositCash(Accts[c]);
                                                        }
                                                    else //Credit card or check received account
                                                        {
                                                            
                                                            DepositChecks(Accts[c]);
                                                        }
                                                }
                                        }
                                    
                                }

";
        }
        public string JNoteItemSelected()
        {
            return @"

                        function NoteItemSelected(data)
                            {
                                
                                $(tblDepItems).append('<tr><td>'+data.cells[1].innerHTML+'</td><td>'+Current.AcctID+'</td><td>'+Current.AcctName+'</td><td>NOTE</td><td>$'+data.cells[4].innerHTML.substr(1)*1+'</td><td  onclick=RemoveFromDeposit(this)>X</td></tr>');
                                var note= Number(data.cells[4].innerHTML.substr(1));      
                                TotalDeposit=(Number(TotalDeposit)+note).toFixed(2);
                                $(lblTotalDeposit).text('$'+TotalDeposit);
                                ItemCount++;

                                for(c=0;c<TransactionTable.length;c++)
                                    {
                                        if(TransactionTable[c].TransactionID==data.cells[1].innerHTML)
                                            {
                                                TransactionTable[c].USED=true;
                                            }
                                    }
                                $(data).remove();
                            }

";

        }
        public string JDepositCash()
        {
            string result = @"
                                function DepositCash(a)
                                    {
                                        
                                            var BalanceText='$'+a.AcctBal;
                                            $(lblCashIn).text(BalanceText+' (Available for deposit) Enter Cash Amount:');
                                            $(lblCashIn).show();
                                            $(CashIn).show();
                                        
                                        $(tblNotes).hide();
                                       
                                    }

";
            return result;
        }
        public string JDepositChecks()
        {
            string result = @"
                                function DepositChecks(a)
                                    {
                                        //Display list of checks received in tblNotes
                                        var rowcount=document.getElementById('tblNotes').rows.length;
                                        
                                        for(var c=1;c<rowcount;c++)
                                             {
                                                 document.getElementById('tblNotes').rows[1].remove();
                                             }

            
                                        for (var c=0;c<TransactionTable.length;c++)
                                            {
                                                if(TransactionTable[c].USED==false)
                                                    {
                                                        $(tblNotes).append('<tr onclick=NoteItemSelected(this)><td>'+TransactionTable[c].DocumentID+'</td><td>'+TransactionTable[c].TransactionID+'</td><td>'+TransactionTable[c].EnterDate+'</td><td>'+TransactionTable[c].Description+'</td><td>$'+TransactionTable[c].Amount+'</td></tr>');
                                                    }
                                            }
                                        if(document.getElementById('tblNotes').rows.length>1)
                                            {
                                                $(tblNotes).show();
                                            }
                                        else
                                            {
                                                $(tblNotes).hide();
                                            }
                                    }

";
            return result;
        }
        public string JAddDepositItems()
        {
            string result = @"
                                function AddDepositItems()
                                    {
                                        var CashD=$(CashIn).val().substr(1)*1;

                                        if(CashD<=Current.AcctBal)
                                            {
                                                var tblrows=document.getElementById('tblDepItems').rows;
                                                var accountexists=new Boolean();
                                        
                                                for(c=1;c<tblrows.length;c++)
                                                    {
                                                        if(Current.AcctID==tblrows[c].cells[1].innerHTML)
                                                            {
                                                                TotalDeposit=TotalDeposit-tblrows[c].cells[4].innerHTML.substr(1)*1;
                                                                tblrows[c].cells[4].innerHTML='$'+CashD;
                                                                accountexists=true;
                                                            }
                                                    }
                                                if(accountexists==false)
                                                    {
                                                        $(tblDepItems).append('<tr><td>'+0+'</td><td>'+Current.AcctID+'</td><td>'+Current.AcctName+'</td><td>CASH</td><td>$'+CashD+'</td><td onclick=RemoveFromDeposit(this)>X</td></tr>');
                                                    }
                                                TotalDeposit=(TotalDeposit+CashD).toFixed(2);
                                                $(lblTotalDeposit).text('$'+TotalDeposit);
                                            }
                                        else
                                            {
                                                alert('Amount entered exceeds available amount to deposit!');
                                            }                                        
                                    }

";
            return result;
        }
        public string JSaveDeposit2()
        {
            string result = @"
                                function SaveDeposit2()
                                    {
                                        //need to validate bank account at some point
                                        var tbl=document.getElementById('tblDepItems');
                                        
                                       var count=0; var items='';
//ASCII 254 delimiters ■
                                       for(c=1;c<tbl.rows.length;c++)
                                            {
                                                var DepTypeID=33;
                                                if(tbl.rows[c].cells[3].innerHTML=='NOTE') DepTypeID=34;
                                                items=items+'{'+tbl.rows[c].cells[0].innerHTML+'■'+tbl.rows[c].cells[1].innerHTML+'■'+tbl.rows[c].cells[2].innerHTML+'■'+DepTypeID+'■'+tbl.rows[c].cells[4].innerHTML.substr(1)+'}';

                                            }
$.post('/Accounting/SaveDeposit',{DDate:$(DDate).val(), DItems:items,BAcct:$(BankAccounts).val(),DepRef:$(DepRef).val()});

                                    }


";
            return result;
        }
        public string JMakeDepositString()
        {
            string result = @"

                                function MakeDepositString(D)
                                    {
                                        var count=0; var items='';
                                        
                                        return count;
                                    }

";

            return result;
        }
        public string JRemoveFromDeposit()
        {
            return @"
                        function RemoveFromDeposit(data)
                            {
                                var TransID=data.parentNode.cells[0].innerHTML;
                                var Amount=data.parentNode.cells[4].innerHTML.substr(1);
                                var AcctName=data.parentNode.cells[2].innerHTML;
                                
                                if(TransID>0)
                                    {
                                       for(c=0;c<TransactionTable.length;c++)
                                            {
                                                if(TransactionTable[c].TransactionID==TransID)
                                                {
                                                    TransactionTable[c].USED=false;
                                                }
                                            }
                                    }
                                
                                TotalDeposit=(TotalDeposit-Amount).toFixed(2);
                                $(lblTotalDeposit).text('$'+TotalDeposit);
                                $(data).parent().remove();
                                
                            }


";
        }

        public string JDocumentReady(string actions)
        {
            string result = @" $(document).ready(function()
                                                    {
                                                        " + actions + @"
                                                    });
";
            return result;
        }
        
        public string JDepNowClicked()
        {
            return @"
                                function DepNowClicked()
                                {
                                    if(DI) {
                                                DI=false;
                                                $(DepRef).val('');
                                                $(FeeDiv).hide();
                                            }
                                    else    {
                                                $(FeeDiv).show();
                                                DI=true;
                                            }
                
                                }


";
        }
        
        public string JRecAcctChange()
        {
            string result = @"
                                function RecAcctChange()
                                        {
                                           if(ApplyPay)
                                            {
                                                $(DivQuickAdd).empty();
                                                $.post('/Accounting/GetPayAgainstDocList',{AcctID:$(ddlReceivableAcct).val()});  
                                            } 
                                            
                                        }

";
            return result;
        }
        
        public string JApplyPaymentClicked() //requires AppliedAmount, InvoiceArray() and AppliedPayList() from MakeReceivePayment
        {
            string result = @"
                                function ApplyPaymentClicked()
                                {
                                    if(ApplyPay)
                                    {
                                        ApplyPay=false;
                                        $(DivQuickAdd).empty();
                                        AppliedAmount=0;
                                        
                                        InvoiceArray.splice(0,InvoiceArray.length);
                                        AppliedPayList.splice(0,AppliedPayList.length);

                                        
                                    }
                                    else
                                    {
                                        ApplyPay=true;
                                        $.post('/Accounting/GetPayAgainstDocList',{AcctID:$(ddlReceivableAcct).val()});
                                    }
                                }


";



            return result;
        }
        public string JApplyPayment() //Requires InvoiceArray(), var AppliedAmount and AppliedPayList() from Apply Payment
        {
            string result = @"
                                function ApplyPayment(rowid) //this is not an id, but is the row element itself
                                    {
                                        
                                        var InvRef=$(rowid).find('td:eq(1)').text();
                                        var InvAMT=$(rowid).find('td:eq(2)').text();
                                        InvAMT=InvAMT.substr(2);
                                        InvAMT=Number(InvAMT).toFixed(2);
                                        var idx=AppliedPayList.indexOf(InvRef);

                                        if(idx==-1)//not listed add
                                            {
                                                
                                                AppliedPayList.push(InvRef);
                                                InvoiceArray.push({InvRef:InvRef,InvAMT:InvAMT});
                                                AppliedAmount=AppliedAmount+InvAMT*1;
                                                $(rowid).addClass('tablerowselected');
                                            }
                                        else //listed remove
                                            {
                                                
                                                AppliedPayList.splice(idx,1);
                                                InvoiceArray.splice(idx,1)
                                                AppliedAmount=Number((AppliedAmount-InvAMT*1).toFixed(2));
                                                $(rowid).removeClass('tablerowselected');
                                            }

                                        

                                        

                                        

                                        $(InvAmount).val('$'+AppliedAmount); //element added and removed in ApplyPaymentClicked()

                                        var PAMT=$(receivedamt).val().substr(1);
                                        
                                        try
                                            {
                                                
                                            }
                                        catch(e)
                                            {
                                                alert(e);
                                            }
                                        
                                    }



";
            

            return result;
        }
        public string JSavePaymentReceived()
        {
            return @"

                            function SavePaymentReceived()
                                {
                                    
                                    if(AppliedPayList.length>0)
                                        {
                                            var count=0;
                                            while(count<AppliedPayList.length)
                                                {
                                                        
                                                        
                                                            AppliedInvoices=AppliedInvoices+'{'+AppliedPayList[count]+'}';
                                                        
                            
                                                    count++;            
                    
                                                }
                                        }
                                            $.post('/Accounting/SaveReceivedPayment',{Pdate:$(Pdate).val(),Pact:$(ddlReceivableAcct).val(),DepRef:$(DepRef).val(),DI:DI,Pmethod:$(ddlPaymentMethod).val(),PayRef:$(tbPayRef).val(),Desc:$(tbDescription).val(),Pamt:$(receivedamt).val(),BankID:$(ddlBankAcct).val(),AppliedInvoices:AppliedInvoices});

                                }



";
        }
        
        
        public string JSavePurchase()
        {
            return @"function SavePurchase()

            {
                
                var fee=0; var cashback=0; var dDate=$(PDate).val(); var Refer=$(PRef).val(); var paymeth=$(PaymentMethod).val(); var InvRec=$(PInv).val();
                var docpath=$(PDocPath).val(); var pacct=$(ddlPurchAcctList).val(); var vacct=$(ddlVenderList).val();
                var shipexp=$(ShipExpense).val().substring(1); var shipcog=$(ShipCOG).val().substring(1); var ptax=$(PTax).val().substring(1); var taxloc=document.getElementById('ddlTaxRateList')[document.getElementById('ddlTaxRateList').selectedIndex].text;
                var RecItems=document.getElementById('ReceiveItems').checked;
                
                if(FEE){fee=$(FeeAmt).val().substring(1);}
                if(CASHBACK){cashback=$(CashBackAmt).val().substring(1);}
                
                var count=0; var items='';
                while(count<itemarray.length)
                {
                    if(itemarray[count].LineID>-1)
                    {
                    items=items+'{'+itemarray[count].Quantity+'▄'+itemarray[count].Item+'▄'+itemarray[count].Amount+'▄'+itemarray[count].Taxable+'▄'+itemarray[count].LineAllocation+'}';
                    }
                    count++;
                    
                }

                serialnumbers=JSON.stringify(snlist);
                   
                $.post('/Accounting/SavePurchase',{PDate:dDate,VendAcct:vacct, PAcct:pacct, TransFee:fee, LineItems:items, tax:ptax, Ref:Refer, CashBack:cashback, DocPath:docpath, PayMethod:paymeth, TaxLocation:taxloc, Sns:serialnumbers, COGShip:shipcog, EXPShip:shipexp, InvRec:InvRec, ReceiveItems:RecItems});
            } ";
        }
        public string JLineClick()
        {
            return @" function LineClick(l,boxid)
            {
                var fee=RemoveDollarSign($(FeeAmt).val());
                var cashback=RemoveDollarSign($(CashBackAmt).val());
                if(boxid.checked)
                    {
                        
                        var PTaxIn=Number($(PTax).val().substring(1));
                        var PTotalIn=Number($(PurchaseTotal).val().substring(1));
                        
                        var TT=Number($(TotalTaxablePurchase).val().substring(1))+Number(l);
                        var ET=Number($(TotalExemptPurchase).val().substring(1))-Number(l);
                        var ptax=Number(($(ddlTaxRateList).val()*TT).toFixed(2));
                        var PTotal=Number(PTotalIn-PTaxIn+ptax);

                        $(TotalTaxablePurchase).val('$'+(TT.toFixed(2)));
                        $(TotalExemptPurchase).val('$'+(ET.toFixed(2)));
                        $(PTax).val('$'+(ptax.toFixed(2)));
                        $(PurchaseTotal).val('$'+(PTotal.toFixed(2)));
                        
                        
                        itemarray[boxid.id.substring(4)].Taxable=true;
                        TaxTotal=ptax;
                    }
                else
                    {
                        var PTaxIn=Number($(PTax).val().substring(1));
                        var PTotalIn=Number($(PurchaseTotal).val().substring(1));

                        var TT=Number($(TotalTaxablePurchase).val().substring(1))-Number(l);
                        var ET=Number($(TotalExemptPurchase).val().substring(1))+Number(l);
                        var ptax=Number(($(ddlTaxRateList).val()*TT).toFixed(2));
                        var PTotal=Number(PTotalIn-PTaxIn+ptax);

                        $(TotalTaxablePurchase).val('$'+(TT.toFixed(2)));
                        $(TotalExemptPurchase).val('$'+(ET.toFixed(2)));
                        $(PTax).val('$'+(ptax.toFixed(2)));
                        $(PurchaseTotal).val('$'+(PTotal.toFixed(2)));
                        
                        itemarray[boxid.id.substring(4)].Taxable=false;
                        TaxTotal=ptax;
                    }
            } ";
        }
        public string JAddItem()
        {
            return @" function AddItem()
            {
                
                if(!(serialized))
                    {
                       ItemAdd();
                    }
                else
                    {
                        MakeSnList($(ddlItemList).val(),$(PQ).val());
                    }  
            } ";
        }
        public string JItemAdd()
        {
            return @" function ItemAdd()
            {
                var itemname=document.getElementById('ddlItemList').options[document.getElementById('ddlItemList').selectedIndex].text;
                var itemID=$(ddlItemList).val();
                var itemprice=$(PAMT).val().substr(1);
                var itemquan=Number($(PQ).val());
                var TotalIn=Number($(TotalTaxablePurchase).val().substr(1));
                var itemtaxrate=$(ddlTaxRateList).val();
                var transfee=Number($(FeeAmt).val().substr(1));
                var cashback=Number($(CashBackAmt).val().substr(1));
                
                

                if(!(FEE))
                    {
                        transfee=0;
                    }
                if(!(CASHBACK))
                    {
                        cashback=0
                    }
                itemtotal=(itemquan*itemprice).toFixed(2);
               
                Total=(TotalIn+Number(itemtotal)).toFixed(2);
                
                var cbid='line'+itemcount;
                var cbeid='LE'+itemcount;
                var Prow='PROW'+itemcount;
                var btnDelLine='DelL'+itemcount;
                
                lineitem={LineID:itemcount,Quantity:itemquan,Item:itemID,Amount:itemprice,Taxable:true,LineAllocation:-1};
                itemarray[itemcount]=lineitem;
                
                

                lineitems=lineitems+'{'+itemquan+','+itemID+','+itemprice+'}';
                $(tblPurchasebody).append('<tr id='+Prow+'><td><button id=' + btnDelLine +' onclick= DelLine('+Prow+','+itemcount+')>REMOVE</button></td><td>'+itemquan+'</td><td>'+itemID+'</td><td>'+itemname+'</td><td>'+itemprice+'</td><td>'+itemtotal+'</td><td><input id='+cbid+' type=checkbox checked=checked onclick=LineClick('+itemtotal+','+cbid+'); ></input></td><td><input id='+cbeid+' type=checkbox onclick=LineAllocationChange('+cbeid+','+Prow+'); ></input></input></td></tr>');
                
                $(TotalTaxablePurchase).val('$'+Total);
                $(PTax).val('$'+(itemtaxrate*Total).toFixed(2));
                $(PQ).val(1); //$(PAMT).val(0);
                itemcount++;
                var itemtax=Number(itemtaxrate*Total).toFixed(2);
                
                $(PurchaseTotal).val('$'+(Number(Number(itemtax)+Number(Total)+transfee+cashback)).toFixed(2));
                TaxTotal=(itemtaxrate*Total).toFixed(2);
            } ";
        }
        public string JItemChange()
        {
            return @" function ItemChange()
            {
                //verify that vender item list has been loaded, otherwise load it
                
                if(SelectedVenderID==0){$(ddlVenderList).change();}
                
                //Check for Vender item info
                for(var c=0;c<Vitems.length;c++)
                    {
                        
                        if(Vitems[c].ItemID==$(ddlItemList).val()) //if found populate fields
                            {
                                $(PAMT).val('$'+((Vitems[c].LastCost*1)).toFixed(2));
                                break;
                            }
                        
                    }

                SetSerialized($(ddlItemList).val());
            } ";
        }

        public string JSaleAddItem()
        {
            return @"
                        function AddItem()
                            {
                                if(ItemVerified)
                                    {
                                        if(!(serialized))
                                            {
                                                ItemAdd();
                                            }
                                        else
                                            {
                                                MakeSnList($(ddlItemList).val(),$(SQ).val());
                                            }  
                                    }
                                else
                                    {
                                        alert('Item not valid for selected pay method');
                                    }
                            }
";
        }
        public string JSaleItemAdd()
        {
            return @"
                        function ItemAdd()
                            {
                                //var sitem=document.getElementById('ddlItemList').options[document.getElementById            ('ddlItemList').selectedIndex].text;
                                var sitem=$('#ddlItemList option:selected').text();
                                
                                var itemdescription=$(ItemDescription).val();
                                
                                
                                if(!(IsInvoice))
                                {
                                    SaleAmount=$(SAMT).val();
                                    lineitem={LineID:itemcount,Quantity:Number($(SQ).val()),Item:$(ddlItemList).val(),JobLineID:0,Description:itemdescription,Amount:SaleAmount,Taxable:false,SN:SCTypeID};//Put service credit type in SN field
                                }
                                else
                                {
                                    SaleAmount=$(SAMT).val().substr(1);
                                    lineitem={LineID:itemcount,Quantity:Number($(SQ).val()),Item:$(ddlItemList).val(),JobLineID:0,Description:itemdescription,Amount:SaleAmount,Taxable:true,SN:0};
                                }
                                subtotal=(Number($(SQ).val()*SaleAmount)).toFixed(2);
                
                                
                
                                var cbid='line'+itemcount;
                                var ddlLE='LE'+itemcount;
                                var Prow='PROW'+itemcount;
                                var btnDelLine='DelL'+itemcount;
                
                                Total=(Number($(TotalTaxableSale).val())+Number(subtotal)).toFixed(2);

                                itemarray[itemcount]=lineitem;
                
               
                                lineitems=lineitems+'{'+$(SQ).val()+','+$(ddlItemList).val()+','+$(SAMT).val()+'}';

                                if(!(IsInvoice))
                                {
                                    $(tblSalebody).append('<tr id='+Prow+'><td><button id=' + btnDelLine +' onclick= DelLine('+Prow+','+itemcount+')>REMOVE</button></td><td>'+$(SQ).val()+'</td><td>'+$(ddlItemList).val()+'</td><td>'+sitem+'-'+itemdescription+'</td><td>'+$(SAMT).val()+'</td><td>'+subtotal+'</td></tr>');
                                    $(Tax).val(0);
                                    $(TotalExemptSale).val(Total);
                                    ServiceCreditsBalance=ServiceCreditsBalance-subtotal;
                                    $(lblCreditsBalanceAmt).text(ServiceCreditsBalance);
                                }
                                else
                                {
                                    $(tblSalebody).append('<tr id='+Prow+'><td><button id=' + btnDelLine +' onclick= DelLine('+Prow+','+itemcount+')>REMOVE</button></td><td>'+$(SQ).val()+'</td><td>'+$(ddlItemList).val()+'</td><td>'+sitem+'-'+itemdescription+'</td><td>'+$(SAMT).val()+'</td><td>'+subtotal+'</td><td><input id='+cbid+' type=checkbox checked=checked onclick=LineClick('+subtotal+','+cbid+'); ></input></td></tr>');
                                    $(Tax).val(($(ddlTaxRateList).val()*Total).toFixed(2));
                                    $(TotalTaxableSale).val(Total);
                                }
                                
                                itemcount++;
                
                                var ptax=Number($(Tax).val()).toFixed(2);
                
                                $(SaleTotal).val((Number(Number(ptax)+Number(Total))).toFixed(2));
                            }


";
        }
        public string JSaleDelLine()
        {
            return @"
                        function DelLine(rid,rcount)
                            {
                
                                $(rid).remove();
                                var idx=0;
                                while(idx<itemarray.length)
                                    {
                   
                                        if(itemarray[idx].LineID==rcount)
                                            {
                                                subtotal=(Number(itemarray[idx].Quantity*itemarray[idx].Amount)).toFixed(2);
                                                if(itemarray[idx].Taxable)
                                                    { 
                                                        Total=(Number($(TotalTaxableSale).val())-Number(subtotal)).toFixed(2);
                                                        $(TotalTaxableSale).val(Total);
                                                        $(Tax).val(($(ddlTaxRateList).val()*Total).toFixed(2));
                                                        var ptax=Number($(Tax).val()).toFixed(2);
                                                        $(SaleTotal).val((Number(Number(ptax)+Number(Total))).toFixed(2));
                                                    }
                                                else
                                                    {
                                                        Total=(Number($(TotalExemptSale).val())-Number(subtotal)).toFixed(2);
                                                        $(TotalExemptSale).val(Total);
                                                        $(SaleTotal).val(Number(Total).toFixed(2));
                                                        if(!(IsInvoice))
                                                            {
                                                                ServiceCreditsBalance=ServiceCreditsBalance*1+subtotal*1;
                                                                $(lblCreditsBalanceAmt).text(ServiceCreditsBalance);
                                                            } 
                                                    }
                                                itemarray[idx].LineID=-1;
                        
                                            }

                                        
    
                                        idx++;
                                    }
                                var count=0;
                                while(count<itemcount)
                                    {
                                        if(snlist[count].Prow==rcount)
                                            {
                                                snlist.splice(count,1);
                                            }
                                        alert(snlist[count].Prow);
                                        count++;
                                    }
                            }
";
        }
        public string JSaveSale()
        {
            return @"
                        function SaveSale()

                            {
                
                                var count=0; var items=''; var serialnumbers='';
                                while(count<itemarray.length)
                                    {
                                        if(itemarray[count].LineID>-1)// delimiter alt 220 ▄
                                        {
                                            items=items+'{'+itemarray[count].Quantity+'▄'+itemarray[count].Item+'▄'+itemarray[count].Description+'▄'+itemarray[count].Amount+'▄'+itemarray[count].Taxable+'▄'+itemarray[count].SN+'▄'+itemarray[count].JobLineID+'}';
                                        }
                            
                                        count++;            
                    
                                    }
                
                                serialnumbers=JSON.stringify(snlist);
                

                                $.post('/Accounting/SaveSale',{IDate:$(SDate).val(), CAcct:$(ddlCust).val(),LineItems:items,tax:$(Tax).val(),PayMethod:$(PayMeth).val(),Paid:Paid,TaxRateName:document.getElementById('ddlTaxRateList')[document.getElementById('ddlTaxRateList').selectedIndex].text,TaxRate:$(ddlTaxRateList).val(),Sns:serialnumbers});
                            }
";
        }
        public string JSaveSalePaid()
        {
            return @"
                        function SaveSalePaid()
                            {
                                Paid=true;
                                SaveSale();
                            }
";
        }
        public string JSaleLineClick()
        {
            return @"
                        function LineClick(l,boxid)
                        {
                
                            if(boxid.checked)
                                {
                                    var TT=Number($(TotalTaxableSale).val())+Number(l);
                                    $(TotalTaxableSale).val(TT.toFixed(2));
                                    var ET=Number($(TotalExemptSale).val())-Number(l);
                                    $(TotalExemptSale).val(ET.toFixed(2));
                                    $(Tax).val(($(ddlTaxRateList).val()*TT).toFixed(2));
                                    $(SaleTotal).val((TT+Number($(Tax).val())+Number($(TotalExemptSale).val())).toFixed(2));
                                    itemarray[boxid.id.substring(4)].Taxable=true;
                                }
                            else
                                {
                                    var TT=Number($(TotalTaxableSale).val())-Number(l);
                                    $(TotalTaxableSale).val(TT.toFixed(2));
                                    var ET=Number($(TotalExemptSale).val())+Number(l);
                                    $(TotalExemptSale).val(ET.toFixed(2));
                                    $(Tax).val(($(ddlTaxRateList).val()*TT).toFixed(2));
                                    $(SaleTotal).val((TT+Number($(Tax).val())+Number($(TotalExemptSale).val())).toFixed(2));
                        
                                    itemarray[boxid.id.substring(4)].Taxable=false;
                                }
                        }
";
        }
        public string JSaleItemChange()
        {
            return @"
                        function ItemChange()
                            {
                                SetSerialized($(ddlItemList).val());
                                SetServiceCredits($(ddlItemList).val());
                                
                            }
";
        }
        
        public string JLineAllocationChange()
        {
            return @" function LineAllocationChange(boxid,rowid)
            {
               
                if(boxid.checked)
                    {
                        
                        
                        $(rowid).append('<td><select id=explist'+rowid.id+' onchange=ExpenseOverideSelected(explist'+rowid.id+','+boxid.id.substring(2)+')>'+expoptions+'</select></td>');
                        
                        itemarray[boxid.id.substring(2)].LineAllocation=0; //default expense selected
                    }
                else
                    {
                        itemarray[boxid.id.substring(2)].LineAllocation=-1; //no expense selected
                        
                        var dcell=rowid.cells[8];
                        $(dcell).remove();
                        
                    }
            } ";
        }
        public string JDelLine()
        {
            return @" function DelLine(rid,rcount)
            {
                $(rid).remove();
                var hcell=document.getElementById('tblPurchase').rows[0].cells[8];
                $(hcell).remove();
                
                


var idx=0; var linetotal=0; var TtotalIn=Number($(TotalTaxablePurchase).val().substring(1)); var EtotalIn=Number($(TotalExemptPurchase).val().substring(1)); var PTotalIn=Number($(PurchaseTotal).val().substring(1)); var TaxIn=Number($(PTax).val().substring(1));
               
                while(idx<itemarray.length)
                {
                   linetotal=(Number(itemarray[idx].Quantity*itemarray[idx].Amount)).toFixed(2);
                   if(itemarray[idx].LineID==rcount)
                    {
                      
                      if(itemarray[idx].Taxable)
                            {
                                var NTotal=Number(PTotalIn-TtotalIn).toFixed(2);
                                
                                var TTotal=(TtotalIn-Number(linetotal)).toFixed(2);
                                $(TotalTaxablePurchase).val('$'+TTotal);
                                NTotal=NTotal*1+TTotal*1;
                                NTotal=NTotal*1-TaxIn*1;
                                var ptax=($(ddlTaxRateList).val()*TTotal).toFixed(2);
                                $(PTax).val('$'+ptax);
                                NTotal=(NTotal*1+ptax*1).toFixed(2);
                                TaxTotal=ptax;
                                $(PurchaseTotal).val('$'+NTotal);
                            }
                      else
                            {
                                
                                var ETotal=Number(EtotalIn-linetotal).toFixed(2);
                                var NTotal=Number(PTotalIn-linetotal).toFixed(2);
                                $(TotalExemptPurchase).val('$'+ETotal);
                                $(PurchaseTotal).val('$'+NTotal); 
                            }

                    

                   itemarray[idx].LineID=-1;
                        



                    }
                    
    
                idx++;
                }

                var count=0;
                while(count<itemcount)
                    {
                        if(snlist[count].Prow==rcount)
                            {
                                snlist.splice(count,1);
                            }
                        
                        count++;
                    }
            } ";
        }
        
        
        
        public string JSaveAs()
        {
            return @" function SaveAs(x) //1=paid and received, 2=Quote Request, 3=Purchase Order, 4=paid not received
            {
                SaveAsThis=x;
                SavePurchase();
            } ";
        }
        public string JFeeBoxClicked()
        {
            return @" function FeeBoxClicked()
            {
                
                if(FEE) {
                            var FeeIn=Number($(FeeAmt).val().substring(1));
                            var TotalIn=Number($(PurchaseTotal).val().substring(1));
                            $(PurchaseTotal).val('$'+(Number(TotalIn-FeeIn)).toFixed(2));
                            FEE=false;
                            $(FeeDiv).hide();
                            
                        }
                else    {
                            $(FeeDiv).show();
                            $(FeeAmt).focus();
                            FEE=true;
                        }
                
            } ";
        }
        public string JFeeAmtBlur()
        {
            string result = @" 
                                function FeeAmtBlur()
                                            {
                                            var TotalIn=Number($(PurchaseTotal).val().substring(1));
                                            var FeeIn=Number($(FeeAmt).val().substring(1));
                                            $(PurchaseTotal).val('$'+(Number(FeeIn+TotalIn)).toFixed(2));
                                            }
";

            return result;
        }
        public string JCashBackBoxClicked()
        {
            return @" function CashBackBoxClicked()
            {
                if(CASHBACK) {
                            var CashBackIn=Number($(CashBackAmt).val().substring(1));
                            var TotalIn=Number($(PurchaseTotal).val().substring(1));
                            $(PurchaseTotal).val('$'+(Number(TotalIn-CashBackIn)).toFixed(2));
                            CASHBACK=false;
                            $(CashBackDiv).hide();
                        }
                else    {
                            $(CashBackDiv).show();
                            $(CashBackAmt).focus();
                            CASHBACK=true;
                        }
            } ";
        }
        public string JCashBackAmtBlur()
        {
            string result = @" 
                                function CashBackAmtBlur()
                                            {
                                            var TotalIn=Number($(PurchaseTotal).val().substring(1));
                                            var CashBackIn=Number($(CashBackAmt).val().substring(1));
                                            $(PurchaseTotal).val('$'+(Number(CashBackIn+TotalIn)).toFixed(2));
                                            
                                            }
";

            return result;
        }
        public string JShippingCOGBlur()
        {
            string result = @" 
                                function ShippingCOGBlur()
                                            {
                                            var TotalIn=Number($(PurchaseTotal).val().substring(1));
                                            var ShipCogIn=Number($(ShipCOG).val().substring(1));
                                            $(PurchaseTotal).val('$'+(Number(ShipCogIn+TotalIn-shipcog)).toFixed(2));
                                            shipcog=ShipCogIn;
                                            }
";

            return result;
        }
        public string JShippingExpBlur()
        {
            string result = @" 
                                function ShippingExpBlur()
                                            {
                                            var TotalIn=Number($(PurchaseTotal).val().substring(1));
                                            var ShipExpIn=Number($(ShipExpense).val().substring(1));
                                            $(PurchaseTotal).val('$'+(Number(ShipExpIn+TotalIn-shipexp)).toFixed(2));
                                            shipexp=ShipExpIn;
                                            }
";

            return result;
        }
        public string JPTaxBlur()
        {
            string result = @"
                                function PTaxBlur()
                                    {
                                            var TotalIn=Number($(PurchaseTotal).val().substring(1));
                                            var PTaxIn=Number($(PTax).val().substring(1));
                                            
                                            $(PurchaseTotal).val('$'+(Number(PTaxIn+TotalIn-TaxTotal)).toFixed(2));
                                            TaxTotal=PTaxIn;
                                            }
";
            return result;
        }
        
        
        public string JTaxChange()
        {
            return @" function TaxChange()
            {
                
                var TaxRate=Number($(ddlTaxRateList).val());
                var TT=Number($(TotalTaxablePurchase).val().substring(1))
                var PTotalIn=$(PurchaseTotal).val().substring(1);
                var TaxIn=Number($(PTax).val().substring(1));
                var NTax=Number((TaxRate*TT).toFixed(2));
                var PTotal=Number(PTotalIn-TaxTotal+NTax);

                $(lblddlTaxRateList).text('Tax Rate:'+(TaxRate*100).toFixed(2)+'%');
                $(PTax).val('$'+(NTax).toFixed(2));
                $(PurchaseTotal).val('$'+(PTotal).toFixed(2));

                TaxTotal=NTax;
            } ";
        }
        public string JNewItem()
        {
            return @" function NewItem()
            {
                $(PitemDiv).show();
            } ";
        }
        
        public string JMakeSnList()
        {
            return @" function MakeSnList(itemid,quantity)
            {
                
                $(DivQuickAdd).empty();
                $(DivQuickAdd).append('<label>Enter Serial Numbers</label><br></br>');
                var lcount=0;
                while(lcount<quantity)
                    {
                        $(DivQuickAdd).append('<input onblur=SerialFieldBlur(this) type=text id=SN'+lcount+'></input>');
                        lcount++;
                    }
                $(DivQuickAdd).append('<Button onclick=SaveSns('+itemid+','+quantity+')>SAVE</Button>');
                $(DivQuickAdd).append('<Button onclick=CancelSns()>CANCEL</Button>');
            } ";
        }
        public string JSaveSns()
        {
            return @" function SaveSns(itemID,quan)
            {
                
                var alldatagood=true;
                if(presnlist.length==quan)
                    {
                        for(var z=0;z<presnlist.length;z++) //make sure all serial number fields contain valid data and if not exit
                            {
                    
                                if(presnlist[z].sn.length<1 || presnlist[z].sn.indexOf('}')>-1 || presnlist[z].sn.indexOf('{')>-1 || presnlist[z].sn.indexOf('" + "\"" + @"')>-1)
                                    {
                                        alldatagood=false;
                                        
                                    }
                            }
                    }
                else
                    {
                        alldatagood=false;
                    }
                if(alldatagood)
                    {
                        
                        var transfersns=new Array();
                        
                        snlist.push({Prow:itemcount,ItemID:itemID,sns:transfersns});
                            var precount=0;
                            while(precount<presnlist.length)
                                {
                                    snlist[snlist.length-1].sns[precount]=presnlist[precount].sn;
                                    precount++;
                                    
                                }
                        $(DivQuickAdd).empty();
                        presnlist.splice(0,presnlist.length); //empty the array
                        ItemAdd();
                    }
                else
                    {
                        alert('Error: missing serial number or a serial number contains one of the following invalid characters: {,}," + "\"" + @"');
                    }
                
            } ";
        }
        public string JCancelSns()
        {
            return @" function CancelSns()
            {
                alert('This item must inclued serial numbers. Item Add Canceled.');
                $(DivQuickAdd).empty();
                presnlist.splice(0,presnlist.length); //empty the array
            } ";
        }
        public string JSerialFieldBlur()
        {
            return @" function SerialFieldBlur(event)
            {
                var count=0;
                var notinlist=true;
                while(count<presnlist.length)
                    {
                        if($(event).data().snid==presnlist[count].ifield)
                                {
                                    presnlist[count].sn=$(event).val();
                                    notinlist=false;
                                }
                        count++;
                    }
                if(notinlist)
                    {
                        
                        $(event).data('snid',count);
                        presnlist.push({ifield:count,sn:$(event).val()});
                    }
                
            } ";
        }
        public string JSetSerialized()
        {
            return @" function SetSerialized(itemID)
            {
                serialized=false;
                for(var i=0;i<serialedItems.length;i++)
                    {
                        if(serialedItems[i]==itemID)
                            {
                                serialized=true;
                                return;
                            }
                    }
            }         ";
        }
        
        public string JSetServiceCredits() //Check if selected item is covered by service credits
        {
            return @"

                        function SetServiceCredits(ItemID)
                            {
                                
                                ItemVerified=true;
                                if(!(IsInvoice))
                                    {
                                        ItemVerified=false;
                                        
                                        if(ServiceCreditsBalance>0)
                                            {
                                                
                                                VerifyItem(ItemID);

                                                if(ItemVerified)
                                                    {
                                                        $(SAMT).hide();
                                                        $(SAMT).val(ItemServiceCredits);
                                                    }
                                                else
                                                    {
                                                        alert('This item is not elegible for service credits');
                                                    }
                                            }
                                        else
                                            {
                                                alert('Choose Service Credit Type first.');
                                            }        
                                    }
                                
                            }

";
        }
        
        public string JRemoveDollarSign()
        {
            return @"
                        function RemoveDollarSign(Damt)
                        {
                            
                            return Damt.substring(1);
                        }
";
        }
        
        public string JCreateExpenseOptionsVariable()
        {
            string result = "";
            string options = M.MakeOptionList("EXPENSES");
            result = @" var expoptions='" + options + @"'; ";
            return result;
        }
        public string JExpenseOverideSelected()
        {
            string result = @"

                                function ExpenseOverideSelected(exid,itemarraysub)
                                {
                                    
                                    itemarray[itemarraysub].LineAllocation=$(exid).val();
                                }




";


            return result;
        }
        public string JPayMethSelected()
        {
            return @"
                        function PayMethSelected() //Called by PayMeth change
                            {
                                
                                if(HasServiceCredits)
                                    {
                                        if(itemarray.length>0)
                                            {
                                                if((IsInvoice)&&($(PayMeth).val()==6))//PayMeth value of 6 is prepaid service credits
                                                    {
                                                        alert('Invoice items can not be converted to Service Credit Items, Select back in order to start a new service credit deduction document');
                                                        $(PayMeth).val(CurrentPayMeth);
                                                    }
                                                if(!(IsInvoice))//When !IsInvoice PayMeth is always = 6 unless attempt is made to change it which is the only time this would be called.
                                                    {
                                                        alert('Service Credit items can not be converted to Invoice Items, Select back in order to start a new Invoice document');
                                                        $(PayMeth).val(6);
                                                    }
                                                CurrentPayMeth=$(PayMeth).val();
                                            }
                                        else
                                            {
                                                CurrentPayMeth=$(PayMeth).val();
                                                if($(PayMeth).val()==6) //PayMeth value of 6 is prepaid service credits
                                                    {
                                                        IsInvoice=false;
                                                    }
                                                else
                                                    {
                                                        IsInvoice=true;
                                                    }
                                            }
                                    }
                                else
                                    {
                                        if($(PayMeth).val()==6)
                                            {
                                                alert('This pay method is only valid if the Customer has Service Credits to use');
                                                $(PayMeth).val(CurrentPayMeth);
                                            }
                                        
                                    }
                                //Verify current selected item
                                VerifyItem($(ddlItemList).val());
                                
                            }

";
        }
        public string JVerifyItem() //Sets validty for item based on wether doc is invoice or service credit deduction. If Service Credit Deduction it also sets the value of ItemServiceCredits 
        {
            return @"
                        function VerifyItem(ItemID) 
                            {
                                
                                ItemVerified=false;
                                ServiceCredits=0;
                                if(IsInvoice)
                                    {
                                        ItemVerified=true;
                                    }
                                else    //Service Credit Deductions verify item is valid and set ItemServiceCredits
                                    {
                                        for(var c=0;c<PrePayValue.length;c++)
                                                {
                                                    if(PrePayValue[c].SCTID==SCTypeID&&PrePayValue[c].ItemID==ItemID)
                                                        {
                                                            ItemServiceCredits=PrePayValue[c].Value;
                                                            ItemVerified=true;
                                                        }
                                                }
                                    }
                            }


";
        }
        #endregion

    }
    

}