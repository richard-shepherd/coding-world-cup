using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Helpers;

namespace BootAndShoot
{
    /// <summary>
    /// A coding-world-cup API which:
    /// - Keeps three players in its own half as defenders.
    /// - Puts two players in the opponents half as forwards.
    /// 
    /// Defenders
    /// ---------
    /// The defenders try to take possession of the ball when it
    /// is in their half. If they get it, they kick it towards the
    /// forwards.
    /// 
    /// When the ball is in the other half, they move back to their
    /// default positions.
    /// 
    /// Forwards
    /// --------
    /// The forwards try to take possession when the ball is in the
    /// opponent's half. If they get it, they shoot for the goal.
    /// 
    /// When the ball is in the other half, they move back to their
    /// default positions.
    /// 
    /// Goalkeeper
    /// ----------
    /// The goalkeeper tries to keep between the ball and the goal centre.
    /// He tries to take possession when the ball is close.
    /// </summary><remarks>
    /// Derives from CodingWorldCupAPI and implements various virtual methods.
    /// </remarks>
    class BootAndShoot : CodingWorldCupAPI
    {
        #region CodingWorldCupAPI implementation

        /// <summary>
        /// Called when team-info has been updated.
        /// 
        /// At this point we know the player-numbers for the players
        /// in our team, so we can choose which players are playing in
        /// which positions.
        /// </summary>
        protected override void onTeamInfoUpdated()
        {
            // We assign players to the positions...
            var playerNumbers = new List<int>(this.teamPlayers.Keys);
            this.playerNumber_LeftWingDefender = playerNumbers[0];
            this.playerNumber_CentreDefender = playerNumbers[1];
            this.playerNumber_RightWingDefender = playerNumbers[2];
            this.playerNumber_LeftWingForward = playerNumbers[3];
            this.playerNumber_RightWingForward = playerNumbers[4];

            // We set up the default positions...
            this.defaultPositions[new PositionKey(this.playerNumber_LeftWingDefender, PlayingDirectionType.RIGHT)] = new Position(25, 10);
            this.defaultPositions[new PositionKey(this.playerNumber_CentreDefender, PlayingDirectionType.RIGHT)] = new Position(25, 25);
            this.defaultPositions[new PositionKey(this.playerNumber_RightWingDefender, PlayingDirectionType.RIGHT)] = new Position(25, 40);
            this.defaultPositions[new PositionKey(this.playerNumber_LeftWingForward, PlayingDirectionType.RIGHT)] = new Position(75, 15);
            this.defaultPositions[new PositionKey(this.playerNumber_RightWingForward, PlayingDirectionType.RIGHT)] = new Position(75, 35);

            this.defaultPositions[new PositionKey(this.playerNumber_LeftWingDefender, PlayingDirectionType.LEFT)] = new Position(75, 40);
            this.defaultPositions[new PositionKey(this.playerNumber_CentreDefender, PlayingDirectionType.LEFT)] = new Position(75, 25);
            this.defaultPositions[new PositionKey(this.playerNumber_RightWingDefender, PlayingDirectionType.LEFT)] = new Position(75, 10);
            this.defaultPositions[new PositionKey(this.playerNumber_LeftWingForward, PlayingDirectionType.LEFT)] = new Position(25, 35);
            this.defaultPositions[new PositionKey(this.playerNumber_RightWingForward, PlayingDirectionType.LEFT)] = new Position(25, 15);
        }

        #endregion

        #region Private functions

        /// <summary>
        /// Returns the default position for the player passed in, when playing 
        /// in the direction passed in.
        /// </summary>
        private Position getDefaultPosition(int playerNumber, PlayingDirectionType playingDirection)
        {
            var key = new PositionKey(playerNumber, playingDirection);
            return this.defaultPositions[key];
        }

        #endregion

        #region Private data

        // The player-numbers for the various positions...
        private int playerNumber_LeftWingDefender = -1;
        private int playerNumber_CentreDefender = -1;
        private int playerNumber_RightWingDefender = -1;
        private int playerNumber_LeftWingForward = -1;
        private int playerNumber_RightWingForward = -1;

        // Default positions, keyed by player-number and direction...
        private class PositionKey : Tuple<int, PlayingDirectionType> 
        {
            public PositionKey(int playerNumber, PlayingDirectionType direction): base(playerNumber, direction) { }
        }
        private Dictionary<PositionKey, Position> defaultPositions = new Dictionary<PositionKey, Position>();

        #endregion
    }
}
