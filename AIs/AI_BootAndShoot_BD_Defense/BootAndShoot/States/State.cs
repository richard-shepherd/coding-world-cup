using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BootAndShoot
{
 
    public abstract class State<T>
    {
        #region Public Methods
        public abstract void Execute(T entity_type);

        public abstract void Exit(T entity_type);

        public abstract bool Enter(T entity_type);

       public abstract bool OnMessage(T entity_type, Telegram t);
        #endregion
    }
}
