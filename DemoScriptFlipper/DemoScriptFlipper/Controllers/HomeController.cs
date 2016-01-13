using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Drawing;
using System.Drawing.Imaging;
using CommonMark;
using System.Text.RegularExpressions;
using System.IO;

namespace DemoScriptFlipper.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            //read and chop up MD file
            string md = System.IO.File.ReadAllText(Server.MapPath("~/SimpleSample.md"));

            string[] h1Sections = Regex.Split(md, @"(?=^#[^#])", RegexOptions.Multiline);
            var fullHtml = string.Empty;
            foreach (string h1Section in h1Sections)
            {
                var document = CommonMarkConverter.Parse(h1Section);
                using (var writer = new StringWriter())
                {
                    CommonMarkConverter.ProcessStage3(document, writer);
                    fullHtml += writer.ToString();
                }


            }

            return Content(fullHtml);

            //return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}