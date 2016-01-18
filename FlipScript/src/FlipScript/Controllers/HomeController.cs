using CommonMark;
using FlipScript.ViewModels.Home;
using HtmlAgilityPack;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FlipScript.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Viewer(IFormFile file)
        {
            //read incoming file to a string
            var fileContent = string.Empty;
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                //ContentDispositionHeaderValue parsedContentDisposition;
                //ContentDispositionHeaderValue.TryParse(file.ContentDisposition, out parsedContentDisposition);
                //title = parsedContentDisposition.Name;
                fileContent = reader.ReadToEnd();
            }

            //get full html string from file
            var fileHTML = string.Empty;
            using (var writer = new StringWriter())
            {
                CommonMarkConverter.ProcessStage3(CommonMarkConverter.Parse(fileContent), writer);
                fileHTML += writer.ToString();
            }

            //extract h1 for page title
            HtmlDocument agilityDoc = new HtmlDocument();
            agilityDoc.LoadHtml(fileHTML);
            var title = agilityDoc.DocumentNode.Descendants("h1").Select(nd => nd.InnerText).FirstOrDefault();

            //split the html doc based on headers
            var nodes = agilityDoc.DocumentNode.ChildNodes.ToArray();
            var sections = nodes.Skip(1).Aggregate(nodes.Take(1).Select(x => x.OuterHtml).ToList(), (a, n) =>
            {
                if (n.Name.ToLower() == "h1" || n.Name.ToLower() == "h2")
                {
                    a.Add("");
                }
                a[a.Count - 1] += n.OuterHtml; return a;
            });

            //enumerate HTML sections and construct array of slides for Owl Carousel
            List<string> slides = new List<string>();
            foreach (var section in sections)
            {
                var outputSb = new StringBuilder();

                //eliminate empty sections
                if (section.ToLower() != "<p>﻿</p>\r\n")
                {
                    //populate owl format carosel slides
                    outputSb.AppendLine("<div>");
                    outputSb.AppendLine(section);
                    outputSb.AppendLine("</div>");
                }

                //add slide to array
                slides.Add(outputSb.ToString());
            }

            return View(new Viewer()
                {
                    Title = title,
                    SlidesHtml = slides
                }
            );
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
