using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiteCrawler
{
    public class GenerateReport
    {
        public static void GenrateReportAndOpen(Dictionary<string, int> visitedUrls, List<string> duplicateUrls, List<string> externalurls)
        {
            StringBuilder reporthtml = CreateReport(visitedUrls, duplicateUrls, externalurls);

            string file = SaveResultToFile(reporthtml.ToString());

            OpenReportInChrome(file);
        }

        private static StringBuilder CreateReport(Dictionary<string, int> visitedUrls, List<string> duplicateUrls, List<string> externalurls)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("<html><head><title>Website Crawl Report</title><style>");
            sb.Append("table { border: solid 3px black; border-collapse: collapse; }");
            sb.Append("table tr th { font-weight: bold; padding: 3px; padding-left: 10px; padding-right: 10px; }");
            sb.Append("table tr td { border: solid 1px black; padding: 3px;}");
            sb.Append("h1, h2, p { font-family: Rockwell; }");
            sb.Append("p { font-family: Rockwell; font-size: smaller; }");
            sb.Append("h2 { margin-top: 45px; }");
            sb.Append("</style></head><body>");
            sb.Append("<h1>Crawl Report</h1>");

            sb.Append("<h2>Internal Urls - In Order Crawled</h2>");
            sb.Append("<p>These are the pages found within the site.</p>");

            sb.Append("<table><tr><th>Url</th><th>ImagesCount</th></tr>");

            foreach (var item in visitedUrls)
            {
                sb.Append("<tr><td>");
                sb.Append(item.Key);
                sb.Append("</td><td>");
                sb.Append(item.Value);
                sb.Append("</td></tr>");
            }

            sb.Append("</table>");


            sb.Append("<h2>External Urls</h2>");
            sb.Append("<p>These are the links to the pages outside the site.</p>");

            sb.Append("<table><tr><th>Url</th></tr>");

            foreach (var url in externalurls)
            {
                sb.Append("<tr><td>");
                sb.Append(url);
                sb.Append("</td></tr>");
            }

            sb.Append("</table>");

            sb.Append("<h2>Duplicate Urls</h2>");
            sb.Append("<p>Any duplicate urls will be listed here.</p>");

            sb.Append("<table><tr><th>Url</th></tr>");

            if (duplicateUrls.Count > 0)
            {
                foreach (var url in duplicateUrls)
                {
                    sb.Append("<tr><td>");
                    sb.Append(url);
                    sb.Append("</td></tr>");
                }
            }
            else
                sb.Append("<tr><td>No Duplicate urls.</td></tr>");

            sb.Append("</table>");

            sb.Append("</body></html>");
            return sb;
        }

        private static void OpenReportInChrome(string file)
        {
            Process.Start("Chrome", Uri.EscapeDataString(file));
        }

        private static string SaveResultToFile(string contents)
        {
            string filePath = String.Format("{0}..\\..\\CrawlReport", AppDomain.CurrentDomain.BaseDirectory);
            string reportFile = String.Format("{0}\\report.htm", filePath);
            FileStream fStream = null;
            if (!Directory.Exists(filePath))
                Directory.CreateDirectory(filePath);

            if (File.Exists(reportFile))
            {
                File.Delete(reportFile);
                fStream = File.Create(reportFile);
            }
            else
                fStream = File.OpenWrite(reportFile);

            using (TextWriter writer = new StreamWriter(fStream))
            {
                writer.WriteLine(contents);
                writer.Flush();
            }

            fStream.Dispose();

            return reportFile;
        }
    }
}
