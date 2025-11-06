using GameFramework.DataTable;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PuddingCat
{
    [Serializable]
    public abstract class AircraftData : TargetableObjectData
    {
        // --- 组合的部件数据 ---
        [SerializeField]
        private ThrusterData m_ThrusterData = null; // 推进器数据

        [SerializeField]
        private List<WeaponData> m_WeaponDatas = new List<WeaponData>(); // 武器数据列表

        [SerializeField]
        private List<ArmorData> m_ArmorDatas = new List<ArmorData>(); // 装甲数据列表

        // --- 由部件属性动态计算得出的飞机最终属性 ---
        [SerializeField]
        private int m_MaxHP = 0; // 最大生命值

        [SerializeField]
        private int m_Defense = 0; // 防御力

        // --- 飞机自身的基础属性 ---
        [SerializeField]
        private int m_DeadEffectId = 0; // 死亡特效ID

        [SerializeField]
        private int m_DeadSoundId = 0; // 死亡音效ID

        /// <summary>
        /// 飞机数据的构造函数。
        /// </summary>
        public AircraftData(int entityId, int typeId, CampType camp)
            : base(entityId, typeId, camp)
        {
            // 从数据表中读取飞机的基础配置
            IDataTable<DRAircraft> dtAircraft = GameEntry.DataTable.GetDataTable<DRAircraft>();
            DRAircraft drAircraft = dtAircraft.GetDataRow(TypeId);
            if (drAircraft == null)
            {
                return;
            }

            // 根据配置中的 ThrusterId，创建并组合推进器数据
            m_ThrusterData = new ThrusterData(GameEntry.Entity.GenerateSerialId(), drAircraft.ThrusterId, Id, Camp);

            // 根据配置，遍历并创建所有初始武器
            for (int index = 0, weaponId = 0; (weaponId = drAircraft.GetWeaponIdAt(index)) > 0; index++)
            {
                AttachWeaponData(new WeaponData(GameEntry.Entity.GenerateSerialId(), weaponId, Id, Camp));
            }

            // 根据配置，遍历并创建所有初始装甲
            for (int index = 0, armorId = 0; (armorId = drAircraft.GetArmorIdAt(index)) > 0; index++)
            {
                AttachArmorData(new ArmorData(GameEntry.Entity.GenerateSerialId(), armorId, Id, Camp));
            }

            // 读取飞机自身的基础配置
            m_DeadEffectId = drAircraft.DeadEffectId;
            m_DeadSoundId = drAircraft.DeadSoundId;

            // 初始化当前生命等于最大生命
            HP = m_MaxHP;
        }

        // --- 公开的属性访问器 ---

        public override int MaxHP { get { return m_MaxHP; } }
        public int Defense { get { return m_Defense; } }
        /// <summary>
        /// 获取速度（代理属性，实际返回的是推进器的速度）。
        /// </summary>
        public float Speed { get { return m_ThrusterData.Speed; } }
        public int DeadEffectId { get { return m_DeadEffectId; } }
        public int DeadSoundId { get { return m_DeadSoundId; } }

        // --- 部件管理方法 ---

        public ThrusterData GetThrusterData() { return m_ThrusterData; }
        public List<WeaponData> GetAllWeaponDatas() { return m_WeaponDatas; }

        public void AttachWeaponData(WeaponData weaponData)
        {
            if (weaponData == null || m_WeaponDatas.Contains(weaponData)) return;
            m_WeaponDatas.Add(weaponData);
        }

        public void DetachWeaponData(WeaponData weaponData)
        {
            if (weaponData == null) return;
            m_WeaponDatas.Remove(weaponData);
        }

        public List<ArmorData> GetAllArmorDatas() { return m_ArmorDatas; }

        public void AttachArmorData(ArmorData armorData)
        {
            if (armorData == null || m_ArmorDatas.Contains(armorData)) return;
            m_ArmorDatas.Add(armorData);
            RefreshData(); // 装备变化，需要重新计算飞机的最终属性
        }

        public void DetachArmorData(ArmorData armorData)
        {
            if (armorData == null) return;
            m_ArmorDatas.Remove(armorData);
            RefreshData(); // 装备变化，需要重新计算飞-机的最终属性
        }

        /// <summary>
        /// 刷新飞机数据。当装备发生变化时调用，以重新计算综合属性。
        /// </summary>
        private void RefreshData()
        {
            // 重置属性
            m_MaxHP = 0;
            m_Defense = 0;
            // 遍历所有已装备的装甲，累加它们的属性
            for (int i = 0; i < m_ArmorDatas.Count; i++)
            {
                m_MaxHP += m_ArmorDatas[i].MaxHP;
                m_Defense += m_ArmorDatas[i].Defense;
            }

            // 确保当前生命值不会超过新的最大生命值
            if (HP > m_MaxHP)
            {
                HP = m_MaxHP;
            }
        }
    }
}