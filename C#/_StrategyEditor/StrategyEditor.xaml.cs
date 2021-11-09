using Constants;
using MessagesNS;
using SciChart.Charting.Visuals;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
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
        GlobalWorldMap strategyMap = new GlobalWorldMap();

        public MainWindow()
        {
            // Set this code once in App.xaml.cs or application startup
            SciChartSurface.SetRuntimeLicenseKey("wsCOsvBlAs2dax4o8qBefxMi4Qe5BVWax7TGOMLcwzWFYRNCa/f1rA5VA1ITvLHSULvhDMKVTc+niao6URAUXmGZ9W8jv/4jtziBzFZ6Z15ek6SLU49eIqJxGoQEFWvjANJqzp0asw+zvLV0HMirjannvDRj4i/WoELfYDubEGO1O+oAToiJlgD/e2lVqg3F8JREvC0iqBbNrmfeUCQdhHt6SKS2QpdmOoGbvtCossAezGNxv92oUbog6YIhtpSyGikCEwwKSDrlKlAab6302LLyFsITqogZychLYrVXJTFvFVnDfnkQ9cDi7017vT5flesZwIzeH497lzGp3B8fKWFQyZemD2RzlQkvj5GUWBwxiKAHrYMnQjJ/PsfojF1idPEEconVsh1LoYofNk2v/Up8AzXEAvxWUEcgzANeQggaUNy+OFet8b/yACa/bgYG7QYzFQZzgdng8IK4vCPdtg4/x7g5EdovN2PI9vB76coMuKnNVPnZN60kSjtd/24N8A==");

            InitializeComponent();
            Init();
        }

        private void Init()
        {
            foreach(var val in Enum.GetValues(typeof(StoppedGameAction)))
                ComboBox_Situation.Items.Add(val.ToString());

            for(int i=0; i<5; i++)
            {
                globalWorldMapDisplay1.InitTeamMate(i, i.ToString(), new Location(i, 0, 0, 0, 0, 0));
            }
        }


        private void MenuItemLoadStrategy_Click(object sender, RoutedEventArgs e)
        {
            strategyMap.Init();
            
            List <PointD> playersList = new List<PointD>() { new PointD(-10.5, 0), new PointD(-7, 3), new PointD(-7, -3), new PointD(-4, 0), new PointD(-1, 1)};
                strategyMap.teammateLocationList = new List<Location>();
            int i = 1;
            foreach (var player in playersList)
            {
                //strategyMap.teammateLocationList.Add(new Location(player.X, player.Y, 0, 0, 0, 0));
                globalWorldMapDisplay1.InitTeamMate(i++, "toto", new Location(player.X, player.Y, 0, 0, 0, 0));
            }
            globalWorldMapDisplay1.UpdateGlobalWorldMap(strategyMap);
            globalWorldMapDisplay1.DisplayWorldMap();
        }

        private void MenuItemSaveStrategy_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MenuItemNewStrategy_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MenuItemSelectStrategyDirectory_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog openDirDlg = new FolderBrowserDialog();
            openDirDlg.SelectedPath = strategyDirectory;
            var result = openDirDlg.ShowDialog();
            if (result.ToString() != string.Empty)
            {
                strategyDirectory = openDirDlg.SelectedPath;
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
}
