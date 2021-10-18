using EventArgsLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace SLAM_NS
{
    public class SLAM
    {

        int robotId;
        private double tEch;
        private double fEch;

        Location SlamLocation = new Location();
        double gyroVTheta;

        public SLAM(int id, double freqEchOdometry)
        {
            robotId = id;
            fEch = freqEchOdometry;
            tEch = 1 / freqEchOdometry;

            Init(0, 0, 0);
        }

        public void Init(double xRefTerrain, double yRefTerrain, double theta)
        {
            SlamLocation = new Location(xRefTerrain, yRefTerrain, theta, 0, 0, 0);
            //OnSLAMLocation(robotId, SlamLocation);
        }

        //public void IterateFilter(double GPS_X_Ref_Terrain, double GPS_Y_Ref_Terrain, double GPS_Theta, double Odo_VX, double Odo_VY, double Odo_VTheta, double Gyro_Theta)
        //{
        //    SlamLocation
        //}

        //public void IterateComplementaryFilter(double GPS_X_Ref_Terrain, double GPS_Y_Ref_Terrain, double GPS_Theta, double Odo_VX, double Odo_VY, double Odo_VTheta, double Gyro_Theta, double alpha = 0.02)
        //{
        //    //kalmanLocationRefTerrain.X = output[0];
        //    //kalmanLocationRefTerrain.Vx = output[1];
        //    //double AxKalman = output[2];

        //    //kalmanLocationRefTerrain.Y = output[3];
        //    //kalmanLocationRefTerrain.Vy = output[4];
        //    //double AyKalman = output[5];

        //    //kalmanLocationRefTerrain.Theta = output[6];
        //    //kalmanLocationRefTerrain.Vtheta = output[7];
        //    //double alpha = 0.02; 

        //    xEst[0] = (1 - alpha) * (xEst[0] + Odo_VX * 1 / fEch) + alpha * GPS_X_Ref_Terrain;  /// X
        //    xEst[3] = (1 - alpha) * (xEst[3] + Odo_VY * 1 / fEch) + alpha * GPS_Y_Ref_Terrain;  /// Y
        //    xEst[6] = (1 - alpha) * (xEst[6] + Odo_VTheta * 1 / fEch) + alpha * GPS_Theta;      /// Theta

        //    xEst[1] = Odo_VX;       /// Vx
        //    xEst[4] = Odo_VY;       /// Vy
        //    xEst[7] = Odo_VTheta;   /// Vtheta
        //}


        //Input events
        public void OnCollisionReceived(object sender, CollisionEventArgs e)
        {
            Init(e.RobotRealPositionRefTerrain.X, e.RobotRealPositionRefTerrain.Y, e.RobotRealPositionRefTerrain.Theta);
        }

        public void OnForcedPositionReceived(object sender, LocationArgs e)
        {
            Init(e.Location.X, e.Location.Y, e.Location.Theta);
        }

        public void OnOdometryRobotSpeedReceived(object sender, PolarSpeedArgs e)
        {
            /// Attention : l'arrivée de données odométrie est le seul point de recalcul de l'odométrie par incrémentation
            if (robotId == e.RobotId)
            {
                var OdometryVxRefRobot = e.Vx;
                var OdometryVyRefRobot = e.Vy;

                SlamLocation.Vx = OdometryVxRefRobot * Math.Cos(SlamLocation.Theta) - OdometryVyRefRobot * Math.Sin(SlamLocation.Theta);
                SlamLocation.Vy = OdometryVxRefRobot * Math.Sin(SlamLocation.Theta) + OdometryVyRefRobot * Math.Cos(SlamLocation.Theta);
                SlamLocation.Vtheta = e.Vtheta;

                /// On extrapole les valeurs de position dans le référentiel terrain en utilisant les vitesses mesurées
                /// Elles peuvent être partiellement écrasées par l'arrivée d'une donnée GPS
                SlamLocation.X += SlamLocation.Vx / fEch;
                SlamLocation.Y += SlamLocation.Vy / fEch;
                //SlamLocation.Theta += SlamLocation.Vtheta / fEch; //Utilisation de VTheta odométrie
                SlamLocation.Theta += gyroVTheta / fEch;            //Utilisation de VTheta gyro
                SlamLocation.Theta = Toolbox.Modulo2PiAngleRad(SlamLocation.Theta);
                
                OnSLAMLocation(robotId, SlamLocation);
            }
        }

        public void OnAbsolutePositionRefTerrainReceived(object sender, PositionArgs e)
        {
            /// L'arrivée de données GPS update en partie la SlamLocation
            double alphaGPS = 0.04;
            SlamLocation.X = (1 - alphaGPS) * SlamLocation.X + alphaGPS * e.X;
            SlamLocation.Y = (1 - alphaGPS) * SlamLocation.Y + alphaGPS * e.Y;
            SlamLocation.Theta = (1 - alphaGPS) * SlamLocation.Theta + alphaGPS * Toolbox.ModuloByAngle(SlamLocation.Theta, e.Theta);
        }

        public void OnGyroRobotSpeedReceived(object sender, GyroArgs e)
        {
            gyroVTheta = e.Vtheta;
        }

        //Output events
        public event EventHandler<LocationArgs> OnSLAMLocationEvent;
        public virtual void OnSLAMLocation(int id, Location locationRefTerrain)
        {
            var handler = OnSLAMLocationEvent;
            if (handler != null)
            {
                handler(this, new LocationArgs { RobotId = id, Location = locationRefTerrain });
            }
        }
    }
}
