using System;
using UnityEngine;

namespace PuddingCat
{
    /// <summary>
    /// 附属物的数据基类。
    /// 适用于所有需要归属于另一个实体的对象，如武器、装甲、子弹等。
    /// </summary>
    [Serializable] // 允许这个类的实例被Unity序列化
    public abstract class AccessoryObjectData : EntityData
    {
        [SerializeField]
        private int m_OwnerId = 0; // 附属物的所有者的实体ID

        [SerializeField]
        private CampType m_OwnerCamp = CampType.Unknown; // 附属物的所有者的阵营

        /// <summary>
        /// 构造函数。
        /// </summary>
        /// <param name="entityId">实体编号（附属物自身的ID）。</param>
        /// <param name="typeId">实体类型编号。</param>
        /// <param name="ownerId">所有者的实体ID。</param>
        /// <param name="ownerCamp">所有者的阵营。</param>
        public AccessoryObjectData(int entityId, int typeId, int ownerId, CampType ownerCamp)
            : base(entityId, typeId) // 调用基类 EntityData 的构造函数
        {
            m_OwnerId = ownerId;
            m_OwnerCamp = ownerCamp;
        }

        /// <summary>
        /// 获取拥有者编号。
        /// </summary>
        public int OwnerId
        {
            get
            {
                return m_OwnerId;
            }
        }

        /// <summary>
        /// 获取拥有者阵营。
        /// 冗余存储拥有者的阵营是为了在进行伤害计算等逻辑时，
        /// 无需再次通过OwnerId查找拥有者实体，从而提高性能。
        /// </summary>
        public CampType OwnerCamp
        {
            get
            {
                return m_OwnerCamp;
            }
        }
    }
}