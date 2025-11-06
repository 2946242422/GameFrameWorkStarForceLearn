using GameFramework.DataTable;
using System;
using UnityEngine;

namespace PuddingCat
{
    [Serializable]
    public class WeaponData : AccessoryObjectData
    {
        [SerializeField]
        private int m_Attack = 0; // 武器的基础攻击力

        [SerializeField]
        private float m_AttackInterval = 0f; // 攻击间隔（秒）

        [SerializeField]
        private int m_BulletId = 0; // 发射的子弹的类型ID

        [SerializeField]
        private float m_BulletSpeed = 0f; // 发射的子弹的基础速度

        [SerializeField]
        private int m_BulletSoundId = 0; // 开火时播放的音效ID

        /// <summary>
        /// 武器数据的构造函数。
        /// </summary>
        public WeaponData(int entityId, int typeId, int ownerId, CampType ownerCamp)
            : base(entityId, typeId, ownerId, ownerCamp)
        {
            // 从数据表中获取武器的配置
            IDataTable<DRWeapon> dtWeapon = GameEntry.DataTable.GetDataTable<DRWeapon>();
            DRWeapon drWeapon = dtWeapon.GetDataRow(TypeId);
            if (drWeapon == null)
            {
                return;
            }

            // 从数据行中读取数据并填充自身属性
            m_Attack = drWeapon.Attack;
            m_AttackInterval = drWeapon.AttackInterval;
            m_BulletId = drWeapon.BulletId;
            m_BulletSpeed = drWeapon.BulletSpeed;
            m_BulletSoundId = drWeapon.BulletSoundId;
        }

        public int Attack { get { return m_Attack; } }
        public float AttackInterval { get { return m_AttackInterval; } }
        public int BulletId { get { return m_BulletId; } }
        public float BulletSpeed { get { return m_BulletSpeed; } }
        public int BulletSoundId { get { return m_BulletSoundId; } }
    }
}