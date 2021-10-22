using Constants;
using EventArgsLibrary;
using HerkulexManagerNS;
using StrategyManagerEurobotNS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static HerkulexManagerNS.HerkulexEventArgs;

namespace StrategyManagerNS
{
    public class TaskParametersModifiersRoboCup : TaskBase
    {
        DateTime timestamp;
        TaskParametersModifiersState state = TaskParametersModifiersState.Idle;

        int temporisation = 1000;

        double fixedObstacleNormalRadius = 0.4;
        double mobileObstacleNormalRadius = 1.0;

        double fixedObstacleTemporaryRadius;
        double mobileObstacleTemporaryRadius;

        double accelLineaireMaxTemporaire;
        double vitesseLineaireMaxTemporaire;

        public TaskParametersModifiersRoboCup() : base()
        {
        }

        public TaskParametersModifiersRoboCup(StrategyGenerique sg) : base(sg)
        {
            parent = sg;
        }

        enum TaskParametersModifiersState
        {
            Init,
            Idle,
            SpeedBoost,
            AvoidanceReduction,
        }

        public override void Init()
        {
            /// Cette tâche ne doit pas être resettée
            isFinished = false;
        }

        public void StartAvoidanceReduction(double radiusObstacleFixe, double radiusObstacleMobile, int duree)
        {
            state = TaskParametersModifiersState.AvoidanceReduction;
            fixedObstacleTemporaryRadius = radiusObstacleFixe;
            mobileObstacleTemporaryRadius = radiusObstacleMobile;
            ResetSubState();
            isFinished = false;
        }
        public void StartSpeedBoost(double vitesseLineaireMax, double accelLineaireMax, int duree)
        {
            state = TaskParametersModifiersState.SpeedBoost;
            accelLineaireMaxTemporaire = accelLineaireMax;
            vitesseLineaireMaxTemporaire = vitesseLineaireMax;
            ResetSubState();
            isFinished = false;
        }

        public override void TaskStateMachine()
        {
            switch (state)
            {
                case TaskParametersModifiersState.Init:
                    switch (subState)
                    {
                        case SubTaskState.Entry:
                            break;
                        case SubTaskState.EnCours:
                            ExitState();
                            break;
                        case SubTaskState.Exit:
                            state = TaskParametersModifiersState.Idle;
                            break;
                    }
                    break;

                case TaskParametersModifiersState.Idle:
                    {
                        /// On exécute en permanence cette tâche pour passer les paramètres par défaut au robot
                        switch(subState)
                        {
                            case SubTaskState.Entry:
                                /// Ajustement des distances d'approche minimales en temps normal
                                parent.FixedObstacleAvoidanceDistance = fixedObstacleNormalRadius;
                                parent.MovingObstacleAvoidanceDistance = mobileObstacleNormalRadius;

                                ///On envoie périodiquement les réglages du PID de vitesse embarqué
                                double KpIndependant = 2.5;
                                double KiIndependant = 300;
                                //On envoie périodiquement les réglages du PID de vitesse embarqué
                                parent.On4WheelsIndependantSpeedPIDSetup(pM1: KpIndependant, iM1: KiIndependant, 0.0, pM2: KpIndependant, iM2: KiIndependant, 0, pM3: KpIndependant, iM3: KiIndependant, 0, pM4: KpIndependant, iM4: KiIndependant, 0.0,
                                    pM1Limit: 4, iM1Limit: 4, 0, pM2Limit: 4.0, iM2Limit: 4.0, 0, pM3Limit: 4.0, iM3Limit: 4.0, 0, pM4Limit: 4.0, iM4Limit: 4.0, 0);

                                parent.OnSetAsservissementMode((byte)AsservissementMode.Independant4Wheels);

                                /// Réglage des constantes de génération de trajectoire
                                parent.OnTrajectoryConstants(robotId: parent.robotId, accelLineaireMax: 1.5, accelRotationCapVitesseMax: 1.5 * 2 * Math.PI, accelRotationOrientationRobotMax: 1.5 * 2 * Math.PI,
                                                                vitesseLineaireMax: 1.8, vitesseRotationCapVitesseMax: 2.0 * 2 * Math.PI, vitesseRotationOrientationRobotMax: 1.6 * 2 * Math.PI);

                                /// Réglage asservissement PID position polaire
                                parent.OnPolarPositionPID(  P_x: 60.0, I_x: 0.0, D_x: 1.00, P_x_Limit: 40, I_x_Limit: 10, D_x_Limit: 10,
                                                            P_y: 60.0, I_y: 0.0, D_y: 1.00, P_y_Limit: 40, I_y_Limit: 10, D_y_Limit: 10,
                                                            P_theta: 40.0, I_theta: 0.0, D_theta: 1.0, P_theta_Limit: 40 * 2 * Math.PI, I_theta_Limit: 10 * 2 * Math.PI, D_theta_Limit: 10 * 2 * Math.PI);

                                timestamp = DateTime.Now;
                                break;
                            case SubTaskState.EnCours:
                                if(DateTime.Now.Subtract(timestamp).TotalMilliseconds>5000)
                                {
                                    ExitState();
                                }
                                break;
                            case SubTaskState.Exit:
                                state = TaskParametersModifiersState.Idle;
                                break;
                        }
                    }
                    break;

                case TaskParametersModifiersState.AvoidanceReduction:
                    {/// On exécute en permanence cette tâche pour passer les paramètres par défaut au robot
                        switch (subState)
                        {
                            case SubTaskState.Entry:
                                /// Ajustement des distances d'approche minimales en temporaire
                                parent.FixedObstacleAvoidanceDistance = fixedObstacleTemporaryRadius;
                                parent.MovingObstacleAvoidanceDistance = mobileObstacleTemporaryRadius;
                                timestamp = DateTime.Now;
                                break;
                            case SubTaskState.EnCours:
                                if (DateTime.Now.Subtract(timestamp).TotalMilliseconds > temporisation)
                                {
                                    ExitState();
                                }
                                break;
                            case SubTaskState.Exit:
                                state = TaskParametersModifiersState.Idle;
                                break;
                        }
                    }
                    break;

                case TaskParametersModifiersState.SpeedBoost:
                    {/// On exécute en permanence cette tâche pour passer les paramètres par défaut au robot
                        switch (subState)
                        {
                            case SubTaskState.Entry:
                                /// Ajustement des distances d'approche minimales en temporaire
                                /// Réglage des constantes de génération de trajectoire
                                
                                parent.OnTrajectoryConstants(robotId: parent.robotId, accelLineaireMax: accelLineaireMaxTemporaire, accelRotationCapVitesseMax: 2.0 * Math.PI, accelRotationOrientationRobotMax: 2.0 * Math.PI,
                                    vitesseLineaireMax: vitesseLineaireMaxTemporaire, vitesseRotationCapVitesseMax: 2.0 * 2 * Math.PI, vitesseRotationOrientationRobotMax: 1.6 * 2 * Math.PI);
                                
                                timestamp = DateTime.Now;
                                break;
                            case SubTaskState.EnCours:
                                if (DateTime.Now.Subtract(timestamp).TotalMilliseconds > temporisation)
                                {
                                    ExitState();
                                }
                                break;
                            case SubTaskState.Exit:
                                state = TaskParametersModifiersState.Idle;
                                break;
                        }
                    }
                    break;

                default:
                    break;
            }
        }
    }
}
