using Constants;
using EventArgsLibrary;
using HeatMap;
using MessagesNS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Utilities;

namespace StrategyManagerNS
{
    public class StrategyRoboCup : StrategyGenerique
    {
        public bool resetRequired = false;

        public GameState gameState = GameState.STOPPED;
        public StoppedGameAction stoppedGameAction = StoppedGameAction.NONE;

        public TaskGameManagementRobocup taskGameManagement;
        public TaskParametersModifiersRoboCup taskParametersModifiers;
        public TaskAffichageLidarRoboCup taskAffichageLidar;

        RoboCupPoste consideredRobotRole = RoboCupPoste.Unassigned;
        public PointD robotDestination = new PointD(0, 0);
        PlayingSide playingSide = PlayingSide.Left;
        public BallHandlingState ballHandlingState = BallHandlingState.NoBall;

        public List<RoboCupPoste> strategyComposition = new List<RoboCupPoste> { RoboCupPoste.GoalKeeper, RoboCupPoste.DefenderLeft, RoboCupPoste.DefenderRight, RoboCupPoste.MidfielderCenter, RoboCupPoste.ForwardCenter };

        public string MessageDisplay = "Debug";

        TaskBallHandlingManagement taskBallHandlingManagement;

        Timer configTimer;

        bool useMulticast;

        public StrategyRoboCup(int robotId, int teamId, string multicastIpAddress) : base(robotId, teamId, multicastIpAddress)
        {
            taskBallHandlingManagement = new TaskBallHandlingManagement(this);
            InitHeatMap();
            RayonRobot = 0.25 * Math.Sqrt(2);
        }


        public List<TaskBase> listTasks { get; private set; }
        public List<MissionBase> listMissions { get; private set; }

        public override void InitStrategy(int robotId, int teamId)
        {
            listTasks = new List<TaskBase>();
            listMissions = new List<MissionBase>();

            taskGameManagement = new TaskGameManagementRobocup(this);
            listTasks.Add(taskGameManagement);
            taskAffichageLidar = new TaskAffichageLidarRoboCup(this);
            listTasks.Add(taskAffichageLidar);
            taskParametersModifiers = new TaskParametersModifiersRoboCup(this);
            listTasks.Add(taskParametersModifiers);

            /// Ajout des events
            OnIOValuesFromRobotEvent += OnIOValuesFromRobotEventReceived;

            //Obtenus directement à partir du script Matlab
            OnOdometryPointToMeter(4.261590e-06);
            On4WheelsAngleSet(1.256637e+00, 2.513274e+00, 3.769911e+00, 5.026548e+00);
            On4WheelsToPolarSet(-3.804226e-01, -2.351141e-01, 2.351141e-01, 3.804226e-01,
                                4.472136e-01, -4.472136e-01, -4.472136e-01, 4.472136e-01,
                                1.955694e+00, 7.470087e-01, 7.470087e-01, 1.955694e+00);
        }

        private void ConfigTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            //Ajustement de la distance minimalee de détection des objets
            RayonRobot = 0.4;
            MovingObstacleAvoidanceDistance = 1.0;

            //Réglage des asservissements
            double KpIndependant = 2.5;
            double KiIndependant = 300;
            //On envoie périodiquement les réglages du PID de vitesse embarqué
            On4WheelsIndependantSpeedPIDSetup(pM1: KpIndependant, iM1: KiIndependant, 0.0, pM2: KpIndependant, iM2: KiIndependant, 0, pM3: KpIndependant, iM3: KiIndependant, 0, pM4: KpIndependant, iM4: KiIndependant, 0.0,
                pM1Limit: 4, iM1Limit: 4, 0, pM2Limit: 4.0, iM2Limit: 4.0, 0, pM3Limit: 4.0, iM3Limit: 4.0, 0, pM4Limit: 4.0, iM4Limit: 4.0, 0);
            //OnSetRobotSpeedIndependantPID(pM1: 4.0, iM1: 300, 0.0, pM2: 4.0, iM2: 300, 0, pM3: 4.0, iM3: 300, 0, pM4: 4.0, iM4: 300, 0.0,
            //    pM1Limit: 4.0, iM1Limit: 4.0, 0, pM2Limit: 4.0, iM2Limit: 4.0, 0, pM3Limit: 4.0, iM3Limit: 4.0, 0, pM4Limit: 4.0, iM4Limit: 4.0, 0);
            //On4WheelsPolarSpeedPIDSetup(px: 4.0, ix: 300, 0.0, py: 4.0, iy: 300, 0, ptheta: 4.0, itheta: 300, 0,
            //pxLimit: 4.0, ixLimit: 4.0, 0, pyLimit: 4.0, iyLimit: 4.0, 0, pthetaLimit: 4.0, ithetaLimit: 4.0, 0);

            OnSetAsservissementMode((byte)AsservissementMode.Independant4Wheels);
        }

        public void OnIOValuesFromRobotEventReceived(object sender, IOValuesEventArgs e)
        {
            bool config0 = (((e.ioValues >> 0) & 0x01) == 0x01);
            bool config1 = (((e.ioValues >> 1) & 0x01) == 0x01);
            bool config2 = (((e.ioValues >> 2) & 0x01) == 0x01);
            bool config3 = (((e.ioValues >> 3) & 0x01) == 0x01);
            bool config4 = (((e.ioValues >> 4) & 0x01) == 0x01);
        }

        public override void InitHeatMap()
        {
            strategyHeatMap = new Heatmap(22.0, 14.0, (int)Math.Pow(2, 7), robotId); //Init HeatMap
            WayPointHeatMap = new Heatmap(22.0, 14.0, (int)Math.Pow(2, 7), robotId); //Init HeatMap
        }

        public override List<LocationExtended> FilterObstacleList(List<LocationExtended> obstacleList)
        {
            return obstacleList;
        }

