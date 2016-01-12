using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Drawing;
using System.Drawing.Imaging;
using CommonMark;
using System.Text.RegularExpressions;

namespace DemoScriptFlipper.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            //read and chop up MD file
            string md = System.IO.File.ReadAllText(Server.MapPath("~/SimpleSample.md"));


            string[] h1Sections = Regex.Split(md, @"(?=^#[^#])", RegexOptions.Multiline);
            foreach (string h1Section in h1Sections)
            {
                var s = h1Section;
            }

            //// parse markdown into document structure
            //var document = CommonMarkConverter.Parse(md);

            //// walk the document node tree
            //List<string> tags = new List<string>();
            //foreach (var node in document.AsEnumerable().Where(n => n.Block != null))
            //{
            //    if (node.Block != null)
            //    {
            //        tags.Add(node.Block.Tag.ToString());
            //    }
            //}


            //string[] mdSections = md.Split('#');

            return View();
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