using Constants;
using MessagesNS;
using Newtonsoft.Json;
using PlayBook_NS;
using SciChart.Charting.Visuals;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Utilities;
using WpfWorldMapDisplay;

namespace _StrategyEditor
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string strategyDirectory = "";

        PlayBook playBook = new PlayBook();

        GlobalWorldMap strategyMap = new GlobalWorldMap();

        public ObservableCollection<TabItem> Tabs { get; set; }

        public MainWindow()
        {
            // Set this code once in App.xaml.cs or application startup
            SciChartSurface.SetRuntimeLicenseKey("wsCOsvBlAs2dax4o8qBefxMi4Qe5BVWax7TGOMLcwzWFYRNCa/f1rA5VA1ITvLHSULvhDMKVTc+niao6URAUXmGZ9W8jv/4jtziBzFZ6Z15ek6SLU49eIqJxGoQEFWvjANJqzp0asw+zvLV0HMirjannvDRj4i/WoELfYDubEGO1O+oAToiJlgD/e2lVqg3F8JREvC0iqBbNrmfeUCQdhHt6SKS2QpdmOoGbvtCossAezGNxv92oUbog6YIhtpSyGikCEwwKSDrlKlAab6302LLyFsITqogZychLYrVXJTFvFVnDfnkQ9cDi7017vT5flesZwIzeH497lzGp3B8fKWFQyZemD2RzlQkvj5GUWBwxiKAHrYMnQjJ/PsfojF1idPEEconVsh1LoYofNk2v/Up8AzXEAvxWUEcgzANeQggaUNy+OFet8b/yACa/bgYG7QYzFQZzgdng8IK4vCPdtg4/x7g5EdovN2PI9vB76coMuKnNVPnZN60kSjtd/24N8A==");

            InitializeComponent();
            Init();
        }

        private void Init()
        {
        }

        private void DisplayPlayBook()
        {
            MyTabControl.Items.Clear();
            for (int i = 0; i < playBook.playingCards.Count; i++)
            {
                var playCardName = playBook.playingCards.Keys.ElementAt(i);
                var playingCard = playBook.playingCards.Values.ElementAt(i);

                PlayCardDisplayControl playingCardDisplay = new PlayCardDisplayControl(playingCard);
                var tabItem = new TabItem();
                tabItem.Content = playingCardDisplay;
                tabItem.Header = playCardName;

                for (int j = 0; j < playingCard.preferredLocation.Values.Count; j++)
                {
                    playingCardDisplay.globalWorldMapDisplay1.InitTeamMate(j, j.ToString(), playingCard.preferredLocation.ElementAt(j).Value);
                }

                playingCardDisplay.UpdatePlayingCardDisplay();

                MyTabControl.Items.Add(tabItem);
            }
        }


        private void MenuItemLoadStrategy_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDlg = new OpenFileDialog();
            openFileDlg.InitialDirectory = strategyDirectory;
            openFileDlg.Filter = "Play Book Files (.pbf)|*.pbf";
            var result = openFileDlg.ShowDialog();
            if (result != System.Windows.Forms.DialogResult.Cancel)
            {
                string filename = openFileDlg.FileName;
                //var js = new DataContractJsonSerializer(typeof(PlayBook));
                IFormatter formatter = new BinaryFormatter();

                using (FileStream SourceStream = File.Open(filename, FileMode.OpenOrCreate))
                {
                    //string json = new StreamReader(SourceStream).ReadToEnd();
                    playBook = (PlayBook)formatter.Deserialize(SourceStream);
                }
            }


            ///Initialisation du playbook
            //playBook = new PlayBook();
            //foreach (var playCard in playBook.playingCards)
            //{
            //    foreach (var loc in playCard.Value.preferredLocation)
            //    { 
            //        playCard.preferredLocation.Add(i, new Location(i, 0, 0, 0, 0, 0));
            //    }
            //    playBook.playingCards.Add(situation.ToString(), playCard);
            //}
            DisplayPlayBook();
        }

        private void MenuItemSaveStrategy_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog openFileDlg = new SaveFileDialog();
            openFileDlg.InitialDirectory = strategyDirectory;
            openFileDlg.Filter = "Play Book Files (.pbf)|*.pbf";
            var result = openFileDlg.ShowDialog();
            if (result != System.Windows.Forms.DialogResult.Cancel)
            {
                if (openFileDlg.FileName != strategyDirectory+"\\")
                {
                    string filename = openFileDlg.FileName;

                    IFormatter formatter = new BinaryFormatter();
                    //var js = new DataContractJsonSerializer(typeof(PlayBook));

                    using (FileStream SourceStream = File.Open(filename, FileMode.OpenOrCreate))
                    {
                        //js.WriteObject(SourceStream, playBook);
                        formatter.Serialize(SourceStream, playBook);
                    }
                }
            }
        }

        private void MenuItemNewStrategy_Click(object sender, RoutedEventArgs e)
        {
            ///Initialisation du playbook
            playBook = new PlayBook();
            foreach (var situation in Enum.GetValues(typeof(PlayingSituations))) 
            {
                PlayCard playCard = new PlayCard();
                for (int i = 0; i < 5; i++)
                {
                    playCard.preferredLocation.Add(i, new Location(i, 0, 0, 0, 0, 0));
                }
                playBook.playingCards.Add(situation.ToString(), playCard);
            }
            DisplayPlayBook();
        }

        private void MenuItemSelectStrategyDirectory_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog openDirDlg = new FolderBrowserDialog();
            openDirDlg.SelectedPath = strategyDirectory;
            var result = openDirDlg.ShowDialog();
            if (result.ToString() != string.Empty)
            {
                strategyDirectory = openDirDlg.SelectedPath;

                System.Configuration.Configuration configFile;
                configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                configFile.AppSettings.Settings["StrategyDirectory"].Value = strategyDirectory.ToString();
                configFile.Save();
            }
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            Configuration configFile;
            configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            try
            {
                double top = 0;
                double.TryParse(configFile.AppSettings.Settings["Top"].Value, out top);
                this.Top = top;

                double left = 0;
                double.TryParse(configFile.AppSettings.Settings["Left"].Value, out left);
                this.Left = left;

                double height = 0;
                double.TryParse(configFile.AppSettings.Settings["Height"].Value, out height);
                this.Height = height;

                double width = 0;
                double.TryParse(configFile.AppSettings.Settings["Width"].Value, out width);
                this.Width = width;

                bool maximized = false;
                bool.TryParse(configFile.AppSettings.Settings["Maximized"].Value, out maximized);
                if (maximized)
                    WindowState = WindowState.Maximized;

                strategyDirectory = configFile.AppSettings.Settings["StrategyDirectory"].Value;
                if (strategyDirectory == "./")
                    strategyDirectory = "";

            }
            catch {; }
        }
        

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            System.Configuration.Configuration configFile;
            configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            try
            {
                if (WindowState == WindowState.Maximized)
                {
                    // Use the RestoreBounds as the current values will be 0, 0 and the size of the screen     

                    configFile.AppSettings.Settings["Top"].Value = RestoreBounds.Top.ToString();
                    configFile.AppSettings.Settings["Left"].Value = RestoreBounds.Left.ToString();
                    configFile.AppSettings.Settings["Width"].Value = RestoreBounds.Width.ToString();
                    configFile.AppSettings.Settings["Height"].Value = RestoreBounds.Height.ToString();
                    configFile.AppSettings.Settings["Maximized"].Value = true.ToString();
                }
                else
                {
                    configFile.AppSettings.Settings["Top"].Value = this.Top.ToString();
                    configFile.AppSettings.Settings["Left"].Value = this.Left.ToString();
                    configFile.AppSettings.Settings["Width"].Value = this.Width.ToString();
                    configFile.AppSettings.Settings["Height"].Value = this.Height.ToString();
                    configFile.AppSettings.Settings["Maximized"].Value = false.ToString();
                }
                configFile.AppSettings.Settings["StrategyDirectory"].Value = strategyDirectory.ToString();
                configFile.Save();
            }
            catch { }
        }
    }

    //public interface ITabViewModel
    //{
    //    String Header { get; set; }
    //}

    //// ViewModelA

    //public class ViewModelA : ITabViewModel
    //{
    //    public string Header { get; set; }
    //    public ViewModelA()
    //    {
    //    }
    //}

    //// ViewModelB

    //public class ViewModelB : ITabViewModel
    //{
    //    public string Header { get; set; }
    //    public ViewModelB()
    //    {
    //    }
    //}

    //// ViewModelC

    //public class ViewModelC : ITabViewModel
    //{
    //    public string Header { get; set; }
    //    public ViewModelC()
    //    {
    //    }
    //}

    //public class TabViewModel
    //{
    //    public ObservableCollection<ITabViewModel> TabViewModels { get; set; }

    //    public TabViewModel()
    //    {
    //        TabViewModels = new ObservableCollection<ITabViewModel>();
    //        TabViewModels.Add(new ViewModelA { Header = "Tab A" });
    //        TabViewModels.Add(new ViewModelB { Header = "Tab B" });
    //        TabViewModels.Add(new ViewModelC { Header = "Tab C" });
    //        TabViewModels.Add(new PlayCardDisplayModel { Header = "Tab C" });
    //    }
    //}
}
