using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.DTO;

namespace Utility.Helper
{
    public static class GameHelper
    {

       public static Player GetClosestPlayer(Position p, List<Player> players, bool excludeGoalKeeper = false)
        {
            Dictionary<Player, double> distances = new Dictionary<Player, double>();
            foreach (var player in players)
            {
                if (excludeGoalKeeper && player.staticState.playerType == "G")
                {
                    continue;
                }
                distances.Add(player, GetDistance(player.dynamicState.position, p));

            }

            var x = distances.Min(m => m.Value);
            return distances.Where(d => d.Value == x).First().Key;
        }

        public static Position GetClosestPosition(Position current, List<Position> positions)
        {
            Dictionary<Position, double> distances = new Dictionary<Position, double>();
            foreach (var p in positions)
            {
                distances.Add(p, GetDistance(current, p));
            }
            var minVal = distances.Select(s => s.Value).Min();
            return distances.Where(x => x.Value == minVal).First().Key;
        }
       
        public static bool IsPositionInGoalCircle(Position p, Position circleCenter, double radius)
        {
            var val = Math.Pow(p.x - circleCenter.x, 2) + Math.Pow(p.y - circleCenter.y, 2);
            return (val < radius * radius);
        }
               
        public static double GetDistance(Position p1, Position p2)
        {
            var temp = Math.Pow(p1.x - p2.x, 2) + Math.Pow(p1.y - p2.y, 2);
            return Math.Sqrt(temp);
        }

        public static string GetRequestType(string json)
        {
            if (json.Contains("CONFIGURE_ABILITIES"))
                return "CONFIGURE_ABILITIES";

            if (json.Contains("PLAY"))
                return "PLAY";

            if (json.Contains("KICKOFF"))
                return "KICKOFF";

            if (json.Contains("GAME_START"))
                return "GAME_START";

            if (json.Contains("TEAM_INFO"))
                return "TEAM_INFO";

            if (json.Contains("HALF_TIME"))
            {
                return "HALF_TIME";
            }

            if (json.Contains("START_OF_TURN"))
            {
                return "START_OF_TURN";
            }

            return "UNKNOWN";
        }

        public static Position GetIntersectionPoint(Position p1, Position p2, Position p3, Position p4)
        {
            var denom = ((p1.x - p2.x) * (p3.y - p4.y)) - ((p1.y - p2.y) * (p3.x - p4.x));
            if (denom == 0)
            {
                return null;
            }

            var px = (((p1.x * p2.y - p1.y * p2.x) * (p3.x - p4.x)) - ((p1.x - p2.x) * (p3.x * p4.y - p3.y * p4.x))) / denom;
            var py = (((p1.x * p2.y - p1.y * p2.x) * (p3.y - p4.y)) - ((p1.y - p2.y) * (p3.x * p4.y - p3.y * p4.x))) / denom;

            return new Position() { x = px, y = py };
        }

        public static Position GetGoalPostIntersection(Position ball_p1, Position ball_p2, Position gp_upper, Position gp_lower)
        {
            return GetIntersectionPoint(ball_p1, ball_p2, gp_upper, gp_lower);
        }

        public static double GetDistanceOfPointFromLine(Position p1, Position p2, Position p0)
        {
            var y2y1 = p2.y - p1.y;
            var x2x1 = p2.x - p1.x;
            var numerator = Math.Abs((y2y1 * p0.x) - (x2x1 * p0.y) + p2.x * p1.y - p2.y * p1.x);
            var denom = Math.Sqrt(Math.Pow(y2y1, 2) + Math.Pow(x2x1, 2));
            return numerator / denom;

        }
                   
    }
}
