using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Constants
{
    //public enum Equipe
    //{
    //    Jaune,
    //    Bleue,
    //}

    public enum GameMode
    {
        RoboCup,
        Eurobot2021,
        Eurobot2022,
        Cachan,
        Demo
    }

    public enum PlayingSide
    {
        Left,
        Right
    }

    public enum ObjectType
    {
        Balle,
        Obstacle,
        RobotTeam1,
        RobotTeam2,
        Poteau,
        Balise,
        LimiteHorizontaleHaute,
        LimiteHorizontaleBasse,
        LimiteVerticaleGauche,
        LimiteVerticaleDroite,
    }
}
