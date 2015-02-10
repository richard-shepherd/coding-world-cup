using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BootAndShoot
{
    class JSWrapper
    {
        #region Action Wrapper
        public JSObject move(int playerNumber, Position destination)
        {
            action = new JSObject();
            action.add("playerNumber", playerNumber);
            action.add("action", "MOVE");
            action.add("destination", destination);
            action.add("speed", 100.0);
            return action;
        }
        public JSObject kick(int playerNumber, Position destination, double speed = 100.0)
        {
            action = new JSObject();
            action.add("action", "KICK");
            action.add("playerNumber", playerNumber);
            action.add("destination", destination);
            action.add("speed", speed);
            return action;
        }

        public JSObject turn(int playerNumber, double direction)
        {
            action = new JSObject();
            action.add("action", "TURN");
            action.add("playerNumber", playerNumber);
            action.add("direction", direction);
            return action;
        }

        public JSObject takePossession(int playerNumber)
        {
            action = new JSObject();
            action.add("playerNumber", playerNumber);
            action.add("action", "TAKE_POSSESSION");
            return action;
        }
        #endregion

        #region private data
        JSObject action;
        #endregion

    }
}
