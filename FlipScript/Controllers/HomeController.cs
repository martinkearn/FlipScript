using CommonMark;
using FlipScript.Models;
using FlipScript.ViewModels.Home;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace FlipScript.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var vm = new FlipScript.ViewModels.Home.Index();
            if (HttpContext.Request.Cookies.ContainsKey("gitHubUrls"))
            {
                string cookie = HttpContext.Request.Cookies["gitHubUrls"];
                var cookieUrls = cookie.Split(new string[] { "----" }, StringSplitOptions.None);
                vm.PreviousUrls = cookieUrls.ToList();
            }
            else
            {
                vm.PreviousUrls = new List<string>();
            }
            return View(vm);
        }

        public IActionResult GitHubUrlHandler(string gitHubUrl)
        {
            if (!string.IsNullOrEmpty(gitHubUrl))
            {
                //store github url in cookie for easy access later
                if (HttpContext.Request.Cookies.ContainsKey("gitHubUrls"))
                {
                    var existingCookie = HttpContext.Request.Cookies["gitHubUrls"];
                    if (!existingCookie.ToString().Contains(gitHubUrl))
                    {
                        var newCookieValue = existingCookie + "----" + gitHubUrl;
                        HttpContext.Response.Cookies.Append("gitHubUrls", newCookieValue, new CookieOptions { Expires = DateTime.UtcNow.AddYears(1) });
                    }
                }
                else
                {
                    HttpContext.Response.Cookies.Append("gitHubUrls", gitHubUrl, new CookieOptions { Expires = DateTime.UtcNow.AddYears(1) });
                }

                //base64 encode url
                var plainTextBytes = Encoding.UTF8.GetBytes(gitHubUrl);
                var base64 =  Convert.ToBase64String(plainTextBytes);

                //redirect to viewer
                return RedirectToAction("Viewer", new { gitHubUrl = base64 });
            }
            return RedirectToAction("Index");
        }

        //[HttpPost]
        public async Task<IActionResult> Viewer(IFormFile file = null, string gitHubUrl = "")
        {
            var fileContent = string.Empty;

            //get file content as string
            if (file != null)
            {
                //read incoming file to a string
                using (var reader = new StreamReader(file.OpenReadStream()))
                {
                    fileContent = reader.ReadToEnd();
                }
            }
            else if (!string.IsNullOrEmpty(gitHubUrl))
            {
                //check if gitHubUrl is base 64, decoded if it is
                gitHubUrl = DecodeBase64(gitHubUrl);

                //get file content from GitHUb
                fileContent = await GetGitHubFile(gitHubUrl);
            }
            else
            {
                //return back to index
                return RedirectToAction("Index");
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

            //View model
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

        private String DecodeBase64(string original)
        {
            var decoded = string.Empty;
            try
            {
                //if it is not a base 64 string, this will throw an exception
                var originalAsBytes = Convert.FromBase64String(original);
                decoded = Encoding.UTF8.GetString(originalAsBytes, 0, originalAsBytes.Length);
            }
            catch
            {
                //not base 64, just set the decoded string to the original
                decoded = original;
            }
            return decoded;
        }

        public GithubUrlData ParseGithubUrl(string GithubUrl)
        {
            var data = new GithubUrlData();

            //var url = GithubUrl.ToLower();
            var url = GithubUrl;
            url = url.Replace("http://", string.Empty);
            url = url.Replace("https://", string.Empty);

            var path = string.Empty;

            if (url.StartsWith("raw.githubusercontent.com/"))
            {
                var urlSplitBySlash = url.Split('/');
                data.Owner = urlSplitBySlash[1];
                data.Repository = urlSplitBySlash[2];
                //get 4 onwards
                var pathArray = urlSplitBySlash.Skip(4);
                foreach (var pathNode in pathArray)
                {
                    path += pathNode + "/";
                }

            }
            else if (url.StartsWith("github.com/"))
            {
                var urlSplitBySlash = url.Split('/');
                data.Owner = urlSplitBySlash[1];
                data.Repository = urlSplitBySlash[2];
                //get 4 onwards
                var pathArray = urlSplitBySlash.Skip(5);
                foreach (var pathNode in pathArray)
                {
                    path += pathNode + "/";
                }
            }

            path = path.TrimEnd('/');
            if (!path.EndsWith(".md")) path += ".md";
            data.Path = path;

            return data;
        }

        private async Task<string> GetGitHubFile(string GitHubUrl)
        {
            var fileContent = string.Empty;
            using (var httpClient = new HttpClient())
            {
                //get GitHub API data from URL
                var gitHubUrlData = ParseGithubUrl(GitHubUrl);

                //construct url
                var baseApi = "https://api.github.com";
                var fullApiPath = string.Format("{0}/repos/{1}/{2}/contents/{3}", baseApi, gitHubUrlData.Owner, gitHubUrlData.Repository, gitHubUrlData.Path);

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
            return fileContent;
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
