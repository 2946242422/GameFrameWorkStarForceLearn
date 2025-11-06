using System;
using UnityEngine;

namespace PuddingCat
{
    /// <summary>
    /// 可作为攻击目标的对象的数据基类。
    /// 继承自 EntityData，并增加了阵营、生命值等战斗相关属性。
    /// </summary>
    [Serializable] // 允许这个类的实例被Unity序列化
    public abstract class TargetableObjectData : EntityData
    {
        [SerializeField]
        private CampType m_Camp = CampType.Unknown; // 对象的阵营

        [SerializeField]
        private int m_HP = 0; // 对象的当前生命值

        /// <summary>
        /// 构造函数。
        /// </summary>
        /// <param name="entityId">实体编号。</param>
        /// <param name="typeId">实体类型编号。</param>
        /// <param name="camp">实体所属阵营。</param>
        public TargetableObjectData(int entityId, int typeId, CampType camp)
            : base(entityId, typeId) // 调用基类 EntityData 的构造函数
        {
            m_Camp = camp;
            m_HP = 0; // 当前生命值在创建时通常由后续逻辑（如读取配置）设置
        }

        /// <summary>
        /// 获取角色阵营。
        /// </summary>
        public CampType Camp
        {
            get
            {
                return m_Camp;
            }
        }

        /// <summary>
        /// 获取或设置当前生命值。
        /// </summary>
        public int HP
        {
            get
            {
                return m_HP;
            }
            set
            {
                m_HP = value;
            }
        }

        /// <summary>
        /// 获取最大生命值。
        /// 这是一个抽象属性，意味着任何继承此类的子类都必须提供自己的实现。
        /// </summary>
        public abstract int MaxHP
        {
            get;
        }

        /// <summary>
        /// 获取当前生命值百分比（范围 0 到 1）。
        /// 这是一个计算属性，用于方便地更新UI血条等。
        /// </summary>
        public float HPRatio
        {
            get
            {
                // 进行安全检查，防止 MaxHP 为 0 时出现除零错误
                return MaxHP > 0 ? (float)HP / MaxHP : 0f;
            }
        }
    }
}