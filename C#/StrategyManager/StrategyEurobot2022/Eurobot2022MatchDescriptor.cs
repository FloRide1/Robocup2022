using Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;
using static StrategyManagerNS.StrategyEurobot2022;

namespace StrategyManagerNS
{
    public class Eurobot2022MatchDescriptor
    {
        public Dictionary<int, Eurobot2022ElementDeJeu> listElementsJeu = new Dictionary<int, Eurobot2022ElementDeJeu>();
        public Dictionary<int, Eurobot2022EmplacementDepose> DictionaryEmplacementDepose = new Dictionary<int, Eurobot2022EmplacementDepose>();
        public Dictionary<string, Eurobot2022StateBrasTurbineRobot> DictionaryBrasTurbine = new Dictionary<string, Eurobot2022StateBrasTurbineRobot>();
        int CompteurGobeletsSurBras = 0;
        public int CompteurDeposeAEffectuer { get; private set; }

        List<int> ListElementsJeuOrdonneeBlueRobotNord;
        List<int> ListElementsJeuOrdonneeBlueRobotSud;
        List<int> ListElementsJeuOrdonneeYellowRobotNord;
        List<int> ListElementsJeuOrdonneeYellowRobotSud;

        public Eurobot2022MatchDescriptor()
        {
            Init();
        }

        public void Init()
        {
            FillElementsdeJeuListQualif();
            FillEmplacementList();
            FillBrasTurbineDictionary();

            DefineStrategyQualifications();
            LoadStrategy();
            CompteurGobeletsSurBras = 0;
            CompteurDeposeAEffectuer = 0;

        }

        void DefineStrategyQualifications()
        {
            ListElementsJeuOrdonneeBlueRobotNord = new List<int>()
            {
                16, 14, 10, 11, 12, 105, 35, 36, 37,38,39, 102, 103, 40,41,42,43,44
            };
            ListElementsJeuOrdonneeBlueRobotSud = new List<int>()
            {
                15, 13, 9, 19, 18, 7, 6, 5, 30, 31, 32, 33, 34
            };
            ListElementsJeuOrdonneeYellowRobotNord = new List<int>()
            {
                2, 4, 7, 6, 5, 104, 34, 33, 32, 31, 30, 100, 101, 25, 26, 27, 28, 29
            };
            ListElementsJeuOrdonneeYellowRobotSud = new List<int>()
            {
                1, 3, 8, 22, 23, 10, 11, 12, 39, 38, 37, 36, 35
            };
        }  

        void LoadStrategy()
        {
            for (int i = 0; i < ListElementsJeuOrdonneeBlueRobotNord.Count; i++)
            {
                if (listElementsJeu.ContainsKey(ListElementsJeuOrdonneeBlueRobotNord[i]))
                {
                    listElementsJeu[ListElementsJeuOrdonneeBlueRobotNord[i]].RobotAttributionBlue = Eurobot2022RobotType.RobotNord;
                    listElementsJeu[ListElementsJeuOrdonneeBlueRobotNord[i]].PriorityBlue = ListElementsJeuOrdonneeBlueRobotNord.Count - i;
                }
            }
            for (int i = 0; i < ListElementsJeuOrdonneeBlueRobotSud.Count; i++)
            {
                if (listElementsJeu.ContainsKey(ListElementsJeuOrdonneeBlueRobotSud[i]))
                {
                    listElementsJeu[ListElementsJeuOrdonneeBlueRobotSud[i]].RobotAttributionBlue = Eurobot2022RobotType.RobotSud;
                    listElementsJeu[ListElementsJeuOrdonneeBlueRobotSud[i]].PriorityBlue = ListElementsJeuOrdonneeBlueRobotSud.Count - i;
                }
            }
            for (int i = 0; i < ListElementsJeuOrdonneeYellowRobotNord.Count; i++)
            {
                if (listElementsJeu.ContainsKey(ListElementsJeuOrdonneeYellowRobotNord[i]))
                {
                    listElementsJeu[ListElementsJeuOrdonneeYellowRobotNord[i]].RobotAttributionYellow = Eurobot2022RobotType.RobotNord;
                    listElementsJeu[ListElementsJeuOrdonneeYellowRobotNord[i]].PriorityYellow = ListElementsJeuOrdonneeYellowRobotNord.Count - i;
                }
            }
            for (int i = 0; i < ListElementsJeuOrdonneeYellowRobotSud.Count; i++)
            {
                if (listElementsJeu.ContainsKey(ListElementsJeuOrdonneeYellowRobotSud[i]))
                {
                    listElementsJeu[ListElementsJeuOrdonneeYellowRobotSud[i]].RobotAttributionYellow = Eurobot2022RobotType.RobotSud;
                    listElementsJeu[ListElementsJeuOrdonneeYellowRobotSud[i]].PriorityYellow = ListElementsJeuOrdonneeYellowRobotSud.Count - i;
                }
            }
        }

