using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using System.Text;
using OurThings.Classes;
using System.Web.Script.Serialization;



namespace OurThings.Models
{
    public class MyMethods
    {
        TheDataDataContext db = new TheDataDataContext();
        AuthorizeDataDataContext adb = new AuthorizeDataDataContext();
      //  MyJava J  = new MyJava();
        //DRCRepository DRCrepos = new DRCRepository();
        //DRCClientDBDataContext db2 = new DRCClientDBDataContext();

        
        public string fixapost(string helpme)//Prepare apostophies from existing strings for incoding
        {
            if (helpme == null) helpme = "empty";
            if (helpme.IndexOf('\'') > (-1))
            {
                int count = 0;
                int problemindex = helpme.IndexOf('\'');
                int nextindex = problemindex;
                List<int> fix = new List<int>();
                fix.Add(problemindex);
                while (nextindex > -1)
                {
                    count++;

                    if (nextindex + 1 > helpme.Length)
                    {
                        nextindex = -1;
                        count--;
                    }
                    else
                    {
                        int currentidx = nextindex + 1;
                        nextindex = helpme.IndexOf('\'', currentidx);
                        if (nextindex > -1) fix.Add(nextindex);
                    }
                }
                if (count == 1)//Only one needs fixing
                {
                    string firsthalf = helpme.Substring(0, problemindex);
                    string lasthalf = helpme.Substring(problemindex + 1);
                    helpme = firsthalf + @"\'" + lasthalf;
                    nextindex = helpme.IndexOf('\'', problemindex);
                }
                else//multiple fixing required
                {
                    List<string> helpmepart = new List<string>();
                    if (fix[0] == 0) helpmepart.Add("\\'");
                    else
                    {
                        helpmepart.Add(helpme.Substring(0, fix[0]));
                        helpmepart.Add("\\'");
                    }
                    for (int i = 0; i < fix.Count; i++)
                    {
                        if (i == fix.Count - 1) helpmepart.Add(helpme.Substring(fix[i] + 1));
                        else
                        {
                            helpmepart.Add(helpme.Substring(fix[i] + 1, fix[i + 1] - fix[i] - 1));
                            helpmepart.Add("\\'");
                        }


                    }
                    helpme = System.String.Concat(helpmepart);
                }

            }


            return helpme;
        }
        public string fixquotes(string text)//Prepare existing double quotes for encoding
        {
            if (text.IndexOf('\"') > (-1))
            {
                int count = 0;
                int problemindex = text.IndexOf('\"');
                int nextindex = problemindex;
                List<int> fix = new List<int>();
                fix.Add(problemindex);
                while (nextindex > -1)
                {
                    count++;

                    if (nextindex + 1 > text.Length)
                    {
                        nextindex = -1;
                        count--;
                    }
                    else
                    {
                        int currentidx = nextindex + 1;
                        nextindex = text.IndexOf('\"', currentidx);
                        if (nextindex > -1) fix.Add(nextindex);
                    }
                }
                if (count == 1)//Only one needs fixing
                {
                    string firsthalf = text.Substring(0, problemindex);
                    string lasthalf = text.Substring(problemindex + 1);
                    text = firsthalf + @"\" + @"""" + lasthalf;
                    nextindex = text.IndexOf('\'', problemindex);
                }
                else//multiple fixing required
                {
                    List<string> helpmepart = new List<string>();
                    if (fix[0] == 0) helpmepart.Add(@"\" + @"""");
                    else
                    {
                        helpmepart.Add(text.Substring(0, fix[0]));
                        helpmepart.Add(@"\" + @"""");
                    }
                    for (int i = 0; i < fix.Count; i++)
                    {
                        if (i == fix.Count - 1) helpmepart.Add(text.Substring(fix[i] + 1));
                        else
                        {
                            helpmepart.Add(text.Substring(fix[i] + 1, fix[i + 1] - fix[i] - 1));
                            helpmepart.Add(@"\" + @"""");
                        }


                    }
                    text = System.String.Concat(helpmepart);
                }

            }


            return text;
        }
        public string fixslash(string text)
        {
            string result = "";
            List<int> idxs = new List<int>();
            //when porting a '\\' to label via java script result the slash will not display and when porting '\\\\' only one will display. This method adds \\ to each \\ in order to correct it.
            if (text.IndexOf('\\') > (-1))
            {
                //add slashes
                for (int i = 0; i < text.Length; i++)
                {
                    if (text.Substring(i, 1) == "\\") result = result+text.Substring(i, 1) + "\\";
                    else result = result + text.Substring(i, 1);
                    
                }
            }

            return result;
        }
        public string DRCuser()
        {
            string usr;
            usr = Membership.GetUser().UserName;
            return usr;
        }
        

        public string tblheaders(Array headers, string tblname) //Add headers to the named table
        {
            //Always clear out the table first
            string result = @"$(" + tblname + @").empty(); " + @"$(" + tblname + @").append('<tr>";
            foreach (string H in headers)
            {
                string T = "";
                string Head = "";
                if (H.IndexOf(':') > -1)
                {
                    T = H.Substring(H.IndexOf(':') + 1);
                    Head = H.Substring(0, H.IndexOf(':'));
                }
                else
                {
                    T = H;
                    Head = H;
                }
                result = result + @"<th title=" + T + @">" + Head + @"</th>";
            }
            result = result + @"</tr>'); ";
            return result;
        }
        public string tbldata(string[] datatype, string[] data, string tblname, int row, int MaxRowsDisplayed)
        {
            string rowID = tblname + row.ToString();
            string result = @"$(" + tblname + @").append('<tr id=" + rowID + @">";
            string actions = "";
            string type = "";
            string F = "";
            int count = 0;


            foreach (string D in datatype)
            {
                string colID = rowID + count.ToString();
                if (D.Substring(0, 1) == "A")//Its an dynamic action type, break it down
                {
                    F = D.Substring(D.IndexOf(":") + 1);
                    type = D.Substring(0, D.Length - F.Length - 1);
                }
                else
                {
                    type = D;
                }
                if (D.Substring(0, 3) == "ddl")
                {
                    string raw = D.Substring(4);
                    string selections = @"<td><select id=" + data[count] + @">";
                    int idx = 0;

                    while (raw.IndexOf(':') != -1)
                    {
                        idx = raw.IndexOf(':');
                        string sel = raw.Substring(0, idx);
                        selections = selections + @"<option value=" + sel + @">" + sel + @"</option>";
                        idx++;
                        raw = raw.Substring(idx);
                    }
                    selections = selections + @"</select></td> ";
                    result = result + selections;
                }
                switch (type)
                {
                    case "rdate":
                        result = result + @"<td>" + data[count] + @"</td>";
                        break;
                    case "wdate":
                        result = result + @"<td><input id=" + tblname + @"R" + row + "C" + count + @" type=text value=" + data[count] + @"></input></td>";
                        break;
                    case "rcheckbox":
                        if (Convert.ToBoolean(data[count]) == true)
                        {
                            result = result + @"<td><input type=checkbox checked=true readonly=true id=" + tblname + @"R" + row + @"C" + count + "></input></td>";
                        }
                        else
                        {
                            result = result + @"<td><input type=checkbox readonly=true id=" + tblname + @"R" + row + @"C" + count + "></input></td>";
                        }
                        break;
                    case "label":
                        result = result + @"<td>" + data[count] + "</td>";
                        break;
                    case "rdecimal":
                        result = result + @"<td>" + data[count] + "</td>";
                        break;
                    case "rlargetext":

                        if (data[count].Length > 50)
                        {
                            result = result + @"<td id=td" + colID + @"><input class=btnshrinkcss type=button value=__ id=btnshrink" + colID + @" onclick=shrinktxt(" + colID + @",btnexpand" + colID + @",btnshrink" + colID + @")><input class=btnexpandcss type=button value=... id=btnexpand" + colID + @" onclick=expandtxt(" + colID + @",btnexpand" + colID + @",btnshrink" + colID + @")></input><textarea readonly=true class=largetext1 id=" + colID + @">" + data[count] + @"</textarea></td>";
                            actions = actions + @" $(btnshrink" + colID + @").hide();";

                        }
                        else
                        {
                            result = result + @"<td><textarea class=largetext1 id=" + colID + @")>" + data[count] + @"</textarea></td>";
                        }

                        break;
                    case "rtime":
                        result = result + @"<td>" + data[count] + "</td>";
                        break;
                    case "Abutton"://send data contents (data[count])  to F
                        result = result + @"<td><button onclick=" + F + @"(" + data[count] + @")>" + data[count] + @"</button></td>";
                        break;

                }
                count++;
            }
            result = result + @"</tr>'); " + actions;

            return result;
        }
        public string tblpager(int records, int MaxRowsDisplayed)
        {
            int remainingrecords = 0;
            string page = "";

            if (records > MaxRowsDisplayed)
            {
                page = @"pager(" + records + @"," + MaxRowsDisplayed + @",'Start');";
            }
            else
            {
                remainingrecords = records;
                page = @"$(PagerInfo).append('Viewing Records 1- '" + records + @" of " + records + @");";
            }

            return page;
        }
        public string inputform(string divname, string[] datatype, string[] dataname, int[] DispO)
        {
            string result = @"$(" + divname + @").append('";
            string actions = "";
            int count = 0;
            string udataname = "";
            string udata = "";


            foreach (string D in datatype)
            {
                udataname = udataname + divname + dataname[DispO[count]] + @".value,";

                if (datatype[DispO[count]].Substring(0, 4) == "pass")//static value
                {
                    result = result + @"<input type=text title=" + dataname[DispO[count]] + @" id=" + divname + dataname[DispO[count]] + @">";
                    actions = actions + @"$(" + divname + dataname[DispO[count]] + @").val('" + dataname[DispO[count]] + @"'); ";
                }

                if (datatype[DispO[count]].Substring(0, 3) == "ddl")//dropdown list
                {
                    string raw = datatype[DispO[count]].Substring(4);
                    string selections = dataname[DispO[count]] + @": <select id=" + divname + dataname[DispO[count]] + @">";
                    int idx = 0;
                    bool done = false;
                    string sels = "";
                    string selname = "";// data[DispO[count]].Substring(0, data[DispO[count]].IndexOf('*'));
                    string selvalue = "";// data[DispO[count]].Substring(data[DispO[count]].IndexOf('*') + 1);
                    string savedvalue = selvalue;
                    while (!done)
                    {

                        idx = raw.IndexOf(':');
                        if (idx > 0) sels = raw.Substring(0, idx);
                        else
                        {
                            sels = raw;
                            done = true;
                        }
                        //sels=raw.Substring(0,idx);
                        selvalue = sels.Substring(sels.IndexOf('*') + 1);
                        selname = sels.Substring(0, sels.IndexOf('*'));
                        selections = selections + @"<option value=" + selvalue + @">" + selname + @"</option>";
                        idx++;
                        raw = raw.Substring(idx);
                    }
                    selections = selections + @"</select> ";
                    result = result + selections;
                }
                switch (datatype[DispO[count]])
                {
                    case "DateTime":
                        result = result + dataname[DispO[count]] + @": <input type=text id=" + divname + dataname[DispO[count]] + @">";
                        actions = actions + @"$(function(){
           $('#" + divname + dataname[DispO[count]] + @"').datepicker({
           changeMonth: true,
           changeYear: true
           });
           });";
                        actions = actions + @" $(" + divname + dataname[DispO[count]] + @").width(80);";
                        break;

                    case "Decimal":
                        result = result + dataname[DispO[count]] + @": <input type=text title=" + dataname[DispO[count]] + @" id=" + divname + dataname[DispO[count]] + @">";

                        break;
                    case "largetext":
                        result = result + @"</input><textarea class=largetext3 id=" + divname + dataname[DispO[count]] + @"></textarea>";
                        break;
                    case "number":
                        result = result + dataname[DispO[count]] + @": <input type=text title=" + dataname[DispO[count]] + @" id=" + divname + dataname[DispO[count]] + @">";
                        break;
                    case "Time":
                        result = result + dataname[DispO[count]] + @": <input type=text title=" + dataname[DispO[count]] + @" id=" + divname + dataname[DispO[count]] + @">";
                        break;
                    case "String":
                        result = result + dataname[DispO[count]] + @": <input type=text title=" + dataname[DispO[count]] + @" id=" + divname + dataname[DispO[count]] + @">";
                        break;
                    case "RString":
                        result = result + dataname[DispO[count]] + @": <input type=text title=" + dataname[DispO[count]] + @" id=" + divname + dataname[DispO[count]] + @" readonly=true" + @">";
                        break;
                    case "Int32":
                        result = result + dataname[DispO[count]] + @": <input type=text title=" + dataname[DispO[count]] + @" id=" + divname + dataname[DispO[count]] + @">";
                        break;
                    case "RInt32":
                        result = result + dataname[DispO[count]] + @": <input readonly=true type=text title=" + dataname[DispO[count]] + @" id=" + divname + dataname[DispO[count]] + @">";
                        break;
                    case "Boolean":
                        result = result + dataname[DispO[count]] + @": <input type=checkbox title=" + dataname[DispO[count]] + @" id=" + divname + dataname[DispO[count]] + @">";
                        break;
                    case "hide":
                        result = result + @"<input type=text title=" + dataname[DispO[count]] + @" id=" + divname + dataname[DispO[count]] + @">";
                        actions = actions + @" $(" + divname + dataname[DispO[count]] + @").hide();";

                        break;
                    case "PhoneNumber":
                        result = result + dataname[DispO[count]] + @": <input type=text title=" + dataname[DispO[count]] + @" id=" + divname + dataname[DispO[count]] + @">";
                        break;
                    case "ZipCode":
                        result = result + dataname[DispO[count]] + @": <input type=text title=" + dataname[DispO[count]] + @" id=" + divname + dataname[DispO[count]] + @">";
                        break;

                }
                count++;
            }

            udataname = udataname.Substring(0, udataname.Length - 1);//get rid of last delimiter
            result = result + @"<button id=btnSaveUpdate type=button>SAVE</button><button id=btnCancelUpdate type=button>CANCEL</button>'); ";
            actions = actions + @" $(btnCancelUpdate).click(function(){
$.post('DRCCustomer/SaveUpdateForm',{divname:'" + divname + @"',data:'Cancel'});

}); 
            $(btnSaveUpdate).click(function(){
var " + divname + @"UpdateArray=new Array(" + udataname + @");
for (var i = 0; i < " + divname + @"UpdateArray.length; i++) {
                   if (" + divname + @"UpdateArray[i] == '') " + divname + @"UpdateArray[i] = '0';
               }
var senddata = " + divname + @"UpdateArray.join('▓');
$.post('DRCCustomer/SaveUpdateForm', { divname:'" + divname + @"',data: senddata });

});
";

            result = result + actions + udata;
            return result;

        }
        public string UpdateForm(string divname, string[] data, string[] datatype, string[] dataname, int[] DispO)
        {
            string result = @"$(" + divname + @").append('";
            string actions = "";
            int count = 0;
            string udataname = "";
            string udata = "";


            foreach (string D in datatype)
            {
                udataname = udataname + divname + dataname[DispO[count]] + @".value,";
                if (datatype[DispO[count]].Substring(0, 3) == "ddl")//Dropdown List
                {
                    string raw = datatype[DispO[count]].Substring(4);
                    string selections = dataname[DispO[count]] + @": <select id=" + divname + dataname[DispO[count]] + @"  tabindex=" + DispO[count] + @">";
                    int idx = 0;
                    string sel = "";
                    bool done = false;
                    string selname = "";
                    string selvalue = "0";
                    string savedvalue = "0";
                    try
                    {
                        selname = data[DispO[count]].Substring(0, data[DispO[count]].IndexOf('*'));
                        selvalue = data[DispO[count]].Substring(data[DispO[count]].IndexOf('*') + 1);

                    }
                    catch
                    {
                        selname = "Unknown";
                        selvalue = "";
                    }//There is no existing data to add to the selection list add unknown
                    savedvalue = selvalue;
                    selections = selections + @"<option value=" + selvalue + @">" + selname + @"</option>";


                    while (!done)
                    {
                        idx = raw.IndexOf(':');
                        if (idx > 0) sel = raw.Substring(0, idx);
                        else
                        {
                            sel = raw;
                            done = true;
                        }
                        selvalue = sel.Substring(sel.IndexOf('*') + 1);
                        selname = sel.Substring(0, sel.IndexOf('*'));
                        //add selection if not the current value
                        if (selvalue != savedvalue) selections = selections + @"<option value=" + selvalue + @">" + selname + @"</option>";
                        idx++;
                        raw = raw.Substring(idx);


                    }
                    selections = selections + @"</select> ";
                    result = result + selections;
                }
                switch (datatype[DispO[count]])
                {
                    case "DateTime":
                        result = result + dataname[DispO[count]] + @": <input type=text id=" + divname + dataname[DispO[count]] + @" value=" + data[DispO[count]] + @">";
                        actions = actions + @"$(function(){
           $('#" + divname + dataname[DispO[count]] + @"').datepicker({
           changeMonth: true,
           changeYear: true
           });
           });";
                        actions = actions + @" $(" + divname + dataname[DispO[count]] + @").width(80);";
                        break;

                    case "Decimal":
                        result = result + dataname[DispO[count]] + @": <input type=text title=" + dataname[DispO[count]] + @" id=" + divname + dataname[DispO[count]] + @" value=" + data[DispO[count]] + @">";
                        int DWidth = data[DispO[count]].Length * 8;
                        actions = actions + @" $(" + divname + dataname[DispO[count]] + @").width(" + DWidth + @");";
                        break;
                    case "largetext":
                        if (data[DispO[count]].Length > 50)
                        {
                            result = result + @"<br/>" + dataname[DispO[count]] + @": <input class=btnshrinkcss type=button value=__ id=btnshrink" + divname + dataname[DispO[count]] + @" onclick=shrinktxtlong(" + divname + dataname[DispO[count]] + @",btnexpand" + divname + dataname[DispO[count]] + @",btnshrink" + divname + dataname[DispO[count]] + @")><input class=btnexpandcss type=button value=... id=btnexpand" + divname + dataname[DispO[count]] + @" onclick=expandtxtlong(" + divname + dataname[DispO[count]] + @",btnexpand" + divname + dataname[DispO[count]] + @",btnshrink" + divname + dataname[DispO[count]] + @")></input><textarea class=largetext3 id=" + divname + dataname[DispO[count]] + @">" + data[DispO[count]] + @"</textarea>";
                        }
                        else
                        {
                            result = result + dataname[DispO[count]] + @": <textarea class=largetext1 id=" + divname + dataname[DispO[count]] + @" onclick=expandtxt(" + divname + dataname[DispO[count]] + @")>" + data[DispO[count]] + @"</textarea>";
                        }
                        break;
                    case "number":
                        result = result + dataname[DispO[count]] + @": <input type=text title=" + dataname[DispO[count]] + @" id=" + divname + dataname[DispO[count]] + @" value=" + data[DispO[count]] + @">";
                        break;
                    case "Time":
                        result = result + dataname[DispO[count]] + @": <input type=text title=" + dataname[DispO[count]] + @" id=" + divname + dataname[DispO[count]] + @" value=" + data[DispO[count]] + @">";
                        break;
                    case "String":
                        result = result + dataname[DispO[count]] + @": <input type=text title=" + dataname[DispO[count]] + @" id=" + divname + dataname[DispO[count]] + @" value=" + data[DispO[count]] + @">";
                        break;
                    case "RString":
                        result = result + dataname[DispO[count]] + @": <input type=text title=" + dataname[DispO[count]] + @" id=" + divname + dataname[DispO[count]] + @" readonly=true value=" + data[DispO[count]] + @">";
                        break;
                    case "Int32":
                        result = result + dataname[DispO[count]] + @": <input type=text title=" + dataname[DispO[count]] + @" id=" + divname + dataname[DispO[count]] + @" value=" + data[DispO[count]] + @">";
                        break;
                    case "RInt32":
                        result = result + dataname[DispO[count]] + @": <input readonly=true type=text title=" + dataname[DispO[count]] + @" id=" + divname + dataname[DispO[count]] + @" value=" + data[DispO[count]] + @">";
                        int FWidth = data[DispO[count]].Length * 8;
                        actions = actions + @" $(" + divname + dataname[DispO[count]] + @").width(" + FWidth + @");";
                        break;
                    case "Boolean":
                        result = result + dataname[DispO[count]] + @": <input type=checkbox title=" + dataname[DispO[count]] + @" id=" + divname + dataname[DispO[count]] + @" value=" + data[DispO[count]] + @">";
                        break;
                    case "hide":
                        result = result + @"<input type=text title=" + dataname[DispO[count]] + @" id=" + divname + dataname[DispO[count]] + @" value=" + data[DispO[count]] + @">";
                        actions = actions + @" $(" + divname + dataname[DispO[count]] + @").hide();";
                        break;
                    case "PhoneNumber":
                        result = result + dataname[DispO[count]] + @": <input type=text title=" + dataname[DispO[count]] + @" id=" + divname + dataname[DispO[count]] + @" value=" + data[DispO[count]] + @">";
                        break;
                    case "ZipCode":
                        result = result + dataname[DispO[count]] + @": <input type=text title=" + dataname[DispO[count]] + @" id=" + divname + dataname[DispO[count]] + @" value=" + data[DispO[count]] + @">";
                        break;

                }
                count++;
            }
            udataname = udataname.Substring(0, udataname.Length - 1);//get rid of last delimiter
            result = result + @"<button id=btnSaveUpdate type=button>SAVE</button><button id=btnCancelUpdate type=button>CANCEL</button>'); ";
            actions = actions + @" $(btnCancelUpdate).click(function(){
$.post('DRCCustomer/SaveUpdateForm',{divname:'" + divname + @"',data:'Cancel'});

}); 
            $(btnSaveUpdate).click(function(){
var " + divname + @"UpdateArray=new Array(" + udataname + @");
for (var i = 0; i < " + divname + @"UpdateArray.length; i++) {
                   if (" + divname + @"UpdateArray[i] == '') " + divname + @"UpdateArray[i] = '0';
               }
var senddata = " + divname + @"UpdateArray.join('▓');
$.post('DRCCustomer/SaveUpdateForm', { divname:'" + divname + @"',data: senddata });

});
";

            result = result + actions + udata;
            return result;
        }
        public string cleanup(string divname)
        {
            string result = "";
            switch (divname)
            {
                case "divUpdateJob":
                    result = @" $(divUpdateJob).hide(); $(btnNewJobLog).hide(); $(divUpdateJob).empty(); $(btnDeleteJob).hide();";
                    break;
                case "divNewJobLog":
                    result = @" $(btnNewJobLog).show(); $(btnManualLogEntry).hide(); $(btnUseTimer).hide();  $(btnNewJobLogCancel).hide(); $(divNewJobLog).hide(); $(divNewJobLog).empty();  $(btnDeleteJob).show(); $(btnAddEquipment).show(); ";
                    break;
                case "divNewJobDetails":
                    result = @" $(btnNewJob).show(); $(divNewJobDetails).hide(); $(divNewJobDetails).empty(); $(divRequestTables).empty(); ";
                    break;
                case "divNewCustomer":
                    result = @" $(divNewCustomer).empty(); $(divNewCustomer).hide();";
                    break;
            }
            return result;
        }
        public string replaceCR(string text)
        {
            string fixedtext = "";
            if (text.IndexOf('\r') > -1)
            {
                int idx = text.IndexOf('\r');
                string ft = text.Substring(0, idx - 1);
                string bt = text.Substring(idx + 2);
                fixedtext = "test";//ft + bt;

            }
            else fixedtext = text;
            return fixedtext;
        }
        //public string contactdll(int AcctID)
        //{
        //    DRC_Contact[] contactlist = DRCrepos.ContactList(AcctID);
        //    string result = @":" + "Unknown*0";//adds Unknown to list
        //    foreach (var T in contactlist)
        //    {
        //        result = result + @":" + T.FirstName + @" " + T.LastName + @"*" + T.ContactID;
        //    }
        //    return result;
        //}
        //public string JobCodedll(int CodeType)
        //{
        //    JobCode[] jobcodelist = DRCrepos.getJobCodes(CodeType);
        //    string result = "";
        //    foreach (var J in jobcodelist)
        //    {
        //        result = result + @":" + J.Code + @"*" + J.CodeID;
        //    }
        //    return result;
        //}
        public string fixbackslash(string text)
        {
            //When VS combines string variables to make a new string, it strips the escape characters off string variables containing them. They need to be in for javascript to display properly. This adds the extra slashes so that when they are stripped, the required amount remains.
            string result = "";
            if (text.IndexOf('\\') > 0)
            {
                
                for (int i = 0; i < text.Length-1; i++)
                {
                    string testtext = text.Substring(i, 1);//Note dthat \\ is treated as one character by Substring
                    if (testtext == "\\") testtext= testtext + @"\";
                    result = result + testtext;
                }
            }
            else { result = text; }
            
            return result;
        }
        public string fixtic(string text)
        {
            //When VS combines string variables to make a new string, it strips the escape characters off string variables containing them. They need to be in for javascript to display properly. This adds the extra slashes so that when they are stripped, the required amount remains.
            string result = "";
            if (text.Contains("'"))
            {

                for (int i = 0; i < text.Length - 1; i++)
                {
                    string testtext = text.Substring(i, 1);
                    if (testtext == "'") testtext = testtext + @"+'''+'";
                    result = result + testtext;
                }
            }
            else { result = text; }

            return result;
        }
        public string fixstring2(string text)
        {
            string result = "";
            
            
                
              
                
                        result = result + @"'+String.fromCharCode(34)";
                    
           
            return result;
        }
        public string fixstring(string text)
        {

            string fixedtext = fixapost(text);
            fixedtext = fixquotes(fixedtext);
            fixedtext = replaceCR(fixedtext);
            fixedtext = fixbackslash(fixedtext);
           // fixedtext = fixtic(fixedtext);
            return fixedtext;
        }
        public string javapost(string post, string pars)
        {
            string result = @" $.post('" + post + @"',{" + pars + @"});";
            return result;
        }
//        public string buildtable(string div, string tblname, string tblclass, string data, int key, bool Flag1, bool Flag2, bool Flag3)
//        {
//            int count = 0;
//            string F = "";
//            string actions = "";
//            string firstthings = "";
//            string result = @"$(" + div + @").append('<table id=" + tblname + @" class=" + tblclass + @"><tr>";
//            string emptyset = "No Records Found";
//            switch (data)
//            {
//                #region JOBS TABLE
//                case "JOBS":
//                    if (Flag3) emptyset = "There are no incomplete jobs for any customers";
//                    if (Flag2) emptyset = "There are no incomplete jobs for this customer";
//                    if (Flag1) emptyset = "There are no completed jobs for this customer";
//                    if (Flag1 && Flag2) emptyset = "There are no jobs for this customer";
//                    List<JobRequest> J = DRCrepos.JobRequestList(key, Flag1, Flag2, Flag3);
//                    if (J.Count > 0)
//                    {
//                        firstthings = @"
//                        
//
//";

