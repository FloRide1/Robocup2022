using Constants;
using EventArgsLibrary;
using MessagesNS;
using Newtonsoft.Json;
using PerformanceMonitorTools;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;
using ZeroFormatter;

namespace WorldMapManager
{
    public class LocalWorldMapManager
    {
        LocalWorldMap localWorldMap;
        bool useMulticastUdp = false;

        public LocalWorldMapManager(int robotId, int teamId, bool useMulticast)
        {
            localWorldMap = new LocalWorldMap();
            localWorldMap.Init();
            localWorldMap.RobotId = robotId;
            localWorldMap.TeamId = teamId;
            useMulticastUdp = useMulticast;
        }

        //public void OnPhysicalPositionReceived(object sender, EventArgsLibrary.LocationArgs e)
        //{
        //    if (localWorldMap == null)
        //        return;
        //    if (robotId == e.RobotId)
        //    {
        //        localWorldMap.robotLocation = e.Location;
        //        OnLocalWorldMap(robotId, localWorldMap);
        //    }
        //}

        DecimalJsonConverter decimalJsonConverter = new DecimalJsonConverter();
        public void OnPerceptionReceived(object sender, EventArgsLibrary.PerceptionArgs e)
        {
            //PerceptionMonitor.PerceptionReceived();
            if (localWorldMap == null)
                return;
            if (localWorldMap.RobotId == e.RobotId)
            {
                ///On ajoute les infos à la Local World Map
                ///On commence par la position du robot 
                localWorldMap.robotLocation = e.Perception.robotKalmanLocation;

                ///On continue avec les positions des teammates et des obstacles
                ///On fait une copie locale des obstacles vus dans la perception pour casser les références.
                LocationExtended[] tempArray = new LocationExtended[e.Perception.obstaclesLocationList.Count]; 
                e.Perception.obstaclesLocationList.CopyTo(tempArray);

                double seuil = 0.3; ///Seuil minimum pour confondre un obstacle perçu avec le robot

                if (localWorldMap.TeamId == (int)TeamId.Team1)
                {
                    localWorldMap.teammateLocationList = tempArray.Where(p => p.Type == ObjectType.RobotTeam1 && Toolbox.Distance(localWorldMap.robotLocation, new Location(p.X, p.Y, p.Theta, p.Vx, p.Vy, p.Vtheta)) > seuil).ToList();
                    localWorldMap.obstacleLocationList = tempArray.Where(p => p.Type != ObjectType.RobotTeam1).ToList();
                }
                else if (localWorldMap.TeamId == (int)TeamId.Team2)
                {
                    localWorldMap.teammateLocationList = tempArray.Where(p => p.Type == ObjectType.RobotTeam2 && Toolbox.Distance(localWorldMap.robotLocation, new Location(p.X, p.Y, p.Theta, p.Vx, p.Vy, p.Vtheta)) > seuil).ToList();
                    localWorldMap.obstacleLocationList = tempArray.Where(p => p.Type != ObjectType.RobotTeam2).ToList();
                }
                
                localWorldMap.ballLocationList = e.Perception.ballLocationList;
                
                //On recopie les infos de la local World Map dans la structure de transfert (sans ce qui coute cher : heatmaps, lidarpoints...)
                LocalWorldMap transferLocalWorldMap = new LocalWorldMap();
                transferLocalWorldMap.RobotId = localWorldMap.RobotId;
                transferLocalWorldMap.TeamId = localWorldMap.TeamId;
                transferLocalWorldMap.robotLocation = localWorldMap.robotLocation;
                transferLocalWorldMap.destinationLocation = localWorldMap.destinationLocation;
                transferLocalWorldMap.waypointLocation = localWorldMap.waypointLocation;
                transferLocalWorldMap.robotGhostLocation = localWorldMap.robotGhostLocation;
                transferLocalWorldMap.robotRole = localWorldMap.robotRole;
                transferLocalWorldMap.ballHandlingState = localWorldMap.ballHandlingState;
                transferLocalWorldMap.messageDisplay = localWorldMap.messageDisplay;
                transferLocalWorldMap.playingSide = localWorldMap.playingSide;
                transferLocalWorldMap.obstacleLocationList = localWorldMap.obstacleLocationList;
                transferLocalWorldMap.ballLocationList = localWorldMap.ballLocationList;
                transferLocalWorldMap.teammateLocationList = localWorldMap.teammateLocationList;

                if (transferLocalWorldMap.robotLocation != null)
                {
                    var s = ZeroFormatterSerializer.Serialize<ZeroFormatterMsg>(transferLocalWorldMap);

                    if (useMulticastUdp)
                        OnMulticastSendLocalWorldMapCommand(s); //Envoi à destination des autres robots en multicast

                    OnLocalWorldMapToGlobalWorldMapGenerator(transferLocalWorldMap); //Envoi à destination du robot lui même en direct

                    OnLocalWorldMapForDisplayOnly(localWorldMap); //Pour affichage uniquement, sinon transmission radio en, multicast

                    //LWMEmiseMonitoring.LWMEmiseMonitor(s.Length);                 
                }
            }
        }