        private void FillElementsdeJeuListQualif()
        {
            lock (listElementsJeu)
            {
                listElementsJeu = new Dictionary<int, Eurobot2022ElementDeJeu>();
                /// Manche à air
                listElementsJeu.Add(100, new Eurobot2022MancheAir(Eurobot2022TypeELementDeJeu.MancheAir, 1.270, -1.00, Eurobot2022TeamReservation.ReservedYellow, null));
                listElementsJeu.Add(101, new Eurobot2022MancheAir(Eurobot2022TypeELementDeJeu.MancheAir, 0.865, -1.00, Eurobot2022TeamReservation.ReservedYellow, null));
                listElementsJeu.Add(102, new Eurobot2022MancheAir(Eurobot2022TypeELementDeJeu.MancheAir, -1.270, -1.00, Eurobot2022TeamReservation.ReservedBlue, null));
                listElementsJeu.Add(103, new Eurobot2022MancheAir(Eurobot2022TypeELementDeJeu.MancheAir, -0.865, -1.00, Eurobot2022TeamReservation.ReservedBlue, null));

                /// Phares
                listElementsJeu.Add(104, new Eurobot2022Phare(Eurobot2022TypeELementDeJeu.Phare, 1.15, 1.00, Eurobot2022TeamReservation.ReservedYellow, null));
                listElementsJeu.Add(105, new Eurobot2022Phare(Eurobot2022TypeELementDeJeu.Phare, -1.15, 1.00, Eurobot2022TeamReservation.ReservedBlue, null));

                /// Gobelets de champ
                listElementsJeu.Add(1, new Eurobot2022Gobelet(1.200, -0.200, Eurobot2022Color.Rouge, Eurobot2022TypeGobelet.Libre, Eurobot2022TeamReservation.ReservedYellow, null));
                listElementsJeu.Add(2, new Eurobot2022Gobelet(1.200, 0.600, Eurobot2022Color.Vert, Eurobot2022TypeGobelet.Libre, Eurobot2022TeamReservation.ReservedYellow, null));
                listElementsJeu.Add(3, new Eurobot2022Gobelet(1.050, -0.080, Eurobot2022Color.Vert, Eurobot2022TypeGobelet.Libre, Eurobot2022TeamReservation.ReservedYellow, null));
                listElementsJeu.Add(4, new Eurobot2022Gobelet(1.050, 0.490, Eurobot2022Color.Rouge, Eurobot2022TypeGobelet.Libre, Eurobot2022TeamReservation.ReservedYellow, null));
                listElementsJeu.Add(5, new Eurobot2022Gobelet(0.830, 0.900, Eurobot2022Color.Vert, Eurobot2022TypeGobelet.Libre, Eurobot2022TeamReservation.Shared, Math.PI / 2));
                listElementsJeu.Add(6, new Eurobot2022Gobelet(0.550, 0.600, Eurobot2022Color.Rouge, Eurobot2022TypeGobelet.Libre, Eurobot2022TeamReservation.Shared, null));
                listElementsJeu.Add(7, new Eurobot2022Gobelet(0.400, 0.200, Eurobot2022Color.Vert, Eurobot2022TypeGobelet.Libre, Eurobot2022TeamReservation.Shared, null));
                listElementsJeu.Add(8, new Eurobot2022Gobelet(0.230, -0.200, Eurobot2022Color.Rouge, Eurobot2022TypeGobelet.Libre, Eurobot2022TeamReservation.Shared, null));
                listElementsJeu.Add(9, new Eurobot2022Gobelet(-0.230, -0.200, Eurobot2022Color.Vert, Eurobot2022TypeGobelet.Libre, Eurobot2022TeamReservation.Shared, null));
                listElementsJeu.Add(10, new Eurobot2022Gobelet(-0.400, 0.200, Eurobot2022Color.Rouge, Eurobot2022TypeGobelet.Libre, Eurobot2022TeamReservation.Shared, null));
                listElementsJeu.Add(11, new Eurobot2022Gobelet(-0.550, 0.600, Eurobot2022Color.Vert, Eurobot2022TypeGobelet.Libre, Eurobot2022TeamReservation.Shared, null));
                listElementsJeu.Add(12, new Eurobot2022Gobelet(-0.830, 0.900, Eurobot2022Color.Rouge, Eurobot2022TypeGobelet.Libre, Eurobot2022TeamReservation.Shared, Math.PI / 2));
                listElementsJeu.Add(13, new Eurobot2022Gobelet(-1.050, -0.080, Eurobot2022Color.Rouge, Eurobot2022TypeGobelet.Libre, Eurobot2022TeamReservation.ReservedBlue, null));
                listElementsJeu.Add(14, new Eurobot2022Gobelet(-1.050, 0.490, Eurobot2022Color.Vert, Eurobot2022TypeGobelet.Libre, Eurobot2022TeamReservation.ReservedBlue, null));
                listElementsJeu.Add(15, new Eurobot2022Gobelet(-1.200, -0.200, Eurobot2022Color.Vert, Eurobot2022TypeGobelet.Libre, Eurobot2022TeamReservation.ReservedBlue, null));
                listElementsJeu.Add(16, new Eurobot2022Gobelet(-1.200, 0.6, Eurobot2022Color.Rouge, Eurobot2022TypeGobelet.Libre, Eurobot2022TeamReservation.ReservedBlue, null));

                /// Gobelets des petits ports
                listElementsJeu.Add(17, new Eurobot2022Gobelet(0.495, -0.955, Eurobot2022Color.Vert, Eurobot2022TypeGobelet.Libre, Eurobot2022TeamReservation.ReservedBlue, -Math.PI / 3));
                listElementsJeu.Add(18, new Eurobot2022Gobelet(0.435, -0.65, Eurobot2022Color.Rouge, Eurobot2022TypeGobelet.Libre, Eurobot2022TeamReservation.ReservedBlue, -Math.PI / 2));
                listElementsJeu.Add(19, new Eurobot2022Gobelet(0.165, -0.65, Eurobot2022Color.Vert, Eurobot2022TypeGobelet.Libre, Eurobot2022TeamReservation.ReservedBlue, -Math.PI / 2));
                listElementsJeu.Add(20, new Eurobot2022Gobelet(0.105, -0.955, Eurobot2022Color.Rouge, Eurobot2022TypeGobelet.Libre, Eurobot2022TeamReservation.ReservedBlue, -2 * Math.PI / 3));

                listElementsJeu.Add(21, new Eurobot2022Gobelet(-0.105, -0.955, Eurobot2022Color.Vert, Eurobot2022TypeGobelet.Libre, Eurobot2022TeamReservation.ReservedYellow, -Math.PI / 3));
                listElementsJeu.Add(22, new Eurobot2022Gobelet(-0.165, -0.650, Eurobot2022Color.Rouge, Eurobot2022TypeGobelet.Libre, Eurobot2022TeamReservation.ReservedYellow, -Math.PI / 2));
                listElementsJeu.Add(23, new Eurobot2022Gobelet(-0.435, -0.650, Eurobot2022Color.Vert, Eurobot2022TypeGobelet.Libre, Eurobot2022TeamReservation.ReservedYellow, -Math.PI / 2));
                listElementsJeu.Add(24, new Eurobot2022Gobelet(-0.495, -0.955, Eurobot2022Color.Rouge, Eurobot2022TypeGobelet.Libre, Eurobot2022TeamReservation.ReservedYellow, -2 * Math.PI / 3));

                /// Ecueil privé jaune
                listElementsJeu.Add(25, new Eurobot2022Gobelet(1.567, -0.750, Eurobot2022Color.Vert, Eurobot2022TypeGobelet.Distributeur, Eurobot2022TeamReservation.ReservedYellow, 0));
                listElementsJeu.Add(26, new Eurobot2022Gobelet(1.567, -0.675, Eurobot2022Color.Rouge, Eurobot2022TypeGobelet.Distributeur, Eurobot2022TeamReservation.ReservedYellow, 0));
                listElementsJeu.Add(27, new Eurobot2022Gobelet(1.567, -0.6, Eurobot2022Color.Vert, Eurobot2022TypeGobelet.Distributeur, Eurobot2022TeamReservation.ReservedYellow, 0));
                listElementsJeu.Add(28, new Eurobot2022Gobelet(1.567, -0.525, Eurobot2022Color.Rouge, Eurobot2022TypeGobelet.Distributeur, Eurobot2022TeamReservation.ReservedYellow, 0));
                listElementsJeu.Add(29, new Eurobot2022Gobelet(1.567, -0.45, Eurobot2022Color.Vert, Eurobot2022TypeGobelet.Distributeur, Eurobot2022TeamReservation.ReservedYellow, 0));

                /// Ecueil partagé coté jaune
                listElementsJeu.Add(30, new Eurobot2022Gobelet(0.8, 1.067, Eurobot2022Color.Rouge, Eurobot2022TypeGobelet.Distributeur, Eurobot2022TeamReservation.Shared, Math.PI / 2));
                listElementsJeu.Add(31, new Eurobot2022Gobelet(0.725, 1.067, Eurobot2022Color.Rouge, Eurobot2022TypeGobelet.Distributeur, Eurobot2022TeamReservation.Shared, Math.PI / 2));
                listElementsJeu.Add(32, new Eurobot2022Gobelet(0.65, 1.067, Eurobot2022Color.Rouge, Eurobot2022TypeGobelet.Distributeur, Eurobot2022TeamReservation.Shared, Math.PI / 2));
                listElementsJeu.Add(33, new Eurobot2022Gobelet(0.575, 1.067, Eurobot2022Color.Rouge, Eurobot2022TypeGobelet.Distributeur, Eurobot2022TeamReservation.Shared, Math.PI / 2));
                listElementsJeu.Add(34, new Eurobot2022Gobelet(0.5, 1.067, Eurobot2022Color.Vert, Eurobot2022TypeGobelet.Distributeur, Eurobot2022TeamReservation.Shared, Math.PI / 2));

                /// Ecueil partagé coté bleu
                listElementsJeu.Add(35, new Eurobot2022Gobelet(-0.5, 1.067, Eurobot2022Color.Rouge, Eurobot2022TypeGobelet.Distributeur, Eurobot2022TeamReservation.Shared, Math.PI / 2));
                listElementsJeu.Add(36, new Eurobot2022Gobelet(-0.575, 1.067, Eurobot2022Color.Vert, Eurobot2022TypeGobelet.Distributeur, Eurobot2022TeamReservation.Shared, Math.PI / 2));
                listElementsJeu.Add(37, new Eurobot2022Gobelet(-0.65, 1.067, Eurobot2022Color.Vert, Eurobot2022TypeGobelet.Distributeur, Eurobot2022TeamReservation.Shared, Math.PI / 2));
                listElementsJeu.Add(38, new Eurobot2022Gobelet(-0.725, 1.067, Eurobot2022Color.Vert, Eurobot2022TypeGobelet.Distributeur, Eurobot2022TeamReservation.Shared, Math.PI / 2));
                listElementsJeu.Add(39, new Eurobot2022Gobelet(-0.8, 1.067, Eurobot2022Color.Vert, Eurobot2022TypeGobelet.Distributeur, Eurobot2022TeamReservation.Shared, Math.PI / 2));

                /// Ecueil privé bleu
                listElementsJeu.Add(40, new Eurobot2022Gobelet(-1.567, -0.75, Eurobot2022Color.Rouge, Eurobot2022TypeGobelet.Distributeur, Eurobot2022TeamReservation.ReservedBlue, Math.PI));
                listElementsJeu.Add(41, new Eurobot2022Gobelet(-1.567, -0.675, Eurobot2022Color.Vert, Eurobot2022TypeGobelet.Distributeur, Eurobot2022TeamReservation.ReservedBlue, Math.PI));
                listElementsJeu.Add(42, new Eurobot2022Gobelet(-1.567, -0.6, Eurobot2022Color.Rouge, Eurobot2022TypeGobelet.Distributeur, Eurobot2022TeamReservation.ReservedBlue, Math.PI));
                listElementsJeu.Add(43, new Eurobot2022Gobelet(-1.567, -0.525, Eurobot2022Color.Vert, Eurobot2022TypeGobelet.Distributeur, Eurobot2022TeamReservation.ReservedBlue, Math.PI));
                listElementsJeu.Add(44, new Eurobot2022Gobelet(-1.567, -0.450, Eurobot2022Color.Rouge, Eurobot2022TypeGobelet.Distributeur, Eurobot2022TeamReservation.ReservedBlue, Math.PI));
            }
        }

