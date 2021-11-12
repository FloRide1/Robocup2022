﻿using Constants;
using EventArgsLibrary;
using HeatMap;
using HerkulexManagerNS;
using LidarProcessor;
using LidarSickNS;
using MessagesNS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using Utilities;
using static HerkulexManagerNS.HerkulexEventArgs;

namespace StrategyManagerNS
{
    /****************************************************************************/
    /// <summary>
    /// Il y a un Strategy Manager par robot, qui partage la même Global World Map -> les stratégies collaboratives sont possibles
    /// Le Strategy Manager a pour rôle de déterminer les déplacements et les actions du robot auquel il appartient
    /// 
    /// Il implante implante à minima le schéma de fonctionnement suivant
    /// - Récupération asynchrone de la Global World Map décrivant l'état du monde autour du robot
    ///     La Global World Map inclus en particulier l'état du jeu (à voir pour changer cela)
    /// - Sur Timer Strategy : détermination si besoin du rôle du robot :
    ///         - simple si Eurobot car les rôles sont figés
    ///         - complexe dans le cas de la RoboCup car les rôles sont changeant en fonction des positions et du contexte.
    /// - Sur Timer Strategy : Itération des machines à état de jeu définissant les déplacements et actions
    ///         - implante les machines à état de jeu à Eurobot, ainsi que les règles spécifiques 
    ///         de jeu (déplacement max en controlant le ballon par exemple à la RoboCup).
    ///         - implante les règles de mise à jour 
    ///             des zones préférentielles de destination (par exemple la balle pour le joueur qui la conteste à la RoboCup), 
    ///             des zones interdites (par exemple les zones de départ à Eurobot), d
    ///             es zones à éviter (par exemple pour se démarquer à la RoboCup)...
    /// - DONE - Sur Timer Strategy : génération de la HeatMap de positionnement X Y donnant l'indication d'intérêt de chacun des points du terrain
    ///     et détermination de la destination théorique (avant inclusion des masquages waypoint)
    /// - DONE - Sur Timer Strategy : prise en compte de la osition des obstacles pour générer la HeatMap de WayPoint 
    ///     et trouver le WayPoint courant.
    /// - Sur Timer Strategy : gestion des actions du robot en fonction du contexte
    ///     Il est à noter que la gestion de l'orientation du robot (différente du cap en déplacement de celui-ci)
    ///     est considérée comme une action, et non comme un déplacement car celle-ci dépend avant tout du contexte du jeu
    ///     et non pas de la manière d'aller à un point.
    /// </summary>

    /****************************************************************************/
    public class StrategyEurobot2022 : StrategyGenerique
    {
        public string DisplayName;

        public GameState gameState = GameState.STOPPED;
        public StoppedGameAction stoppedGameAction = StoppedGameAction.NONE;
        
        public Eurobot2022RobotType robotType = Eurobot2022RobotType.RobotNord;

        LidarProcessor.LidarProcessor lidarProcessorTIM561;

        public Eurobot2022MatchDescriptor matchDescriptor;

        public Eurobot2022TaskGameManagement taskGameManagement;
        public Eurobot2022TaskDeplacementsParametersModifiers taskDeplacementsParametersModifiers;
        public Eurobot2022TaskAvoidanceParametersModifiers taskAvoidanceParametersModifiers;
        public Eurobot2022TaskAffichageLidar taskAffichageLidar;

        public Dictionary<string,Eurobot2022TaskBrasTurbine> taskBras_dict = new Dictionary<string,Eurobot2022TaskBrasTurbine>();
        public Eurobot2022TaskBrasDeclencheur taskBrasDeclencheur;
        public Eurobot2022TaskBrasDrapeau taskBrasDrapeau;

