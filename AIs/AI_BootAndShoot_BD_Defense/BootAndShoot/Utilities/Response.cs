using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BootAndShoot
{
    public class Response
    {
        #region Constructors
        public Response() {}
        //Move or Kick
        public Response(int number, Action act, Position dest, double speed = 100)
        {
            this.playerNumber = number;
            this.action = act;
            this.destination = dest;
            this.speed = speed;
            Logger.log("Player " + number + " " + act.ToString() + "s to " + dest, Logger.LogLevel.INFO);
        }
        //TP
        public Response(int number, Action act)
        {
            this.playerNumber = number;
            this.action = act;
            Logger.log("Player " + number + " Takes Possession", Logger.LogLevel.INFO);
        }
        //Turn
        public Response(int number, Action act, double direction)
        {
            this.playerNumber = number;
            this.action = act;
            this.direction = direction;

            Logger.log("Player " + number + " Turns by " + direction, Logger.LogLevel.INFO);
        }
        #endregion
        
        public bool MoveOrTurn(Player player, Position targetPosition, double speed = 100.0)
        {
            int number = player.playerNumber;
            var ballPosition = player.Ball().position;
            var playerPosition = player.position;
            if (playerPosition.IsEqual(targetPosition) == false)
            {
                this.playerNumber = number;
                this.action = Action.Move;
                this.destination = targetPosition;
                this.speed = speed;
                Logger.log("Player " + number + " Moves to " + targetPosition, Logger.LogLevel.INFO);
                return true;
            }
            else
            {
                //If already at targetPosition then track ball
                double direction = playerPosition.getAngle(ballPosition);
                this.playerNumber = number;
                this.action = Action.Turn;
                this.direction = direction;
                Logger.log("Player " + number + " Turns by " + direction, Logger.LogLevel.INFO);
                return true;
            }
        }



        //Called by goalkeeper only
        public bool MoveOrTP(Player player, Position moveTo, double speed = 100.0)
        {
            int number = player.playerNumber;
            var ballPosition = player.Ball().position;
            var playerPosition = player.position;
            if (player.position.distanceFrom(moveTo) > 0.5)
            {
                this.playerNumber = number;
                this.action = Action.Move;
                this.destination = moveTo;
                this.speed = speed;
                Logger.log("Player " + number + " Moves to " + moveTo, Logger.LogLevel.INFO);
                return true;
            }
            else
            {
                //If already at moveTo Position then call TakePossesion
                this.playerNumber = number;
                this.action = Action.TakePossession;
                Logger.log("Player " + number + " Takes Possession", Logger.LogLevel.INFO);
                return true;
            }
        }
        
        public bool KickOrTurn(FieldPlayer player, Position target, double speed = 100.0)
        {
            var playerNumber = player.playerNumber;
            var playerPosition = player.position;
            double playerDirection = player.direction;
            double destinationDirection = Math.Round(playerPosition.getAngle(target), 3);
           
            double distanceFCP = player.closestOpponentPlayerDistance;
            if (playerDirection == destinationDirection || distanceFCP <= 1.6)
            {
                this.playerNumber = player.playerNumber;
                this.action = Action.Kick;
                this.destination = target;
                this.speed = speed;
                Logger.log("Player " + playerNumber + " Kicks to " + destination, Logger.LogLevel.INFO);
                return true;
            }
            else
            {
                this.playerNumber = player.playerNumber;;
                this.action = Action.Turn;
                this.direction = destinationDirection;
                Logger.log("Player " + playerNumber + " Turns by " + destinationDirection, Logger.LogLevel.INFO);
                return true;
            }
        }
        public bool ShootOrTurn(FieldPlayer player, Position target)
        {
            var playerNumber = player.playerNumber;
            var playerPosition = player.position;
            double playerDirection = player.direction;
            double destinationDirection = Math.Round(playerPosition.getAngle(target), 3);

            double distanceFCP = player.closestOpponentPlayerDistance;
            if (playerDirection == destinationDirection || distanceFCP <= 1.6)
            {
                if(distanceFCP <= 1.6)
                {
                    destination = player.team.getGoalCentre(Team.GoalType.THEIR_GOAL);
                    Random rnd = new Random();
                    destination.y += ((rnd.NextDouble() < 0.5) ? 2.0 : -2.0);
                }
                this.playerNumber = player.playerNumber;
                this.action = Action.Kick;
                this.destination = target;
                this.speed = 100.0;
                Logger.log("Player " + playerNumber + " Kicks to " + destination, Logger.LogLevel.INFO);
                return true;
            }
            else
            {
                this.playerNumber = player.playerNumber; ;
                this.action = Action.Turn;
                this.direction = destinationDirection;
                Logger.log("Player " + playerNumber + " Turns by " + destinationDirection, Logger.LogLevel.INFO);
                return true;
            }
        }
        public bool KickOrTurn(GoalKeeper player, Position target, double speed = 100.0)
        {
            var playerNumber = player.playerNumber;
            var playerPosition = player.position;
            double playerDirection = player.direction;
            double destinationDirection = Math.Round(playerPosition.getAngle(target), 3);
            if (playerDirection == destinationDirection)
            {
                this.playerNumber = player.playerNumber;
                this.action = Action.Kick;
                this.destination = target;
                this.speed = speed;
                Logger.log("Player " + playerNumber + " Kicks to " + destination, Logger.LogLevel.INFO);
                return true;
            }
            else
            {
                this.playerNumber = player.playerNumber; ;
                this.action = Action.Turn;
                this.direction = destinationDirection;
                Logger.log("Player " + playerNumber + " Turns by " + direction, Logger.LogLevel.INFO);
                return true;
            }
        }
        public enum Action { Move,Kick,TakePossession,Turn }

        public int playerNumber { get; set; }
        public Action action { get; set;}
        public Position destination { get; set; }
        public double speed { get; set; }
        public double direction { get; set; }
        public string ActToString()
        {

            switch(action)
            {
                case(Action.Kick):
                    {
                        return "KICK";
                    }
                case (Action.Move):
                    {
                        return "MOVE";
                    }
                case (Action.TakePossession):
                    {
                        return "TAKE_POSSESSION";
                    }
                case (Action.Turn):
                    {
                        return "TURN";
                    }
                default:
                    return null;
            }
        }

        public JSObject ToJson()
        {
            var response = new JSObject();
            response.add("playerNumber", playerNumber);
            response.add("action", ActToString());
            switch (action)
            {
                case (Action.Kick):
                    {
                        response.add("destination", destination);
                        response.add("speed", speed);
                        break;
                    }
                case (Action.Move):
                    {
                        response.add("destination", destination);
                        response.add("speed", speed);
                        break;
                    }
                case (Action.TakePossession):
                    {
                        //nothing
                        break;
                    }
                case (Action.Turn):
                    {
                        response.add("direction", direction);
                        break;
                    }
            }
            return response;
        }
    }
}