        void FillEmplacementList()
        {
            DictionaryEmplacementDepose = new Dictionary<int, Eurobot2022EmplacementDepose>();

            ///Emplacement Grand Port Bleu
            DictionaryEmplacementDepose.Add(101, new Eurobot2022EmplacementDepose(-1.44, 0.48, Eurobot2022Color.Vert, Eurobot2022SideColor.Blue, -Math.PI,
                robotAttributionYellow: Eurobot2022RobotType.RobotSud, robotAttributionBlue: Eurobot2022RobotType.RobotNord, new List<int>(new int[] { })));
            DictionaryEmplacementDepose.Add(102, new Eurobot2022EmplacementDepose(-1.36, 0.48, Eurobot2022Color.Vert, Eurobot2022SideColor.Blue, -Math.PI,
                robotAttributionYellow: Eurobot2022RobotType.RobotSud, robotAttributionBlue: Eurobot2022RobotType.RobotNord, new List<int>(new int[] { 101 })));
            DictionaryEmplacementDepose.Add(103, new Eurobot2022EmplacementDepose(-1.28, 0.48, Eurobot2022Color.Vert, Eurobot2022SideColor.Blue, -Math.PI,
                robotAttributionYellow: Eurobot2022RobotType.RobotSud, robotAttributionBlue: Eurobot2022RobotType.RobotNord, new List<int>(new int[] { 102 })));
            DictionaryEmplacementDepose.Add(104, new Eurobot2022EmplacementDepose(-1.20, 0.48, Eurobot2022Color.Vert, Eurobot2022SideColor.Blue, -Math.PI,
                robotAttributionYellow: Eurobot2022RobotType.RobotSud, robotAttributionBlue: Eurobot2022RobotType.RobotNord, new List<int>(new int[] { 103 })));
            DictionaryEmplacementDepose.Add(105, new Eurobot2022EmplacementDepose(-1.12, 0.48, Eurobot2022Color.Vert, Eurobot2022SideColor.Blue, -Math.PI,
                robotAttributionYellow: Eurobot2022RobotType.RobotSud, robotAttributionBlue: Eurobot2022RobotType.RobotNord, new List<int>(new int[] { 104 })));

            DictionaryEmplacementDepose.Add(106, new Eurobot2022EmplacementDepose(-1.44, -0.085, Eurobot2022Color.Rouge, Eurobot2022SideColor.Blue, -Math.PI,
                robotAttributionYellow: Eurobot2022RobotType.RobotSud, robotAttributionBlue: Eurobot2022RobotType.RobotNord, new List<int>(new int[] { })));
            DictionaryEmplacementDepose.Add(107, new Eurobot2022EmplacementDepose(-1.36, -0.085, Eurobot2022Color.Rouge, Eurobot2022SideColor.Blue, -Math.PI,
                robotAttributionYellow: Eurobot2022RobotType.RobotSud, robotAttributionBlue: Eurobot2022RobotType.RobotNord, new List<int>(new int[] { 106 })));
            DictionaryEmplacementDepose.Add(108, new Eurobot2022EmplacementDepose(-1.28, -0.085, Eurobot2022Color.Rouge, Eurobot2022SideColor.Blue, -Math.PI,
                robotAttributionYellow: Eurobot2022RobotType.RobotSud, robotAttributionBlue: Eurobot2022RobotType.RobotNord, new List<int>(new int[] { 107 })));
            DictionaryEmplacementDepose.Add(109, new Eurobot2022EmplacementDepose(-1.20, -0.085, Eurobot2022Color.Rouge, Eurobot2022SideColor.Blue, -Math.PI,
                robotAttributionYellow: Eurobot2022RobotType.RobotSud, robotAttributionBlue: Eurobot2022RobotType.RobotNord, new List<int>(new int[] { 108 })));
            DictionaryEmplacementDepose.Add(110, new Eurobot2022EmplacementDepose(-1.12, -0.085, Eurobot2022Color.Rouge, Eurobot2022SideColor.Blue, -Math.PI,
                robotAttributionYellow: Eurobot2022RobotType.RobotSud, robotAttributionBlue: Eurobot2022RobotType.RobotNord, new List<int>(new int[] { 109 })));

            DictionaryEmplacementDepose.Add(127, new Eurobot2022EmplacementDepose(-1.44, 0.155, Eurobot2022Color.Neutre, Eurobot2022SideColor.Blue, -Math.PI,
                robotAttributionYellow: Eurobot2022RobotType.RobotSud, robotAttributionBlue: Eurobot2022RobotType.RobotNord, new List<int>(new int[] { })));
            DictionaryEmplacementDepose.Add(128, new Eurobot2022EmplacementDepose(-1.36, 0.155, Eurobot2022Color.Neutre, Eurobot2022SideColor.Blue, -Math.PI,
                robotAttributionYellow: Eurobot2022RobotType.RobotSud, robotAttributionBlue: Eurobot2022RobotType.RobotNord, new List<int>(new int[] { 127, 132 })));
            DictionaryEmplacementDepose.Add(129, new Eurobot2022EmplacementDepose(-1.28, 0.155, Eurobot2022Color.Neutre, Eurobot2022SideColor.Blue, -Math.PI,
                robotAttributionYellow: Eurobot2022RobotType.RobotSud, robotAttributionBlue: Eurobot2022RobotType.RobotNord, new List<int>(new int[] { 128, 133 })));
            DictionaryEmplacementDepose.Add(130, new Eurobot2022EmplacementDepose(-1.20, 0.155, Eurobot2022Color.Neutre, Eurobot2022SideColor.Blue, -Math.PI,
                robotAttributionYellow: Eurobot2022RobotType.RobotSud, robotAttributionBlue: Eurobot2022RobotType.RobotNord, new List<int>(new int[] { 129, 134 })));
            DictionaryEmplacementDepose.Add(131, new Eurobot2022EmplacementDepose(-1.12, 0.155, Eurobot2022Color.Neutre, Eurobot2022SideColor.Blue, -Math.PI,
                robotAttributionYellow: Eurobot2022RobotType.RobotSud, robotAttributionBlue: Eurobot2022RobotType.RobotNord, new List<int>(new int[] { 130, 135 })));

            DictionaryEmplacementDepose.Add(132, new Eurobot2022EmplacementDepose(-1.44, 0.24, Eurobot2022Color.Neutre, Eurobot2022SideColor.Blue, -Math.PI,
                robotAttributionYellow: Eurobot2022RobotType.RobotSud, robotAttributionBlue: Eurobot2022RobotType.RobotNord, new List<int>(new int[] { })));
            DictionaryEmplacementDepose.Add(133, new Eurobot2022EmplacementDepose(-1.36, 0.24, Eurobot2022Color.Neutre, Eurobot2022SideColor.Blue, -Math.PI,
                robotAttributionYellow: Eurobot2022RobotType.RobotSud, robotAttributionBlue: Eurobot2022RobotType.RobotNord, new List<int>(new int[] { 127, 132 })));
            DictionaryEmplacementDepose.Add(134, new Eurobot2022EmplacementDepose(-1.28, 0.24, Eurobot2022Color.Neutre, Eurobot2022SideColor.Blue, -Math.PI,
                robotAttributionYellow: Eurobot2022RobotType.RobotSud, robotAttributionBlue: Eurobot2022RobotType.RobotNord, new List<int>(new int[] { 128, 133 })));
            DictionaryEmplacementDepose.Add(135, new Eurobot2022EmplacementDepose(-1.20, 0.24, Eurobot2022Color.Neutre, Eurobot2022SideColor.Blue, -Math.PI,
                robotAttributionYellow: Eurobot2022RobotType.RobotSud, robotAttributionBlue: Eurobot2022RobotType.RobotNord, new List<int>(new int[] { 129, 134 })));
            DictionaryEmplacementDepose.Add(136, new Eurobot2022EmplacementDepose(-1.12, 0.24, Eurobot2022Color.Neutre, Eurobot2022SideColor.Blue, -Math.PI,
                robotAttributionYellow: Eurobot2022RobotType.RobotSud, robotAttributionBlue: Eurobot2022RobotType.RobotNord, new List<int>(new int[] { 130, 135 })));

            ///Emplacement Grand Port Jaune
            DictionaryEmplacementDepose.Add(1, new Eurobot2022EmplacementDepose(1.44, 0.48, Eurobot2022Color.Rouge, Eurobot2022SideColor.Yellow, 0,
                robotAttributionYellow: Eurobot2022RobotType.RobotNord, robotAttributionBlue: Eurobot2022RobotType.RobotSud, new List<int>(new int[] { })));
            DictionaryEmplacementDepose.Add(2, new Eurobot2022EmplacementDepose(1.36, 0.48, Eurobot2022Color.Rouge, Eurobot2022SideColor.Yellow, 0,
                robotAttributionYellow: Eurobot2022RobotType.RobotNord, robotAttributionBlue: Eurobot2022RobotType.RobotSud, new List<int>(new int[] { 1 })));
            DictionaryEmplacementDepose.Add(3, new Eurobot2022EmplacementDepose(1.28, 0.48, Eurobot2022Color.Rouge, Eurobot2022SideColor.Yellow, 0,
                robotAttributionYellow: Eurobot2022RobotType.RobotNord, robotAttributionBlue: Eurobot2022RobotType.RobotSud, new List<int>(new int[] { 2 })));
            DictionaryEmplacementDepose.Add(4, new Eurobot2022EmplacementDepose(1.20, 0.48, Eurobot2022Color.Rouge, Eurobot2022SideColor.Yellow, 0,
                robotAttributionYellow: Eurobot2022RobotType.RobotNord, robotAttributionBlue: Eurobot2022RobotType.RobotSud, new List<int>(new int[] { 3 })));
            DictionaryEmplacementDepose.Add(5, new Eurobot2022EmplacementDepose(1.12, 0.48, Eurobot2022Color.Rouge, Eurobot2022SideColor.Yellow, 0,
                robotAttributionYellow: Eurobot2022RobotType.RobotNord, robotAttributionBlue: Eurobot2022RobotType.RobotSud, new List<int>(new int[] { 4 })));

            DictionaryEmplacementDepose.Add(6, new Eurobot2022EmplacementDepose(1.44, -0.085, Eurobot2022Color.Vert, Eurobot2022SideColor.Yellow, 0,
                robotAttributionYellow: Eurobot2022RobotType.RobotNord, robotAttributionBlue: Eurobot2022RobotType.RobotSud, new List<int>(new int[] { })));
            DictionaryEmplacementDepose.Add(7, new Eurobot2022EmplacementDepose(1.36, -0.085, Eurobot2022Color.Vert, Eurobot2022SideColor.Yellow, 0,
                robotAttributionYellow: Eurobot2022RobotType.RobotNord, robotAttributionBlue: Eurobot2022RobotType.RobotSud, new List<int>(new int[] { 6 })));
            DictionaryEmplacementDepose.Add(8, new Eurobot2022EmplacementDepose(1.28, -0.085, Eurobot2022Color.Vert, Eurobot2022SideColor.Yellow, 0,
                robotAttributionYellow: Eurobot2022RobotType.RobotNord, robotAttributionBlue: Eurobot2022RobotType.RobotSud, new List<int>(new int[] { 7 })));
            DictionaryEmplacementDepose.Add(9, new Eurobot2022EmplacementDepose(1.20, -0.085, Eurobot2022Color.Vert, Eurobot2022SideColor.Yellow, 0,
                robotAttributionYellow: Eurobot2022RobotType.RobotNord, robotAttributionBlue: Eurobot2022RobotType.RobotSud, new List<int>(new int[] { 8 })));
            DictionaryEmplacementDepose.Add(10, new Eurobot2022EmplacementDepose(1.12, -0.085, Eurobot2022Color.Vert, Eurobot2022SideColor.Yellow, 0,
                robotAttributionYellow: Eurobot2022RobotType.RobotNord, robotAttributionBlue: Eurobot2022RobotType.RobotSud, new List<int>(new int[] { 9 })));

            DictionaryEmplacementDepose.Add(27, new Eurobot2022EmplacementDepose(1.44, 0.155, Eurobot2022Color.Neutre, Eurobot2022SideColor.Yellow, 0,
                robotAttributionYellow: Eurobot2022RobotType.RobotNord, robotAttributionBlue: Eurobot2022RobotType.RobotSud, new List<int>(new int[] { })));
            DictionaryEmplacementDepose.Add(28, new Eurobot2022EmplacementDepose(1.36, 0.155, Eurobot2022Color.Neutre, Eurobot2022SideColor.Yellow, 0,
                robotAttributionYellow: Eurobot2022RobotType.RobotNord, robotAttributionBlue: Eurobot2022RobotType.RobotSud, new List<int>(new int[] { 27, 32 })));
            DictionaryEmplacementDepose.Add(29, new Eurobot2022EmplacementDepose(1.28, 0.155, Eurobot2022Color.Neutre, Eurobot2022SideColor.Yellow, 0,
                robotAttributionYellow: Eurobot2022RobotType.RobotNord, robotAttributionBlue: Eurobot2022RobotType.RobotSud, new List<int>(new int[] { 28, 33 })));
            DictionaryEmplacementDepose.Add(30, new Eurobot2022EmplacementDepose(1.20, 0.155, Eurobot2022Color.Neutre, Eurobot2022SideColor.Yellow, 0,
                robotAttributionYellow: Eurobot2022RobotType.RobotNord, robotAttributionBlue: Eurobot2022RobotType.RobotSud, new List<int>(new int[] { 29, 34 })));
            DictionaryEmplacementDepose.Add(31, new Eurobot2022EmplacementDepose(1.12, 0.155, Eurobot2022Color.Neutre, Eurobot2022SideColor.Yellow, 0,
                robotAttributionYellow: Eurobot2022RobotType.RobotNord, robotAttributionBlue: Eurobot2022RobotType.RobotSud, new List<int>(new int[] { 30, 35 })));

            DictionaryEmplacementDepose.Add(32, new Eurobot2022EmplacementDepose(1.44, 0.24, Eurobot2022Color.Neutre, Eurobot2022SideColor.Yellow, 0,
                robotAttributionYellow: Eurobot2022RobotType.RobotNord, robotAttributionBlue: Eurobot2022RobotType.RobotSud, new List<int>(new int[] { })));
            DictionaryEmplacementDepose.Add(33, new Eurobot2022EmplacementDepose(1.36, 0.24, Eurobot2022Color.Neutre, Eurobot2022SideColor.Yellow, 0,
                robotAttributionYellow: Eurobot2022RobotType.RobotNord, robotAttributionBlue: Eurobot2022RobotType.RobotSud, new List<int>(new int[] { 27, 32 })));
            DictionaryEmplacementDepose.Add(34, new Eurobot2022EmplacementDepose(1.28, 0.24, Eurobot2022Color.Neutre, Eurobot2022SideColor.Yellow, 0,
                robotAttributionYellow: Eurobot2022RobotType.RobotNord, robotAttributionBlue: Eurobot2022RobotType.RobotSud, new List<int>(new int[] { 28, 33 })));
            DictionaryEmplacementDepose.Add(35, new Eurobot2022EmplacementDepose(1.20, 0.24, Eurobot2022Color.Neutre, Eurobot2022SideColor.Yellow, 0,
                robotAttributionYellow: Eurobot2022RobotType.RobotNord, robotAttributionBlue: Eurobot2022RobotType.RobotSud, new List<int>(new int[] { 29, 34 })));
            DictionaryEmplacementDepose.Add(36, new Eurobot2022EmplacementDepose(1.12, 0.24, Eurobot2022Color.Neutre, Eurobot2022SideColor.Yellow, 0,
                robotAttributionYellow: Eurobot2022RobotType.RobotNord, robotAttributionBlue: Eurobot2022RobotType.RobotSud, new List<int>(new int[] { 30, 35 })));

            //Emplacement Petit Port Jaune
            DictionaryEmplacementDepose.Add(23, new Eurobot2022EmplacementDepose(-0.445, -0.695, Eurobot2022Color.Vert, Eurobot2022SideColor.Yellow, -Math.PI / 2,
                robotAttributionYellow: Eurobot2022RobotType.RobotSud, robotAttributionBlue: Eurobot2022RobotType.RobotNord, new List<int>(new int[] { 24 })));
            DictionaryEmplacementDepose.Add(19, new Eurobot2022EmplacementDepose(-0.445, -0.780, Eurobot2022Color.Vert, Eurobot2022SideColor.Yellow, -Math.PI / 2,
                robotAttributionYellow: Eurobot2022RobotType.RobotSud, robotAttributionBlue: Eurobot2022RobotType.RobotNord, new List<int>(new int[] { 20 })));
            DictionaryEmplacementDepose.Add(15, new Eurobot2022EmplacementDepose(-0.445, -0.870, Eurobot2022Color.Vert, Eurobot2022SideColor.Yellow, -Math.PI / 2,
                robotAttributionYellow: Eurobot2022RobotType.RobotSud, robotAttributionBlue: Eurobot2022RobotType.RobotNord, new List<int>(new int[] { 16 })));
            DictionaryEmplacementDepose.Add(11, new Eurobot2022EmplacementDepose(-0.445, -0.950, Eurobot2022Color.Vert, Eurobot2022SideColor.Yellow, -Math.PI / 2,
                robotAttributionYellow: Eurobot2022RobotType.RobotSud, robotAttributionBlue: Eurobot2022RobotType.RobotNord, new List<int>(new int[] { 12 })));

            DictionaryEmplacementDepose.Add(24, new Eurobot2022EmplacementDepose(-0.360, -0.695, Eurobot2022Color.Vert, Eurobot2022SideColor.Yellow, -Math.PI / 2,
                robotAttributionYellow: Eurobot2022RobotType.RobotSud, robotAttributionBlue: Eurobot2022RobotType.RobotNord, new List<int>(new int[] { 19 })));
            DictionaryEmplacementDepose.Add(20, new Eurobot2022EmplacementDepose(-0.360, -0.780, Eurobot2022Color.Vert, Eurobot2022SideColor.Yellow, -Math.PI / 2,
                robotAttributionYellow: Eurobot2022RobotType.RobotSud, robotAttributionBlue: Eurobot2022RobotType.RobotNord, new List<int>(new int[] { 15 })));
            DictionaryEmplacementDepose.Add(16, new Eurobot2022EmplacementDepose(-0.360, -0.870, Eurobot2022Color.Vert, Eurobot2022SideColor.Yellow, -Math.PI / 2,
                robotAttributionYellow: Eurobot2022RobotType.RobotSud, robotAttributionBlue: Eurobot2022RobotType.RobotNord, new List<int>(new int[] { 11 })));
            DictionaryEmplacementDepose.Add(12, new Eurobot2022EmplacementDepose(-0.360, -0.950, Eurobot2022Color.Vert, Eurobot2022SideColor.Yellow, -Math.PI / 2,
                robotAttributionYellow: Eurobot2022RobotType.RobotSud, robotAttributionBlue: Eurobot2022RobotType.RobotNord, new List<int>(new int[] { })));  //Premier Vert

            DictionaryEmplacementDepose.Add(25, new Eurobot2022EmplacementDepose(-0.250, -0.695, Eurobot2022Color.Rouge, Eurobot2022SideColor.Yellow, -Math.PI / 2,
                robotAttributionYellow: Eurobot2022RobotType.RobotSud, robotAttributionBlue: Eurobot2022RobotType.RobotNord, new List<int>(new int[] { 26 })));
            DictionaryEmplacementDepose.Add(21, new Eurobot2022EmplacementDepose(-0.250, -0.780, Eurobot2022Color.Rouge, Eurobot2022SideColor.Yellow, -Math.PI / 2,
                robotAttributionYellow: Eurobot2022RobotType.RobotSud, robotAttributionBlue: Eurobot2022RobotType.RobotNord, new List<int>(new int[] { 22 })));
            DictionaryEmplacementDepose.Add(17, new Eurobot2022EmplacementDepose(-0.250, -0.870, Eurobot2022Color.Rouge, Eurobot2022SideColor.Yellow, -Math.PI / 2,
                robotAttributionYellow: Eurobot2022RobotType.RobotSud, robotAttributionBlue: Eurobot2022RobotType.RobotNord, new List<int>(new int[] { 18 })));
            DictionaryEmplacementDepose.Add(13, new Eurobot2022EmplacementDepose(-0.250, -0.950, Eurobot2022Color.Rouge, Eurobot2022SideColor.Yellow, -Math.PI / 2,
                robotAttributionYellow: Eurobot2022RobotType.RobotSud, robotAttributionBlue: Eurobot2022RobotType.RobotNord, new List<int>(new int[] { 14 })));

            DictionaryEmplacementDepose.Add(26, new Eurobot2022EmplacementDepose(-0.155, -0.695, Eurobot2022Color.Rouge, Eurobot2022SideColor.Yellow, -Math.PI / 2,
                robotAttributionYellow: Eurobot2022RobotType.RobotSud, robotAttributionBlue: Eurobot2022RobotType.RobotNord, new List<int>(new int[] { 21 })));
            DictionaryEmplacementDepose.Add(22, new Eurobot2022EmplacementDepose(-0.155, -0.780, Eurobot2022Color.Rouge, Eurobot2022SideColor.Yellow, -Math.PI / 2,
                robotAttributionYellow: Eurobot2022RobotType.RobotSud, robotAttributionBlue: Eurobot2022RobotType.RobotNord, new List<int>(new int[] { 17 })));
            DictionaryEmplacementDepose.Add(18, new Eurobot2022EmplacementDepose(-0.155, -0.870, Eurobot2022Color.Rouge, Eurobot2022SideColor.Yellow, -Math.PI / 2,
                robotAttributionYellow: Eurobot2022RobotType.RobotSud, robotAttributionBlue: Eurobot2022RobotType.RobotNord, new List<int>(new int[] { 13 })));
            DictionaryEmplacementDepose.Add(14, new Eurobot2022EmplacementDepose(-0.155, -0.950, Eurobot2022Color.Rouge, Eurobot2022SideColor.Yellow, -Math.PI / 2,
                robotAttributionYellow: Eurobot2022RobotType.RobotSud, robotAttributionBlue: Eurobot2022RobotType.RobotNord, new List<int>(new int[] { })));

            //Emplacement Petit Port Bleu
            DictionaryEmplacementDepose.Add(123, new Eurobot2022EmplacementDepose(0.445, -0.695, Eurobot2022Color.Rouge, Eurobot2022SideColor.Blue, -Math.PI / 2,
                robotAttributionYellow: Eurobot2022RobotType.RobotNord, robotAttributionBlue: Eurobot2022RobotType.RobotSud, new List<int>(new int[] { 124 })));
            DictionaryEmplacementDepose.Add(119, new Eurobot2022EmplacementDepose(0.445, -0.780, Eurobot2022Color.Rouge, Eurobot2022SideColor.Blue, -Math.PI / 2,
                robotAttributionYellow: Eurobot2022RobotType.RobotNord, robotAttributionBlue: Eurobot2022RobotType.RobotSud, new List<int>(new int[] { 120 })));
            DictionaryEmplacementDepose.Add(115, new Eurobot2022EmplacementDepose(0.445, -0.870, Eurobot2022Color.Rouge, Eurobot2022SideColor.Blue, -Math.PI / 2,
                robotAttributionYellow: Eurobot2022RobotType.RobotNord, robotAttributionBlue: Eurobot2022RobotType.RobotSud, new List<int>(new int[] { 116 })));
            DictionaryEmplacementDepose.Add(111, new Eurobot2022EmplacementDepose(0.445, -0.950, Eurobot2022Color.Rouge, Eurobot2022SideColor.Blue, -Math.PI / 2,
                robotAttributionYellow: Eurobot2022RobotType.RobotNord, robotAttributionBlue: Eurobot2022RobotType.RobotSud, new List<int>(new int[] { 112 })));

            DictionaryEmplacementDepose.Add(124, new Eurobot2022EmplacementDepose(0.360, -0.695, Eurobot2022Color.Rouge, Eurobot2022SideColor.Blue, -Math.PI / 2,
                robotAttributionYellow: Eurobot2022RobotType.RobotNord, robotAttributionBlue: Eurobot2022RobotType.RobotSud, new List<int>(new int[] { 119 })));
            DictionaryEmplacementDepose.Add(120, new Eurobot2022EmplacementDepose(0.360, -0.780, Eurobot2022Color.Rouge, Eurobot2022SideColor.Blue, -Math.PI / 2,
                robotAttributionYellow: Eurobot2022RobotType.RobotNord, robotAttributionBlue: Eurobot2022RobotType.RobotSud, new List<int>(new int[] { 115 })));
            DictionaryEmplacementDepose.Add(116, new Eurobot2022EmplacementDepose(0.360, -0.870, Eurobot2022Color.Rouge, Eurobot2022SideColor.Blue, -Math.PI / 2,
                robotAttributionYellow: Eurobot2022RobotType.RobotNord, robotAttributionBlue: Eurobot2022RobotType.RobotSud, new List<int>(new int[] { 111 })));
            DictionaryEmplacementDepose.Add(112, new Eurobot2022EmplacementDepose(0.360, -0.950, Eurobot2022Color.Rouge, Eurobot2022SideColor.Blue, -Math.PI / 2,
                robotAttributionYellow: Eurobot2022RobotType.RobotNord, robotAttributionBlue: Eurobot2022RobotType.RobotSud, new List<int>(new int[] { })));

            DictionaryEmplacementDepose.Add(125, new Eurobot2022EmplacementDepose(0.250, -0.695, Eurobot2022Color.Vert, Eurobot2022SideColor.Blue, -Math.PI / 2,
                robotAttributionYellow: Eurobot2022RobotType.RobotNord, robotAttributionBlue: Eurobot2022RobotType.RobotSud, new List<int>(new int[] { 126 })));
            DictionaryEmplacementDepose.Add(121, new Eurobot2022EmplacementDepose(0.250, -0.780, Eurobot2022Color.Vert, Eurobot2022SideColor.Blue, -Math.PI / 2,
                robotAttributionYellow: Eurobot2022RobotType.RobotNord, robotAttributionBlue: Eurobot2022RobotType.RobotSud, new List<int>(new int[] { 122 })));
            DictionaryEmplacementDepose.Add(117, new Eurobot2022EmplacementDepose(0.250, -0.870, Eurobot2022Color.Vert, Eurobot2022SideColor.Blue, -Math.PI / 2,
                robotAttributionYellow: Eurobot2022RobotType.RobotNord, robotAttributionBlue: Eurobot2022RobotType.RobotSud, new List<int>(new int[] { 118 })));
            DictionaryEmplacementDepose.Add(113, new Eurobot2022EmplacementDepose(0.250, -0.950, Eurobot2022Color.Vert, Eurobot2022SideColor.Blue, -Math.PI / 2,
                robotAttributionYellow: Eurobot2022RobotType.RobotNord, robotAttributionBlue: Eurobot2022RobotType.RobotSud, new List<int>(new int[] { 114 })));

            DictionaryEmplacementDepose.Add(126, new Eurobot2022EmplacementDepose(0.155, -0.695, Eurobot2022Color.Vert, Eurobot2022SideColor.Blue, -Math.PI / 2,
                robotAttributionYellow: Eurobot2022RobotType.RobotNord, robotAttributionBlue: Eurobot2022RobotType.RobotSud, new List<int>(new int[] { 121 })));
            DictionaryEmplacementDepose.Add(122, new Eurobot2022EmplacementDepose(0.155, -0.780, Eurobot2022Color.Vert, Eurobot2022SideColor.Blue, -Math.PI / 2,
                robotAttributionYellow: Eurobot2022RobotType.RobotNord, robotAttributionBlue: Eurobot2022RobotType.RobotSud, new List<int>(new int[] { 117 })));
            DictionaryEmplacementDepose.Add(118, new Eurobot2022EmplacementDepose(0.155, -0.870, Eurobot2022Color.Vert, Eurobot2022SideColor.Blue, -Math.PI / 2,
                robotAttributionYellow: Eurobot2022RobotType.RobotNord, robotAttributionBlue: Eurobot2022RobotType.RobotSud, new List<int>(new int[] { 113 })));
            DictionaryEmplacementDepose.Add(114, new Eurobot2022EmplacementDepose(0.155, -0.950, Eurobot2022Color.Vert, Eurobot2022SideColor.Blue, -Math.PI / 2,
                robotAttributionYellow: Eurobot2022RobotType.RobotNord, robotAttributionBlue: Eurobot2022RobotType.RobotSud, new List<int>(new int[] { })));

        }