        public Eurobot2022MissionPriseGobelet missionPriseGobelet;
        public Eurobot2022MissionDeposeGobelet missionDeposeGobelet;
        public Eurobot2022MissionPhare missionPhare;
        public Eurobot2022MissionWindFlag missionWindFlag;
        public Eurobot2022MissionZoneMouillage missionZoneMouillage;
        public Eurobot2022MissionRamassageAutomatique missionRamassageAutomatique;

        public Eurobot2022MissionLCDSM missionLCDSM;

        public StrategyEurobot2022(int robotId, int teamId, string teamIpAddress) : base(robotId, teamId, teamIpAddress)
        {
            matchDescriptor = new Eurobot2022MatchDescriptor(this);
            RayonRobot = 0.15;
        }
        
        public List<TaskBase> listTasks { get; private set; }
        public List<MissionBase> listMissions { get; private set; }

        public override void InitStrategy(int robotId, int teamId)
        {
            lidarProcessorTIM561 = new LidarProcessor.LidarProcessor(robotId, GameMode.Eurobot);
            listTasks = new List<TaskBase>();
            listMissions = new List<MissionBase>();

            ////Taches de bas niveau
            taskBras_dict.Add("Bras_0", new Eurobot2022TaskBrasTurbine(this, ServoId.Turbine_0, StrategyManagerEurobotNS.Eurobot2022PilotageTurbineID.Turbine_0));
            taskBras_dict.Add("Bras_45", new Eurobot2022TaskBrasTurbine(this, ServoId.Turbine_45, StrategyManagerEurobotNS.Eurobot2022PilotageTurbineID.Turbine_45));
            taskBras_dict.Add("Bras_135", new Eurobot2022TaskBrasTurbine(this, ServoId.Turbine_135, StrategyManagerEurobotNS.Eurobot2022PilotageTurbineID.Turbine_135));
            taskBras_dict.Add("Bras_180", new Eurobot2022TaskBrasTurbine(this, ServoId.Turbine_180, StrategyManagerEurobotNS.Eurobot2022PilotageTurbineID.Turbine_180));
            taskBras_dict.Add("Bras_315", new Eurobot2022TaskBrasTurbine(this, ServoId.Turbine_315, StrategyManagerEurobotNS.Eurobot2022PilotageTurbineID.Turbine_315));
            taskBrasDeclencheur = new Eurobot2022TaskBrasDeclencheur(this, ServoId.Bras_225);
            taskBrasDrapeau = new Eurobot2022TaskBrasDrapeau(this, ServoId.PorteDrapeau);
            //Initialisation des taches de la stratégie

            //listTasks.Add(taskTest);
            foreach (var task in taskBras_dict.Values)
                listTasks.Add(task);
            listTasks.Add(taskBrasDeclencheur);
            listTasks.Add(taskBrasDrapeau);

            //Initialisation des missions de jeu
            missionPhare = new Eurobot2022MissionPhare(this);
            listMissions.Add(missionPhare);
            missionWindFlag = new Eurobot2022MissionWindFlag(this);
            listMissions.Add(missionWindFlag);
            missionPriseGobelet = new Eurobot2022MissionPriseGobelet(this);
            listMissions.Add(missionPriseGobelet);
            missionDeposeGobelet = new Eurobot2022MissionDeposeGobelet(this);
            listMissions.Add(missionDeposeGobelet);
            missionZoneMouillage = new Eurobot2022MissionZoneMouillage(this);
            listMissions.Add(missionZoneMouillage);
            missionLCDSM = new Eurobot2022MissionLCDSM(this);
            listMissions.Add(missionLCDSM);
            missionRamassageAutomatique = new Eurobot2022MissionRamassageAutomatique(this);
            listMissions.Add(missionRamassageAutomatique);

            taskGameManagement = new Eurobot2022TaskGameManagement(this);
            listTasks.Add(taskGameManagement);
            taskAffichageLidar = new Eurobot2022TaskAffichageLidar(this);
            listTasks.Add(taskAffichageLidar);
            taskDeplacementsParametersModifiers = new Eurobot2022TaskDeplacementsParametersModifiers(this);
            listTasks.Add(taskDeplacementsParametersModifiers);
            taskAvoidanceParametersModifiers = new Eurobot2022TaskAvoidanceParametersModifiers(this);
            listTasks.Add(taskAvoidanceParametersModifiers);
            //taskHerkulexCouple = new TaskHerkulexCouple(this);
            //listTasks.Add(taskHerkulexCouple);

            /// Ajout des events
            OnIOValuesFromRobotEvent += OnIOValuesFromRobotEventReceived;
           
            //Config Eurobot des paramètre embarqués
            OnOdometryPointToMeter(1.211037e-06);
            On4WheelsAngleSet(7.853982e-01, 2.356194e+00, 3.926991e+00, 5.497787e+00);
            On4WheelsToPolarSet(-3.535534e-01, -3.535534e-01, 3.535534e-01, 3.535534e-01,
                                3.535534e-01, -3.535534e-01, -3.535534e-01, 3.535534e-01,
                                1.872659e+00, 1.872659e+00, 1.872659e+00, 1.872659e+00);

            OnEnableDisableMotorCurrentData(true);
            OnEnableDisableRxForwardFromHerkulex(true);
        }

