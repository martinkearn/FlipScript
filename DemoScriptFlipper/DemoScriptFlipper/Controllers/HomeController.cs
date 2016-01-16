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
using HtmlAgilityPack;
using System.Text;
using DemoScriptFlipper.Models;

namespace DemoScriptFlipper.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            //read file
            string fileMd = System.IO.File.ReadAllText(Server.MapPath("~/Sample.md"));

            //get full html string from md
            var fileHTML = string.Empty;
            using (var writer = new StringWriter())
            {
                CommonMarkConverter.ProcessStage3(CommonMarkConverter.Parse(fileMd), writer);
                fileHTML += writer.ToString();
            }

            //extract h1 for page title
            HtmlDocument agilityDoc = new HtmlDocument();
            agilityDoc.LoadHtml(fileHTML);
            ViewBag.Title = agilityDoc.DocumentNode.Descendants("h1").Select(nd => nd.InnerText).FirstOrDefault();

            //view model
            var viewModel = new IndexViewModel()
            {
                ConvertedHtml = fileHTML  
            };

            return View(viewModel);
        }




        public ActionResult Slides(string html)
        {
            //load html document
            HtmlDocument agilityDoc = new HtmlDocument();
            agilityDoc.LoadHtml(html);

            //split section by header
            string[] sections = Regex.Split(html, @"(?=<h2>)", RegexOptions.Multiline);

            //output HTML
            var outputSb = new StringBuilder();
            foreach (var section in sections)
            {
                //owl format
                outputSb.AppendLine(string.Format("<div>"));
                outputSb.AppendLine(section);
                outputSb.AppendLine("</div>");
            }

            return Content(outputSb.ToString());
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