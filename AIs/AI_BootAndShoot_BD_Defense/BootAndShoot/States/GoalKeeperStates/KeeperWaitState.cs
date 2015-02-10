using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BootAndShoot
{
    public class KeeperWaitState : State<GoalKeeper>
    {
        #region Singlton
        private static KeeperWaitState m_instance;
        public static KeeperWaitState instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new KeeperWaitState();
                }
                return m_instance;
            }
        }
        private KeeperWaitState() { }

        #endregion


        public override bool Enter(GoalKeeper player)
        {

            Logger.log("Golakeepr " + player.playerNumber + " enters wait state", Logger.LogLevel.INFO);
            if (player.action == null)
            {
                return true;
            }
            return false;

        }

        public override void Execute(GoalKeeper player)
        {
            var goalkeeperPosition = player.position;
            var ballPosition = player.Ball().position;
            Vector ballVector = player.Ball().vector;
            double distance = player.position.distanceFrom(ballPosition);
            Position goalCentre = player.team.getGoalCentre(Team.GoalType.OUR_GOAL);
            if(player.hasBall && player.position.IsEqual(player.returnPosition))
            {
                player.stateMachine.ChangeState(PassBall.instance);
                return;
            }
            if (player.hasBall && !player.position.IsEqual(player.returnPosition))
            {
                player.stateMachine.ChangeState(KeeperReturnState.instance);
                return;
            }
            if (player.BallIsInGoalArea() && distance <= 0.5)
            {
   
                // We are close to the ball, so we try to take possession...
                player.action = new Response(player.playerNumber, Response.Action.TakePossession);
                return;
            }
            if (player.BallIsInGoalArea() && (player.Ball().speed == 0))
            {
                // We are too far away, so we move towards the ball...
                player.action = new Response(player.playerNumber, Response.Action.Move, ballPosition);
                return;
            }
            if (player.Ball().speed != 0)
            {

                bool found = false;
                double scale = (player.Ball().speed * player.Ball().speed) / (2 * Ball.friction);
                Vector scaledVector = player.Ball().vector.getScaledVector(scale);
                var moveTo = ballPosition.getPositionPlusVector(scaledVector);

                if ((player.team.playingDirection == Team.DirectionType.RIGHT && moveTo.x <= goalCentre.x) || (player.team.playingDirection == Team.DirectionType.LEFT && moveTo.x >= goalCentre.x))
                {
                    // ball will hit the goalline
                    found = true;
                    moveTo.y = crossingPoint(ballPosition, moveTo, goalCentre.x);
                    moveTo.x = goalCentre.x;
                }
                if (found && moveTo.y >= 20.9 && moveTo.y <= 29.1)
                {

                    double x = (player.team.playingDirection == Team.DirectionType.RIGHT) ? 0.5 : 99.5;
                    double y = crossingPoint(moveTo, ballPosition, x);

                    //Vector scaleDistance = targetVector.getScaledVector(0.5);
                    // We move to this position...
                    //Position move = moveTo.getPositionPlusVector(scaleDistance);

                    Position move = new Position(x, y);

                    //optimizing move positon

                    double offset = 4 / Math.Sqrt(3);
                    x = (player.team.playingDirection == Team.DirectionType.RIGHT) ? (0.5 + offset) : (99.5 - offset);
                    y = crossingPoint(ballPosition, move, x);
                    Position fromPoint = new Position(x, y);
                    Position optimizedMove = closetOnLineFromPoint(fromPoint, move, goalkeeperPosition);


                    var response = new Response();
                    if(response.MoveOrTP(player,move))
                    {
                        player.action = response;
                        return;
                    }

                }
            }
            //move to default position if not on default position
            if(!player.position.IsEqual(player.returnPosition))
            {
                player.stateMachine.ChangeState(KeeperReturnState.instance);
                return;
            }

            player.TrackBall();

            return;

                
        }

        public override void Exit(GoalKeeper player)
        {
            Logger.log("Goalkeeper " + player.playerNumber + " exits wait state", Logger.LogLevel.INFO);
        }

        public override bool OnMessage(GoalKeeper player, Telegram t) { return false; }

        //
        // crossingPoint
        // -------------
        // Finds the line between p1 and p2 and returns its intersection with
        // the line x=x.
        //
        private double crossingPoint(Position p1, Position p2, double x)
        {
            // We calculate the formula for the line, y=mx+c...
            double diffY = p2.y - p1.y;
            double diffX = p2.x - p1.x;
            double m = 0.0;
            if (diffX == 0.0)
            {
                m = 0.0;
            }
            else
            {
                m = diffY / diffX;
            }
            double c = p1.y - m * p1.x;

            // And calculate the value for x...
            double y = m * x + c;
            return y;
        }

        private Position closetOnLineFromPoint(Position v, Position w, Position p)
        {
            Vector vw = new Vector(v, w);
            double l2 = vw.SquareLength;  // i.e. |w-v|^2 -  avoid a sqrt
            if (l2 == 0.0)
            {
                return v;   // v == w case
            }

            // Consider the line extending the segment, parameterized as v + t (w - v).
            // We find projection of point p onto the line. 
            // It falls where t = [(p-v) . (w-v)] / |w-v|^2

            //double t = dot(p - v, w - v) / l2;
            Vector vp = new Vector(v, p);
            double t = vp.Dot(vw) / l2;
            if (t < 0.0)
            {
                return v;      // Beyond the 'v' end of the segment
            }
            else if (t > 1.0)
            {
                return w;  // Beyond the 'w' end of the segment
            }
            //Vector projection = v + t * (w - v);  // Projection falls on the segment

            Vector temp = vw.getScaledVector(t);
            Position projection = v.getPositionPlusVector(temp);
            return projection;
        }
    }
}