        void FillBrasTurbineDictionary()
        {
            DictionaryBrasTurbine = new Dictionary<string, Eurobot2022StateBrasTurbineRobot>();

            DictionaryBrasTurbine.Add("Bras_0", new Eurobot2022StateBrasTurbineRobot(0));
            DictionaryBrasTurbine.Add("Bras_45", new Eurobot2022StateBrasTurbineRobot(Math.PI / 4));
            DictionaryBrasTurbine.Add("Bras_135", new Eurobot2022StateBrasTurbineRobot(3 * Math.PI / 4));
            DictionaryBrasTurbine.Add("Bras_180", new Eurobot2022StateBrasTurbineRobot(4 * Math.PI / 4));
            DictionaryBrasTurbine.Add("Bras_315", new Eurobot2022StateBrasTurbineRobot(7 * Math.PI / 4));
        }

        public void SetCompteurDeposeAEffectuer(int value)
        {
            CompteurDeposeAEffectuer = value;
            Console.WriteLine(" Depose à effectuer mises à jour : reste " + CompteurDeposeAEffectuer + " déposes");
        }
        public void ForçageCompteurDeposeAEffectuer(int value)
        {
            CompteurDeposeAEffectuer = value;
            Console.WriteLine(" Forcage nb Deposes à effectuer : reste " + CompteurDeposeAEffectuer + " déposes");
        }
        public void DecrementeCompteurDeposeAEffectuer()
        {
            Console.WriteLine(" Depose effectuée : reste " + CompteurDeposeAEffectuer + " déposes");
            CompteurDeposeAEffectuer--;
        }
        public void DecrementeCompteurGobelets()
        {
            Console.WriteLine(" Depose effectuée : reste " + CompteurGobeletsSurBras + " gobelets");
            CompteurGobeletsSurBras--;
        }

