using Constants;
using EventArgsLibrary;
using HerkulexManagerNS;
using RefereeBoxAdapter;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Utilities;
using WorldMap;
using static HerkulexManagerNS.HerkulexEventArgs;

namespace StrategyManagerNS
{

    public class Eurobot2021TaskGameManagement : TaskBase
    {
        private enum TaskGameManagementState
        {
            Init,
            JackPresent,
            Match,
            DeplacementToZoneMouillage,
            MatchEnded,
        }

        public Eurobot2021TaskGameManagement() : base()
        { }

        public Eurobot2021TaskGameManagement(StrategyGenerique sg) : base(sg)
        {
            parent = sg;
        }

        public override void Init()
        {
            /// On ne fait rien, cette task ne doit pas être resettée
        }

        TaskGameManagementState state = TaskGameManagementState.Init;

        Random random = new Random(654654684);
        


        DateTime timestamp;
        public override void TaskStateMachine()
        {
            if (parent != null)
            {
                StrategyEurobot2021 p = parent as StrategyEurobot2021;

                //Forçage d'état si besoin
                if (p.jackIsPresent && state != TaskGameManagementState.JackPresent)
                {
                    state = TaskGameManagementState.JackPresent;
                    ResetSubState(); //Imperatif dans les Forçages;
                }

                switch (state)
                {
                    case TaskGameManagementState.Init: //On fait une tempo de 2 secondes
                        switch (subState)
                        {
                            case SubTaskState.Entry:
                                timestamp = DateTime.Now;
                                Console.WriteLine("Initialisation du Game Management");
                                break;
                            case SubTaskState.EnCours:
                                if (p.jackIsPresent)
                                    ExitState();                                /// A appeler quand on souhaite passer à Exit                                    
                                break;
                            case SubTaskState.Exit:                             /// L'état suivant ne doit être défini que dans le substate Exit
                                Console.WriteLine("Passage en attente de retrait de jack");
                                state = TaskGameManagementState.JackPresent;
                                break;
                        }
                        break;
                    case TaskGameManagementState.JackPresent:
                        switch (subState)
                        {
                            case SubTaskState.Entry:
                                timestamp = DateTime.Now;
                                Console.WriteLine("Début Attente de retrait de jack");
                                break;
                            case SubTaskState.EnCours:
                                //Si on a retiré le Jack
                                if (!p.jackIsPresent)
                                    ExitState();                                /// A appeler quand on souhaite passer à Exit
                                else
                                {
                                    string textToDisplayLine1 = "Attente Jack";
                                    string textToDisplayLine2 = "";
                                    if (p.playingColor == StrategyEurobot2021.Eurobot2021SideColor.Blue)
                                    {
                                        textToDisplayLine2 += "BLEU ";
                                    }
                                    else
                                    {
                                        textToDisplayLine2 += "JAUNE ";
                                    }
                                    if (p.robotType == StrategyEurobot2021.Eurobot2021RobotType.RobotNord)
                                    {
                                        textToDisplayLine2 += "NORD ";
                                        textToDisplayLine2 += textToDisplayLine2;
                                    }
                                    else
                                    {
                                        textToDisplayLine2 += "SUD ";
                                        textToDisplayLine2 += textToDisplayLine2;
                                    }
                                    p.taskAffichageLidar.StartAffichagePermanentLigne1(textToDisplayLine1);
                                    p.taskAffichageLidar.StartAffichagePermanentLigne2(textToDisplayLine2);

                                    /// Si pas de jack, on réinit les tasks
                                    foreach (var task in p.listTasks)
                                    {
                                        task.Init();
                                    }
                                    p.matchDescriptor.Init();
                                    /// On défini la position initiale du robot, et on coupe le moteur et l'asservissement, 
                                    /// et on définit le waypoint courant à la position réelle du robot
                                    /// 
                                    Location locationDepart = new Location(0, 0, 0, 0, 0, 0);

                                    if (p.playingColor == StrategyEurobot2021.Eurobot2021SideColor.Blue)
                                    {
                                        if (p.robotType == StrategyEurobot2021.Eurobot2021RobotType.RobotNord)
                                        {
                                            locationDepart = new Location(-1.35, 0.345, Toolbox.Modulo2PiAngleRad(Toolbox.DegToRad(180 - 0)), 0, 0, 0);
                                        }

                                        if (p.robotType == StrategyEurobot2021.Eurobot2021RobotType.RobotSud)
                                        {
                                            locationDepart = new Location(-1.25, 0.06, Toolbox.Modulo2PiAngleRad(Toolbox.DegToRad(180 - 0)), 0, 0, 0);
                                        }
                                    }

                                    if (p.playingColor == StrategyEurobot2021.Eurobot2021SideColor.Yellow)
                                    {
                                        if (p.robotType == StrategyEurobot2021.Eurobot2021RobotType.RobotNord)
                                        {
                                            locationDepart = new Location(1.35, 0.345, Toolbox.Modulo2PiAngleRad(Toolbox.DegToRad(0)), 0, 0, 0);
                                        }

                                        if (p.robotType == StrategyEurobot2021.Eurobot2021RobotType.RobotSud)
                                        {
                                            locationDepart = new Location(1.25, 0.06, Toolbox.Modulo2PiAngleRad(Toolbox.DegToRad(0)), 0, 0, 0);
                                        }
                                    }
                                    p.OnForcedLocation(0, locationDepart);
                                    p.OnEnableDisableMotor(false);
                                }
                                break;
                            case SubTaskState.Exit:
                                Console.WriteLine("Jack retiré, on passe en match");
                                state = TaskGameManagementState.Match;                     /// L'état suivant ne doit être défini que dans le substate Exit
                                break;
                        }
                        break;
                    case TaskGameManagementState.Match:
                        switch (subState)
                        {
                            case SubTaskState.Entry:
                                timestamp = DateTime.Now;
                                Console.WriteLine("Début Match");

                                //On active les moteurs
                                p.OnEnableDisableMotor(true);
                                p.taskAvoidanceParametersModifiers.StartAvoidanceReduction(0.20, 0.3, 2000);

                                //p.taskParametersModifiers.StartSpeedBoost(4.0, 4.0, 2000);

                                break;
                            case SubTaskState.EnCours:
                                if ((p.robotType == StrategyEurobot2021.Eurobot2021RobotType.RobotSud && DateTime.Now.Subtract(timestamp).TotalMilliseconds > 90000) ||
                                    (p.robotType == StrategyEurobot2021.Eurobot2021RobotType.RobotNord && DateTime.Now.Subtract(timestamp).TotalMilliseconds > 94000))
                                {
                                    //On demarre la tache FinDeMatch
                                    //p.taskFinDeMatch.Start();
                                    //On sort de l'etat
                                    ExitState();                                /// A appeler quand on souhaite passer à Exit
                                }
                                else
                                {
                                    /// On est en cours de match
                                    /// On recherche la task de mission active
                                    bool newMissionRequested = false;

                                    var l = p.listMissions.Where(task => task.isRunning == true).ToList();
                                    if (l.Count == 1)
                                    {
                                        /// Tout est normal, on ne doit pas avoir deux missions qui runnent en même temps.
                                        var mission = l[0];
                                        if (mission.isFinished)
                                        {
                                            /// On relance une nouvelle mission
                                            newMissionRequested = true;
                                        }
                                    }
                                    else if (l.Count == 0)
                                    {
                                        ///
                                        newMissionRequested = true;
                                    }
                                    else
                                    {
                                        Console.WriteLine("TaskGameManagement : cas anormal, plusieurs missions actives");
                                    }

                                    if (newMissionRequested)
                                    {
                                        ///// Mission au hasard
                                        //int i = random.Next(p.listMissions.Count);
                                        //p.listMissions[i].Start();
                                        //p.missionPhare.Start();
                                        /// On regarde les missions atteignables
                                        /// On commence par récupérer les gobelets restant à aller chercher
                                        var dictionaryElementsRestants = new Dictionary<int, Eurobot2021ElementDeJeu>();
                                        lock (p.matchDescriptor.listElementsJeu)
                                        {
                                            if (p.playingColor == StrategyEurobot2021.Eurobot2021SideColor.Blue)
                                            {
                                                dictionaryElementsRestants = p.matchDescriptor.listElementsJeu.Where(x => x.Value.isAvailable == true &&
                                                x.Value.ReservationToTeam != Eurobot2021TeamReservation.ReservedYellow && x.Value.RobotAttributionBlue == p.robotType)
                                                    .OrderByDescending(x => x.Value.PriorityBlue).ToDictionary(x => x.Key, y => y.Value);
                                            }
                                            else
                                            {
                                                dictionaryElementsRestants = p.matchDescriptor.listElementsJeu.Where(x => x.Value.isAvailable == true &&
                                                x.Value.ReservationToTeam != Eurobot2021TeamReservation.ReservedBlue && x.Value.RobotAttributionYellow == p.robotType)
                                                    .OrderByDescending(x => x.Value.PriorityYellow).ToDictionary(x => x.Key, y => y.Value);
                                            }
                                        }
                                        ///La dépose est prioritaire sur la pose dans le code
                                        if (p.matchDescriptor.CompteurDeposeAEffectuer > 0)
                                        {
                                            /// On n'est pas dans sequence prise, donc on dépose
                                            var dictionaryBrasPleins = p.matchDescriptor.DictionaryBrasTurbine.Where(x => x.Value.HasGobelet == true).ToDictionary(x => x.Key, y => y.Value);
                                            Dictionary<int, Eurobot2021EmplacementDepose> dictionaryEmplacementsUtilisables = new Dictionary<int, Eurobot2021EmplacementDepose>();

                                            /// Au cas où il y ait eu une boulette dans la gestion, on ramène le nombre de Depose à Effectuer au nombre max de bras occupés
                                            var nbDeposes = Math.Min(p.matchDescriptor.CompteurDeposeAEffectuer, dictionaryBrasPleins.Count);
                                            if (p.matchDescriptor.CompteurDeposeAEffectuer != nbDeposes)
                                                p.matchDescriptor.SetCompteurDeposeAEffectuer(nbDeposes);

                                            if (dictionaryBrasPleins.Count > 0)
                                            {
                                                ///Si il y a des bras pleins, on traite le premer bras dispo
                                                var brasPlein = dictionaryBrasPleins.ElementAt(0);

                                                ///Donc on a sa couleur, donc on peut filtrer la liste des emplacements de dépose
                                                var dictionaryEmplacementsRestants = new Dictionary<int, Eurobot2021EmplacementDepose>();
                                                if (p.playingColor == StrategyEurobot2021.Eurobot2021SideColor.Blue)
                                                {
                                                    /// On récupère la liste des emplacements : libres / de la couleur de l'équipe / 
                                                    /// correspondant au rôle du robot dans l'équipe, de la couleur du gobelet
                                                    dictionaryEmplacementsRestants = p.matchDescriptor.DictionaryEmplacementDepose.Where(x => x.Value.IsAvailable == true && x.Value.SideColor == p.playingColor
                                                    && x.Value.RobotAttributionBlue == p.robotType && x.Value.Color == brasPlein.Value.GobletCapturedColor).ToDictionary(x => x.Key, y => y.Value);

                                                    /// Ensuite, on extrait la sous-liste des emplacements dont les antécédents sont validés
                                                    dictionaryEmplacementsUtilisables = GetListEmplacementsAntecedentsValides(dictionaryEmplacementsRestants);
                                                    Console.WriteLine("Nombre d'emplacements utilisables " + brasPlein.Value.GobletCapturedColor.ToString() + " coté bleu : " + dictionaryEmplacementsUtilisables.Count);

                                                    /// Si on a des emplacements utilisables, c'est ok, sinon on cherche aussi dans les emplacements neutres
                                                    if (dictionaryEmplacementsUtilisables.Count == 0)
                                                    {
                                                        /// On récupère la liste des emplacements : libres / de la couleur de l'équipe / 
                                                        /// correspondant au rôle du robot dans l'équipe, de la couleur neutre
                                                        dictionaryEmplacementsRestants = p.matchDescriptor.DictionaryEmplacementDepose.Where(x => x.Value.IsAvailable == true && x.Value.SideColor == p.playingColor
                                                        && x.Value.RobotAttributionBlue == p.robotType && x.Value.Color == Eurobot2021Color.Neutre).ToDictionary(x => x.Key, y => y.Value);

                                                        /// Ensuite, on extrait la sous-liste des emplacements dont les antécédents sont validés
                                                        dictionaryEmplacementsUtilisables = GetListEmplacementsAntecedentsValides(dictionaryEmplacementsRestants);
                                                        Console.WriteLine("Nombre d'emplacements utilisables neutres côté bleu : " + dictionaryEmplacementsUtilisables.Count);
                                                    }

                                                    /// Si on a des emplacements utilisables, c'est ok, sinon on cherche aussi dans tous les emplacements
                                                    if (dictionaryEmplacementsUtilisables.Count == 0)
                                                    {
                                                        /// On récupère la liste des emplacements : libres / de la couleur de l'équipe / 
                                                        /// correspondant au rôle du robot dans l'équipe, de n'importe quelle couleur
                                                        dictionaryEmplacementsRestants = p.matchDescriptor.DictionaryEmplacementDepose.Where(x => x.Value.IsAvailable == true && x.Value.SideColor == p.playingColor
                                                        && x.Value.RobotAttributionBlue == p.robotType).ToDictionary(x => x.Key, y => y.Value);

                                                        /// Ensuite, on extrait la sous-liste des emplacements dont les antécédents sont validés
                                                        dictionaryEmplacementsUtilisables = GetListEmplacementsAntecedentsValides(dictionaryEmplacementsRestants);
                                                        Console.WriteLine("Nombre d'emplacements utilisables couleur quelconque côté bleu : " + dictionaryEmplacementsUtilisables.Count);
                                                    }
                                                }
                                                else
                                                {
                                                    /// Coté Yellow
                                                    /// On récupère la liste des emplacements : libres / de la couleur de l'équipe / 
                                                    /// correspondant au rôle du robot dans l'équipe, de la couleur du gobelet
                                                    dictionaryEmplacementsRestants = p.matchDescriptor.DictionaryEmplacementDepose.Where(x => x.Value.IsAvailable == true && x.Value.SideColor == p.playingColor
                                                    && x.Value.RobotAttributionYellow == p.robotType && x.Value.Color == brasPlein.Value.GobletCapturedColor).ToDictionary(x => x.Key, y => y.Value);

                                                    /// Ensuite, on extrait la sous-liste des emplacements dont les antécédents sont validés
                                                    dictionaryEmplacementsUtilisables = GetListEmplacementsAntecedentsValides(dictionaryEmplacementsRestants);
                                                    Console.WriteLine("Nombre d'emplacements utilisables " + brasPlein.Value.GobletCapturedColor.ToString() + " coté jaune : " + dictionaryEmplacementsUtilisables.Count);

                                                    /// Si on a des emplacements utilisables, c'est ok, sinon on cherche aussi dans les emplacements neutres
                                                    if (dictionaryEmplacementsUtilisables.Count == 0)
                                                    {
                                                        /// On récupère la liste des emplacements : libres / de la couleur de l'équipe / 
                                                        /// correspondant au rôle du robot dans l'équipe, de la couleur neutre
                                                        dictionaryEmplacementsRestants = p.matchDescriptor.DictionaryEmplacementDepose.Where(x => x.Value.IsAvailable == true && x.Value.SideColor == p.playingColor
                                                        && x.Value.RobotAttributionYellow == p.robotType && x.Value.Color == Eurobot2021Color.Neutre).ToDictionary(x => x.Key, y => y.Value);

                                                        /// Ensuite, on extrait la sous-liste des emplacements dont les antécédents sont validés
                                                        dictionaryEmplacementsUtilisables = GetListEmplacementsAntecedentsValides(dictionaryEmplacementsRestants);
                                                        Console.WriteLine("Nombre d'emplacements utilisables neutres côté jaune : " + dictionaryEmplacementsUtilisables.Count);
                                                    }

                                                    /// Si on a des emplacements utilisables, c'est ok, sinon on cherche dans tous les emplacements
                                                    if (dictionaryEmplacementsUtilisables.Count == 0)
                                                    {
                                                        /// On récupère la liste des emplacements : libres / de la couleur de l'équipe / 
                                                        /// correspondant au rôle du robot dans l'équipe, de n'importe quelle couleur
                                                        dictionaryEmplacementsRestants = p.matchDescriptor.DictionaryEmplacementDepose.Where(x => x.Value.IsAvailable == true && x.Value.SideColor == p.playingColor
                                                        && x.Value.RobotAttributionYellow == p.robotType).ToDictionary(x => x.Key, y => y.Value);

                                                        /// Ensuite, on extrait la sous-liste des emplacements dont les antécédents sont validés
                                                        dictionaryEmplacementsUtilisables = GetListEmplacementsAntecedentsValides(dictionaryEmplacementsRestants);
                                                        Console.WriteLine("Nombre d'emplacements utilisables couleur quelconque côté jaune : " + dictionaryEmplacementsUtilisables.Count);
                                                    }
                                                }
                                                /// Si on a un emplacement de dépose libre 
                                                if (dictionaryEmplacementsUtilisables.Count > 0)
                                                {
                                                    p.missionDeposeGobelet.Start(dictionaryEmplacementsUtilisables.ElementAt(0).Value, brasPlein.Key);
                                                    ///On valide qu'une dépose à été effectuée
                                                    p.matchDescriptor.DecrementeCompteurDeposeAEffectuer();
                                                    p.matchDescriptor.DecrementeCompteurGobelets();
                                                }
                                            }
                                            else
                                            {
                                                /// Si il n'y a plus de gobelets sur les bras
                                            }
                                        }
                                        else
                                        {
                                            ///Si on n'est pas en dépose, on traite l'élément choisi
                                            if (dictionaryElementsRestants.Count > 0)
                                            {
                                                var ElementChoisi = dictionaryElementsRestants.ElementAt(0);

                                                switch (ElementChoisi.Value.GetType().Name.ToString())
                                                {
                                                    case "Eurobot2021PositionTerrain":
                                                        var positionChoisie = (Eurobot2021PositionTerrain)ElementChoisi.Value;
                                                        p.missionLCDSM.Start(positionChoisie);
                                                        break;
                                                    case "Eurobot2021Gobelet":
                                                        /// Il n'y a pas de dépose à effectuer
                                                        /// On récupère la liste des bras disponibles dans le robot
                                                        var gobeletChoisi = (Eurobot2021Gobelet)ElementChoisi.Value;
                                                        var dictionarBrasVides = p.matchDescriptor.DictionaryBrasTurbine.Where(x => x.Value.HasGobelet == false).ToDictionary(x => x.Key, y => y.Value);

                                                        /// Si on a des bras dispo et des gobelets restant à prendre
                                                        if (dictionarBrasVides.Count > 0 && dictionaryElementsRestants.Count > 0)
                                                        {
                                                            ///var gobeletChoisi = dictionaryGobeletsRestants.ElementAt(0);
                                                            /// On prend le premier bras dispo : bof bof
                                                            /// var brasVide = dictionarBrasVides.ElementAt(0);

                                                            /// On prend le bras vide le plus proche angulairement de 
                                                            var dictionaryBrasVidesByDistanceAngulaire = dictionarBrasVides.OrderBy(x =>
                                                            {
                                                            ///On gère si l'angle de prise est défini ou pas
                                                            double anglePrise;
                                                                if (gobeletChoisi.AnglePrise != null)
                                                                {
                                                                    anglePrise = (double)gobeletChoisi.AnglePrise;
                                                                }
                                                                else
                                                                {
                                                                    anglePrise = Math.Atan2(gobeletChoisi.Pos.Y - p.robotCurrentLocation.Y, gobeletChoisi.Pos.X - p.robotCurrentLocation.X);
                                                                }
                                                                return Math.Abs(Toolbox.Modulo2PiAngleRad(x.Value.AngleBras + p.robotCurrentLocation.Theta - anglePrise));
                                                            });

                                                            var brasVide = dictionaryBrasVidesByDistanceAngulaire.ElementAt(0);

                                                            if (gobeletChoisi.AnglePrise != null)
                                                            {
                                                                p.missionPriseGobelet.Start(gobeletChoisi, (double)gobeletChoisi.AnglePrise, brasVide.Key);
                                                            }
                                                            else
                                                            {
                                                                double angle = Math.Atan2(gobeletChoisi.Pos.Y - p.robotCurrentLocation.Y, gobeletChoisi.Pos.X - p.robotCurrentLocation.X);
                                                                p.missionPriseGobelet.Start(gobeletChoisi, angle, brasVide.Key);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            /// Il n'y a plus de gobelets mais il reste des bras vides
                                                            /// On vide les bras chargés
                                                            p.matchDescriptor.ForçageCompteurDeposeAEffectuer(5 - dictionarBrasVides.Count);
                                                        }
                                                        break;
                                                    case "Eurobot2021MancheAir":
                                                        var mancheAirChoisie = (Eurobot2021MancheAir)ElementChoisi.Value;
                                                        p.missionWindFlag.Start(mancheAirChoisie);
                                                        break;
                                                    case "Eurobot2021Phare":
                                                        var PhareChoisi = (Eurobot2021Phare)ElementChoisi.Value;
                                                        p.missionPhare.Start(PhareChoisi);
                                                        break;
                                                    default:
                                                        break;

                                                }
                                            }
                                            else
                                            {
                                                var dictionarBrasPleins = p.matchDescriptor.DictionaryBrasTurbine.Where(x => x.Value.HasGobelet == true).ToDictionary(x => x.Key, y => y.Value);

                                                /// Si on a des gobelets dans les bras, on force à déposer
                                                if (dictionarBrasPleins.Count > 0)
                                                {
                                                    p.matchDescriptor.ForçageCompteurDeposeAEffectuer(dictionarBrasPleins.Count); //On force à vider tout
                                                }
                                                else
                                                {
                                                    /// Sinon on lance une tâche de scan de terrain et préhension automatique
                                                    /// On ne lance la tâche que si on est Sud
                                                    if(p.robotType == StrategyEurobot2021.Eurobot2021RobotType.RobotSud)
                                                        p.missionRamassageAutomatique.Start();
                                                }
                                            }
                                        }
                                        CalculateScore();
                                    }
                                }
                                break;
                            case SubTaskState.Exit:
                                state = TaskGameManagementState.DeplacementToZoneMouillage;                    /// L'état suivant ne doit être défini que dans le substate Exit
                                Console.WriteLine("Match terminé");
                                break;
                        }
                        break;
                    case TaskGameManagementState.DeplacementToZoneMouillage:
                        switch (subState)
                        {
                            case SubTaskState.Entry:
                                foreach (var task in p.listTasks)
                                {
                                    task.Init();
                                }
                                foreach (var mission in p.listMissions)
                                {
                                    mission.Init();
                                }

                                p.taskAvoidanceParametersModifiers.StartAvoidanceReduction(0.20, 0.3, 10000);/// Jusqu'à la fin du match
                                p.missionZoneMouillage.Start();
                                break;
                            case SubTaskState.EnCours:
                                if(DateTime.Now.Subtract(timestamp).TotalMilliseconds > 98000)
                                {
                                    ExitState();
                                }
                                break;
                            case SubTaskState.Exit:
                                state = TaskGameManagementState.MatchEnded;
                                break;
                        }
                        break;
                    case TaskGameManagementState.MatchEnded:
                        switch (subState)
                        {
                            case SubTaskState.Entry:
                                Console.WriteLine("Fin de match");
                                /// On calcule le score final avant de réinit les missions et tâches
                                CalculateScore();
                                /// Si pas de jack, on réinit les tasks, à voir plus tard TODO
                                foreach (var task in p.listTasks)
                                {
                                    task.Init();
                                }
                                foreach (var mission in p.listMissions)
                                {
                                    mission.Init();
                                }
                                p.OnEnableDisableMotor(false);
                                p.taskBrasDrapeau.StartPositionnementDrapeauSorti();
                                p.taskAffichageLidar.StartAffichagePermanentLigne2("Fin de Match");
                                break;
                            case SubTaskState.EnCours:
                                ///On ne quitte jamais l'état
                                ///On desactive les moteurs, asservissement
                                //ExitState();                                /// A appeler quand on souhaite passer à Exit
                                break;
                            case SubTaskState.Exit:
                                //state = TaskGameManagementState.Init;                       /// L'état suivant ne doit être défini que dans le substate Exit
                                Console.WriteLine("Tempo TROIS terminée");
                                break;
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        void SelectBestStrategy(Dictionary<int, Eurobot2021Gobelet> dictGobeletsRestants, Dictionary<int, Eurobot2021EmplacementDepose> dictEmplacementsUtilisables, Dictionary<int , Eurobot2021StateBrasTurbineRobot> dictBrasVides, Dictionary<int, Eurobot2021StateBrasTurbineRobot> dictBrasAvecGobelets)
        {

        }

        Dictionary<int, Eurobot2021EmplacementDepose> GetListEmplacementsAntecedentsValides(Dictionary<int, Eurobot2021EmplacementDepose> dictionaryEmplacementsRestants)
        {
            Dictionary<int, Eurobot2021EmplacementDepose> dictionaryEmplacementUtilisables = new Dictionary<int, Eurobot2021EmplacementDepose>();
            if (parent != null)
            {
                StrategyEurobot2021 p = parent as StrategyEurobot2021;
                foreach (var key in dictionaryEmplacementsRestants.Keys)
                {
                    bool ok = true;
                    var currentEmplacement = dictionaryEmplacementsRestants[key];
                    foreach (var keyPredecesseur in currentEmplacement.UnlockIdList)
                    {
                        /// SI un des emplacements prédécesseurs est libre, l'emplacement n'est pas utilisable.
                        if (p.matchDescriptor.DictionaryEmplacementDepose[keyPredecesseur].IsAvailable == true)
                            ok = false;
                    }
                    /// SI l'emplacement est utilisable après validation des prédécesseurs, on l'ajoute au dictionnaire des emplacements utilisables
                    if (ok)
                    {
                        dictionaryEmplacementUtilisables.Add(key, currentEmplacement);
                    }
                }
            }
            return dictionaryEmplacementUtilisables;
        }

        void CalculateScore()
        {
            StrategyEurobot2021 p = parent as StrategyEurobot2021;
            /// On calcule le score actuellement effectué par le robot
            /// On ajoute deux points si le phare est présent
            double score = 1;
            /// On récupère le nombre de gobelets vert déposés à un emplacement vert
            var dictionaryGobeletsVertDeposes = p.matchDescriptor.DictionaryEmplacementDepose.Where(x => x.Value.IsAvailable == false && x.Value.Color == Eurobot2021Color.Vert).ToDictionary(x => x.Key, y => y.Value);
            /// On récupère le nombre de gobelets rouge déposés à un emplacement rouge
            var dictionaryGobeletsRougeDeposes = p.matchDescriptor.DictionaryEmplacementDepose.Where(x => x.Value.IsAvailable == false && x.Value.Color == Eurobot2021Color.Rouge).ToDictionary(x => x.Key, y => y.Value);
            /// On récupère le nombre de gobelets déposés dans un emplacement neutre
            var dictionaryGobeletsNeutreDeposes = p.matchDescriptor.DictionaryEmplacementDepose.Where(x => x.Value.IsAvailable == false && x.Value.Color == Eurobot2021Color.Neutre).ToDictionary(x => x.Key, y => y.Value);

            /// Pour un gobelet neutre on ajoute un point
            score += dictionaryGobeletsNeutreDeposes.Count;
            /// Pour un gobelet correctement positionné sur un chenal de la même couleur, on ajoute un point suplémentaire
            score += 2 * (dictionaryGobeletsRougeDeposes.Count + dictionaryGobeletsVertDeposes.Count);
            /// Pour chaque paire de gobelets, on ajoute 2 points
            if (dictionaryGobeletsRougeDeposes.Count >= dictionaryGobeletsVertDeposes.Count)
                score += 2 * dictionaryGobeletsVertDeposes.Count;
            else
                score += 2 * dictionaryGobeletsRougeDeposes.Count;
            lock (p.matchDescriptor.listElementsJeu)
            {
                /// On recherche les manches à airs relevé
                var dictionaryMancheAir = p.matchDescriptor.listElementsJeu.Where(x => x.Value.isAvailable == false && x.Value is Eurobot2021MancheAir).ToDictionary(x => x.Key, y => y.Value);
                /// Si une manche à air relevée, on ajoute 5 points
                if (dictionaryMancheAir.Count == 1)
                    score += 5;
                /// Si deux manches à air relevées, on ajoute 15 points
                else if (dictionaryMancheAir.Count == 2)
                    score += 15;
                /// Si le phare a été activé, on ajoute 3 points. S'il est actif à la fin du match, on ajoute 10 points
                var dictionaryPhare = p.matchDescriptor.listElementsJeu.Where(x => x.Value.isAvailable == false && x.Value is Eurobot2021Phare).ToDictionary(x => x.Key, y => y.Value);
                if (dictionaryPhare.Count == 1)
                    score += 13;
                /// Si le drapeau est levé, on ajoute 10 points
                if (state == TaskGameManagementState.MatchEnded)
                    score += 5;
            }
            /// Si le robot est dans le zone de mouillage indiqué par la girouette (10 points par robot)
            /// Sinon si valide dans l'autre zone de mouillage (3 points par robot)
            // Mission non implémentée
            // /!\ Solution temporaire /!\
            // Si un robot est dans la bonne zone de mouillage ça fait 10 points,
            // S'il est dans la mauvaise 3 points
            // On fait exprès d'envoyer un robot dans chaque zone
            // On ajoute que 3 au score pour chaque robot, hypothèse basse
            if (state == TaskGameManagementState.MatchEnded && p.missionZoneMouillage.isFinished)
            {
                if (p.robotType == StrategyEurobot2021.Eurobot2021RobotType.RobotNord)
                    score += 3;
                else
                    score += 3;
            }

            //Si on a un score décent, on pénalise de 5 pts par robots : fail statistique
            if (score >= 20)
                score -= 5;

            /// On affiche le score sur le lidar
            string textToDisplayLine1 = "Score : " + score + " points";
            if (p.taskAffichageLidar != null)
                p.taskAffichageLidar.StartAffichagePermanentLigne1(textToDisplayLine1);
        }
    }
}