        public bool jackIsPresent = false;
        public enum Eurobot2022SideColor { Blue, Yellow };
        public enum Eurobot2022RobotType { RobotSud, RobotNord, None };
        public Eurobot2022SideColor playingColor = Eurobot2022SideColor.Blue;
        public enum Eurobot2022StrategyType { Soft, LaConchaDeSuMadre };
        public Eurobot2022StrategyType strategyType = Eurobot2022StrategyType.Soft;
        public void OnIOValuesFromRobotEventReceived(object sender, IOValuesEventArgs e)
        {
            jackIsPresent = (((e.ioValues >> 0) & 0x01) == 0x00);
            if (((e.ioValues >> 1) & 0x01) == 0x01)
                playingColor = Eurobot2022SideColor.Blue;
            else
                playingColor = Eurobot2022SideColor.Yellow;

            if (((e.ioValues >> 2) & 0x01) == 0x01)
                robotType = Eurobot2022RobotType.RobotNord;
            else
                robotType = Eurobot2022RobotType.RobotSud;

            if (((e.ioValues >> 3) & 0x01) == 0x01)
                strategyType = Eurobot2022StrategyType.LaConchaDeSuMadre;
            else
                strategyType = Eurobot2022StrategyType.Soft;

            //bool config2 = (((e.ioValues >> 2) & 0x01) == 0x01);

            //bool config3 = (((e.ioValues >> 3) & 0x01) == 0x01);
            bool config4 = (((e.ioValues >> 4) & 0x01) == 0x01);
        }

        double distanceValidationDestination = 0.03;
        public bool isDeplacementFinished
        {
            get
            {
                if (Math.Abs(robotOrientation - Toolbox.ModuloByAngle(robotOrientation, robotCurrentLocation.Theta)) < Toolbox.DegToRad(1.0) &&
                    Toolbox.Distance(new PointD(robotCurrentLocation.X, robotCurrentLocation.Y), robotDestination) < distanceValidationDestination)
                    return true;
                else
                    return false;
            }
            private set
            {

            }
        }

        //************************ Events reçus ************************************************/

        public override void OnRefBoxMsgReceived(object sender, RefBoxMessageArgs e)
        {
            var command = e.refBoxMsg.command;
            var targetTeam = e.refBoxMsg.targetTeam;

        }

