using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace SiteCrawler
{
    public class ParseSiteLinks
    {
        public Dictionary<string, int> _visitedUrls = new Dictionary<string, int>();
        public List<string> _duplicateUrls = new List<string>();
        public List<string> _externalUrls = new List<string>();
        private static List<string> _internalUrls = new List<string>();
        private static int _totalLinks = 0;
        private static int _totalImgTags = 0;

        public void ParseUrl(string siteUrl)
        {
            // "Step" counter for console feedback
            int currentStep = 1;

            // Creating "HTTP" Object to issue the "Request" for the HomePage of the site
            HttpWebRequest httpClient = (HttpWebRequest)HttpWebRequest.Create(siteUrl);
            httpClient.Method = "GET";

            // Console Feedback
            LogStep(currentStep++);

            // HttpWebResponse - Contains the "Response" of this specific Request
            HttpWebResponse httpResponse = (HttpWebResponse)httpClient.GetResponse();

            LogStep(currentStep++);

            string htmlResponse = DownloadResponse(httpResponse);
            
            if (String.IsNullOrWhiteSpace(htmlResponse))
            {
                Console.WriteLine("Error Executing Request. Check your internet connection");
                return;
            }

            LogStep(currentStep++);

            // Parse the Links from this page
            List<string> _urls = ParseLinks(htmlResponse);
            _totalLinks = _urls.Count;

            // Iterating over all the Links on the home page
            foreach (var url in _urls)
            {
                if (!SelectionPolicy(url))
                    _externalUrls.Add(url);
                else
                    _internalUrls.Add(url);
            }

            LogStep(currentStep++);
            LogStep(currentStep++);

            // Iterating over all the internal links found and avoiding vsit to duplicate url
            foreach (var url in _internalUrls)
            {
                //Adding domain for Relative Urls
                string _absoluteUrl = !url.ToLower().Contains("wiprodigital.com") ? siteUrl + url : url;

                if (!ReVisitPolicy(_absoluteUrl))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("\t=> [Re-Visit Policy] Duplicate Url. Won't be visited again");
                    Console.ForegroundColor = ConsoleColor.White;
                    _duplicateUrls.Add(_absoluteUrl);
                    continue;
                }

                try
                {
                    httpClient = (HttpWebRequest)HttpWebRequest.Create(_absoluteUrl);
                    httpClient.Method = "GET";
                    httpResponse = (HttpWebResponse)httpClient.GetResponse();

                    // Read String Response
                    htmlResponse = DownloadResponse(httpResponse);

                    // Count Image Tags on this "Page"
                    var _imgTags = CountImageTags(htmlResponse);
                    _totalImgTags += _imgTags;

                    // flag this url as "visited"
                    _visitedUrls.Add(_absoluteUrl, _imgTags);
                    
                    
                    // Console Feedback
                    Console.WriteLine("\t=> [Re-Visit Policy] Visited Url {0}. Found {1} <img> tags", _absoluteUrl, _imgTags);

                    // After a page is visited wait for a second
                    PolitenessPolicy();
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\t=> [Error] Ops. Error Visiting Url : " + _absoluteUrl);
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
        }

        //To log the current step of the process to a console
        private static void LogStep(int step)
        {
            switch (step)
            {
                case 1:
                    Console.WriteLine("Executing HTTP Request for '{0}' website", ConfigurationManager.AppSettings["HomePage"]);
                    break;

                case 2:
                    Console.WriteLine("Reading Response of the HTTP Request");
                    break;

                case 3:
                    Console.WriteLine("Response 'OK'");
                    break;

                case 4:
                    Console.WriteLine("Extrated Links from the 'HomePage'. Found : {0}", _totalLinks);
                    break;

                case 5:
                    Console.WriteLine("Visiting Urls found on the 'HomePage'");
                    break;
            }
        }

       // Returns string value of the response
        private static string DownloadResponse(HttpWebResponse response)
        {
            string htmlResponse;

            using (Stream dataStream = response.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(dataStream))
                {
                    htmlResponse = reader.ReadToEnd();
                }
            }

            return htmlResponse;
        }

        // Returns List of links found
        private static List<string> ParseLinks(string htmlResponse)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(htmlResponse);
            
            // Returning all the "href" nodes of this Page
            return doc.DocumentNode.SelectNodes("//a").Select(t => t.Attributes["href"].Value).ToList();
        }

        // Returns true if the url should be visited
        private static bool SelectionPolicy(string url)
        {
            var externalDomains = new[] { "linkedin", "facebook", "twitter" };
            Match match = Regex.Match(url, @"http://([A-Za-z0-9\-]+)\.wiprodigital\.com", RegexOptions.IgnoreCase);

            if ((match.Success && externalDomains.Any(match.Value.Contains)) || externalDomains.Any(url.Contains))
                return false;
            return true;
        }

        // returns true if the url is not already visited
        private bool ReVisitPolicy(string url)
        {
            return !_visitedUrls.Keys.Contains(url);
        }

        // Explicit wait
        private static void PolitenessPolicy()
        {
            Thread.Sleep(1000);
        }

        // Returns Number of <img> tags found on a page
        private static int CountImageTags(string htmlResponse)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(htmlResponse);
            // Returning all the "img" nodes of this Page
            return doc.DocumentNode.SelectNodes("//img").Count;
        }

        /// <summary>
        /// Prints a "Summary" of the process
        /// </summary>
        public void PrintSummary()
        {
            Console.WriteLine("\n***************************************************");
            Console.WriteLine("Total Page Links                               : {0}", _totalLinks);
            Console.WriteLine("Valid Links (Selection Policy 'OK')            : {0}",
                (_internalUrls.Count + _externalUrls.Count));
            Console.WriteLine("Visited Links                                  : {0}", _visitedUrls.Count);
            Console.WriteLine("Skipped Valid Links (Re-Visit Policy 'NOT OK') : {0}",
                (_visitedUrls.Count - _duplicateUrls.Count));
            Console.WriteLine("\nTotal <img> tags found on all pages          : {0}", _totalImgTags);
            Console.WriteLine("Average <img> tags per page                    : {0}",
                ((double)_totalImgTags / _visitedUrls.Count));
            Console.WriteLine("\n***************************************************\n");
        }
    }
}
