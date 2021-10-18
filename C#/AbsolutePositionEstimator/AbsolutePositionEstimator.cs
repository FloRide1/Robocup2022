using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using EventArgsLibrary;
using LidarProcessor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace AbsolutePositionEstimatorNS
{
    public class AbsolutePositionEstimator
    {
        int robotId = 0;
        List<PolarPointRssi> LidarPtList;
        Location currentLocation;
        bool mirrorMode = false;

        public AbsolutePositionEstimator(int id)
        {
            robotId = id;
        }

        public void OnRawLidarDataReceived(object sender, RawLidarArgs e)
        {
            LidarPtList = e.PtList;
        }

        public void OnPhysicalPositionReceived(object sender, EventArgsLibrary.LocationArgs e)
        {
            //On récupère la position courante
            if (robotId == e.RobotId)
            {
                currentLocation = e.Location;
            }
        }

        public void OnMirrorModeReceived(object sender, BoolEventArgs e)
        {
            mirrorMode = e.value;
        }

        public void OnLidarBalisesListExtractedEvent(object sender, LidarDetectedObjectListArgs e)
        {
            ///On a reçu la liste des balises possibles
            var BalisesPotentielleList = e.LidarObjectList;

            /// Détermination des positions absolues possibles pour le robot
            var possibleLocations = EurobotAbsolutePositionEvaluation(BalisesPotentielleList);

            /// Détermination de la meilleure location, celle qui correspond le mieux au positionnement courant
            var bestLocation = FindBestPossibleLocation(possibleLocations);

            /// Transmission du positionnement déterminé au robot (filtre de kalman par exemple)
            if (bestLocation != null)
                OnPositionCalculatedEvent(bestLocation.X, bestLocation.Y, bestLocation.Theta, 1);

        }

        public List<Location> EurobotAbsolutePositionEvaluation(List<LidarDetectedObject> BalisesPotentielleList)
        {
            //On commence par générer une liste des balises pouvant exister (allié et adversaire)
            Dictionary<int, PointD> BalisesTheoriquesList = new Dictionary<int, PointD>();
            BalisesTheoriquesList.Add(0, new PointD(-1.55, 0.95));
            BalisesTheoriquesList.Add(1, new PointD(-1.55, 0));
            BalisesTheoriquesList.Add(2, new PointD(-1.55, -0.95));
            BalisesTheoriquesList.Add(3, new PointD(1.55, 0.95));
            BalisesTheoriquesList.Add(4, new PointD(1.55, 0));
            BalisesTheoriquesList.Add(5, new PointD(1.55, -0.95));

            //Génération de la liste des triplets théoriques possibles
            Dictionary<int, TripletBalise> TripletBalisesTheoriquesDict = new Dictionary<int, TripletBalise>();
            for (int i = 0; i < BalisesTheoriquesList.Count; i++)
            {
                for (int j = 0; j < BalisesTheoriquesList.Count; j++)
                {
                    for (int k = 0; k < BalisesTheoriquesList.Count; k++)
                    {
                        if (i != j && i != k && j != k)
                        {
                            int id = i * 100 + j * 10 + k;
                            var B1 = BalisesTheoriquesList[i];
                            var B2 = BalisesTheoriquesList[j];
                            var B3 = BalisesTheoriquesList[k];

                            var distance12 = Toolbox.Distance(B1, B2);
                            var distance13 = Toolbox.Distance(B1, B3);
                            var angle123 = Toolbox.Modulo2PiAngleRad(Math.Atan2(B3.Y - B1.Y, B3.X - B1.X) - Math.Atan2(B2.Y - B1.Y, B2.X - B1.X));
                            TripletBalisesTheoriquesDict.Add(id, new TripletBalise(distance12, distance13, angle123));
                        }
                    }
                }
            }

            //Génération de la liste des doublets théoriques possibles
            Dictionary<int, DoubletBalise> DoubletBalisesTheoriquesDict = new Dictionary<int, DoubletBalise>();
            for (int i = 0; i < BalisesTheoriquesList.Count; i++)
            {
                for (int j = 0; j < BalisesTheoriquesList.Count; j++)
                {
                        if (i != j )
                        {
                            int id = i * 10 + j;
                            var B1 = BalisesTheoriquesList[i];
                            var B2 = BalisesTheoriquesList[j];

                            var distance12 = Toolbox.Distance(B1, B2);
                            DoubletBalisesTheoriquesDict.Add(id, new DoubletBalise(distance12));
                        }
                }
            }

            ///Si on a 3 balises ou plus            
            List<Location> possibleRobotLocations = new List<Location>();
            if (BalisesPotentielleList.Count >= 3)
            {
                /// Si on a 3 balises ou plus, on prend les 3 dont la sommes des distances relatives est la plus grande
                /// On commence par générer les sommes des distances relatives
                Dictionary<int, double> BalisesDistancesSumDict = new Dictionary<int, double>();
                for (int i = 0; i < BalisesPotentielleList.Count; i++)
                {
                    for (int j = 0; j < BalisesPotentielleList.Count; j++)
                    {
                        for (int k = 0; k < BalisesPotentielleList.Count; k++)
                        {
                            if (i != j && i != k && j != k)
                            {
                                int id = i * 100 + j * 10 + k;
                                var BP1 = BalisesPotentielleList[i];
                                var BP2 = BalisesPotentielleList[j];
                                var BP3 = BalisesPotentielleList[k];

                                var BPt1 = new PointD(BP1.XMoyen, BP1.YMoyen);
                                var BPt2 = new PointD(BP2.XMoyen, BP2.YMoyen);
                                var BPt3 = new PointD(BP3.XMoyen, BP3.YMoyen);

                                var distanceBP12 = Toolbox.Distance(BPt1, BPt2);
                                var distanceBP13 = Toolbox.Distance(BPt1, BPt3);
                                var distanceBP23 = Toolbox.Distance(BPt2, BPt3);
                                BalisesDistancesSumDict.Add(id, Math.Sqrt(Math.Pow(distanceBP13, 2) + Math.Pow(distanceBP13, 2) + Math.Pow(distanceBP13, 2)));
                            }
                        }
                    }
                }

                /// On cherche le max des distances relatives
                var maxDistanceSum = BalisesDistancesSumDict.Values.Max();
                /// On cherche l'index du max des distances relatives
                var TripletMaxDistId = BalisesDistancesSumDict.FirstOrDefault(x => x.Value == maxDistanceSum);
                /// On récupère les indices des balises à utiliser
                var IndexB1 = (int)(TripletMaxDistId.Key / 100);
                var IndexB2 = (int)((TripletMaxDistId.Key % 100) / 10);
                var IndexB3 = (int)(TripletMaxDistId.Key % 10);
                                
                var B1 = new PointD(BalisesPotentielleList[IndexB1].XMoyen, BalisesPotentielleList[IndexB1].YMoyen);
                var B2 = new PointD(BalisesPotentielleList[IndexB2].XMoyen, BalisesPotentielleList[IndexB2].YMoyen);
                var B3 = new PointD(BalisesPotentielleList[IndexB3].XMoyen, BalisesPotentielleList[IndexB3].YMoyen);
                List<PointD> listeBalisesPotentielles = new List<PointD>();
                listeBalisesPotentielles.Add(B1);
                listeBalisesPotentielles.Add(B2);
                listeBalisesPotentielles.Add(B3);

                /// On calcule les caractéristiques du triplet de balises choises
                var distance12 = Toolbox.Distance(B1, B2);
                var distance13 = Toolbox.Distance(B1, B3);
                var angle123 = Toolbox.Modulo2PiAngleRad(Math.Atan2(B3.Y - B1.Y, B3.X - B1.X) - Math.Atan2(B2.Y - B1.Y, B2.X - B1.X));
                var TripletBalisesReelles = new TripletBalise(distance12, distance13, angle123);

                Dictionary<int, double> scoreTripletDict = new Dictionary<int, double>();

                ///On évalue le score du triplet de balises choisies par rapport au triplets théoriques
                double toleranceDistance = 0.1;
                double toleranceAngle = 0.05;
                foreach (var tripletTheorique in TripletBalisesTheoriquesDict)
                {
                    double score = Math.Max(1 - Math.Abs(TripletBalisesReelles.Distance12 - tripletTheorique.Value.Distance12) / toleranceDistance, 0) 
                        * Math.Max(1 - Math.Abs(TripletBalisesReelles.Distance13 - tripletTheorique.Value.Distance13) / toleranceDistance, 0) 
                        * Math.Max(1 - Math.Abs(TripletBalisesReelles.Angle123 - tripletTheorique.Value.Angle123) / toleranceAngle, 0);
                    scoreTripletDict.Add(tripletTheorique.Key, score);
                }

                //On recherche les triplets ayant les meilleurs scores
                //On récupère l'index du rssi max (pour les 3 lignes)
                var scoreMax = scoreTripletDict.Max(p => p.Value);
                var bestTripletList = scoreTripletDict.Where(x => x.Value == scoreMax).ToList();

                //A présent on détermine la liste des positionnements possibles associés aux triplets théoriques ayant le score max
                foreach(var tripletTheorique in bestTripletList)
                {
                    List<int> balisesTripletIndexList = new List<int>();
                    balisesTripletIndexList.Add((int)(tripletTheorique.Key / 100));
                    balisesTripletIndexList.Add((int)((tripletTheorique.Key % 100) / 10));
                    balisesTripletIndexList.Add((int)(tripletTheorique.Key % 10));

                    /// Pour chaque couple de balises du triplet, on détermine un positionnement possible
                    for(int i=0; i<balisesTripletIndexList.Count; i++)
                    {
                        for(int j=i+1; j<balisesTripletIndexList.Count;j++)
                        {
                            var B1Theorique = BalisesTheoriquesList[balisesTripletIndexList[i]];
                            var B2Theorique = BalisesTheoriquesList[balisesTripletIndexList[j]];

                            var B1Reelle = listeBalisesPotentielles[i];
                            var B2Reelle = listeBalisesPotentielles[j];

                            //Calcul de l'angle entre le vecteur 1-3 et le vecteur 1-Robot
                            double angleVector12Vector1Robot = Math.Atan2(B2Reelle.Y - B1Reelle.Y, B2Reelle.X - B1Reelle.X) - Math.Atan2(0 - B1Reelle.Y, 0 - B1Reelle.X);   //Validé
                            double angleVector12 = Math.Atan2(B2Theorique.Y - B1Theorique.Y, B2Theorique.X - B1Theorique.X);            //Validé
                            double normVector1Robot = Toolbox.Distance(B1Reelle, new PointD(0, 0));                                           //Validé
                            double xRobot = B1Theorique.X + normVector1Robot * Math.Cos(angleVector12 - angleVector12Vector1Robot);       //Validé
                            double yRobot = B1Theorique.Y + normVector1Robot * Math.Sin(angleVector12 - angleVector12Vector1Robot);       //Validé
                            double angleRobot1ThVectorRobot1 = Toolbox.Modulo2PiAngleRad(Math.Atan2(B1Theorique.Y - yRobot, B1Theorique.X - xRobot) - Math.Atan2(B1Reelle.Y, B1Reelle.X));  //Non Validé

                            possibleRobotLocations.Add(new Location(xRobot, yRobot, angleRobot1ThVectorRobot1, 0, 0, 0));
                        }
                    }

                    //On calcule les différents positionnement possibles à l'aide des 3 combinaisons de balises possible du triplet

                    ////Calcul de l'angle entre le vecteur 1-3 et le vecteur 1-Robot
                    //double angleVector13Vector1Robot = Math.Atan2(B3.Y - B1.Y, B3.X - B1.X) - Math.Atan2(0 - B1.Y, 0 - B1.X);   //Validé
                    //double angleVector13 = Math.Atan2(B3Theorique.Y - B1Theorique.Y, B3Theorique.X - B1Theorique.X);            //Validé
                    //double normVector1Robot = Toolbox.Distance(B1, new PointD(0, 0));                                           //Validé
                    //double xRobot = B1Theorique.X + normVector1Robot * Math.Cos(angleVector13-angleVector13Vector1Robot);       //Validé
                    //double yRobot = B1Theorique.Y + normVector1Robot * Math.Sin(angleVector13-angleVector13Vector1Robot);       //Validé
                    //double angleRobot1ThVectorRobot1 = Toolbox.Modulo2PiAngleRad(Math.Atan2(B1Theorique.Y - yRobot, B1Theorique.X - xRobot) - Math.Atan2(B1.Y, B1.X));  //Non Validé

                    //possibleRobotLocations.Add(new Location(xRobot, yRobot, angleRobot1ThVectorRobot1, 0, 0, 0));
                }
            }

            if (BalisesPotentielleList.Count == 2)
            {
                var B1 = new PointD(BalisesPotentielleList[0].XMoyen, BalisesPotentielleList[0].YMoyen);
                var B2 = new PointD(BalisesPotentielleList[1].XMoyen, BalisesPotentielleList[1].YMoyen);

                var distance12 = Toolbox.Distance(B1, B2);
                var DoubletBalisesReelles = new DoubletBalise(distance12);

                Dictionary<int, double> scoreDoubletDict = new Dictionary<int, double>();

                ///On évalue le score du triplet réel par rapport au triplets théoriques
                double toleranceDistance = 0.1;
                double toleranceAngle = 0.05;
                foreach (var doubletTheorique in DoubletBalisesTheoriquesDict)
                {
                    double score = Math.Max(1 - Math.Abs(DoubletBalisesReelles.Distance12 - doubletTheorique.Value.Distance12) / toleranceDistance, 0);
                    scoreDoubletDict.Add(doubletTheorique.Key, score);
                }

                //On recherche les triplets ayant les meilleurs scores
                //On récupère l'index du rssi max (pour les 3 lignes)
                var scoreMax = scoreDoubletDict.Max(p => p.Value);
                var bestDoubletList = scoreDoubletDict.Where(x => x.Value == scoreMax).ToList();

                //A présent on détermine la liste des positionnements possibles associés aux triplets théoriques ayant le score max
                foreach (var doubletTheorique in bestDoubletList)
                {
                    var B1Theorique = BalisesTheoriquesList[(int)(doubletTheorique.Key / 10)];
                    var B2Theorique = BalisesTheoriquesList[(int)(doubletTheorique.Key % 10)];

                    //Calcul de l'angle entre le vecteur 1-2 et le vecteur 1-Robot
                    double angleVector12Vector1Robot = Math.Atan2(B2.Y - B1.Y, B2.X - B1.X) - Math.Atan2(0 - B1.Y, 0 - B1.X);   //Validé
                    double angleVector12 = Math.Atan2(B2Theorique.Y - B1Theorique.Y, B2Theorique.X - B1Theorique.X);            //Validé
                    double normVector1Robot = Toolbox.Distance(B1, new PointD(0, 0));                                           //Validé
                    double xRobot = B1Theorique.X + normVector1Robot * Math.Cos(angleVector12 - angleVector12Vector1Robot);       //Validé
                    double yRobot = B1Theorique.Y + normVector1Robot * Math.Sin(angleVector12 - angleVector12Vector1Robot);       //Validé
                    double angleRobot1ThVectorRobot1 = Toolbox.Modulo2PiAngleRad(Math.Atan2(B1Theorique.Y - yRobot, B1Theorique.X - xRobot) - Math.Atan2(B1.Y, B1.X));  //Non Validé

                    possibleRobotLocations.Add(new Location(xRobot, yRobot, angleRobot1ThVectorRobot1, 0, 0, 0));
                }
            }
            return possibleRobotLocations;
        }


        bool resetLocationRequested=true;
        public Location FindBestPossibleLocation(List<Location> possibleLocationList)
        {
            //On commence par recopier la liste des Location Possibles
            List<Location> possibleLocationListCopy = new List<Location>();
            foreach(var loc in possibleLocationList)
            {
                possibleLocationListCopy.Add(new Location(loc.X, loc.Y, loc.Theta, 0, 0, 0));
            }

            if (currentLocation != null && possibleLocationListCopy.Count>0) 
            {
                if (resetLocationRequested == false)
                {
                    //Dans le cas standard
                    double toleranceDistance = 0.5;
                    double toleranceAngle = 0.2;
                    //On calcul l'écart minimal entre la location courante et les différentes location possibles
                    List<double> locationScoreList = new List<double>();
                    possibleLocationListCopy.ForEach(possibleLocation => locationScoreList.Add(
                        Math.Max(0, 1 - Toolbox.Distance(new PointD(possibleLocation.X, possibleLocation.Y), new PointD(currentLocation.X, currentLocation.Y)) / toleranceDistance)
                        * Math.Max(0, 1 - Math.Abs(currentLocation.Theta - Toolbox.ModuloByAngle(currentLocation.Theta, possibleLocation.Theta)) / toleranceAngle)));
                    double bestLocationScore = locationScoreList.Max();
                    var indexOfBestLocation = locationScoreList.IndexOf(bestLocationScore);
                    if (bestLocationScore > 0.5)
                        return possibleLocationListCopy[indexOfBestLocation];
                    else
                        return null;
                }
                else
                {
                    //Dans le cas où on a demandé un reset d'accrochage de positionnement
                    double toleranceDistance = 3;
                    double toleranceAngle = Math.PI;
                    //On calcul l'écart minimal entre la location courante et les différentes location possibles
                    List<double> locationScoreList = new List<double>();
                    possibleLocationListCopy.ForEach(possibleLocation => locationScoreList.Add(
                        Math.Max(0, 1 - Toolbox.Distance(new PointD(possibleLocation.X, possibleLocation.Y), new PointD(currentLocation.X, currentLocation.Y)) / toleranceDistance)
                    * Math.Max(0, 1 - Math.Abs(currentLocation.Theta - Toolbox.ModuloByAngle(currentLocation.Theta, possibleLocation.Theta)) / toleranceAngle)));
                    double bestLocationScore = locationScoreList.Max();
                    var indexOfBestLocation = locationScoreList.IndexOf(bestLocationScore);
                    //resetLocationRequested = false;
                    if (bestLocationScore > 0.5)
                        return possibleLocationListCopy[indexOfBestLocation];
                    else
                        return null;
                }
            }
            else
                return null;
        }

        // Event position évaluée
        public event EventHandler<PositionArgs> OnAbsolutePositionCalculatedEvent;
        public virtual void OnPositionCalculatedEvent(double x, double y, double angle, double reliability)
        {
            OnAbsolutePositionCalculatedEvent?.Invoke(this, new PositionArgs { X = x, Y = y, Theta = angle, Reliability = reliability });
        }

    }

    public class TripletBalise
    {
        public int B1Id;
        public int B2Id;
        public int B3Id;
        public double Distance12;
        public double Distance13;
        public double Angle123;

        public TripletBalise(double d12, double d13, double a123)
        {
            Distance12 = d12;
            Distance13 = d13;
            Angle123 = a123;
        }
    }
    public class DoubletBalise
    {
        public int B1Id;
        public int B2Id;
        public double Distance12;

        public DoubletBalise(double d12)
        {
            Distance12 = d12;
        }
    }
}
