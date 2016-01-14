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

            //view model
            var viewModel = new IndexViewModel()
            {
                ConvertedHtml = fileHTML  
            };

            return View(viewModel);
        }

        public ActionResult Headers(string html)
        {
            //load html document
            HtmlDocument agilityDoc = new HtmlDocument();
            agilityDoc.LoadHtml(html);

            //extract headers
            var headerElements = agilityDoc.DocumentNode.Descendants("h2").Select(nd => nd.InnerText);

            //output HTML
            var outputSb = new StringBuilder();
            outputSb.AppendLine("<ul class=\"tabs\" data-tabs id=\"example-tabs\">");
            var count = 1;
            foreach (var headerElement in headerElements)
            {
                var activeClass = (count == 1) ? "is-active" : string.Empty;
                outputSb.AppendLine(string.Format("<li class=\"tabs-title {0}\"><a href=\"#{1}\">{2}</a></li>", activeClass, count, headerElement));
                count += 1;
            }
            outputSb.AppendLine("</ul>");

            return Content(outputSb.ToString());
        }

        public ActionResult Tabs(string html)
        {
            //load html document
            HtmlDocument agilityDoc = new HtmlDocument();
            agilityDoc.LoadHtml(html);

            //split section by header
            string[] sections = Regex.Split(html, @"(?=<h2>)", RegexOptions.Multiline);


            //output HTML
            var outputSb = new StringBuilder();
            outputSb.AppendLine("<div class=\"tabs-content\" data-tabs-content=\"example-tabs\">");
            var count = 1;
            foreach (var section in sections)
            {
                var activeClass = (count == 1) ? "is-active" : string.Empty;
                outputSb.AppendLine(string.Format("<div class=\"tabs-panel {0}\" id=\"{1}\">", activeClass, count));
                outputSb.AppendLine(section);
                outputSb.AppendLine("</div>");
                count += 1;
            }
            outputSb.AppendLine("</div>");

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