        public override void DetermineRobotRole() //A définir dans les classes héritées
        {
            ///Affichage dynamique des objets sur le terrain
            var l = new PointDExtendedListArgs();
            l.RobotId = robotId;
            l.PtList = new List<PointDExtended>();
            try
            {
                foreach (var elementJeu in matchDescriptor.listElementsJeu)
                {
                    if (elementJeu.Value is Eurobot2022Gobelet)
                    {
                        var gobelet = elementJeu.Value as Eurobot2022Gobelet;

                        bool gobeletColorOk = false;
                        if (playingColor == Eurobot2022SideColor.Blue)
                        {
                            if (gobelet.ReservationToTeam != Eurobot2022TeamReservation.ReservedYellow && gobelet.RobotAttributionBlue == robotType)
                                gobeletColorOk = true;
                        }
                        else
                        {
                            if (gobelet.ReservationToTeam != Eurobot2022TeamReservation.ReservedBlue && gobelet.RobotAttributionYellow == robotType)
                                gobeletColorOk = true;
                        }

                        if (gobelet.isAvailable && gobeletColorOk)
                        {
                            switch (gobelet.Color)
                            {
                                case Eurobot2022Color.Rouge:
                                    l.PtList.Add(new PointDExtended(gobelet.Pos, System.Drawing.Color.Red, 5));
                                    break;
                                case Eurobot2022Color.Vert:
                                    if (gobelet.isAvailable)
                                        l.PtList.Add(new PointDExtended(gobelet.Pos, System.Drawing.Color.Green, 5));
                                    break;
                                case Eurobot2022Color.Neutre:
                                    if (gobelet.isAvailable)
                                        l.PtList.Add(new PointDExtended(gobelet.Pos, System.Drawing.Color.Gray, 5));
                                    break;
                            }
                        }
                        else
                        {
                            l.PtList.Add(new PointDExtended(gobelet.Pos, System.Drawing.Color.Black, 5));
                        }
                    }
                }
                foreach (var gobeletTrouve in GobeletsPotentielsRefTerrain)
                {
                    l.PtList.Add(new PointDExtended(gobeletTrouve.Pos, System.Drawing.Color.Blue, 5));
                }
            }
            catch { }

            OnStrategyPtList(this, l);
        }

