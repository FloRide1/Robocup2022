using Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;
using static StrategyManagerNS.StrategyEurobot2021;

namespace StrategyManagerNS
{
    public class Eurobot2021MatchDescriptor
    {
        public Dictionary<int, Eurobot2021ElementDeJeu> listElementsJeu = new Dictionary<int, Eurobot2021ElementDeJeu>();
        public Dictionary<int, Eurobot2021ElementDeJeu> listElementsJeuDynamiques = new Dictionary<int, Eurobot2021ElementDeJeu>();
        public Dictionary<int, Eurobot2021EmplacementDepose> DictionaryEmplacementDepose = new Dictionary<int, Eurobot2021EmplacementDepose>();
        public Dictionary<string, Eurobot2021StateBrasTurbineRobot> DictionaryBrasTurbine = new Dictionary<string, Eurobot2021StateBrasTurbineRobot>();
        int CompteurGobeletsSurBras = 0;
        public int CompteurDeposeAEffectuer { get; private set; }

        List<int> ListElementsJeuOrdonneeBlueRobotNord;
        List<int> ListElementsJeuOrdonneeBlueRobotSud;
        List<int> ListElementsJeuOrdonneeYellowRobotNord;
        List<int> ListElementsJeuOrdonneeYellowRobotSud;

        StrategyEurobot2021 parentStrategie;

        public Eurobot2021MatchDescriptor(StrategyGenerique sg)
        {
            parentStrategie = sg as StrategyEurobot2021;
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
                16, 14, 13, 15, 12, 105, 11, 35, 36, 37, 39, 102, 103, 40, 41, 42, 43, 44
            };
            ListElementsJeuOrdonneeYellowRobotNord = new List<int>()
            {
                2, 4, 3, 1, 5, 104, 6, 34, 33, 32, 30, 100, 101, 25, 26, 27, 28, 29
            };


            if (parentStrategie.strategyType == Eurobot2021StrategyType.LaConchaDeSuMadre)
            {
                ListElementsJeuOrdonneeBlueRobotSud = new List<int>()
                {
                    1001, 34, 30, 18, 19
                };
                ListElementsJeuOrdonneeYellowRobotSud = new List<int>()
                {
                    1000, 35, 39, 23, 22
                };
            }
            else
            {
                ListElementsJeuOrdonneeBlueRobotSud = new List<int>()
                {
                    6, 30, 34, 18, 19
                };
                ListElementsJeuOrdonneeYellowRobotSud = new List<int>()
                {
                    11, 39, 35, 23, 22
                };
            }
        }  

