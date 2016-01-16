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
using System.Threading.Tasks;

namespace DemoScriptFlipper.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            //view model
            var viewModel = new IndexViewModel()
            {
            };

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Viewer(FormCollection collection)
        {
            //read file in incoiung form to a string
            var files = GetFilesFromForm(Request.Files);
            var fileMd = Encoding.UTF8.GetString(files.FirstOrDefault());

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
            var viewModel = new ViewerViewModel()
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

        private List<byte[]> GetFilesFromForm(HttpFileCollectionBase formFiles)
        {
            List<byte[]> files = new List<byte[]>();
            for (int i = 0; i < formFiles.AllKeys.Count(); i++)
            {
                HttpPostedFileBase file = formFiles[i];
                if (file.ContentLength > 0)
                {
                    using (var binaryReader = new BinaryReader(file.InputStream))
                    {
                        byte[] fileByteArray = binaryReader.ReadBytes(file.ContentLength);
                        files.Add(fileByteArray);
                    }
                }
            }
            return files;
        }

        static string GetStringFromByteArray(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }
    }
}