        public override void DetermineRobotZones() //A définir dans les classes héritées        
        {
            obstacleFixeList = new List<LocationExtended>();
            /// Définition des zones d'exclusion
            /// Bords du terrain
            AddStrictlyAllowedRectangle(new RectangleD(-1.51 + RayonRobot, 1.51 - RayonRobot, -1 + RayonRobot, 1 - RayonRobot));

            ///Girouette
            AddForbiddenRectangle(new RectangleD(-0.15 - RayonRobot, 0.15 + RayonRobot, 0.97, 1));
            obstacleFixeList.Add(new LocationExtended(-0.1, 0.97, 0, ObjectType.Obstacle));
            obstacleFixeList.Add(new LocationExtended(0.1, 0.97, 0, ObjectType.Obstacle));

            ///Tasseaux
            AddForbiddenRectangle(new RectangleD(0 - RayonRobot, 0 + RayonRobot, -1, -0.7 + RayonRobot));
            obstacleFixeList.Add(new LocationExtended(0, -0.7, 0, ObjectType.Obstacle));
            obstacleFixeList.Add(new LocationExtended(0, -0.9, 0, ObjectType.Obstacle));
            AddForbiddenRectangle(new RectangleD(-0.6 - RayonRobot, -0.6 + RayonRobot, -1, -0.85 + RayonRobot));
            obstacleFixeList.Add(new LocationExtended(-0.6, -0.85, 0, ObjectType.Obstacle));
            AddForbiddenRectangle(new RectangleD(0.6 - RayonRobot, 0.6 + RayonRobot, -1, -0.85 + RayonRobot));
            obstacleFixeList.Add(new LocationExtended(0.6, -0.85, 0, ObjectType.Obstacle));

            if (playingColor == Eurobot2022SideColor.Blue)
            {
                /// Zone de départ jaune
                AddForbiddenRectangle(new RectangleD(0.98, 1.5, -0.1 - RayonRobot, 0.5 + RayonRobot));
                AddForbiddenRectangle(new RectangleD(0.98 - RayonRobot, 0.98, -0.1, 0.5));
                obstacleFixeList.Add(new LocationExtended(0.98, -0.1, 0, ObjectType.Obstacle));
                obstacleFixeList.Add(new LocationExtended(1.2, -0.1, 0, ObjectType.Obstacle));
                obstacleFixeList.Add(new LocationExtended(1.4, -0.1, 0, ObjectType.Obstacle));
                obstacleFixeList.Add(new LocationExtended(0.98, 0.5, 0, ObjectType.Obstacle));
                obstacleFixeList.Add(new LocationExtended(1.2, 0.5, 0, ObjectType.Obstacle));
                obstacleFixeList.Add(new LocationExtended(1.4, 0.5, 0, ObjectType.Obstacle));

                /// Port opposé jaune
                AddForbiddenRectangle(new RectangleD(-0.45 - RayonRobot, -0.15 + RayonRobot, -1, -0.58));
                AddForbiddenRectangle(new RectangleD(-0.45, -0.15, -0.58, -0.58 + RayonRobot));
                obstacleFixeList.Add(new LocationExtended(-0.15, -0.58, 0, ObjectType.Obstacle));
                obstacleFixeList.Add(new LocationExtended(-0.45, -0.58, 0, ObjectType.Obstacle));

            }
            else if (playingColor == Eurobot2022SideColor.Yellow)
            {
                /// Zone de départ bleue
                AddForbiddenRectangle(new RectangleD(-1.5, -0.98, -0.1 - RayonRobot, 0.5 + RayonRobot));
                AddForbiddenRectangle(new RectangleD(-0.98, -0.98 + RayonRobot, -0.1, 0.5));
                obstacleFixeList.Add(new LocationExtended(-0.98, -0.1, 0, ObjectType.Obstacle));
                obstacleFixeList.Add(new LocationExtended(-1.2, -0.1, 0, ObjectType.Obstacle));
                obstacleFixeList.Add(new LocationExtended(-1.4, -0.1, 0, ObjectType.Obstacle));
                obstacleFixeList.Add(new LocationExtended(-0.98, 0.5, 0, ObjectType.Obstacle));
                obstacleFixeList.Add(new LocationExtended(-1.2, 0.5, 0, ObjectType.Obstacle));
                obstacleFixeList.Add(new LocationExtended(-1.4, 0.5, 0, ObjectType.Obstacle));

                /// Port opposé bleu
                AddForbiddenRectangle(new RectangleD(0.15 - RayonRobot, 0.45 + RayonRobot, -1, -0.58));
                AddForbiddenRectangle(new RectangleD(0.15, 0.45, -0.58, -0.58 + RayonRobot));
                obstacleFixeList.Add(new LocationExtended(0.15, -0.58, 0, ObjectType.Obstacle));
                obstacleFixeList.Add(new LocationExtended(0.45, -0.58, 0, ObjectType.Obstacle));
            }

            /// Ajout d'obstacles autour des gobelets déjà déposés
            var listEmplacementDeposeOccupe = matchDescriptor.DictionaryEmplacementDepose.Where(p => p.Value.IsAvailable == false);
            try
            {
                foreach (var depose in listEmplacementDeposeOccupe)
                {
                    obstacleFixeList.Add(new LocationExtended(depose.Value.Pos.X, depose.Value.Pos.Y, 0, ObjectType.Obstacle));
                }
            }
            catch
            { }
        }

        public override List<LocationExtended> FilterObstacleList(List<LocationExtended> obstacleList)
        {
            //var obstacleListInField = obstacleList.Where(pt => Math.Abs(pt.X) < 1.48 && Math.Abs(pt.Y) < 0.98).ToList();
            //return obstacleListInField;
            return obstacleList;
        }

        public override void IterateStateMachines() //A définir dans les classes héritées
        {
            ;
        }
        public override void InitHeatMap()
        {
            strategyHeatMap = new Heatmap(3.0, 2.0, (int)Math.Pow(2, 7), robotId); //Init HeatMap Strategy Eurobot
            WayPointHeatMap = new Heatmap(3.0, 2.0, (int)Math.Pow(2, 7), robotId); //Init HeatMap WayPoint Eurobot
        }


