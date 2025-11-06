using System;
using UnityEngine;

namespace PuddingCat
{
    [Serializable]
    public class BulletData : EntityData
    {
        [SerializeField]
        private int m_OwnerId = 0; // 发射者的实体ID

        [SerializeField]
        private CampType m_OwnerCamp = CampType.Unknown; // 发射者的阵营

        [SerializeField]
        private int m_Attack = 0; // 子弹的攻击力

        [SerializeField]
        private float m_Speed = 0f; // 子弹的飞行速度

        /// <summary>
        /// 子弹数据的构造函数。
        /// 它的属性由创建者（通常是WeaponData）在创建时动态计算并直接传入，
        /// 而不是从数据表中读取。
        /// </summary>
        public BulletData(int entityId, int typeId, int ownerId, CampType ownerCamp, int attack, float speed)
            : base(entityId, typeId)
        {
            m_OwnerId = ownerId;
            m_OwnerCamp = ownerCamp;
            m_Attack = attack;
            m_Speed = speed;
        }

        public int OwnerId { get { return m_OwnerId; } }
        public CampType OwnerCamp { get { return m_OwnerCamp; } }
        public int Attack { get { return m_Attack; } }
        public float Speed { get { return m_Speed; } }
    }
}