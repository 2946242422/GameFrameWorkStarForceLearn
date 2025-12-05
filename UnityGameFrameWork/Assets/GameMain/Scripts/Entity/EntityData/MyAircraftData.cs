using System;
using System.Collections.Generic;
using UnityEngine;

namespace PuddingCat
{
    [Serializable]
    public class MyAircraftData : AircraftData
    {
        [SerializeField]
        private string m_Name = null;
        public List<DRUpgrade> AcquiredUpgrades { get; private set; }
        public MyAircraftData(int entityId, int typeId)
            : base(entityId, typeId, CampType.Player)
        {
            AcquiredUpgrades = new List<DRUpgrade>();
        }

        /// <summary>
        /// 角色名称。
        /// </summary>
        public string Name
        {
            get
            {
                return m_Name;
            }
            set
            {
                m_Name = value;
            }
        }
    }
}