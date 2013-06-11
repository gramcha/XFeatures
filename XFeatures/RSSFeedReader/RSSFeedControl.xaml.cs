using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;
using System.Xml.XPath;
using System.Collections.ObjectModel;
using System.IO;
using Atmel.XFeatures.Settings;
using System.ServiceModel.Syndication;
using System.ComponentModel;

namespace Atmel.XFeatures.RSSFeedReader
{
    /// <summary>
    /// Interaction logic for RSSFeedControl.xaml
    /// </summary>
    public partial class RSSFeedControl : UserControl
    {
        /// <summary>
        /// http://en.wikipedia.org/wiki/RSS
        /// </summary>

        private string rssUrl ="http://www.avrfreaks.net/forumrss.php";// "http://asf.atmel.com/bugzilla/buglist.cgi?bug_status=UNCONFIRMED&bug_status=NEW&bug_status=ASSIGNED&bug_status=REOPENED&content=&product=Atmel%20Studio%206&query_format=specific&title=Bug%20List&ctype=atom";
        System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
        ObservableCollection<RSSFeedDataset> rssfeedcollection =
       new ObservableCollection<RSSFeedDataset>();
        private bool istimerrunning = false;
        Object thisLock = new Object();
        private string recenturl;

        //private readonly BackgroundWorker worker = new BackgroundWorker();

        public ObservableCollection<RSSFeedDataset> FeedCollection
        {
            get { return rssfeedcollection; 
            }
        }
        public RSSFeedControl()
        {
            InitializeComponent();            
            this.DataContext = this;
            init();
        }
        void init()
        {
            dispatcherTimer.Tick += new EventHandler(DispatcherTimerRssFeedAutoUpdate);
            recenturl = SettingsProvider.GetRssFeedUrl();
            GetFeeds(recenturl);
            SettingsManager.UpdateHandler += UpdateRSSFeed;
        }
        private void UpdateRSSFeed(object o)
        {
            if (recenturl.Equals(SettingsProvider.GetRssFeedUrl())==false)
            {
                recenturl = SettingsProvider.GetRssFeedUrl();
                GetFeeds(recenturl);
                address.Text = recenturl;
            }
        }

        private void StartTimer()
        {
            if (istimerrunning == false)
            {
                dispatcherTimer.Interval = new TimeSpan(0, SettingsProvider.GetRssFeedUpdateTime(), 0);
                dispatcherTimer.Start();
                istimerrunning = true;
            }
        }

        private void StopTimer()
        {
            if (istimerrunning == true)
            {
                dispatcherTimer.Stop();
                istimerrunning = false;
            }
        }

        private void DispatcherTimerRssFeedAutoUpdate(object sender, EventArgs e)
        {
            new Thread(sample).Start(recenturl);// .GetFeeds(recenturl);
        }
        void sample(object o)
        {
            GetFeeds((string) o);
        }