        void LoadStrategy()
        {
            for (int i = 0; i < ListElementsJeuOrdonneeBlueRobotNord.Count; i++)
            {
                if (listElementsJeu.ContainsKey(ListElementsJeuOrdonneeBlueRobotNord[i]))
                {
                    listElementsJeu[ListElementsJeuOrdonneeBlueRobotNord[i]].RobotAttributionBlue = Eurobot2021RobotType.RobotNord;
                    listElementsJeu[ListElementsJeuOrdonneeBlueRobotNord[i]].PriorityBlue = ListElementsJeuOrdonneeBlueRobotNord.Count - i;
                }
            }
            for (int i = 0; i < ListElementsJeuOrdonneeBlueRobotSud.Count; i++)
            {
                if (listElementsJeu.ContainsKey(ListElementsJeuOrdonneeBlueRobotSud[i]))
                {
                    listElementsJeu[ListElementsJeuOrdonneeBlueRobotSud[i]].RobotAttributionBlue = Eurobot2021RobotType.RobotSud;
                    listElementsJeu[ListElementsJeuOrdonneeBlueRobotSud[i]].PriorityBlue = ListElementsJeuOrdonneeBlueRobotSud.Count - i;
                }
            }
            for (int i = 0; i < ListElementsJeuOrdonneeYellowRobotNord.Count; i++)
            {
                if (listElementsJeu.ContainsKey(ListElementsJeuOrdonneeYellowRobotNord[i]))
                {
                    listElementsJeu[ListElementsJeuOrdonneeYellowRobotNord[i]].RobotAttributionYellow = Eurobot2021RobotType.RobotNord;
                    listElementsJeu[ListElementsJeuOrdonneeYellowRobotNord[i]].PriorityYellow = ListElementsJeuOrdonneeYellowRobotNord.Count - i;
                }
            }
            for (int i = 0; i < ListElementsJeuOrdonneeYellowRobotSud.Count; i++)
            {
                if (listElementsJeu.ContainsKey(ListElementsJeuOrdonneeYellowRobotSud[i]))
                {
                    listElementsJeu[ListElementsJeuOrdonneeYellowRobotSud[i]].RobotAttributionYellow = Eurobot2021RobotType.RobotSud;
                    listElementsJeu[ListElementsJeuOrdonneeYellowRobotSud[i]].PriorityYellow = ListElementsJeuOrdonneeYellowRobotSud.Count - i;
                }
            }
        }
        private void FillElementsdeJeuListQualif()
        {
            lock (listElementsJeu)
            {
                listElementsJeu = new Dictionary<int, Eurobot2021ElementDeJeu>();
                listElementsJeu.Add(1000, new PositionTerrain(-0.65, 0.70, null, TeamReservation.ReservedYellow));
                listElementsJeu.Add(1001, new PositionTerrain(0.65, 0.70, null, TeamReservation.ReservedBlue));

                /// Manche à air
                listElementsJeu.Add(100, new MancheAir(TypeELementDeJeu.MancheAir, 1.270, -1.00, TeamReservation.ReservedYellow, null));
                listElementsJeu.Add(101, new MancheAir(TypeELementDeJeu.MancheAir, 0.865, -1.00, TeamReservation.ReservedYellow, null));
                listElementsJeu.Add(102, new MancheAir(TypeELementDeJeu.MancheAir, -1.270, -1.00, TeamReservation.ReservedBlue, null));
                listElementsJeu.Add(103, new MancheAir(TypeELementDeJeu.MancheAir, -0.865, -1.00, TeamReservation.ReservedBlue, null));

                /// Phares
                listElementsJeu.Add(104, new Phare(TypeELementDeJeu.Phare, 1.19, 1.00, TeamReservation.ReservedYellow, null));
                listElementsJeu.Add(105, new Phare(TypeELementDeJeu.Phare, -1.19, 1.00, TeamReservation.ReservedBlue, null));

                /// Gobelets de champ
                listElementsJeu.Add(1, new Gobelet(1.200, -0.200, Color.Rouge, TypeGobelet.Libre, TeamReservation.ReservedYellow, null));
                listElementsJeu.Add(2, new Gobelet(1.200, 0.600, Color.Vert, TypeGobelet.Libre, TeamReservation.ReservedYellow, null));
                listElementsJeu.Add(3, new Gobelet(1.050, -0.080, Color.Vert, TypeGobelet.Libre, TeamReservation.ReservedYellow, null));
                listElementsJeu.Add(4, new Gobelet(1.050, 0.490, Color.Rouge, TypeGobelet.Libre, TeamReservation.ReservedYellow, null));
                listElementsJeu.Add(5, new Gobelet(0.830, 0.900, Color.Vert, TypeGobelet.Libre, TeamReservation.Shared, Math.PI / 2));
                listElementsJeu.Add(6, new Gobelet(0.550, 0.600, Color.Rouge, TypeGobelet.Libre, TeamReservation.Shared, null));
                listElementsJeu.Add(7, new Gobelet(0.400, 0.200, Color.Vert, TypeGobelet.Libre, TeamReservation.Shared, null));
                listElementsJeu.Add(8, new Gobelet(0.230, -0.200, Color.Rouge, TypeGobelet.Libre, TeamReservation.Shared, null));
                listElementsJeu.Add(9, new Gobelet(-0.230, -0.200, Color.Vert, TypeGobelet.Libre, TeamReservation.Shared, null));
                listElementsJeu.Add(10, new Gobelet(-0.400, 0.200, Color.Rouge, TypeGobelet.Libre, TeamReservation.Shared, null));
                listElementsJeu.Add(11, new Gobelet(-0.550, 0.600, Color.Vert, TypeGobelet.Libre, TeamReservation.Shared, null));
                listElementsJeu.Add(12, new Gobelet(-0.830, 0.900, Color.Rouge, TypeGobelet.Libre, TeamReservation.Shared, Math.PI / 2));
                listElementsJeu.Add(13, new Gobelet(-1.050, -0.080, Color.Rouge, TypeGobelet.Libre, TeamReservation.ReservedBlue, null));
                listElementsJeu.Add(14, new Gobelet(-1.050, 0.490, Color.Vert, TypeGobelet.Libre, TeamReservation.ReservedBlue, null));
                listElementsJeu.Add(15, new Gobelet(-1.200, -0.200, Color.Vert, TypeGobelet.Libre, TeamReservation.ReservedBlue, null));
                listElementsJeu.Add(16, new Gobelet(-1.200, 0.6, Color.Rouge, TypeGobelet.Libre, TeamReservation.ReservedBlue, null));

                /// Gobelets des petits ports
                listElementsJeu.Add(17, new Gobelet(0.495, -0.955, Color.Vert, TypeGobelet.Libre, TeamReservation.ReservedBlue, -Math.PI / 3));
                listElementsJeu.Add(18, new Gobelet(0.435, -0.65, Color.Rouge, TypeGobelet.Libre, TeamReservation.ReservedBlue, -Math.PI / 2));
                listElementsJeu.Add(19, new Gobelet(0.165, -0.65, Color.Vert, TypeGobelet.Libre, TeamReservation.ReservedBlue, -Math.PI / 2));
                listElementsJeu.Add(20, new Gobelet(0.105, -0.955, Color.Rouge, TypeGobelet.Libre, TeamReservation.ReservedBlue, -2 * Math.PI / 3));

                listElementsJeu.Add(21, new Gobelet(-0.105, -0.955, Color.Vert, TypeGobelet.Libre, TeamReservation.ReservedYellow, -Math.PI / 3));
                listElementsJeu.Add(22, new Gobelet(-0.165, -0.650, Color.Rouge, TypeGobelet.Libre, TeamReservation.ReservedYellow, -Math.PI / 2));
                listElementsJeu.Add(23, new Gobelet(-0.435, -0.650, Color.Vert, TypeGobelet.Libre, TeamReservation.ReservedYellow, -Math.PI / 2));
                listElementsJeu.Add(24, new Gobelet(-0.495, -0.955, Color.Rouge, TypeGobelet.Libre, TeamReservation.ReservedYellow, -2 * Math.PI / 3));

                // Ecueil privé jaune
                listElementsJeu.Add(25, new Gobelet(1.567, -0.750, Color.Vert, TypeGobelet.Distributeur, TeamReservation.ReservedYellow, 0));
                listElementsJeu.Add(26, new Gobelet(1.567, -0.675, Color.Rouge, TypeGobelet.Distributeur, TeamReservation.ReservedYellow, 0));
                listElementsJeu.Add(27, new Gobelet(1.567, -0.6, Color.Vert, TypeGobelet.Distributeur, TeamReservation.ReservedYellow, 0));
                listElementsJeu.Add(28, new Gobelet(1.567, -0.525, Color.Rouge, TypeGobelet.Distributeur, TeamReservation.ReservedYellow, 0));
                listElementsJeu.Add(29, new Gobelet(1.567, -0.45, Color.Vert, TypeGobelet.Distributeur, TeamReservation.ReservedYellow, 0));

                /// Ecueil partagé coté jaune
                listElementsJeu.Add(30, new Gobelet(0.8, 1.067, Color.Rouge, TypeGobelet.Distributeur, TeamReservation.Shared, Math.PI / 2));
                listElementsJeu.Add(31, new Gobelet(0.725, 1.067, Color.Neutre, TypeGobelet.Distributeur, TeamReservation.Shared, Math.PI / 2));
                listElementsJeu.Add(32, new Gobelet(0.65, 1.067, Color.Neutre, TypeGobelet.Distributeur, TeamReservation.Shared, Math.PI / 2));
                listElementsJeu.Add(33, new Gobelet(0.575, 1.067, Color.Neutre, TypeGobelet.Distributeur, TeamReservation.Shared, Math.PI / 2));
                listElementsJeu.Add(34, new Gobelet(0.5, 1.067, Color.Vert, TypeGobelet.Distributeur, TeamReservation.Shared, Math.PI / 2));

                /// Ecueil partagé coté bleu
                listElementsJeu.Add(35, new Gobelet(-0.5, 1.067, Color.Rouge, TypeGobelet.Distributeur, TeamReservation.Shared, Math.PI / 2));
                listElementsJeu.Add(36, new Gobelet(-0.575, 1.067, Color.Neutre, TypeGobelet.Distributeur, TeamReservation.Shared, Math.PI / 2));
                listElementsJeu.Add(37, new Gobelet(-0.65, 1.067, Color.Neutre, TypeGobelet.Distributeur, TeamReservation.Shared, Math.PI / 2));
                listElementsJeu.Add(38, new Gobelet(-0.725, 1.067, Color.Neutre, TypeGobelet.Distributeur, TeamReservation.Shared, Math.PI / 2));
                listElementsJeu.Add(39, new Gobelet(-0.8, 1.067, Color.Vert, TypeGobelet.Distributeur, TeamReservation.Shared, Math.PI / 2));

                /// Ecueil privé bleu
                listElementsJeu.Add(40, new Gobelet(-1.567, -0.75, Color.Rouge, TypeGobelet.Distributeur, TeamReservation.ReservedBlue, Math.PI));
                listElementsJeu.Add(41, new Gobelet(-1.567, -0.675, Color.Vert, TypeGobelet.Distributeur, TeamReservation.ReservedBlue, Math.PI));
                listElementsJeu.Add(42, new Gobelet(-1.567, -0.6, Color.Rouge, TypeGobelet.Distributeur, TeamReservation.ReservedBlue, Math.PI));
                listElementsJeu.Add(43, new Gobelet(-1.567, -0.525, Color.Vert, TypeGobelet.Distributeur, TeamReservation.ReservedBlue, Math.PI));
                listElementsJeu.Add(44, new Gobelet(-1.567, -0.450, Color.Rouge, TypeGobelet.Distributeur, TeamReservation.ReservedBlue, Math.PI));
            }


            void FillEmplacementList()
        {
            DictionaryEmplacementDepose = new Dictionary<int, Eurobot2021EmplacementDepose>();

            ///Emplacement Grand Port Bleu
            DictionaryEmplacementDepose.Add(101, new Eurobot2021EmplacementDepose(-1.44, 0.48, Eurobot2021Color.Vert, Eurobot2021SideColor.Blue, -Math.PI,
                robotAttributionYellow: Eurobot2021RobotType.RobotSud, robotAttributionBlue: Eurobot2021RobotType.RobotNord, new List<int>(new int[] { })));
            DictionaryEmplacementDepose.Add(102, new Eurobot2021EmplacementDepose(-1.36, 0.48, Eurobot2021Color.Vert, Eurobot2021SideColor.Blue, -Math.PI,
                robotAttributionYellow: Eurobot2021RobotType.RobotSud, robotAttributionBlue: Eurobot2021RobotType.RobotNord, new List<int>(new int[] { 101 })));
            DictionaryEmplacementDepose.Add(103, new Eurobot2021EmplacementDepose(-1.28, 0.48, Eurobot2021Color.Vert, Eurobot2021SideColor.Blue, -Math.PI,
                robotAttributionYellow: Eurobot2021RobotType.RobotSud, robotAttributionBlue: Eurobot2021RobotType.RobotNord, new List<int>(new int[] { 102 })));
            DictionaryEmplacementDepose.Add(104, new Eurobot2021EmplacementDepose(-1.20, 0.48, Eurobot2021Color.Vert, Eurobot2021SideColor.Blue, -Math.PI,
                robotAttributionYellow: Eurobot2021RobotType.RobotSud, robotAttributionBlue: Eurobot2021RobotType.RobotNord, new List<int>(new int[] { 103 })));
            DictionaryEmplacementDepose.Add(105, new Eurobot2021EmplacementDepose(-1.12, 0.48, Eurobot2021Color.Vert, Eurobot2021SideColor.Blue, -Math.PI,
                robotAttributionYellow: Eurobot2021RobotType.RobotSud, robotAttributionBlue: Eurobot2021RobotType.RobotNord, new List<int>(new int[] { 104 })));

            DictionaryEmplacementDepose.Add(106, new Eurobot2021EmplacementDepose(-1.44, -0.085, Eurobot2021Color.Rouge, Eurobot2021SideColor.Blue, -Math.PI,
                robotAttributionYellow: Eurobot2021RobotType.RobotSud, robotAttributionBlue: Eurobot2021RobotType.RobotNord, new List<int>(new int[] { })));
            DictionaryEmplacementDepose.Add(107, new Eurobot2021EmplacementDepose(-1.36, -0.085, Eurobot2021Color.Rouge, Eurobot2021SideColor.Blue, -Math.PI,
                robotAttributionYellow: Eurobot2021RobotType.RobotSud, robotAttributionBlue: Eurobot2021RobotType.RobotNord, new List<int>(new int[] { 106 })));
            DictionaryEmplacementDepose.Add(108, new Eurobot2021EmplacementDepose(-1.28, -0.085, Eurobot2021Color.Rouge, Eurobot2021SideColor.Blue, -Math.PI,
                robotAttributionYellow: Eurobot2021RobotType.RobotSud, robotAttributionBlue: Eurobot2021RobotType.RobotNord, new List<int>(new int[] { 107 })));
            DictionaryEmplacementDepose.Add(109, new Eurobot2021EmplacementDepose(-1.20, -0.085, Eurobot2021Color.Rouge, Eurobot2021SideColor.Blue, -Math.PI,
                robotAttributionYellow: Eurobot2021RobotType.RobotSud, robotAttributionBlue: Eurobot2021RobotType.RobotNord, new List<int>(new int[] { 108 })));
            DictionaryEmplacementDepose.Add(110, new Eurobot2021EmplacementDepose(-1.12, -0.085, Eurobot2021Color.Rouge, Eurobot2021SideColor.Blue, -Math.PI,
                robotAttributionYellow: Eurobot2021RobotType.RobotSud, robotAttributionBlue: Eurobot2021RobotType.RobotNord, new List<int>(new int[] { 109 })));

            DictionaryEmplacementDepose.Add(127, new Eurobot2021EmplacementDepose(-1.44, 0.155, Eurobot2021Color.Neutre, Eurobot2021SideColor.Blue, -Math.PI,
                robotAttributionYellow: Eurobot2021RobotType.RobotSud, robotAttributionBlue: Eurobot2021RobotType.RobotNord, new List<int>(new int[] { })));
            DictionaryEmplacementDepose.Add(128, new Eurobot2021EmplacementDepose(-1.36, 0.155, Eurobot2021Color.Neutre, Eurobot2021SideColor.Blue, -Math.PI,
                robotAttributionYellow: Eurobot2021RobotType.RobotSud, robotAttributionBlue: Eurobot2021RobotType.RobotNord, new List<int>(new int[] { 127, 132 })));
            DictionaryEmplacementDepose.Add(129, new Eurobot2021EmplacementDepose(-1.28, 0.155, Eurobot2021Color.Neutre, Eurobot2021SideColor.Blue, -Math.PI,
                robotAttributionYellow: Eurobot2021RobotType.RobotSud, robotAttributionBlue: Eurobot2021RobotType.RobotNord, new List<int>(new int[] { 128, 133 })));
            DictionaryEmplacementDepose.Add(130, new Eurobot2021EmplacementDepose(-1.20, 0.155, Eurobot2021Color.Neutre, Eurobot2021SideColor.Blue, -Math.PI,
                robotAttributionYellow: Eurobot2021RobotType.RobotSud, robotAttributionBlue: Eurobot2021RobotType.RobotNord, new List<int>(new int[] { 129, 134 })));
            DictionaryEmplacementDepose.Add(131, new Eurobot2021EmplacementDepose(-1.12, 0.155, Eurobot2021Color.Neutre, Eurobot2021SideColor.Blue, -Math.PI,
                robotAttributionYellow: Eurobot2021RobotType.RobotSud, robotAttributionBlue: Eurobot2021RobotType.RobotNord, new List<int>(new int[] { 130, 135 })));

            DictionaryEmplacementDepose.Add(132, new Eurobot2021EmplacementDepose(-1.44, 0.24, Eurobot2021Color.Neutre, Eurobot2021SideColor.Blue, -Math.PI,
                robotAttributionYellow: Eurobot2021RobotType.RobotSud, robotAttributionBlue: Eurobot2021RobotType.RobotNord, new List<int>(new int[] { })));
            DictionaryEmplacementDepose.Add(133, new Eurobot2021EmplacementDepose(-1.36, 0.24, Eurobot2021Color.Neutre, Eurobot2021SideColor.Blue, -Math.PI,
                robotAttributionYellow: Eurobot2021RobotType.RobotSud, robotAttributionBlue: Eurobot2021RobotType.RobotNord, new List<int>(new int[] { 127, 132 })));
            DictionaryEmplacementDepose.Add(134, new Eurobot2021EmplacementDepose(-1.28, 0.24, Eurobot2021Color.Neutre, Eurobot2021SideColor.Blue, -Math.PI,
                robotAttributionYellow: Eurobot2021RobotType.RobotSud, robotAttributionBlue: Eurobot2021RobotType.RobotNord, new List<int>(new int[] { 128, 133 })));
            DictionaryEmplacementDepose.Add(135, new Eurobot2021EmplacementDepose(-1.20, 0.24, Eurobot2021Color.Neutre, Eurobot2021SideColor.Blue, -Math.PI,
                robotAttributionYellow: Eurobot2021RobotType.RobotSud, robotAttributionBlue: Eurobot2021RobotType.RobotNord, new List<int>(new int[] { 129, 134 })));
            DictionaryEmplacementDepose.Add(136, new Eurobot2021EmplacementDepose(-1.12, 0.24, Eurobot2021Color.Neutre, Eurobot2021SideColor.Blue, -Math.PI,
                robotAttributionYellow: Eurobot2021RobotType.RobotSud, robotAttributionBlue: Eurobot2021RobotType.RobotNord, new List<int>(new int[] { 130, 135 })));

            ///Emplacement Grand Port Jaune
            DictionaryEmplacementDepose.Add(1, new Eurobot2021EmplacementDepose(1.44, 0.48, Eurobot2021Color.Rouge, Eurobot2021SideColor.Yellow, 0,
                robotAttributionYellow: Eurobot2021RobotType.RobotNord, robotAttributionBlue: Eurobot2021RobotType.RobotSud, new List<int>(new int[] { })));
            DictionaryEmplacementDepose.Add(2, new Eurobot2021EmplacementDepose(1.36, 0.48, Eurobot2021Color.Rouge, Eurobot2021SideColor.Yellow, 0,
                robotAttributionYellow: Eurobot2021RobotType.RobotNord, robotAttributionBlue: Eurobot2021RobotType.RobotSud, new List<int>(new int[] { 1 })));
            DictionaryEmplacementDepose.Add(3, new Eurobot2021EmplacementDepose(1.28, 0.48, Eurobot2021Color.Rouge, Eurobot2021SideColor.Yellow, 0,
                robotAttributionYellow: Eurobot2021RobotType.RobotNord, robotAttributionBlue: Eurobot2021RobotType.RobotSud, new List<int>(new int[] { 2 })));
            DictionaryEmplacementDepose.Add(4, new Eurobot2021EmplacementDepose(1.20, 0.48, Eurobot2021Color.Rouge, Eurobot2021SideColor.Yellow, 0,
                robotAttributionYellow: Eurobot2021RobotType.RobotNord, robotAttributionBlue: Eurobot2021RobotType.RobotSud, new List<int>(new int[] { 3 })));
            DictionaryEmplacementDepose.Add(5, new Eurobot2021EmplacementDepose(1.12, 0.48, Eurobot2021Color.Rouge, Eurobot2021SideColor.Yellow, 0,
                robotAttributionYellow: Eurobot2021RobotType.RobotNord, robotAttributionBlue: Eurobot2021RobotType.RobotSud, new List<int>(new int[] { 4 })));

            DictionaryEmplacementDepose.Add(6, new Eurobot2021EmplacementDepose(1.44, -0.085, Eurobot2021Color.Vert, Eurobot2021SideColor.Yellow, 0,
                robotAttributionYellow: Eurobot2021RobotType.RobotNord, robotAttributionBlue: Eurobot2021RobotType.RobotSud, new List<int>(new int[] { })));
            DictionaryEmplacementDepose.Add(7, new Eurobot2021EmplacementDepose(1.36, -0.085, Eurobot2021Color.Vert, Eurobot2021SideColor.Yellow, 0,
                robotAttributionYellow: Eurobot2021RobotType.RobotNord, robotAttributionBlue: Eurobot2021RobotType.RobotSud, new List<int>(new int[] { 6 })));
            DictionaryEmplacementDepose.Add(8, new Eurobot2021EmplacementDepose(1.28, -0.085, Eurobot2021Color.Vert, Eurobot2021SideColor.Yellow, 0,
                robotAttributionYellow: Eurobot2021RobotType.RobotNord, robotAttributionBlue: Eurobot2021RobotType.RobotSud, new List<int>(new int[] { 7 })));
            DictionaryEmplacementDepose.Add(9, new Eurobot2021EmplacementDepose(1.20, -0.085, Eurobot2021Color.Vert, Eurobot2021SideColor.Yellow, 0,
                robotAttributionYellow: Eurobot2021RobotType.RobotNord, robotAttributionBlue: Eurobot2021RobotType.RobotSud, new List<int>(new int[] { 8 })));
            DictionaryEmplacementDepose.Add(10, new Eurobot2021EmplacementDepose(1.12, -0.085, Eurobot2021Color.Vert, Eurobot2021SideColor.Yellow, 0,
                robotAttributionYellow: Eurobot2021RobotType.RobotNord, robotAttributionBlue: Eurobot2021RobotType.RobotSud, new List<int>(new int[] { 9 })));

            DictionaryEmplacementDepose.Add(27, new Eurobot2021EmplacementDepose(1.44, 0.155, Eurobot2021Color.Neutre, Eurobot2021SideColor.Yellow, 0,
                robotAttributionYellow: Eurobot2021RobotType.RobotNord, robotAttributionBlue: Eurobot2021RobotType.RobotSud, new List<int>(new int[] { })));
            DictionaryEmplacementDepose.Add(28, new Eurobot2021EmplacementDepose(1.36, 0.155, Eurobot2021Color.Neutre, Eurobot2021SideColor.Yellow, 0,
                robotAttributionYellow: Eurobot2021RobotType.RobotNord, robotAttributionBlue: Eurobot2021RobotType.RobotSud, new List<int>(new int[] { 27, 32 })));
            DictionaryEmplacementDepose.Add(29, new Eurobot2021EmplacementDepose(1.28, 0.155, Eurobot2021Color.Neutre, Eurobot2021SideColor.Yellow, 0,
                robotAttributionYellow: Eurobot2021RobotType.RobotNord, robotAttributionBlue: Eurobot2021RobotType.RobotSud, new List<int>(new int[] { 28, 33 })));
            DictionaryEmplacementDepose.Add(30, new Eurobot2021EmplacementDepose(1.20, 0.155, Eurobot2021Color.Neutre, Eurobot2021SideColor.Yellow, 0,
                robotAttributionYellow: Eurobot2021RobotType.RobotNord, robotAttributionBlue: Eurobot2021RobotType.RobotSud, new List<int>(new int[] { 29, 34 })));
            DictionaryEmplacementDepose.Add(31, new Eurobot2021EmplacementDepose(1.12, 0.155, Eurobot2021Color.Neutre, Eurobot2021SideColor.Yellow, 0,
                robotAttributionYellow: Eurobot2021RobotType.RobotNord, robotAttributionBlue: Eurobot2021RobotType.RobotSud, new List<int>(new int[] { 30, 35 })));

            DictionaryEmplacementDepose.Add(32, new Eurobot2021EmplacementDepose(1.44, 0.24, Eurobot2021Color.Neutre, Eurobot2021SideColor.Yellow, 0,
                robotAttributionYellow: Eurobot2021RobotType.RobotNord, robotAttributionBlue: Eurobot2021RobotType.RobotSud, new List<int>(new int[] { })));
            DictionaryEmplacementDepose.Add(33, new Eurobot2021EmplacementDepose(1.36, 0.24, Eurobot2021Color.Neutre, Eurobot2021SideColor.Yellow, 0,
                robotAttributionYellow: Eurobot2021RobotType.RobotNord, robotAttributionBlue: Eurobot2021RobotType.RobotSud, new List<int>(new int[] { 27, 32 })));
            DictionaryEmplacementDepose.Add(34, new Eurobot2021EmplacementDepose(1.28, 0.24, Eurobot2021Color.Neutre, Eurobot2021SideColor.Yellow, 0,
                robotAttributionYellow: Eurobot2021RobotType.RobotNord, robotAttributionBlue: Eurobot2021RobotType.RobotSud, new List<int>(new int[] { 28, 33 })));
            DictionaryEmplacementDepose.Add(35, new Eurobot2021EmplacementDepose(1.20, 0.24, Eurobot2021Color.Neutre, Eurobot2021SideColor.Yellow, 0,
                robotAttributionYellow: Eurobot2021RobotType.RobotNord, robotAttributionBlue: Eurobot2021RobotType.RobotSud, new List<int>(new int[] { 29, 34 })));
            DictionaryEmplacementDepose.Add(36, new Eurobot2021EmplacementDepose(1.12, 0.24, Eurobot2021Color.Neutre, Eurobot2021SideColor.Yellow, 0,
                robotAttributionYellow: Eurobot2021RobotType.RobotNord, robotAttributionBlue: Eurobot2021RobotType.RobotSud, new List<int>(new int[] { 30, 35 })));

            //Emplacement Petit Port Jaune
            DictionaryEmplacementDepose.Add(23, new Eurobot2021EmplacementDepose(-0.445, -0.695, Eurobot2021Color.Vert, Eurobot2021SideColor.Yellow, -Math.PI / 2,
                robotAttributionYellow: Eurobot2021RobotType.RobotSud, robotAttributionBlue: Eurobot2021RobotType.RobotNord, new List<int>(new int[] { 24 })));
            DictionaryEmplacementDepose.Add(19, new Eurobot2021EmplacementDepose(-0.445, -0.780, Eurobot2021Color.Vert, Eurobot2021SideColor.Yellow, -Math.PI / 2,
                robotAttributionYellow: Eurobot2021RobotType.RobotSud, robotAttributionBlue: Eurobot2021RobotType.RobotNord, new List<int>(new int[] { 20 })));
            DictionaryEmplacementDepose.Add(15, new Eurobot2021EmplacementDepose(-0.445, -0.870, Eurobot2021Color.Vert, Eurobot2021SideColor.Yellow, -Math.PI / 2,
                robotAttributionYellow: Eurobot2021RobotType.RobotSud, robotAttributionBlue: Eurobot2021RobotType.RobotNord, new List<int>(new int[] { 16 })));
            DictionaryEmplacementDepose.Add(11, new Eurobot2021EmplacementDepose(-0.445, -0.950, Eurobot2021Color.Vert, Eurobot2021SideColor.Yellow, -Math.PI / 2,
                robotAttributionYellow: Eurobot2021RobotType.RobotSud, robotAttributionBlue: Eurobot2021RobotType.RobotNord, new List<int>(new int[] { 12 })));

            DictionaryEmplacementDepose.Add(24, new Eurobot2021EmplacementDepose(-0.360, -0.695, Eurobot2021Color.Vert, Eurobot2021SideColor.Yellow, -Math.PI / 2,
                robotAttributionYellow: Eurobot2021RobotType.RobotSud, robotAttributionBlue: Eurobot2021RobotType.RobotNord, new List<int>(new int[] { 19 })));
            DictionaryEmplacementDepose.Add(20, new Eurobot2021EmplacementDepose(-0.360, -0.780, Eurobot2021Color.Vert, Eurobot2021SideColor.Yellow, -Math.PI / 2,
                robotAttributionYellow: Eurobot2021RobotType.RobotSud, robotAttributionBlue: Eurobot2021RobotType.RobotNord, new List<int>(new int[] { 15 })));
            DictionaryEmplacementDepose.Add(16, new Eurobot2021EmplacementDepose(-0.360, -0.870, Eurobot2021Color.Vert, Eurobot2021SideColor.Yellow, -Math.PI / 2,
                robotAttributionYellow: Eurobot2021RobotType.RobotSud, robotAttributionBlue: Eurobot2021RobotType.RobotNord, new List<int>(new int[] { 11 })));
            DictionaryEmplacementDepose.Add(12, new Eurobot2021EmplacementDepose(-0.360, -0.950, Eurobot2021Color.Vert, Eurobot2021SideColor.Yellow, -Math.PI / 2,
                robotAttributionYellow: Eurobot2021RobotType.RobotSud, robotAttributionBlue: Eurobot2021RobotType.RobotNord, new List<int>(new int[] { })));  //Premier Vert

            DictionaryEmplacementDepose.Add(25, new Eurobot2021EmplacementDepose(-0.250, -0.695, Eurobot2021Color.Rouge, Eurobot2021SideColor.Yellow, -Math.PI / 2,
                robotAttributionYellow: Eurobot2021RobotType.RobotSud, robotAttributionBlue: Eurobot2021RobotType.RobotNord, new List<int>(new int[] { 26 })));
            DictionaryEmplacementDepose.Add(21, new Eurobot2021EmplacementDepose(-0.250, -0.780, Eurobot2021Color.Rouge, Eurobot2021SideColor.Yellow, -Math.PI / 2,
                robotAttributionYellow: Eurobot2021RobotType.RobotSud, robotAttributionBlue: Eurobot2021RobotType.RobotNord, new List<int>(new int[] { 22 })));
            DictionaryEmplacementDepose.Add(17, new Eurobot2021EmplacementDepose(-0.250, -0.870, Eurobot2021Color.Rouge, Eurobot2021SideColor.Yellow, -Math.PI / 2,
                robotAttributionYellow: Eurobot2021RobotType.RobotSud, robotAttributionBlue: Eurobot2021RobotType.RobotNord, new List<int>(new int[] { 18 })));
            DictionaryEmplacementDepose.Add(13, new Eurobot2021EmplacementDepose(-0.250, -0.950, Eurobot2021Color.Rouge, Eurobot2021SideColor.Yellow, -Math.PI / 2,
                robotAttributionYellow: Eurobot2021RobotType.RobotSud, robotAttributionBlue: Eurobot2021RobotType.RobotNord, new List<int>(new int[] { 14 })));

            DictionaryEmplacementDepose.Add(26, new Eurobot2021EmplacementDepose(-0.155, -0.695, Eurobot2021Color.Rouge, Eurobot2021SideColor.Yellow, -Math.PI / 2,
                robotAttributionYellow: Eurobot2021RobotType.RobotSud, robotAttributionBlue: Eurobot2021RobotType.RobotNord, new List<int>(new int[] { 21 })));
            DictionaryEmplacementDepose.Add(22, new Eurobot2021EmplacementDepose(-0.155, -0.780, Eurobot2021Color.Rouge, Eurobot2021SideColor.Yellow, -Math.PI / 2,
                robotAttributionYellow: Eurobot2021RobotType.RobotSud, robotAttributionBlue: Eurobot2021RobotType.RobotNord, new List<int>(new int[] { 17 })));
            DictionaryEmplacementDepose.Add(18, new Eurobot2021EmplacementDepose(-0.155, -0.870, Eurobot2021Color.Rouge, Eurobot2021SideColor.Yellow, -Math.PI / 2,
                robotAttributionYellow: Eurobot2021RobotType.RobotSud, robotAttributionBlue: Eurobot2021RobotType.RobotNord, new List<int>(new int[] { 13 })));
            DictionaryEmplacementDepose.Add(14, new Eurobot2021EmplacementDepose(-0.155, -0.950, Eurobot2021Color.Rouge, Eurobot2021SideColor.Yellow, -Math.PI / 2,
                robotAttributionYellow: Eurobot2021RobotType.RobotSud, robotAttributionBlue: Eurobot2021RobotType.RobotNord, new List<int>(new int[] { })));

            //Emplacement Petit Port Bleu
            DictionaryEmplacementDepose.Add(123, new Eurobot2021EmplacementDepose(0.445, -0.695, Eurobot2021Color.Rouge, Eurobot2021SideColor.Blue, -Math.PI / 2,
                robotAttributionYellow: Eurobot2021RobotType.RobotNord, robotAttributionBlue: Eurobot2021RobotType.RobotSud, new List<int>(new int[] { 124 })));
            DictionaryEmplacementDepose.Add(119, new Eurobot2021EmplacementDepose(0.445, -0.780, Eurobot2021Color.Rouge, Eurobot2021SideColor.Blue, -Math.PI / 2,
                robotAttributionYellow: Eurobot2021RobotType.RobotNord, robotAttributionBlue: Eurobot2021RobotType.RobotSud, new List<int>(new int[] { 120 })));
            DictionaryEmplacementDepose.Add(115, new Eurobot2021EmplacementDepose(0.445, -0.870, Eurobot2021Color.Rouge, Eurobot2021SideColor.Blue, -Math.PI / 2,
                robotAttributionYellow: Eurobot2021RobotType.RobotNord, robotAttributionBlue: Eurobot2021RobotType.RobotSud, new List<int>(new int[] { 116 })));
            DictionaryEmplacementDepose.Add(111, new Eurobot2021EmplacementDepose(0.445, -0.950, Eurobot2021Color.Rouge, Eurobot2021SideColor.Blue, -Math.PI / 2,
                robotAttributionYellow: Eurobot2021RobotType.RobotNord, robotAttributionBlue: Eurobot2021RobotType.RobotSud, new List<int>(new int[] { 112 })));

            DictionaryEmplacementDepose.Add(124, new Eurobot2021EmplacementDepose(0.360, -0.695, Eurobot2021Color.Rouge, Eurobot2021SideColor.Blue, -Math.PI / 2,
                robotAttributionYellow: Eurobot2021RobotType.RobotNord, robotAttributionBlue: Eurobot2021RobotType.RobotSud, new List<int>(new int[] { 119 })));
            DictionaryEmplacementDepose.Add(120, new Eurobot2021EmplacementDepose(0.360, -0.780, Eurobot2021Color.Rouge, Eurobot2021SideColor.Blue, -Math.PI / 2,
                robotAttributionYellow: Eurobot2021RobotType.RobotNord, robotAttributionBlue: Eurobot2021RobotType.RobotSud, new List<int>(new int[] { 115 })));
            DictionaryEmplacementDepose.Add(116, new Eurobot2021EmplacementDepose(0.360, -0.870, Eurobot2021Color.Rouge, Eurobot2021SideColor.Blue, -Math.PI / 2,
                robotAttributionYellow: Eurobot2021RobotType.RobotNord, robotAttributionBlue: Eurobot2021RobotType.RobotSud, new List<int>(new int[] { 111 })));
            DictionaryEmplacementDepose.Add(112, new Eurobot2021EmplacementDepose(0.360, -0.950, Eurobot2021Color.Rouge, Eurobot2021SideColor.Blue, -Math.PI / 2,
                robotAttributionYellow: Eurobot2021RobotType.RobotNord, robotAttributionBlue: Eurobot2021RobotType.RobotSud, new List<int>(new int[] { })));

            DictionaryEmplacementDepose.Add(125, new Eurobot2021EmplacementDepose(0.250, -0.695, Eurobot2021Color.Vert, Eurobot2021SideColor.Blue, -Math.PI / 2,
                robotAttributionYellow: Eurobot2021RobotType.RobotNord, robotAttributionBlue: Eurobot2021RobotType.RobotSud, new List<int>(new int[] { 126 })));
            DictionaryEmplacementDepose.Add(121, new Eurobot2021EmplacementDepose(0.250, -0.780, Eurobot2021Color.Vert, Eurobot2021SideColor.Blue, -Math.PI / 2,
                robotAttributionYellow: Eurobot2021RobotType.RobotNord, robotAttributionBlue: Eurobot2021RobotType.RobotSud, new List<int>(new int[] { 122 })));
            DictionaryEmplacementDepose.Add(117, new Eurobot2021EmplacementDepose(0.250, -0.870, Eurobot2021Color.Vert, Eurobot2021SideColor.Blue, -Math.PI / 2,
                robotAttributionYellow: Eurobot2021RobotType.RobotNord, robotAttributionBlue: Eurobot2021RobotType.RobotSud, new List<int>(new int[] { 118 })));
            DictionaryEmplacementDepose.Add(113, new Eurobot2021EmplacementDepose(0.250, -0.950, Eurobot2021Color.Vert, Eurobot2021SideColor.Blue, -Math.PI / 2,
                robotAttributionYellow: Eurobot2021RobotType.RobotNord, robotAttributionBlue: Eurobot2021RobotType.RobotSud, new List<int>(new int[] { 114 })));

            DictionaryEmplacementDepose.Add(126, new Eurobot2021EmplacementDepose(0.155, -0.695, Eurobot2021Color.Vert, Eurobot2021SideColor.Blue, -Math.PI / 2,
                robotAttributionYellow: Eurobot2021RobotType.RobotNord, robotAttributionBlue: Eurobot2021RobotType.RobotSud, new List<int>(new int[] { 121 })));
            DictionaryEmplacementDepose.Add(122, new Eurobot2021EmplacementDepose(0.155, -0.780, Eurobot2021Color.Vert, Eurobot2021SideColor.Blue, -Math.PI / 2,
                robotAttributionYellow: Eurobot2021RobotType.RobotNord, robotAttributionBlue: Eurobot2021RobotType.RobotSud, new List<int>(new int[] { 117 })));
            DictionaryEmplacementDepose.Add(118, new Eurobot2021EmplacementDepose(0.155, -0.870, Eurobot2021Color.Vert, Eurobot2021SideColor.Blue, -Math.PI / 2,
                robotAttributionYellow: Eurobot2021RobotType.RobotNord, robotAttributionBlue: Eurobot2021RobotType.RobotSud, new List<int>(new int[] { 113 })));
            DictionaryEmplacementDepose.Add(114, new Eurobot2021EmplacementDepose(0.155, -0.950, Eurobot2021Color.Vert, Eurobot2021SideColor.Blue, -Math.PI / 2,
                robotAttributionYellow: Eurobot2021RobotType.RobotNord, robotAttributionBlue: Eurobot2021RobotType.RobotSud, new List<int>(new int[] { })));

        }

        void FillBrasTurbineDictionary()
        {
            DictionaryBrasTurbine = new Dictionary<string, Eurobot2021StateBrasTurbineRobot>();

            DictionaryBrasTurbine.Add("Bras_0", new Eurobot2021StateBrasTurbineRobot(0));
            DictionaryBrasTurbine.Add("Bras_45", new Eurobot2021StateBrasTurbineRobot(Math.PI / 4));
            DictionaryBrasTurbine.Add("Bras_135", new Eurobot2021StateBrasTurbineRobot(3 * Math.PI / 4));
            DictionaryBrasTurbine.Add("Bras_180", new Eurobot2021StateBrasTurbineRobot(4 * Math.PI / 4));
            DictionaryBrasTurbine.Add("Bras_315", new Eurobot2021StateBrasTurbineRobot(7 * Math.PI / 4));
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

    public enum Eurobot2021SideColor 
    { 
        Blue, 
        Yellow 
    };
    
    public enum Eurobot2021RobotType 
    { 
        RobotSud, 
        RobotNord, 
        None 
    };

    public enum Eurobot2021StrategyType 
    { 
        Soft, 
        LaConchaDeSuMadre 
    };

    public enum Eurobot2021Color
    {
        Vert,
        Rouge,
        Neutre
    }

    public enum Eurobot2021TypeGobelet
    {
        Libre,
        Distributeur
    }

    public enum Eurobot2021TeamReservation
    {
        Shared,
        ReservedBlue,
        ReservedYellow,
    }

    public enum Eurobot2021TypeELementDeJeu
    {
        Gobelet,
        Phare,
        MancheAir
    }


    public abstract class Eurobot2021ElementDeJeu
    {
        public PointD Pos;
        public double? AnglePrise;
        public double PriorityBlue = 1;
        public double PriorityYellow = 1;
        public Eurobot2021RobotType RobotAttributionYellow;
        public Eurobot2021RobotType RobotAttributionBlue;
        public Eurobot2021TeamReservation ReservationToTeam = Eurobot2021TeamReservation.Shared;
        public Eurobot2021TypeELementDeJeu elementDeJeu = Eurobot2021TypeELementDeJeu.Gobelet;
        public bool isAvailable;
    }

    public class Eurobot2021Gobelet : Eurobot2021ElementDeJeu
    {
        public Eurobot2021Color Color;
        public Eurobot2021TypeGobelet Type;
        public int? NbDeposeToTrigger = null;

        public Eurobot2021Gobelet(double x, double y, Eurobot2021Color color, Eurobot2021TypeGobelet type,
            Eurobot2021TeamReservation reserved, double? anglePrise,
            Eurobot2021RobotType robotAttributionBlue = Eurobot2021RobotType.None, Eurobot2021RobotType robotAttributionYellow = Eurobot2021RobotType.None,
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

    public class Eurobot2021MancheAir : Eurobot2021ElementDeJeu
    {
        public Eurobot2021MancheAir(Eurobot2021TypeELementDeJeu mancheAir, double x, double y, Eurobot2021TeamReservation reserved, double? anglePrise,
            Eurobot2021RobotType robotAttributionBlue = Eurobot2021RobotType.None, Eurobot2021RobotType robotAttributionYellow = Eurobot2021RobotType.None,
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

    public class Eurobot2021Phare : Eurobot2021ElementDeJeu
    {
        public Eurobot2021Phare(Eurobot2021TypeELementDeJeu phare, double x, double y, Eurobot2021TeamReservation reserved, double? anglePrise,
            Eurobot2021RobotType robotAttributionBlue = Eurobot2021RobotType.None, Eurobot2021RobotType robotAttributionYellow = Eurobot2021RobotType.None,
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

    public class Eurobot2021EmplacementDepose
    {
        //public Dictionary<int, CaseDepose> positionsDeposeList = new Dictionary<int, CaseDepose>();
        //public int Id;
        public PointD Pos;
        public double AngleDepose;
        public Eurobot2021SideColor SideColor;
        public Eurobot2021Color Color;
        public bool IsAvailable;
        public Eurobot2021RobotType RobotAttributionYellow;
        public Eurobot2021RobotType RobotAttributionBlue;
        public List<int> UnlockIdList = new List<int>();

        public Eurobot2021EmplacementDepose(double x, double y, Eurobot2021Color couleur, Eurobot2021SideColor team, double angleDepose,
            Eurobot2021RobotType robotAttributionYellow, Eurobot2021RobotType robotAttributionBlue, List<int> unlockList)
        {
            Pos = new PointD(x, y);
            Color = couleur;
            SideColor = team;
            AngleDepose = angleDepose;
            UnlockIdList = unlockList;
            RobotAttributionBlue = robotAttributionBlue;
            RobotAttributionYellow = robotAttributionYellow;
            IsAvailable = true;
        }
    }

    public class Eurobot2021StateBrasTurbineRobot
    {
        public bool HasGobelet = false;
        public Eurobot2021Color GobletCapturedColor = Eurobot2021Color.Neutre;
        public double AngleBras;

        public Eurobot2021StateBrasTurbineRobot(double angleBras)
        {
            AngleBras = angleBras;
        }
    }

    class Eurobot2021CaseDepose
    {
        public PointD Pos;
        public bool isFull;

        public Eurobot2021CaseDepose(double x, double y)
        {
            Pos = new PointD(x, y);
            isFull = false;
        }
    }
}
