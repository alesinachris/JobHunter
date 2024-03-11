using HtmlAgilityPack;
using PuppeteerSharp;

namespace ParserLib
{
    public class HHParser
    {
        private const string executablePath = "C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe";

        public static async Task<List<Ad>> Go(bool isHeadless, List<Ad> adListCache)
        {
            List<Ad> ads = new();
            string url = "https://hh.ru/search/vacancy?area=113&ored_clusters=true&schedule=remote&text=c%23&order_by=publication_time";
            Browser browser = (Browser)await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = isHeadless,
                ExecutablePath = executablePath,
                Args = new[] { "--start-maximized" },
                //IgnoreDefaultArgs = true,
                //UserDataDir = "C:\\Users\\Maxxx\\AppData\\Local\\MyBot\\2ti2mva3.5jc"
            }).ConfigureAwait(false);

            Page page = (Page)await browser.NewPageAsync();
            await page.SetViewportAsync(new ViewPortOptions
            {
                Width = 1920,
                Height = 1080,
            });

            try
            {
                _ = await page.GoToAsync(url, 60000);
            }
            catch { }
            await Task.Delay(5000);

            string html = await page.GetContentAsync();
            HtmlDocument htmlDoc = new();

            htmlDoc.LoadHtml(html);
            HtmlNodeCollection nodes = htmlDoc.DocumentNode.SelectNodes("//div[@class=\"serp-item serp-item_link\"]");
            int count = 0;
            foreach (HtmlNode node in nodes)
            {
                HtmlNode linkNode = node.SelectSingleNode(".//span[@class=\"serp-item__title-link-wrapper\"]/a[@class=\"bloko-link\"]");
                string link = linkNode.GetAttributeValue("href", null);

                //парсим ID вакансии
                string[] ss = link.Split('?')[0].Split('/');
                string idString = ss[^1];
                int id = int.Parse(idString);

                //проверка существования объявления
                bool isExist = false;
                foreach (Ad cacheAd in adListCache)
                {
                    if (cacheAd.Id == id)
                    {
                        isExist = true;
                        break;
                    }
                }
                if (isExist)
                {
                    continue;
                }

                //парсим название вакансии
                string title = linkNode.InnerText;

                //тут Крис получает зарплату и компанию
                HtmlNode companyNameNode = node.SelectSingleNode(".//div[@class=\"vacancy-serp-item__meta-info-company\"]/a");
                string companyName = companyNameNode.InnerText;


                Ad ad = new()
                {
                    Id = id,
                    Title = title,
                    DateTime = DateTime.Now,
                    Company = companyName
                };

                count++;
                if (count == 10)
                {
                    //break;
                }
                await SetSalary(ad, page);
                ads.Add(ad);

            }

            return ads;
        }

        private static async Task SetSalary(Ad ad, Page page)
        {
            string vacancyUrl = "https://hh.ru/vacancy/" + ad.Id.ToString();
            _ = await page.GoToAsync(vacancyUrl, 60000);
            string html2 = await page.GetContentAsync();
            HtmlDocument htmlDoc2 = new();

            htmlDoc2.LoadHtml(html2);
            HtmlNode salaryNode = htmlDoc2.DocumentNode.SelectSingleNode(".//span[@data-qa=\"vacancy-salary-compensation-type-net\"]");
            if (salaryNode != null)
            {
                string salaryString = salaryNode.InnerText.Replace("до", "").Replace("от", "").Replace("&nbsp;", "").Replace("  ", " ").Replace("  ", " ").Split('₽')[0].Trim();
                string[] strings = salaryString.Split(' ');
                int minSalary = int.Parse(strings[0]);
                ad.MinSalary = minSalary;
                if (strings.Length == 2)
                {
                    int maxSalary = int.Parse(strings[1]);
                    ad.MaxSalary = maxSalary;
                }
                ad.SalaryString = salaryString;
            }
        }
    }
}