        public void IncrementeGobeletSurBras(bool AutoRequestDepose = true)
        {
            CompteurGobeletsSurBras++;
            if (CompteurGobeletsSurBras >= 5 && AutoRequestDepose)
            {
                CompteurDeposeAEffectuer = 5;
            }
            Console.WriteLine(" Nombre de gobelets sur bras : " + CompteurGobeletsSurBras + " - Déposes à effectuer : " + CompteurDeposeAEffectuer);
        }

        //public void RequestDepose(int nbDeposes)
        //{
        //    CompteurDeposeAEffectuer = nbDeposes;
        //}
        public void ConfirmDepose()
        {
            CompteurGobeletsSurBras--;
            CompteurDeposeAEffectuer--;
        }
    }


    public enum Eurobot2022SideColor
    {
        Blue,
        Yellow
    };

    public enum Eurobot2022RobotType
    {
        RobotSud,
        RobotNord,
        None
    };

    public enum Eurobot2022StrategyType
    {
        Soft,
        LaConchaDeSuMadre
    };

    public enum Eurobot2022Color
    {
        Vert,
        Rouge,
        Neutre
    }

    public enum Eurobot2022TypeGobelet
    {
        Libre,
        Distributeur
    }

    public enum Eurobot2022TeamReservation
    {
        Shared,
        ReservedBlue,
        ReservedYellow,
    }

