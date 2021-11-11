using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace PlayBook_NS
{
    public class PlayBook
    {
        public Dictionary<string, PlayCard> playingCards = new Dictionary<string, PlayCard>();
        public PlayBook()
        {
            playingCards = new Dictionary<string, PlayCard>();
        }
    }

    public class PlayCard
    {
        public Dictionary<int, Location> preferredLocation = new Dictionary<int, Location>();
        public Dictionary<int, List<Comportment>> preferredComportements = new Dictionary<int, List<Comportment>>();
        public Dictionary<int, RectangleD> preferredZones = new Dictionary<int, RectangleD>();

        public PlayCard()
        {
            preferredLocation = new Dictionary<int, Location>();
            preferredComportements = new Dictionary<int, List<Comportment>>();
            preferredZones = new Dictionary<int, RectangleD>();
        }
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
}
