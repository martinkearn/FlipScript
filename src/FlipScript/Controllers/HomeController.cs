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
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Microsoft.AspNet.Routing;

namespace FlipScript.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Viewer(IFormFile file, string owner, string repo, string path)
        {

            var fileContent = string.Empty;

            //if a file was passed in
            if (file != null)
            {
                //read incoming file to a string
                using (var reader = new StreamReader(file.OpenReadStream()))
                {
                    fileContent = reader.ReadToEnd();
                }
            }
            else
            {
                if ((string.IsNullOrEmpty(owner)) || (string.IsNullOrEmpty(repo)) || (string.IsNullOrEmpty(path)))
                {
                    return RedirectToAction("Index");
                }

                using (var httpClient = new HttpClient())
                {
                    var baseApi = "https://api.github.com";
                    var fullApiPath = string.Format("{0}/repos/{1}/{2}/contents/{3}", baseApi, owner.Trim('/'), repo.Trim('/'), path.Trim('/'));

                    //setup HttpClient
                    httpClient.BaseAddress = new Uri(baseApi);
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    //set up user agent - required by GitHub api to avoid protocol violation errors
                    var message = new HttpRequestMessage(HttpMethod.Get, fullApiPath);
                    message.Headers.Add("User-Agent", "FlipScript");

                    //make request
                    var response = await httpClient.SendAsync(message);

                    //read and deserialise response
                    var responseContent = await response.Content.ReadAsStringAsync();
                    dynamic responseObj = JObject.Parse(responseContent);

                    //get and decode 'content' property
                    string contentBase64EncodedString = responseObj.content;
                    var contentBase64EncodedBytes = Convert.FromBase64String(contentBase64EncodedString);
                    fileContent = Encoding.UTF8.GetString(contentBase64EncodedBytes);
                }
            }


            //convert markdown file to array of html section strings
            var sections = ConvertFile(fileContent);

            //find and extract h1 for page title from array of html strings
            var title = GetTitle(sections);

            //enumerate HTML sections and construct array of slides for Owl Carousel
            List<string> slides = new List<string>();
            foreach (var section in sections)
            {
                var outputSb = new StringBuilder();

                //eliminate empty sections
                if (section.ToLower() != "<p>﻿</p>\r\n")
                {
                    //populate owl format carosel slides
                    outputSb.AppendLine("<div class=\"item\">");
                    outputSb.AppendLine(section);
                    outputSb.AppendLine("</div>");
                }

                //add slide to array
                slides.Add(outputSb.ToString());
            }

            var viewModel = new Viewer()
            {
                Title = title,
                SlidesHtml = slides
            };

            return View(viewModel);
        }

        public IActionResult Error()
        {
            return View();
        }

        private List<string> ConvertFile(string fileContent)
        {
            //get full html string from file
            var fileHTML = string.Empty;
            using (var writer = new StringWriter())
            {
                CommonMarkConverter.ProcessStage3(CommonMarkConverter.Parse(fileContent), writer);
                fileHTML += writer.ToString();
            }

            //split the html doc based on headers
            HtmlDocument agilityDoc = new HtmlDocument();
            agilityDoc.LoadHtml(fileHTML);
            var nodes = agilityDoc.DocumentNode.ChildNodes.ToArray();
            List<string> sections = nodes.Skip(1).Aggregate(nodes.Take(1).Select(x => x.OuterHtml).ToList(), (a, n) =>
            {
                if (n.Name.ToLower() == "h1" || n.Name.ToLower() == "h2")
                {
                    a.Add("");
                }
                a[a.Count - 1] += n.OuterHtml; return a;
            });

            return sections;
        }

        private string GetTitle(List<string> sections)
        {
            HtmlDocument agilityDoc = new HtmlDocument();
            var title = string.Empty;
            foreach (var section in sections)
            {
                agilityDoc.LoadHtml(section);
                title = agilityDoc.DocumentNode.Descendants("h1").Select(nd => nd.InnerText).FirstOrDefault();
                if (title != string.Empty) break;
            }
            return title;
        }
    }
}
