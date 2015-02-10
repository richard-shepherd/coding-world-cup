using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BootAndShoot
{

    class EntityManager
    {
        private Dictionary<int, BaseGameEntity> EntityDic;


        #region Singlton
        private static EntityManager m_instance;
        public static EntityManager instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new EntityManager();
                }
                return m_instance;
            }
        }
        private EntityManager() 
        {
            EntityDic = new Dictionary<int, BaseGameEntity>();
        }

        #endregion


        public BaseGameEntity GetEntityFromID(int id)
        {
            //find the entity
            if (EntityDic.ContainsKey(id))
            {
                return EntityDic[id];
            }

            Logger.log("<EntityManager::GetEntityFromID>: invalid ID", Logger.LogLevel.ERROR);
            return null;

        }


        public void RegisterEntity(BaseGameEntity NewEntity)
        {
          //m_EntityMap.insert(std::make_pair(NewEntity->ID(), NewEntity));

            EntityDic.Add(NewEntity.id, NewEntity);
        }

    }
}
