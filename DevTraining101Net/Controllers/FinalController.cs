using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DevTraining101Net.Controllers
{
    public class FinalController : Controller
    {
        //
        // GET: /Final/
        public ActionResult Index()
        {
            ViewBag.Status = Request.QueryString["event"];
            return View("Final");
        }
	}
}