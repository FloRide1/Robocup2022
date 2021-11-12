using Constants;
using EventArgsLibrary;
using HerkulexManagerNS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Utilities;
using static HerkulexManagerNS.HerkulexEventArgs;

namespace StrategyManagerNS
{
    public class Eurobot2022MissionRamassageAutomatique : MissionBase
    {
        MissionRamassageAutomatiqueState missionRamassageAutomatiqueState = MissionRamassageAutomatiqueState.Idle;
        DateTime timestamp;
        double indicativeTime;

        StrategyEurobot2022 parentStrategie;
        public Eurobot2022MissionRamassageAutomatique() : base()
        { }

        public Eurobot2022MissionRamassageAutomatique(StrategyGenerique sg) : base(sg)
        {
            parentStrategie = sg as StrategyEurobot2022;
        }

        public override void Init()
        {
            missionRamassageAutomatiqueState = MissionRamassageAutomatiqueState.Idle;
            ResetSubState();
            isFinished = false;
        }

        public void Start()
        {
            missionRamassageAutomatiqueState = MissionRamassageAutomatiqueState.DeplacementToScan;
            isFinished = false;
            ResetSubState();
        }

        enum MissionRamassageAutomatiqueState
        {
            Init,
            Idle,
            DeplacementToScan,
            Observation,
        }

        public void Pause()
        {
            missionRamassageAutomatiqueState = MissionRamassageAutomatiqueState.Idle;
            isFinished = false;
        }

        int switchPosObservation = 0;

