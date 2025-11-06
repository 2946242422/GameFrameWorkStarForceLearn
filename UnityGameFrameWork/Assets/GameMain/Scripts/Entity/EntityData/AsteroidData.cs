using GameFramework.DataTable;
using System;
using UnityEngine;

namespace PuddingCat
{
    [Serializable]
    public class AsteroidData : TargetableObjectData
    {
        [SerializeField]
        private int m_MaxHP = 0; // 小行星的最大生命值

        [SerializeField]
        private int m_Attack = 0; // 撞击玩家时造成的伤害

        [SerializeField]
        private float m_Speed = 0f; // 移动速度

        [SerializeField]
        private float m_AngularSpeed = 0f; // 旋转角速度

        [SerializeField]
        private int m_DeadEffectId = 0; // 死亡时播放的特效ID

        [SerializeField]
        private int m_DeadSoundId = 0; // 死亡时播放的音效ID

        /// <summary>
        /// 小行星数据的构造函数。
        /// </summary>
        public AsteroidData(int entityId, int typeId)
            : base(entityId, typeId, CampType.Neutral) // 小行星属于中立阵营
        {
            // 从数据表中获取小行星的配置
            IDataTable<DRAsteroid> dtAsteroid = GameEntry.DataTable.GetDataTable<DRAsteroid>();
            DRAsteroid drAsteroid = dtAsteroid.GetDataRow(TypeId);
            if (drAsteroid == null)
            {
                return;
            }

            // 从数据行中读取数据并填充自身属性
            HP = m_MaxHP = drAsteroid.MaxHP; // 初始化当前生命等于最大生命
            m_Attack = drAsteroid.Attack;
            m_Speed = drAsteroid.Speed;
            m_AngularSpeed = drAsteroid.AngularSpeed;
            m_DeadEffectId = drAsteroid.DeadEffectId;
            m_DeadSoundId = drAsteroid.DeadSoundId;
        }

        /// <summary>
        /// 获取最大生命值（实现父类的抽象属性）。
        /// </summary>
        public override int MaxHP
        {
            get
            {
                return m_MaxHP;
            }
        }

        /// <summary>
        /// 获取攻击力。
        /// </summary>
        public int Attack
        {
            get
            {
                return m_Attack;
            }
        }

        /// <summary>
        /// 获取速度。
        /// </summary>
        public float Speed
        {
            get
            {
                return m_Speed;
            }
        }

        /// <summary>
        /// 获取角速度。
        /// </summary>
        public float AngularSpeed
        {
            get
            {
                return m_AngularSpeed;
            }
        }

        /// <summary>
        /// 获取死亡特效ID。
        /// </summary>
        public int DeadEffectId
        {
            get
            {
                return m_DeadEffectId;
            }
        }

        /// <summary>
        /// 获取死亡音效ID。
        /// </summary>
        public int DeadSoundId
        {
            get
            {
                return m_DeadSoundId;
            }
        }
    }
}