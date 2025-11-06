using GameFramework.DataTable;
using System;
using UnityEngine;

namespace PuddingCat
{
    /// <summary>
    /// 推进器的数据类。
    /// 这是一个具体的数据类，继承自 AccessoryObjectData。
    /// </summary>
    [Serializable]
    public class ThrusterData : AccessoryObjectData
    {
        [SerializeField]
        private float m_Speed = 0f; // 推进器的速度，这是它独有的属性

        /// <summary>
        /// 构造函数。
        /// </summary>
        /// <param name="entityId">实体编号（推进器自身的ID）。</param>
        /// <param name="typeId">实体类型编号（用于查询数据表）。</param>
        /// <param name="ownerId">所有者的实体ID。</param>
        /// <param name="ownerCamp">所有者的阵营。</param>
        public ThrusterData(int entityId, int typeId, int ownerId, CampType ownerCamp)
            : base(entityId, typeId, ownerId, ownerCamp) // 首先调用父类 AccessoryObjectData 的构造函数
        {
            // --- 数据驱动的核心逻辑 ---

            // 1. 获取专门存储推进器配置的数据表
            IDataTable<DRThruster> dtThruster = GameEntry.DataTable.GetDataTable<DRThruster>();
            // 2. 使用传入的 TypeId 在数据表中查找对应的数据行
            DRThruster drThruster = dtThruster.GetDataRow(TypeId);
            if (drThruster == null)
            {
                // 如果找不到对应配置，则直接返回，防止后续错误
                return;
            }

            // 3. 从查找到的数据行中读取 Speed 字段的值，并赋给自身的 m_Speed 变量
            m_Speed = drThruster.Speed;
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
    }
}