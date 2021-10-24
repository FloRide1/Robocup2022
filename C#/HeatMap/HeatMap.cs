﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace HeatMap
{
    public class Heatmap
    {
        public double FieldLength;
        public double FieldHeight;
        public double Precision;
        double HalfFieldLength;
        double HalfFieldHeight;

        public double[,] BaseHeatMapData;
        public double BaseXCellSize;
        public double BaseYCellSize;
        public int nbCellInBaseHeatMapHeight;
        public int nbCellInBaseHeatMapWidth;

        public float preferedDestinationX;
        public float preferedDestinationY;

        public double minimumPossibleValue = -1;

        public int robotId;

        public Heatmap(double length, double height, int lengthCellNumber, int id)//, int iterations)
        {
            BaseXCellSize = length / lengthCellNumber;
            BaseYCellSize = height / Math.Floor(height / BaseXCellSize);// BaseXCellSize * height / length;
            Precision = Math.Sqrt(BaseXCellSize* BaseXCellSize  + BaseYCellSize* BaseYCellSize);

            FieldLength = length;
            FieldHeight = height;
            HalfFieldLength = FieldLength / 2;
            HalfFieldHeight = FieldHeight / 2;
            
            nbCellInBaseHeatMapHeight = (int)(FieldHeight / BaseYCellSize) +1;
            nbCellInBaseHeatMapWidth = (int)(FieldLength / BaseXCellSize) +1;
            BaseHeatMapData = new double[nbCellInBaseHeatMapHeight, nbCellInBaseHeatMapWidth];

            robotId = id;

            //Trace.Listeners.Add(new TextWriterTraceListener("debug.log"));
            //Trace.AutoFlush = true;
        }

        public PointD GetFieldPosFromBaseHeatMapCoordinates(double xHeatMap, double yHeatMap)
        {
            //return new PointD(-HalfFieldLength + x * BaseCellSize, -HalfFieldHeight + y * BaseCellSize);
            double xField = (xHeatMap / (nbCellInBaseHeatMapWidth - 1) - 0.5) * FieldLength;
            double yField = (yHeatMap / (nbCellInBaseHeatMapHeight - 1) - 0.5) * FieldHeight;

            return new PointD(xField, yField);

        }
        public PointD GetBaseHeatMapPosFromFieldCoordinates(PointD ptTerrain)
        {
            if (ptTerrain != null)
            {
                double xHeatmap = (ptTerrain.X / FieldLength + 0.5) * (nbCellInBaseHeatMapWidth - 1);
                double yHeatmap = (ptTerrain.Y / FieldHeight + 0.5) * (nbCellInBaseHeatMapHeight - 1);

                return new PointD(xHeatmap, yHeatmap);
            }
            else
                return null;
        }

        public double GetBaseHeatMapXPosFromFieldCoordinates(double posX)
        {
            return (posX / FieldLength + 0.5) * (nbCellInBaseHeatMapWidth - 1);
        }
        public double GetBaseHeatMapYPosFromFieldCoordinates(double posY)
        {
            return (posY / FieldHeight+ 0.5) * (nbCellInBaseHeatMapHeight - 1);
        }

        public double GetBaseHeatMapDistanceFromFieldDistance(double distTerrain)
        {
            //return new PointD((x + HalfFieldLength) / BaseXCellSize, (y + HalfFieldHeight) / BaseYCellSize);

            return distTerrain / FieldLength * (nbCellInBaseHeatMapWidth - 1);
        }


        bool evaluateHeatMapGeneration = true;
        string heatMapGenerationLog = "";

        public void GenerateHeatMap(List<Zone> preferredZonesList, List<Zone> avoidanceZonesList, 
            List<RectangleZone> forbiddenRectangleList,  List<RectangleZone> strictlyAllowedRectangleList,
            List<RectangleZone> preferredRectangleList, List<ConicalZone> avoidanceConicalZoneList, 
            List<SegmentZone> preferredSegmentZoneList, List<ConvexPolygonD> strictlyAllowedPolygonList)
        {
        Stopwatch sw = new Stopwatch();

            if (evaluateHeatMapGeneration)
            {
                sw.Restart();
                heatMapGenerationLog = "\nRobot : " + robotId + " - Heat Map Generation report";
            }

            /// Initialisation de la HeatMap à 0
            BaseHeatMapData = new double[nbCellInBaseHeatMapHeight, nbCellInBaseHeatMapWidth];
            //for (int y = 0; y < nbCellInBaseHeatMapHeight; y++)
            //{
            //    for (int x = 0; x < nbCellInBaseHeatMapWidth; x++)
            //    {
            //        BaseHeatMapData[y, x] = 0;
            //    }
            //}
            //BaseHeatMapData = Enumerable.Range(0, nbCellInBaseHeatMapHeight)
            //    .SelectMany(x => Enumerable.Range(0, nbCellInBaseHeatMapWidth), (row, col) => new { row, col })
            //    .Aggregate(new double[nbCellInBaseHeatMapHeight, nbCellInBaseHeatMapWidth],(board, position) =>
            //    {
            //        board[position.row, position.col] = 0;
            //        return board;
            //    });

            if (evaluateHeatMapGeneration)
            {
                sw.Stop();
                heatMapGenerationLog += "\nHeatMap Init : " + sw.Elapsed.TotalMilliseconds + " ms";
                sw.Restart();
            }

            ///Fonction couteuse en temps de calcul...
            lock (strictlyAllowedPolygonList)
            {
                foreach (var allowedPolygon in strictlyAllowedPolygonList)
                {
                    for (int y = 0; y < nbCellInBaseHeatMapHeight; y++)
                    {
                        for (int x = 0; x < nbCellInBaseHeatMapWidth; x++)
                        {
                            ///Si le pt n'est pas dans le polygone, on l'exclut
                            PointD ptHeatMapRefTerrain = GetFieldPosFromBaseHeatMapCoordinates(x, y);
                            if (!allowedPolygon.IsInside(ptHeatMapRefTerrain))
                                BaseHeatMapData[y, x] = -1;
                        }
                    }
                }
            }

            if (evaluateHeatMapGeneration)
            {
                sw.Stop();
                heatMapGenerationLog += "\nHeatMap Strictly Allowed Polygons : " + sw.Elapsed.TotalMilliseconds + " ms - "+ strictlyAllowedPolygonList.Count+" polygons";
                sw.Restart();
            }

            /// Gestion des zones strictement autorisées(cela signifie que le rectangle est ok, mais pas l'extérieur du rectangle
            lock (strictlyAllowedRectangleList)
            {
                foreach (var allowedRectangle in strictlyAllowedRectangleList)
                {
                    var xMinHeatMap = GetBaseHeatMapXPosFromFieldCoordinates(allowedRectangle.rectangularZone.Xmin);
                    var xMaxHeatMap = GetBaseHeatMapXPosFromFieldCoordinates(allowedRectangle.rectangularZone.Xmax);
                    var yMinHeatMap = GetBaseHeatMapYPosFromFieldCoordinates(allowedRectangle.rectangularZone.Ymin);
                    var yMaxHeatMap = GetBaseHeatMapYPosFromFieldCoordinates(allowedRectangle.rectangularZone.Ymax);

                    for (int y = 0; y < nbCellInBaseHeatMapHeight; y++)
                    {
                        for (int x = 0; x < nbCellInBaseHeatMapWidth; x++)
                        {
                            BaseHeatMapData[y, x] = -1;
                        }
                    }
                    for (int y = (int)Math.Max(0, yMinHeatMap); y < (int)(Math.Min(nbCellInBaseHeatMapHeight, yMaxHeatMap)); y++)
                    {
                        for (int x = (int)Math.Max(0, xMinHeatMap); x < (int)(Math.Min(nbCellInBaseHeatMapWidth, xMaxHeatMap)); x++)
                        {
                            BaseHeatMapData[y, x] = 0;
                        }
                    }
                }
            }

            if (evaluateHeatMapGeneration)
            {
                sw.Stop();
                heatMapGenerationLog += "\nHeatMap Strictly Allowed Rectangles : " + sw.Elapsed.TotalMilliseconds + " ms - " + strictlyAllowedPolygonList.Count + " rectangles";
                sw.Restart();
            }

            //Gestion des zones interdites
            lock (forbiddenRectangleList)
            {
                foreach (var forbiddenRectangle in forbiddenRectangleList)
                {
                    var xMinHeatMap = GetBaseHeatMapXPosFromFieldCoordinates(forbiddenRectangle.rectangularZone.Xmin);
                    var xMaxHeatMap = GetBaseHeatMapXPosFromFieldCoordinates(forbiddenRectangle.rectangularZone.Xmax);
                    var yMinHeatMap = GetBaseHeatMapYPosFromFieldCoordinates(forbiddenRectangle.rectangularZone.Ymin);
                    var yMaxHeatMap = GetBaseHeatMapYPosFromFieldCoordinates(forbiddenRectangle.rectangularZone.Ymax);

                    for (int y = (int)Math.Max(0, yMinHeatMap); y < (int)(Math.Min(nbCellInBaseHeatMapHeight, yMaxHeatMap)); y++)
                    {
                        for (int x = (int)Math.Max(0, xMinHeatMap); x < (int)(Math.Min(nbCellInBaseHeatMapWidth, xMaxHeatMap)); x++)
                        {
                            BaseHeatMapData[y, x] = -1;
                        }
                    }
                }
            }
            if (evaluateHeatMapGeneration)
            {
                sw.Stop();
                heatMapGenerationLog += "\nHeatMap Forbidden Rectangles : " + sw.Elapsed.TotalMilliseconds + " ms - " + forbiddenRectangleList.Count + " rectangles";
                sw.Restart();
            }

            //Gestion des zones rectangulaires préférées
            lock (preferredRectangleList)
            {
                foreach (var rectangle in preferredRectangleList)
                {
                    var xMinHeatMap = GetBaseHeatMapXPosFromFieldCoordinates(rectangle.rectangularZone.Xmin);
                    var xMaxHeatMap = GetBaseHeatMapXPosFromFieldCoordinates(rectangle.rectangularZone.Xmax);
                    var yMinHeatMap = GetBaseHeatMapYPosFromFieldCoordinates(rectangle.rectangularZone.Ymin);
                    var yMaxHeatMap = GetBaseHeatMapYPosFromFieldCoordinates(rectangle.rectangularZone.Ymax);

                    for (int y = (int)Math.Max(0, yMinHeatMap); y < (int)(Math.Min(nbCellInBaseHeatMapHeight, yMaxHeatMap)); y++)
                    {
                        for (int x = (int)Math.Max(0, xMinHeatMap); x < (int)(Math.Min(nbCellInBaseHeatMapWidth, xMaxHeatMap)); x++)
                        {
                            BaseHeatMapData[y, x] += rectangle.strength;
                        }
                    }
                }
            }

            if (evaluateHeatMapGeneration)
            {
                sw.Stop();
                heatMapGenerationLog += "\nHeatMap Prefered Rectangles " + sw.Elapsed.TotalMilliseconds + " ms - " + preferredRectangleList.Count + " rectangles";
                sw.Restart();
            }

            lock (avoidanceZonesList)
            {
                foreach (var avoidanceZone in avoidanceZonesList)
                {
                    var centerRefHeatMap = GetBaseHeatMapPosFromFieldCoordinates(avoidanceZone.center);
                    var radiusRefHeatMap = GetBaseHeatMapDistanceFromFieldDistance(avoidanceZone.radius);
                    var strength = avoidanceZone.strength;

                    for (int y = (int)Math.Max(0, centerRefHeatMap.Y - radiusRefHeatMap); y < (int)(Math.Min(nbCellInBaseHeatMapHeight, centerRefHeatMap.Y + radiusRefHeatMap)); y++)
                    {
                        for (int x = (int)Math.Max(0, centerRefHeatMap.X - radiusRefHeatMap); x < (int)(Math.Min(nbCellInBaseHeatMapWidth, centerRefHeatMap.X + radiusRefHeatMap)); x++)
                        {
                            if (BaseHeatMapData[y, x] > -1) //On regarde si on n'est pas dans une zone exclue (valeur <= -1)
                                BaseHeatMapData[y, x] -= strength * Math.Max(0, 1 - Math.Sqrt((centerRefHeatMap.X - x) * (centerRefHeatMap.X - x) + (centerRefHeatMap.Y - y) * (centerRefHeatMap.Y - y)) / radiusRefHeatMap);
                            else
                                BaseHeatMapData[y, x] = -1;
                        }
                    }
                }
            }

            if (evaluateHeatMapGeneration)
            {
                sw.Stop();
                heatMapGenerationLog += "\nHeatMap Avoidance Spherical Zones : " + sw.Elapsed.TotalMilliseconds + " ms - " + avoidanceZonesList.Count + " zones";
                sw.Restart();
            }

            lock (preferredZonesList)
            {
                foreach (var preferredZone in preferredZonesList)
                {
                    var centerRefHeatMap = GetBaseHeatMapPosFromFieldCoordinates(preferredZone.center);
                    var radiusRefHeatMap = GetBaseHeatMapDistanceFromFieldDistance(preferredZone.radius);
                    var strength = preferredZone.strength;
                    if (centerRefHeatMap != null)
                    {
                        for (int y = (int)Math.Max(0, centerRefHeatMap.Y - radiusRefHeatMap); y < (int)(Math.Min(nbCellInBaseHeatMapHeight, centerRefHeatMap.Y + radiusRefHeatMap)); y++)
                        {
                            for (int x = (int)Math.Max(0, centerRefHeatMap.X - radiusRefHeatMap); x < (int)(Math.Min(nbCellInBaseHeatMapWidth, centerRefHeatMap.X + radiusRefHeatMap)); x++)
                            {
                                if (BaseHeatMapData[y, x] > -1) //On regarde si on n'est pas dans une zone exclue (valeur <= -1)
                                    BaseHeatMapData[y, x] += strength * Math.Max(0, 1 - Math.Sqrt((centerRefHeatMap.X - x) * (centerRefHeatMap.X - x) + (centerRefHeatMap.Y - y) * (centerRefHeatMap.Y - y)) / radiusRefHeatMap);
                                else
                                    BaseHeatMapData[y, x] = -1;
                            }
                        }
                    }
                }
            }

            if (evaluateHeatMapGeneration)
            {
                sw.Stop();
                heatMapGenerationLog += "\nHeatMap Preferred Spherical Zones : " + sw.Elapsed.TotalMilliseconds + " ms - " + preferredZonesList.Count + " zones";
                sw.Restart();
            }

            lock (preferredSegmentZoneList)
            {
                foreach (var segmentZone in preferredSegmentZoneList)
                {
                    var ptARefHeatMap = GetBaseHeatMapPosFromFieldCoordinates(segmentZone.PointA);
                    var ptBRefHeatMap = GetBaseHeatMapPosFromFieldCoordinates(segmentZone.PointB);
                    var radiusRefHeatMap = GetBaseHeatMapDistanceFromFieldDistance(segmentZone.Radius);
                    var strength = segmentZone.Strength;

                    for (int y = (int)Math.Max(0, Math.Min(ptARefHeatMap.Y, ptBRefHeatMap.Y) - radiusRefHeatMap); y < (int)Math.Min(nbCellInBaseHeatMapHeight, Math.Max(ptARefHeatMap.Y, ptBRefHeatMap.Y) + radiusRefHeatMap); y++)
                    {
                        for (int x = (int)Math.Max(0, Math.Min(ptARefHeatMap.X, ptBRefHeatMap.X) - radiusRefHeatMap); x < (int)(Math.Min(nbCellInBaseHeatMapWidth, Math.Max(ptARefHeatMap.X, ptBRefHeatMap.X) + radiusRefHeatMap)); x++)
                        {
                            if (BaseHeatMapData[y, x] > -1) //On regarde si on n'est pas dans une zone exclue (valeur <= -1)
                            {
                                double distanceRefHeatMap = Toolbox.DistancePointToSegment(new PointD(x, y), ptARefHeatMap, ptBRefHeatMap);
                                BaseHeatMapData[y, x] += strength * Math.Max(0, 1 - distanceRefHeatMap / radiusRefHeatMap);
                            }
                            else
                                BaseHeatMapData[y, x] = -1;
                        }
                    }
                }
            }

            if (evaluateHeatMapGeneration)
            {
                sw.Stop();
                heatMapGenerationLog += "\nHeatMap Preferred Segment Zones : " + sw.Elapsed.TotalMilliseconds + " ms - " + preferredSegmentZoneList.Count + " segments";
                sw.Restart();
            }

            lock (avoidanceConicalZoneList)
            {
                foreach (var conicalZone in avoidanceConicalZoneList)
                {
                    int[] listAbscissesZoneExclusionInferieure, listAbscissesZoneExclusionSuperieure;
                    DefineZoneConique(conicalZone.InitPoint, conicalZone.Cible, conicalZone.Radius, out listAbscissesZoneExclusionInferieure, out listAbscissesZoneExclusionSuperieure);
                    Parallel.For(0, nbCellInBaseHeatMapWidth, i =>
                    {
                        for (int j = Math.Max(0, listAbscissesZoneExclusionInferieure[i]); j <= Math.Min(nbCellInBaseHeatMapHeight - 1, listAbscissesZoneExclusionSuperieure[i]); j++)
                        {
                            BaseHeatMapData[j, i] -= 0.5;
                        }
                    }
                    );
                }
            }

            if (evaluateHeatMapGeneration)
            {
                sw.Stop();
                heatMapGenerationLog += "\nHeatMap Avoidance Conical Zones : " + sw.Elapsed.TotalMilliseconds + " ms - " + avoidanceConicalZoneList.Count + " zones";
                Console.WriteLine(heatMapGenerationLog);
                //Trace.WriteLine(heatMapGenerationLog);
                sw.Restart();
            }

        }

        public void ExcludeMaskedZones(PointD robotLocation, List<LocationExtended> obstacleLocationList, double exclusionRadius, Location mapCenter)
        {
            Stopwatch sw = new Stopwatch();
                        
            if (evaluateHeatMapGeneration)
            {
                sw.Restart();
            }

            /// On calcule la pénalisation sur la liste des obstacles à éviter
            
            lock (obstacleLocationList)
            {
                foreach (var obstacle in obstacleLocationList)
                {
                    int[] listAbscissesZoneExclusionInferieure, listAbscissesZoneExclusionSuperieure;

                    DefineZoneConique(Toolbox.OffsetLocation(robotLocation, mapCenter), Toolbox.OffsetLocation(new PointD(obstacle.X, obstacle.Y), mapCenter), exclusionRadius, out listAbscissesZoneExclusionInferieure, out listAbscissesZoneExclusionSuperieure);
                    Parallel.For(0, nbCellInBaseHeatMapWidth, i =>
                    {
                        for (int j = Math.Max(0, listAbscissesZoneExclusionInferieure[i]); j <= Math.Min(nbCellInBaseHeatMapHeight - 1, listAbscissesZoneExclusionSuperieure[i]); j++)
                        {
                            BaseHeatMapData[j, i] = -1;
                        }
                    }
                    );
                }
            }

            if (evaluateHeatMapGeneration)
            {
                sw.Stop();
                heatMapGenerationLog = "\nRobot: " + robotId + " - HeatMap Avoidance Conical Zones Obstacles : " + sw.Elapsed.TotalMilliseconds + " ms - " + obstacleLocationList.Count + " obstacles";
                Console.WriteLine(heatMapGenerationLog);
                //Trace.WriteLine(heatMapGenerationLog);
                sw.Restart();
            }

        }

        private void DefineZoneConique(PointD initPoint, PointD obstacle, double exclusionRadius, out int[] listAbscissesZoneExclusionInf, out int[] listAbscissesZoneExclusionSup)
        {
            int nbPolygonPoints = 12;

            var InitPointRefHeatMap = GetBaseHeatMapPosFromFieldCoordinates(new PointD(initPoint.X, initPoint.Y));
            var centerObstacleRefHeatMap = GetBaseHeatMapPosFromFieldCoordinates(new PointD(obstacle.X, obstacle.Y));
            var exclusionRadiusRefHeatMap = GetBaseHeatMapDistanceFromFieldDistance(exclusionRadius);

            int[] listAbscissesZoneExclusionInferieure = new int[nbCellInBaseHeatMapWidth];
            int[] listAbscissesZoneExclusionSuperieure = new int[nbCellInBaseHeatMapWidth];
            for (int i = 0; i < nbCellInBaseHeatMapWidth; i++)
            {
                listAbscissesZoneExclusionInferieure[i] = nbCellInBaseHeatMapHeight - 1;
            }


            /// Si le point est distant de moins de la distance d'exclusion, on est dans un cas particulier ou il faut éliminr un demi plan et la boule
            if (Toolbox.Distance(InitPointRefHeatMap, centerObstacleRefHeatMap) < exclusionRadiusRefHeatMap)
            {
                //On élimine les boules autour des obstacles
                for (int i = Math.Max(0, (int)(centerObstacleRefHeatMap.X - exclusionRadiusRefHeatMap)); i < Math.Min(nbCellInBaseHeatMapWidth, centerObstacleRefHeatMap.X + exclusionRadiusRefHeatMap); i++)
                {
                    for (int j = Math.Max(0, (int)(centerObstacleRefHeatMap.Y - exclusionRadiusRefHeatMap)); j < Math.Min(nbCellInBaseHeatMapHeight, centerObstacleRefHeatMap.Y + exclusionRadiusRefHeatMap); j++)
                    {
                        BaseHeatMapData[j, i] = -1;
                    }
                }

                /// Le demi plan passant par le robot et normal au segment robot ostacle est éliminé
                double angleCentreObstacle = Math.Atan2(centerObstacleRefHeatMap.Y - InitPointRefHeatMap.Y, centerObstacleRefHeatMap.X - InitPointRefHeatMap.X);
                if (centerObstacleRefHeatMap.Y > InitPointRefHeatMap.Y)
                {
                    /// L'obstacle est au dessus du robot, on exclut la zone inférieure                            
                    Parallel.For(0, nbCellInBaseHeatMapWidth, x =>
                    //for (int x = 0; x < nbCellInBaseHeatMapWidth; x++)
                    {
                        PointD perimetrePt = new PointD(x, (int)InitPointRefHeatMap.Y + (x - InitPointRefHeatMap.X) * Math.Tan(angleCentreObstacle - Math.PI / 2));
                        SetZoneExclusionInferieure(listAbscissesZoneExclusionInferieure, listAbscissesZoneExclusionSuperieure, perimetrePt);
                        SetZoneExclusionSuperieure(listAbscissesZoneExclusionInferieure, listAbscissesZoneExclusionSuperieure, new PointD(x, nbCellInBaseHeatMapHeight - 1));
                    }
                    );
                }
                else
                {
                    /// L'obstacle est en dessous du robot, on exclut la zone supérieure
                    Parallel.For(0, nbCellInBaseHeatMapWidth, x =>
                    //for (int x = 0; x < nbCellInBaseHeatMapWidth; x++)
                    {
                        PointD perimetrePt = new PointD(x, (int)InitPointRefHeatMap.Y + (x - InitPointRefHeatMap.X) * Math.Tan(angleCentreObstacle - Math.PI / 2));
                        SetZoneExclusionSuperieure(listAbscissesZoneExclusionInferieure, listAbscissesZoneExclusionSuperieure, perimetrePt);
                        SetZoneExclusionInferieure(listAbscissesZoneExclusionInferieure, listAbscissesZoneExclusionSuperieure, new PointD(x, 0));
                    }
                    );
                }

            }
            else
            {
                double angleCentreObstacle = Math.Atan2(centerObstacleRefHeatMap.Y - InitPointRefHeatMap.Y, centerObstacleRefHeatMap.X - InitPointRefHeatMap.X);

                /// Pour chaque obstacle, on détermine les points d'un polygone l'entourant à n cotés pour approximer le cercle 
                /// d'exclusion du point obstacle considéré.
                /// Pour chacun de ces points, on détermine l'angle Robot-Point, et on trouve le plus grand et le plus petit qui forment les
                /// angles max et min du cone d'exclusion.
                /// 

                List<PointD> listPtsPolygonExclusion = new List<PointD>();
                List<double> listAnglePtsPolygonExclusion = new List<double>();
                for (int i = 0; i < nbPolygonPoints; i++)
                {
                    listPtsPolygonExclusion.Add(new PointD(centerObstacleRefHeatMap.X + exclusionRadiusRefHeatMap * Math.Cos(2 * Math.PI / nbPolygonPoints * i), centerObstacleRefHeatMap.Y + exclusionRadiusRefHeatMap * Math.Sin(2 * Math.PI / nbPolygonPoints * i)));
                    /// On détermine l'angle de chacun des points de la zone d'exclusion du polygon centré autour de l'obstacle
                    double anglePt = Math.Atan2(listPtsPolygonExclusion[i].Y - InitPointRefHeatMap.Y, listPtsPolygonExclusion[i].X - InitPointRefHeatMap.X);
                    /// On décale cet angle autour de l'angle du centre de l'obstacle pour éviter les discontinuités 0 / 2*PI
                    anglePt = Toolbox.ModuloByAngle(angleCentreObstacle, anglePt);
                    listAnglePtsPolygonExclusion.Add(anglePt);
                }

                double minAngle = listAnglePtsPolygonExclusion.Min();
                int indexAngleMin = listAnglePtsPolygonExclusion.IndexOf(minAngle);
                PointD ptAngleMin = listPtsPolygonExclusion[indexAngleMin];

                double maxAngle = listAnglePtsPolygonExclusion.Max();
                int indexAngleMax = listAnglePtsPolygonExclusion.IndexOf(maxAngle);
                PointD ptAngleMax = listPtsPolygonExclusion[indexAngleMax];

                ptAngleMin.X = Toolbox.LimitToInterval(ptAngleMin.X, 0, nbCellInBaseHeatMapWidth - 1);
                ptAngleMin.Y = Toolbox.LimitToInterval(ptAngleMin.Y, 0, nbCellInBaseHeatMapHeight - 1);
                ptAngleMax.X = Toolbox.LimitToInterval(ptAngleMax.X, 0, nbCellInBaseHeatMapWidth - 1);
                ptAngleMax.Y = Toolbox.LimitToInterval(ptAngleMax.Y, 0, nbCellInBaseHeatMapHeight - 1);


                /// On connait les deux points de tangence de la zone d'exclusion ptMinAngle et ptMaxAngle
                /// On va éliminer les pts masqué par balayage en X uniquement, on veut donc un pt de périmètre 
                /// pour chaque valeur de X entre ptAngleMin.X et nbCellInBaseHeatMapWidth si Angle Min est compris entre -PI/2 et PI/2
                /// On peut s'arrêter avant si on a atteint le bord supérieur ou inférieur de la HeatMap
                /// 
                /// pour chaque valeur de X entre ptAngleMin.X et 0 si Angle Min n'est pas compris entre -PI/2 et PI/2
                /// On peut s'arrêter avant si on a atteint le bord supérieur ou inférieur de la HeatMap
                //List<PointD> listPtsPerimetreExclusion = new List<PointD>();


                /// Attention : algo un peu compliqué... 
                /// Si les angles min et max sont dans le même demi-plan vertical par rapport au robot 
                /// alors on est dans un cas ou il y a une partie supérieure et une partie inférieure au cone d'exclusion
                /// Si ce n'est pas le cas, on est de par et d'autre de la verticale : il n'y a donc qu'une partie supérieure OU une partie inférieure
                /// 

                if (((Math.Abs(minAngle) < Math.PI / 2) && (Math.Abs(maxAngle) < Math.PI / 2))
                    || ((Math.Abs(minAngle) >= Math.PI / 2) && (Math.Abs(maxAngle) >= Math.PI / 2)))
                {
                    ///On est dans la situation classique avec une partie sup et une partie inférieure

                    /// Partie inférieure du périmètre d'exclusion
                    if (Math.Abs(minAngle) < Math.PI / 2)
                    {
                        /// On commence par la portion linéaire du cone
                        Parallel.For((int)ptAngleMin.X, nbCellInBaseHeatMapWidth, x =>
                        //for (int x = (int)ptAngleMin.X; x < nbCellInBaseHeatMapWidth; x++)
                        {
                            PointD perimetrePt = new PointD(x, (int)ptAngleMin.Y + (x - ptAngleMin.X) * Math.Tan(minAngle));
                            SetZoneExclusionInferieure(listAbscissesZoneExclusionInferieure, listAbscissesZoneExclusionSuperieure, perimetrePt);
                        }
                        );
                        /// On gère la partie circulaire
                        Parallel.For((int)(centerObstacleRefHeatMap.X - exclusionRadiusRefHeatMap), (int)ptAngleMin.X + 1, x =>
                        //for (int x = (int)(centerObstacleRefHeatMap.X - exclusionRadiusRefHeatMap); x <= ptAngleMin.X; x++)
                        {
                            int y = (int)(centerObstacleRefHeatMap.Y - Math.Sqrt(Math.Max(0, exclusionRadiusRefHeatMap * exclusionRadiusRefHeatMap - (x - centerObstacleRefHeatMap.X) * (x - centerObstacleRefHeatMap.X))));
                            PointD perimetrePt = new PointD(x, y);
                            SetZoneExclusionInferieure(listAbscissesZoneExclusionInferieure, listAbscissesZoneExclusionSuperieure, perimetrePt);
                        }
                        );
                    }
                    else
                    {
                        /// Si on est dans la partie abs(angle)>Pi/2, le minAngle est en haut, et le maxAngle est en bas ! Attention !!!
                        Parallel.For(0, (int)ptAngleMax.X + 1, x =>
                        //for (int x = (int)ptAngleMax.X; x >= 0; x--)
                        {
                            PointD perimetrePt = new PointD(x, (int)ptAngleMax.Y + (x - ptAngleMax.X) * Math.Tan(maxAngle));
                            SetZoneExclusionInferieure(listAbscissesZoneExclusionInferieure, listAbscissesZoneExclusionSuperieure, perimetrePt);
                        }
                        );

                        /// On gère la partie circulaire
                        Parallel.For((int)ptAngleMax.X, (int)(centerObstacleRefHeatMap.X + exclusionRadiusRefHeatMap) + 1, x =>
                        //for (int x = (int)ptAngleMax.X; x <= (int)(centerObstacleRefHeatMap.X + exclusionRadiusRefHeatMap); x++)
                        {
                            int y = (int)(centerObstacleRefHeatMap.Y - Math.Sqrt(Math.Max(0, exclusionRadiusRefHeatMap * exclusionRadiusRefHeatMap - (x - centerObstacleRefHeatMap.X) * (x - centerObstacleRefHeatMap.X))));
                            PointD perimetrePt = new PointD(x, y);
                            SetZoneExclusionInferieure(listAbscissesZoneExclusionInferieure, listAbscissesZoneExclusionSuperieure, perimetrePt);
                        }
                        );
                    }

                    /// Partie supérieure du périmètre d'exclusion
                    if (Math.Abs(maxAngle) < Math.PI / 2)
                    {
                        Parallel.For((int)ptAngleMax.X, nbCellInBaseHeatMapWidth, x =>
                        //for (int x = (int)ptAngleMax.X; x < nbCellInBaseHeatMapWidth; x++)
                        {
                            PointD perimetrePt = new PointD(x, (int)ptAngleMax.Y + (x - ptAngleMax.X) * Math.Tan(maxAngle));
                            SetZoneExclusionSuperieure(listAbscissesZoneExclusionInferieure, listAbscissesZoneExclusionSuperieure, perimetrePt);
                        }
                        );
                        /// On gère la partie circulaire
                        Parallel.For((int)(centerObstacleRefHeatMap.X - exclusionRadiusRefHeatMap), (int)(ptAngleMax.X) + 1, x =>
                        //for (int x = (int)(centerObstacleRefHeatMap.X - exclusionRadiusRefHeatMap); x <= ptAngleMax.X; x++)
                        {
                            int y = (int)(centerObstacleRefHeatMap.Y + Math.Sqrt(Math.Max(0, exclusionRadiusRefHeatMap * exclusionRadiusRefHeatMap - (x - centerObstacleRefHeatMap.X) * (x - centerObstacleRefHeatMap.X))));
                            PointD perimetrePt = new PointD(x, y);
                            SetZoneExclusionSuperieure(listAbscissesZoneExclusionInferieure, listAbscissesZoneExclusionSuperieure, perimetrePt);
                        }
                        );
                    }
                    else
                    {
                        /// Si on est dans la partie abs(angle)>Pi/2, le minAngle est en haut, et le maxAngle est en bas ! Attention !!!
                        Parallel.For(0, (int)ptAngleMin.X + 1, x =>
                        //for (int x = (int)ptAngleMin.X; x >= 0; x--)
                        {
                            PointD perimetrePt = new PointD(x, (int)ptAngleMin.Y + (x - ptAngleMin.X) * Math.Tan(minAngle));
                            SetZoneExclusionSuperieure(listAbscissesZoneExclusionInferieure, listAbscissesZoneExclusionSuperieure, perimetrePt);
                        }
                        );

                        /// On gère la partie circulaire
                        Parallel.For((int)ptAngleMin.X, (int)(centerObstacleRefHeatMap.X + exclusionRadiusRefHeatMap) + 1, x =>
                        //for (int x = (int)ptAngleMin.X; x <= (int)(centerObstacleRefHeatMap.X + exclusionRadiusRefHeatMap); x++)
                        {
                            int y = (int)(centerObstacleRefHeatMap.Y + Math.Sqrt(Math.Max(0, exclusionRadiusRefHeatMap * exclusionRadiusRefHeatMap - (x - centerObstacleRefHeatMap.X) * (x - centerObstacleRefHeatMap.X))));
                            PointD perimetrePt = new PointD(x, y);
                            SetZoneExclusionSuperieure(listAbscissesZoneExclusionInferieure, listAbscissesZoneExclusionSuperieure, perimetrePt);
                        }
                        );
                    }
                }
                else
                {
                    //On gère les cas où le minAngle et le maxAngle sont dans des cadrans différents
                    /// Partie inférieure du périmètre d'exclusion
                    if (Toolbox.Modulo2PiAngleRad(angleCentreObstacle) < 0)
                    {
                        //On est dans le demi-plan inférieur, et on n'a que des périmètre supérieur
                        //Angle Max est forcément supérieur à -Pi/2
                        Parallel.For((int)ptAngleMax.X, nbCellInBaseHeatMapWidth, x =>
                        //for (int x = (int)ptAngleMax.X; x < nbCellInBaseHeatMapWidth; x++)
                        {
                            PointD perimetrePt = new PointD(x, (int)ptAngleMax.Y + (x - ptAngleMax.X) * Math.Tan(maxAngle));
                            SetZoneExclusionSuperieure(listAbscissesZoneExclusionInferieure, listAbscissesZoneExclusionSuperieure, perimetrePt);
                            SetZoneExclusionInferieure(listAbscissesZoneExclusionInferieure, listAbscissesZoneExclusionSuperieure, new PointD(x, 0));
                        }
                        );
                        //Angle Min est forcément inférieur à -Pi/2
                        Parallel.For(0, (int)ptAngleMin.X + 1, x =>
                        //for (int x = (int)ptAngleMin.X; x >= 0; x--)
                        {
                            PointD perimetrePt = new PointD(x, (int)ptAngleMin.Y + (x - ptAngleMin.X) * Math.Tan(minAngle));
                            SetZoneExclusionSuperieure(listAbscissesZoneExclusionInferieure, listAbscissesZoneExclusionSuperieure, perimetrePt);
                            SetZoneExclusionInferieure(listAbscissesZoneExclusionInferieure, listAbscissesZoneExclusionSuperieure, new PointD(x, 0));
                        }
                        );

                        ///// On gère la partie circulaire
                        Parallel.For((int)ptAngleMin.X, (int)ptAngleMax.X + 1, x =>
                        //for (int x = (int)ptAngleMin.X; x <= ptAngleMax.X; x++)
                        {
                            int y = (int)(centerObstacleRefHeatMap.Y + Math.Sqrt(Math.Max(0, exclusionRadiusRefHeatMap * exclusionRadiusRefHeatMap - (x - centerObstacleRefHeatMap.X) * (x - centerObstacleRefHeatMap.X))));
                            PointD perimetrePt = new PointD(x, y);
                            SetZoneExclusionSuperieure(listAbscissesZoneExclusionInferieure, listAbscissesZoneExclusionSuperieure, perimetrePt);
                            SetZoneExclusionInferieure(listAbscissesZoneExclusionInferieure, listAbscissesZoneExclusionSuperieure, new PointD(x, 0));
                        }
                        );
                    }

                    else
                    {
                        //On est dans le demi-plan supérieur, et on n'a que des périmètre supérieur
                        //Angle Max est forcément inférieur à -Pi/2
                        /// On commence par la portion linéaire du cone
                        Parallel.For((int)ptAngleMin.X, nbCellInBaseHeatMapWidth, x =>
                        //for (int x = (int)ptAngleMin.X; x < nbCellInBaseHeatMapWidth; x++)
                        {
                            PointD perimetrePt = new PointD(x, (int)ptAngleMin.Y + (x - ptAngleMin.X) * Math.Tan(minAngle));
                            SetZoneExclusionInferieure(listAbscissesZoneExclusionInferieure, listAbscissesZoneExclusionSuperieure, perimetrePt);
                            SetZoneExclusionSuperieure(listAbscissesZoneExclusionInferieure, listAbscissesZoneExclusionSuperieure, new PointD(x, nbCellInBaseHeatMapHeight - 1));
                        }
                        );
                        Parallel.For(0, (int)ptAngleMax.X + 1, x =>
                        //for (int x = (int)ptAngleMax.X; x >= 0; x--)
                        {
                            PointD perimetrePt = new PointD(x, (int)ptAngleMax.Y + (x - ptAngleMax.X) * Math.Tan(maxAngle));
                            SetZoneExclusionInferieure(listAbscissesZoneExclusionInferieure, listAbscissesZoneExclusionSuperieure, perimetrePt);
                            SetZoneExclusionSuperieure(listAbscissesZoneExclusionInferieure, listAbscissesZoneExclusionSuperieure, new PointD(x, nbCellInBaseHeatMapHeight - 1));
                        }
                        );
                        /// On gère la partie circulaire
                        Parallel.For((int)ptAngleMax.X, (int)ptAngleMin.X + 1, x =>
                        //for (int x = (int)ptAngleMax.X; x <= (int)ptAngleMin.X; x++)
                        {
                            int y = (int)(centerObstacleRefHeatMap.Y - Math.Sqrt(Math.Max(0, exclusionRadiusRefHeatMap * exclusionRadiusRefHeatMap - (x - centerObstacleRefHeatMap.X) * (x - centerObstacleRefHeatMap.X))));
                            PointD perimetrePt = new PointD(x, y);
                            SetZoneExclusionInferieure(listAbscissesZoneExclusionInferieure, listAbscissesZoneExclusionSuperieure, perimetrePt);
                            SetZoneExclusionSuperieure(listAbscissesZoneExclusionInferieure, listAbscissesZoneExclusionSuperieure, new PointD(x, nbCellInBaseHeatMapHeight - 1));
                        }
                        );
                    }
                }

                //On affiche les points périmètre exclus pour le DEBUG
                //foreach (var pt in listPtsPerimetreExclusion)
                //{
                //    BaseHeatMapData[(int)pt.Y, (int)pt.X] = -1;
                //}
            }
            listAbscissesZoneExclusionInf = listAbscissesZoneExclusionInferieure;
            listAbscissesZoneExclusionSup = listAbscissesZoneExclusionSuperieure;
        }

        private void SetZoneExclusionInferieure(int[] listAbscissesZoneExclusionInferieure, int[] listAbscissesZoneExclusionSuperieure, PointD perimetrePt)
        {
            perimetrePt.Y = Toolbox.LimitToInterval(perimetrePt.Y, 0, nbCellInBaseHeatMapHeight - 1);
            perimetrePt.X = Toolbox.LimitToInterval(perimetrePt.X, 0, nbCellInBaseHeatMapWidth-1);
            //listPtsPerimetreExclusion.Add(perimetrePt);
            listAbscissesZoneExclusionInferieure[(int)perimetrePt.X] = (int)perimetrePt.Y;
        }

        private void SetZoneExclusionSuperieure(int[] listAbscissesZoneExclusionInferieure, int[] listAbscissesZoneExclusionSuperieure, PointD perimetrePt)
        {
            perimetrePt.Y = Toolbox.LimitToInterval(perimetrePt.Y, 0, nbCellInBaseHeatMapHeight - 1);
            perimetrePt.X = Toolbox.LimitToInterval(perimetrePt.X, 0, nbCellInBaseHeatMapWidth - 1);
            //listPtsPerimetreExclusion.Add(perimetrePt);
            listAbscissesZoneExclusionSuperieure[(int)perimetrePt.X] = (int)perimetrePt.Y;
        }

        public PointD GetOptimalPosition()
        {

            //Détermination
            double[] tabMax = new double[nbCellInBaseHeatMapHeight];
            //double[] tabMin = new double[nbCellInBaseHeatMapHeight];
            int[] tabIndexMax = new int[nbCellInBaseHeatMapHeight];
            //int[] tabIndexMin = new int[nbCellInBaseHeatMapHeight];

            //double max = Double.NegativeInfinity;
            //double min = 0;
            //int xMax = 0;
            //int yMax = 0;
            //for (int i = 0; i < nbCellInBaseHeatMapHeight; i++)
            //{
            //    for (int j = 0; j < nbCellInBaseHeatMapWidth; j++)
            //    {
            //        if (BaseHeatMapData[i, j] > max)
            //        {
            //            max = BaseHeatMapData[i, j];
            //            yMax = i;
            //            xMax = j;
            //        }
            //    }
            //}

            ////On a le point dans le référentiel de la heatMap, on la passe en référentiel Terrain
            //return GetFieldPosFromBaseHeatMapCoordinates(xMax, yMax);


            Parallel.For(0, nbCellInBaseHeatMapHeight, i =>
            //for (int i = 0; i < nbCellInBaseHeatMapHeight; i++)
            {
                tabMax[i] = 0;
                tabIndexMax[i] = 0;
                for (int j = 0; j < nbCellInBaseHeatMapWidth; j++)
                {
                    if (BaseHeatMapData[i, j] > tabMax[i])
                    {
                        tabMax[i] = BaseHeatMapData[i, j];
                        tabIndexMax[i] = j;
                    }
                }
            }
            );

            //Recherche du maximum
            double max = 0;
            int indexMax = 0;

            for (int i = 0; i < nbCellInBaseHeatMapHeight; i++)
            {
                if (tabMax[i] > max)
                {
                    max = tabMax[i];
                    indexMax = i;
                }
            }

            int maxYpos = indexMax;// indexMax % heatMap.nbCellInBaseHeatMapWidth;
            int maxXpos = tabIndexMax[indexMax];// indexMax / heatMap.nbCellInBaseHeatMapWidth;
            if (maxXpos == 0 && maxYpos == 0 && ((max == 0) || (max == -1)))
                return null;
            else
            {
                //On a le point dans le référentiel de la heatMap, on la passe en référentiel Terrain
                //return new PointD(maxXpos, maxYpos);
                return GetFieldPosFromBaseHeatMapCoordinates(maxXpos, maxYpos);
            }

        }
    }
}