        private bool TryRSSFormat(string url)
        {
            if (string.IsNullOrEmpty(url) || string.IsNullOrWhiteSpace(url))
                return false;
            // set the wait cursor
            
            // create a new xml doc
            XmlDocument doc = new XmlDocument();
            DateTime date;
            try
            {
                // load the xml doc
                doc.Load(url);
                date = DateTime.Now;
            }
            catch (Exception ex1)
            {
                //// return the cursor
                //this.Cursor = Cursors.Arrow;

                // tell a story
                MessageBox.Show(ex1.Message, "Atmel Studio - TryRSSFormat");
                return false;
            }
            // get an xpath navigator   
            XPathNavigator navigator = doc.CreateNavigator();
            List<RSSFeedDataset> list = new List<RSSFeedDataset>();
            //Dictionary<string, RSSFeedDataset> list = new Dictionary<string, RSSFeedDataset>();
            try
            {
                // look for the path to the rss item titles; navigate
                // through the nodes to get all titles
                XPathNodeIterator nodes;
                nodes = navigator.Select("//rss/channel/item/title");
                while (nodes.MoveNext())
                {
                    // clean up the text for display
                    XPathNavigator node = nodes.Current;
                    if (node != null)
                    {
                        string tmp = node.Value.Trim();
                        tmp = tmp.Replace("\n", "");
                        tmp = tmp.Replace("\r", "");

                        // add a new treeview node for this
                        // news item title
                        var item = new RSSFeedDataset();
                        item.Subject = tmp;
                        item.Time = date.ToString();
                        list.Add(item);
                    }
                }

                // set a position counter
                int position = 0;

                // Get the links from the RSS feed
                XPathNodeIterator nodesLink = navigator.Select("//rss/channel/item/link");
                while (nodesLink.MoveNext())
                {
                    // clean up the link
                    XPathNavigator node = nodesLink.Current;
                    if (node != null)
                    {
                        string tmp = node.Value.Trim();
                        tmp = tmp.Replace("\n", "");
                        tmp = tmp.Replace("\r", "");

                        // use the position counter
                        // to add a link child node
                        // to each news item title
                        list[position].Url = tmp;
                    }

                    // increment the position counter
                    position++;
                }
                position = 0;
                // Get the links from the RSS feed
                XPathNodeIterator nodesDescription = navigator.Select("//rss/channel/item/description");
                while (nodesDescription.MoveNext())
                {
                    // clean up the link
                    XPathNavigator node = nodesDescription.Current;
                    if (node != null)
                    {
                        string tmp = node.Value.Trim();
                        //tmp = tmp.Replace("\n", "");
                        //tmp = tmp.Replace("\r", "");

                        // use the position counter
                        // to add a link child node
                        // to each news item title
                        list[position].Description = tmp;
                    }

                    // increment the position counter
                    position++;
                }

                //foreach (var item in list)
                //{
                //    rssfeedcollection.Add(item);
                //}
                if (list.Any())
                {
                    Dispatcher.CurrentDispatcher.Invoke(new Action<object>((Action) =>
                                                                               {
                                                                                   foreach (var item in list)
                                                                                   {
                                                                                       rssfeedcollection.Add(item);
                                                                                   }
                                                                               }));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "RSS Feed Load Error");
                return false;
            }
            switch (list.Any())
            {
                case false:
                    return false;
                default:
                    return true;
            }
        }
        private bool TryAtomFormat(string url)
        {
            try
            {
                //rssfeedcollection = new ObservableCollection<RSSFeedDataset>();
                WebRequest request = WebRequest.Create(url);

                request.Timeout = 50000;

                using (WebResponse response = request.GetResponse())
                using (XmlReader reader = XmlReader.Create(response.GetResponseStream()))
                {

                    SyndicationFeed feed = SyndicationFeed.Load(reader);
                    DateTime date = DateTime.Now;
                    if (feed != null)
                    {
                        List<RSSFeedDataset> list = new List<RSSFeedDataset>();
                        foreach (var nodeitem in feed.Items)
                        {
                            // Work with the Title and PubDate properties of the FeedItem
                            var item = new RSSFeedDataset();
                            item.Subject = nodeitem.Title.Text;
                            item.Time = date.ToString(CultureInfo.InvariantCulture);
                            item.Url = nodeitem.Id;
                            //item.Title.Text
                            //item.Id
                            if (nodeitem.Summary != null)
                            {
                                //item.Summary.Text
                                item.Description = nodeitem.Summary.Text;
                            }
                            else
                            {
                                if (nodeitem.Content != null)
                                {
                                    item.Description =
                                        ((System.ServiceModel.Syndication.TextSyndicationContent)(nodeitem.Content)).Text;
                                }
                            }
                            list.Add(item);
                            rssfeedcollection.Add(item);
                            
                        }
                        //if (list.Any())
                        //{
                        //    //Dispatcher.CurrentDispatcher.Invoke(new Action<object>((Action) =>
                        //    //                                                           {
                        //    //                                                               foreach (var item in list)
                        //    //                                                               {
                        //    //                                                                   rssfeedcollection.Add(
                        //    //                                                                       item);
                        //    //                                                               }
                        //    //                                                           }));

                        //    Dispatcher.CurrentDispatcher.Invoke(new Action<object>((Action) =>
                        //                                                               {
                        //                                                                   for(int i=0;i<list.Count;i++)
                        //                                                                       rssfeedcollection.Add(list[i]);
                        //                                                               }));
                        //}
                    }
                }
                if (rssfeedcollection.Any() == false)
                    return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "AtmelStudio - TryAtomFormat");
                return false;
            }
            
            return true;
        }

        private void GetFeeds(string url, bool manualchange=false)
        {
            //this.Cursor = Cursors.Wait;
            StopTimer();
            lock (thisLock)
            {
                rssfeedcollection.Clear();
                
                if (TryRSSFormat(url) == false)
                {
                    if (TryAtomFormat(url) == false)
                        return;
                }
                if(manualchange == true)
                {
                    UpdateXSettingsFile(url);
                }
            }
            StartTimer();
            //// return the cursor
            //this.Cursor = Cursors.Arrow;
        }
        private void UpdateXSettingsFile(string url)
        {
            SettingsManager.XSettings.OtherFeatures.Link = url;
            SettingsManager.WriteSettings(SettingsManager.XSettings);
        }

        private void lviewrss_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                lock (thisLock)
                {
                    var item = (RSSFeedDataset)lviewrss.SelectedItem;
                    if (item == null)
                        return;
                    // Get URI to navigate to
                    Uri uri = new Uri(item.Url, UriKind.RelativeOrAbsolute);
                    // Only absolute URIs can be navigated to
                    if (!uri.IsAbsoluteUri)
                    {
                        MessageBox.Show("The Address URI must be absolute eg 'http://www.atmel.com'");
                        return;
                    }
                    //MessageBox.Show(item.Description);
                    string htmlcontent = "<html><body>" + item.Description + "</body></html>";
                    var fileName = System.IO.Path.GetTempFileName() + ".html";
                    var streamWriter = new StreamWriter(fileName);
                    //MessageBox.Show(htmlcontent);
                    streamWriter.Write(htmlcontent);
                    streamWriter.Close();
                    Description.Navigate(new Uri(fileName));
                    webBrowser1.Navigate(uri);
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message,"Atmel Studio - Selection Changed");
            }
            
        }

        private void goButton_Click(object sender, RoutedEventArgs e)
        {
            GetFeeds(address.Text,true);
        }
    }
}