    public enum Eurobot2022TypeELementDeJeu
    {
        Gobelet,
        Phare,
        MancheAir
    }


    public abstract class Eurobot2022ElementDeJeu
    {
        public PointD Pos;
        public double? AnglePrise;
        public double PriorityBlue = 1;
        public double PriorityYellow = 1;
        public Eurobot2022RobotType RobotAttributionYellow;
        public Eurobot2022RobotType RobotAttributionBlue;
        public Eurobot2022TeamReservation ReservationToTeam = Eurobot2022TeamReservation.Shared;
        public Eurobot2022TypeELementDeJeu elementDeJeu = Eurobot2022TypeELementDeJeu.Gobelet;
        public bool isAvailable;
    }

    public class Eurobot2022Gobelet : Eurobot2022ElementDeJeu
    {
        public Eurobot2022Color Color;
        public Eurobot2022TypeGobelet Type;
        public int? NbDeposeToTrigger = null;

        public Eurobot2022Gobelet(double x, double y, Eurobot2022Color color, Eurobot2022TypeGobelet type,
            Eurobot2022TeamReservation reserved, double? anglePrise,
            Eurobot2022RobotType robotAttributionBlue = Eurobot2022RobotType.None, Eurobot2022RobotType robotAttributionYellow = Eurobot2022RobotType.None,
            double priorityBlue = 1.0, double priorityYellow = 1.0, int? nbDeposeToTrigger = null)
        {
            Pos = new PointD(x, y);
            Color = color;
            Type = type;
            isAvailable = true;
            ReservationToTeam = reserved;
            AnglePrise = anglePrise;
            RobotAttributionYellow = robotAttributionYellow;
            RobotAttributionBlue = robotAttributionBlue;
            PriorityBlue = priorityBlue;
            PriorityYellow = priorityYellow;
            NbDeposeToTrigger = nbDeposeToTrigger;
        }
    }

