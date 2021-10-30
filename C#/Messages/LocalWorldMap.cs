using Constants;
using HeatMap;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Utilities;
using ZeroFormatter;

namespace MessagesNS
{
    [ZeroFormattable]
    public class LocalWorldMap:ZeroFormatterMsg
    {// UnionKey value must return constant value(Type is free, you can use int, string, enum, etc...)
        public override ZeroFormatterMsgType Type
        {
            get
            {
                return ZeroFormatterMsgType.LocalWM;
            }
        }

        [Index(0)]
        public virtual int RobotId { get; set; }
        [Index(1)]
        public virtual int TeamId { get; set; }
        [Index(2)]
        public virtual Location robotLocation { get; set; }
        [Index(3)]
        public virtual RoboCupPoste robotRole { get; set; }
        [Index(4)]
        public virtual BallHandlingState ballHandlingState { get; set; }
        [Index(5)]
        public virtual string messageDisplay { get; set; }

        [Index(6)]
        public virtual PlayingSide playingSide { get; set; }
        [Index(7)]
        public virtual Location robotGhostLocation { get; set; }
        [Index(8)]
        public virtual Location destinationLocation { get; set; }
        [Index(9)]
        public virtual Location waypointLocation { get; set; }
        [Index(10)]
        public virtual List<Location> ballLocationList { get; set; }
        [Index(11)]
        public virtual List<LocationExtended> teammateLocationList { get; set; }
        [Index(12)]
        public virtual List<LocationExtended> obstacleLocationList { get; set; }
        [IgnoreFormat]
        public virtual object obstacleLocationListLock { get; set; }
        //[IgnoreFormat]
        //public virtual List<PolarPointListExtended> lidarObjectList { get; set; }
        //[IgnoreFormat]
        //public virtual List<PolarPointListExtended> strategyObjectList { get; set; }

        [JsonIgnore]
        [IgnoreFormat]
        public virtual List<PointDExtended> lidarRawPtsList { get; set; }
        [JsonIgnore]
        [IgnoreFormat]
        public virtual List<PointDExtended> lidarProcessedPtsList { get; set; }
        [JsonIgnore]
        [IgnoreFormat]
        public virtual List<PointDExtended> strategyPtsList { get; set; }
        [JsonIgnore]
        [IgnoreFormat]
        //public virtual List<PointDExtended> lidarMapProcessed1 { get; set; }
        //[JsonIgnore]
        //[IgnoreFormat]
        //public virtual List<PointDExtended> lidarMapProcessed2 { get; set; }
        //[JsonIgnore]
        //[IgnoreFormat]
        //public virtual List<PointDExtended> lidarMapProcessed3 { get; set; }
        //[JsonIgnore]
        //[IgnoreFormat]
        public virtual List<SegmentExtended> lidarSegmentList { get; set; }
        [JsonIgnore]
        [IgnoreFormat]
        public virtual Heatmap heatMapStrategy { get; set; }
        [JsonIgnore]
        [IgnoreFormat]
        public virtual Heatmap heatMapWaypoint { get; set; }

        public LocalWorldMap()
        {
            //Type = "LocalWorldMap";
            obstacleLocationListLock = new object();
        }

        public void Init()
        {
            robotLocation = new Location(0, 0, 0, 0, 0, 0);
            robotGhostLocation = new Location(0, 0, 0, 0, 0, 0); 
        }
    }
}
