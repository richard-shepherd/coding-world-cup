using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility.DTO
{
    public enum DirectionType
    {
        // We don't yet know the playing direction...
        DONT_KNOW,

        // We are shooting at the left goal...
        LEFT,

        // We are shooting at the right goal...
        RIGHT
    }

    public class GameUpdateDTO
    {
        public GameTime game { get; set; }
        public Ball ball { get; set; }
        public TeamInfo team1 { get; set; }
        public TeamInfo team2 { get; set; }
        public string eventType { get; set; }
        public string messageType { get; set; }
    }

    public class GameTime
    {
        public float currentTimeSeconds { get; set; }
    }

    public class Ball
    {
        public Position position { get; set; }
        public Vector vector { get; set; }
        public double speed { get; set; }
        public int controllingPlayerNumber { get; set; }
    }

    public class Position
    {
       public double x
        {
            get;
            set;
        }
        public double y
        {
            get;
            set;
        }
    }

    public class Vector
    {
        public float x { get; set; }
        public float y { get; set; }
    }

    public class TeamInfo
    {
        public Team team { get; set; }
        public Player[] players { get; set; }
    }

    public class Team
    {
        public string name { get; set; }
        public int score { get; set; }
        public string direction { get; set; }
    }

    public class Player
    {
        public Staticstate staticState { get; set; }
        public Dynamicstate dynamicState { get; set; }
    }

    public class Staticstate
    {
        public int playerNumber { get; set; }
        public string playerType { get; set; }
        public float kickingAbility { get; set; }
        public float runningAbility { get; set; }
        public float ballControlAbility { get; set; }
        public float tacklingAbility { get; set; }
    }

    public class Dynamicstate
    {
        public Position position { get; set; }
        public bool hasBall { get; set; }
        public float direction { get; set; }
    }

    public class ConfigureAbilitiesRequestDTO
    {
        public string requestType { get; set; }
        public int totalKickingAbility { get; set; }
        public int totalRunningAbility { get; set; }
        public int totalBallControlAbility { get; set; }
        public int totalTacklingAbility { get; set; }
        public string messageType { get; set; }
    }

    public class ConfigureAbilitiesResponseDTO
    {
        public string requestType { get; set; }
        public PlayerAbility[] players { get; set; }
    }

    public class PlayerAbility
    {
        public int playerNumber { get; set; }
        public float kickingAbility { get; set; }
        public float runningAbility { get; set; }
        public float ballControlAbility { get; set; }
        public float tacklingAbility { get; set; }
    }

    public class KickOffRequestDTO
    {
        public string eventType { get; set; }
        public Team team1 { get; set; }
        public Team team2 { get; set; }
        public int teamKickingOff { get; set; }
        public string messageType { get; set; }
    }

    public class KickOffResponseDTO
    {
        public string requestType { get; set; }
        public KickOffPlayer[] players { get; set; }
    }

    public class KickOffPlayer
    {
        public int playerNumber { get; set; }
        public Position position { get; set; }
        public int direction { get; set; }
    }

    public class GoalRequestdTO
    {
        public string eventType { get; set; }
        public GoalTeam team1 { get; set; }
        public GoalTeam team2 { get; set; }
        public string messageType { get; set; }
    }

    public class GoalTeam
    {
        public string name { get; set; }
        public int score { get; set; }
        public string direction { get; set; }
    }



    public class PlayResponseDTO
    {
        public string requestType { get; set; }
        public Action[] actions { get; set; }
    }

    public class Action
    {
        public int playerNumber { get; set; }
        public string action { get; set; }
        public Position destination { get; set; }
        public double speed { get; set; }
    }

    public class Destination
    {
        public float x { get; set; }
        public float y { get; set; }
    }

    public class GridCell
    {
        public double X { get; set; }
        public double Y { get; set; }
    }

    public class GameStartDTO
    {
        public string eventType { get; set; }
        public Pitch pitch { get; set; }
        public int gameLengthSeconds { get; set; }
        public string messageType { get; set; }
    }

    public class Pitch
    {
        public int width { get; set; }
        public int height { get; set; }
        public int goalCentre { get; set; }
        public int goalY1 { get; set; }
        public int goalY2 { get; set; }
        public Centrespot centreSpot { get; set; }
        public int centreCircleRadius { get; set; }
        public int goalAreaRadius { get; set; }
    }

    public class Centrespot
    {
        public int x { get; set; }
        public int y { get; set; }
    }

    public class TeamInfoDTO
    {
        public string eventType { get; set; }
        public int teamNumber { get; set; }
        public TeamInfoPlayer[] players { get; set; }
        public string messageType { get; set; }
    }

    public class TeamInfoPlayer
    {
        public int playerNumber { get; set; }
        public string playerType { get; set; }
    }



}