    public class Eurobot2022MancheAir : Eurobot2022ElementDeJeu
    {
        public Eurobot2022MancheAir(Eurobot2022TypeELementDeJeu mancheAir, double x, double y, Eurobot2022TeamReservation reserved, double? anglePrise,
            Eurobot2022RobotType robotAttributionBlue = Eurobot2022RobotType.None, Eurobot2022RobotType robotAttributionYellow = Eurobot2022RobotType.None,
            double priorityBlue = 1.0, double priorityYellow = 1.0)
        {
            elementDeJeu = mancheAir;
            Pos = new PointD(x, y);
            isAvailable = true;
            ReservationToTeam = reserved;
            AnglePrise = anglePrise;
            RobotAttributionYellow = robotAttributionYellow;
            RobotAttributionBlue = robotAttributionBlue;
            PriorityBlue = priorityBlue;
            PriorityYellow = priorityYellow;
        }
    }

    public class Eurobot2022Phare : Eurobot2022ElementDeJeu
    {
        public Eurobot2022Phare(Eurobot2022TypeELementDeJeu phare, double x, double y, Eurobot2022TeamReservation reserved, double? anglePrise,
            Eurobot2022RobotType robotAttributionBlue = Eurobot2022RobotType.None, Eurobot2022RobotType robotAttributionYellow = Eurobot2022RobotType.None,
            double priorityBlue = 1.0, double priorityYellow = 1.0)
        {
            elementDeJeu = phare;
            Pos = new PointD(x, y);
            isAvailable = true;
            ReservationToTeam = reserved;
            AnglePrise = anglePrise;
            RobotAttributionYellow = robotAttributionYellow;
            RobotAttributionBlue = robotAttributionBlue;
            PriorityBlue = priorityBlue;
            PriorityYellow = priorityYellow;
        }
    }