        /****************************************** Events envoyés ***********************************************/

        public event EventHandler<TrajectoryGeneratorConstants> OnTrajectoryConstantsEvent;
        public virtual void OnTrajectoryConstants(object sender, TrajectoryGeneratorConstants val)
        {
            OnTrajectoryConstantsEvent?.Invoke(sender, val);
        }

        public event EventHandler<MotorsCurrentsEventArgs> OnMotorCurrentReceiveForwardEvent;
        public void OnMotorCurrentReceive(object sender, MotorsCurrentsEventArgs e)
        {
            //Forward event to task on low level
            OnMotorCurrentReceiveForwardEvent?.Invoke(sender, e);
        }

        public List<Eurobot2022GobeletPotentiel> GobeletsPotentielsRefTerrain = new List<Eurobot2022GobeletPotentiel>();

        double distanceCentreRobotCentreTIM561 = 0.115;
        public void Lidar_TIM561_PointsAvailable(object sender, RawLidarArgs e)
        {
            var objectsList = lidarProcessorTIM561.DetectionGobelets(e.PtList, distanceMin: 0.3, distanceMax: 2.0, tailleSegmentationObjet: 1, tolerance: 0.1);
            var objetsFiltered = objectsList.Where(p => p.Largeur > 0.05 && p.Largeur<0.15).ToList();
            GobeletsPotentielsRefTerrain = new List<Eurobot2022GobeletPotentiel>();
            foreach (var obj in objetsFiltered)
            {
                /// Filtrage par position des gobelets détectés, de manière à prendre seulement dans un rectanlge donné
                var objPositionRefTerrain = new PointD(robotCurrentLocation.X + distanceCentreRobotCentreTIM561 * Math.Cos(robotCurrentLocation.Theta) + obj.DistanceMoyenne * Math.Cos(obj.AngleMoyen + robotCurrentLocation.Theta),
                                    robotCurrentLocation.Y + distanceCentreRobotCentreTIM561 * Math.Sin(robotCurrentLocation.Theta) + obj.DistanceMoyenne * Math.Sin(obj.AngleMoyen + robotCurrentLocation.Theta));

                if (Math.Abs(objPositionRefTerrain.X) < 0.9 && objPositionRefTerrain.Y > -0.55 && objPositionRefTerrain.Y < 0.95) //Filtrage Zone de ramassage possible
                {
                    GobeletsPotentielsRefTerrain.Add(
                    new Eurobot2022GobeletPotentiel(objPositionRefTerrain, obj.Largeur, obj.RssiMoyen, obj.RssiCentral, obj.RssiStdDev));
                }
            }
        }

        public Dictionary<int, Int16> HerkulexMesuredTorque = new Dictionary<int, Int16>();

        public void OnTorqueInfo(object sender, TorqueEventArgs e)
        {
            if (HerkulexMesuredTorque.ContainsKey(e.servoID))
                HerkulexMesuredTorque[e.servoID] = (Int16)e.Value;
            else
                HerkulexMesuredTorque.Add(e.servoID, (Int16)e.Value);
            //Console.WriteLine("Torque ID " + e.servoID + "=" + e.Value.ToString("X2"));
        }
    }

    public class Eurobot2022GobeletPotentiel
    {
        public PointD Pos;
        public double Largeur;
        public double RssiMoyen;
        public double RssiCentral;
        public double RssiStdDev;

        public Eurobot2022GobeletPotentiel(PointD pos, double largeur, double rssiMoyen, double rssiCentral, double rssiStdDev)
        {
            Pos = pos;
            Largeur = largeur;
            RssiMoyen = rssiMoyen;
            RssiCentral = rssiCentral;
            RssiStdDev = rssiStdDev;
        }
    }
}