        public override void DetermineRobotRole()
        {
            /// La détermination des rôles du robot se fait robot par robot, chacun détermine son propre rôle en temps réel. 
            /// Il n'y a pas de centralisation de la détermination dans la Base Station, ce qui permettra ultérieurement de jouer sans base station.
            /// 
            /// Le Gamestate est donné par la BaseStation via la localWorldMap car il intègre les commandes de la Referee Box
            /// 
            /// On détermine la situation de jeu : defense / attaque / arret / placement avant remise en jeu / ...
            /// et on détermine le rôle du robot.
            /// 


            ///Création du dictionnaire de classifieur de rôle qui sert à extraire
            ///les caractéristiques permettant de faire les choix stratégiques.            
            Dictionary<int, TeamMateRoleClassifier> teamRoleClassifier = new Dictionary<int, TeamMateRoleClassifier>();

            if (localWorldMap.robotLocation != null)
            {
                var id = robotId % 10;
                consideredRobotRole = strategyComposition[id];
                TeamMateRoleClassifier robotConsidere = new TeamMateRoleClassifier(localWorldMap.robotLocation.ToPointD(), strategyComposition[id]);
                teamRoleClassifier.Add(0, robotConsidere);
            }

            if (localWorldMap.teammateLocationList != null)
            {
                int n = 1;
                foreach (var teammate in localWorldMap.teammateLocationList)
                {
                    /// On ne connait pas le rôle a priori des teammates
                    teamRoleClassifier.Add(n++, new TeamMateRoleClassifier(teammate.ToPointD(), RoboCupPoste.Unassigned));
                }
            }


            switch (gameState)
            {
                case GameState.STOPPED:                    
                    break;
                case GameState.STOPPED_GAME_POSITIONING:                    
                    break;
                case GameState.PLAYING:
                    {
                        /// On commence par créer une liste de TeamMateRoleClassifier qui va permettre de trier intelligemment                         /// 
                        /// 
                        /// On détermine le rang du joueur dans l'équipe en fonction de sa distance au ballon   
                        /// On détermine le rang du joueur dans l'équipe en fonction de sa distance au but   
                        /// 
                        /// On regarde si une des équipe a la balle
                        /// 

                        if (playingSide == PlayingSide.Right)
                        {
                            //foreach (var teammate in teamRoleClassifier.)
                            {
                                //var robotId = teammate.Key % 10;
                                ////switch(robotId)
                                ////{
                                ////    case 
                                ////}
                                //if (teammate.Key % 10 != 0)
                                //{
                                //    teamRoleClassifier[teammate.Key].Role = RoboCupRobotRole.Stone;
                                //}
                            }
                        }
                        else
                        {
                            /// On détermine le rang du joueur dans l'équipe en fonction de sa distance au ballon                        
                            int rangDistanceBalle = -1;
                            if (localWorldMap.ballLocationList.Count > 0)
                            {
                                if (localWorldMap.ballLocationList[0] != null)
                                {
                                    var ballPosition = new PointD(localWorldMap.ballLocationList[0].X, localWorldMap.ballLocationList[0].Y);
                                    for (int i = 0; i < teamRoleClassifier.Count(); i++)
                                    {
                                        ///On ajoute à la liste en premier la distance à chacun des coéquipiers
                                        teamRoleClassifier.ElementAt(i).Value.DistanceBalle = Toolbox.Distance(teamRoleClassifier.ElementAt(i).Value.Position, ballPosition);
                                    }
                                }
                            }

                            /// On détermine le rang du joueur dans l'équipe en fonction de sa distance au but 
                            /// Pour cela il faut d'abord définir la position du but.
                            PointD offensiveGoalPosition;
                            PointD defensiveGoalPosition;
                            if (playingSide == PlayingSide.Right)
                            {
                                offensiveGoalPosition = new PointD(-11, 0);
                                defensiveGoalPosition = new PointD(11, 0);
                            }
                            else
                            {
                                defensiveGoalPosition = new PointD(-11, 0);
                                offensiveGoalPosition = new PointD(11, 0);
                            }

                            for (int i = 0; i < localWorldMap.teammateLocationList.Count(); i++)
                            {
                                teamRoleClassifier.ElementAt(i).Value.DistanceButOffensif = Toolbox.Distance(teamRoleClassifier.ElementAt(i).Value.Position, offensiveGoalPosition);
                            }

                            ///On détermine à présent si l'équipe à la balle et quel joueur la possède
                            ///
                            var teamBallHandlingState = BallHandlingState.NoBall;
                            int IdplayerHandlingBall = -1;

                            //foreach (var teammate in localWorldMap.teammateBallHandlingStateList)
                            //{
                            //    if (teammate.Value != BallHandlingState.NoBall)
                            //    {
                            //        teamBallHandlingState = teammate.Value;
                            //        IdplayerHandlingBall = teammate.Key;
                            //    }
                            //}

                            /// Les indicateurs principaux nécessaire à la stratégie ont été déterminés : 
                            /// Possession de balle, distance au but de chacun des coéquipiers et
                            /// distance à la balle de chacun des équipiers.
                            /// Il est donc posssible de prendre des décisions stratégiques de jeu
                            /// On commence par choisir le mode défense ou attaque selon que l'on a la balle ou pas
                            /// 

                            //if (teamBallHandlingState == BallHandlingState.NoBall)
                            //{
                            //    /// L'équipe n'a pas la balle, elle se place en mode défense
                            //    /// On veut deux joueurs en défense placée qui marquent les deux joueurs les plus en avant de l'équipe adverse
                            //    /// On veut un joueur en contestation de balle
                            //    /// On veut un joueur en défense d'interception qui coupe les lignes de passe adverses

                            //    /// A présent, on filtre la liste de l'équipe de manière à trouver le joueur 
                            //    /// le plus proches de la balle n'étant pas le gardien
                            //    var teamFiltered1 = teamRoleClassifier.Where(x => x.Value.Role == RoboCupPoste.Unassigned).OrderBy(elt => elt.Value.DistanceBalle).ToList();

                            //    if (teamFiltered1.Count > 0)
                            //    {
                            //        teamRoleClassifier[teamFiltered1.ElementAt(0).Key].Role = RoboCupPoste.DefenseurContesteur;
                            //    }

                            //    /// A présent, on filtre la liste de l'équipe de manière à trouver le joueur le plus proche du but à défendre
                            //    /// n'étant pas gardien, ni défenseur au marquage
                            //    /// Il devient contesteur de balle
                            //    var teamFiltered2 = teamRoleClassifier.Where(x => (x.Value.Role == RoboCupPoste.Unassigned))
                            //                                          .OrderBy(elt => elt.Value.DistanceButDefensif).ToList();
                            //    if (teamFiltered2.Count > 1)
                            //    {
                            //        teamRoleClassifier[teamFiltered2.ElementAt(1).Key].Role = RoboCupPoste.DefenseurMarquage;
                            //        teamRoleClassifier[teamFiltered2.ElementAt(2).Key].Role = RoboCupPoste.DefenseurMarquage;
                            //    }

                            //    /// A présent, on filtre la liste de l'équipe de manière à trouver le joueur le plus proche du but à défendre
                            //    /// n'étant pas gardien, ni défenseur au marquage, ni contesteur
                            //    /// Il devient défenseur intercepteur
                            //    var teamFiltered3 = teamRoleClassifier.Where(x => (x.Value.Role == RoboCupPoste.Unassigned))
                            //                                          .OrderBy(elt => elt.Value.DistanceButDefensif).ToList();
                            //    if (teamFiltered3.Count > 0)
                            //    {
                            //        teamRoleClassifier[teamFiltered3.ElementAt(0).Key].Role = RoboCupPoste.DefenseurIntercepteur;
                            //    }
                            //}

                            //else
                            //{
                            //    /// L'équipe a la balle, elle se place en mode attaque
                            //    /// On a un joueur ayant le ballon qui est l'attaquant avec balle
                            //    /// On veut deux joueurs attaquants démarqués avec lignes de passes ouvertes
                            //    /// On veut un attaquant placé entre un défenseur et l'attaquant ayant la balle
                            //    /// 
                            //    teamRoleClassifier[IdplayerHandlingBall].Role = RoboCupPoste.AttaquantAvecBalle;

                            //    var teamFiltered1 = teamRoleClassifier.Where(x => (x.Value.Role == RoboCupPoste.Unassigned))
                            //                                          .OrderBy(elt => elt.Value.DistanceButOffensif).ToList();

                            //    if (teamFiltered1.Count > 1)
                            //    {
                            //        teamRoleClassifier[teamFiltered1.ElementAt(0).Key].Role = RoboCupPoste.AttaquantDemarque;
                            //        teamRoleClassifier[teamFiltered1.ElementAt(1).Key].Role = RoboCupPoste.AttaquantDemarque;
                            //    }

                            //    /// A présent, on filtre la liste de l'équipe de manière à trouver le joueur le plus proche du but en attaque
                            //    /// n'étant pas gardien, ni attaquant avec le ballon ni attaquant Démarqué 
                            //    /// Il devient attaquant intercepteur
                            //    var teamFiltered2 = teamRoleClassifier.Where(x => (x.Value.Role == RoboCupPoste.Unassigned))
                            //                                          .OrderBy(elt => elt.Value.DistanceButOffensif).ToList();
                            //    if (teamFiltered2.Count > 0)
                            //    {
                            //        teamRoleClassifier[teamFiltered2.ElementAt(0).Key].Role = RoboCupPoste.AttaquantIntercepteur;
                            //    }
                            //}
                        }
                        //role = teamRoleClassifier[robotId].Role;
                    }
                    break;
            }

            OnRole(robotId, consideredRobotRole);
            OnBallHandlingState(robotId, BallHandlingState.NoBall);
            OnMessageDisplay(robotId, MessageDisplay);
        }

