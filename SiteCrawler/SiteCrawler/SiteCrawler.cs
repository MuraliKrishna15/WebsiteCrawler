using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using HtmlAgilityPack;

namespace SiteCrawler
{
    public class SiteCrawler
    {
        private static ParseSiteLinks _parseSiteLinks = new ParseSiteLinks();
        // Url of the "Home Page"
        private static string _homePageUrl = ConfigurationManager.AppSettings["HomePage"];

        private static void Main(string[] args)
        {
            _parseSiteLinks.ParseUrl(_homePageUrl);

            // Summary of Visits            
            _parseSiteLinks.PrintSummary();

            //Wait for user input to exit
            Console.Write("To view detailed report press enter key...");
            Console.Read();

            //Generate an html report and open in a browser
            GenerateReport.GenrateReportAndOpen(_parseSiteLinks._visitedUrls, _parseSiteLinks._duplicateUrls, _parseSiteLinks._externalUrls);
            
        }
    }
}