//                        actions = actions + @" var jobtbl=" + tblname + @"; ";
//                        string[] headers = { "Job:JobNumber", "RDate:RequestDate", "P:Priority", "Status:Status", "Description", "Requester:RequestedBy", "ATech:AssignedTechnician", "C:Completed", "CDate:DateCompleted", "OSR:OverallStatisfactionRating" };

//                        foreach (string H in headers)
//                        {
//                            string T = "";
//                            string Head = "";
//                            if (H.IndexOf(':') > -1)
//                            {
//                                T = H.Substring(H.IndexOf(':') + 1);
//                                Head = H.Substring(0, H.IndexOf(':'));
//                            }
//                            else
//                            {
//                                T = H;
//                                Head = H;
//                            }
//                            result = result + @"<th title=" + T + @">" + Head + @"</th>";
//                        }

//                        foreach (var job in J)
//                        {
//                            result = result + @"<tr id=Row" + tblname + job.JobNumber + @"><td><input type=button id=btn" + job.JobNumber + @" onclick=JobSel(Row" + tblname + job.JobNumber + @"," + job.JobNumber + @") value=" + job.JobNumber + @"></td>";
//                            // F = F + @" function " + tblname + job.JobNumber + @"() {";
//                            //  result = result + @"<tr><td style=color:black;font-weight:900>" + job.JobNumber + @"</td>";
//                            result = result + @"<td id=tdcompdate" + job.JobNumber + @">" + job.RequestDate.Value.ToShortDateString() + @"</td>";

//                            result = result + @"<td>" + DRCrepos.ConvertCode("jobcodes", Convert.ToInt32(job.Priority)) + DS.makeddl("PRIORITY", job.Priority.ToString(), "JobPriority" + job.JobNumber.ToString(), "", 20, 100, 400, 55) + @"</td>";
//                            //DS.makeTBLddlInputPair("STATELIST", "JobPriority" + job.JobNumber.ToString(), "", "", "", "", 0, 5) + @"</td>";

//                            actions = actions + @" $(JobPriority" + job.JobNumber.ToString() + @").hide();";

//                            result = result + @"<td>" + DRCrepos.ConvertCode("jobcodes", Convert.ToInt32(job.Status)) + DS.makeddl("STATUS", job.Status.ToString(), "JobStatus" + job.JobNumber.ToString(), "", 20, 100, 400, 55) + @"</td>";
//                            string colID = "7";
//                            result = result + @"<td id=td" + count + colID + @"><input class=btnshrinkcss type=button value=__ id=btns" + count + colID + @" onclick=shrinktxt(T" + job.JobNumber + @",btne" + count + colID + @",btns" + count + colID + @")><input class=btnexpandcss type=button value=... id=btne" + count + colID + @" onclick=expandtxt(T" + job.JobNumber + @",btne" + count + colID + @",btns" + count + colID + @")></input><textarea class=largetext1 id=T" + job.JobNumber.ToString() + @">" + fixstring(job.Description) + @"</textarea></td>";

//                            if (job.Description.Length > 50)
//                            {

//                                actions = actions + @" $(btns" + count + colID + @").hide(); ";
//                            }
//                            else
//                            {
//                                actions = actions + @" $(btns" + count + colID + @").hide(); $(btne" + count + colID + @").hide();";
//                            }

//                            result = result + @"<td>" + DRCrepos.ConvertCode("contact", Convert.ToInt32(job.RequestedBy)) + "</td>";
//                            result = result + @"<td>" + DRCrepos.ConvertCode("contact", Convert.ToInt32(job.AsignedTech)) + "</td>";//13 is the company acctID
//                            if (job.Completed == true)
//                            {
//                                result = result + @"<td><input type=checkbox checked=true readonly=true id=Complete" + job.JobNumber + @"></input></td>";
//                            }
//                            else
//                            {
//                                result = result + @"<td><input type=checkbox readonly=true id=Complete" + job.JobNumber + @"></input></td>";
//                            }
//                            string Cdate = "0";
//                            if (job.DateCompleted != null) Cdate = job.DateCompleted.Value.ToShortDateString();
//                            result = result + @"<td>" + Cdate + "</td>";

//                            result = result + @"<td>" + DRCrepos.ConvertCode("jobcodes", Convert.ToInt32(job.SatisfactionRating)) + @"</td>";

//                            count++;
//                        }

//                        actions = actions + @" function smsov" + tblname + @"(rowbtn) {$(rowbtn).css({'background-color':'yellow'});} function smsout" + tblname + @"(rowbtn) {$(rowbtn).css({'background-color':'#D9D9D9'});} function cmsov" + tblname + @"(rowbtn) {$(rowbtn).css({'background-color':'red'});} function cmsout" + tblname + @"(rowbtn) {$(rowbtn).css({'background-color':'#D9D9D9'});}
//
//    function jrowedit(rowbtn,selectedjob){}
////                        {if (!(lockbuttons))
////                               {lockbuttons=true; JobID=selectedjob; $(rowbtn).append ('<td><input type=button onmouseover=smsov" + tblname + @"(SaveRow" + tblname + @") onmouseout=smsout" + tblname + @"(SaveRow" + tblname + @") id=SaveRow" + tblname + @" value=SAVE onclick=PostRow()>
////                                <input type=button onmouseover=cmsov" + tblname + @"(CancelSaveRow" + tblname + @") onmouseout=cmsout" + tblname + @"(CancelSaveRow" + tblname + @") id=CancelSaveRow" + tblname + @" value=CANCEL onclick=CancelPostRow()></td>'); 
////                                $(rowbtn).css({'background-color':'LightBlue','border':'10px solid DeepSkyBlue'});             SelectedRowID=rowbtn;}
////                        
////$(btnNewJobLog).show(); $(btnDeleteJob).show(); $(btnAddEquipment).show(); $(btnNewJob).hide(); $(btnViewCompleted).hide(); $(btnViewIncomplete).hide();  var rowcount=0;
////                        for (r in " + tblname + @".rows) 
////                            {$(" + tblname + @".rows[rowcount]).hide(); rowcount++;}
////                        $(rowbtn).show(); $(DRCmenu).hide(); $(CustmenuContainer).hide(); var jcd=SelectedRowID.cells[1].innerHTML; SelectedRowID.cells[1].innerHTML='<input type=text id=SelectedJobReqDate style=width:65px>'; SelectedRowID.cells[1].firstChild.value=jcd;
////                         $(function(){
////                           $('#SelectedJobReqDate').datepicker({
////                           changeMonth: true,
////                           changeYear: true
////                           });
////                           });}
//// 
////
////    function PostRow() {$.post('/DRCCustomer/savereqtable',{JobID: SelectedRowID.cells[0].firstChild.value, ReqDate:SelectedRowID.cells[1].firstChild.value, priority:SelectedRowID.cells[2].firstChild.value, status:SelectedRowID.cells[3].firstChild.value, descript:SelectedRowID.cells[4].childNodes[3].value, Requester:SelectedRowID.cells[5].firstChild.value, ATech:SelectedRowID.cells[6].firstChild.value, complete:SelectedRowID.cells[7].firstChild.checked, CompltDate:SelectedRowID.cells[8].firstChild.value, OSR:SelectedRowID.cells[9].firstChild.value}); $(SelectedRowID).css({'background-color':'White','border':'0px solid DeepSkyBlue'}); $(SaveRow" + tblname + @").remove(); $(CancelSaveRow" + tblname + @").remove(); lockbuttons=false; $(btnNewJobLog).hide(); $(btnDeleteJob).hide(); $(btnAddEquipment).hide(); $(btnNewJob).show(); $(btnViewCompleted).show(); $(btnViewIncomplete).show();   for (r in " + tblname + @".rows) {$(" + tblname + @".rows[rowcount]).hide(); rowcount++;} $(DRCmenu).show(); $(CustmenuContainer).show();}
//// 
////    function CancelPostRow(){$(SelectedRowID).css({'background-color':'White','border':'0px solid DeepSkyBlue'}); $(SaveRow" + tblname + @").remove(); $(CancelSaveRow" + tblname + @").remove(); lockbuttons=false; $(btnNewJobLog).hide(); $(btnDeleteJob).hide(); $(btnAddEquipment).hide(); $(btnNewJob).show(); $(btnViewCompleted).show(); $(btnViewIncomplete).show();  var rowcount=0; for (r in " + tblname + @".rows) {$(" + tblname + @".rows[rowcount]).show(); rowcount++;} $(divJobLogs).empty(); $(DRCmenu).show(); $(CustmenuContainer).show(); $(" + tblname + @").remove(); $.post('/DRCCustomer/getJobRequests', { CustID:PCustID , Complete: Complete, InComplete: InComplete, All: All });} 
//
//
//
//
//
//";

//                        result = result + @"</table>'); JobTableName=" + tblname + @"; " + actions + F;
//                    }
//                    else
//                    {
//                        result = result + @"<td>" + emptyset + @".<tr></table>'); ";//JobTableName=" + tblname + @"; ";
//                    }
//                    break;
//                #endregion

//                #region JOB LOGS TABLE
//                case "JOBLOGS":
//                    List<JobLog> jl = DRCrepos.JobLogList(key);
//                    if (jl.Count > 0)
//                    {
//                        string[] headers = { "Log:LogNumber", "IN:JobInvoiceStatus", "ST:ServiceTagNumber", "EDate:EntryDate", "CoStaff:Person who worked", "TypeS:TypeofService", "LR:TimerRunning", "TimeIn", "TimeOut", "WH:WorkHours", "BH:BillingHours" };

//                        foreach (string H in headers)
//                        {
//                            string T = "";
//                            string Head = "";
//                            if (H.IndexOf(':') > -1)
//                            {
//                                T = H.Substring(H.IndexOf(':') + 1);
//                                Head = H.Substring(0, H.IndexOf(':'));
//                            }
//                            else
//                            {
//                                T = H;
//                                Head = H;
//                            }
//                            result = result + @"<th style=background-color:#CC99FF title=" + T + @">" + Head + @"</th>";
//                        }

//                        foreach (JobLog jbl in jl)
//                        {
//                            result = result + @"<tr id=Row" + tblname + jbl.LogEntryNumber + @"><td>" + jbl.LogEntryNumber + @"</td>";
//                            if (jbl.Invoiced == true)
//                            {
//                                result = result + @"<td><input type=checkbox checked=true readonly=true id=Complete" + jbl.LogEntryNumber + @"></input></td>";
//                            }
//                            else
//                            {
//                                result = result + @"<td><input type=checkbox readonly=true id=Complete" + jbl.LogEntryNumber + @"></input></td>";
//                            }
//                            result = result + @"<td>" + jbl.ServiceTag + @"</td>";
//                            result = result + @"<td>" + jbl.EntryDate.Value.ToShortDateString() + @"</td>";

//                            result = result + @"<td>" + DRCrepos.ConvertCode("contact", Convert.ToInt32(jbl.TechName)) + @" </td>";//13 is the company acctID
//                            result = result + @"<td>" + DRCrepos.ConvertCode("service", Convert.ToInt32(jbl.TypeOfService)) + @"</td>";

//                            if (jbl.LogRunning == true)
//                            {
//                                result = result + @"<td><input type=checkbox checked=true readonly=true id=Complete" + jbl.LogEntryNumber + @"></input></td>";
//                            }
//                            else
//                            {
//                                result = result + @"<td><input type=checkbox readonly=true id=Complete" + jbl.LogEntryNumber + @"></input></td>";
//                            }

//                            result = result + @"<td>" + jbl.TimeIn.Value.ToShortTimeString() + @"</td>";
//                            // actions = actions + @"function forcetime(timefield){if $(timefield).value.length<1){}";
//                            result = result + @"<td>" + jbl.TimeOut.Value.ToShortTimeString() + @"</td>";

//                            result = result + @"<td>" + jbl.HoursWorked + @"</td>";

//                            result = result + @"<td>" + jbl.HoursBilled + @"</td></tr>";
//                            result = result + @"</tr><td colspan=11><B>ENTRY:</B> " + fixstring(jbl.Entry) + @"</td></tr>";
//                            result = result + @"</tr><td colspan=11><B>SIDENOTE:</B> " + fixstring(jbl.SideNote) + @"</td>";

//                            //string colID = "4";
//                            //result = result + @"<input class=btnshrinkcss type=button id=btns" + count + colID + @" onclick=shrinktxt(T" + jbl.LogEntryNumber + colID + @",btne" + count + colID + @",btns" + count + colID + @")><input class=btnexpandcss type=button id=btne" + count + colID + @" onclick=expandtxt(T" + jbl.LogEntryNumber + colID + @",btne" + count + colID + @",btns" + count + colID + @")></input><textarea class=largetext1 id=T" + jbl.LogEntryNumber + colID + @">" + fixstring(jbl.Entry) + @"</textarea>";
//                            //if (jbl.Entry.Length > 50)
//                            //{

//                            //    actions = actions + @" $(btns" + count + colID + @").hide(); ";
//                            //}
//                            //else
//                            //{
//                            //    actions = actions + @" $(btns" + count + colID + @").hide(); $(btne" + count + colID + @").hide();";
//                            //}
//                            //colID = "5";
//                            //result = result + @"<input class=btnshrinkcss type=button value=__ id=btns" + count + colID + @" onclick=shrinktxt(T" + jbl.LogEntryNumber + colID + @",btne" + count + colID + @",btns" + count + colID + @")><input class=btnexpandcss type=button id=btne" + count + colID + @" onclick=expandtxt(T" + jbl.LogEntryNumber + colID + @",btne" + count + colID + @",btns" + count + colID + @")></input><textarea class=largetext1 id=T" + jbl.LogEntryNumber + colID + @">" + fixstring(jbl.SideNote) + @"</textarea>";
//                            //if (jbl.SideNote.Length > 50)
//                            //{

//                            //    actions = actions + @" $(btns" + count + colID + @").hide(); ";
//                            //}
//                            //else
//                            //{
//                            //    actions = actions + @" $(btns" + count + colID + @").hide(); $(btne" + count + colID + @").hide();";
//                            //}

//                            count++;
//                        }
//                        actions = actions + @"function jlrowedit(rowbtn,selectedlog) {if (!(lockbuttons)){lockbuttons=true; JobID=selectedlog; $(rowbtn).append('<td><input type=button onmouseover=smsov" + tblname + @"(SaveRow" + tblname + @") onmouseout=smsout" + tblname + @"(SaveRow" + tblname + @") id=SaveRow" + tblname + @" value=SAVE onclick=PostRow()><input type=button onmouseover=cmsov" + tblname + @"(CancelSaveRow" + tblname + @") onmouseout=cmsout" + tblname + @"(CancelSaveRow" + tblname + @") id=CancelSaveRow" + tblname + @" value=CANCEL onclick=CancelPostRow()></td>'); $(rowbtn).css({'background-color':'LightBlue','border':'10px solid DeepSkyBlue'}); SelectedRowID=rowbtn;} $(btnNewJobLog).show(); $(btnDeleteJob).show(); $(btnAddEquipment).show(); $(btnNewJob).hide(); $(btnViewCompleted).hide(); $(btnViewIncomplete).hide();  } ";

//                        result = result + @"</table>'); LogTableName=" + tblname + @";" + actions + F;

//                    }
//                    else
//                    {
//                        result = result + @"<td>No logs exist for request number" + key + @"</td><td><input value=CLOSE type=button onclick=emptydiv(" + div + @")></td><tr></table>'); LogTableName='0'; $(" + div + @").empty(); ";
//                    }
//                    break;
//                #endregion

//                #region AcctList Table
//                case "AcctList":

//                    List<Account> BAS = DRCrepos.AccountList(key);
//                    if (BAS.Count > 0)
//                    {
//                        string[] headers = { "AcctID:AccountNumber", "Name:AccountName", "Balance:AccountBalance" };

//                        foreach (string H in headers)
//                        {
//                            string T = "";
//                            string Head = "";
//                            if (H.IndexOf(':') > -1)
//                            {
//                                T = H.Substring(H.IndexOf(':') + 1);
//                                Head = H.Substring(0, H.IndexOf(':'));
//                            }
//                            else
//                            {
//                                T = H;
//                                Head = H;
//                            }
//                            result = result + @"<th style=background-color:#CC99FF title=" + T + @">" + Head + @"</th>";
//                        }
//                        foreach (var acct in BAS)
//                        {
//                            result = result + @"<tr id=Row" + tblname + acct.AccountID + @"><td><a href=../../DRCBanking/Custinfo/?AcctID=" + acct.AccountID + @" target=_blank>" + acct.AccountID + @"</a></td>";
//                            result = result + @"<td>" + fixstring(acct.Name) + @"</td>";
//                            decimal bal = Math.Round(acctbal(acct.AccountID, key), 2);
//                            if (bal > 0)
//                            {
//                                result = result + @"<td>$" + bal + @"</td>";
//                            }
//                            else if (bal < 0) result = result + @"<td>$(" + Convert.ToDecimal(bal.ToString().Substring(1)) + @")</td>";
//                            else result = result + @"<td>" + bal + @"</td></tr>";
//                        }
//                        actions = actions + @" function acctsel(a,aid){$.post('/DRCBanking/BankAcctBtnClick',{acctID:aid}); } ";
//                        result = result + @"</table>'); " + actions;
//                    }
//                    else result = "No Accounts Exist";

//                    break;
//                #endregion

//                #region BankTransfer Table
//                case "BANKTRANSFER":
//                    result = @"$(" + div + @").prepend('<table id=" + tblname + @"><tr>";//override result opening here to prepend

//                    result = result + @"<th title=Transfer Date >Date</th><th style=background-color:#CC99FF title=Transfer Funds From Account>Transfer From Account</th><th style=background-color:#CC99FF title=Transfer To Account>Transfer To Account</th><th title=Transfer Method >Transfer Method</th> <th title=Check or Reference Number>Reference</th><th title=Amount To Transfer >Amount</th><th></th></tr>";
//                    result = result + @"<td><input type=text id=TransferDate" + tblname + @" style=width:65px></td>";
//                    result = result + @"<td></td>";

//                    result = result + @"<td></td>";

//                    result = result + @"<td ></td>";

//                    result = result + @"<td><input type=text id=TRReference" + tblname + @"></input></td>";
//                    result = result + @"<td></td>";
//                    result = result + @"<td><input type=button id=save" + tblname + @" onmouseover=smsov(save" + tblname + @") onmouseout=smsout(save" + tblname + @") onclick=savetransfer() value=POST></input><input type=button id=cancel" + tblname + @" onclick=canceltransfer() onmouseover=cmsov(cancel" + tblname + @") onmouseout=cmsout(cancel" + tblname + @") value=CANCEL></td></tr>";
//                    result = result + @"</table>');";
//                    actions = actions + @" $(function(){
//                           $('#TransferDate" + tblname + @"').datepicker({
//                           changeMonth: true,
//                           changeYear: true
//                          })});";
//                    actions = actions + @" function savetransfer(){
//
//valdate($(TransferDate" + tblname + @").val());  if(tdate){$(TransferDate" + tblname + @").css('background-color','white');} else{ $(TransferDate" + tblname + @").css('background-color','red'); alert('Please enter a valid date.'); $(TransferDate" + tblname + @").css('background-color','#FFCCCC'); $(TransferDate" + tblname + @").focus();}
//
//valcreddebaccts($(ddlSourceAccount).val(),$(ddlTargetAccount).val());
//if(taccteql){alert('Transfer source and target accts can not be the same'); $(ddlSourceAccount).css('background-color','red'); $(ddlTargetAccount).css('background-color','red');} else{ $(ddlSourceAccount).css('background-color','white'); $(ddlTargetAccount).css('background-color','white');}
//
//valmoney($(TRAmount" + tblname + @").val()); if (tamt){$(TransferAmt).css('background-color','white');}else{$(TransferAmt).css('background-color','red'); alert('Please enter valid amount for transaction ($0.00)'); $(TransferAmt).focus();}
//
//valfield($(TRReference" + tblname + @").val()); if(tfield){$(TRReference" + tblname + @").css('background-color','white');} else{$(TRReference" + tblname + @").css('background-color','red'); alert('Reference Number Required');}
//
//if(tdate && !(taccteql) && tamt && tfield){var TA=$(ddlTargetAccount).val().substr(0,$(ddlTargetAccount).val().indexOf('♫')); var SA=$(ddlSourceAccount).val().substr(0,$(ddlSourceAccount).val().indexOf('♫')); var agree=confirm('Transfer ' +$(TRAmount" + tblname + @").val()+' from Acct '+SA+' to Acct '+TA+' on '+$(TransferDate" + tblname + @").val()); 
//        if(agree==true){$.post('/DRCBanking/PostTransfer', { Tdate:$(TransferDate" + tblname + @").val(),TSource:SA,Ttarget:TA,Tref:$(TRReference" + tblname + @").val(),Tamount:$(TransferAmt).val(),TMeth:PayMethod,TSaccttype:$(ddlSourceAccount).val().substr($(ddlSourceAccount).val().indexOf('♫')+1),TTaccttype:$(ddlTargetAccount).val().substr($(ddlTargetAccount).val().indexOf('♫')+1)});} else{alert('Transfer Canceled');} }
//
//}
//    ";
//                    actions = actions + @" function canceltransfer(){IsOnAcctXfrs=false; $(btnAcctXfrs).css({'color':'yellow'}); $(TransferAmt).remove();}
//    ";

//                    result = result + actions;
//                    break;
//                #endregion

//                #region Deposit Table

//                case "DEPOSIT":
//                    List<DRC_DepositTable> todeposit = DRCrepos.GetOpenDeposits();
//                    result = @"$(" + div + @").prepend('<table id=" + tblname + @"><tr>";//override result opening here to prepend

//                    result = result + @"<th title=Received Date style=background-color:#3399FF;color:white>Date Received</th><th title=Received Document Number style=background-color:#3399FF;color:white>RcvDocNum</th><th style=background-color:#3399FF;color:white title=Received From>Received From:</th><th style=background-color:#3399FF;color:white title=Amount Received>Amount</th></tr>";
//                    foreach (var item in todeposit)
//                    {
//                        result = result + @"<tr>";
//                        result = result + @"<td>" + item.FinancialDateReceived.Value.ToShortDateString() + @"</td>";
//                        result = result + @"<td>" + item.FromDocumentID.Value.ToString() + @"</td>";
//                        result = result + @"<td>" + fixstring(AcctName(item.FromAccountID.Value)) + @"</td>";
//                        result = result + @"<td><input id=damt" + item.DepositID.ToString() + @" type=button onclick=addtodeposit(" + item.Amount + @",damtremove" + item.DepositID + @",damt" + item.DepositID.ToString() + @"," + item.DepositID + @") value=$" + item.Amount + @"><input id=damtremove" + item.DepositID.ToString() + @" type=button style=background-color:green;color:white  onclick=removefromdeposit(" + item.Amount + @",damtremove" + item.DepositID + @",damt" + item.DepositID.ToString() + @"," + item.DepositID + @") value=$" + item.Amount + @"></td>";
//                        result = result + @"</tr>";
//                        actions = actions + @" $(damtremove" + item.DepositID.ToString() + @").hide(); ";
//                    }



//                    result = result + @"</table>'); " + actions;
//                    break;

//                #endregion

//                default:
//                    break;
//            }

