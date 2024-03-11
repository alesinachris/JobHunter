using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Newtonsoft.Json;
using ParserLib;

namespace JobHunter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<Ad> Ads { get; set; } = new();

        private List<Ad> adListCache = new();
        private readonly string blackListPath = "blackList.json";

        public MainWindow()
        {
            InitializeComponent();
            adsDataGrid.ItemsSource = Ads;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitBlackList();
        }

        private void InitBlackList()
        {
            try
            {
                if (File.Exists(blackListPath))
                {
                    string jsonText = File.ReadAllText(blackListPath);
                    adListCache = JsonConvert.DeserializeObject<List<Ad>>(jsonText);
                }
            }
            catch
            {

            }
        }

        private void KillChrome()
        {
            try
            {
                foreach (Process process in Process.GetProcessesByName("chrome"))
                {
                    process.Kill();
                }
            }
            catch { }
        }

        private async void ParserStart()
        {
            while (true)
            {
                try
                {
                    KillChrome();
                    List<Ad> ads = await HHParser.Go(browserVisibilityCheckbox.IsChecked ?? false, adListCache);
                    bool isAdAdded = false;
                    foreach (Ad ad in ads)
                    {
                        bool isExist = false;
                        foreach (Ad _ad in adListCache)
                        {
                            if (_ad.Id == ad.Id)
                            {
                                isExist = true;
                                break;
                            }
                        }
                        if (!isExist)
                        {
                            Ads.Insert(0, ad);
                            isAdAdded = true;
                            adListCache.Add(ad);
                            SaveAdListCache();

                        }
                    }
                    if (isAdAdded)
                    {
                        PlaySound();
                    }
                }
                catch
                {

                }
                _ = Task.Delay(60000);
            }

        }

        private void SaveAdListCache()
        {
            try
            {
                string output = JsonConvert.SerializeObject(adListCache);
                File.WriteAllText(blackListPath, output);
            }
            catch (Exception)
            {

            }

        }

        private void PlaySound()
        {
            MediaPlayer player = new();
            player.MediaFailed += (s, e) => MessageBox.Show("Error");
            player.Open(new Uri("C:\\Users\\XXX\\Desktop\\JobHunter\\JobHunter\\bin\\Debug\\net6.0-windows\\vk.mp3", UriKind.RelativeOrAbsolute));
            player.Play();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string tag = ((Button)sender).Tag.ToString();
            string vacancyUrl = "https://hh.ru/vacancy/" + tag;
            _ = Process.Start("C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe", vacancyUrl);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Ads.Clear();
        }

        private void Button_Start(object sender, RoutedEventArgs e)
        {
            ParserStart();
        }
    }
}