        public override void DetermineRobotZones()
        {
            double signX = 1;
            double signY = -1;

            if (playingSide == PlayingSide.Right)
            {
                signX = 1;
                signY = 1;
            }
            else
            {
                signX = -1;
                signY = -1;
            }


            switch (gameState)
            { 
                case GameState.STOPPED:
                    {
                        ///On force le robot à s'arrêter
                        AddPreferedZone(robotCurrentLocation.ToPointD(), 1, 1);
                        break;
                    }
                case GameState.STOPPED_GAME_POSITIONING:
                    {
                        switch (stoppedGameAction)
                        {
                            case StoppedGameAction.CORNER:
                                if (localWorldMap.ballLocationList[0].Y >= 0) ///Corner dans le coin en haut à gauche
                                    switch (consideredRobotRole)
                                    {
                                        case RoboCupPoste.DefenderCenter:
                                            AddPreferedZone(new PointD(signX * -1, signY * 0), 6, 0.5);
                                            break;
                                        case RoboCupPoste.DefenderRight:
                                            AddPreferedZone(new PointD(signX * -1, signY * -3), 6, 0.5);
                                            break;
                                        case RoboCupPoste.DefenderLeft:
                                            AddPreferedZone(new PointD(signX * -1, signY * 4), 6, 0.5);
                                            break;
                                        case RoboCupPoste.MidfielderCenter:
                                            AddPreferedZone(new PointD(signX * 3, signY * 3), 6, 0.5);
                                            break;
                                        case RoboCupPoste.MidfielderRight:
                                            AddPreferedZone(new PointD(signX * 3, signY * -1), 6, 0.5);
                                            break;
                                        case RoboCupPoste.MidfielderLeft:
                                            AddPreferedZone(new PointD(signX * 3, signY * 5), 6, 0.5);
                                            break;
                                        case RoboCupPoste.ForwardCenter:
                                            AddPreferedZone(new PointD(signX * 8, signY * 3), 6, 0.5);
                                            break;
                                        case RoboCupPoste.ForwardRight:
                                            AddPreferedZone(new PointD(signX * 8, signY * -1), 6, 0.5);
                                            break;
                                        case RoboCupPoste.ForwardLeft:
                                            AddPreferedZone(new PointD(signX * 11.5, signY * 7.5), 6, 0.5);
                                            break;
                                    }
                                else
                                    switch (consideredRobotRole)
                                    {
                                        case RoboCupPoste.DefenderCenter:
                                            AddPreferedZone(new PointD(signX * -1, signY * 0), 6, 0.5);
                                            break;
                                        case RoboCupPoste.DefenderRight:
                                            AddPreferedZone(new PointD(signX * -1, signY * -4), 6, 0.5);
                                            break;
                                        case RoboCupPoste.DefenderLeft:
                                            AddPreferedZone(new PointD(signX * -1, signY * 3), 6, 0.5);
                                            break;
                                        case RoboCupPoste.MidfielderCenter:
                                            AddPreferedZone(new PointD(signX * 3, signY * -3), 6, 0.5);
                                            break;
                                        case RoboCupPoste.MidfielderRight:
                                            AddPreferedZone(new PointD(signX * 3, signY * -5), 6, 0.5);
                                            break;
                                        case RoboCupPoste.MidfielderLeft:
                                            AddPreferedZone(new PointD(signX * 3, signY * 1), 6, 0.5);
                                            break;
                                        case RoboCupPoste.ForwardCenter:
                                            AddPreferedZone(new PointD(signX * 8, signY * -3), 6, 0.5);
                                            break;
                                        case RoboCupPoste.ForwardRight:
                                            AddPreferedZone(new PointD(signX * 8, signY * 7.5), 6, 0.5);
                                            break;
                                        case RoboCupPoste.ForwardLeft:
                                            AddPreferedZone(new PointD(signX * 11.5, signY * -1), 6, 0.5);
                                            break;
                                    }
                                break;
                            case StoppedGameAction.CORNER_OPPONENT:
                                if (localWorldMap.ballLocationList[0].Y >= 0) ///Corner dans le coin en haut à gauche
                                    switch (consideredRobotRole)
                                    {
                                        case RoboCupPoste.DefenderCenter:
                                            AddPreferedZone(new PointD(signX * -9, signY * 0), 6, 0.5);
                                            break;
                                        case RoboCupPoste.DefenderRight:
                                            AddPreferedZone(new PointD(signX * -10, signY * 4), 6, 0.5);
                                            break;
                                        case RoboCupPoste.DefenderLeft:
                                            AddPreferedZone(new PointD(signX * -9, signY * 0), 6, 0.5);
                                            break;
                                        case RoboCupPoste.MidfielderCenter:
                                            AddPreferedZone(new PointD(signX * -8, signY * 0), 6, 0.5);
                                            break;
                                        case RoboCupPoste.MidfielderRight:
                                            AddPreferedZone(new PointD(signX * -8.5, signY * 4), 6, 0.5);
                                            break;
                                        case RoboCupPoste.MidfielderLeft:
                                            AddPreferedZone(new PointD(signX * -8, signY * -2), 6, 0.5);
                                            break;
                                        case RoboCupPoste.ForwardCenter:
                                            AddPreferedZone(new PointD(signX * -5, signY * 0), 6, 0.5);
                                            break;
                                        case RoboCupPoste.ForwardRight:
                                            AddPreferedZone(new PointD(signX * -5, signY * 4), 6, 0.5);
                                            break;
                                        case RoboCupPoste.ForwardLeft:
                                            AddPreferedZone(new PointD(signX * -5, signY * -2), 6, 0.5);
                                            break;
                                    }
                                else if (localWorldMap.ballLocationList[0].Y < 0) ///Corner dans le coin en haut à gauche
                                    switch (consideredRobotRole)
                                    {
                                        case RoboCupPoste.DefenderCenter:
                                            AddPreferedZone(new PointD(signX * -9, signY * 0), 6, 0.5);
                                            break;
                                        case RoboCupPoste.DefenderRight:
                                            AddPreferedZone(new PointD(signX * -10, -signY * 0), 6, 0.5);
                                            break;
                                        case RoboCupPoste.DefenderLeft:
                                            AddPreferedZone(new PointD(signX * -9, signY * -4), 6, 0.5);
                                            break;
                                        case RoboCupPoste.MidfielderCenter:
                                            AddPreferedZone(new PointD(signX * -8, signY * 0), 6, 0.5);
                                            break;
                                        case RoboCupPoste.MidfielderRight:
                                            AddPreferedZone(new PointD(signX * -8.5, signY * 2), 6, 0.5);
                                            break;
                                        case RoboCupPoste.MidfielderLeft:
                                            AddPreferedZone(new PointD(signX * -8, signY * -4), 6, 0.5);
                                            break;
                                        case RoboCupPoste.ForwardCenter:
                                            AddPreferedZone(new PointD(signX * -5, signY * 0), 6, 0.5);
                                            break;
                                        case RoboCupPoste.ForwardRight:
                                            AddPreferedZone(new PointD(signX * -5, signY * 2), 6, 0.5);
                                            break;
                                        case RoboCupPoste.ForwardLeft:
                                            AddPreferedZone(new PointD(signX * -5, signY * -4), 6, 0.5);
                                            break;
                                    }
                                break;
                        }
                        break;
                    }
                case GameState.PLAYING:
                    {
                        double radiusZonePoste = 22;
                        /// Bords du terrain
                        //AddStrictlyAllowedRectangle(new RectangleD(-11 + RayonRobot, 11 - RayonRobot, -7 + RayonRobot, 7 - RayonRobot));
                        //AddStrictlyAllowedConvexPolygon(new ConvexPolygonD(new List<PointD> { new PointD(-11, -7), new PointD(11, -7),
                        //    new PointD(11, 7), new PointD(-11, 7) }));

                        ///On exclut d'emblée les surface de réparation pour tous les joueurs
                        if (consideredRobotRole != RoboCupPoste.GoalKeeper)
                        {
                            AddForbiddenRectangle(new RectangleD(-11, -11 + 0.75 + 0.2, -3.9 / 2 - 0.2, 3.9 / 2 + 0.2));
                            AddForbiddenRectangle(new RectangleD(-11, -11 + 0.75 + 0.2, -3.9 / 2 - 0.2, 3.9 / 2 + 0.2));
                            AddForbiddenRectangle(new RectangleD(+11 - 0.75 + 0.2, +11, -3.9 / 2 - 0.2, 3.9 / 2 + 0.2));
                        }

                        /// On a besoin du rang des adversaires en fonction de leur distance au but 
                        /// Pour cela il faut d'abord définir la position du but.
                        PointD offensiveGoalPosition;
                        PointD defensiveGoalPosition;
                        if (playingSide == PlayingSide.Right)
                        {
                            offensiveGoalPosition = new PointD(-11, 0);
                            defensiveGoalPosition = new PointD(11, 0);
                        }
                        else
                        {
                            defensiveGoalPosition = new PointD(-11, 0);
                            offensiveGoalPosition = new PointD(11, 0);
                        }

                        switch (consideredRobotRole)
                        {
                            case RoboCupPoste.GoalKeeper:
                                /// Gestion du cas du gardien
                                /// Exclusion de tout le terrain sauf la surface de réparation
                                /// Ajout d'une zone préférentielle centrée sur le but
                                /// Réglage du cap pour faire toujours face à la balle
                                if (playingSide == PlayingSide.Right)
                                {
                                    AddPreferredRectangle(new RectangleD(11 - 0.75, 11, -3.9 / 2, 3.9 / 2), 0.2);
                                    AddPreferedZone(new PointD(10.6, 0), radiusZonePoste, 0.5);
                                }
                                else
                                {
                                    AddPreferredRectangle(new RectangleD(-11, -11 + 0.75, -3.9 / 2, 3.9 / 2), 0.2);
                                    AddPreferedZone(new PointD(-10.6, 0), radiusZonePoste, 0.5);
                                }

                                if (localWorldMap.ballLocationList.Count > 0)
                                    robotOrientation = Math.Atan2(localWorldMap.ballLocationList[0].Y - robotCurrentLocation.Y, localWorldMap.ballLocationList[0].X - robotCurrentLocation.X);
                                break;

                            case RoboCupPoste.DefenderCenter:
                                if (playingSide == PlayingSide.Right)
                                {
                                    AddPreferredRectangle(new RectangleD(0, RoboCupField.Length / 2, -RoboCupField.Width / 2, RoboCupField.Width / 2), 0.2);
                                    AddPreferedZone(new PointD(RoboCupField.Length / 4, 0), radiusZonePoste, 0.5);
                                }
                                else
                                {
                                    AddPreferredRectangle(new RectangleD(-RoboCupField.Length / 2, 0, -RoboCupField.Width / 2, RoboCupField.Width / 2), 0.2);
                                    AddPreferedZone(new PointD(-RoboCupField.Length / 4, 0), radiusZonePoste, 0.5);
                                }
                                break;

                            case RoboCupPoste.DefenderRight:
                                if (playingSide == PlayingSide.Right)
                                {
                                    AddPreferredRectangle(new RectangleD(0, RoboCupField.Length / 2, -RoboCupField.Width / 2, RoboCupField.Width / 2), 0.2);
                                    AddPreferedZone(new PointD(RoboCupField.Length / 4, RoboCupField.Width / 4), radiusZonePoste, 0.5);
                                }
                                else
                                {
                                    AddPreferredRectangle(new RectangleD(-RoboCupField.Length / 2, 0, -RoboCupField.Width / 2, RoboCupField.Width / 2), 0.2);
                                    AddPreferedZone(new PointD(-RoboCupField.Length / 4, -RoboCupField.Width / 4), radiusZonePoste, 0.5);
                                }
                                break;

                            case RoboCupPoste.DefenderLeft:
                                if (playingSide == PlayingSide.Right)
                                {
                                    AddPreferredRectangle(new RectangleD(0, RoboCupField.Length / 2, -RoboCupField.Width / 2, RoboCupField.Width / 2), 0.2);
                                    AddPreferedZone(new PointD(RoboCupField.Length / 4, -RoboCupField.Width / 4), radiusZonePoste, 0.5);
                                }
                                else
                                {
                                    AddPreferredRectangle(new RectangleD(-RoboCupField.Length / 2, 0, -RoboCupField.Width / 2, RoboCupField.Width / 2), 0.2);
                                    AddPreferedZone(new PointD(-RoboCupField.Length / 4, RoboCupField.Width / 4), radiusZonePoste, 0.5);
                                }
                                break;

                            case RoboCupPoste.MidfielderCenter:
                                if (playingSide == PlayingSide.Right)
                                {
                                    AddPreferredRectangle(new RectangleD(-RoboCupField.Length / 4, RoboCupField.Length / 4, -RoboCupField.Width / 2, RoboCupField.Width / 2), 0.2);
                                    AddPreferedZone(new PointD(0, 0), radiusZonePoste, 0.5);
                                }
                                else
                                {
                                    AddPreferredRectangle(new RectangleD(-RoboCupField.Length / 4, RoboCupField.Length / 4, -RoboCupField.Width / 2, RoboCupField.Width / 2), 0.2);
                                    AddPreferedZone(new PointD(0, 0), radiusZonePoste, 0.5);
                                }
                                break;

                            case RoboCupPoste.MidfielderRight:
                                if (playingSide == PlayingSide.Right)
                                {
                                    AddPreferredRectangle(new RectangleD(-RoboCupField.Length / 4, RoboCupField.Length / 4, -RoboCupField.Width / 2, RoboCupField.Width / 2), 0.2);
                                    AddPreferedZone(new PointD(0, RoboCupField.Width / 4), radiusZonePoste, 0.5);
                                }
                                else
                                {
                                    AddPreferredRectangle(new RectangleD(-RoboCupField.Length / 4, RoboCupField.Length / 4, -RoboCupField.Width / 2, RoboCupField.Width / 2), 0.2);
                                    AddPreferedZone(new PointD(0, -RoboCupField.Width / 4), radiusZonePoste, 0.5);
                                }
                                break;

                            case RoboCupPoste.MidfielderLeft:
                                if (playingSide == PlayingSide.Right)
                                {
                                    AddPreferredRectangle(new RectangleD(-RoboCupField.Length / 4, RoboCupField.Length / 4, -RoboCupField.Width / 2, RoboCupField.Width / 2), 0.2);
                                    AddPreferedZone(new PointD(0, -RoboCupField.Width / 4), radiusZonePoste, 0.5);
                                }
                                else
                                {
                                    AddPreferredRectangle(new RectangleD(-RoboCupField.Length / 4, RoboCupField.Length / 4, -RoboCupField.Width / 2, RoboCupField.Width / 2), 0.2);
                                    AddPreferedZone(new PointD(0, RoboCupField.Width / 4), radiusZonePoste, 0.5);
                                }
                                break;

                            case RoboCupPoste.ForwardCenter:
                                if (playingSide == PlayingSide.Right)
                                {
                                    AddPreferredRectangle(new RectangleD(-RoboCupField.Length / 2, 0, -RoboCupField.Width / 2, RoboCupField.Width / 2), 0.2);
                                    AddPreferedZone(new PointD(-RoboCupField.Length / 4, 0), radiusZonePoste, 0.2);
                                }
                                else
                                {
                                    AddPreferredRectangle(new RectangleD(0, RoboCupField.Length / 2, -RoboCupField.Width / 2, RoboCupField.Width / 2), 0.2);
                                    AddPreferedZone(new PointD(RoboCupField.Length / 4, 0), radiusZonePoste, 0.2);
                                }
                                break;

                            case RoboCupPoste.ForwardRight:
                                if (playingSide == PlayingSide.Right)
                                {
                                    AddPreferredRectangle(new RectangleD(-RoboCupField.Length / 2, 0, -RoboCupField.Width / 2, RoboCupField.Width / 2), 0.2);
                                    AddPreferedZone(new PointD(-RoboCupField.Length / 4, RoboCupField.Width / 4), radiusZonePoste, 0.2);
                                }
                                else
                                {
                                    AddPreferredRectangle(new RectangleD(0, RoboCupField.Length / 2, -RoboCupField.Width / 2, RoboCupField.Width / 2), 0.2);
                                    AddPreferedZone(new PointD(RoboCupField.Length / 4, -RoboCupField.Width / 4), radiusZonePoste, 0.2);
                                }
                                break;

                            case RoboCupPoste.ForwardLeft:
                                if (playingSide == PlayingSide.Right)
                                {
                                    AddPreferredRectangle(new RectangleD(-RoboCupField.Length / 2, 0, -RoboCupField.Width / 2, RoboCupField.Width / 2), 0.2);
                                    AddPreferedZone(new PointD(-RoboCupField.Length / 4, -RoboCupField.Width / 4), radiusZonePoste, 0.2);
                                }
                                else
                                {
                                    AddPreferredRectangle(new RectangleD(0, RoboCupField.Length / 2, -RoboCupField.Width / 2, RoboCupField.Width / 2), 0.2);
                                    AddPreferedZone(new PointD(RoboCupField.Length * 3 / 4, RoboCupField.Width / 4), radiusZonePoste, 0.2);
                                }
                                break;
                        }

                        //case RoboCupPoste.DefenderCenter:
                        //if (localWorldMap.ballLocationList.Count > 0)
                        //    AddPreferedZone(new PointD(localWorldMap.ballLocationList[0].X, localWorldMap.ballLocationList[0].Y), 3, 0.5);
                        //if (localWorldMap.ballLocationList.Count > 0)
                        //    robotOrientation = Math.Atan2(localWorldMap.ballLocationList[0].Y - robotCurrentLocation.Y, localWorldMap.ballLocationList[0].X - robotCurrentLocation.X);
                        //break;

                        //case RoboCupPoste.DefenseurMarquage:
                        //    {
                        //        /// On va placer un défenseur à une distance définie de l'attaquant, sur la ligne attaquant but
                        //        /// Les zones d'intérêt sont devant les deux attaquants les plus en pointe
                        //        /// Il faut donc commencer par les trouver
                        //        Dictionary<int, TeamMateRoleClassifier> adversaireClassifier = new Dictionary<int, TeamMateRoleClassifier>();
                        //        int i = 0;
                        //        lock (localWorldMap)
                        //        {
                        //            foreach (var adversaire in localWorldMap.obstacleLocationList)
                        //            {
                        //                var adv = new TeamMateRoleClassifier(new PointD(adversaire.X, adversaire.Y), RoboCupPoste.Adversaire);
                        //                adv.DistanceButDefensif = Toolbox.Distance(new PointD(adversaire.X, adversaire.Y), defensiveGoalPosition);
                        //                adversaireClassifier.Add(i++, adv);
                        //            }
                        //        }

                        //        /// A présent, on filtre la liste des adversaires de manière à trouver les joueurs les plus proches du but en défense
                        //        /// 
                        //        var teamFiltered1 = adversaireClassifier.OrderBy(elt => elt.Value.DistanceButDefensif).ToList();

                        //        if (teamFiltered1.Count > 1)
                        //        {
                        //            if (playingSide == PlayingSide.Right)
                        //            {
                        //                AddPreferedZone(new PointD(teamFiltered1[0].Value.Position.X + 2, teamFiltered1[0].Value.Position.Y), 1.5);
                        //                AddPreferedZone(new PointD(teamFiltered1[1].Value.Position.X + 2, teamFiltered1[1].Value.Position.Y), 1.5);
                        //            }
                        //            else
                        //            {
                        //                AddPreferedZone(new PointD(teamFiltered1[0].Value.Position.X - 2, teamFiltered1[0].Value.Position.Y), 1.5);
                        //                AddPreferedZone(new PointD(teamFiltered1[1].Value.Position.X - 2, teamFiltered1[1].Value.Position.Y), 1.5);
                        //            }
                        //        }

                        //        if (localWorldMap.ballLocationList.Count > 0)
                        //        {
                        //            if (localWorldMap.ballLocationList[0] != null)
                        //                robotOrientation = Math.Atan2(localWorldMap.ballLocationList[0].Y - robotCurrentLocation.Y, localWorldMap.ballLocationList[0].X - robotCurrentLocation.X);
                        //        }

                        //    }
                        //    break;

                        //case RoboCupPoste.DefenseurIntercepteur:
                        //    {
                        //        /// On va placer un défenseur à une distance définie de l'attaquant, sur la ligne attaquant but
                        //        /// Les zones d'intérêt sont devant les deux attaquants les plus en pointe
                        //        /// Il faut donc commencer par les trouver
                        //        Dictionary<int, TeamMateRoleClassifier> adversaireClassifier = new Dictionary<int, TeamMateRoleClassifier>();
                        //        int i = 0;
                        //        foreach (var adversaire in localWorldMap.obstacleLocationList)
                        //        {
                        //            var adv = new TeamMateRoleClassifier(new PointD(adversaire.X, adversaire.Y), RoboCupPoste.Adversaire);
                        //            adv.DistanceButDefensif = Toolbox.Distance(new PointD(adversaire.X, adversaire.Y), defensiveGoalPosition);
                        //            adversaireClassifier.Add(i++, adv);
                        //        }

                        //        /// A présent, on filtre la liste des adversaires de manière à trouver les joueurs les plus proches du but en défense
                        //        /// 
                        //        var teamFiltered1 = adversaireClassifier.OrderBy(elt => elt.Value.DistanceButDefensif).ToList();

                        //        if (teamFiltered1.Count > 1)
                        //        {
                        //            var adversaire1 = teamFiltered1[0].Value.Position;
                        //            var adversaire2 = teamFiltered1[1].Value.Position;
                        //            AddPreferredSegmentZoneList(new PointD(adversaire1.X, adversaire1.Y), new PointD(adversaire2.X, adversaire2.Y), 0.4, 0.1);
                        //            AddPreferedZone(new PointD((adversaire1.X + adversaire2.X) / 2, (adversaire1.Y + adversaire2.Y) / 2), 0.4, 0.3);
                        //            AddAvoidanceZone(new PointD(adversaire1.X, adversaire1.Y), 2, 0.5);

                        //            //AddPreferedZone(new PointD((teamFiltered1[0].Value.Position.X + teamFiltered1[1].Value.Position.X) / 2, (teamFiltered1[0].Value.Position.Y + teamFiltered1[0].Value.Position.Y) / 2), 2.5);
                        //        }

                        //        if (localWorldMap.ballLocationList.Count > 0)
                        //            robotOrientation = Math.Atan2(localWorldMap.ballLocationList[0].Y - robotCurrentLocation.Y, localWorldMap.ballLocationList[0].X - robotCurrentLocation.X);

                        //    }
                        //    break;
                        //case RoboCupPoste.AttaquantDemarque:
                        //    /// Gestion du cas de l'attaquant démarqué
                        //    /// Il doit faire en sorte que la ligne de passe entre lui et le porteur du ballon soit libre
                        //    ///     Pour cela il faudrait idéalement placer une zone de pénalisation conique de centre le joueur dans l'axe de chaque adversaire
                        //    /// Il doit également se placer dans un position de tir possible 
                        //    ///     Pour cela il faudrait idéalement placer une zone de pénalisation conique de centre le but dans l'axe de chaque adversaire

                        //    if (playingSide == PlayingSide.Left)
                        //    {
                        //        AddPreferredSegmentZoneList(new PointD(7, -3), new PointD(7, 3), 3, 1);
                        //        //AddPreferedZone(new PointD(8, 3), 3, 0.1);
                        //        //AddPreferedZone(new PointD(8, -3), 3, 0.1);
                        //    }
                        //    else
                        //    {
                        //        //AddPreferredSegmentZoneList(new PointD(-7, -3), new PointD(-7, 3), 3, 1);
                        //        //AddPreferedZone(new PointD(-8, 3), 3, 0.1);
                        //        //AddPreferedZone(new PointD(-8, -3), 3, 0.1);
                        //    }

                        //    foreach (var adversaire in localWorldMap.obstacleLocationList)
                        //    {
                        //        AddAvoidanceConicalZoneList(new PointD(robotCurrentLocation.X, robotCurrentLocation.Y), new PointD(adversaire.X, adversaire.Y), 2);
                        //        //AddAvoidanceConicalZoneList(Toolbox.OffsetLocation(new PointD(robotCurrentLocation.X, robotCurrentLocation.Y), robotCurrentLocation), Toolbox.OffsetLocation(new PointD(adversaire.X, adversaire.Y), robotCurrentLocation), 2);
                        //    }

                        //    if (localWorldMap.ballLocationList.Count > 0)
                        //        robotOrientation = Math.Atan2(localWorldMap.ballLocationList[0].Y - robotCurrentLocation.Y, localWorldMap.ballLocationList[0].X - robotCurrentLocation.X);

                        //    break;

                        //case RoboCupPoste.AttaquantAvecBalle:
                        //    {
                        //        /// L'attaquant avec balle doit aller vers le but si il a de l'espace
                        //        /// Il peut faire des passes à ses coéquipiers démarqués si il est dans une zone un peu dense
                        //        /// 
                        //        /// On va placer un défenseur à une distance définie de l'attaquant, sur la ligne attaquant but
                        //        /// Les zones d'intérêt sont devant les deux attaquants les plus en pointe
                        //        /// Il faut donc commencer par les trouver

                        //        Dictionary<int, TeamMateRoleClassifier> adversaireClassifier = new Dictionary<int, TeamMateRoleClassifier>();
                        //        int i = 0;
                        //        foreach (var adversaire in localWorldMap.obstacleLocationList)
                        //        {
                        //            var adv = new TeamMateRoleClassifier(new PointD(adversaire.X, adversaire.Y), RoboCupPoste.Adversaire);
                        //            adv.DistanceRobotConsidere = Toolbox.Distance(new PointD(adversaire.X, adversaire.Y), new PointD(robotCurrentLocation.X, robotCurrentLocation.Y));
                        //            adversaireClassifier.Add(i++, adv);
                        //        }

                        //        /// A présent, on filtre la liste des adversaires de manière à trouver les joueurs les plus proches du robot considéré
                        //        /// 
                        //        var teamFiltered1 = adversaireClassifier.OrderBy(elt => elt.Value.DistanceRobotConsidere).ToList();

                        //        if (teamFiltered1.Count > 0)
                        //        {
                        //            var adversaireLePlusProche = teamFiltered1[0].Value.Position;
                        //            if (teamFiltered1[0].Value.DistanceRobotConsidere > 2)
                        //            {
                        //                ///On a au moins 2m devant nous, on va vers le but
                        //                ///TODO : raffiner pour ne prendre en compte que les robots entre le but et nous...
                        //                AddPreferedZone(offensiveGoalPosition, 5);
                        //                robotOrientation = Math.Atan2(offensiveGoalPosition.Y - robotCurrentLocation.Y, offensiveGoalPosition.X - robotCurrentLocation.X);
                        //                /// Si on est suffisament proche du but, on tire
                        //                if (Toolbox.Distance(new PointD(robotCurrentLocation.X, robotCurrentLocation.Y), offensiveGoalPosition) < 4)
                        //                {
                        //                    ///On tire !
                        //                    OnShootRequest(robotId, 6);
                        //                }
                        //            }
                        //            else
                        //            {
                        //                ///Il y a du monde en face, on prépare une passe
                        //                Dictionary<int, TeamMateRoleClassifier> teamMateClassifier = new Dictionary<int, TeamMateRoleClassifier>();
                        //                int j = 0;
                        //                //foreach (var teamMateLoc in localWorldMap.teammateLocationList)
                        //                //{
                        //                //    var adv = new TeamMateRoleClassifier(new PointD(adversaire.X, adversaire.Y), RobotRole.Adversaire);
                        //                //    adv.DistanceRobotConsidere = Toolbox.Distance(new PointD(adversaire.X, adversaire.Y), new PointD(robotCurrentLocation.X, robotCurrentLocation.Y));
                        //                //    adversaireClassifier.Add(i++, adv);
                        //                //}
                        //                AddPreferedZone(offensiveGoalPosition, 2);


                        //            }
                        //            //AddPreferredSegmentZoneList(new PointD(adversaire1.X, adversaire1.Y), new PointD(adversaire2.X, adversaire2.Y), 0.4, 0.1);
                        //            //AddAvoidanceZone(new PointD(adversaire1.X, adversaire1.Y), 2, 0.5);

                        //            //AddPreferedZone(new PointD((teamFiltered1[0].Value.Position.X + teamFiltered1[1].Value.Position.X) / 2, (teamFiltered1[0].Value.Position.Y + teamFiltered1[0].Value.Position.Y) / 2), 2.5);
                        //        }
                        //    }

                        break;

                    }
            }
        }

