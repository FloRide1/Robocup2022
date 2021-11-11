using EventArgsLibrary;
using MessagesNS;
using PlayBook_NS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Utilities;

namespace _StrategyEditor
{
    /// <summary>
    /// Logique d'interaction pour PlayCardDisplay.xaml
    /// </summary>
    public partial class PlayCardDisplayControl : UserControl
    {
        GlobalWorldMap globalWorldMap = new GlobalWorldMap();
        PlayCard playingCard;
        int currentPlayer = 0;

        public PlayCardDisplayControl(PlayCard pc)
        {
            InitializeComponent();

            globalWorldMapDisplay1.OnCtrlClickOnHeatMapEvent += this.GlobalWorldMapDisplay1_OnCtrlClickOnHeatMapEvent;

            playingCard = pc;

            /// Init de la comboBox de sélection de rôle
            for (int i = 0; i < 5; i++)
            {
                ComboBoxItem item = new ComboBoxItem();
                if (i == 0)
                    item.Content = "Gardien";
                else
                    item.Content = "Player " + i.ToString();
                item.Tag = i.ToString();
                ComboBox_Player.Items.Add(item);
            }

            InitPlayCardDisplay();
        }

        public void InitPlayCardDisplay()
        {

            for (int j = 0; j < playingCard.preferredLocation.Values.Count; j++)
            {
                var teammateLocation = playingCard.preferredLocation[j];
                globalWorldMap.teammateLocationList.Add(teammateLocation);
                globalWorldMap.teammateDestinationLocationList.Add(j, teammateLocation);
                globalWorldMap.teammateWayPointList.Add(j, teammateLocation);
            }

            UpdateWorldMap(globalWorldMap);
        }

        public void UpdatePlayingCardDisplay()
        {
            for (int j = 0; j < playingCard.preferredLocation.Values.Count; j++)
            {
                var teammateLocation = playingCard.preferredLocation[j];
                globalWorldMap.teammateLocationList[j] = teammateLocation;
                globalWorldMap.teammateDestinationLocationList[j] = teammateLocation;
                globalWorldMap.teammateWayPointList[j] = teammateLocation;
            }
            UpdateWorldMap(globalWorldMap);
        }


        /// <summary>
        ///  Forward de l'évènement click sur la Global World Map
        /// </summary>
        private void GlobalWorldMapDisplay1_OnCtrlClickOnHeatMapEvent(object sender, PositionArgs e)
        {
            playingCard.preferredLocation[currentPlayer] = new Location(e.X, e.Y, 0, 0, 0, 0);
            UpdatePlayingCardDisplay();

        }

        public void UpdateWorldMap(GlobalWorldMap gwm)
        {
            globalWorldMap = gwm;
            UpdateDisplay();
        }

        public void UpdateDisplay()
        {
            globalWorldMapDisplay1.UpdateGlobalWorldMap(globalWorldMap);            
        }

        private void ComboBox_Player_Selected(object sender, SelectionChangedEventArgs e)
        {
            currentPlayer = int.Parse(((ComboBoxItem)ComboBox_Player.SelectedItem).Tag.ToString());
        }
    }

    //public class PlayCardDisplayModel : ITabViewModel
    //{
    //    public string Header { get; set; }
    //    public String Titre = "Toto";
    //}
}
