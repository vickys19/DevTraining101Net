using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using System.Diagnostics;

namespace DevTraining101Net.Controllers
{
    public class SignController : Controller
    {

        //
        // GET: /Sign/
        public ActionResult Index()
        {
            Response.StatusCode = 501;
            return View();
        }

	}
}


