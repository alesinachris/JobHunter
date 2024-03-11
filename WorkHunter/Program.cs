using System.Diagnostics;
using System.Xml.Serialization;
using ParserLib;
using PuppeteerSharp;

namespace WorkHunter
{
    internal class Program
    {
        public static readonly List<Ad> Ads = new();
        //private static readonly List<Ad> CachedAds = new();

#pragma warning disable IDE0210 // Convert to top-level statements
#pragma warning disable IDE0060 // Remove unused parameter
        private static void Main(string[] args)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            try
            {
                foreach (Process process in Process.GetProcessesByName("chrome"))
                {
                    process.Kill();
                }
            }
            catch { }
            Task<List<Ad>> t = HHParser.Go();
            t.Wait();
            List<Ad> res = t.Result;
        }



    }
}