        public void OnWaypointReceived(object sender, EventArgsLibrary.LocationArgs e)
        {
            if (localWorldMap == null)
                return;
            if (localWorldMap.RobotId == e.RobotId)
            {
                localWorldMap.waypointLocation = e.Location;
            }
        }

        public void OnGhostLocationReceived(object sender, EventArgsLibrary.LocationArgs e)
        {
            if (localWorldMap == null)
                return;
            if (localWorldMap.RobotId == e.RobotId)
            {
                localWorldMap.robotGhostLocation = e.Location;
            }
        }

        public void OnDestinationReceived(object sender, EventArgsLibrary.LocationArgs e)
        {
            if (localWorldMap == null)
                return;
            if (localWorldMap.RobotId == e.RobotId)
            {
                localWorldMap.destinationLocation = e.Location;
            }
        }

        public void OnRoleReceived(object sender, EventArgsLibrary.RoleArgs e)
        {
            if (localWorldMap == null)
                return;
            if (localWorldMap.RobotId == e.RobotId)
            {
                localWorldMap.robotRole = e.Role;
            }
        }

        public void OnBallHandlingStateReceived(object sender, EventArgsLibrary.BallHandlingStateArgs e)
        {
            if (localWorldMap == null)
                return;
            if (localWorldMap.RobotId == e.RobotId)
            {
                localWorldMap.ballHandlingState = e.State;
            }
        }

        public void OnMessageDisplayReceived(object sender, EventArgsLibrary.MessageDisplayArgs e)
        {
            if (localWorldMap == null)
                return;
            if (localWorldMap.RobotId == e.RobotId)
            {
                localWorldMap.messageDisplay = e.Message;
            }
        }

        public void OnPlayingSideReceived(object sender, EventArgsLibrary.PlayingSideArgs e)
        {
            if (localWorldMap == null)
                return;
            if (localWorldMap.RobotId == e.RobotId)
            {
                localWorldMap.playingSide = e.PlaySide;
            }
        }

        public void OnHeatMapStrategyReceived(object sender, EventArgsLibrary.HeatMapArgs e)
        {
            if (localWorldMap == null)
                return;
            if (localWorldMap.RobotId == e.RobotId)
            {
                localWorldMap.heatMapStrategy = e.HeatMap;
            }
        }

        public void OnHeatMapWaypointReceived(object sender, EventArgsLibrary.HeatMapArgs e)
        {
            if (localWorldMap == null)
                return;
            if (localWorldMap.RobotId == e.RobotId)
            {
                localWorldMap.heatMapWaypoint = e.HeatMap;
            }
        }


