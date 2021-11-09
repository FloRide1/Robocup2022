using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace PlayBook_NS
{
    public class PlayBook
    {
        Dictionary<string, PlayCard> playingCards = new Dictionary<string, PlayCard>();
    }

    public class PlayCard
    {
        Dictionary<int, Location> preferredLocation = new Dictionary<int, Location>();
        Dictionary<int, List<Comportment>> preferredComportements = new Dictionary<int, List<Comportment>>();
        Dictionary<int, RectangleD> preferredZones = new Dictionary<int, RectangleD>();
    }

    public enum Comportment
    {
        Waiting,
        Pressing,
        Unmarking,
        BetweenOwnGoalAndBall_2m,
        BetweenOwnGoalAndBall_5m,
        PrepareForPassToTeammate,
    }

    public enum PlayingSituations
    {
        Defense,
        Attack,
        Kickoff,
        ThrowInFor,
        ThrowInAgainst,
        CornerFor,
        CornerAgainst,
        PenaltyFor,
        PenaltyAgainst,
        GoalKickFor,
        GoalKickAgainst,

    }


    //public class Comportment
    //{

    //}
}
