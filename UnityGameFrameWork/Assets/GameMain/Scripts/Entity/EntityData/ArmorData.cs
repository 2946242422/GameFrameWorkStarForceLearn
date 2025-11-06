using GameFramework.DataTable;
using System;
using UnityEngine;

namespace PuddingCat
{
    /// <summary>
    /// 装甲的数据类。
    /// 继承自 AccessoryObjectData，代表可以装备在飞机上的部件。
    /// </summary>
    [Serializable]
    public class ArmorData : AccessoryObjectData
    {
        [SerializeField]
        private int m_MaxHP = 0; // 此装甲提供的最大生命值加成

        [SerializeField]
        private int m_Defense = 0; // 此装甲提供的防御力加成

        /// <summary>
        /// 装甲数据的构造函数。
        /// </summary>
        /// <param name="entityId">实体编号（装甲自身的ID）。</param>
        /// <param name="typeId">实体类型编号（用于查询数据表）。</param>
        /// <param name="ownerId">所有者的实体ID。</param>
        /// <param name="ownerCamp">所有者的阵营。</param>
        public ArmorData(int entityId, int typeId, int ownerId, CampType ownerCamp)
            : base(entityId, typeId, ownerId, ownerCamp) // 调用父类 AccessoryObjectData 的构造函数
        {
            // --- 数据驱动的初始化 ---

            // 1. 获取专门存储装甲配置的数据表
            IDataTable<DRArmor> dtArmor = GameEntry.DataTable.GetDataTable<DRArmor>();
            // 2. 使用传入的 TypeId 在数据表中查找对应的数据行
            DRArmor drArmor = dtArmor.GetDataRow(TypeId);
            if (drArmor == null)
            {
                // 如果找不到配置，则直接返回
                return;
            }

            // 3. 从查找到的数据行中读取数据，并填充自身的属性
            m_MaxHP = drArmor.MaxHP;
            m_Defense = drArmor.Defense;
        }

        /// <summary>
        /// 获取最大生命值加成。
        /// </summary>
        public int MaxHP
        {
            get
            {
                return m_MaxHP;
            }
        }

        /// <summary>
        /// 获取防御力加成。
        /// </summary>
        public int Defense
        {
            get
            {
                return m_Defense;
            }
        }
    }
}