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
    public class JobsController : Controller
    {
        //
        // GET: /Jobs/
        MyJava J = new MyJava();
        MyMethods M = new MyMethods();
        [Authorize(Roles = "PowerUser")]
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public JavaScriptResult StartJobs()
        {
            string result = J.MakeJobsMain();
            return JavaScript(result);
        }
        
        public ActionResult OneJob(int JOBID)
        {
            ViewData["JOBID"] = JOBID;
            return View(JOBID);
        }
        [HttpPost]
        public JavaScriptResult SingleJob(int JOBID)
        {
            string result = J.MakeSingleJob(JOBID);

            return JavaScript(result);
        }
        [HttpPost]
        public JavaScriptResult SaveJob(string ReqDate,string Description,string DueDate,int RequestedBy,bool CompletionStatus,int AcctID,int JobID,string CompleteDate)
        {
            DateTime RD = new DateTime();
            DateTime DD = new DateTime();
            DateTime CD = new DateTime();
            if (CompletionStatus)
            {
                if (CompleteDate.Length > 0)
                {
                    try
                    {
                        CD = Convert.ToDateTime(CompleteDate);
                    }
                    catch
                    {
                        return JavaScript(@" alert('Invalid Completion date, please correct or remove date'); ");
                    }
                }
                else
                {
                    CD = DateTime.Now;
                }
            }
            try
            {
                RD = Convert.ToDateTime(ReqDate);
                DD = Convert.ToDateTime(DueDate);

            }
            catch
            {
                return JavaScript(@" alert('Invalid date, please check that dates have been selected and try again'); ");
            }
            string result = "Job Saved Successfully";
            if (JobID > 0)
            {
                result = "Job Updated Successfully";
            }
            Job J = new Job { Completed = CompletionStatus, CustID = AcctID, DueDate = DD, JID = JobID, JobDescription = Description,OrderedBy=RequestedBy,RequestDate=RD };
            //Accept only valid CD (Completion Date) >year 1776
            if (CD.Year > 1776) J.CompleteDate = CD;
            if (!(M.SaveJob(J)))
            {
                result = "Job Failed to Save!";
            }

            return JavaScript(@" alert('"+result+@"'); ");
        }
        [HttpPost]
        public JavaScriptResult SaveJobLog(int JobID, int LogID,string LogTime,string StartTime,string EndTime,string TotalTime,int EmployeeID,int ItemID, string Description)
        {
            DateTime LT=new DateTime(); DateTime ST=new DateTime(); DateTime ET=new DateTime(); decimal TT=0;
            string result = @" alert('Log successfully saved'); ";
            if (LogID > 0) result = @" alert('Log successfully Updated'); ";
            try { LT = Convert.ToDateTime(LogTime); }
            catch
            {
                LT = DateTime.Now;
            }
            try { ST = Convert.ToDateTime(StartTime); }
            catch
            {
                result = @" alert('Enter a valid start time'); ";
                return JavaScript(result);
            }
            try { ET = Convert.ToDateTime(EndTime); }
            catch
            {
                result = @" alert('Enter a valid end time'); ";
                return JavaScript(result);
            }
            try { TT = Convert.ToDecimal(TotalTime); }
            catch
            {
                result = @" alert('Enter total time'); ";
                return JavaScript(result);
            }
            try
            {
                JobLine J = new JobLine { Description = Description, EmployeeID = EmployeeID, JobID = JobID, ServiceItemID = ItemID, TimeIn = ST, TimeOut = ET, TotalTime = TT,JobLineID=LogID };
                if (!(M.SaveJobLine(J)))
                {
                    result = @" alert('Log Save Failed!'); ";
                    return JavaScript(result);
                }
            }
            catch
            {
                result = @" alert('Log Save Failed!'); ";
                return JavaScript(result);
            }
            return JavaScript(result);
        }
        [HttpPost]
        public JavaScriptResult GetLaborToInvoice(int CID, string Element)
        {
            
            string result = @" alert('The selected customer has no labor to invoice!'); ";
            List<JobLine> labor = M.getPendingLabor(CID);
            if (labor.Count > 0)
            {
                result=J.MakeLaborInvoiceItems(labor, Element);
            }
                        return JavaScript(result);
        }
    }
}