        public override void MissionStateMachine()
        {
            if (missionRamassageAutomatiqueState != MissionRamassageAutomatiqueState.Idle)
                isRunning = true;
            else
                isRunning = false;

            switch (missionRamassageAutomatiqueState)
            {
                case MissionRamassageAutomatiqueState.Init:
                    switch (subState)
                    {
                        case SubTaskState.Entry:
                            timestamp = DateTime.Now;
                            break;
                        case SubTaskState.EnCours:
                            if (DateTime.Now.Subtract(timestamp).TotalMilliseconds > 1000.0)
                            {
                                ExitState();/// A appeler quand on souhaite passer à Exit       
                            }
                            break;
                        case SubTaskState.Exit:                             /// L'état suivant ne doit être défini que dans le substate Exit
                            isRunning = false;
                            missionRamassageAutomatiqueState = MissionRamassageAutomatiqueState.Idle;
                            break;
                    }
                    break;
                case MissionRamassageAutomatiqueState.Idle: //On fait une tempo de 2 secondes
                    switch (subState)
                    {
                        case SubTaskState.Entry:
                            isFinished = true;
                            break;
                    }
                    break;
                case MissionRamassageAutomatiqueState.DeplacementToScan:
                    switch (subState)
                    {
                        case SubTaskState.Entry:
                            ///Deplacement de pôsitionnement sans sortir le bras
                            foreach (var task in parentStrategie.listTasks)
                            {
                                task.Init();
                            }

                            if (parentStrategie.playingColor == StrategyEurobot2022.Eurobot2022SideColor.Blue)
                            {
                                if (parentStrategie.robotType == StrategyEurobot2022.Eurobot2022RobotType.RobotSud)
                                {
                                    switch (switchPosObservation)
                                    {
                                        case 0:
                                            indicativeTime = parentStrategie.SetRobotDestination(new PointD(0.6, 0.3), Math.PI - Math.PI / 4);
                                            break;
                                        case 1:
                                            indicativeTime = parentStrategie.SetRobotDestination(new PointD(0.3, 0.1), Math.PI - Math.PI / 3);
                                            break;
                                        case 2:
                                            indicativeTime = parentStrategie.SetRobotDestination(new PointD(0, 0.3), Math.PI - Math.PI / 2);
                                            break;
                                    }
                                }
                                //else
                                //{
                                //    indicativeTime = parentStrategie.SetRobotDestination(new PointD(-0.65, 0.8), 0);
                                //}
                            }
                            else if (parentStrategie.playingColor == StrategyEurobot2022.Eurobot2022SideColor.Yellow)
                            {
                                if (parentStrategie.robotType == StrategyEurobot2022.Eurobot2022RobotType.RobotSud)
                                {

                                    switch (switchPosObservation)
                                    {
                                        case 0:
                                            indicativeTime = parentStrategie.SetRobotDestination(new PointD(-0.6, 0.3), Math.PI / 4);
                                            break;
                                        case 1:
                                            indicativeTime = parentStrategie.SetRobotDestination(new PointD(-0.3, 0.1), Math.PI / 3);
                                            break;
                                        case 2:
                                            indicativeTime = parentStrategie.SetRobotDestination(new PointD(0, 0.3), Math.PI / 2);
                                            break;
                                    }
                                }
                                else
                                {
                                    //Rush initial validé TODO plus tard
                                    //indicativeTime = parentStrategie.SetRobotDestination(new PointD(-0.7, 0.8), 0);
                                    //parentStrategie.taskDeplacementsParametersModifiers.StartSpeedBoost(2.0, 2.0, 2000);
                                    //parentStrategie.taskAvoidanceParametersModifiers.StartAvoidanceReduction(0.2, 0.9, 2000);

                                }
                            }
                            timestamp = DateTime.Now;
                            break;
                        case SubTaskState.EnCours:
                            if (parentStrategie.isDeplacementFinished || DateTime.Now.Subtract(timestamp).TotalMilliseconds > indicativeTime)
                                ExitState();                                /// A appeler quand on souhaite passer à Exit
                            break;
                        case SubTaskState.Exit:
                            if (DateTime.Now.Subtract(timestamp).TotalMilliseconds > indicativeTime)
                                Console.WriteLine("Ramassage Automatique : déplacement aborted");
                            else
                                Console.WriteLine("Ramassage Automatique : déplacement successful");
                            missionRamassageAutomatiqueState = MissionRamassageAutomatiqueState.Observation;                    /// L'état suivant ne doit être défini que dans le substate Exit
                            break;
                    }
                    break;
                case MissionRamassageAutomatiqueState.Observation:
                    {
                        switch (subState)
                        {
                            case SubTaskState.Entry:
                                timestamp = DateTime.Now;
                                parentStrategie.SetRobotDestination(new PointD(parentStrategie.robotCurrentLocation.X, parentStrategie.robotCurrentLocation.Y), parentStrategie.robotCurrentLocation.Theta);
                                parentStrategie.OnEnableDisableMotor(false);
                                break;
                            case SubTaskState.EnCours:
                                if (DateTime.Now.Subtract(timestamp).TotalMilliseconds > 1000)
                                    ExitState();
                                break;
                            case SubTaskState.Exit:
                                parentStrategie.OnEnableDisableMotor(true);
                                ///On attaque le premier gobelet trouvé au lidar en vérifiant qu'il n'est ni à l'extérieur du terrain ni dans une zone interdite
                                ///
                                //ON commence par reconstruire la liste dans le référentiel du terrain
                                var gobeletListeRefTerrain = new List<Eurobot2022GobeletPotentiel>();
                                if (parentStrategie.GobeletsPotentielsRefTerrain.Count > 0)
                                {
                                    //Pour l'instant, on ne prend pas les gobelets couchés
                                    var gobeletOrderedList = parentStrategie.GobeletsPotentielsRefTerrain.Where(elt => elt.Largeur < 0.09).OrderBy(elt => Toolbox.Distance(elt.Pos, new PointD(parentStrategie.robotCurrentLocation.X, parentStrategie.robotCurrentLocation.Y)));
                                    for (int i = 0; i < Math.Min(3, gobeletOrderedList.Count()); i++)
                                    {
                                        var chosenGobelet = gobeletOrderedList.ElementAt(i);
                                        /// On filtre les gobelets debout et les gobelets couchés
                                        Eurobot2022TypeGobelet typeGobelet = Eurobot2022TypeGobelet.Libre;
                                        if (chosenGobelet.Largeur > 0.09)
                                            typeGobelet = Eurobot2022TypeGobelet.LibreCouche;

                                        Eurobot2022Color colorGobelet = Eurobot2022Color.Rouge;
                                        if (chosenGobelet.RssiStdDev > 950)
                                            colorGobelet = Eurobot2022Color.Vert;

                                        parentStrategie.matchDescriptor.listElementsJeu.Add(parentStrategie.matchDescriptor.listElementsJeu.Count + 1000,
                                            new Eurobot2022Gobelet(chosenGobelet.Pos.X, chosenGobelet.Pos.Y, color: colorGobelet, typeGobelet, reserved: Eurobot2022TeamReservation.Shared, anglePrise: null,
                                            robotAttributionBlue: parentStrategie.robotType, robotAttributionYellow: parentStrategie.robotType));
                                        Console.WriteLine(String.Format("RSSI DEV : {0} / TYPE {1} / COLOR {2}", chosenGobelet.RssiStdDev, typeGobelet, colorGobelet));
                                    }
                                }
                                switchPosObservation++;
                                if (switchPosObservation >= 3)
                                    switchPosObservation = 0;
                                missionRamassageAutomatiqueState = MissionRamassageAutomatiqueState.Idle;
                                break;
                        }
                        break;
                    }
                    break;
                default:
                    break;
            }
        }

        public event EventHandler<SpeedConsigneToMotorArgs> OnPilotageVentouseEvent;
        public virtual void OnPilotageVentouse(byte motorNumber, double vitesse)
        {
            OnPilotageVentouseEvent?.Invoke(this, new SpeedConsigneToMotorArgs { MotorNumber = motorNumber, V = vitesse });
        }
        public void OnMotorCurrentReceive(object sender, MotorsCurrentsEventArgs e)
        {
        }

        public event EventHandler<HerkulexPositionsArgs> OnHerkulexPositionRequestEvent;
        public virtual void OnHerkulexPositionRequest(Dictionary<ServoId, int> positionDictionary)
        {
            OnHerkulexPositionRequestEvent?.Invoke(this, new HerkulexPositionsArgs { servoPositions = positionDictionary });
        }
    }
}
