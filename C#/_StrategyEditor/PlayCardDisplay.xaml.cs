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

namespace _StrategyEditor
{
    /// <summary>
    /// Logique d'interaction pour PlayCardDisplay.xaml
    /// </summary>
    public partial class PlayCardDisplayControl : UserControl
    {
        //private PlayCard playCard;
        GlobalWorldMap globalWorldMap = new GlobalWorldMap();
        public PlayCardDisplayControl()//PlayCard pc)
        {
            InitializeComponent();
            //playCard = pc;
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
    }

    //public class PlayCardDisplayModel : ITabViewModel
    //{
    //    public string Header { get; set; }
    //    public String Titre = "Toto";
    //}
}
