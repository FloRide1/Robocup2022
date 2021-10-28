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
    public class Eurobot2021TaskDeplacementsParametersModifiers : TaskBase
    {
        DateTime timestamp;
        TaskDeplacementsParametersModifiersState state = TaskDeplacementsParametersModifiersState.Idle;

        int temporisation = 1000;

        double accelLineaireMaxTemporaire;
        double vitesseLineaireMaxTemporaire;

        public Eurobot2021TaskDeplacementsParametersModifiers() : base()
        {
        }

        public Eurobot2021TaskDeplacementsParametersModifiers(StrategyGenerique sg) : base(sg)
        {
            parent = sg;
        }

        enum TaskDeplacementsParametersModifiersState
        {
            Init,
            Idle,
            SpeedBoost,
        }

        public override void Init()
        {
            /// Cette tâche ne doit pas être resettée
            isFinished = false;
        }
        public void StartSpeedBoost(double vitesseLineaireMax, double accelLineaireMax, int duree)
        {
            state = TaskDeplacementsParametersModifiersState.SpeedBoost;
            accelLineaireMaxTemporaire = accelLineaireMax;
            vitesseLineaireMaxTemporaire = vitesseLineaireMax;
            temporisation = duree;

            ResetSubState();
            isFinished = false;
        }

        public override void TaskStateMachine()
        {
            switch (state)
            {
                case TaskDeplacementsParametersModifiersState.Init:
                    switch (subState)
                    {
                        case SubTaskState.Entry:
                            break;
                        case SubTaskState.EnCours:
                            ExitState();
                            break;
                        case SubTaskState.Exit:
                            state = TaskDeplacementsParametersModifiersState.Idle;
                            break;
                    }
                    break;

                case TaskDeplacementsParametersModifiersState.Idle:
                    {
                        /// On exécute en permanence cette tâche pour passer les paramètres par défaut au robot
                        switch (subState)
                        {
                            case SubTaskState.Entry:
                                ///On envoie périodiquement les réglages du PID de vitesse embarqué
                                double p = 1; //1 validé
                                double ki = 50;

                                /// Réglage asservissement PID vitesse embarqué
                                parent.On4WheelsIndependantSpeedPIDSetup(pM1: p, iM1: ki, 0.0, pM2: p, iM2: ki, 0, pM3: p, iM3: ki, 0, pM4: p, iM4: ki, 0.0,
                                            pM1Limit: 4.0, iM1Limit: 4.0, 0, pM2Limit: 4.0, iM2Limit: 4.0, 0, pM3Limit: 4.0, iM3Limit: 4.0, 0, pM4Limit: 4.0, iM4Limit: 4.0, 0);

                                /// Réglage des constantes de génération de trajectoire
                                parent.OnTrajectoryConstants(robotId: parent.robotId, accelLineaireMax: 1.2, accelRotationCapVitesseMax: 5.0 * 2 * Math.PI, accelRotationOrientationRobotMax: 0.6 * 2 * Math.PI,
                                                                vitesseLineaireMax: 1.5, vitesseRotationCapVitesseMax: 5.0 * 2 * Math.PI, vitesseRotationOrientationRobotMax: 0.9 * 2 * Math.PI);

                                /// Réglage asservissement PID position polaire
                                parent.OnPolarPositionPID(P_x: 140.0, I_x: 0.0, D_x: 0.70, P_x_Limit: 40, I_x_Limit: 10, D_x_Limit: 10,
                                                            P_y: 140.0, I_y: 0.0, D_y: 0.70, P_y_Limit: 40, I_y_Limit: 10, D_y_Limit: 10,
                                                            P_theta: 30.0, I_theta: 0.0, D_theta: 1.0, P_theta_Limit: 40 * 2 * Math.PI, I_theta_Limit: 10 * 2 * Math.PI, D_theta_Limit: 10 * 2 * Math.PI);

                                timestamp = DateTime.Now;
                                break;
                            case SubTaskState.EnCours:
                                if (DateTime.Now.Subtract(timestamp).TotalMilliseconds > 5000)
                                {
                                    ExitState();
                                }
                                break;
                            case SubTaskState.Exit:
                                state = TaskDeplacementsParametersModifiersState.Idle;
                                break;
                        }
                    }
                    break;

                case TaskDeplacementsParametersModifiersState.SpeedBoost:
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
                                state = TaskDeplacementsParametersModifiersState.Idle;
                                break;
                        }
                    }
                    break;

                default:
                    break;
            }
        }
    }

    public class Eurobot2021TaskAvoidanceParametersModifiers : TaskBase
    {
        DateTime timestamp;
        TaskAvoidanceParametersModifiersState state = TaskAvoidanceParametersModifiersState.Idle;

        int temporisation = 1000;

        double fixedObstacleNormalRadius = 0.20;
        double mobileObstacleNormalRadius = 0.40;

        double fixedObstacleTemporaryRadius;
        double mobileObstacleTemporaryRadius;

        public Eurobot2021TaskAvoidanceParametersModifiers() : base()
        {
        }

        public Eurobot2021TaskAvoidanceParametersModifiers(StrategyGenerique sg) : base(sg)
        {
            parent = sg;
        }

        enum TaskAvoidanceParametersModifiersState
        {
            Init,
            Idle,
            AvoidanceReduction,
        }

        public override void Init()
        {
            /// Cette tâche ne doit pas être resettée
            isFinished = false;
        }

        public void StartAvoidanceReduction(double radiusObstacleFixe, double radiusObstacleMobile, int duree)
        {
            state = TaskAvoidanceParametersModifiersState.AvoidanceReduction;
            fixedObstacleTemporaryRadius = radiusObstacleFixe;
            mobileObstacleTemporaryRadius = radiusObstacleMobile;
            temporisation = duree;
            ResetSubState();
            isFinished = false;
        }

        public override void TaskStateMachine()
        {
            switch (state)
            {
                case TaskAvoidanceParametersModifiersState.Init:
                    switch (subState)
                    {
                        case SubTaskState.Entry:
                            break;
                        case SubTaskState.EnCours:
                            ExitState();
                            break;
                        case SubTaskState.Exit:
                            state = TaskAvoidanceParametersModifiersState.Idle;
                            break;
                    }
                    break;

                case TaskAvoidanceParametersModifiersState.Idle:
                    {
                        /// On exécute en permanence cette tâche pour passer les paramètres par défaut au robot
                        switch (subState)
                        {
                            case SubTaskState.Entry:
                                /// Ajustement des distances d'approche minimales en temps normal
                                parent.FixedObstacleAvoidanceDistance = fixedObstacleNormalRadius;
                                parent.MovingObstacleAvoidanceDistance = mobileObstacleNormalRadius;

                                timestamp = DateTime.Now;
                                break;
                            case SubTaskState.EnCours:
                                if (DateTime.Now.Subtract(timestamp).TotalMilliseconds > 5000)
                                {
                                    ExitState();
                                }
                                break;
                            case SubTaskState.Exit:
                                state = TaskAvoidanceParametersModifiersState.Idle;
                                break;
                        }
                    }
                    break;

                case TaskAvoidanceParametersModifiersState.AvoidanceReduction:
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
                                state = TaskAvoidanceParametersModifiersState.Idle;
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
