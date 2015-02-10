using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BootAndShoot
{

    public class BaseGameEntity
    {
        private static int m_iNextValidID = 0;
        public BaseGameEntity(int val) { SetID(val); }
        public int id { get; protected set; }
        public Vector positionVector { get; set; }

        public Vector heading { get; set; }

        public virtual bool HandleMessage(Telegram msg) { return false; }

        public virtual void Update() { }

        public static int GetNextValidID() { return m_iNextValidID; }

         //----------------------------- SetID -----------------------------------------
        //
        //  this must be called within each constructor to make sure the ID is set
        //  correctly. It verifies that the value passed to the method is greater
        //  or equal to the next valid ID, before setting the ID and incrementing
        //  the next valid ID
        //-----------------------------------------------------------------------------
        public void SetID(int val)
        {
            //make sure the val is equal to or greater than the next available ID
            if(val > m_iNextValidID)
            {
                Logger.log("<BaseGameEntity::SetID>: invalid ID", Logger.LogLevel.ERROR);
            }

            id = val;
    
            m_iNextValidID = id + 1;
        }

    }
}
