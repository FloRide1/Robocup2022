﻿using Constants;
using EventArgsLibrary;
using HeatMap;
using HerkulexManagerNS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
    public abstract class StrategyGenerique
    {
        public int robotId = 0;
        public int teamId = 0;
        public string teamIpAddress = "";
        public string DisplayName;

        //public GlobalWorldMap globalWorldMap;
        public LocalWorldMap localWorldMap;
        public Heatmap WayPointHeatMap;
        public Heatmap strategyHeatMap;
        public double HeatMapPrecision;
        public Location robotCurrentLocation = new Location(0, 0, 0, 0, 0, 0);
        public PointD robotDestination { get; private set; }
        public double robotOrientation;

        public List<LocationExtended> obstacleFixeList = new List<LocationExtended>();
        /// <summary>
        /// Liste d'obstacles additionnels permettant d'interdire dynamiquement certaines zones en cours de match
        /// </summary>
        public List<LocationExtended> obstacleFixeListAdditional = new List<LocationExtended>();

        public double RayonRobot { get; set; }

        public double MovingObstacleAvoidanceDistance { get; set; }
        public double FixedObstacleAvoidanceDistance { get; set; }


        Stopwatch sw = new Stopwatch();
        Stopwatch swGlobal = new Stopwatch();
        System.Timers.Timer timerStrategy;

        public StrategyGenerique(int robotId, int teamId, string teamIpAddress)
        {
            this.teamId = teamId;
            this.robotId = robotId;
            this.teamIpAddress = teamIpAddress;

            //globalWorldMap = new GlobalWorldMap();
            localWorldMap = new LocalWorldMap();

            InitHeatMap();

            timerStrategy = new System.Timers.Timer();
            timerStrategy.Interval = 50;
            timerStrategy.Elapsed += TimerStrategy_Elapsed;
            timerStrategy.Start();
        }

        public abstract void InitStrategy(int robotId, int teamId);

        public abstract void InitHeatMap();

        public abstract List<LocationExtended> FilterObstacleList(List<LocationExtended> obstacleList);

        public double SetRobotDestination(PointD destination, double orientation)
        {
            /// Renvoie l'estimation de timeout pour le déplacement considéré
            robotDestination = destination;
            robotOrientation = orientation;

            double vitesseLineaireIndicative = 0.4; ///bof bof mais il faudrait la remplacer par la vitesse lineaire avec un coeff de sécurité
            double vitesseAngulaireIndicative = 0.4; ///bof bof mais il faudrait la remplacer par la vitesse Angulaire avec un coeff de sécurité

            double estimationTimeoutDistance = Toolbox.Distance(destination, new PointD(robotCurrentLocation.X, robotCurrentLocation.Y))/vitesseLineaireIndicative;
            double estimationTimeoutAngle = Math.Abs(Toolbox.Modulo2PiAngleRad(orientation - robotCurrentLocation.Theta))/vitesseAngulaireIndicative;
            
            return Math.Max(1000, 1000*Math.Max(estimationTimeoutDistance, estimationTimeoutAngle));
        }

        //************************ Events reçus ************************************************/
        public abstract void OnRefBoxMsgReceived(object sender, WorldMap.RefBoxMessageArgs e);

        /////Event de récupération d'une GlobalWorldMap mise à jour
        //public void OnGlobalWorldMapReceived(object sender, GlobalWorldMapArgs e)
        //{
        //    //On récupère la nouvelle global worldMap
        //    lock (globalWorldMap)
        //    {
        //        globalWorldMap = e.GlobalWorldMap;
        //    }
        //}

        ///Event de récupération d'une GlobalWorldMap mise à jour
        public void OnLocalWorldMapReceived(object sender, LocalWorldMapArgs e)
        {
            //On récupère la nouvelle global worldMap
            lock (localWorldMap)
            {
                localWorldMap = e.LocalWorldMap;
            }
        }

        public void OnPositionRobotReceived(object sender, LocationArgs location)
        {
            robotCurrentLocation.X = location.Location.X;
            robotCurrentLocation.Y = location.Location.Y;
            robotCurrentLocation.Theta = location.Location.Theta;

            robotCurrentLocation.Vx = location.Location.Vx;
            robotCurrentLocation.Vy = location.Location.Vy;
            robotCurrentLocation.Vtheta = location.Location.Vtheta;
        }        

        bool displayConsole = false;
        object strategyLock = new object();
        bool isLocked = false;
        private void TimerStrategy_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (isLocked == false)
            {
                lock (strategyLock)
                {
                    isLocked = true;
                    InitRobotRoleDeterminationZones();

                    /// On ajoute une zone préférée autour du point RobotDestination
                    preferredZonesList.Add(new Zone(robotDestination, 3.0, 1.0));
                    
                    /// On commence par définir le rôle du robot
                    DetermineRobotRole();  //if (displayConsole) Console.WriteLine("Tps calcul détermination des rôles : " + sw.Elapsed.TotalMilliseconds.ToString("N4") + " ms"); sw.Restart();

                    /// On défini ensuite les zones autorisées et préférées du robot
                    DetermineRobotZones();

                    /// Itération sur les machines à états
                    IterateStateMachines(); //if (displayConsole) Console.WriteLine("Tps calcul State machines : " + sw.Elapsed.TotalMilliseconds.ToString("N4") + " ms");  sw.Restart();

                    GenerateStrategyHeatMap(); //if (displayConsole) Console.WriteLine("Tps calcul Heatmap Destination : " + sw.Elapsed.TotalMilliseconds.ToString("N4") + " ms");  sw.Restart();
                    var optimalPosition = GetOptimalStrategyDestination(); //if (displayConsole) Console.WriteLine("Tps calcul Get Optimal Destination : " + sw.Elapsed.TotalMilliseconds.ToString("N4") + " ms"); sw.Restart();

                    /// On regarde si la position optimale sur la map est très proche de la robotDestination, auquel cas la précision est limitée par le pas de discrétisation de la map
                    /// et on prend la robot Destination comme cible
                    if (optimalPosition != null && robotDestination != null)
                    {
                        if (Toolbox.Distance(optimalPosition, robotDestination) < strategyHeatMap.Precision)
                            optimalPosition = robotDestination;
                    }

                    List<LocationExtended> obstacleList = new List<LocationExtended>();

                    /////Construction de la liste des obstacles dynamiques en enlevant le robot lui-même
                    //lock (globalWorldMap)
                    //{
                    //    if (globalWorldMap.obstacleLocationList != null)
                    //    {
                    //        foreach (var obstacle in globalWorldMap.obstacleLocationList)
                    //        {
                    //            if (Toolbox.Distance(new PointD(obstacle.X, obstacle.Y), new PointD(robotCurrentLocation.X, robotCurrentLocation.Y)) > RayonRobot)
                    //                obstacleList.Add(obstacle);
                    //        }
                    //    }
                    //    if (globalWorldMap.teammateLocationList != null)
                    //    {
                    //        foreach (var teammate in globalWorldMap.teammateLocationList)
                    //        {
                    //            obstacleList.Add(new LocationExtended(teammate.X, teammate.Y, 0, 0, 0, 0, ObjectType.Obstacle)); ///On considère tous les robots et obstacles comme des obstacles
                    //        }
                    //    }
                    //}

                    ///Construction de la liste des obstacles dynamiques en enlevant le robot lui-même
                    lock (localWorldMap)
                    {
                        if (localWorldMap.obstacleLocationList != null)
                        {
                            foreach (var obstacle in localWorldMap.obstacleLocationList)
                            {
                                if (Toolbox.Distance(new PointD(obstacle.X, obstacle.Y), new PointD(robotCurrentLocation.X, robotCurrentLocation.Y)) > RayonRobot)
                                    obstacleList.Add(obstacle);
                            }
                        }
                        if (localWorldMap.teammateLocationList != null)
                        {
                            foreach (var teammate in localWorldMap.teammateLocationList)
                            {
                                obstacleList.Add(new LocationExtended(teammate.X, teammate.Y, 0, 0, 0, 0, ObjectType.Obstacle)); ///On considère tous les robots et obstacles comme des obstacles
                            }
                        }
                    }

                    // if (displayConsole) Console.WriteLine("Tps calcul Génération obstacles : " + sw.Elapsed.TotalMilliseconds.ToString("N4") + " ms"); sw.Restart();
                    OnHeatMapStrategy(robotId, strategyHeatMap); // if (displayConsole) Console.WriteLine("Tps envoi strat Heatmap : " + sw.Elapsed.TotalMilliseconds.ToString("N4") + " ms"); sw.Restart();                    
                    
                    ///On copie la heatmap de strategy dans la heatmap de waypoint pour la compléter sans abimer la première
                    CopyStrategyHeatMapToWayPointHeatMap();

                    /// Calcul de la HeatMap WayPoint
                    var obstacleListInField = FilterObstacleList(obstacleList);
                    WayPointHeatMap.ExcludeMaskedZones(new PointD(robotCurrentLocation.X, robotCurrentLocation.Y), obstacleListInField, MovingObstacleAvoidanceDistance, robotCurrentLocation); //if (displayConsole) Console.WriteLine("Tps calcul zones exclusion obstacles : " + sw.Elapsed.TotalMilliseconds.ToString("N4") + " ms");  sw.Restart();
                    WayPointHeatMap.ExcludeMaskedZones(new PointD(robotCurrentLocation.X, robotCurrentLocation.Y), obstacleFixeList, FixedObstacleAvoidanceDistance, robotCurrentLocation); //if (displayConsole) Console.WriteLine("Tps calcul zones exclusion obstacles : " + sw.Elapsed.TotalMilliseconds.ToString("N4") + " ms");  sw.Restart();

                    OnHeatMapWayPoint(robotId, WayPointHeatMap); //if (displayConsole) Console.WriteLine("Tps calcul HeatMap WayPoint : " + sw.Elapsed.TotalMilliseconds.ToString("N4") + " ms"); sw.Restart();
                    var optimalWayPoint = GetOptimalWayPointDestination(); //if (displayConsole) Console.WriteLine("Tps calcul Get Optimal Waypoint : " + sw.Elapsed.TotalMilliseconds.ToString("N4") + " ms"); sw.Restart();

                    /// On regarde si la position optimale sur la map est très proche de la robotDestination, auquel cas la précision est limitée par le pas de discrétisation de la map
                    /// et on prend la robot Destination comme cible
                    if (optimalWayPoint != null && robotDestination != null)
                    {
                        if (Toolbox.Distance(optimalWayPoint, robotDestination) < WayPointHeatMap.Precision)
                            optimalWayPoint = robotDestination;
                    }

                    //Mise à jour de la destination
                    if (optimalPosition == null)
                        OnDestination(robotId, new Location((float)robotCurrentLocation.X, (float)robotCurrentLocation.Y, (float)robotOrientation, 0, 0, 0));
                    else
                        OnDestination(robotId, new Location((float)optimalPosition.X, (float)optimalPosition.Y, (float)robotOrientation, 0, 0, 0));

                    if (optimalWayPoint == null)
                        OnWaypoint(robotId, new Location((float)robotCurrentLocation.X, (float)robotCurrentLocation.Y, (float)robotOrientation, 0, 0, 0));
                    else
                        OnWaypoint(robotId, new Location((float)optimalWayPoint.X, (float)optimalWayPoint.Y, (float)robotOrientation, 0, 0, 0));

                    //if (displayConsole) Console.WriteLine("Tps events waypoint et destination : " + sw.Elapsed.TotalMilliseconds.ToString("N4") + " ms"); // Affichage de la mesure
                    //if (displayConsole) Console.WriteLine("Tps calcul Global Stratégie : " + swGlobal.Elapsed.TotalMilliseconds.ToString("N4") + " ms \n\n"); // Affichage de la mesure globale
                    //Thread.Sleep(100);

                    OnUpdateWorldMapDisplay(robotId);

                    isLocked = false;
                }
            }
            else
            {
                Console.WriteLine("Calcul de strategie déjà en cours");
            }
        }

        private void InitRobotRoleDeterminationZones()
        {
            InitPreferedZones();
            InitAvoidanceZones();
            InitForbiddenRectangleList();
            InitStrictlyAllowedRectangleList();
            InitPreferredRectangleList();
            InitAvoidanceConicalZoneList();
            InitPreferredSegmentZoneList();
            InitStrictlyAllowedConvexPolygonList();
        }


        public abstract void DetermineRobotRole(); //A définir dans les classes héritées
        public abstract void DetermineRobotZones(); //A définir dans les classes héritées

        public abstract void IterateStateMachines(); //A définir dans les classes héritées

        private void GenerateStrategyHeatMap()
        {
            //Génération de la HeatMap            
            strategyHeatMap.GenerateHeatMap(preferredZonesList, avoidanceZonesList, forbiddenRectangleList, 
                strictlyAllowedRectangleList, preferredRectangleList, avoidanceConicalZoneList, preferredSegmentZoneList, strictlyAllowedConvexPolygonList);
        }

        public PointD GetOptimalStrategyDestination()
        {
            PointD optimalPosition = strategyHeatMap.GetOptimalPosition();
            return optimalPosition;
        }

        public PointD GetOptimalWayPointDestination()
        {
            PointD optimalPosition = WayPointHeatMap.GetOptimalPosition();
            return optimalPosition;
        }

        public void CopyStrategyHeatMapToWayPointHeatMap()
        { 
            for(int i=0; i<strategyHeatMap.nbCellInBaseHeatMapHeight; i++)
            {
                for (int j = 0; j < strategyHeatMap.nbCellInBaseHeatMapWidth; j++)
                    WayPointHeatMap.BaseHeatMapData[i,j] = strategyHeatMap.BaseHeatMapData[i,j];
            }
        }


        //Zones circulaires préférentielles
        List<Zone> preferredZonesList = new List<Zone>();
        public void InitPreferedZones()
        {
            lock (preferredZonesList)
            {
                preferredZonesList = new List<Zone>();
            }
        }
        public void AddPreferedZone(PointD location, double radius, double strength=1)
        {
            lock (preferredZonesList)
            {
                preferredZonesList.Add(new Zone(location, radius, strength));
            }
        }

        //Zones circulaires à éviter
        List<Zone> avoidanceZonesList = new List<Zone>();
        public void InitAvoidanceZones()
        {
            lock (avoidanceZonesList)
            {
                avoidanceZonesList = new List<Zone>();
            }
        }
        public void AddAvoidanceZone(PointD location, double radius, double strength=1)
        {
            lock (avoidanceZonesList)
            {
                avoidanceZonesList.Add(new Zone(location, radius, strength));
            }
        }

        //Zones rectangulaires interdites
        List<RectangleZone> forbiddenRectangleList = new List<RectangleZone>();
        public void InitForbiddenRectangleList()
        {
            lock (forbiddenRectangleList)
            {
                forbiddenRectangleList = new List<RectangleZone>();
            }
        }
        public void AddForbiddenRectangle(RectangleD rect)
        {
            lock (forbiddenRectangleList)
            {
                forbiddenRectangleList.Add(new RectangleZone(rect));
            }
        }

        //Zones coniques déconseillée
        List<ConicalZone> avoidanceConicalZoneList = new List<ConicalZone>();
        public void InitAvoidanceConicalZoneList()
        {
            lock (avoidanceConicalZoneList)
            {
                avoidanceConicalZoneList = new List<ConicalZone>();
            }
        }
        public void AddAvoidanceConicalZoneList(PointD initPt, PointD ciblePt, double radius)
        {
            lock (avoidanceConicalZoneList)
            {
                avoidanceConicalZoneList.Add(new ConicalZone(initPt, ciblePt, radius));
            }
        }

        //Zones Segment préférentielles
        List<SegmentZone> preferredSegmentZoneList = new List<SegmentZone>();
        public void InitPreferredSegmentZoneList()
        {
            lock (preferredSegmentZoneList)
            {
                preferredSegmentZoneList = new List<SegmentZone>();
            }
        }
        public void AddPreferredSegmentZoneList(PointD ptA, PointD ptB, double radius, double strength = 1)
        {
            lock (preferredSegmentZoneList)
            {
                preferredSegmentZoneList.Add(new SegmentZone(ptA, ptB, radius, strength));
            }
        }


        //Zones polygones convexes autorisées et interdites
        List<ConvexPolygonD> strictlyAllowedConvexPolygonList = new List<ConvexPolygonD>();
        public void InitStrictlyAllowedConvexPolygonList()
        {
            lock (strictlyAllowedConvexPolygonList)
            {
                strictlyAllowedConvexPolygonList = new List<ConvexPolygonD>();
            }
        }
        public void AddStrictlyAllowedConvexPolygon(ConvexPolygonD rect)
        {
            lock (strictlyAllowedConvexPolygonList)
            {
                strictlyAllowedConvexPolygonList.Add(rect);
            }
        }



        //Zones rectangulaires interdites
        List<RectangleZone> strictlyAllowedRectangleList = new List<RectangleZone>();
        public void InitStrictlyAllowedRectangleList()
        {
            lock (strictlyAllowedRectangleList)
            {
                strictlyAllowedRectangleList = new List<RectangleZone>();
            }
        }
        public void AddStrictlyAllowedRectangle(RectangleD rect)
        {
            lock (strictlyAllowedRectangleList)
            {
                strictlyAllowedRectangleList.Add(new RectangleZone(rect));
            }
        }

        //Zones rectangulaires interdites
        List<RectangleZone> preferredRectangleList = new List<RectangleZone>();
        public void InitPreferredRectangleList()
        {
            lock (preferredRectangleList)
            {
                preferredRectangleList = new List<RectangleZone>();
            }
        }
        public void AddPreferredRectangle(RectangleD rect, double strength)
        {
            lock (preferredRectangleList)
            {
                preferredRectangleList.Add(new RectangleZone(rect, strength));
            }
        }


        /****************************************** Events envoyés ***********************************************/

        public event EventHandler<HeatMapArgs> OnHeatMapStrategyEvent;
        public virtual void OnHeatMapStrategy(int id, Heatmap heatMap)
        {
            OnHeatMapStrategyEvent?.Invoke(this, new HeatMapArgs { RobotId = id, HeatMap = heatMap });
        }

        public event EventHandler<HeatMapArgs> OnHeatMapWayPointEvent;
        public virtual void OnHeatMapWayPoint(int id, Heatmap heatMap)
        {
            var handler = OnHeatMapWayPointEvent;
            if (handler != null)
            {
                handler(this, new HeatMapArgs { RobotId = id, HeatMap = heatMap });
            }
        }

        public event EventHandler<LocationArgs> OnDestinationEvent;
        public virtual void OnDestination(int id, Location location)
        {
            OnDestinationEvent?.Invoke(this, new LocationArgs { RobotId = id, Location = location });
        }

        public event EventHandler<RoleArgs> OnRoleEvent;
        public virtual void OnRole(int id, RoboCupPoste role)
        {
            OnRoleEvent?.Invoke(this, new RoleArgs { RobotId = id, Role = role });
        }

        public event EventHandler<BallHandlingStateArgs> OnBallHandlingStateEvent;
        public virtual void OnBallHandlingState(int id, BallHandlingState state)
        {
            OnBallHandlingStateEvent?.Invoke(this, new BallHandlingStateArgs { RobotId = id, State = state });
        }

        public event EventHandler<MessageDisplayArgs> OnMessageDisplayEvent;
        public virtual void OnMessageDisplay(int id, string msg)
        {
            OnMessageDisplayEvent?.Invoke(this, new MessageDisplayArgs { RobotId = id, Message = msg});
        }

        //public event EventHandler<PlayingSideArgs> OnPlayingSideEvent;
        //public virtual void OnPlayingSide(int id, PlayingSide playSide)
        //{
        //    OnPlayingSideEvent?.Invoke(this, new  PlayingSideArgs { RobotId = id, PlaySide = playSide});
        //}



        public event EventHandler<LocationArgs> OnWaypointEvent;
        public virtual void OnWaypoint(int id, Location wayPointlocation)
        {
            var handler = OnWaypointEvent;
            if (handler != null)
            {
                handler(this, new LocationArgs { RobotId = id, Location = wayPointlocation });
            }
        }

        public EventHandler<EventArgs> OnUpdateWorldMapDisplayEvent;
        public virtual void OnUpdateWorldMapDisplay(int id)
        {
            var handler = OnUpdateWorldMapDisplayEvent;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

        public event EventHandler<GameStateArgs> OnGameStateChangedEvent;
        public virtual void OnGameStateChanged(int robotId, GameState state)
        {
            var handler = OnGameStateChangedEvent;
            if (handler != null)
            {
                handler(this, new GameStateArgs { RobotId = robotId, gameState = state });
            }
        }

        public event EventHandler<PolarPIDSetupArgs> On4WheelsPolarSpeedPIDSetupEvent;
        public virtual void On4WheelsPolarSpeedPIDSetup(double px, double ix, double dx, double py, double iy, double dy, double ptheta, double itheta, double dtheta,
            double pxLimit, double ixLimit, double dxLimit, double pyLimit, double iyLimit, double dyLimit, double pthetaLimit, double ithetaLimit, double dthetaLimit
            )
        {
            On4WheelsPolarSpeedPIDSetupEvent?.Invoke(this, new PolarPIDSetupArgs
            {
                P_x = px,
                I_x = ix,
                D_x = dx,
                P_y = py,
                I_y = iy,
                D_y = dy,
                P_theta = ptheta,
                I_theta = itheta,
                D_theta = dtheta,
                P_x_Limit = pxLimit,
                I_x_Limit = ixLimit,
                D_x_Limit = dxLimit,
                P_y_Limit = pyLimit,
                I_y_Limit = iyLimit,
                D_y_Limit = dyLimit,
                P_theta_Limit = pthetaLimit,
                I_theta_Limit = ithetaLimit,
                D_theta_Limit = dthetaLimit
            });
        }

        public event EventHandler<PolarPIDSetupArgs> On2WheelsPolarSpeedPIDSetupEvent;
        public virtual void On2WheelsPolarSpeedPIDSetup(double px, double ix, double dx, double ptheta, double itheta, double dtheta,
            double pxLimit, double ixLimit, double dxLimit, double pthetaLimit, double ithetaLimit, double dthetaLimit
            )
        {
            On2WheelsPolarSpeedPIDSetupEvent?.Invoke(this, new PolarPIDSetupArgs
            {
                P_x = px,
                I_x = ix,
                D_x = dx,
                P_theta = ptheta,
                I_theta = itheta,
                D_theta = dtheta,
                P_x_Limit = pxLimit,
                I_x_Limit = ixLimit,
                D_x_Limit = dxLimit,
                P_theta_Limit = pthetaLimit,
                I_theta_Limit = ithetaLimit,
                D_theta_Limit = dthetaLimit
            });
        }

        public event EventHandler<LidarMessageArgs> OnMessageEvent;
        public virtual void OnLidarMessage(string message, int line)
        {
            OnMessageEvent?.Invoke(this, new LidarMessageArgs { Value = message, Line = line });
        }

        public event EventHandler<IndependantPIDSetupArgs> On4WheelsIndependantSpeedPIDSetupEvent;
        public virtual void On4WheelsIndependantSpeedPIDSetup(double pM1, double iM1, double dM1, double pM2, double iM2, double dM2, double pM3, double iM3, double dM3, double pM4, double iM4, double dM4,
            double pM1Limit, double iM1Limit, double dM1Limit, double pM2Limit, double iM2Limit, double dM2Limit, double pM3Limit, double iM3Limit, double dM3Limit, double pM4Limit, double iM4Limit, double dM4Limit
            )
        {
            On4WheelsIndependantSpeedPIDSetupEvent?.Invoke(this, new IndependantPIDSetupArgs
            {
                P_M1 = pM1,
                I_M1 = iM1,
                D_M1 = dM1,
                P_M2 = pM2,
                I_M2 = iM2,
                D_M2 = dM2,
                P_M3 = pM3,
                I_M3 = iM3,
                D_M3 = dM3,
                P_M4 = pM4,
                I_M4 = iM4,
                D_M4 = dM4,
                P_M1_Limit = pM1Limit,
                I_M1_Limit = iM1Limit,
                D_M1_Limit = dM1Limit,
                P_M2_Limit = pM2Limit,
                I_M2_Limit = iM2Limit,
                D_M2_Limit = dM2Limit,
                P_M3_Limit = pM3Limit,
                I_M3_Limit = iM3Limit,
                D_M3_Limit = dM3Limit,
                P_M4_Limit = pM4Limit,
                I_M4_Limit = iM4Limit,
                D_M4_Limit = dM4Limit
            });
        }

        public event EventHandler<IndependantPIDSetupArgs> On2WheelsIndependantSpeedPIDSetupEvent;
        public virtual void On2WheelsIndependantSpeedPIDSetup(double pM1, double iM1, double dM1, double pM2, double iM2, double dM2, 
            double pM1Limit, double iM1Limit, double dM1Limit, double pM2Limit, double iM2Limit, double dM2Limit)
        {
            On2WheelsIndependantSpeedPIDSetupEvent?.Invoke(this, new IndependantPIDSetupArgs
            {
                P_M1 = pM1,
                I_M1 = iM1,
                D_M1 = dM1,
                P_M2 = pM2,
                I_M2 = iM2,
                D_M2 = dM2,
                P_M1_Limit = pM1Limit,
                I_M1_Limit = iM1Limit,
                D_M1_Limit = dM1Limit,
                P_M2_Limit = pM2Limit,
                I_M2_Limit = iM2Limit,
                D_M2_Limit = dM2Limit,
            });
        }
               

        public event EventHandler<ByteEventArgs> OnSetAsservissementModeEvent;
        public virtual void OnSetAsservissementMode(byte val)
        {
            OnSetAsservissementModeEvent?.Invoke(this, new ByteEventArgs { Value = val });
        }

        public event EventHandler<SpeedConsigneToMotorArgs> OnSetSpeedConsigneToMotor;
        public virtual void OnSetSpeedConsigneToMotorEvent(object sender, SpeedConsigneToMotorArgs e)
        {
            OnSetSpeedConsigneToMotor?.Invoke(sender, e);
        }

        public event EventHandler<BoolEventArgs> OnEnableDisableMotorCurrentDataEvent;
        public virtual void OnEnableDisableMotorCurrentData(bool val)
        {
            OnEnableDisableMotorCurrentDataEvent?.Invoke(this, new BoolEventArgs { value = val });
        }

        public event EventHandler<BoolEventArgs> OnEnableDisableRxForwardFromHerkulexEvent;
        public virtual void OnEnableDisableRxForwardFromHerkulex(bool val)
        {
            OnEnableDisableRxForwardFromHerkulexEvent?.Invoke(this, new BoolEventArgs { value = val });
        }

        public event EventHandler<CollisionEventArgs> OnCollisionEvent;
        public virtual void OnCollision(int id, Location robotLocation)
        {
            OnCollisionEvent?.Invoke(this, new CollisionEventArgs { RobotId = id, RobotRealPositionRefTerrain = robotLocation });
        }

        public event EventHandler<LocationArgs> OnForcedLocationEvent;
        public virtual void OnForcedLocation(int id, Location robotLocation)
        {
            OnForcedLocationEvent?.Invoke(this, new LocationArgs { RobotId = id, Location = robotLocation });
        }

        public event EventHandler<IOValuesEventArgs> OnIOValuesFromRobotEvent;
        public void OnIOValuesFromRobot(object sender, IOValuesEventArgs e)
        {
            OnIOValuesFromRobotEvent?.Invoke(sender, e);
        }

        public event EventHandler<DoubleEventArgs> OnOdometryPointToMeterEvent;
        public void OnOdometryPointToMeter(double value)
        {
            OnOdometryPointToMeterEvent?.Invoke(this, new DoubleEventArgs { Value = value });
        }

        public event EventHandler<TwoWheelsAngleArgs> On2WheelsAngleSetEvent;
        public void On4WheelsAngleSet(double angleM1, double angleM2)
        {
            On2WheelsAngleSetEvent?.Invoke(this, new TwoWheelsAngleArgs { angleMotor1 = angleM1, angleMotor2 = angleM2});
        }

        public event EventHandler<TwoWheelsToPolarMatrixArgs> On2WheelsToPolarSetEvent;
        public void On4WheelsToPolarSet(double mX1, double mX2, double mTheta1, double mTheta2)
        {
            On2WheelsToPolarSetEvent?.Invoke(this, new TwoWheelsToPolarMatrixArgs
            {
                mx1 = mX1,
                mx2 = mX2,
                mtheta1 = mTheta1,
                mtheta2 = mTheta2,
            });
        }

        public event EventHandler<FourWheelsAngleArgs> On4WheelsAngleSetEvent;
        public void On4WheelsAngleSet(double angleM1, double angleM2, double angleM3, double angleM4)
        {
            On4WheelsAngleSetEvent?.Invoke(this, new FourWheelsAngleArgs { angleMotor1 = angleM1, angleMotor2 = angleM2, angleMotor3 = angleM3, angleMotor4 = angleM4 });
        }

        public event EventHandler<FourWheelsToPolarMatrixArgs> On4WheelsToPolarSetEvent;
        public void On4WheelsToPolarSet(double mX1, double mX2, double mX3, double mX4, double mY1, double mY2, double mY3, double mY4, double mTheta1, double mTheta2, double mTheta3, double mTheta4)
        {
            On4WheelsToPolarSetEvent?.Invoke(this, new FourWheelsToPolarMatrixArgs
            {
                mx1 = mX1,
                mx2 = mX2,
                mx3 = mX3,
                mx4 = mX4,
                my1 = mY1,
                my2 = mY2,
                my3 = mY3,
                my4 = mY4,
                mtheta1 = mTheta1,
                mtheta2 = mTheta2,
                mtheta3 = mTheta3,
                mtheta4 = mTheta4
            });
        }

        public void OnPilotageTurbine(byte turbineNumber, ushort periodUs)
        {
            OnSetPololuTargetUsEvent?.Invoke(this, new PololuServoUsArgs { servoChannel = turbineNumber, servoUs = periodUs });
        }

        public void OnHerkulexSetPosition(Dictionary<ServoId, int> positionDictionary)
        {
            OnHerkulexPositionRequestEvent?.Invoke(this, new HerkulexPositionsArgs { servoPositions = positionDictionary });
        }

        /****************************************** Events envoyés ***********************************************/
        //On fait juste un forward d'event sans le récupérer localement
        public event EventHandler<HerkulexPositionsArgs> OnHerkulexPositionRequestEvent;
        public void OnHerkulexPositionRequestForwardEvent(object sender, HerkulexPositionsArgs e)
        {
            OnHerkulexPositionRequestEvent?.Invoke(sender, e);
        }

        public event EventHandler<HerkulexRamReadArgs> OnHerkulexRamReadRequestEvent;
        public void OnHerkulexRamReadRequest(int servoId, HerkulexDescription.RAM_ADDR RamRegister, int length)
        {
            HerkulexRamReadArgs args = new HerkulexRamReadArgs(servoId, RamRegister, length);
            OnHerkulexRamReadRequestEvent?.Invoke(this, args);
        }

        public event EventHandler<BoolEventArgs> OnHerkulexEnableDisableTorqueEvent;
        public void OnHerkulexEnableDisableTorque(object sender, BoolEventArgs e)
        {
            OnHerkulexEnableDisableTorqueEvent?.Invoke(sender, e);
        }
        public event EventHandler<PololuServoUsArgs> OnSetPololuTargetUsEvent;
        public void OnSetPololuTargetUs(object sender, PololuServoUsArgs e)
        {
            //Forward event to task on low level
            OnSetPololuTargetUsEvent?.Invoke(sender, e);
        }

        public event EventHandler<BoolEventArgs> OnEnableDisableMotorEvent;
        public virtual void OnEnableDisableMotor(bool val)
        {
            OnEnableDisableMotorEvent?.Invoke(this, new BoolEventArgs { value = val });
        }

        public event EventHandler<TrajectoryGeneratorConstants> OnTrajectoryConstantsEvent;
        public virtual void OnTrajectoryConstants(int robotId, double accelLineaireMax, double accelRotationCapVitesseMax, double accelRotationOrientationRobotMax,
                                                double vitesseLineaireMax, double vitesseRotationCapVitesseMax, double vitesseRotationOrientationRobotMax)
        {
            var trajParam = new TrajectoryGeneratorConstants();
            trajParam.accelLineaireMax = accelLineaireMax;
            trajParam.accelRotationCapVitesseMax = accelRotationCapVitesseMax;
            trajParam.accelRotationOrientationRobotMax = accelRotationOrientationRobotMax;
            trajParam.vitesseLineaireMax = vitesseLineaireMax;
            trajParam.vitesseRotationCapVitesseMax = vitesseRotationCapVitesseMax;
            trajParam.vitesseRotationOrientationRobotMax = vitesseRotationOrientationRobotMax;
            trajParam.RobotId = robotId;
            OnTrajectoryConstantsEvent?.Invoke(this, trajParam);
        }
        public event EventHandler<PolarPIDSetupArgs> OnPolarPositionPIDConstantsEvent;
        public virtual void OnPolarPositionPID(double P_x, double I_x, double D_x, double P_x_Limit, double I_x_Limit, double D_x_Limit,
            double P_y, double I_y, double D_y, double P_y_Limit, double I_y_Limit, double D_y_Limit,
            double P_theta, double I_theta, double D_theta, double P_theta_Limit, double I_theta_Limit, double D_theta_Limit)
        {
            var p = new PolarPIDSetupArgs();            
            p.P_x = P_x;
            p.I_x = I_x;
            p.D_x = D_x;
            p.P_x_Limit = P_x_Limit;
            p.I_x_Limit = I_x_Limit;
            p.D_x_Limit = D_x_Limit;

            p.P_y = P_y;
            p.I_y = I_y;
            p.D_y = D_y;
            p.P_y_Limit = P_y_Limit;
            p.I_y_Limit = I_y_Limit;
            p.D_y_Limit = D_y_Limit;

            p.P_theta = P_theta;
            p.D_theta = D_theta;
            p.I_theta = I_theta;
            p.P_theta_Limit = P_theta_Limit;
            p.D_theta_Limit = D_theta_Limit;
            p.I_theta_Limit = I_theta_Limit;
            OnPolarPositionPIDConstantsEvent?.Invoke(this, p);
        }

        public event EventHandler<PointDExtendedListArgs> OnStrategyPtListEvent;
        public virtual void OnStrategyPtList(object sender, PointDExtendedListArgs e)
        {
            var handler = OnStrategyPtListEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }    
}
