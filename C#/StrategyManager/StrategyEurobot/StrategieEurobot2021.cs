using Constants;
using EventArgsLibrary;
using HeatMap;
using HerkulexManagerNS;
using LidarProcessor;
using LidarSickNS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using Utilities;
using WorldMap;
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
    public class StrategyEurobot2021 : StrategyGenerique
    {
        public string DisplayName;

        public GameState gameState = GameState.STOPPED;
        public StoppedGameAction stoppedGameAction = StoppedGameAction.NONE;
        public Location externalRefBoxPosition = new Location();
               
        //RoboCupRobotRole role = RoboCupRobotRole.Stopped;
        public RobotType robotType = RobotType.RobotNord;
        System.Timers.Timer configTimer;

        public MatchDescriptor matchDescriptor = new MatchDescriptor();

        public TaskGameManagement taskGameManagement;
        public TaskParametersModifiers taskParametersModifiers;
        public TaskAffichageLidar taskAffichageLidar;

        public Dictionary<string,TaskBrasTurbine> taskBras_dict = new Dictionary<string,TaskBrasTurbine>();
        public TaskBrasDeclencheur taskBrasDeclencheur;
        public TaskBrasDrapeau taskBrasDrapeau;

        public MissionPriseGobelet missionPriseGobelet;
        public MissionDeposeGobelet missionDeposeGobelet;
        public MissionPhare missionPhare;
        public MissionWindFlag missionWindFlag;
        public MissionZoneMouillage missionZoneMouillage;

        public StrategyEurobot2021(int robotId, int teamId, string teamIpAddress) : base(robotId, teamId, teamIpAddress)
        {
            globalWorldMap = new GlobalWorldMap();
            InitHeatMap();
            RayonRobot = 0.16;
        }
        public List<TaskBase> listTasks { get; private set; }

        public List<MissionBase> listMissions { get; private set; }


        public override void InitStrategy(int robotId, int teamId)
        {
            lidarProcessorTIM561 = new LidarProcessor.LidarProcessor(robotId, GameMode.Eurobot);
            
            listTasks = new List<TaskBase>();
            listMissions = new List<MissionBase>();

            ////Taches de bas niveau
            taskBras_dict.Add("Bras_0", new TaskBrasTurbine(this, ServoId.Turbine_0, StrategyManagerEurobotNS.PilotageTurbineID.Turbine_0));
            taskBras_dict.Add("Bras_45", new TaskBrasTurbine(this, ServoId.Turbine_45, StrategyManagerEurobotNS.PilotageTurbineID.Turbine_45));
            taskBras_dict.Add("Bras_135", new TaskBrasTurbine(this, ServoId.Turbine_135, StrategyManagerEurobotNS.PilotageTurbineID.Turbine_135));
            taskBras_dict.Add("Bras_180", new TaskBrasTurbine(this, ServoId.Turbine_180, StrategyManagerEurobotNS.PilotageTurbineID.Turbine_180));
            taskBras_dict.Add("Bras_315", new TaskBrasTurbine(this, ServoId.Turbine_315, StrategyManagerEurobotNS.PilotageTurbineID.Turbine_315));
            taskBrasDeclencheur = new TaskBrasDeclencheur(this, ServoId.Bras_225);
            taskBrasDrapeau = new TaskBrasDrapeau(this, ServoId.PorteDrapeau);
            //Initialisation des taches de la stratégie

            //listTasks.Add(taskTest);
            foreach (var task in taskBras_dict.Values)
                listTasks.Add(task);
            listTasks.Add(taskBrasDeclencheur);
            listTasks.Add(taskBrasDrapeau);

            //Initialisation des missions de jeu
            missionPhare = new MissionPhare(this);
            listMissions.Add(missionPhare);
            missionWindFlag = new MissionWindFlag(this);
            listMissions.Add(missionWindFlag);
            missionPriseGobelet = new MissionPriseGobelet(this);
            listMissions.Add(missionPriseGobelet);
            missionDeposeGobelet = new MissionDeposeGobelet(this);
            listMissions.Add(missionDeposeGobelet);
            missionZoneMouillage = new MissionZoneMouillage(this);
            listMissions.Add(missionZoneMouillage);

            taskGameManagement = new TaskGameManagement(this);
            listTasks.Add(taskGameManagement);
            taskAffichageLidar = new TaskAffichageLidar(this);
            listTasks.Add(taskAffichageLidar);
            taskParametersModifiers = new TaskParametersModifiers(this);
            listTasks.Add(taskParametersModifiers);

            /// Ajout des events
            OnIOValuesFromRobotEvent += OnIOValuesFromRobotEventReceived;
           


            //On initialisae le timer de réglage récurrent 
            //Il permet de modifier facilement les paramètre des asservissement durant l'exécution
            //configTimer = new System.Timers.Timer(1000);
            //configTimer.Elapsed += ConfigTimer_Elapsed;
            //configTimer.Start();

            //Config Eurobot des paramètre embarqués
            OnOdometryPointToMeter(1.211037e-06);
            On4WheelsAngleSet(7.853982e-01, 2.356194e+00, 3.926991e+00, 5.497787e+00);
            On4WheelsToPolarSet(-3.535534e-01, -3.535534e-01, 3.535534e-01, 3.535534e-01,
                                3.535534e-01, -3.535534e-01, -3.535534e-01, 3.535534e-01,
                                1.872659e+00, 1.872659e+00, 1.872659e+00, 1.872659e+00);

            //OnOdometryPointToMeter(1.211037464120243e-06);
            //On4WheelsAngleSet(Toolbox.DegToRad(72), Toolbox.DegToRad(144), Toolbox.DegToRad(216), Toolbox.DegToRad(288));
            //On4WheelsToPolarSet(-3.967532e-01, -2.720655e-01, +2.720655e-01, 3.967532e-01,
            //                 +3.776278e-01, -3.776278e-01, -3.776278e-01, 3.776278e-01,
            //                 +2.106947e+00, +1.341329e+00, +1.341329e+00, +2.106947e+00);

            OnEnableDisableMotorCurrentData(true);
        }

        public bool jackIsPresent = false;
        public enum SideColor { Blue, Yellow };
        public enum RobotType { RobotSud, RobotNord, None };
        public SideColor playingColor = SideColor.Blue;
        public enum StrategyType { Soft, LaConchaDeSuMadre};
        public StrategyType strategyType = StrategyType.Soft;
        public void OnIOValuesFromRobotEventReceived(object sender, IOValuesEventArgs e)
        {
            jackIsPresent = (((e.ioValues >> 0) & 0x01) == 0x00);
            if (((e.ioValues >> 1) & 0x01) == 0x01)
                playingColor = SideColor.Blue;
            else
                playingColor = SideColor.Yellow;

            if (((e.ioValues >> 2) & 0x01) == 0x01)
                robotType = RobotType.RobotNord;
            else
                robotType = RobotType.RobotSud;

            if (((e.ioValues >> 3) & 0x01) == 0x01)
                strategyType = StrategyType.Soft;
            else
                strategyType = StrategyType.LaConchaDeSuMadre;

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
        //private void ConfigTimer_Elapsed(object sender, ElapsedEventArgs e)
        //{
        //    //Ajustement de la distance minimalee de détection des objets
        //    RayonRobot = 0.15;
        //    MovingObstacleAvoidanceDistance = 0.4;

        //    //On envoie périodiquement les réglages du PID de vitesse embarqué
        //    double p = 1.0;
        //    double ki = 50;
            
        //    On4WheelsIndependantSpeedPIDSetup(pM1: p, iM1: ki, 0.0, pM2: p, iM2: ki, 0, pM3: p, iM3: ki, 0, pM4: p, iM4: ki, 0.0,
        //        pM1Limit: 4.0, iM1Limit: 4.0, 0, pM2Limit: 4.0, iM2Limit: 4.0, 0, pM3Limit: 4.0, iM3Limit: 4.0, 0, pM4Limit: 4.0, iM4Limit: 4.0, 0);

            
        //    //On4WheelsPolarSpeedPIDSetup(px: p, ix: 00, 0.0, py: p, iy: 00, 0, ptheta: p, itheta: 00, 0,
        //    //    pxLimit: double.PositiveInfinity, ixLimit: double.PositiveInfinity, 0, pyLimit: double.PositiveInfinity, iyLimit: double.PositiveInfinity, 0, pthetaLimit: double.PositiveInfinity, ithetaLimit: double.PositiveInfinity, 0);

        //    //OnSetAsservissementMode((byte)AsservissementMode.Independant2Wheels);
        //}

        //************************ Events reçus ************************************************/
              
        public override void OnRefBoxMsgReceived(object sender, WorldMap.RefBoxMessageArgs e)
        {
            var command = e.refBoxMsg.command;
            var targetTeam = e.refBoxMsg.targetTeam;
                        
        }

        public override void DetermineRobotRole() //A définir dans les classes héritées
        {
            DefinePlayerZones();

            ///Affichage dynamique des objets sur le terrain
            var l = new PointDExtendedListArgs();
            l.RobotId = robotId;
            l.PtList = new List<PointDExtended>();
            try
            {
                foreach (var elementJeu in matchDescriptor.listElementsJeu)
                {
                    if (elementJeu.Value is Gobelet)
                    {
                        var gobelet = elementJeu.Value as Gobelet;

                        bool gobeletColorOk = false;
                        if (playingColor == SideColor.Blue)
                        {
                            if (gobelet.ReservationToTeam != TeamReservation.ReservedYellow && gobelet.RobotAttributionBlue == robotType)
                                gobeletColorOk = true;
                        }
                        else
                        {
                            if (gobelet.ReservationToTeam != TeamReservation.ReservedBlue && gobelet.RobotAttributionYellow == robotType)
                                gobeletColorOk = true;
                        }

                        if (gobelet.isAvailable && gobeletColorOk)
                        {
                            switch (gobelet.Color)
                            {
                                case Color.Rouge:
                                    l.PtList.Add(new PointDExtended(gobelet.Pos, System.Drawing.Color.Red, 5));
                                    break;
                                case Color.Vert:
                                    if (gobelet.isAvailable)
                                        l.PtList.Add(new PointDExtended(gobelet.Pos, System.Drawing.Color.Green, 5));
                                    break;
                                case Color.Neutre:
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
                foreach (var gobeletTrouve in GobeletsDetectedList)
                {
                    l.PtList.Add(new PointDExtended(new PointD(gobeletTrouve.X, gobeletTrouve.Y), System.Drawing.Color.Blue, 5));
                }
            }
            catch { }

            OnStrategyPtList(this, l);
        }

        public void DefinePlayerZones()
        {
            obstacleFixeList = new List<LocationExtended>();
            /// Définition des zones d'exclusion
            /// Bords du terrain
            AddStrictlyAllowedRectangle(new RectangleD(-1.5 + RayonRobot, 1.5 - RayonRobot, -1 + RayonRobot, 1 - RayonRobot));

            ///Girouette
            AddForbiddenRectangle(new RectangleD(-0.15 - RayonRobot, 0.15 + RayonRobot, 0.97, 1));
            obstacleFixeList.Add(new LocationExtended(-0.1, 0.97, 0, ObjectType.Obstacle));
            obstacleFixeList.Add(new LocationExtended(0.1, 0.97, 0,  ObjectType.Obstacle));

            ///Tasseaux
            AddForbiddenRectangle(new RectangleD(0 - RayonRobot, 0 + RayonRobot, -1, -0.7 + RayonRobot));
            obstacleFixeList.Add(new LocationExtended(0, -0.7, 0, ObjectType.Obstacle));
            obstacleFixeList.Add(new LocationExtended(0, -0.9, 0, ObjectType.Obstacle));
            AddForbiddenRectangle(new RectangleD(-0.6 - RayonRobot, -0.6 + RayonRobot, -1, -0.85 + RayonRobot));
            obstacleFixeList.Add(new LocationExtended(-0.6, -0.85, 0, ObjectType.Obstacle));
            AddForbiddenRectangle(new RectangleD(0.6 - RayonRobot, 0.6 + RayonRobot, -1, -0.85 + RayonRobot));
            obstacleFixeList.Add(new LocationExtended(0.6, -0.85, 0, ObjectType.Obstacle));

            if (playingColor == SideColor.Blue)
            {
                /// Zone de départ jaune
                AddForbiddenRectangle(new RectangleD(1.1, 1.5, - 0.1 - RayonRobot, 0.5 + RayonRobot));
                AddForbiddenRectangle(new RectangleD(1.1 - RayonRobot, 1.1, -0.1, 0.5));
                obstacleFixeList.Add(new LocationExtended(1.1, -0.1, 0, ObjectType.Obstacle));
                obstacleFixeList.Add(new LocationExtended(1.1, 0.5, 0, ObjectType.Obstacle));

                /// Port opposé jaune
                AddForbiddenRectangle(new RectangleD(-0.45-RayonRobot, -0.15+RayonRobot, -1, -0.61));
                AddForbiddenRectangle(new RectangleD(-0.45, -0.15, -0.61, -0.61+RayonRobot));
                obstacleFixeList.Add(new LocationExtended(-0.15, -0.61, 0, ObjectType.Obstacle));
                obstacleFixeList.Add(new LocationExtended(-0.45, -0.61, 0, ObjectType.Obstacle));

            }
            else if(playingColor == SideColor.Yellow)
            {
                /// Zone de départ bleue
                AddForbiddenRectangle(new RectangleD(-1.5, -1.1, -0.1 - RayonRobot, 0.5 + RayonRobot));
                AddForbiddenRectangle(new RectangleD(-1.1, -1.1+RayonRobot, -0.1, 0.5));
                obstacleFixeList.Add(new LocationExtended(-1.1, -0.1, 0, ObjectType.Obstacle));
                obstacleFixeList.Add(new LocationExtended(-1.1, 0.5, 0, ObjectType.Obstacle));

                /// Port opposé bleu
                AddForbiddenRectangle(new RectangleD(0.15 - RayonRobot, 0.45 + RayonRobot, -1, -0.61));
                AddForbiddenRectangle(new RectangleD(0.15, 0.45, -0.61, -0.61 + RayonRobot));
                obstacleFixeList.Add(new LocationExtended(0.15, -0.61, 0, ObjectType.Obstacle));
                obstacleFixeList.Add(new LocationExtended(0.45, -0.61, 0, ObjectType.Obstacle));
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
            //switch (role)
            //{
            //    case RoboCupRobotRole.Positioning:
            //        AddPreferedZone(new PointD(externalRefBoxPosition.X, externalRefBoxPosition.Y), 1.0);
            //        robotOrientation = 0;
            //        break;
            //}
        }

        public override List<LocationExtended> FilterObstacleList(List<LocationExtended> obstacleList)
        {
            var obstacleListInField = obstacleList.Where(pt => Math.Abs(pt.X) < 1.45 && Math.Abs(pt.Y) < 0.95).ToList();
            return obstacleListInField;
        }
         

        public override void IterateStateMachines() //A définir dans les classes héritées
        {
            ;
        }
        public override void InitHeatMap()
        {
            strategyHeatMap = new Heatmap(3.0, 2.0, (int)Math.Pow(2, 7)); //Init HeatMap Strategy Eurobot
            WayPointHeatMap = new Heatmap(3.0, 2.0, (int)Math.Pow(2, 7)); //Init HeatMap WayPoint Eurobot
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

        List<PointD> GobeletsDetectedList = new List<PointD>();
        LidarProcessor.LidarProcessor lidarProcessorTIM561;
        double distanceCentreRobotCentreTIM561 = 0.115;
        public void Lidar_TIM561_PointsAvailable(object sender, RawLidarArgs e)
        {
            var objectsList = lidarProcessorTIM561.DetectionGobelets(e.PtList, distanceMin: 0.15, distanceMax: 2.0, tailleSegmentationObjet:1, tolerance: 0.1);
            var objetsFiltered = objectsList.Where(p => p.Largeur > 0.05 && p.Largeur<0.15).ToList();
            GobeletsDetectedList = new List<PointD>();
            foreach (var obj in objectsList)
            {
                GobeletsDetectedList.Add(new PointD(robotCurrentLocation.X + distanceCentreRobotCentreTIM561*Math.Cos(robotCurrentLocation.Theta)+ obj.DistanceMoyenne * Math.Cos(obj.AngleMoyen + robotCurrentLocation.Theta),
                    robotCurrentLocation.Y + distanceCentreRobotCentreTIM561 * Math.Sin(robotCurrentLocation.Theta) + obj.DistanceMoyenne * Math.Sin(obj.AngleMoyen + robotCurrentLocation.Theta)));
            }
        }
    }
}