        public override void IterateStateMachines()
        {
        }
        

        /*********************************** Events reçus **********************************************/
        public void OnBallHandlingSensorInfoReceived(object sender, BallHandlingSensorArgs e)
        {
            if (e.RobotId == robotId)
            {
                if (e.IsHandlingBall && taskBallHandlingManagement.state != TaskBallHandlingManagementState.PossessionBalleEnCours)
                    //Force l'état balle prise dans la machine à état de gestion de la prise tir de 
                    taskBallHandlingManagement.SetTaskState(TaskBallHandlingManagementState.PossessionBalle);

            }
            else
                Console.WriteLine("Probleme d'ID robot");
        }

        public override void OnRefBoxMsgReceived(object sender, RefBoxMessageArgs e)
        {
            var command = e.refBoxMsg.command;
            //var robotId = e.refBoxMsg.robotID;
            var targetTeam = e.refBoxMsg.targetTeam;

            switch (command)
            {
                case RefBoxCommand.START:
                    gameState = GameState.PLAYING;
                    stoppedGameAction = StoppedGameAction.NONE;
                    break;
                case RefBoxCommand.STOP:
                    gameState = GameState.STOPPED;
                    break;
                case RefBoxCommand.DROP_BALL:
                    gameState = GameState.STOPPED_GAME_POSITIONING;
                    stoppedGameAction = StoppedGameAction.DROPBALL;
                    break;
                case RefBoxCommand.HALF_TIME:
                    break;
                case RefBoxCommand.END_GAME:
                    break;
                case RefBoxCommand.GAME_OVER:
                    break;
                case RefBoxCommand.PARK:
                    gameState = GameState.STOPPED_GAME_POSITIONING;
                    stoppedGameAction = StoppedGameAction.PARK;
                    break;
                case RefBoxCommand.FIRST_HALF:
                    break;
                case RefBoxCommand.SECOND_HALF:
                    break;
                case RefBoxCommand.FIRST_HALF_OVER_TIME:
                    break;
                case RefBoxCommand.RESET:
                    break;
                case RefBoxCommand.WELCOME:
                    break;
                case RefBoxCommand.KICKOFF:
                    gameState = GameState.STOPPED_GAME_POSITIONING;
                    if (targetTeam == teamIpAddress)
                        stoppedGameAction = StoppedGameAction.KICKOFF;
                    else
                        stoppedGameAction = StoppedGameAction.KICKOFF_OPPONENT;
                    break;
                case RefBoxCommand.FREEKICK:
                    gameState = GameState.STOPPED_GAME_POSITIONING;
                    if (targetTeam == teamIpAddress)
                        stoppedGameAction = StoppedGameAction.FREEKICK;
                    else
                        stoppedGameAction = StoppedGameAction.FREEKICK_OPPONENT;
                    break;
                case RefBoxCommand.GOALKICK:
                    gameState = GameState.STOPPED_GAME_POSITIONING;
                    if (targetTeam == teamIpAddress)
                        stoppedGameAction = StoppedGameAction.GOALKICK;
                    else
                        stoppedGameAction = StoppedGameAction.GOALKICK_OPPONENT;
                    break;
                case RefBoxCommand.THROWIN:
                    gameState = GameState.STOPPED_GAME_POSITIONING;
                    if (targetTeam == teamIpAddress)
                        stoppedGameAction = StoppedGameAction.THROWIN;
                    else
                        stoppedGameAction = StoppedGameAction.THROWIN_OPPONENT;
                    break;
                case RefBoxCommand.CORNER:
                    gameState = GameState.STOPPED_GAME_POSITIONING;
                    if (targetTeam == teamIpAddress)
                        stoppedGameAction = StoppedGameAction.CORNER;
                    else
                        stoppedGameAction = StoppedGameAction.CORNER_OPPONENT;
                    break;
                case RefBoxCommand.PENALTY:
                    gameState = GameState.STOPPED_GAME_POSITIONING;
                    if (targetTeam == teamIpAddress)
                        stoppedGameAction = StoppedGameAction.PENALTY;
                    else
                        stoppedGameAction = StoppedGameAction.PENALTY_OPPONENT;
                    break;
                case RefBoxCommand.GOAL:
                    break;
                case RefBoxCommand.SUBGOAL:
                    break;
                case RefBoxCommand.REPAIR:
                    break;
                case RefBoxCommand.YELLOW_CARD:
                    break;
                case RefBoxCommand.DOUBLE_YELLOW:
                    break;
                case RefBoxCommand.RED_CARD:
                    break;
                case RefBoxCommand.SUBSTITUTION:
                    break;
                case RefBoxCommand.IS_ALIVE:
                    gameState = GameState.STOPPED_GAME_POSITIONING;
                    if (targetTeam == teamIpAddress)
                        stoppedGameAction = StoppedGameAction.KICKOFF;
                    else
                        stoppedGameAction = StoppedGameAction.KICKOFF_OPPONENT;
                    break;
                case RefBoxCommand.GOTO:
                    if (e.refBoxMsg.robotID == robotId)
                    {
                        gameState = GameState.STOPPED_GAME_POSITIONING;
                        //externalRefBoxPosition = new Location(e.refBoxMsg.posX, e.refBoxMsg.posY, e.refBoxMsg.posTheta, 0, 0, 0);
                        if (targetTeam == teamIpAddress)
                            stoppedGameAction = StoppedGameAction.GOTO;
                        else
                            stoppedGameAction = StoppedGameAction.GOTO_OPPONENT;
                    }
                    else
                    {

                    }
                    break;
                case RefBoxCommand.PLAYLEFT:
                    //currentGameState = GameState.STOPPED_GAME_POSITIONING;
                    if (targetTeam == teamIpAddress)
                        playingSide = PlayingSide.Left;
                    else
                        playingSide = PlayingSide.Right;
                    break;
                case RefBoxCommand.PLAYRIGHT:
                    //currentGameState = GameState.STOPPED_GAME_POSITIONING;
                    if (targetTeam == teamIpAddress)
                        playingSide = PlayingSide.Right;
                    else
                        playingSide = PlayingSide.Left;
                    break;
            }
            
        }

        /*********************************** Events de sortie **********************************************/
        public event EventHandler<ShootEventArgs> OnShootRequestEvent;
        public virtual void OnShootRequest(int id, double speed)
        {
            var handler = OnShootRequestEvent;
            if (handler != null)
            {
                handler(this, new ShootEventArgs { RobotId = id, shootingSpeed = speed });
            }
        }
    }

    public class TeamMateRoleClassifier
    {
        public PointD Position;
        public RoboCupPoste Role;
        public double DistanceBalle;
        public double DistanceRobotConsidere;
        public double DistanceButOffensif;
        public double DistanceButDefensif;

        public TeamMateRoleClassifier(PointD position, RoboCupPoste role)
        {
            this.Role = role;
            Position = position;
        }
    }

}