    public class Eurobot2022EmplacementDepose
    {
        //public Dictionary<int, CaseDepose> positionsDeposeList = new Dictionary<int, CaseDepose>();
        //public int Id;
        public PointD Pos;
        public double AngleDepose;
        public Eurobot2022SideColor Eurobot2022SideColor;
        public Eurobot2022Color Color;
        public bool IsAvailable;
        public Eurobot2022RobotType RobotAttributionYellow;
        public Eurobot2022RobotType RobotAttributionBlue;
        public List<int> UnlockIdList = new List<int>();

        public Eurobot2022EmplacementDepose(double x, double y, Eurobot2022Color couleur, Eurobot2022SideColor team, double angleDepose,
            Eurobot2022RobotType robotAttributionYellow, Eurobot2022RobotType robotAttributionBlue, List<int> unlockList)
        {
            Pos = new PointD(x, y);
            Color = couleur;
            Eurobot2022SideColor = team;
            AngleDepose = angleDepose;
            UnlockIdList = unlockList;
            RobotAttributionBlue = robotAttributionBlue;
            RobotAttributionYellow = robotAttributionYellow;
            IsAvailable = true;
        }
    }

    public class Eurobot2022StateBrasTurbineRobot
    {
        public bool HasGobelet = false;
        public Eurobot2022Color GobletCapturedColor = Eurobot2022Color.Neutre;
        public double AngleBras;

        public Eurobot2022StateBrasTurbineRobot(double angleBras)
        {
            AngleBras = angleBras;
        }
    }

    class Eurobot2022CaseDepose
    {
        public PointD Pos;
        public bool isFull;

        public Eurobot2022CaseDepose(double x, double y)
        {
            Pos = new PointD(x, y);
            isFull = false;
        }
    }
}