//            string expantions = @"
//                                function expandtxt(cellID, expandbtn, shrinkbtn) {
//               var L = ($(cellID).val()).length;
//               var H = 25;
//               while (L>50) {
//                   H = H + 20;
//                   L = L - 50;
//               }
//               if (H > 200) { H = 200 };
//               $(cellID).addClass('largetext2');
//               $(cellID).removeClass('largetext1');
//               $(cellID).focus();
//               $(expandbtn).hide();
//               $(shrinkbtn).show();
//               $(cellID).height(H);
//           }
//
//           function shrinktxt(cellID,expbtn, shrbtn) {
//               $(cellID).removeClass('largetext2');
//               $(cellID).addClass('largetext1');
//               $(shrbtn).hide();
//               $(expbtn).show();
//               $(cellID).height(50);
//           }
//
//           function expandtxtlong(cellID, expandbtn, shrinkbtn) {
//               
//               $(cellID).width(800);
//               $(cellID).focus();
//               $(expandbtn).hide();
//               $(shrinkbtn).show();
//               $(cellID).height(400);
//           }
//
//           function shrinktxtlong(cellID, expbtn, shrbtn) {
//               
//               $(shrbtn).hide();
//               $(expbtn).show();
//               $(cellID).height(100);
//           }
//
//
//";
//            // result = @" function closetable(tbl){$(tbl).remove();} function emptydiv(d){$(d).empty();} " + result + actions;
//            return result + expantions;
//        }
        //public decimal acctbal(int AcctID, int AcctType)
        //{
        //    decimal result = 0;
        //    List<DRC_GeneralLedger> Adebits = DRCrepos.BankDebits(AcctID);
        //    List<DRC_GeneralLedger> Acredits = DRCrepos.BankCredits(AcctID);

        //    decimal Tcredits = (from A in Acredits select A.Amount).Sum();
        //    decimal Tdebits = (from A in Adebits select A.Amount).Sum();

        //    switch (AcctType)
        //    {
        //        case 3://Bank Account
        //            result = Tdebits - Tcredits;
        //            break;

        //        case 4://Credit Card
        //            result = Tcredits - Tdebits;
        //            break;

        //        case 20://Cash Account
        //            result = Tdebits - Tcredits;
        //            break;

        //        default:
        //            result = Tcredits - Tdebits;
        //            break;
        //    }


        //    return result;
        //}
        public List<string> MakeStringList(string delimitedtext, char delimiter)
        {
            List<string> resultlist = new List<string>();
            int L = delimitedtext.Length;
            string[] SA = delimitedtext.Split(delimiter);
            foreach (string item in SA)
            {
                resultlist.Add(item);
            }
            return resultlist;
        }
        //public string EditTable(string div, string tblname, List<string> data)
        //{
        //    string result = @" $(" + div + @").append('<table id=" + tblname + @">";
        //    int row = 0;
        //    int cell = 0;
        //    if (data.Count < 1) { result = @"alert('No Data!')"; }
        //    else
        //    {
        //        foreach (string record in data)
        //        {

        //            result = result + @"<tr id=" + tblname + @"Row" + row + @">";
        //            string[] cells = record.Split('♫');
        //            foreach (string cel in cells)
        //            {
        //                result = result + @"<td><input id=" + tblname + @"Row" + row + @"Cell" + cell + @" type=text value=" + cel + @"/></td>";
        //                cell++;
        //            }
        //            result = result + @"</tr>";
        //            row++;
        //        }

        //        result = result + @"</table>'); ";
        //    }
        //    return result;
        //}
        //public string makeItemNumber(int ItemT, int ItemM)
        //{
        //    string result = "";
        //    int i = 0;
        //    int man = 0;
        //    int part = 0;
        //    bool filledgap = false;
        //    List<int> ItemPart = new List<int>();
        //    var ItemTypes = from it in db.DRC_ItemInvAccountTables where it.PartType == ItemT select it;

        //    foreach (var itm in ItemTypes)
        //    {
        //        if (itm.Man == man)
        //        {
        //            ItemPart.Add(itm.PartType);
        //        }
        //    }
        //    if (ItemPart.Count > 0)
        //    {
        //        foreach (int im in ItemPart)
        //        {
        //            if (im != i)
        //            {
        //                part = i;
        //                filledgap = true;
        //            }
        //            i++;

        //        }
        //        if (!(filledgap)) part = i;
        //    }
        //    else part = 0;

        //    int l = 4 - ItemT.ToString().Length;
        //    int c = 0;
        //    while (c < l) { result = result + "0"; c++; }
        //    result = result + ItemT.ToString() + @"-";
        //    l = 4 - ItemM.ToString().Length;
        //    c = 0;
        //    while (c < l) { result = result + "0"; c++; }
        //    result = result + ItemM.ToString() + @"-";
        //    l = 4 - part.ToString().Length;
        //    c = 0;
        //    while (c < l)
        //    {
        //        result = result + "0";
        //        c++;
        //    }
        //    result = result + part.ToString();
        //    return result;
        //}
        //public string checkfordupName(string TM, int AT)
        //{
        //    var check = from names in db.Accounts where names.Name == TM && names.AccountType == AT select names;

        //    string result = TM + @" already exists as a customer.";

        //    if (check.Count() > 0)
        //        foreach (var item in check)
        //        {
        //            result = result + @" ," + item;
        //        }
        //    else result = "";

        //    return result;
        //}
        //public CustVendEmpInfo makeCustVendEmpInfo(int type, int AcctID)
        //{
        //    DRC_AccountInfoTable Acctinfo = DRCrepos.getAcctInfo(DRCrepos.getAcctInfoID(AcctID));
        //    Account Acct = DRCrepos.getAccount(AcctID);
        //    CustVendEmpInfo info = new CustVendEmpInfo();
        //    info.Name = Acct.Name; info.Phone = Acctinfo.BusinessTelephoneNumber; info.Fax = Acctinfo.BusinessFaxNumber; info.Website = Acctinfo.WebPage; info.Street = Acctinfo.BusinessAddressStreet; info.Suite = Acctinfo.BusinessAddressSuite; info.City = Acctinfo.BusinessAddressCity; info.State = Acctinfo.BusinessAddressState; info.Zip = Acctinfo.BusinessAddressPostalCode; info.Country = Acctinfo.BusinessAddressCountry; info.Terms = Acct.Terms; info.TaxCode = Convert.ToInt32(Acctinfo.TaxCode); info.TaxLocation = Acctinfo.TaxLocation; info.CVEType = type; info.Email = Acctinfo.EmailAddress1; info.Notes = Acctinfo.Notes; info.PhoneExt = info.PhoneExt;
        //    return info;
        //}
        //public TaxInfo makeTaxInfo(int TaxCodeID)
        //{
        //    DRC_TaxCode TaxCode = DRCrepos.getTaxCode(TaxCodeID);
        //    TaxInfo tax = new TaxInfo();
        //    tax.CurrentRate = Convert.ToDouble(TaxCode.CurrentRate);
        //    tax.Description = TaxCode.Description;
        //    tax.Name = TaxCode.Name;
        //    tax.TCodeID = TaxCode.TaxCodeID;
        //    tax.Ttype = Convert.ToInt32(TaxCode.TaxType);
        //    tax.TVendID = Convert.ToInt32(TaxCode.TaxVendorID);
        //    tax.TxID = Convert.ToInt32(TaxCode.TaxID);
        //    if (tax.TVendID > 0)
        //    {
        //        Account TaxVend = DRCrepos.getAccount(tax.TVendID);
        //        tax.TaxVendName = TaxVend.Name;
        //        tax.TaxVendRate = Convert.ToDouble(TaxVend.InterestRate);
        //        tax.CombinedTax = tax.CurrentRate + tax.TaxVendRate;
        //    }
        //    else
        //    {
        //        tax.TaxVendName = "Non-Taxable";
        //        tax.TaxVendRate = 0;
        //        tax.CombinedTax = 0;
        //    }


        //    tax.TaxMultiplyer = 1 + tax.CombinedTax;
        //    tax.TaxPercent = tax.CombinedTax * 1000;

        //    return tax;
        //}
        //public int checkdupBldgName(string txt, int BldgID, int WHID)
        //{
        //    int? WareHouseID = 0;
        //    if (WHID < 1)
        //    {
        //        WareHouseID = (from W in db.DRC_ItemLocations where W.BuildingID == BldgID select W).Single().WarehouseID;
        //    }
        //    else
        //    {
        //        WareHouseID = WHID;
        //    }
        //    int test = Convert.ToInt32(WareHouseID);
        //    List<int?> BIDS = (from L in db.DRC_ItemLocations where L.WarehouseID == WareHouseID select L.BuildingID).ToList();

        //    try
        //    {
        //        foreach (int B in BIDS)
        //        {
        //            var bldg = (from BT in db.DRC_ItemBuildings where B == BT.BuildingID select BT).Single();
        //            if (bldg.Name == txt && bldg.BuildingID != BldgID)
        //            {
        //                test = 0;

        //            }
        //        }
        //    }
        //    catch { }//exception thrown because only one test keeps original value
        //    return test;

        //}
        //public int checkdupRoomName(string txt, int RoomID, int BID)
        //{
        //    int? BldgID = 0;
        //    if (BID < 1)
        //    {
        //        BldgID = (from R in db.DRC_ItemLocations where R.RoomID == RoomID select R).Single().BuildingID;
        //    }
        //    else
        //    {
        //        BldgID = BID;
        //    }
        //    int test = Convert.ToInt32(BldgID);
        //    List<int?> RIDS = (from L in db.DRC_ItemLocations where L.BuildingID == BldgID select L.RoomID).ToList();
        //    try
        //    {
        //        foreach (int R in RIDS)
        //        {
        //            var room = (from RT in db.DRC_ItemRooms where R == RT.RoomID select RT).Single();
        //            if (room.Name == txt && room.RoomID != RoomID)
        //            {
        //                test = 0;

        //            }
        //        }
        //    }
        //    catch { }//exception thrown because only one test keeps original value
        //    return test;
        //}
        //public int checkdupBinName(string txt, int BinID, int RID)
        //{
        //    int? RoomID = 0;
        //    if (RID < 1)
        //    {
        //        RoomID = (from B in db.DRC_ItemLocations where B.BinID == BinID select B).Single().RoomID;
        //    }
        //    else
        //    {
        //        RoomID = RID;
        //    }
        //    int test = Convert.ToInt32(RoomID);
        //    List<int?> BIDS = (from L in db.DRC_ItemLocations where L.RoomID == RoomID select L.BinID).ToList();
        //    try
        //    {
        //        foreach (int B in BIDS)
        //        {
        //            var bin = (from BT in db.DRC_ItemBins where B == BT.BinID select BT).Single();
        //            if (bin.Name == txt && bin.BinID != BinID)
        //            {
        //                test = 0;

        //            }
        //        }
        //    }
        //    catch { }//exception thrown because only one test keeps original value
        //    return test;
        //}
        public bool validatenonzeroANDnonnegetiveint(int I)
        {
            if ((Math.Abs(I) == I * -1) || (I == 0)) return false;
            else return true;
        }
        public bool validateItemLocation(int W, int B, int R, int Bin)
        {
            if (validatenonzeroANDnonnegetiveint(W))
            {
                if (validatenonzeroANDnonnegetiveint(B))
                {
                    if (validatenonzeroANDnonnegetiveint(R))
                    {
                        if (validatenonzeroANDnonnegetiveint(Bin))
                        {
                            return true;
                        }
                        else return false;

                    }
                    else return false;

                }
                else return false;
            }
            else return false;
        }
        public int GetKeyTypeIDFromTypeName(string KeyTypeName)
        {
            var check = from names in db.KeyTypes where names.KeyTypeName == KeyTypeName select names;

            int result = 0;

            if (check.Count() > 0)
                foreach (var item in check)
                {
                    result = item.KeyTypeID;
                }

            return result;
        }
        public int SaveNewKeyType(string KeyTypeName)
        {

            int result = 0;
            KeyType NewKeyType = new KeyType();

            try
            {
                NewKeyType.KeyTypeName = KeyTypeName;
                db.KeyTypes.InsertOnSubmit(NewKeyType);
                db.SubmitChanges();
            }
            catch (Exception)
            {
                result = -1;
            }
            if (result == 0)//Save succeeded get Key Type ID
            {
                result = GetKeyTypeIDFromTypeName(KeyTypeName);
            }

            return result;
        }
        public int GetKeyIDFromTypeNamePair(int KeyTypeID, string KeyName)
        {
            var check = from keys in db.KEYs where keys.KeyName == KeyName && keys.KeyTypeID == KeyTypeID select keys;

            int result = 0;

            if (check.Count() > 0)
                foreach (var item in check)
                {
                    result = item.KeyID;
                }

            return result;
        }
        public int SaveNewKey(int KeyTypeID, string KeyName)
        {
            int result = 0;
            KEY NewKey = new KEY();

            try
            {
                NewKey.KeyTypeID = KeyTypeID;
                NewKey.KeyName = KeyName;
                db.KEYs.InsertOnSubmit(NewKey);
                db.SubmitChanges();
            }
            catch (Exception)
            {
                result = -1;
            }
            if (result == 0)//Save succeeded get KeyID
            {
                result = GetKeyIDFromTypeNamePair(KeyTypeID, KeyName);
            }

            return result;
        }
        public int SaveBook(BOOKDATA newbook)
        {
            int result = 0;
            try
            {
                Book savebook = new Book { Title = newbook.Title, BookAuthorID = newbook.BookAuthorID, PublisherID = newbook.PublisherID, Pages = newbook.Pages, ISBN = newbook.ISBN, HardBack = newbook.HardBack, LocID = newbook.LocID, OwnerID = newbook.OwnerID, BookCost = newbook.Cost, BookPrice = newbook.Price, BookApprasial = newbook.Appraisal };
                db.Books.InsertOnSubmit(savebook);
                db.SubmitChanges();
                result = savebook.BookID;
            }
            catch { result = -1; }

            return result;
        }
        public int GetBookTitleDuplicateCount(string title)
        {

            var books = from b in db.Books where b.Title == title select b;
            int result = books.Count();
            return result;
        }
        public List<Book> getBooks()
        {
            return (from B in db.Books select B).ToList();
        }
        public string GetBookPubNameFromID(int? PubID)
        {
            return (from P in db.BookPubs where P.BookPubID == PubID select P).Single().Name;
        }
        public string GetLocationLabelFromID(int? LocID)
        {
            string result;
            try
            {
                result = (from L in db.Locations where L.LocID == LocID select L).Single().LocLabel;
            }
            catch
            {
                result = "";
            }
            return result;
        }
        public string GetOwnerNameFromID(int? OwnerID)
        {
            return (from O in db.Owners where O.OwnerID == OwnerID select O).Single().Name;
        }
        public string GetMediaTypeFromID(int? MediaTypeID)
        {
            return (from Med in db.MediaTypes where Med.MediaTypeID == MediaTypeID select Med).Single().MediaTypeName;
        }
        public List<string> GetContents(string type, int ParentID)
        {
            List<string> Content = new List<string>();
            switch (type)
            {
                case "SOFTWARE":
                    Content= (from C in db.SoftwareContents where C.SoftwareID == ParentID select C.Title).ToList();
                    break;
                default:
                    break;
            }
            return Content;
        }
        public List<Software> GetSoftwareData()
        {
            return (from S in db.Softwares select S).ToList();
        }
        public int GetSoftTitleDuplicateCount(string title)
        {

            var soft = from s in db.Softwares where s.Title == title select s;
            int result = soft.Count();
            return result;
        }
        public int SaveSoft(SOFTDATA newsoft)
        {
            int result = 0;
            try
            {
                Software savesoft = new Software { Title = newsoft.Title, PublisherID = newsoft.PublisherID, MediaTypeID = newsoft.MediaTypeID, ISBN = newsoft.ISBN, LocID = newsoft.LocID, OwnerID = newsoft.OwnerID, Cost = newsoft.Cost, Price = newsoft.Price, Appraisal = newsoft.Appraisal };
                db.Softwares.InsertOnSubmit(savesoft);
                db.SubmitChanges();
                result = savesoft.SoftwareID;
            }
            catch { result = -1; }

            return result;
        }
        public int GetSoftContentDuplicateCount(int SoftID, string Title)
        {
            var softc = from sc in db.SoftwareContents where sc.Title == Title && sc.SoftwareID==SoftID select sc;
            int result = softc.Count();
            return result;
        }
        public int SaveSoftContent(SoftwareContent newsftcontent)
        {
            int result = 0;
            try
            {
                db.SoftwareContents.InsertOnSubmit(newsftcontent);
                db.SubmitChanges();
                result = newsftcontent.ContentID;
            }
            catch
            {
                result = -1;
            }

            return result;
        }
        public string GetSoftNameFromID(int SoftID)
        {
            return (from Soft in db.Softwares where Soft.SoftwareID == SoftID select Soft).Single().Title;
        }
        public List<Item> GetItemData(string Order)
        {
            List<Item> result = new List<Item>();
            switch (Order)
            {
               case "ID":
                    result=(from I in db.Items orderby I.ItemID select I).ToList();
                    break;
                case "NAME":
                    result = (from I in db.Items orderby I.ItemName select I).ToList();
                    break;
                case "TYPE":
                    result = (from I in db.Items orderby I.ItemTypeID select I).ToList();
                    break;
                case "MANUFACTURER":
                    result = (from I in db.Items orderby I.ItemManufacturerID select I).ToList();
                    break;
                case "UPC":
                    result = (from I in db.Items orderby I.UPC select I).ToList();
                    break;
                case "DESCRIPTION":
                    result = (from I in db.Items orderby I.ItemDescription select I).ToList();
                    break;
                case "QUANTITY":
                   
                    break;
                case "NEW":
                    result = (from I in db.Items orderby I.ItemNew select I).ToList();
                    break;
                case "TESTED":
                    result = (from I in db.Items orderby I.ItemTested select I).ToList();
                    break;
                case "LOCATION":
                    //List<ItemLoc> L = (from loc in db.ItemLocs orderby loc.LocID select loc).ToList();
                    
                    //foreach (ItemLoc item in L)
                    //{
                    //    result.Add((from I in db.Items where I.ItemID=item.
                    //}
                    break;
                case "OWNER":
                    result = (from I in db.Items orderby I.OwnerID select I).ToList();
                    break;
                case "COST":
                    result = (from I in db.Items orderby I.ItemCost select I).ToList();
                    break;
                case "PRICE":
                    result = (from I in db.Items orderby I.ItemPrice select I).ToList();
                    break;
                case "APPRAISAL":
                    result = (from I in db.Items orderby I.ItemApprasial select I).ToList();
                    break;
            }
            return result;
        }
        public string GetItemTypeFromID(int? TypeID)
        {
            string ItypeName = "None";
            try
            {
                ItypeName=(from T in db.ItemTypes where T.ItemTypeID == TypeID select T).Single().Name;
            }
            catch 
            {
                //Nothing, just leave ItypeName as None
                
            }
            return ItypeName;
        }
        public int getItemTypeIDFromItemID(int ItemID)
        {
            return (from I in db.Items where I.ItemID == ItemID select I.ItemTypeID).Single();
        }
        public string GetManufacturerFromID(int? ManID)
        {
            string man = "Unknown";
            try
            {
                man=(from MAN in db.ItemManufacturers where MAN.MaunufacturerID== ManID select MAN).Single().Name;
            }
            catch 
            {
                //Nothing, just leave man as Unknown
                
            }
            return man;
        }
        public int GetItemNameDuplicateCount(string Name)
        {

            var its = from i in db.Items where i.ItemName == Name select i;
            int result = its.Count();
            return result;
        }
        public decimal CalcItemQuantity(int ItemID)
        {
            decimal sum = 0;
            var isum = from i in db.ItemLocs where i.ItemID == ItemID select i;
            foreach (var q in isum)
            {
                sum = sum + q.Quantity;
            }
            return sum;
        }
        public int GetOwnerDuplicateCount(string Name)
        {

            var own = from o in db.Owners where o.Name == Name select o;
            int result = own.Count();
            return result;
        }
        public int GetManufacturerDuplicateCount(string Name)
        {

            var man = from mn in db.ItemManufacturers where mn.Name == Name select mn;
            int result = man.Count();
            return result;
        }
        public int GetLocTypeDuplicateCount(string Name)
        {

            var lt = from t in db.LocationTypes where t.Name == Name select t;
            int result = lt.Count();
            return result;
        }
        public int GetLocationDuplicateCount(string Name, int LocType)
        {

            var loc = from l in db.Locations where l.LocLabel == Name && l.LocType==LocType select l;
            int result = loc.Count();
            return result;
        }
        #region Items
        public int SaveItem(ITEMDATA newitem)
        {
            int result = 0;
            try
            {
                Item saveitem = new Item { ItemName = newitem.Name, ItemTypeID = newitem.ItemTypeID, ItemManufacturerID = newitem.ManufacturerID, UPC = newitem.UPC, ItemDescription = newitem.Description,ItemNew=newitem.ItemNew,ItemTested=newitem.ItemTested, OwnerID = newitem.OwnerID, ItemCost = newitem.Cost, ItemPrice = newitem.Price, ItemApprasial = newitem.Appraisal,Serialized=newitem.Serialized };
                db.Items.InsertOnSubmit(saveitem);
                db.SubmitChanges();
                result = saveitem.ItemID;
            }
            catch { result = -1; }
            if (newitem.Quantity > 0 && newitem.LocID > 0)//quantity of over zero must have a location with it to be saved
            {
                try
                {
                    SaveQuantityLoc(newitem.ItemID, newitem.LocID, newitem.Quantity);
                }
                catch { result = -2; }
            }
            
            return result;
        }
        public int SaveItemTypeAccounts(int ItemTypeID,int ExpAcct, int RevAcct,int InvAcct, int COGAct)
        {
            ItemTypeRevExp newrec=new ItemTypeRevExp{ItemTypeID=ItemTypeID,ExpAcctID=ExpAcct,RevAcctID=RevAcct,InvAcctID=InvAcct,CoGAcctID=COGAct};
            db.ItemTypeRevExps.InsertOnSubmit(newrec);
            db.SubmitChanges();

            return newrec.ItemTypeRevExpID;

        }
        public int SaveQuantityLoc(int ItemID, int LocID, decimal Quantity)
        {
            int result = 0;
            if (ItemID > 0 && LocID > 0)
            {
                try
                {
                    ItemLoc IL = new ItemLoc { ItemID = ItemID, LocID = LocID, Quantity = Quantity };
                    db.ItemLocs.InsertOnSubmit(IL);
                    db.SubmitChanges();
                    result = IL.ItemLocID;
                }
                catch { }//just leave result at 0
            }
            return result;
        }
        public bool UpdateItemLocQ(int ItemLocID, decimal Q)
        {
            bool result = true;
            try
            {
                ItemLoc saveq = (from ilq in db.ItemLocs where ilq.ItemLocID == ItemLocID select ilq).Single();
                saveq.Quantity = Q;
                db.SubmitChanges();
            }
            catch { result = false; }
            return result;
        }
        public bool ItemEditSave(ITEMDATA updateitem)
        {
            bool result = true;
            try
            {
                Item edited = (from e in db.Items where e.ItemID == updateitem.ItemID select e).Single();
                edited.ItemName = updateitem.Name; edited.ItemTypeID = updateitem.ItemTypeID; edited.ItemManufacturerID = updateitem.ManufacturerID; edited.UPC = updateitem.UPC; edited.ItemDescription = updateitem.Description; edited.ItemNew = updateitem.ItemNew; edited.ItemTested = updateitem.ItemTested; edited.OwnerID = updateitem.OwnerID; edited.ItemCost = updateitem.Cost; edited.ItemPrice = updateitem.Price; edited.ItemApprasial = updateitem.Appraisal;
                
                db.SubmitChanges();
                
            }
            catch
            {
                result = false;
            }
            return result;
        }
        public int GetItemTypeDuplicateCount(string Name)
        {

            var it = from i in db.ItemTypes where i.Name == Name select i;
            int result = it.Count();
            return result;
        }
        public string GetItemNameFromID(int ItemID)
        {
            string result = (from i in db.Items where i.ItemID == ItemID select i).Single().ItemName;
            return result;
        }
        public List<ItemLoc> GetItemQuantities(int ItemID)
        {
            return (from iq in db.ItemLocs where iq.ItemID==ItemID select iq).ToList();
        }
        public int SaveItemQuantity(ItemLoc newquan)
        {
            int result = 0;
            try
            {
                db.ItemLocs.InsertOnSubmit(newquan);
                db.SubmitChanges();
                result = newquan.ItemLocID;
            }
            catch { } //just keeps result at 0
            return result;
        }
        public bool ItemDelete(int ItemID)
        {
            bool result = true;
            try
            {
                Item deleteme = (from i in db.Items where i.ItemID == ItemID select i).Single();
                db.Items.DeleteOnSubmit(deleteme);
                db.SubmitChanges();
            }
            catch { result = false; }
            return result;
        }
        public int SaveItType(string Name, int RevAcct, int ExpAcct,int InvAcct, int COGAcct)
        {
            int result = 0;

            try
            {
                ItemType newitemtype = new ItemType { Name = Name };
                db.ItemTypes.InsertOnSubmit(newitemtype);
                db.SubmitChanges();
                result = newitemtype.ItemTypeID;
            }
            catch { result = -1; }
            if (result > 0) //Save account type designations
            {
                try
                {
                    int ItemTypeExpRevID = SaveItemTypeAccounts(result, ExpAcct, RevAcct,InvAcct,COGAcct);
                }
                catch
                {
                    result = -2;
                }
            }


            return result;
        }
        public string AllocateItems(string Items)
        {
            string result = @" alert('Item/s Allocated Successfully'); ";
            List<string> DATA = ParcJSdata(Items);
            List<ItemAllocation> AItems = ConvertToAllicationItems(DATA);
            foreach (ItemAllocation IA in AItems)
            {

                //get additional info

                switch (IA.AllocationType)
                {
                    case 1://Expense
                        TransLineItem TLI = GetLineItemFromID(IA.LineID);
                        
                        //Credit GoodsReceived(21) account and Debit Expence(IA.AllocationID) Account with transaction type of internal(11)
                       int TransID= SaveTransaction(21, 11, TLI.DocumentID, IA.AllocationID, "Moved From Receiving to Expense", TLI.Price);
                       if (TransID > 0)
                       {
                           if (!(ChangeLineAllocationToAllocated(TLI.LineID)))
                           {
                               result = @" alert('Line Allocation Toggle Failed'); ";
                           }
                          
                       }
                       else
                       {
                           result = @" alert('Item Expense Transaction Failed'); ";
                       }
                       
                        
                        break;
                    case 3://Inventory

                        break;
                    default:
                        break;
                }
            }

            return result;

        }
        public List<ItemAllocation> ConvertToAllicationItems(List<string> DATA)
        {
            List<ItemAllocation> result = new List<ItemAllocation>();
            foreach (string d in DATA)
            {
                int idx2 = 0;
                int LineID = Convert.ToInt32(d.Substring(idx2, d.IndexOf(",")));
                idx2 = idx2 + LineID.ToString().Length + 1;
                decimal AQuantity = Convert.ToDecimal(d.Substring(idx2, d.IndexOf(",", idx2) - idx2));
                idx2 = idx2 + AQuantity.ToString().Length + 1;
                int AllocationType = Convert.ToInt32(d.Substring(idx2, d.IndexOf(",", idx2) - idx2));
                idx2 = idx2 + AllocationType.ToString().Length + 1;
                int AllocationID = Convert.ToInt32(d.Substring(idx2));
                result.Add(new ItemAllocation { LineID = LineID, Quantity = AQuantity, AllocationType = AllocationType, AllocationID = AllocationID });
            }
            return result;
        }
        public List<ItemLoc> GetLocationItems(int LocID)
        {
            
            return (from I in db.ItemLocs where I.LocID == LocID select I).ToList();
        }
        public List<ITEMDATA> GetReceivedItems()
        {
            List<ITEMDATA> newitems = new List<ITEMDATA>();
            List<ItemLoc> items = GetLocationItems(55);//55 is always the receiving location
            foreach (ItemLoc I in items)
            {
                newitems.Add(new ITEMDATA {ItemID=I.ItemID,Name=GetItemNameFromID(I.ItemID),Quantity=I.Quantity});
            }

            return newitems;
        }
        public ITEMDATA GetReceivedItemFromItemID(int ItemID)
        {
            ITEMDATA result = new ITEMDATA();
            //55 is always the receiving location
            ItemLoc il = (from l in db.ItemLocs where l.ItemID == ItemID && l.LocID == 55 select l).Single();
            result.ItemID = ItemID;
            result.Quantity = il.Quantity;

            return result;
        }
        public List<ITEMDATA> GetUnAllocatedLineItems()
        {
            List<ITEMDATA> unitems = new List<ITEMDATA>();
            List<TransLineItem> items = (from i in db.TransLineItems where i.Allocated == false select i).ToList();
            foreach (TransLineItem li in items)
            {
                unitems.Add(new ITEMDATA { ItemID = li.ItemID, LineID = li.LineID, Quantity = li.Quantity, Price = li.Price,Name=GetItemNameFromID(li.ItemID) });
            }
            return unitems;
        }
        public bool RemoveItemQuantityFromLocation(int ItemID, decimal Quantity, int LocID)
        {
            bool result = false;
            string currentuser = System.Web.Security.Membership.GetUser().UserName;
            try
            {
                ItemLoc IL = (from i in db.ItemLocs where ItemID == i.ItemID && LocID == i.LocID select i).Single();
                if (IL.Quantity == Quantity)
                {
                    //Remove Record from ItemLocs
                    db.ItemLocs.DeleteOnSubmit(IL);
                    db.SubmitChanges();
                }
                else
                {
                    IL.Quantity = IL.Quantity - Quantity;
                    db.SubmitChanges();
                }
                result = true;
            }
            catch (Exception e)
            {
                Error newerror = new Error { ErrorMessage = e.Message, ErrorModule = "OUR THINGS--Classes--MyMethods.cs--SaveTransaction", ErrorTime = DateTime.Now, ErrorUser = currentuser, ErrorHelpLink = e.HelpLink, ErrorInnerException = e.InnerException.Message, ErrorSource = e.Source, ErrorStack = e.StackTrace, ModuleInput = @"ItemID:" + ItemID + @" Quantity:" + Quantity + @" LocID:" + LocID};
                db.Errors.InsertOnSubmit(newerror);
                db.SubmitChanges();
            }
            return result;
        }
        #endregion
        public int SaveOwner(string Name)
        {
            int result = 0;

            try
            {
                Owner newowner = new Owner { Name = Name };
                db.Owners.InsertOnSubmit(newowner);
                db.SubmitChanges();
                result = newowner.OwnerID;
            }
            catch { result = -1; }



            return result;
        }
        public int SaveManufacturer(string Name)
        {
            int result = 0;

            try
            {
                ItemManufacturer newmanufacturer = new ItemManufacturer { Name = Name };
                db.ItemManufacturers.InsertOnSubmit(newmanufacturer);
                db.SubmitChanges();
                result = newmanufacturer.MaunufacturerID;
            }
            catch { result = -1; }



            return result;
        }


        #region LOCATION**********************************************************************************************
        public int SaveLocType(string Name)
        {
            int result = 0;

            try
            {
                LocationType newloctype = new LocationType { Name = Name };
                db.LocationTypes.InsertOnSubmit(newloctype);
                db.SubmitChanges();
                result = newloctype.LocTypeID;
            }
            catch { result = -1; }



            return result;
        }
        public int SaveLocation(string Name, int LocType)
        {
            int result = 0;

            try
            {
                Location newlocation = new Location { LocType=LocType,LocLabel=Name };
                db.Locations.InsertOnSubmit(newlocation);
                db.SubmitChanges();
                result = newlocation.LocID;
            }
            catch { result = -1; }



            return result;
        }
        public LocationType GetOneLocType(int ID)
        {
            LocationType result = new LocationType();   
            var LocT = from l in db.LocationTypes select l;
            foreach (var item in LocT)
            {
                if (item.LocTypeID == ID) result = item;
            }

            return result;
        }
        public int AddLocationRecord(int Type, string Label, bool Removeable)
        {
            Location NL=new Location{LocLabel=Label,LocType=Type,Removable=Removeable};
            db.Locations.InsertOnSubmit(NL);
            db.SubmitChanges();
            int result = 0;
            result = NL.LocID;
            return result;
        }
        public int AddItemLocRecord(int ItemID, int LocID, decimal Q)
        {
            ItemLoc NIL = new ItemLoc { ItemID = ItemID, LocID = LocID, Quantity = Q };
            db.ItemLocs.InsertOnSubmit(NIL);
            db.SubmitChanges();
            int result = 0;
            result = NIL.ItemLocID;
            return result;
        }
        #endregion

        #region ACCOUNT************************************************************************************************
        public string SaveNewAccount(string AcctName, string AcctNumber, int AcctType)
        {
            string result = "";
            if (GetAcctNameDuplicateCount(AcctName) < 1)
            {
                try
                {
                    AccountTable newAccount = new AccountTable { AcctName = AcctName, AcctNumber = AcctNumber, AcctTypeID = AcctType };
                    db.AccountTables.InsertOnSubmit(newAccount);
                    db.SubmitChanges();
                    result = newAccount.AcctID.ToString();
                }
                catch (Exception e)
                {

                    result = e.Message + "*************" + e.InnerException + "**************" + e.Data;
                }
            }
            else result = AcctName + @" already exists!";
            return result;
        }
        public int GetAcctNameDuplicateCount(string Name)
        {

            var Accts = from a in db.AccountTables where a.AcctName == Name select a;
            int result = Accts.Count();
            return result;
        }
        public List<AccountTable> getAccounts(int AcctTypeID)
        {
            List<AccountTable> result = new List<AccountTable>();
            if (AcctTypeID<1)
            {
                result = (from a in db.AccountTables orderby a.AcctName select a).ToList();
            }
            else
            {
                result = (from a in db.AccountTables where a.AcctTypeID == AcctTypeID orderby a.AcctName select a).ToList();
            }
            
            return result;
        }
        public List<AccountTable> getAllAccounts()
        {
            return (from a in db.AccountTables select a).ToList();
        }
        public AccountTable getAccountFromID(int AcctID)
        {
            AccountTable result = new AccountTable();
            if (AcctID > 0)
            {
                result= (from a in db.AccountTables where a.AcctID == AcctID select a).Single();
            }
            return result;
        }
        public string getAccountTypeFromID(int AcctTypeID)
        {
            return (from at in db.AccountTypeTables where at.AcctTypeID == AcctTypeID select at).Single().AcctType;
        }
        public int getAccountTypeIDFromType(string AcctType)
        {
            return (from at in db.AccountTypeTables where at.AcctType == AcctType select at).Single().AcctTypeID;
        }
        public int GetAccountTypeIDFromAccountID(int AcctID)
        {
            return (from a in db.AccountTables where a.AcctID == AcctID select a).Single().AcctTypeID;
        }
        public string getPayableNameFromPurchaseDocument(int DocID)
        {
            string PayName = "";
            int PayAcct=0;
            List<TransactionTable> Trans = getTransactionsFromDocID(DocID);
            List<int> TransDebitAccts = new List<int>();
            foreach (TransactionTable T in Trans)//get possible payable accounts by using debit field
            {
                if (!(TransDebitAccts.Contains(T.CreditID))) TransDebitAccts.Add(T.CreditID);
            }
            //Find the account that has a type of payable (6)
            foreach (int AN in TransDebitAccts)
            {
                if (GetAccountTypeIDFromAccountID(AN) == 6) PayAcct = AN;
                if (PayAcct > 0)
                {
                    PayName = getAccountFromID(PayAcct).AcctName;
                    break;
                }
            }
            

            return PayName;
        }
        public List<AccountTypeTable> getAccountTypes()
        {
            return (from at in db.AccountTypeTables select at).ToList();
        }
        #endregion

        #region DOCUMENT***********************************************************************************************
        public int GetDocumentIDFromTransactionID(int TransID)
        {
            return (from t in db.TransactionTables where t.TransactionID == TransID select t).Single().DocumentID;
        }
        public Document GetDoc(int DocID)
        {
            return (from d in db.Documents where d.DocumentID == DocID select d).Single();
        }
        public DateTime GetDocumentDateFromID(int DocID)
        {
            return (from d in db.Documents where d.DocumentID == DocID select d).Single().DocumentTime;
        }
        public string GetDocumentReferenceFromID(int DocID)
        {
            return (from d in db.Documents where d.DocumentID == DocID select d).Single().DocumentReference;
        }
        public string GetDocumentPathFromID(int DocID)
        {
            return (from d in db.Documents where d.DocumentID == DocID select d).Single().DocumentPath;
        }
        public Document GetDocumentFromID(int DocID)
        {
            return (from d in db.Documents where d.DocumentID == DocID select d).Single();
        }
        public int getdoctypeIDFromDocID(int DocID)
        {
            return (from d in db.Documents where d.DocumentID == DocID select d).Single().DocumentTypeID;
        }
        public string GetDocTypeFromTypeID(int DocTypeID)
        {
            return (from dt in db.DocumentTypes where dt.DocumentTypeID == DocTypeID select dt).Single().DocType;
        }
        public List<Document> GetDocuments(int DocTypeID)
        {
            if (DocTypeID > 0)
            {
                return (from d in db.Documents where d.DocumentTypeID == DocTypeID select d).ToList();
            }
            else
            {
                return (from d in db.Documents select d).ToList();
            }
        }
        public int InsertDocument(int DocType, int ParentDocID, DateTime DocTime, string DocRef, string DocPath, decimal AMT,int PayMethID)
        {
            int DocID = 0;
            if (DocRef == "") DocRef = "*";
            MembershipUser currentuser = System.Web.Security.Membership.GetUser();
            Document newdoc = new Document { DocumentPath = DocPath, DocumentReference = DocRef, DocumentTime = DocTime, DocumentTypeID = DocType, EnteredBy = currentuser.UserName, ParentDocumentID = ParentDocID, TimeEntered = DateTime.Now,Amount=AMT,Reconciled=false,PaymentMethodID=PayMethID };
            try
            {
                db.Documents.InsertOnSubmit(newdoc);
                db.SubmitChanges();
                DocID = newdoc.DocumentID;
            }
            catch (Exception e)
            {
                
                Error newerror = new Error { ErrorMessage = e.Message, ErrorModule = "OUR THINGS--Classes--MyMethods.cs--InsertDocument", ErrorTime = DateTime.Now, ErrorUser = currentuser.UserName,ErrorHelpLink=e.HelpLink,ErrorInnerException=e.InnerException.Message,ErrorSource=e.Source,ErrorStack=e.StackTrace,ModuleInput=@"DocType:"+DocType+@" ParentDocID:"+ParentDocID+@" DocTime:"+DocTime+@" DocRef:"+DocRef+@" DocPath:"+DocPath+@" AMT:"+AMT};
                db.Errors.InsertOnSubmit(newerror);
                db.SubmitChanges();
            }
            return DocID;
        }
        public int InsertDocument(Document Doc)
        {
            int DocID = 0;
            MembershipUser currentuser = System.Web.Security.Membership.GetUser();
            Doc.EnteredBy = currentuser.UserName; Doc.TimeEntered = DateTime.Now;
            
            try
            {
                db.Documents.InsertOnSubmit(Doc);
                db.SubmitChanges();
                DocID = Doc.DocumentID;
            }
            catch (Exception e)
            {

                Error newerror = new Error { ErrorMessage = e.Message, ErrorModule = "OUR THINGS--Classes--MyMethods.cs--InsertDocument", ErrorTime = DateTime.Now, ErrorUser = currentuser.UserName, ErrorHelpLink = e.HelpLink, ErrorInnerException = e.InnerException.Message, ErrorSource = e.Source, ErrorStack = e.StackTrace, ModuleInput = @"DocType:" + Doc.DocumentTypeID + @" ParentDocID:" + Doc.ParentDocumentID + @" DocTime:" + Doc.DocumentTime + @" DocRef:" + Doc.DocumentReference + @" DocPath:" + Doc.DocumentPath + @" AMT:" + Doc.Amount };
                db.Errors.InsertOnSubmit(newerror);
                db.SubmitChanges();
            }
            return DocID;
        }
        public bool UpdateDocument(Document Doc)
        {
            bool result = false;
            try
            {
                db.SubmitChanges();
                result = true;
            }
            catch 
            {
                
                
            }
            return result;
        }
        public int SaveDocument(int DocID, int DocType, int ParentDoc, DateTime DocTime, decimal DocAmt, string DocRef, string DocPath)
        {
            int result = 0;
            string currentuser = System.Web.Security.Membership.GetUser().UserName;
            if (DocID > 0)//Update Doc
            {
                try
                {

                }
                catch (Exception e)
                {
                    Error newerror = new Error { ErrorMessage = e.Message, ErrorModule = "OUR THINGS--Classes--MyMethods.cs--SaveDocument--UpdateDocument", ErrorTime = DateTime.Now, ErrorUser = currentuser, ErrorHelpLink = e.HelpLink, ErrorInnerException = e.InnerException.Message, ErrorSource = e.Source, ErrorStack = e.StackTrace, ModuleInput = @"DocID:" + DocID + @" DocType:" + DocType + @" ParentDoc:" + ParentDoc + @" DocTime" + DocTime + @" DocAmt" + DocAmt + @" DocRef" + DocRef + @" DocPath" + DocPath };
                    db.Errors.InsertOnSubmit(newerror);
                    db.SubmitChanges();
                }
            }
            else//Insert new Doc
            {
                try
                {
                    Document newdoc = new Document { Amount = DocAmt, DocumentPath = DocPath, DocumentReference = DocRef, DocumentTime = DocTime, DocumentTypeID = DocType, ParentDocumentID = ParentDoc, EnteredBy = currentuser, TimeEntered = DateTime.Now };
                    db.Documents.InsertOnSubmit(newdoc);
                    db.SubmitChanges();
                    result = newdoc.DocumentID;
                }
                catch (Exception e)
                {
                    Error newerror = new Error { ErrorMessage = e.Message, ErrorModule = "OUR THINGS--Classes--MyMethods.cs--SaveDocument--New Document", ErrorTime = DateTime.Now, ErrorUser = currentuser, ErrorHelpLink = e.HelpLink, ErrorInnerException = e.InnerException.Message, ErrorSource = e.Source, ErrorStack = e.StackTrace, ModuleInput = @"DocID:" + DocID + @" DocType:" + DocType + @" ParentDoc:" + ParentDoc + @" DocTime" + DocTime + @" DocAmt" + DocAmt + @" DocRef" + DocRef + @" DocPath" + DocPath };
                    db.Errors.InsertOnSubmit(newerror);
                    db.SubmitChanges();
                }
            }
            return result;
        }
        public string saveDocPath(int DocID,string DocPath)
        {
            string result = @" alert('Document Path Saved'); ";
            
            try
            {
                Document ED = ED = (from d in db.Documents where d.DocumentID == DocID select d).Single();
                ED.DocumentPath = DocPath;
                db.SubmitChanges();
                
            }
            catch (Exception)
            {

                result = @" alert('Failed to Save Document Path!'); ";
            }
            return result;
        }
        public string DeleteDocument(int DocID)
        {
            string result = @" alert('Document " + DocID + @" deletion failed!'); ";
            string deltext = @" alert('Document " + DocID + @" deleted: "; 
            try
            {

                deltext = deltext + @" '); ";
                result = deltext;
            }
            catch (Exception e)
            {
                
                
            }

            

            return result;
        }
        public List<Document> getIncompletPayments()
        {
            
            DateTime SDate = Convert.ToDateTime(getAPstartDate()); DateTime EDate = Convert.ToDateTime(getAPendDate());
            return (from D in db.Documents where D.Reconciled == false && D.DocumentTypeID == 3 && D.DocumentTime>=SDate && D.DocumentTime<=EDate select D).ToList();

            
        }
        public PaymentDetail getPaymentDetails(int DocID)
        {
            PaymentDetail result = new PaymentDetail();
            //Assign payment doc which includes the initial payment amount
            result.PayDoc = (from D in db.Documents where D.DocumentID == DocID select D).Single();
            result.UnallocatedPayment = result.PayDoc.Amount;
            result.PDate = result.PayDoc.DocumentTime;
            //retreive all transactions (excluding Trans type 27 which is initial total payment received) related to the doc and adjust unallocated payment if any
            List<TransactionTable> Trans = getTransactionsFromDocID(DocID);
            
            foreach (TransactionTable TT in Trans)
            {
                if (TT.TransactionTypeID == 27)
                {
                    result.PayTrans = TT;
                    result.PaymentCreditID = TT.CreditID;
                    result.PaymentDebitID = TT.DebitID;
                    result.PaymentCreditName = getAccountFromID(TT.CreditID).AcctName;
                    result.PaymentDebitName = getAccountFromID(TT.DebitID).AcctName;
                }
                else
                {
                    result.PayAllocations.Add(TT);
                    result.UnallocatedPayment = result.UnallocatedPayment - TT.Amount;
                }
            }
            

            return result;
        }
        public List<InvoiceDetail> getIncompleteInvoicesByAcctID(int AcctID)
        {
            List<InvoiceDetail> result = new List<InvoiceDetail>();
            //get transaction of the type invoiceout(26) with AcctID in the Debit field
            List<TransactionTable> TS = (from T in db.TransactionTables where T.TransactionTypeID == 26 && T.DebitID == AcctID select T).ToList();
            //Pull all documents associated with the transactions if the document is unreconciled
            List<Document> Docs = new List<Document>();
            foreach (TransactionTable T in TS)
            {
                try
                {
                    Docs.Add((from DD in db.Documents where DD.DocumentID == T.DocumentID && DD.Reconciled == false select DD).Single());
                }
                    //Do nothing the doc is simply not added
                catch {

                }
            }
            
            foreach (Document D in Docs)
            {
                
                try
                {
                    List<TransactionTable> IT = (from T in db.TransactionTables where T.Description == D.DocumentReference select T).ToList();
                    decimal paidamount = (from T in db.TransactionTables where T.Description == D.DocumentReference select T.Amount).Sum();
                    decimal remainingamount = D.Amount - paidamount;
                    result.Add(new InvoiceDetail { DebitAcctID = AcctID, InvoiceAmount = D.Amount, InvoiceDocID = D.DocumentID, InvoiceNumber = D.DocumentReference,Payments=IT,UnpaidAmount=remainingamount });
                }
                catch { result.Add(new InvoiceDetail { DebitAcctID = AcctID, InvoiceAmount = D.Amount, InvoiceDocID = D.DocumentID, InvoiceNumber = D.DocumentReference,UnpaidAmount=D.Amount }); }
            }

            return result;
        }
        public InvoiceDetail getInvoiceDetail(Document D)
        {
            InvoiceDetail ID = new InvoiceDetail();
            ID.InvoiceAmount = D.Amount; ID.InvoiceDocID = D.DocumentID; ID.InvoiceNumber = D.DocumentReference;
            try
            {
                List<TransactionTable> IT = (from T in db.TransactionTables where T.Description == D.DocumentReference select T).ToList();
                decimal paidamount = (from T in db.TransactionTables where T.Description == D.DocumentReference select T.Amount).Sum();
                decimal remainingamount = D.Amount - paidamount;
                ID.DebitAcctID = IT[0].DebitID; ID.Payments = IT; ID.UnpaidAmount = remainingamount;
            }
            catch { ID.DebitAcctID = 0; ID.UnpaidAmount = D.Amount; }


            return ID;
        }
        public bool AllocatePayment(int PayDoc, int InvDoc)
        {
            bool result = true;

            try
            {
                Document ID = GetDocumentFromID(InvDoc);
                //Find what is left over to pay from payment
                PaymentDetail P = getPaymentDetails(PayDoc);
                InvoiceDetail I = getInvoiceDetail(ID);
                decimal differ = P.UnallocatedPayment - I.UnpaidAmount;
                List<TransLineItem> Lines = getUnallocatedLines(InvDoc);
                List<TaxCollection> taxes = getUncollectedTaxes(InvDoc);
                #region Payment large enough or larger

                if (differ>=0)//Payment is large enough or larger
                {
                    //record Revenue allocations(35) and mark invoice doc as reconciled
                    //debit revenue equity account (74) and credit assigned revenue account per item type or handle tax if tax line
                    //Use payment doc ID as docID and InvoiceDoc Reference (InvoiceNumber) as description.
                    foreach (TransLineItem TL in Lines)
                    {
                        int TransID = 0;
                        ItemTypeRevExp IA = getItemAccts(TL.ItemID);
                        TransID = SaveTransaction(IA.RevAcctID,35, PayDoc, 74, ID.DocumentReference, TL.Quantity * TL.Price);
                        if (TransID > 0)
                        {
                            bool success=AllocateLine(TL.LineID);
                        }
                       
                    }
                    //Check for Tax Transactions
                    
                    bool taxsuccess = false;
                    foreach (TaxCollection  TC in taxes)
                    {
                        int TransID = 0;
                        //record tax collection(32)
                        int TaxAcctID = getTaxPayAcctID(TC.TaxRateID);
                        TransID = SaveTransaction(TaxAcctID, 32, PayDoc, P.PaymentCreditID, ID.DocumentReference, TC.TaxAmount);
                        if (TransID > 0)
                        {
                            taxsuccess=SetTaxCollected(TC.TaxCollectID);
                        }
                        
                    }
                    bool recsuccess = false;
                    
                        //mark invoice as reconciled
                       recsuccess= ReconcileDocument(InvDoc);
                    
                    if (differ==0 && recsuccess)
                    {
                        //mark payment as reconciled
                        bool recpayment = ReconcileDocument(PayDoc);
                    }
                }
                #endregion
                else//Payment is not large enough to cover the invoice
                {
                    //record Revenue allocations(35) and mark Payment as reconciled
                    decimal totaltaxes = 0;
                    decimal taxpercent = 0;
                    decimal payment = P.UnallocatedPayment;
                    decimal taxpayment = 0;
                    foreach (TaxCollection TC in taxes)
                    {
                        totaltaxes = totaltaxes + TC.TaxAmount;
                        taxpercent = taxpercent + getTaxRateFromID(TC.TaxRateID).Rate;
                    }
                    if (totaltaxes > 0)
                    {
                        foreach (TaxCollection TXC in taxes)
                        {
                            taxpayment = P.UnallocatedPayment * getTaxRateFromID(TXC.TaxRateID).Rate;
                            payment = payment - taxpayment;
                            int TransID = 0;
                            //record tax collection(32)
                            int TaxAcctID = getTaxPayAcctID(TXC.TaxRateID);
                            TransID = SaveTransaction(TaxAcctID, 32, PayDoc, P.PaymentCreditID, ID.DocumentReference, taxpayment);
                        }
                    }
                    while (payment>0)
                    {
                        foreach (TransLineItem TL in Lines)
                        {
                            int TransID = 0;
                            ItemTypeRevExp IA = getItemAccts(TL.ItemID);
                            decimal LinePayment = TL.Quantity * TL.Price;
                            if (payment >= LinePayment)
                            {
                                
                                TransID = SaveTransaction(IA.RevAcctID, 35, PayDoc, 74, ID.DocumentReference, LinePayment);
                                if (TransID > 0)
                                {
                                    bool success = AllocateLine(TL.LineID);
                                }
                                payment = payment - LinePayment;
                                if (payment == 0) break;
                            }
                            else
                            {
                                TransID = SaveTransaction(IA.RevAcctID, 35, PayDoc, 74, ID.DocumentReference, payment);
                                payment = 0;
                                break;
                            }
                        }
                    }
                    bool recpayment = ReconcileDocument(PayDoc);
                }
            }
            catch
            {
                result = false;
            }

            return result;
        }
        #endregion
        
        #region TRANSACTION************************************************************************************
        public int SaveTransaction(int Cact, int Tmethod, int DocID, int Dact, string Description, decimal AMT)
        {
            int TransID=0;
            string currentuser = System.Web.Security.Membership.GetUser().UserName;
            TransactionTable PE = new TransactionTable { CreditID = Cact, DebitID = Dact, TransactionTypeID = Tmethod, DocumentID = DocID, Amount = AMT, EnteredBy = currentuser, Description = Description, EnterDate = DateTime.Now };
            try
            {
                db.TransactionTables.InsertOnSubmit(PE);
                db.SubmitChanges();
                TransID = PE.TransactionID;
            }

            catch (Exception e)
            {

                Error newerror = new Error { ErrorMessage = e.Message, ErrorModule = "OUR THINGS--Classes--MyMethods.cs--SaveTransaction", ErrorTime = DateTime.Now, ErrorUser = currentuser, ErrorHelpLink = e.HelpLink, ErrorInnerException = e.InnerException.Message, ErrorSource = e.Source, ErrorStack = e.StackTrace, ModuleInput = @"Cact:" + Cact + @" Tmethod:" + Tmethod + @" DocID:" + DocID + @" Dact:" + Dact + @" Description:" + Description + @" AMT:" + AMT };
                db.Errors.InsertOnSubmit(newerror);
                db.SubmitChanges();
            }

            return TransID;
        }
        public string getTransactionTypeFromID(int TTypeID)
        {
            return (from TT in db.TransactionTypeTables where TT.TransactionTypeID == TTypeID select TT).Single().TransactionType;
        }
        public List<TransactionTable> getAllTransactions()
        {
            return (from t in db.TransactionTables select t).ToList();
        }
        public List<TransactionTable> getTransactionsFromDocID(int DocID)
        {
            return (from t in db.TransactionTables where t.DocumentID==DocID select t).ToList();
        }
        public decimal getTransactionDocAcctCredits(int DocID, int AcctID)
        {
            decimal result = 0;
            var AD = from t in db.TransactionTables where t.CreditID == AcctID && t.DocumentID == DocID select t.Amount;
            if (AD.Count() > 0) result = AD.Sum();
            return result;
        }
        public decimal getTransactionDocAcctDebits(int DocID, int AcctID)
        {
            decimal result=0;
            var AD=from t in db.TransactionTables where t.DebitID == AcctID && t.DocumentID == DocID select t.Amount;
            if (AD.Count() > 0) result = AD.Sum();
            return result;
        }
        public TransLineItemStatus getLineItemStatusFromSN(string SN)
        {
            return (from i in db.TransLineItemStatus where i.SerialNumber == SN select i).Single();
        }
        public TransLineItem getTransactionLineByTransLineID(int TransLineID)
        {
            return (from ti in db.TransLineItems where ti.LineID == TransLineID select ti).Single();
        }
        public List<TransLineItemStatus> getUnreceivedTransLineItemStatusByLineID(int LineID)
        {
            return (from lis in db.TransLineItemStatus where lis.LineID == LineID && (lis.LineItemStatusID==1 || lis.LineItemStatusID==2) select lis).ToList(); //Select Ordered (1) or Shipped (2) as only status that can be changed to received
        }
        public List<TransactionTable> getTransactionsByAcctID(int AcctID)
        {
            return (from t in db.TransactionTables where t.CreditID == AcctID || t.DebitID == AcctID select t).ToList();
        }
        public List<TransactionTable> SortTransactionsByDate(List<TransactionTable> Transactions)
        {
            List<TransRecord> TRAN = new List<TransRecord>();
            foreach (TransactionTable T in Transactions)
            {
                TRAN.Add(new TransRecord { DocID = T.DocumentID, TransID = T.TransactionID, DocTime = GetDoc(T.DocumentID).DocumentTime });
            }
            var sorted = from t in TRAN orderby t.DocTime select t;
            List<TransactionTable> result = new List<TransactionTable>();
            foreach (TransRecord rec in sorted)
            {
                result.Add(getTransaction(rec.TransID));
            }
            return result;
        }
        public TransactionTable getTransaction(int TransID)
        {
            return (from t in db.TransactionTables where t.TransactionID == TransID select t).Single();
        }
        public List<TransactionTable> getCreditsbyAcctID(int AcctID)
        {
            
            List<TransactionTable> T = (from TT in db.TransactionTables where TT.CreditID == AcctID select TT).ToList();
            //Filter by master Date
            return FilterTransByDateRange(T);
        }
        public List<TransactionTable> getDebitsbyAcctID(int AcctID)
        {
           
            List<TransactionTable> T = (from TT in db.TransactionTables where TT.DebitID == AcctID select TT).ToList();
            //Filter by master Date
            return FilterTransByDateRange(T);
            
        }
        public List<TransactionTable> FilterTransByDateRange(List<TransactionTable> T)
        {
            List<TransactionTable> result=new List<TransactionTable>();
            DateTime SDate = Convert.ToDateTime(getAPstartDate());
            DateTime EDate = Convert.ToDateTime(getAPendDate());
            foreach (TransactionTable Trans in T)
            {
                DateTime D = (from DOC in db.Documents where DOC.DocumentID == Trans.DocumentID select DOC.DocumentTime).Single().Date;
                if (D >= SDate && D <= EDate)
                {
                    result.Add(Trans);
                }
            }
            return result;
        }
        #endregion

        #region LINES****************************************************************************************
        public bool ForceReceiveLineItem(LineItem line)
        {
            //Must determine quantity on each line
            bool result = false;
            string currentuser = System.Web.Security.Membership.GetUser().UserName;
            if (line.SerialNumber.Length < 1) line.SerialNumber = "#";
            try
            {
                TransLineItemStatus ForceReceive = new TransLineItemStatus { CreatedBy = currentuser, CreateTime = DateTime.Now, LineID = line.TransactionLineID, LineItemStatusID = 3, SerialNumber = line.SerialNumber,LocationID=0 };
                db.TransLineItemStatus.InsertOnSubmit(ForceReceive);
                db.SubmitChanges();
                result = true;
            }
            catch (Exception)
            {

               //result stays false
            }
            return result;
        }
        public int SaveLineItem(int DocID, LineItem line)
        {
            int LineID = 0;
            MembershipUser currentuser = System.Web.Security.Membership.GetUser();
            TransLineItem l = new TransLineItem { DocumentID = DocID, ItemID = line.ItemID, Price = line.Price, Quantity = line.Quantity, TaxPaid = line.Taxable, Allocated = line.Allocated,JobLineID=Convert.ToInt32(line.JobLineID)};
            //If line is not a labor line any item notes would be in line description so record it here. If it is a job line it would easily have more that 50 characters and cause ItemNote save to fail. Labor line have all descriptive data in the Jobline table
            if (line.JobLineID < 1) l.ItemNote = line.Description;
            try
            {
                db.TransLineItems.InsertOnSubmit(l);
                db.SubmitChanges();
                LineID = l.LineID;
            }
            catch (Exception e)
            {

                Error newerror = new Error { ErrorMessage = e.Message, ErrorModule = "OUR THINGS--Classes--MyMethods.cs--SaveLineItem", ErrorTime = DateTime.Now, ErrorUser = currentuser.UserName, ErrorHelpLink = e.HelpLink, ErrorInnerException = e.InnerException.Message, ErrorSource = e.Source, ErrorStack = e.StackTrace, ModuleInput = @"DocID:" + DocID + @" line.Price:" + line.Price + @" line.Quantity:" + line.Quantity + @" line.Taxable:" + line.Taxable + @"Allocated=line.Allocated:" + line.Allocated };
                db.Errors.InsertOnSubmit(newerror);
                db.SubmitChanges();
            }

            return LineID;
        }
        public int InsertJobLine(LineItem Jline)
        {
            int result = 0;
            try
            {
                JobLine JL = new JobLine { Description = Jline.Description, EmployeeID = Jline.EmployeeID, EnteredBy = Membership.GetUser().UserName, JobID = Jline.JobID, HourRate = Jline.Price, EnterTime = DateTime.Now, ServiceItemID = Jline.ItemID, TimeIn = Jline.TimeIn, TimeOut = Jline.TimeOut, TotalTime = Jline.Quantity };
                db.JobLines.InsertOnSubmit(JL);
                db.SubmitChanges();
                result = JL.JobLineID;
            }
            catch { }

            return result;
        }
        public bool ReceiveLineItem(LineItem line)
        {
            bool result = false;
            //string currentuser = System.Web.Security.Membership.GetUser().UserName;
            //if (line.SerialNumber != "0")//Process Serialized Item
            //{
            //    try
            //    {//Update Item Status Tracking table
            //        TransLineItemStatus LineItemStatus=  getLineItemStatusFromSN(line.SerialNumber);
            //        LineItemStatus.LineItemStatusID = 3;
            //        LineItemStatus.ModifiedBy = currentuser;
            //        LineItemStatus.ModifiedTime = DateTime.Now;
            //        TransLineItem LineItem = getTransactionLineByTransLineID(line.TransactionLineID);
            //        LineItem.StatusID = 3;
            //        db.SubmitChanges();
            //        result = true;//result will be returned as true to indicate success
            //    }
            //    catch (Exception e)
            //    {
                    
            //       //result will be return as false to indicate failure
            //    }
              
            //}

            //else //Duplicate LineIDs in table possible as one is saved for every one quantity though no distinction is made
            //{
            //    try
            //    {//Update Item Status Tracking table
            //        int receivecount = 0;
            //        List<TransLineItemStatus> potentialitems = getUnreceivedTransLineItemStatusByLineID(line.TransactionLineID);
            //        //Process the first line in list only
            //        potentialitems[0].LineItemStatusID = 3;
            //        potentialitems[0].ModifiedBy = currentuser;
            //        potentialitems[0].ModifiedTime = DateTime.Now;

            //        TransLineItem LineItem = getTransactionLineByTransLineID(line.TransactionLineID);
            //        LineItem.StatusID = 3;
            //        db.SubmitChanges();
            //        result = true;//result will be returned as true to indicate success
            //    }
            //    catch (Exception e)
            //    {

            //        //result will be return as false to indicate failure
            //    }
            //}
            //for (int c = 0; c < line.Quantity; c++)
            //{

            //}
            ////Check if item already exist in recieving (LocID 55) and if so add quantity
            //if ((from l in db.ItemLocs where l.ItemID == line.ItemID && l.LocID == 55 select l).Count() > 0)
            //{
            //    try
            //    {
            //        ItemLoc cl = (from l in db.ItemLocs where l.ItemID == line.ItemID && l.LocID == 55 select l).Single();
            //        cl.Quantity = cl.Quantity + line.Quantity;
            //        db.SubmitChanges();
            //        ItemLocID = cl.ItemLocID;
            //    }
            //    catch (Exception e)
            //    {
            //        Error newerror = new Error { ErrorMessage = e.Message, ErrorModule = "OUR THINGS--Classes--MyMethods.cs--ReceiveLineItem", ErrorTime = DateTime.Now, ErrorUser = currentuser, ErrorHelpLink = e.HelpLink, ErrorInnerException = e.InnerException.Message, ErrorSource = e.Source, ErrorStack = e.StackTrace, ModuleInput = @"line.ItemID:" + line.ItemID + @" LocID:" + 55 + @" line.Quantity:" + line.Quantity };
            //        db.Errors.InsertOnSubmit(newerror);
            //        db.SubmitChanges();
            //    }
                
            //}
            //else
            //{
            //    //Add new ItemLoc Record
                
            //    try
            //    {
            //        ItemLoc nl = new ItemLoc { ItemID = line.ItemID, LocID = 55, Quantity = line.Quantity };
            //        db.ItemLocs.InsertOnSubmit(nl);
            //        db.SubmitChanges();
            //        ItemLocID = nl.ItemLocID;
            //    }
            //    catch (Exception e)
            //    {
            //        Error newerror = new Error { ErrorMessage = e.Message, ErrorModule = "OUR THINGS--Classes--MyMethods.cs--ReceiveLineItem", ErrorTime = DateTime.Now, ErrorUser = currentuser, ErrorHelpLink = e.HelpLink, ErrorInnerException = e.InnerException.Message, ErrorSource = e.Source, ErrorStack = e.StackTrace, ModuleInput = @"line.ItemID:" + line.ItemID + @" LocID:" + 55 + @" line.Quantity:" + line.Quantity };
            //        db.Errors.InsertOnSubmit(newerror);
            //        db.SubmitChanges();
                   
            //    }
            //}

            return result;
        }
        public int OrderLineItem(LineItem line)
        {

            int ItemLocID = 0;
            string currentuser = System.Web.Security.Membership.GetUser().UserName;
            //Check if item already exist in onorder(LocID 56) and if so add quantity
            if ((from l in db.ItemLocs where l.ItemID == line.ItemID && l.LocID == 56 select l).Count() > 0)
            {
                try
                {
                    ItemLoc cl = (from l in db.ItemLocs where l.ItemID == line.ItemID && l.LocID == 56 select l).Single();
                    cl.Quantity = cl.Quantity + line.Quantity;
                    db.SubmitChanges();
                    ItemLocID = cl.ItemLocID;
                }
                catch (Exception e)
                {
                    Error newerror = new Error { ErrorMessage = e.Message, ErrorModule = "OUR THINGS--Classes--MyMethods.cs--ReceiveLineItem", ErrorTime = DateTime.Now, ErrorUser = currentuser, ErrorHelpLink = e.HelpLink, ErrorInnerException = e.InnerException.Message, ErrorSource = e.Source, ErrorStack = e.StackTrace, ModuleInput = @"line.ItemID:" + line.ItemID + @" LocID:" + 56 + @" line.Quantity:" + line.Quantity };
                    db.Errors.InsertOnSubmit(newerror);
                    db.SubmitChanges();
                }

            }
            else
            {
                //Add new ItemLoc Record

                try
                {
                    ItemLoc nl = new ItemLoc { ItemID = line.ItemID, LocID = 56, Quantity = line.Quantity };
                    db.ItemLocs.InsertOnSubmit(nl);
                    db.SubmitChanges();
                    ItemLocID = nl.ItemLocID;
                }
                catch (Exception e)
                {
                    Error newerror = new Error { ErrorMessage = e.Message, ErrorModule = "OUR THINGS--Classes--MyMethods.cs--ReceiveLineItem", ErrorTime = DateTime.Now, ErrorUser = currentuser, ErrorHelpLink = e.HelpLink, ErrorInnerException = e.InnerException.Message, ErrorSource = e.Source, ErrorStack = e.StackTrace, ModuleInput = @"line.ItemID:" + line.ItemID + @" LocID:" + 56 + @" line.Quantity:" + line.Quantity };
                    db.Errors.InsertOnSubmit(newerror);
                    db.SubmitChanges();

                }
            }

            return ItemLocID;
        }
        public TransLineItem GetLineItemFromID(int LineID)
        {
            return (from l in db.TransLineItems where l.LineID == LineID select l).Single();
        }
        public bool ChangeLineAllocationToAllocated(int LineID)
        {
            string currentuser = System.Web.Security.Membership.GetUser().UserName;
            bool result = true;
            try
            {
                TransLineItem UA = (from u in db.TransLineItems where u.LineID == LineID select u).Single();
                UA.Allocated = true;
                db.SubmitChanges();
            }
            catch (Exception e)
            {
                result = false;
                Error newerror = new Error { ErrorMessage = e.Message, ErrorModule = "OUR THINGS--Classes--MyMethods.cs--ChangeLineAllocationToAllocated", ErrorTime = DateTime.Now, ErrorUser = currentuser, ErrorHelpLink = e.HelpLink, ErrorInnerException = e.InnerException.Message, ErrorSource = e.Source, ErrorStack = e.StackTrace, ModuleInput = @"LineID:" + LineID};
                db.Errors.InsertOnSubmit(newerror);
                db.SubmitChanges();
            }
            return result;
        }
        public List<LineItem> ConvertToLineItems(List<string> jsdata, int linetype)
        {
            // linetype 1 for purchase line 2 for sale line
            List<LineItem> result = new List<LineItem>();

            foreach (string js in jsdata)
            {
                int idx = 0;
                List<string> fields = new List<string>();
                string data = js;
                string C = "";
                while (data.IndexOf("▄") > -1)
                {
                    idx = data.IndexOf("▄");
                    C = data.Substring(0, idx);
                    idx = idx + 1;
                    data = data.Substring(idx);
                    fields.Add(C);
                }
                //Add last piece of data
                fields.Add(data);
                decimal Q = Convert.ToDecimal(fields[0]);//Quantity
                int I = Convert.ToInt32(fields[1]);//ItemID
                
                bool T = false; 
                int E = 0;//ExpenseAccount
                string S = "";//serialNumber
                decimal A = 0;
                int JobLineID = 0; 
                switch (linetype)
                {
                    case 1://For Purchases
                        A = Convert.ToDecimal(fields[2]);//Amount
                        E = Convert.ToInt32(fields[4]);
                        T=Convert.ToBoolean(fields[3]);//TaxStatus
                        result.Add(new LineItem { ItemID = I, Price = A, Quantity = Q, Taxable = T, ExpenseAcct = E, SerialNumber = S, Allocated = true });
                        break;
                    case 2://For Sales
                        A = Convert.ToDecimal(fields[3]);//Amount
                        S = fields[5];
                        T=Convert.ToBoolean(fields[4]);//TaxStatus
                        result.Add(new LineItem { ItemID = I, Price = A, Quantity = Q, Taxable = T, ExpenseAcct = E, SerialNumber = S, Allocated = true,Description=fields[2],JobLineID=JobLineID });
                        JobLineID = Convert.ToInt32(fields[6]);
                        break;
                    case 3://For Service Credits 
                        
                        A=Convert.ToDecimal(fields[3]);//Amount
                        int SCTID = Convert.ToInt32(fields[5]);
                        result.Add(new LineItem { ItemID = I, Price = A, Quantity = Q, Description=fields[2],ServiceCreditTypeID=SCTID });
                        JobLineID = Convert.ToInt32(fields[6]);
                        break;
                    default:
                        break;
                }
                
                
                
                
            }
            return result;
        }
        public List<TransLineItem> getTransLinesByDocID(int DocID)
        {
            return(from tl in db.TransLineItems where tl.DocumentID==DocID select tl).ToList();
        }
        public List<TransLineItem> getUnallocatedLines(int DocID)
        {
            return (from tl in db.TransLineItems where tl.DocumentID == DocID && tl.Allocated == false select tl).ToList();
        }
        bool AllocateLine(int LineID)
        {
            bool result = true;
            try
            {
                TransLineItem TL = (from T in db.TransLineItems where T.LineID == LineID select T).Single();
                TL.Allocated = true;
                db.SubmitChanges();
            }
            catch
            {
                result = false;
            }
            return result;
        }
        #endregion

        #region USER********************************************************************************************
        public string SetUserPrefMobile(int pref)//1=DeskTop 2=Mobile this is option preference 1
        {
            string result = "";
            UserPreferenceTable U = getUserPrefMobile();
            U.Preference = pref;
            try { db.SubmitChanges(); }
            catch (Exception e) { result = @" alert('Problem setting Display Preference " + e + @"'); "; }
            return result;

        }
        public UserPreferenceTable getUserPrefMobile()
        {
            UserPreferenceTable UP = new UserPreferenceTable();
            string user = Membership.GetUser().UserName;
            try { UP = (from up in db.UserPreferenceTables where user == up.UserName && up.PrefOption == 1 select up).Single(); }
            catch
            {
                UP.UserName = user; UP.PrefOption = 1; UP.Preference = 1;
                db.UserPreferenceTables.InsertOnSubmit(UP);
                db.SubmitChanges();
            }
            return UP;
        }
        public string UserIsLoggedIn()
        {
            string result = "";
            try { result = Membership.GetUser().UserName; }
            catch { }
            return result;
        }
        #endregion
        public string getPayerNameFromRecDocID(int RecDocID)
        {
            int PayerID = (from t in db.TransactionTables where t.DocumentID == RecDocID select t.CreditID).Single();
            
            return getAccountFromID(PayerID).AcctName;
        }
        public List<string> ParcJSdata(string jsdata)
        {
            List<string> result = new List<string>();
            while (jsdata.Length > 0)
            {
                string d = jsdata.Substring(jsdata.IndexOf("{") + 1, jsdata.IndexOf("}") - 1);
                result.Add(d);
                jsdata = jsdata.Substring(d.Length + 2);
            }
            return result;
        }
        
        public decimal getBalance(int AcctID)
        {
            decimal credits = 0;
            decimal debits = 0;
            decimal Bal = 0;
            
            int AcctTypeID = getAccountFromID(AcctID).AcctTypeID;
            if (AcctTypeID == 15)//Customer account, get invoices and reciepts
            {
                List<Document> Invoices = getCustInvoices(AcctID);
                List<Document> Payments = getCustPayments(AcctID);
                debits = (from I in Invoices select I.Amount).Sum();
                credits = (from P in Payments select P.Amount).Sum();
                Bal = debits-credits;
            }
            else
            {
                try
                {
                    
                    List<TransactionTable> T= getCreditsbyAcctID(AcctID);
                    credits = (from tr in T select tr.Amount).Sum();
                    
                }
                catch { credits = 0; }
                try
                {
                    
                    List<TransactionTable> T=getDebitsbyAcctID(AcctID);
                    debits = (from tr in T select tr.Amount).Sum();
                }
                catch { debits = 0; }



                //adjust balance calculation based on type of Account
                if (AcctTypeID == 1 || AcctTypeID == 2 || AcctTypeID == 12 || AcctTypeID == 9 || AcctTypeID == 7 || AcctTypeID == 20) Bal = debits - credits;
                else Bal = credits - debits;
            }
            Bal = Math.Round(Bal, 2);
            return Bal;
        }
        public decimal getBalance(int AcctID,DateTime SDate,DateTime EDate)//Overloaded With DateRange
        {
            decimal credits = 0;
            decimal debits = 0;
            decimal Bal = 0;
            try
            {
                credits = (from T in db.TransactionTables where T.CreditID == AcctID && T.EnterDate>=SDate && T.EnterDate<=EDate select T.Amount).Sum();
            }
            catch { credits = 0; }
            try
            {
                debits = (from T in db.TransactionTables where T.DebitID == AcctID && T.EnterDate>=SDate && T.EnterDate<=EDate select T.Amount).Sum();
            }
            catch { debits = 0; }
            //adjust balance calculation based on type of Account
            int AcctTypeID = getAccountFromID(AcctID).AcctTypeID;
            if (AcctTypeID == 1 || AcctTypeID == 2 || AcctTypeID == 12 || AcctTypeID == 9 || AcctTypeID == 7 ) Bal = debits - credits;
            else Bal = credits - debits;
            Bal = Math.Round(Bal, 2);
            return Bal;
        }
        public string SaveFundsTransfer(DateTime Tdate, int Cact, int Dact, int Tmethod, string Reference, string Desc, decimal TAMT, decimal FEEAMT,int PayAct,int FeeAct)
        {
            string result = @" alert('Transfer Save Complete'); ";
            MembershipUser currentuser = System.Web.Security.Membership.GetUser();
            decimal DocAmt = TAMT + FEEAMT;
            int DocID = 0; int TransID = 0;
            DocID = InsertDocument(1, 0, Tdate, Reference, "", DocAmt, Tmethod);
            if (DocID > 0)
            {
                TransID=SaveTransaction(Cact, Tmethod, DocID, Dact, Desc, TAMT);

            }
            else
            {
                result = @" alert('Transfer Save Failed to save Document'); ";
            }
            if (TransID > 0)//Test Tranaction Save
            {
                if (FEEAMT > 0)//Check for Fee and if so proccess
                {
                    TransID = 0;
                    TransID = SaveTransaction(PayAct, Tmethod, DocID, FeeAct, Desc, FEEAMT);
                    if (TransID > 0)
                    {
                        TransID = 0;
                        TransID = SaveTransaction(Cact, Tmethod, DocID, PayAct, Desc, FEEAMT);
                        if (TransID < 1) result = @" alert('Transfer Save Failed to save Fee Payment Transaction'); ";
                    }
                    else { result = @" alert('Transfer Save Failed to save Vender Fee Transaction'); "; }
                }
            }
            else
            {
                result = @" alert('Transfer Save Failed to save Transfer Transaction'); ";
            }
            

            return result;
        }
        public string SaveFundsTransferWithFee(DateTime Tdate, int Fromact, int Toact, int Tmethod, string Reference, string Desc, decimal AMT, decimal FeeAMT, int FeeAct, int PayAct)
        {
            string result = @" alert('Transfer Save with Fee Complete'); ";
            bool failed = true;
            MembershipUser currentuser = System.Web.Security.Membership.GetUser();
            //Save Transfer
            //Create Transfer Document
            int DocID = SaveDocument(0, 1, 0, Tdate, AMT + FeeAMT, Reference, "");
            if (DocID > 0)
            {//Credit Withdrawal Account and Debit Deposit Account
                if (SaveTransaction(Fromact, Tmethod, DocID, Toact, Desc, AMT) < 1)
                {
                    result = @" alert('Transfer Save Failed to save Transaction'); ";
                }
                else
                {
                    failed = false;
                }
            }
            else
            {
                result = @" alert('Transfer Save Failed to save Document'); ";
            }
            //Save Fee
            if (!(failed))
            {
                //Record expense entry (Payment of the Fee expense owed to bank or vendor
                if (SaveTransaction(PayAct, Tmethod, DocID, FeeAct, "Transaction Fee Charged", FeeAMT) > 0)
                {
                    //Record payable entry (Fee expense owed to bank or vendor)
                    if (SaveTransaction(Fromact, Tmethod, DocID, PayAct, "Transaction Fee Payment", FeeAMT) < 1)
                    {
                        result = @" alert('Transaction Fee Payable Failed'); ";
                    }

                }
                else
                {
                    result = @" alert('Transaction Fee Payable Failed'); ";
                }
            }
            return result;
        }
        public int SavePaymentReceived(DateTime Pdate, int Pact, int Pmethod, string DocRef, string Desc, decimal AMT, string AppliedInvoices)
        {
            
            
            bool result = true;
            int RAcctID = getPayMethodRecAcct(Pmethod);
            MembershipUser currentuser = System.Web.Security.Membership.GetUser();
            int TransID = 0;
            Desc = "PAYMENT-" + Desc;
            
            int DocID = InsertDocument(3, 0, Pdate, DocRef, "*", AMT, Pmethod);
            //Transaction Type 27 for Payment
            if (DocID > 0)
            {
                //debit money receieved account and credit customer account
                TransID = SaveTransaction(Pact, 27, DocID, RAcctID, Desc, AMT);
                    if (!(TransID > 0)) result = false;
            }
            else
            {
                result = false;
            }
            if (TransID > 0)
            {
              
                if (AppliedInvoices.Length > 0)//Apply payment to designated invoices
                {
                    List<string> InvsNums = ExtractInvoiceNumbers(AppliedInvoices);
                    List<Document> Invs = new List<Document>();

                    foreach (string invnum in InvsNums)
                    {
                        Invs.Add(getInvoiceFromReference(invnum));
                    }
                    decimal UnApplied = ApplyPayment(DocID, Invs);
                    if (UnApplied == -1) return 0;//failed to apply

                }
                
            }



            return TransID;
        }
        public List<string> ExtractInvoiceNumbers(string InvsString)
        {
            List<string> result = new List<string>();
            while (InvsString.Length>1)
            {
                int B = InvsString.IndexOf('{');
                int E = InvsString.IndexOf('}');
                result.Add(InvsString.Substring(B+1,E-1));
                if (!((E + 1) > InvsString.Length))
                {
                    InvsString = InvsString.Substring(E + 1);
                }
                else InvsString = "*";
            }


            return result;
        }
        public decimal ApplyPayment(int PayDocID, List<Document> Invs)
        {
            decimal result = -1;//return of -1 indicates the function failed
            int TransID = 0;
            decimal AMT = GetDocumentFromID(PayDocID).Amount;
            TransactionTable PTrans=getPaymentTransactionFromPaymentDoc(PayDocID);
            foreach (Document D in Invs)
            {
                decimal InvAMT = D.Amount;
                //Handle tax
                TaxCollection tax = new TaxCollection();
                tax.TaxAmount = 0;
                int TaxPayAcct = 0;
                int TaxColID=0;
                tax=getTaxFromDocID(D.DocumentID);
                if (tax.TaxAmount > 0)
                {
                    TaxPayAcct = getTaxPayAcctID(tax.TaxRateID);
                    TaxColID=tax.TaxCollectID;
                }
                //Check for previous payments to invoice
                List<Payment> PrevPayments = getPaymentsFromInvID(D.DocumentID);
                if (PrevPayments.Count > 0)//Adjust InvAMT based on prior payments
                {
                    foreach (Payment P in PrevPayments)
                    {
                        InvAMT = InvAMT - P.Amount;
                    }
                }
                //Check that payment is enough to pay invoice
                
                if (AMT < InvAMT)//Payment only covers part of invoice
                {
                    //Add record to Payment Table
                    Payment Paid = new Payment { Amount = AMT, InvDocID = D.DocumentID, PayDocID = PayDocID };
                    db.Payments.InsertOnSubmit(Paid);
                    db.SubmitChanges();
                    //Check for Tax
                    if (tax.TaxAmount > 0)
                    {
                        //Calculate tax portion based on amount applied
                        decimal TaxRate=getTaxRateFromID(tax.TaxRateID).Rate;
                        decimal TaxToPay=Math.Round(AMT-(AMT*(100/(TaxRate+100))),2);
                        TransID = SaveTransaction(TaxPayAcct, 32, PayDocID, PTrans.DebitID, "Customer Tax Payment", TaxToPay);
                        //Adjust tax left to collect
                        TaxCollection orgtax = getTaxCollected(TaxColID);
                        orgtax.TaxAmount = orgtax.TaxAmount - TaxToPay;
                        db.SubmitChanges();
                        AMT = AMT - TaxToPay;
                    }
                    //Apply the rest to respective revenue accounts
                    List<TransLineItem> LineItems = getTransLinesByDocID(D.DocumentID);
                    List<AcctAMTs> RevAmounts = new List<AcctAMTs>();
                    foreach (TransLineItem TL in LineItems)
                    {
                        Item LItem = getItemFromID(TL.ItemID);
                        int ItemRevID = getItemRevAccount(LItem.ItemTypeID);
                        bool RevAcctFound = false;
                        decimal linetotal = TL.Quantity * TL.Price;
                        //Add all Matching Revenue Ids together
                        if (AMT >= linetotal)
                        {
                            foreach (AcctAMTs RA in RevAmounts)
                            {
                                if (RA.AcctID == ItemRevID)
                                {
                                    RA.AMT = RA.AMT + (linetotal);
                                    RevAcctFound = true;
                                }
                            }
                            if (!(RevAcctFound))
                            {
                                RevAmounts.Add(new AcctAMTs { AcctID = ItemRevID, AMT = (linetotal) });
                            }
                            AMT = AMT - linetotal;
                        }
                        else
                        {
                            foreach (AcctAMTs RA in RevAmounts)
                            {
                                if (RA.AcctID == ItemRevID)
                                {
                                    RA.AMT = RA.AMT + (AMT);
                                    RevAcctFound = true;
                                }
                            }
                            if (!(RevAcctFound))
                            {
                                RevAmounts.Add(new AcctAMTs { AcctID = ItemRevID, AMT = (AMT) });
                            }
                            AMT = 0;
                        }
                        

                        //Type 35 Revenue Allocation

                    }
                    foreach (AcctAMTs RA in RevAmounts)
                    {
                        TransID = SaveTransaction(PTrans.DebitID, 35, PayDocID, RA.AcctID, "Revenue Allocation", RA.AMT);
                    }


                }
                else//No breakdown needed, pay all
                {
                    //Add record to Payment Table
                    Payment Paid = new Payment { Amount = InvAMT, InvDocID = D.DocumentID, PayDocID = PayDocID };
                    AMT = AMT - InvAMT;
                    db.Payments.InsertOnSubmit(Paid);
                    db.SubmitChanges();
                    
                    if (tax.TaxAmount > 0)//Document has tax, proccess it Customer Tax Payment type 32
                    {
                        TransID = SaveTransaction(TaxPayAcct, 32, PayDocID, PTrans.DebitID, "Customer Tax Payment", tax.TaxAmount);
                        //delete tax collection record
                        TaxCollection dletax = getTaxCollected(TaxColID);
                        db.TaxCollections.DeleteOnSubmit(dletax);
                        db.SubmitChanges();
                    }
                    //Apply the rest to respective revenue accounts
                    List<TransLineItem> LineItems = getTransLinesByDocID(D.DocumentID);
                    List<AcctAMTs> RevAmounts = new List<AcctAMTs>();
                    foreach(TransLineItem TL in LineItems)
                    {
                        Item LItem = getItemFromID(TL.ItemID);
                        int ItemRevID = getItemRevAccount(LItem.ItemTypeID);
                        bool RevAcctFound = false;
                        
                        //Add all Matching Revenue Ids together
                        foreach (AcctAMTs RA in RevAmounts)
                        {
                            if (RA.AcctID == ItemRevID)
                            {
                                RA.AMT = RA.AMT + (TL.Price * TL.Quantity);
                                RevAcctFound = true;
                            }
                        }
                            if (!(RevAcctFound))
                            {
                                RevAmounts.Add(new AcctAMTs { AcctID = ItemRevID, AMT = (TL.Price * TL.Quantity) });
                            }
                        
                        //Type 35 Revenue Allocation
                        
                    }
                    foreach(AcctAMTs RA in RevAmounts)
                    {
                        TransID = SaveTransaction(PTrans.DebitID, 35, PayDocID, RA.AcctID, "Revenue Allocation", RA.AMT);
                    }
                    //Mark invoice as paid in full
                    D.Reconciled = true;
                    db.SubmitChanges();
                }
            }
            //If total payment used up record as reconciled to indicate all has been applied
            if (AMT == 0)
            {
                Document PD = GetDoc(PayDocID);
                PD.Reconciled = true;
                db.SubmitChanges();
            }
            result = AMT;

            return result;
        }
        public List<Payment> getPaymentsFromInvID(int InvID)
        {
            return (from P in db.Payments where P.InvDocID == InvID select P).ToList();
        }
        public TaxCollection getTaxCollected(int TaxColID)
        {
            return (from Tx in db.TaxCollections where Tx.TaxCollectID == TaxColID select Tx).Single();
        }
        public int getItemRevAccount(int ItemTypeID)
        {
            return (from I in db.ItemTypeRevExps where I.ItemTypeID == ItemTypeID select I.RevAcctID).Single();
        }
        public int getTaxPayAcctID(int TaxRateID)
        {
            return (from Tx in db.TaxRates where Tx.TaxLocID == TaxRateID select Tx.PayableAcctID).Single();
        }
        public TransactionTable getPaymentTransactionFromPaymentDoc(int PayDocID)
        {
            return (from T in db.TransactionTables where T.DocumentID == PayDocID && T.TransactionTypeID == 27 select T).Single();
        }
        public int getCustomerIDFromPayment(int PayDocID)
        {
            return (from T in db.TransactionTables where T.DocumentID == PayDocID && T.TransactionTypeID==27 select T.DebitID).Single();
        }
        public TaxCollection getTaxFromDocID(int DocID)
        {
            //Create TaxCollection first to avoid return null and causing error if not tax is associated
            TaxCollection result = new TaxCollection { TaxAmount = 0 };
            
            try{
                result = (from TC in db.TaxCollections where TC.DocID == DocID select TC).Single();
            }
            catch{
                
            }
            return result;
        }
        public bool SetTaxCollected(int TaxColID)
        {
            bool result = true;
            try
            {
                TaxCollection tc = (from tx in db.TaxCollections where tx.TaxCollectID == TaxColID select tx).Single();
                tc.Collected = true;
                db.SubmitChanges();
            }
            catch
            {
                result = false;
            }
            return result;
        }
        //public int SaveAppliedPaymentReceived(DateTime Pdate, int Pact, int Pmethod, string DocRef, string Desc, decimal AMT)
        //{
        //    //From SavePaymentReceived, modify for applied payment
        //    //Need to setup for payment applied to multiple invoices
        //    int DepLineID = 0;
        //    bool result = true;
        //    //Get Account for Pay Method

        //    int RAcctID = getPayMethodRecAcct(Pmethod);
        //    MembershipUser currentuser = System.Web.Security.Membership.GetUser();

        //    //Handle splitting of payment if tax was charged



        //    //Apply Increases to Revenue Accounts based on purchase


        //    int TransID = 0;
        //    int ParentDocID = getDocumentFromReference(DocRef).DocumentID;//Returns 0 if more than one
        //    //Doctype 16 Payment Applied
        //    int DocID = InsertDocument(16, 0, Pdate, DocRef, "*", AMT, Pmethod);
        //    if (DocID > 0)
        //    {
        //        TransID = SaveTransaction(Pact, Pmethod, DocID, RAcctID, Desc, AMT);
        //        if (!(TransID > 0)) result = false;
        //    }
        //    else
        //    {
        //        result = false;
        //    }
        //    if (TransID > 0)
        //    {
        //        DepLineID = InsertDepositLine(TransID);

        //    }


        //    return DepLineID;
        //}
        public Document getInvoiceFromReference(string InvNum)
        {
            return (from D in db.Documents where D.DocumentTypeID == 7 && D.DocumentReference == InvNum select D).Single();
        }
        public int getPayMethodRecAcct(int PayMethID)
        {
            return (from pm in db.PayMethods where pm.PayMethID == PayMethID select pm.RecAcctID).Single();
        }
        public string SaveTransactionFee(DateTime Tdate, int Cact, int Tmethod, int DocID, int PayAct, int FeeAct, decimal AMT, string username)
        {
            string result = @" alert('Transaction Fee Save Complete'); ";

            //Record expense entry (Payment of the Fee expense owed to bank or vendor
            if (SaveTransaction(PayAct, Tmethod, DocID, FeeAct, "Transaction Fee Charged", AMT) > 0)
            {
                //Record payable entry (Fee expense owed to bank or vendor)
                if (SaveTransaction(Cact, Tmethod, DocID, PayAct, "Transaction Fee Payment", AMT) < 1)
                {
                    result = @" alert('Transaction Fee Payable Failed'); ";
                }

            }
            else
            {
                result = @" alert('Transaction Fee Charge Failed'); ";
            }
            return result;

        }
        public string SaveDeposit(DateTime Ddate, int Dmethod, int BankAct, string Desc,string DocRef, List<DepositLine> Receipts)
        {
            //Account type 12 and 20 are depositable accounts
            decimal AMT = 0;
            bool success = true;
            string result = @" alert('Deposit Saved Successfully'); ";
            MembershipUser currentuser = System.Web.Security.Membership.GetUser();
            int TransID = 0;
            
            //Calculate total deposit
            foreach (DepositLine D in Receipts)
            {
                TransactionTable T = getTransaction(D.TransID);
                AMT = AMT + T.Amount;
            }

            //Create Deposit Document
            int DocID = InsertDocument(2, 0, Ddate, DocRef, "*", AMT, Dmethod);
            
            //Create deposit line entries
            if (DocID > 0)
            {
                foreach (DepositLine DforDoc in Receipts)
                {
                    DforDoc.DepositDocID = DocID;
                    UpdateDepositLine(DforDoc);
                }
                
            }

            //Create Transaction
            if (success)
            {
                TransID=SaveTransaction(4, Dmethod, DocID, BankAct, Desc, AMT);
            }

            if (!(TransID > 0)) result = @" alert('Deposit Save Failed!'); ";
            return result;
        }
        public string SaveDeposit2(DateTime Ddate, int BankAcct, string DocRef, string DLines)
        {
            string user = Membership.GetUser().UserName;
            string result = @" alert('Deposit Saved'); ";
            List<string> Lines = JSArrayToList(DLines);
            decimal TotalDeposit = 0;
            
            List<DepositItem> Items = JSDepItemListToDepositItems(Lines);
            
            Document D = new Document() { DocumentReference = DocRef, DocumentTime = Ddate,DocumentTypeID=2 ,EnteredBy=user,TimeEntered=DateTime.Now,ParentDocumentID=0,PaymentMethodID=0,DocumentPath="*",Reconciled=false};
            int DocID=InsertDocument(D);
            if (DocID > 0)
            {
                foreach (DepositItem di in Items)
                {
                    int TransID=0;
                    TransID=SaveTransaction(di.ItemAcctID, di.DepositType, DocID, BankAcct, di.Reference, di.Amount);
                    TotalDeposit = TotalDeposit + di.Amount;
                    if (TransID > 0)
                    {
                        bool success = SaveDepositLineItem(di.ItemTransID, DocID);
                        if (!(success)) return @" alert('Deposit document save failed'); ";
                    }
                }
                D.Amount = TotalDeposit;
                UpdateDocument(D);
            }
            else
            {
                result = @" alert('Deposit document save failed'); ";
            }

            return result;
        }
        public bool SaveDepositLineItem(int PaymentTransID, int DepDoc)
        {
            //0 paymenttransid is a cash item, otherwise a bank note
            bool result = true;
            try
            {
                DepositLine DL = new DepositLine { DepositDocID = DepDoc, TransID = PaymentTransID };
                db.DepositLines.InsertOnSubmit(DL);
                db.SubmitChanges();
            }
            catch
            {
                result = false;
            }
            return result;
        }
        public int InsertDepositLine(int TransID)
        {
            MembershipUser currentuser = System.Web.Security.Membership.GetUser();
            DepositLine DP = new DepositLine { TransID = TransID };
            int DepLineID = 0;
            try
            {
                db.DepositLines.InsertOnSubmit(DP);
                db.SubmitChanges();
                DepLineID = DP.DepositLineID;
            }
            catch (Exception e)
            {
                Error newerror = new Error { ErrorMessage = e.Message, ErrorModule = "OUR THINGS--Classes--MyMethods.cs--InsertDepositLine", ErrorTime = DateTime.Now, ErrorUser = currentuser.UserName, ErrorHelpLink = e.HelpLink, ErrorInnerException = e.InnerException.Message, ErrorSource = e.Source, ErrorStack = e.StackTrace };
                db.Errors.InsertOnSubmit(newerror);
                db.SubmitChanges();
            }
            return DepLineID;
        }
        public bool UpdateDepositLine(DepositLine ud)
        {
            MembershipUser currentuser = System.Web.Security.Membership.GetUser();
            
            bool result = false;
            try
            {
                
                
                db.SubmitChanges();
                result = true;
            }
            catch (Exception e)
            {

                Error newerror = new Error { ErrorMessage = e.Message, ErrorModule = "OUR THINGS--Classes--MyMethods.cs--UpdateDepositLine", ErrorTime = DateTime.Now, ErrorUser = currentuser.UserName, ErrorHelpLink = e.HelpLink, ErrorInnerException = e.InnerException.Message, ErrorSource = e.Source, ErrorStack = e.StackTrace};
                db.Errors.InsertOnSubmit(newerror);
                db.SubmitChanges();
            }
            return result;
        }
        public List<TransactionTable> getNonDepositedBankNotes()
        {
            List<TransactionTable> result = new List<TransactionTable>();
            //List all transactions with a BankNoteAcct(20) in the debit field and a payment transaction type(27) who's transaction id does not exist in the deposit line table
            List<AccountTable> Accts = getAccounts(20);
            List<int> DL = (from D in db.DepositLines select D.TransID).ToList();
            foreach (AccountTable  A in Accts)
            {
                List<TransactionTable> pays = (from T in db.TransactionTables where T.DebitID == A.AcctID && T.TransactionTypeID == 27 select T).ToList();
                foreach (TransactionTable  TP in pays)
                {
                    if (!(DL.Contains(TP.DebitID))) result.Add(TP);
                }
            }
            return result;
        }
        public string RecordPurchase(DateTime PDate, int Pacct, int VendAcct, string items, decimal transfee, decimal tax, string Ref, decimal Cash, string DocPath, int PayMethID, string TaxLocation,string Sns, decimal COGShip, decimal EXPShip, string InvRec, bool ReceiveItems)
        {
            //Use paymethod's RecAcctID field to determine wether the purchase is already paid or on terms. All terms paymethods have a RecAcctID of 0 service credits are -1 all other accounts have a positive int. If not on terms proccess documents as paid otherwise bills to be paid. Paid Purchase DocTypeID 4 Bill DocTypeID 9
            int PaidMethRecID = getPayMethodRecAcctID(PayMethID);
            int DocTypeID = 8;//Bill
            if (PaidMethRecID > 0) DocTypeID = 4;//Change to Paid Purchase
            
            List<string> jsitems = ParcJSdata(items);
            List<LineItem> lineitems = ConvertToLineItems(jsitems,1);

            string result = @" alert('Purchase Saved'); ";
            bool failed = false; int TransID = 0; decimal ItemAmt = 0; int count = 0; int LineID = 0; bool success = false; decimal DocAmt = 0;
            List<ItemAccountAmounts> ItemAcctSums = new List<ItemAccountAmounts>();
            List<int> ItemTransAccts = new List<int>();

            //Record Document*****************************************************
            Document D = new Document { Amount = DocAmt, DocumentReference = InvRec, DocumentTime = PDate, DocumentTypeID = DocTypeID, PaymentMethodID = PayMethID, Reconciled = false, DocumentPath = DocPath };
            int DocID = InsertDocument(D);
            
            if (DocID > 0)
            {
                foreach (LineItem l in lineitems)
                {
                    
                    ItemTypeRevExp ItemAccts = getItemAccts(l.ItemID);
                    int ItemTypeInvDesignator = getItemInvDesignator(l.ItemID);
                    decimal LineTotal = Math.Round(l.Price * l.Quantity, 2);
                    ItemAmt = ItemAmt + LineTotal;
                    
                    //Update vender item info
                    UpdateVendItemInfo(VendAcct,l);
                    
                    //Record Transaction Line Item
                    if(!(failed))
                    {
                        LineID = SaveLineItem(DocID, l);
                        l.TransactionLineID = LineID;
                        if (LineID < 1) failed = true;  
                    }
                    else
                    {
                        result = @" alert('Purchase Save Failed to Save Line Item " + count + @"'); ";
                        return result;
                        //Roll back and return
                    }
                   

                    //Accumulate Item Type Account and totals for Item account transaction entries and adjust inventory location quantities for not expensed items
                    if (!(failed))
                    {
                        if (l.ExpenseAcct>-1)
                        {
                            if (l.ExpenseAcct == 0)//Use default expense account
                            {
                                ItemAcctSums.Add(new ItemAccountAmounts { AcctID = ItemAccts.ExpAcctID, Amount = LineTotal });
                                if (!(ItemTransAccts.Contains(ItemAccts.ExpAcctID))) ItemTransAccts.Add(ItemAccts.ExpAcctID);
                            }
                            else//Expense account overidden use l.ExpensAcct as Expense account
                            {
                                ItemAcctSums.Add(new ItemAccountAmounts { AcctID = l.ExpenseAcct, Amount = LineTotal });
                                if (!(ItemTransAccts.Contains(l.ExpenseAcct))) ItemTransAccts.Add(l.ExpenseAcct);
                            }
                            if (!(ReceiveItems))
                            {
                                //Check loc table for preexisting vendor location ID and if it does not exist add record to location table for vender (loctype 12, vender account id in label)
                                var testforLocation = from loc in db.Locations where loc.LocLabel == VendAcct.ToString() select loc;
                                int LOCID = 0;
                                if (testforLocation.Count() > 0)
                                {
                                    //Only one record will exist so use last to get LOCID
                                    LOCID = testforLocation.Last().LocID;

                                }
                                else
                                {
                                    //Add new location record for vendor (loctype 12, vender account id in label)
                                    LOCID = AddLocationRecord(12, VendAcct.ToString(), true);
                                }
                                //Check for ItemLoc record
                                var testforrecord = from its in db.ItemLocs where its.ItemID == l.ItemID && its.LocID == LOCID select its;
                                if (testforrecord.Count() > 0)
                                {
                                    //Exist, update quantity
                                    UpdateItemLocQ(testforrecord.Last().ItemLocID, l.Quantity);
                                }
                                else
                                {
                                    //Doesnt exist create new record
                                    int ItemLocID = AddItemLocRecord(l.ItemID, LOCID, l.Quantity);
                                }
                            }

                        }
                        else//Not expensed must be inventory( so why have ItemTypeInvDesignator?) Proccess as inventory
                        {
                            ItemAcctSums.Add(new ItemAccountAmounts { AcctID = ItemAccts.InvAcctID, Amount = LineTotal });
                            if (!(ItemTransAccts.Contains(ItemAccts.InvAcctID))) ItemTransAccts.Add(ItemAccts.InvAcctID);

                            //Check if item type is pysical inventory (1) and if so add line items to inventory received location (1)
                                if (ItemTypeInvDesignator == 1)
                            
                            {

                                //If receive items when purchased then receive item into inventory.
                                if (ReceiveItems)
                                
                                {
                                    //Check receiving for pre-existing items

                                    var testforrecord = from its in db.ItemLocs where its.ItemID == l.ItemID && its.LocID == 1 select its;

                                    if (testforrecord.Count() > 0)
                                    {
                                        ItemLoc Existing = (from its in db.ItemLocs where its.ItemID == l.ItemID && its.LocID == 1 select its).Single();
                                        //add line quantity to existing location record
                                        decimal newquan = Existing.Quantity + l.Quantity;
                                        success = UpdateItemLocQ(Existing.ItemLocID, newquan);
                                    }
                                    else
                                    {
                                        //insert new record
                                        ItemLoc newitemloc = new ItemLoc { ItemID = l.ItemID, LocID = 1, Quantity = l.Quantity };
                                        success = InsertItemLoc(newitemloc);
                                    }


                                    if (failed)
                                    {

                                        result = @" alert('Purchase Save Failed to Save Inventory to receiving!'); ";
                                        //roll back all saves
                                        return result;
                                    }
                                }

                                else //Waiting for paid item to be delivered use itemloc.  until received.
                                {
                                    //Check loc table for preexisting vendor location ID and if it does not exist add record to location table for vender (loctype 12, vender account id in label)
                                    var testforLocation = from loc in db.Locations where loc.LocLabel == VendAcct.ToString() select loc;
                                    int LOCID = 0;
                                    if (testforLocation.Count() > 0)
                                    {
                                        //Only one record will exist so use last to get LOCID
                                        LOCID = testforLocation.Last().LocID;
                                        
                                    }
                                    else
                                    {
                                        //Add new location record for vendor (loctype 12, vender account id in label)
                                        LOCID = AddLocationRecord(12, VendAcct.ToString(), true);
                                    }
                                    //Check for ItemLoc record
                                    var testforrecord = from its in db.ItemLocs where its.ItemID == l.ItemID && its.LocID == LOCID select its;
                                    if (testforrecord.Count() > 0)
                                    {
                                        //Exist, update quantity
                                        UpdateItemLocQ(testforrecord.Last().ItemLocID, l.Quantity);
                                    }
                                    else
                                    {
                                        //Doesnt exist create new record
                                        int ItemLocID=AddItemLocRecord(l.ItemID, LOCID, l.Quantity);
                                    }
                                }
                            }
                        }

                    }
                    else
                    {
                        result = @" alert('Purchase Save Failed to serial number at line " + count + @"'); ";
                        return result;
                        //Roll back and return
                    }
                   

                    count++;
                }
                

                DocAmt = ItemAmt + transfee + tax + Cash+COGShip+EXPShip;
            }
            else
            {
                result = @" alert('Purchase Save Failed to Save Document!'); ";
                return result;
            }
            //Last Save-Update Document Amount
            if (!(failed))
            {
                success = false;
                D.Amount = DocAmt;
                success = UpdateDocument(D);
            }
            else
            {
                //Roll everything back
                result = @" alert('Purchase Save Failed to Update Document!'); ";
                return result;
            }


            //Receive Items if selected Note:this needs to apply to expense and inventory items.
            //if (ReceiveItems)
            //{
            //    //Receive all items on purchase
            //    //Use external method for this
            //}

            
            if (success)//error message for failure in LineItem block
            {
                //Save Transactions
               
                        //Record Taxes if any
                        if (tax > 0)
                        {
                            //Credit Vender Acct and debit Sales Tax Expense Account
                            int TaxExpAcctID = (from te in db.TaxRates where te.LocationName == TaxLocation select te).Single().ExpenseAcctID;
                            //Use transaction type 20 for tax
                            TransID = SaveTransaction(VendAcct, 20, DocID, 10, "Tax on Purchase", tax);
                           
                        }
                    
            }
            else
            {
                //Roll everything back
                result = @" alert('Purchase Save Failed to Update Document!'); ";
                return result;
            }
            if (!(failed))
            {

                if (transfee > 0)
                {
                    TransID = 0;
                    //Record Transaction Fee if any
                    //Credit Vender Acct and debit Transaction Fee Account (2)
                    //Use transaction type 21 for Transfer Fee
                    TransID = SaveTransaction(VendAcct, 21, DocID, 2, "Transaction Fee on Purchase", transfee);
                    if (TransID < 1) failed = true;

                }
            }
            else
            {
                result = @" alert('Purchase Save Failed to Save Tax!'); ";
                return result;
            }


                if (!(failed))//Record Cashback if any
                {
                    TransID = 0;
                    if (Cash > 0)
                    {
                        
                        //Credit Vender and debit Cash Received (3)
                        //Use transaction type 22 for Cash Back
                        TransID = SaveTransaction(VendAcct, 22, DocID, 3, "Cash Back to Cash Received", Cash);
                        if (TransID < 1) failed = true;
                    }
                    
                }
               
            
            else
            {
                result = @" alert('Purchase Save Failed to Save Transaction Fee!'); ";
                return result;
            }
            
            if (!(failed))
            //Record item transactions
            {
                //Combine all item amounts with matching item account
                foreach (int I in ItemTransAccts)
                {
                    decimal ItemTransAmt = 0;
                    foreach (ItemAccountAmounts IAA in ItemAcctSums)
                    {
                        if (IAA.AcctID == I) ItemTransAmt = ItemTransAmt + IAA.Amount;
                    }

                    //Record Item Transaction
                    //Use transaction type 23 for Item Purchase
                    TransID = SaveTransaction(VendAcct, 23, DocID, I, "Purchase", ItemTransAmt);
                    if (TransID < 1)
                    {
                        failed = true;
                        //Roll all back
                    }
                }
            }
                
            else
            {
                result = @" alert('Purchase Save Failed to Save Cash Back Amount!'); ";
                //roll everything back
                return result;
            }
            //Record Vendor Payment Transaction
            //Use transaction type 24 for VenderPayment
            if (!(failed))
            {
                TransID = SaveTransaction(Pacct, 24, DocID, VendAcct, "Vendor Payment", DocAmt);
            }
            else
            {
                result = @" alert('Purchase Save Failed to Save Item Transactions!'); ";
                return result;
            }
            //Record Shipping/Freight Charges
            if (!(failed))
            {
                if (COGShip > 0)//Fixed acct number 68(COG Shipping) and transaction type 36 Freight Charge
                {
                    TransID = SaveTransaction(VendAcct, 36, DocID, 68, "COG Shipping", COGShip);
                }
                if (EXPShip > 0)//Fixed acct number 69(Shipping Expense) and transaction type 36 Freight Charge
                {
                    TransID = SaveTransaction(VendAcct, 36, DocID, 69, "Shipping Expense", COGShip);
                }

            }
            else
            {
                result = @" alert('Purchase Save Failed to Save Shipping Charges!'); ";
                return result;
            }
            if (!(failed))//Process serial Numbers
            {
                if (Sns!="[]")
                {
                    try
                    {
                        List<SNObject> SNS = JsnToOsn(Sns);
                        foreach (SNObject so in SNS)
                        {
                            List<ItemSN> allsns = new List<ItemSN>();
                            foreach (string sn in so.sns)
                            {
                                ItemSN newsn = new ItemSN { ItemID = so.ItemID, SN = sn };
                                allsns.Add(newsn);
                            }
                            db.ItemSNs.InsertAllOnSubmit(allsns);
                            db.SubmitChanges();
                        }
                    }
                    catch
                    {
                        failed = true;
                        
                    }
                }
            }
            if (failed)
            {
                //Roll everything back
                result = @" alert('Purchase Save Failed to Save Item Serial Numbers'); ";
                return result;
            }
            return result;
        }
        public int SaveLineItemStatus(int TransLineID, int ItemStatus,int LocID, string SN, int ConditionID)
        {
            MembershipUser currentuser = System.Web.Security.Membership.GetUser();
            int LISID = 0;
            if (SN.Length < 1) SN = "*";
            try
            {
                TransLineItemStatus IS = new TransLineItemStatus { CreatedBy = currentuser.UserName, CreateTime = DateTime.Now, LineID = TransLineID, LineItemStatusID = ItemStatus, LocationID = LocID, SerialNumber = SN,ConditionID=ConditionID};
                db.TransLineItemStatus.InsertOnSubmit(IS);
                db.SubmitChanges();
                LISID = IS.TransLineItemStatusID;
            }
            catch
            {
            }

            return LISID;

        }
        public string SaveNewTaxLoc(string Location, string TaxCode, decimal Rate)
        {
            string result = @" alert('New Tax Location Saved'); ";
            MembershipUser currentuser = System.Web.Security.Membership.GetUser();
            try
            {
                TaxRate newloc = new TaxRate { LocationName = Location, Code = TaxCode, Rate = Rate, CreatedBy = currentuser.UserName, CreateTime = DateTime.Now };
                db.TaxRates.InsertOnSubmit(newloc);
                db.SubmitChanges();
            }
            catch
            {
                result = @" alert('New Tax Location Save Failed!'); ";
            }
            return result;
        }
        public List<DepLine> getDepositLinesFromDocID(int DocID)
        {
            List<DepLine> result = new List<DepLine>();
            List<DepositLine> lines = (from dl in db.DepositLines where dl.DepositDocID == DocID select dl).ToList();
            foreach (DepositLine l in lines)
            {
                TransactionTable T = getTransaction(l.TransID);

                result.Add(new DepLine { AMT = T.Amount, TransType = getTransactionTypeFromID( T.TransactionTypeID), PayerName = getAccountFromID(T.CreditID).AcctName,DepLineID=l.DepositLineID});
            }


            return result;
        }
        public DepositLine getDepositLineFromDepLineID(int DepLineID)
        {
            return (from dl in db.DepositLines where dl.DepositLineID == DepLineID select dl).Single();
        }
        public int getTaxRateIDFromName(string TaxRateName)
        {
            return (from tn in db.TaxRates where tn.LocationName == TaxRateName select tn).Single().TaxLocID;
        }
        public string RecordSale(DateTime SDate, int CustAcct, string items, decimal tax, int PayMethID, bool Paid, string TaxRateName, decimal TaxRate)
        {
            //ALL:Save Invoice Document, Save Line Items
            //UNPAID:
            //PAID:
            string result = @" alert('Sale Saved'); ";
            int DocTypeID = 7;//7 is invoice
            int LineItemType = 2; //Type 2 is for converting to invoice line types
            
            List<string> jsitems = ParcJSdata(items);
            List<LineItem> lineitems = ConvertToLineItems(jsitems,LineItemType);
            bool failed = false; int TransID = 0; decimal ItemAmt = 0; int count = 0; int LineID = 0; bool success = false; decimal DocAmt = 0;
            List<ItemAccountAmounts> RevAcctSums = new List<ItemAccountAmounts>();
            List<CogsInvAmount> CogsInvAcctsSums = new List<CogsInvAmount>();
            List<int> ItemRevAccts = new List<int>();
            List<ItemInvCogsAccts> ItemCogsInvAccts = new List<ItemInvCogsAccts>();
            string Description = "INVOICE";
            if (Paid) Description = "RECIEPT";
            int TaxLocID=0;
            if(tax>0) TaxLocID = (from te in db.TaxRates where te.LocationName == TaxRateName select te).Single().TaxLocID;
            #region Document and line Item entries
            
            //Record Document----------------------------------------------------------------------------------
            Document D = new Document { Amount = DocAmt, DocumentReference = "*", DocumentTime = SDate, DocumentTypeID = DocTypeID, PaymentMethodID = PayMethID, Reconciled = false, DocumentPath = "" };
            int DocID = InsertDocument(D);
            if (DocID > 0)
            {
                foreach (LineItem l in lineitems)
                {
                    ItemTypeRevExp ItemAccts = getItemAccts(l.ItemID);
                    int ItemTypeInvDesignator = getItemInvDesignator(l.ItemID);
                    decimal LineTotal = Math.Round(l.Price * l.Quantity, 2);
                    ItemAmt = ItemAmt + LineTotal;

                    //Record Transaction Line Item
                    if (!(failed))
                    {

                        LineID = SaveLineItem(DocID, l);
                        l.TransactionLineID = LineID;
                        if (LineID < 1) failed = true;
                        if (!(failed)&&l.JobLineID>0)//Set Jobline indocument to true
                        {
                            try
                            {
                                JobLine JL = getJobLineFromID(Convert.ToInt32(l.JobLineID));
                                JL.InDocument = true;
                                db.SubmitChanges();
                            }
                            catch
                            {
                                failed = true;
                            }
                        }

                    }
                    else
                    {
                        result = @" alert('New Sale Save Failed to Save Line Item " + count + @"'); ";
                        return result;
                        //Roll back and return
                    }
                    if (!(failed)) //If Service Credit Deduction record job description
                    {
                        
                        
                    }
                    else
                    {
                        result = @" alert('New Sale Save Failed to Save Line Item " + count + @"'); ";
                        //Roll back and return
                        return result;
                    }
                    //Accumulate Item Type Account and totals for Item account transaction entries and adjust inventory location quantities for inventory items
                    if (!(failed))
                    {

                        ItemInvCogsAccts testacct = new ItemInvCogsAccts { CogsAcctID = ItemAccts.CoGAcctID, InvAcctID = ItemAccts.InvAcctID };
                        RevAcctSums.Add(new ItemAccountAmounts { AcctID = ItemAccts.RevAcctID, Amount = LineTotal });
                        CogsInvAcctsSums.Add(new CogsInvAmount { InvCogsAcct=testacct, Amount = LineTotal });
                        
                        if (!(ItemCogsInvAccts.Contains(testacct))) ItemCogsInvAccts.Add(testacct);
                        if (!(ItemRevAccts.Contains(ItemAccts.RevAcctID))) ItemRevAccts.Add(ItemAccts.RevAcctID);

                        //Check if item type is pysical inventory (1) and if so add line items to inventory received location (1)
                        if (ItemTypeInvDesignator == 1)
                        {
                            //Check receiving for pre-existing items

                            var testforrecord = from its in db.ItemLocs where its.ItemID == l.ItemID && its.LocID == 1 select its;

                            if (testforrecord.Count() > 0)
                            {
                                ItemLoc Existing = (from its in db.ItemLocs where its.ItemID == l.ItemID && its.LocID == 1 select its).Single();
                                //add line quantity to existing location record
                                decimal newquan = Existing.Quantity + l.Quantity;
                                success = UpdateItemLocQ(Existing.ItemLocID, newquan);
                            }
                            else
                            {
                                //insert new record
                                ItemLoc newitemloc = new ItemLoc { ItemID = l.ItemID, LocID = 1, Quantity = l.Quantity };
                                success = InsertItemLoc(newitemloc);
                            }


                            if (failed)
                            {

                                result = @" alert('Purchase Save Failed to Save Inventory to receiving!'); ";
                                //roll back all saves
                                return result;
                            }
                        }

                    }

                    else
                    {
                        result = @" alert('Purchase Save Failed to serial number at line " + count + @"'); ";
                        return result;
                        //Roll back and return
                    }

                    //Check if item type is prepay purchase (65) and if so add the credits
                    if (IsPrepayItem(l.ItemID))
                    {
                        int SCID = AddServiceCredits(l.ItemID, l.Quantity,CustAcct,SDate);
                    }


                    count++;


                }

                DocAmt = ItemAmt + tax;
            }
            else
            {
                result = @" alert('Purchase Save Failed to Save Document!'); ";
                return result;
            }
            //Last Save-Update Document Amount and Make Reference the invoice/receipt number which is
            if (!(failed))
            {
                success = false;
                D.Amount = DocAmt;
                D.DocumentReference = DocID.ToString() + D.TimeEntered.Month.ToString() + D.TimeEntered.Year.ToString();
                success = UpdateDocument(D);
            }
            else
            {
                //Roll everything back
                result = @" alert('Purchase Save Failed to Update Document!'); ";
                return result;
            }

            #endregion

            if (success)//error message for failure in LineItem block
            {
                //Save Transactions

                //Record Taxes if any
                if (TaxLocID > 0)
                {
                    
                    if (Paid) //Record in Tax Liability (Fixed Acct number 12) comment tax id collected for
                    {
                        TransID = 0;
                        TransID = SaveTransaction(12, PayMethID, DocID, CustAcct, @"Tax for: " + TaxRateName + @":" + TaxLocID, tax);
                        if (TransID > 0) success = true;
                        else success = false;
                    }
                    else //Record entry in Tax Collections Table
                    {
                        TaxCollection TC = new TaxCollection { DocID = DocID, TaxRateID = TaxLocID, TaxAmount = tax,Collected=true };
                        success = InsertTaxCollection(TC);
                    }
                }

            }
            else
            {
                //Roll everything back
                result = @" alert('New Sale Save Failed to Update Document!'); ";
                return result;
            }

            if (success)
            {

                //Combine all Rev item amounts with matching Rev item account
                TransID = 0;
                foreach (int I in ItemRevAccts)
                {
                    decimal ItemTransAmt = 0;
                    foreach (ItemAccountAmounts IAA in RevAcctSums)
                    {
                        if (IAA.AcctID == I) ItemTransAmt = ItemTransAmt + IAA.Amount;
                    }
                    int eqrev = I;
                    if (!(Paid)) eqrev = 74; //Revenue Equity account if unpaid else stay as revenue for item
                    
                    //Record Customer Receivable Transaction
                    //Use transaction type 26 for Item Sale
                    TransID = SaveTransaction(eqrev, 26, DocID, CustAcct, Description, ItemTransAmt);
                    if (TransID < 1)
                    {
                        failed = true;
                        //Roll all back
                    }

                    
                }



            }
            else
            {
                result = @" alert('New Sale Save Failed to Save Customer Tax!'); ";
                return result;
            }

            if (!(failed))
            {
            if (Paid)
            {
                


                //Record Inventory and Cogs Transactions
                //Note!!!!!!!! Cog should not be calculated for each transaction. Purchases and sales should be recorded in the same units and then Cog is calculated at any time based on total units sold against the average cost of the same number of units purchased.
                //if (!(failed))
                //{
                //    TransID = 0;
                //    //Combine all CogsInv item amounts with matching CogsInvAccount item account
                //    foreach (ItemInvCogsAccts I in ItemCogsInvAccts)
                //    {
                //        decimal ItemTransAmt = 0;
                //        foreach (CogsInvAmount CIA in CogsInvAcctsSums)
                //        {
                //            if (CIA.InvCogsAcct == I) ItemTransAmt = ItemTransAmt + CIA.Amount;
                //        }

                //        //Record Inventory Cogs Transaction
                //        //Use transaction type 28 for InventoryCOGS
                //        TransID = SaveTransaction(I.CogsAcctID, 28, DocID, I.InvAcctID, "*", ItemTransAmt);
                //        if (TransID < 1)
                //        {
                //            failed = true;
                //            //Roll all back
                //        }
                //    }
                //}
                //else
                //{
                //    result = @" alert('New Sale Save Failed to Save Receivables!'); ";
                //    return result;
                //}
                //Save Customer Payment if Paid

                
                  // TransID = 0;

                    //Record customer payment
                    //If payment method is cash Debit Cash else Debit Payments received and Credit Customer account
                    //Cash payment Transaction type 30
                  //  if (PayMethID == 6) TransID = SaveTransaction(1, 30, DocID, CustAcct, "*", DocAmt - tax);
                 //   if (TransID < 1) failed = true;


                
                //if (!(failed))
                //{
                //    TransID = 0;
                //    // Move any tax from collected to Tax Liability Account
                //    if (TaxLocID > 0)
                //    {
                //        int TaxExpID = getTaxExpID(TaxLocID);
                //       //Customer Tax Payment Transaction type is 32
                //    TransID = SaveTransaction(CustAcct, 32, DocID, TaxExpID, "*", tax);
                //    if (TransID < 1) failed = true;
                //        //Remove Tax from tax collected
                //    success = DelTaxCollected(DocID);
                //    if (!(success)) failed = true;
                //    }

                //    if (failed)
                //    { 
                //        result = @" alert('New Sale Save Failed to Save Customer Tax Payment!'); ";
                //        //Rollback all
                //        return result;
                //    }
                //}
                //else
                //{
                //    //Roll everything back
                //    result = @" alert('New Sale Save Failed to Save Customer Payment!'); ";
                //    return result;
                //}
            }
            else //Not Paid
            {

            }
            }
            else
            {
                //Roll everything back
                result = @" alert('New Sale Save Failed to Save Inventory and CostOfGoodsSold!'); ";
                return result;
            }
            if (!(failed)) //Turn result into printable/emailable receipt or invoice
            {
                

                if (Paid)//Make Receipt
                {
                    result = @"window.open('/Accounting/ShowReceipt/?DocID=" + DocID + @"');";
                }
                else//Make Invoice
                {
                    result = "window.open('/Accounting/ShowInvoice/?DocID=" + DocID + @"');";
                }
            }
            return result;
        }
        public string RecordServiceCreditDeduction(DateTime SDate, int CustAcct, string items, int PayMethID, int JobID)
        {
            
            string result = @" alert('Credit Deductions Saved'); ";
            int DocID = 0;
            bool AutoAddJobInfo = false;
            string User = Membership.GetUser().UserName;
            List<string> jsitems = ParcJSdata(items);
            //Change this to job line
            List<LineItem> lineitems = ConvertToLineItems(jsitems, 3);//For Service Credit Deduction Line type(includes location)
            bool failed = false; int TransID = 0; decimal ItemAmt = 0; int count = 0; int LineID = 0; bool success = false; decimal DocAmt = 0;
            Document D = new Document { Amount = DocAmt, DocumentReference = @"Job#:" + JobID.ToString(), DocumentTime = SDate, DocumentTypeID = 15, PaymentMethodID = PayMethID, Reconciled = true, DocumentPath = "" };
            string Description = "SERVICE CREDIT DEDUCTION";
            if (JobID==0)//create job
            {
                AutoAddJobInfo = true;
                Job NJ = new Job { CustID = CustAcct, JobDescription = "Entered From Payment", Completed = true, DueDate = DateTime.Now, EnterDate = DateTime.Now, EnteredBy = User };
                JobID = InsertJob(NJ);
                if (JobID > 0) success = true;
                else return @" alert('Failed to add new job for service credits'); ";
            }
            
            #region Document and Job Item entries

            //Record Document----------------------------------------------------------------------------------
            if (success)
            {
                success = false;
                
                DocID = InsertDocument(D);
                if (DocID > 0) success = true;
                else return @" alert('Failed to add new Document'); ";
            }
            if (success)
            {
                
                foreach (LineItem l in lineitems)
                {
                    
                    decimal LineTotal = Math.Round(l.Price * l.Quantity, 2);
                    ItemAmt = ItemAmt + LineTotal;
                    if (AutoAddJobInfo)
                    {
                        //Record Job Line
                        if (success)
                        {
                            
                            l.JobID = JobID;
                            l.TimeIn = DateTime.Now;// until done right
                            l.TimeOut = DateTime.Now;// until done right

                            LineID = InsertJobLine(l);
                            

                            if (LineID > 0) success = true;
                            else return @" alert('Failed to Save JobLine " + count + @"'); ";

                        }
                        else
                        {
                            result = @" alert('Failed to Save JobLine " + count + @"'); ";
                            return result;
                            //Roll back and return
                        }
                    }
                    if (success) //Record Service Credit deduction
                    {
                        decimal LineAmt = (l.Quantity * l.Price * -1);
                        
                        ServiceCredit SC = new ServiceCredit { AcctID = CustAcct, ServiceType =l.ServiceCreditTypeID, JobLineID = LineID, DocID = DocID, ChangeDate = SDate, Amount = LineAmt, EnteredBy = User, EntryDate = DateTime.Now };
                        SaveServiceCredits(SC);
                        if (SC.SCID > 0) success = true;
                        else return @" alert('Failed to Record Service Credit Deduction " + count + @"'); ";


                    }
                    else
                    {
                        return @" alert('Failed to Record Job Line " + count + @"'); ";
                        //Roll back and return
                        
                    }
                    

                    count++;


                }

                DocAmt = ItemAmt;
            }
            else
            {
                result = @" alert('Service Credit Deduction Failed to Save Document!'); ";
                return result;
            }
            //Last Save-Update Document Amount and Make Reference the invoice/receipt number which is
            if (!(failed))
            {
                success = false;
                D.Amount = DocAmt;
                D.DocumentReference = DocID.ToString() + D.TimeEntered.Month.ToString() + D.TimeEntered.Year.ToString();
                success = UpdateDocument(D);
            }
            else
            {
                //Roll everything back
                result = @" alert('Purchase Save Failed to Update Document!'); ";
                return result;
            }

            #endregion
            if (success) //Turn result into printable/emailable service credit deduction statement
            {

                    result = "window.open('/Accounting/ShowSCDeduction/?DocID=" + DocID + @"');";
 
            }
           
            return result;
        }
        public int RecordCustomerTax(TaxCollection TC)
        {
            int result = 0;
            try
            {
                db.TaxCollections.InsertOnSubmit(TC);
                db.SubmitChanges();
                result = TC.TaxCollectID;
            }
            catch { }
            return result;
        }
        public List<TaxCollection> getUncollectedTaxes(int DocID)
        {
            return (from TC in db.TaxCollections where TC.DocID == DocID && TC.Collected==false select TC).ToList();
        }
        public bool getItemIsSerialized(int ItemID)
        {
            return (from i in db.Items where i.ItemID == ItemID select i).Single().Serialized;
        }
        public bool InsertItemLoc(ItemLoc IL)
        {
            bool result = false;
            try
            {
                db.ItemLocs.InsertOnSubmit(IL);
                db.SubmitChanges();
                result = true;
            }
            catch 
            {
                
               
            }

            return result;
        }
        public bool InsertItemSN(ItemSN itemsn)
        {
            bool result = false;
            try
            {
                db.ItemSNs.InsertOnSubmit(itemsn);
                db.SubmitChanges();
                result = true;
            }
            catch 
            {

            }
            return result;
        }
        public ItemTypeRevExp getItemAccts(int ItemID)
        {
            int ItemTypeID=(from it in db.Items where it.ItemID == ItemID select it).Single().ItemTypeID;
            return (from ei in db.ItemTypeRevExps where ei.ItemTypeID == ItemTypeID select ei).Single();
        }
        public Item getItemFromID(int ItemID)
        {
            return (from I in db.Items where I.ItemID == ItemID select I).Single();
        }
        public int getItemInvDesignator(int ItemID)
        {
            return (from result in db.ItemTypes where result.ItemTypeID == ((from it in db.Items where it.ItemID == ItemID select it).Single().ItemTypeID) select result).Single().InvDesignation;
        }
        public bool InsertTaxCollection(TaxCollection TC)
        {
            bool result = true;
            try
            {
                db.TaxCollections.InsertOnSubmit(TC);
                db.SubmitChanges();
            }
            catch { result = false; }
            return result;
        }
        public int getTaxExpID(int TaxLocID)
        {
            return (from te in db.TaxRates where te.TaxLocID == TaxLocID select te).Single().ExpenseAcctID;
        }
        public bool DelTaxCollected(int DocID)
        {
            try
            {
                TaxCollection deltax = (from dt in db.TaxCollections where dt.DocID == DocID select dt).Single();
                db.TaxCollections.DeleteOnSubmit(deltax);
                db.SubmitChanges();
                return true;
            }
            catch
            {

                return false;
            }
        }
        public List<int> getUserAcctSumView()
        {
           string user = Membership.GetUser().UserName;
           return (from uv in db.UserActSumViews where uv.UserName == user select uv.AcctTypeID).ToList();
        }
        public bool getOneUserAcctSumView(int AcctTypeID)
        {
            try
            {
                UserActSumView testview = (from v in db.UserActSumViews where v.AcctTypeID == AcctTypeID && v.UserName == Membership.GetUser().UserName select v).Single();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool ToggleUserSumView(int AcctType, bool Showit)
        {
            bool result = true;
            UserActSumView uv = new UserActSumView();
            string user = Membership.GetUser().UserName;
            try
            {
                try
                {
                    uv = (from v in db.UserActSumViews where v.UserName == user && v.AcctTypeID == AcctType select v).Single();
                    db.UserActSumViews.DeleteOnSubmit(uv);
                }
                catch
                {
                    uv.UserName = user; uv.AcctTypeID = AcctType;
                    db.UserActSumViews.InsertOnSubmit(uv);
                }
                db.SubmitChanges();
            }
            catch
            {
                result = false;
            }
            return result;
        }
        public bool ChangeDateRange(string StartDate, string EndDate)
        {
            bool result = true;
            UserDateRange UDR = new UserDateRange();
            string user = Membership.GetUser().UserName;
            if ((StartDate == "") && (EndDate == "")) result = false;
            else
            {
                try//check for existing record and add if it does not exist
                {
                    UDR = (from u in db.UserDateRanges where u.Username == user select u).Single();
                }
                catch
                {
                    UDR.Username = user;
                    db.UserDateRanges.InsertOnSubmit(UDR);
                    db.SubmitChanges();
                    UDR = (from u in db.UserDateRanges where u.Username == user select u).Single();
                }
                if (!(StartDate == ""))
                {
                    try
                    {
                        UDR.StartDate = Convert.ToDateTime(StartDate);
                    }
                    catch
                    {
                        result = false;
                    }
                }
                if (!(EndDate == ""))
                {
                    try
                    {
                        UDR.EndDate = Convert.ToDateTime(EndDate);
                    }
                    catch
                    {
                        result = false;
                    }
                
                }
                try
                {
                    db.SubmitChanges();
                }
                catch
                {
                    result = false;
                }

            }

            return result;
        }
        public DateTime? getAPstartDate()
        {
            //set default start time to include entries all the way back to the beginning
            DateTime? result = new DateTime(1900, 1, 1);//use 1900 to garrantee earlier than any date in system
            
            string user = Membership.GetUser().UserName;
            try
            {
                result= (from sd in db.UserDateRanges where sd.Username == user select sd.StartDate).Single();
            }
            catch
            {
                //Nothing occurs, result keeps initial value if no value is in the table
            }
            return result;
        }
        public DateTime? getAPendDate()
        {
            //Set default date to include dates all the way to present
            DateTime? result = DateTime.Now;
            string user = Membership.GetUser().UserName;
            try
            {
                result = (from sd in db.UserDateRanges where sd.Username == user select sd.EndDate).Single();
            }
            catch
            {
                //Nothing occurs, result keeps initial value if no value is in the table
            }
            return result;
        }
        public List<Item> getSaleItems()
        {
            //items determine by item type InvDesignator 1=inventory type and 2=labor type and 3=resellable service
            //Get Item Type List
            
            List<Item> result = (from item in db.Items join it in db.ItemTypes on item.ItemTypeID equals it.ItemTypeID where it.InvDesignation == 1 || it.InvDesignation == 2 || it.InvDesignation == 3 orderby item.ItemName select item).ToList();
            return result;

        }
        public List<OptionList> GetOptionList(string type)
        {
            List<OptionList> result = new List<OptionList>();
            switch (type)
            {
                case "PUBLISHER":
                    List<BookPub> Pub = (from PU in db.BookPubs orderby PU.Name select PU).ToList();
                    foreach (BookPub BP in Pub)
                    {
                        result.Add(new OptionList { Ovalue = BP.BookPubID.ToString(), Otext = BP.Name });
                        
                    }
                    
                    break;
                case "LOCATION":
                    List<Location> Loc = (from LO in db.Locations orderby LO.LocLabel select LO).ToList();
                    foreach (Location L in Loc)
                    {
                        result.Add(new OptionList { Ovalue = L.LocID.ToString(), Otext = L.LocLabel });
                    }
                    
                    break;
                case "OWNER":
                    List<Owner> Own = (from OW in db.Owners orderby OW.Name select OW).ToList();
                    foreach (Owner O in Own)
                    {
                        result.Add(new OptionList { Ovalue = O.OwnerID.ToString(), Otext = O.Name });
                    }
                    
                    break;
                case "SOFTWAREPUB":
                    List<SofwarePub> SPub = (from SPU in db.SofwarePubs orderby SPU.Name select SPU).ToList();
                    foreach (SofwarePub SP in SPub)
                    {
                        result.Add(new OptionList { Ovalue = SP.SoftPubID.ToString(), Otext = SP.Name });
                    }
                    
                    break;
                case "MEDIA":
                    List<MediaType> Med = (from ME in db.MediaTypes orderby ME.MediaTypeName select ME).ToList();
                    foreach (MediaType media in Med)
                    {
                        result.Add(new OptionList { Ovalue = media.MediaTypeID.ToString(), Otext = media.MediaTypeName });
                    }
                    
                    break;
                case "ITEMTYPE":
                    List<ItemType> It = (from Ity in db.ItemTypes orderby Ity.Name select Ity).ToList();
                    foreach (ItemType IT in It)
                    {
                        result.Add(new OptionList { Ovalue = IT.ItemTypeID.ToString(), Otext = IT.Name });
                    }
                    
                    break;
                case "MANUFACTURERID":
                    List<ItemManufacturer> MAN = (from mn in db.ItemManufacturers orderby mn.Name select mn).ToList();
                    foreach (ItemManufacturer man in MAN)
                    {
                        result.Add(new OptionList { Ovalue = man.MaunufacturerID.ToString(), Otext = man.Name });
                    }
                    
                    break;
                case "LOCTYPE":
                    List<LocationType> LT = (from lt in db.LocationTypes orderby lt.Name select lt).ToList();
                    foreach (LocationType loct in LT)
                    {
                        result.Add(new OptionList { Ovalue = loct.LocTypeID.ToString(), Otext = loct.Name });
                    }
                    
                    break;
                case "EXPENCES":
                    List<AccountTable> AT = (from at in db.AccountTables where at.AcctTypeID == 9 orderby at.AcctName select at).ToList();
                    foreach (AccountTable acct in AT)
                    {
                        result.Add(new OptionList { Ovalue = acct.AcctID.ToString(), Otext = acct.AcctName });
                    }
                    
                    break;
                case "PAYMETH":
                    List<PayMethod> PM = (from p in db.PayMethods orderby p.PayMeth select p).ToList();
                    foreach (PayMethod pmeth in PM)
                    {
                        result.Add(new OptionList { Ovalue = pmeth.PayMethID.ToString(), Otext = pmeth.PayMeth });
                    }
                    
                    break;
                case "CONTACTS":
                    List<Contact> C = (from c in db.Contacts orderby c.LastName select c).ToList();
                    foreach (Contact con in C)
                    {
                        result.Add(new OptionList { Ovalue = con.ContactID.ToString(), Otext = con.FirstName+con.LastName });
                    }

                    break;
                    case "SCDEDUCTIONS":
                    List<ServiceCredit> SC = (from sc in db.ServiceCredits orderby sc.AcctID select sc).ToList();
                    List<int?> DocIDs = new List<int?>();
                    foreach (ServiceCredit sc in SC)
                    {
                        if (!(DocIDs.Contains(sc.DocID)))
                        {
                            DocIDs.Add(sc.DocID);
                            result.Add(new OptionList { Ovalue = sc.DocID.ToString(), Otext = sc.AcctID + "-" + sc.ChangeDate.ToShortDateString() });
                        }
                    }

                    break;
            }
            return result;
        }
        public List<int> getSerialedItems()
        {
            return (from item in db.Items where item.Serialized == true select item.ItemID).ToList();
        }
        public List<DepositItem> JSDepItemListToDepositItems(List<string> JSDepItemList)
        {
            List<DepositItem> DI = new List<DepositItem>();
            foreach (string js in JSDepItemList)
            {
                string item = js;
                
                
                

                int TransID = Convert.ToInt32(item.Substring(1, (item.IndexOf('■') - 1)));
                item = item.Substring(item.IndexOf('■')+1);
                int AcctID = Convert.ToInt32(item.Substring(0, item.IndexOf('■')));
                item = item.Substring(item.IndexOf('■') + 1);
                string AcctName = item.Substring(0, item.IndexOf('■'));
                item = item.Substring(item.IndexOf('■') + 1);
                int DepTypeID = Convert.ToInt32(item.Substring(0, item.IndexOf('■')));
                item = item.Substring(item.IndexOf('■') + 1);
                item = item.Substring(item.LastIndexOf('■')+1);
                item = item.Substring(0, item.Length - 1);//Remove last currly bracket
                string SAmount = item;
                decimal Amount = Convert.ToDecimal(SAmount);
                
               DI.Add(new DepositItem { ItemTransID=TransID, ItemAcctID=AcctID,AcctName=AcctName,DepositType=DepTypeID,Amount=Amount});
            }

            return DI;
        }
        public List<SNObject> JsnToOsn(string Jstring)
        {
            List<SNObject> result = new List<SNObject>();

            
            List<string> FirstArray = ParseJsonObjectArray(Jstring);
            foreach (string s in FirstArray)
            {
                SNObject S = new SNObject();
                int IDstart = s.IndexOf("ItemID") + 8;
                int IDend = s.IndexOf(",", IDstart)-IDstart;
                int SNsStart = s.IndexOf("sns") + 5;
                int SNsend = s.IndexOf("]", SNsStart) - SNsStart+1;
                string test= s.Substring(IDstart,IDend);
                string SNstring = s.Substring(SNsStart, SNsend);
                S.sns = ParseJsonArray(SNstring);
                S.ItemID = Convert.ToInt32(test);
                result.Add(S);
            }
            return result;
        }
        public List<string> JSArrayToList(string JObjectArray)
        {
            List<string> result = new List<string>();

            

            while (JObjectArray.Length>1) 
            {

                
                string temp = JObjectArray.Substring(0, (JObjectArray.IndexOf('}')+1));

                result.Add(temp);
                JObjectArray = JObjectArray.Substring(temp.Length);
                


            }

            

            return result;
        }
        public List<string> ParseJsonObjectArray(string JObjectArray)
        {
            List<string> result = new List<string>();

            int start = 1;
            
            while (start>0) //break up json string data array
            {

                int end = JObjectArray.IndexOf("}", start) - start + 1;
                string temp = JObjectArray.Substring(start, end);

                result.Add(temp);

                start = JObjectArray.IndexOf("{", start + 1);
                
                
            }

            //One object in string
            
            return result;
        }
        public List<string> ParseJsonArray(string Jarray)
        {
            List<string> result = new List<string>();

            int start = 2;
            while (start > 0) //break up json string data array
            {

                int end = Jarray.IndexOf("\"", start+1)-start;


                result.Add(Jarray.Substring(start, end));

                start = Jarray.IndexOf("\"", start + 1);//end quote
                start = Jarray.IndexOf("\"", start + 1)+1;//start quote
            }

            return result;
        }

        public List<VendItem> getVenderItemList(int VendID)
        {
            return (from vi in db.VendItems where vi.VendID == VendID select vi).ToList();
            
        }
        public string MakeOptionList(string ListType)
        {
            string result = "<option value=0>DEFAULT</option>";
            switch (ListType)
            {
                case "EXPENSES":
                    List<AccountTable> items = getAccounts(9);
                    
                    foreach (var item in items)
                    {
                        result = result + @"<option value=" + item.AcctID + @">" + item.AcctName + @"</option>";
                    }
                    break;

                default:
                    break;
            }
            return result;
        }
        public void UpdateVendItemInfo(int VendAcct,LineItem l)
        {
            try
            {
                VendItem check = (from vi in db.VendItems where vi.ItemID == l.ItemID && vi.VendID == VendAcct select vi).Single();
                check.LastCost = l.Price;
                db.SubmitChanges();
            }
            catch
            {
                try
                {
                    VendItem addit = new VendItem { ItemID = l.ItemID, VendID = VendAcct, LastCost = l.Price };
                    db.VendItems.InsertOnSubmit(addit);
                    db.SubmitChanges();
                }
                catch { }
            }
        }
        public Document getDocumentFromReference(string Reference)
        {
            Document result = new Document();
            result.DocumentID = 0;
            try
            {
                result = (from d in db.Documents where d.DocumentReference == Reference select d).Single();
            }
                //if doc with more than one document exist with reference then returns blank doc with 0 for ID
            catch { }
            return result;
        }
        public List<Document> getOutstandingInvoices(int AcctID)
        {
            List<Document> result = new List<Document>();
            List<Document> AllDocs = getDocsFromAcctID(AcctID);
            foreach (Document di in AllDocs)//Invoice has document type ID of 7
            {
                if ((!(di.Reconciled)) && di.DocumentTypeID==7) result.Add(di);
            }
            return result;
        }
        
        public List<Document> getDocsFromAcctID(int AcctID)
        {
            //Find credits and debits that = the AcctID and send a list of the DocIDs associated with them but don't duplicate any
            List<int> DocIDs = (from T in db.TransactionTables where T.CreditID == AcctID || T.DebitID == AcctID select T.DocumentID).ToList();
            List<Document> result=new List<Document>();
            foreach (int I in DocIDs)
            {
                Document D=(from d in db.Documents where d.DocumentID==I select d).Single();
                if(!(result.Contains(D))) result.Add(D);
            }
            return result;

        }
        public List<int> getOutStandingDocIDs(int AcctID)
        {
            return (from docs in db.ReceivableDocs where docs.AcctID == AcctID && docs.Paid == false select docs.DocID).ToList();
        }
        public TransDocument MakeTransDocument(int DocID, string Reference, int AcctID, int PayMethID, DateTime DocDateTime, List<LineItem> lines, string TaxName, decimal taxrate, decimal tax, decimal TotalAmt)
        {
            TransDocument result = new TransDocument();
            try//get contact info
            {
                Contact PrimContact = (from c in db.Contacts where c.AcctID == AcctID select c).Single();
                result.Address = PrimContact.Address; result.City = PrimContact.City; result.State = PrimContact.State;
                result.Zip = PrimContact.PostalCode; result.Phone = PrimContact.Phone; result.EmailTo = PrimContact.Email;
            }
            catch
            {
                result.Address = "*"; result.City = "*"; result.State = "*";
                result.Zip = "*"; result.Phone = "*"; result.EmailTo = "*";
            }
            result.CustVendName = getAccountFromID(AcctID).AcctName;
            result.Date = DocDateTime.ToShortDateString();
            result.Time = DocDateTime.ToShortTimeString();
            result.DocID = DocID; result.DocumentNumber = DocDateTime.Month.ToString() + DocDateTime.Year.ToString() + DocID;
            result.items = lines; result.PayMeth = (from pm in db.PayMethods where pm.PayMethID == PayMethID select pm.PayMeth).Single();
            result.Reference = Reference; result.Tax = tax; result.TaxLocName = TaxName; result.TaxRate = taxrate.ToString();
            result.Total = TotalAmt;
            return result;
        }
        public string StringIfyTransDoc(TransDocument T)
        {
            //alt 200 = ╚ used as delimeter
            return T.Address + "╚" + T.City + "╚" + T.CustVendName + "╚" + T.Date + "╚" + T.DocID + "╚" + T.DocumentNumber + "╚" + T.EmailTo + "╚" + T.PayMeth + "╚" + T.Phone + "╚";
        }
        public TransDocument getTransDoc(int DocID)
        {
            TransDocument TD = new TransDocument();
            Document D = (from doc in db.Documents where doc.DocumentID == DocID select doc).Single();
            TD.Date = D.DocumentTime.ToShortDateString(); TD.DocID = DocID; TD.Total = D.Amount; TD.Time = D.TimeEntered.ToShortTimeString(); TD.PayMeth = (from PM in db.PayMethods where PM.PayMethID == D.PaymentMethodID select PM.PayMeth).Single(); TD.Reference = D.DocumentReference;
            
            // Get ItemSale Transaction (26)
            TransactionTable ISTrans = getItemsSaleTrans(DocID);
            Contact C = getPrimaryContact(ISTrans.DebitID);
            TD.CustVendAcctID = ISTrans.DebitID;
            TD.CustVendName = getAccountFromID(ISTrans.DebitID).AcctName;
            TD.Address = C.Address; TD.City = C.City; TD.EmailTo = C.Email; TD.Phone = C.Phone; TD.State = C.State; TD.Zip = C.PostalCode;
            //get transaction lines and tax info
            List<TransLineItem> items = getTransLinesByDocID(DocID);
            List<LineItem> litems = new List<LineItem>();
            foreach (TransLineItem TLI in items)
            {
                //what if item is serialized
                string IN = GetItemNameFromID(TLI.ItemID);
                litems.Add(new LineItem { ItemID = TLI.ItemID, Price = TLI.Price, Quantity = TLI.Quantity,ItemName=IN,JobLineID=TLI.JobLineID,Taxable=TLI.TaxPaid,Description=TLI.ItemNote});//Tax paid actually means tax charged
            }
            TD.items = litems;
            
            TaxCollection taxinfo = getDocTax(DocID);
            if (taxinfo.DocID == DocID)//the doc has tax, add to TD
            {
                TD.Tax = taxinfo.TaxAmount;
                TaxRate TR = getTaxRateFromID(taxinfo.TaxRateID);
                TD.TaxRate = TR.Rate.ToString(); TD.TaxLocName = TR.LocationName; TD.TaxLocID = TR.TaxLocID;
            }
            return TD;
        }
        public TransDocument getSCDeductionDoc(int DocID)
        {
            TransDocument TD = new TransDocument();
            Document D = (from doc in db.Documents where doc.DocumentID == DocID select doc).Single();
            TD.Date = D.DocumentTime.ToShortDateString(); TD.DocID = DocID; TD.Total = D.Amount; TD.Time = D.TimeEntered.ToShortTimeString(); TD.PayMeth = (from PM in db.PayMethods where PM.PayMethID == D.PaymentMethodID select PM.PayMeth).Single(); TD.Reference = D.DocumentReference;
            
            
            

            //Get Service Credits Info\
            
            List<ServiceCredit> SC = getServiceCreditsFromDocID(DocID);
            List<LineItem> SCLines = new List<LineItem>();
            TD.CustVendAcctID = SC[0].AcctID;
            List<Prepaid> CustPPs = getCustPrepays(SC[0].AcctID);
            Contact C = getPrimaryContact(TD.CustVendAcctID);
            TD.CustVendName = getAccountFromID(TD.CustVendAcctID).AcctName;
            TD.Address = C.Address; TD.City = C.City; TD.EmailTo = C.Email; TD.Phone = C.Phone; TD.State = C.State; TD.Zip = C.PostalCode;
            //Make Lines
            foreach (ServiceCredit sc in SC)
            {
                LineItem scline = new LineItem { DateOfService = sc.ChangeDate, ServiceCreditTypeName = getServiceTypeNameFromID(sc.ServiceType) };
                JobLine jl = getJobLineFromID(sc.JobLineID);
                scline.Price = jl.HourRate; scline.Description = jl.Description; scline.ItemName = GetItemNameFromID(Convert.ToInt32(jl.ServiceItemID)); scline.JobID = Convert.ToInt32(jl.JobID); scline.JobLineID = jl.JobLineID; scline.Quantity = jl.TotalTime; scline.TimeIn = Convert.ToDateTime(jl.TimeIn); scline.TimeOut = Convert.ToDateTime(jl.TimeOut);
                foreach (Prepaid pp in CustPPs)
                {
                    if(pp.ServiceTypeID==sc.ServiceType) scline.Balance=pp.Balance;
                }
                SCLines.Add(scline);
            }
            TD.items = SCLines;
            
            return TD;
        }
        public List<ServiceCredit> getServiceCreditsFromDocID(int DocID)
        {
            return (from sc in db.ServiceCredits where sc.DocID == DocID select sc).ToList();
        }
        public string getServiceTypeNameFromID(int STID)
        {
            return (from st in db.ServiceCreditTypes where st.SCTID==STID select st).Single().ServiceType;
        }
        public JobLine getJobLineFromID(int? JLID)
        {
            return (from jl in db.JobLines where jl.JobLineID == JLID select jl).Single();
        }
        public List<JobLine> getJobLinesFromJobID(int JobID)
        {
            return (from J in db.JobLines where J.JobID == JobID select J).ToList();
        }
        public Job getJobFromID(int JobID)
        {
            return (from j in db.Jobs where j.JID==JobID select j).Single();
        }
        public List<AccountTable> getCompanyAccts()//when more than one company exist
        {
            return (from A in db.AccountTables where A.AcctTypeID == 0 select A).ToList();
        }
        public AccountTable getCompanyAcct()//when only one company exist
        {
            return (from A in db.AccountTables where A.AcctTypeID == 0 select A).Single();
        }
        public Contact getPrimaryContact(int AcctID)
        {
            //Type 1 is primary contact
            Contact result = new Contact();
            try
            {
                result = (from C in db.Contacts where C.AcctID == AcctID && C.ContactTypeID == 1 select C).Single();
            }
            catch
            {
                //returns null contact
            }
            return result;
        }
        public Contact getContactFromID(int CID)
        {
            return (from C in db.Contacts where C.ContactID == CID select C).Single();
        }
        public bool SaveContact(Contact C)
        {
            bool result = true;
            try
            {
                if (C.ContactID < 1)
                {
                    db.Contacts.InsertOnSubmit(C);
                }
                else
                {
                    Contact EC = (from c in db.Contacts where c.ContactID == C.ContactID select c).Single();
                    EC = C;
                }
                db.SubmitChanges();
            }
            catch (Exception e)
            {

                result = false;
            }

            return result;
        }
        public bool SaveJob(Job J)
        {
            J.EnterDate = DateTime.Now;
            J.EnteredBy=Membership.GetUser().UserName;
            
            bool result = true;
            try
            {
                if (J.JID < 1)
                {
                    db.Jobs.InsertOnSubmit(J);
                }
                else
                {
                    Job EJ = (from j in db.Jobs where j.JID == J.JID select j).Single();

                    EJ.CompleteDate = J.CompleteDate; 
                    EJ.Completed = J.Completed; EJ.CustID = J.CustID; EJ.DueDate = J.DueDate; EJ.JobDescription = J.JobDescription; EJ.OrderedBy = J.OrderedBy;
                }
                db.SubmitChanges();
            }
            catch
            {
                result = false;
            }
            return result;
        }
        public bool SaveJobLine(JobLine JL)
        {
            JL.EnteredBy=Membership.GetUser().UserName;
            JL.EnterTime = DateTime.Now;//Time entered or last modified
            try
            {
            if (JL.JobLineID > 0)// Line edit
            {
                JobLine OJL = (from jl in db.JobLines where jl.JobLineID == JL.JobLineID select jl).Single();
                OJL.Description = JL.Description; OJL.EmployeeID = JL.EmployeeID; OJL.EnteredBy = JL.EnteredBy; OJL.EnterTime = JL.EnterTime; OJL.ServiceItemID = JL.ServiceItemID; OJL.TimeIn = JL.TimeIn; OJL.TimeOut = JL.TimeOut; OJL.TotalTime = JL.TotalTime;
                //Make sure time does not change if log has already been invoiced
                if(!(OJL.InDocument))
                {
                    OJL.TimeIn = JL.TimeIn; OJL.TimeOut = JL.TimeOut; OJL.TotalTime = JL.TotalTime;
                }
            }
            else
            {
                db.JobLines.InsertOnSubmit(JL);
            }

                db.SubmitChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        public List<TransLineItem> getLineItemsFromDocID(int DocID)
        {
            return (from li in db.TransLineItems where li.DocumentID == DocID select li).ToList();
        }
        public TaxCollection getDocTax(int DocID)
        {
            TaxCollection T = new TaxCollection();
            try { T=(from t in db.TaxCollections where t.DocID == DocID select t).Single(); }
            catch{}
            return T;
        }
        public TaxRate getTaxRateFromID(int TaxID)
        {
            return (from tr in db.TaxRates where tr.TaxLocID==TaxID select tr).Single();
        }
        public TransactionTable getItemsSaleTrans(int DocID)
        {
            return (from its in db.TransactionTables where its.DocumentID == DocID && its.TransactionTypeID == 26 select its).Single();
        }
        public List<AccountDetails> getDepositCandidates()
        {
            List<AccountDetails> result = new List<AccountDetails>();
            //Get Cash accounts (type 12) and CC/Checking payment accounts (type 20)
            List<AccountTable> CA = (from ca in db.AccountTables where ca.AcctTypeID == 12 || ca.AcctTypeID==20 select ca).ToList();
            //Populate account details
            foreach (AccountTable at in CA)
            {
                decimal b=getBalance(at.AcctID);
                result.Add(new AccountDetails { AcctID = at.AcctID, Name = at.AcctName,Balance=b,AcctType=at.AcctTypeID });
            }

            return result;
        }
        public AccountDetails getAcctDetails(int AcctID)
        {
            AccountDetails result=new AccountDetails();
            AccountTable ACCT=getAccountFromID(AcctID);
            result.Balance = getBalance(AcctID);
            result.AcctType = ACCT.AcctTypeID;
            result.AcctID = AcctID;
            result.Name = ACCT.AcctName;
            return result;
        }
        public List<AccountDetails> getAcctsWithPlusBalance(int AcctTypeID)
        {
            return (from A in getAcctsOfTypeDetails(AcctTypeID) where A.Balance > 0 select A).ToList();
        }
        public List<AccountDetails> getAcctsOfTypeDetails(int AcctTypeID)
        {
            List<AccountDetails> result = new List<AccountDetails>();
            List<int> AcctIDs = (from A in db.AccountTables where A.AcctTypeID == AcctTypeID select A.AcctID).ToList();
            foreach (int ID in AcctIDs)
            {
                result.Add(getAcctDetails(ID));
            }
            return result;
        }
        public bool IsPrepayItem(int ItemID)
        {
            //Prepay item is type 65
            int ItemType = (from it in db.Items where it.ItemID == ItemID select it).Single().ItemTypeID;
            if (ItemType == 65) return true;
            else return false;
        }
        public int AddServiceCredits(int ItemID, decimal Quantity,int AcctID,DateTime ChangeDate)
        {

            ServiceCredit AC = new ServiceCredit { AcctID = AcctID, ChangeDate = ChangeDate, Amount = Quantity, EntryDate = DateTime.Now, EnteredBy = Membership.GetUser().UserName,ServiceType=getServiceTypeIDFromSCItemID(ItemID) };
            int result = SaveServiceCredits(AC);
            return result;

        }
        public int getServiceTypeIDFromSCItemID(int ItemID)
        {
            return (from st in db.ServiceCreditTypes where st.ItemID == ItemID select st).Single().SCTID;
        }
        public int SaveServiceCredits(ServiceCredit SC)
        {
            int SCID = 0;
            try
            {
                db.ServiceCredits.InsertOnSubmit(SC);
                db.SubmitChanges();
                SCID = SC.SCID;
            }
            catch (Exception)
            {
                
                throw;
            }
            
            return SCID;
        }
        public List<Prepaid> getPrepays()//all prepays with a greater than 0 balance
        {
            List<Prepaid> result = new List<Prepaid>();
            
            //Make prepay account list
            
            List<ServiceCreditType> ST = (from st in db.ServiceCreditTypes select st).ToList();
            
            foreach (ServiceCreditType  SCT in ST)
            {
                List<int> AList = new List<int>();
                List<ServiceCredit> AllRecords=(from a in db.ServiceCredits where a.ServiceType==SCT.SCTID select a).ToList();
                foreach (ServiceCredit  SC in AllRecords)
                {
                    if (!(AList.Contains(SC.AcctID)))
                    {
                        AList.Add(SC.AcctID);
                        Prepaid temp = new Prepaid { AcctID = SC.AcctID, ServiceTypeID = SC.ServiceType,ItemID=SCT.ItemID };
                        temp.Balance = (from a in db.ServiceCredits where a.AcctID == SC.AcctID && a.ServiceType==SC.ServiceType select a.Amount).ToList().Sum();
                        if (temp.Balance > 0)
                        {
                            result.Add(temp);
                        }
                    }
                }
            }
            
            
            return result;
        }
        public string getPrepayTypeName(int PTypeID)
        {
            return (from pt in db.ServiceCreditTypes where pt.SCTID == PTypeID select pt.ServiceType).Single();
        }
        public List<Prepaid> getCustPrepays(int CustAcct)
        {
            List<Prepaid> PP = new List<Prepaid>();
            //Make prepay account list

            List<ServiceCreditType> ST = (from st in db.ServiceCreditTypes select st).ToList();
            foreach (ServiceCreditType SCT in ST)
            {
                List<int> AList = new List<int>();
                List<ServiceCredit> AllRecords = (from a in db.ServiceCredits where a.ServiceType == SCT.SCTID && a.AcctID==CustAcct select a).ToList();
                foreach (ServiceCredit SC in AllRecords)
                {
                    if (!(AList.Contains(SC.AcctID)))
                    {
                        AList.Add(SC.AcctID);
                        Prepaid temp = new Prepaid { AcctID = SC.AcctID, ServiceTypeID = SC.ServiceType, ItemID = SCT.ItemID };
                        temp.Balance = (from a in db.ServiceCredits where a.AcctID == SC.AcctID && a.ServiceType == SC.ServiceType select a.Amount).ToList().Sum();
                        PP.Add(temp);
                    }
                }

            }
            return PP;
        }
        public List<AccountTable> getPrepayCustomers()
        {
            List<AccountTable> result = new List<AccountTable>();

            List<int> AList = new List<int>();

            List<ServiceCreditType> ST = (from st in db.ServiceCreditTypes select st).ToList();

            foreach (ServiceCreditType SCT in ST)
            {
                
                List<ServiceCredit> AllRecords = (from a in db.ServiceCredits where a.ServiceType == SCT.SCTID select a).ToList();
                foreach (ServiceCredit SC in AllRecords)
                {
                    if (!(AList.Contains(SC.AcctID)))
                    {
                        AList.Add(SC.AcctID);
                    }
                }
            }
            foreach (int item in AList)
            {
                result.Add((from a in db.AccountTables where a.AcctID == item select a).Single());
            }


            return result;
        }
        public List<Prepaid> getPrepaidStatementData(int AcctID, string sd, string ed, int SCType)
        {
            List<Prepaid> result = new List<Prepaid>();
            DateTime SD = new DateTime(); DateTime ED = new DateTime();
            
            List<int> DocIDs = new List<int>();
            List<int> SCTIDS = new List<int>();
            List<int> AcctIDs = new List<int>();

            //Proccess service credit records
            List<ServiceCredit> SCs = getServiceCreditsByAcct(AcctID);//return all unless other than 0 is specified sorted by acctid , SCtypeID and change date
            //If service credit type specified
            if (SCType > 0) SCs = (from sc in SCs where sc.ServiceType == SCType select sc).ToList();
            //Date ranges
            if (!(sd == ""))
            {
                try
                {
                    SD = Convert.ToDateTime(sd);
                    SCs = (from sc in SCs where sc.ChangeDate >= SD select sc).ToList();
                }
                catch (Exception)
                {
                    return result;
                    throw;
                }
            }
            if (!(ed == ""))
            {
                try
                {
                    ED = Convert.ToDateTime(sd);
                    SCs = (from sc in SCs where sc.ChangeDate <= ED select sc).ToList();
                }
                catch (Exception)
                {
                    return result;
                    throw;
                }
            }

            int AcctCount = 0;
            int SCTypeCount = 0;
            int DocCount = 0;
            int SCCount = 0;
            int resultCount = 0;
            decimal BalanceForward = 0;
            decimal LineAmount = 0;
            foreach (ServiceCredit S in SCs)
            {
                
                if (AcctID == 0)//Collect Accts
                {
                    if (!(AcctIDs.Contains(S.AcctID))) AcctIDs.Add(S.AcctID);
                    SCTIDS.Clear();//Reset Service Type IDs
                }

                if (SCType == 0)//Collect Sctypes
                {
                    if (!(SCTIDS.Contains(S.ServiceType)))
                    {
                        SCTIDS.Add(S.ServiceType);
                        BalanceForward = 0;//start new balance on first occurance of service type
                        DocIDs.Clear();//Reset doc ids to allow accounting for multiple service types on one doc
                    }
                }
                
                if (!(DocIDs.Contains(S.DocID)))
                {
                    DocIDs.Add(S.DocID);
                    Document D = GetDocumentFromID(S.DocID);
                    AccountTable A=getAccountFromID(S.AcctID);
                    string SCTypeName=getServiceTypeNameFromID(S.ServiceType);
                    result.Add(new Prepaid { LineDate = D.DocumentTime, Reference = D.DocumentReference, Description=SCTypeName+@" "+ GetDocTypeFromTypeID(D.DocumentTypeID) });
                    
                    if (D.DocumentTypeID == 15) //Service Credit Deduction
                    {
                        int JobID =Convert.ToInt32( getJobLineFromID(S.JobLineID).JobID);
                        result[resultCount].Description = result[resultCount].Description + @" Job#:" + JobID + @"-" + getJobFromID(JobID).JobDescription;
                        result[resultCount].JobID = JobID;
                    }
                    
                    
                    if (result.Count > 1)
                    {
                        result[resultCount - 1].Amount = LineAmount;
                        BalanceForward = BalanceForward + LineAmount;
                        result[resultCount - 1].Balance = BalanceForward;
                    }
                    LineAmount = S.Amount;//Reset lineamount used for total document ammount of given type
                    resultCount++;
                }
                else
                {
                    LineAmount = LineAmount + S.Amount;//Talley document amount for given SCtype
                }
                
                

            }




            result[result.Count - 1].Amount = LineAmount;
            result[result.Count - 1].Balance = BalanceForward + LineAmount;
            



            return result;
        }
        public List<ServiceCredit> getServiceCreditsByAcct(int AcctID)//returns all if AcctID=0 sorted by acctid and date
        {
            if (AcctID > 0) return (from sc in db.ServiceCredits where sc.AcctID == AcctID orderby sc.ServiceType, sc.ChangeDate select sc).ToList();
            else return (from sc in db.ServiceCredits orderby sc.AcctID, sc.ServiceType, sc.ChangeDate select sc).ToList();
        }
        public int InsertJob(Job J)
        {
            try
            {
                db.Jobs.InsertOnSubmit(J);
                db.SubmitChanges();
                
            }
            catch
            {
            }
            return J.JID;
        }
        public List<JobLine> getPendingLabor(int CustID)
        {
            List<JobLine> result = new List<JobLine>();

            List<Job> J = (from j in db.Jobs where j.CustID == CustID select j).ToList();

            foreach (Job j in J)
            {
                List<JobLine> JL = (from l in db.JobLines where l.JobID == j.JID && l.InDocument == false select l).ToList();
                foreach (JobLine jl in JL)
                {
                    result.Add(jl);
                }
            }

            return result;
        }
        public List<Document> getCustInvoices(int CustID)
        {
            //get master date range
            DateTime SDate = Convert.ToDateTime(getAPstartDate());
            DateTime EDate = Convert.ToDateTime(getAPendDate());
            List<TransactionTable> TT= (from T in db.TransactionTables where T.DebitID==CustID && T.TransactionTypeID==26 select T).ToList();//26 is new sale
            List<Document> result = new List<Document>();
            foreach (TransactionTable t in TT)
            {
                Document D = GetDocumentFromID(t.DocumentID);
                //transaction have no date range so pull from document associated with transaction
                if((!(result.Contains(D))&&(D.DocumentTime.Date>=SDate)&&(D.DocumentTime.Date<=EDate)))
                {
                result.Add(D);
                }
            }
            return result;
        }
        public List<Document> getCustPayments(int CustID)
        {
            //get master date range
            DateTime SDate = Convert.ToDateTime(getAPstartDate());
            DateTime EDate = Convert.ToDateTime(getAPendDate());
            List<TransactionTable> TT = (from T in db.TransactionTables where T.CreditID == CustID && (T.TransactionTypeID == 27 || T.TransactionTypeID==22) select T).ToList();//27 is Customer Payment
            List<Document> result = new List<Document>();
            foreach (TransactionTable t in TT)
            {
                Document D = GetDocumentFromID(t.DocumentID);
                //transaction have no date range so pull from document associated with transaction
                if ((!(result.Contains(D)) && (D.DocumentTime.Date >= SDate) && (D.DocumentTime.Date <= EDate)))
                {
                    result.Add(D);
                }
            }
            return result;
        }
        public bool ReconcileDocument(int DocID)
        {
            bool result = true;
            try{Document D=(from d in db.Documents where d.DocumentID==DocID select d).Single();
                D.Reconciled=true;
                db.SubmitChanges();
            }
            catch{
                result=false;
            }
            return result;
        }
        public int getPayMethodRecAcctID(int PayMethID)
        {
            return (from pm in db.PayMethods where pm.PayMethID == PayMethID select pm.RecAcctID).Single();
        }
    }
}