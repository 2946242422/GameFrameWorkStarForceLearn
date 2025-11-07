using System;
using UnityEngine;

namespace PuddingCat
{
    [Serializable]
    public class EffectData : EntityData
    {
        [SerializeField]
        private float m_KeepTime = 0f; // 特效的持续时间（秒）

        /// <summary>
        /// 特效数据的构造函数。
        /// </summary>
        public EffectData(int entityId, int typeId)
            : base(entityId, typeId)
        {
            // 可以在这里从数据表读取持续时间，或者像这样硬编码一个默认值
            m_KeepTime = 3f;
        }

        /// <summary>
        /// 获取特效持续时间。
        /// </summary>
        public float KeepTime
        {
            get
            {
                return m_KeepTime;
            }
        }
    }
}