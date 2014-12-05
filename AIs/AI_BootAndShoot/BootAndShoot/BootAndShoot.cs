using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Helpers;

namespace BootAndShoot
{
    /// <summary>
    /// A coding-world-cup API which:
    /// - Keeps two players in its own half as defenders.
    /// - Tries to keep the players in "zones"
    /// - Kicks the ball forward into another player's zone if further than 30 from the goal.
    /// - Shoots at goal if within 30m.
    /// </summary><remarks>
    /// Derives from CodingWorldCupAPI and implements its abstract methods.
    /// </remarks>
    class BootAndShoot : CodingWorldCupAPI
    {
        protected override void processRequest_Play(dynamic data)
        {
            throw new NotImplementedException();
        }
    }
}