        public void OnRawLidarDataReceived(object sender, EventArgsLibrary.RawLidarArgs e)
        {
            if (localWorldMap == null || localWorldMap.robotLocation == null)
                return;
            if (localWorldMap.RobotId == e.RobotId && e.PtList.Count != 0)
            {
                List<PointDExtended> listPtLidar = new List<PointDExtended>();

                try
                {
                    listPtLidar = e.PtList.Select(
                           pt => new PointDExtended(
                               new PointD(localWorldMap.robotLocation.X + pt.Distance * Math.Cos(pt.Angle + localWorldMap.robotLocation.Theta),
                                            localWorldMap.robotLocation.Y + pt.Distance * Math.Sin(pt.Angle + localWorldMap.robotLocation.Theta)),
                               System.Drawing.Color.White, 3)).ToList();
                    switch (e.Type)
                    {
                        case LidarDataType.RawPtsList:
                            localWorldMap.lidarRawPtsList = listPtLidar;
                            break;
                            //case LidarDataType.ProcessedData1:
                            //    localWorldMap.lidarMapProcessed1 = listPtLidar;
                            //    break;
                            //case LidarDataType.ProcessedData2:
                            //    localWorldMap.lidarMapProcessed2 = listPtLidar;
                            //    break;
                            //case LidarDataType.ProcessedData3:
                            //    localWorldMap.lidarMapProcessed3 = listPtLidar;
                            //    break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("LocalWorldMapManager Exception : " + ex.ToString());
                }
            }
        }

        public void OnLidarDataReceived(object sender, EventArgsLibrary.LidarPolarPtListExtendedArgs e)
        {
            if (localWorldMap == null || localWorldMap.robotLocation == null)
                return;
            if (localWorldMap.RobotId == e.RobotId && e.PtList.Count!=0)
            {
                List<PointDExtended> listPtLidar = new List<PointDExtended>();

                try
                {
                    listPtLidar = e.PtList.Select(
                           pt => new PointDExtended(
                               new PointD(localWorldMap.robotLocation.X + pt.Pt.Distance * Math.Cos(pt.Pt.Angle + localWorldMap.robotLocation.Theta),
                                            localWorldMap.robotLocation.Y + pt.Pt.Distance * Math.Sin(pt.Pt.Angle + localWorldMap.robotLocation.Theta)),
                               pt.Color, pt.Width)).ToList();
                    switch (e.Type)
                    {
                        case LidarDataType.ProcessedPtsList:
                            localWorldMap.lidarProcessedPtsList = listPtLidar;
                            break;
                        default:
                            Console.WriteLine("LWM : OnLidarDataReceived : Cas anormal");
                            break;
                    }
                }
                catch {
                };

            }
        }

        public void OnStrategyDataReceived(object sender, PointDExtendedListArgs e)
        {
            if (localWorldMap == null || localWorldMap.robotLocation == null)
                return;
            if (localWorldMap.RobotId == e.RobotId && e.PtList.Count != 0)
            {
                List<PointDExtended> listPtStrategy = new List<PointDExtended>();
                try
                {                    
                    localWorldMap.strategyPtsList = e.PtList;
                }
                catch
                {
                };

            }
        }

        public void OnLidarProcessedSegmentsReceived(object sender, SegmentExtendedListArgs e)
        {
            if (localWorldMap == null || localWorldMap.robotLocation == null)
                return;
            if (localWorldMap.RobotId == e.RobotId && e.SegmentList.Count > 0)
            {
                localWorldMap.lidarSegmentList = e.SegmentList;
            }
        }
        //public void OnProcessedLidarDataReceived(object sender, EventArgsLibrary.RawLidarArgs e)
        //{
        //    if (localWorldMap == null || localWorldMap.robotLocation == null)
        //        return;
        //    if (localWorldMap.RobotId == e.RobotId && e.PtList.Count != 0)
        //    {
        //        List<PointD> listPtLidar = new List<PointD>();

        //        try
        //        {
        //            //for (int i = 0; i < 500; i++) //Stress test
        //            {
        //                listPtLidar = e.PtList.Select(
        //                pt => new PointD(localWorldMap.robotLocation.X + pt.Distance * Math.Cos(pt.Angle + localWorldMap.robotLocation.Theta),
        //                                 localWorldMap.robotLocation.Y + pt.Distance * Math.Sin(pt.Angle + localWorldMap.robotLocation.Theta))).ToList();
        //            }
        //        }
        //        catch { };

        //        localWorldMap.lidarMapProcessed1 = listPtLidar;
        //    }
        //}

        //public void OnLidarObjectsReceived(object sender, EventArgsLibrary.PolarPointListExtendedListArgs e)
        //{
        //    if (localWorldMap == null || localWorldMap.robotLocation == null)
        //        return;
        //    if (localWorldMap.RobotId == e.RobotId)
        //    {
        //        localWorldMap.lidarObjectList = e.ObjectList;
        //    }
        //}

        //Output events
        public event EventHandler<DataReceivedArgs> OnMulticastSendLocalWorldMapEvent;
        public virtual void OnMulticastSendLocalWorldMapCommand(byte[] data)
        {
            var handler = OnMulticastSendLocalWorldMapEvent;
            if (handler != null)
            {
                handler(this, new DataReceivedArgs { Data = data });
            }
        }

        public event EventHandler<LocalWorldMapArgs> OnLocalWorldMapTransferEvent;
        public virtual void OnLocalWorldMapToGlobalWorldMapGenerator(LocalWorldMap data)
        {
            var handler = OnLocalWorldMapTransferEvent;
            if (handler != null)
            {
                handler(this, new LocalWorldMapArgs { LocalWorldMap = data});
            }
        }

        ////Output event for display only : NO USE for transmitting data !
        public event EventHandler<LocalWorldMapArgs> OnLocalWorldMapForDisplayOnlyEvent;
        public virtual void OnLocalWorldMapForDisplayOnly(LocalWorldMap map)
        {
            var handler = OnLocalWorldMapForDisplayOnlyEvent;
            if (handler != null)
            {
                handler(this, new LocalWorldMapArgs { LocalWorldMap = map });
            }
        }

        ////Output event for Multicast Bypass : NO USE at RoboCup !
        public event EventHandler<LocalWorldMapArgs> OnLocalWorldMapBypassEvent;
        public virtual void OnLocalWorldMapBypass(LocalWorldMap map)
        {
            var handler = OnLocalWorldMapBypassEvent;
            if (handler != null)
            {
                handler(this, new LocalWorldMapArgs { LocalWorldMap = map });
            }
        }
    }
}
