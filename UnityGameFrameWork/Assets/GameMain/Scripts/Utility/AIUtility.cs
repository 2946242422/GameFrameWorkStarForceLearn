using System;
using System.Collections.Generic;
using System.Runtime.InteropServices; // 用于 StructLayout
using UnityEngine;
using UnityGameFramework.Runtime;

namespace PuddingCat
{
    /// <summary>
    /// AI 工具类。
    /// 包含所有通用的AI、阵营关系和战斗计算逻辑。
    /// </summary>
    public static class AIUtility
    {
        // 字典1: 存储“阵营对”到“关系”的映射，是阵营关系的核心速查表。
        private static Dictionary<CampPair, RelationType> s_CampPairToRelation = new Dictionary<CampPair, RelationType>();
        // 字典2: 缓存“某个阵营的特定关系有哪些阵营”的查询结果，用于优化性能。
        private static Dictionary<KeyValuePair<CampType, RelationType>, CampType[]> s_CampAndRelationToCamps = new Dictionary<KeyValuePair<CampType, RelationType>, CampType[]>();

        /// <summary>
        /// 静态构造函数，在第一次访问该类时自动执行一次。
        /// </summary>
        static AIUtility()
        {
            // 在这里硬编码定义所有阵营之间的关系
            s_CampPairToRelation.Add(new CampPair(CampType.Player, CampType.Player), RelationType.Friendly);
            s_CampPairToRelation.Add(new CampPair(CampType.Player, CampType.Enemy), RelationType.Hostile);
            s_CampPairToRelation.Add(new CampPair(CampType.Player, CampType.Neutral), RelationType.Neutral);
            s_CampPairToRelation.Add(new CampPair(CampType.Player, CampType.Player2), RelationType.Hostile);
                      s_CampPairToRelation.Add(new CampPair(CampType.Player, CampType.Enemy2), RelationType.Hostile);
            s_CampPairToRelation.Add(new CampPair(CampType.Player, CampType.Neutral2), RelationType.Neutral);

            s_CampPairToRelation.Add(new CampPair(CampType.Enemy, CampType.Enemy), RelationType.Friendly);
            s_CampPairToRelation.Add(new CampPair(CampType.Enemy, CampType.Neutral), RelationType.Neutral);
            s_CampPairToRelation.Add(new CampPair(CampType.Enemy, CampType.Player2), RelationType.Hostile);
            s_CampPairToRelation.Add(new CampPair(CampType.Enemy, CampType.Enemy2), RelationType.Hostile);
            s_CampPairToRelation.Add(new CampPair(CampType.Enemy, CampType.Neutral2), RelationType.Neutral);

            s_CampPairToRelation.Add(new CampPair(CampType.Neutral, CampType.Neutral), RelationType.Neutral);
            s_CampPairToRelation.Add(new CampPair(CampType.Neutral, CampType.Player2), RelationType.Neutral);
            s_CampPairToRelation.Add(new CampPair(CampType.Neutral, CampType.Enemy2), RelationType.Neutral);
            s_CampPairToRelation.Add(new CampPair(CampType.Neutral, CampType.Neutral2), RelationType.Hostile);

            s_CampPairToRelation.Add(new CampPair(CampType.Player2, CampType.Player2), RelationType.Friendly);
            s_CampPairToRelation.Add(new CampPair(CampType.Player2, CampType.Enemy2), RelationType.Hostile);
            s_CampPairToRelation.Add(new CampPair(CampType.Player2, CampType.Neutral2), RelationType.Neutral);

            s_CampPairToRelation.Add(new CampPair(CampType.Enemy2, CampType.Enemy2), RelationType.Friendly);
            s_CampPairToRelation.Add(new CampPair(CampType.Enemy2, CampType.Neutral2), RelationType.Neutral);

            s_CampPairToRelation.Add(new CampPair(CampType.Neutral2, CampType.Neutral2), RelationType.Neutral);
        }

        /// <summary>
        /// 获取两个阵营之间的关系。
        /// </summary>
        public static RelationType GetRelation(CampType first, CampType second)
        {
            // 技巧：通过排序，确保查询时 first 总是小于 second，
            // 这样在定义关系时只需要定义一半 (如 Player vs Enemy)，而无需再定义 Enemy vs Player。
            if (first > second)
            {
                CampType temp = first;
                first = second;
                second = temp;
            }

            RelationType relationType;
            // 高效地从字典中查找关系
            if (s_CampPairToRelation.TryGetValue(new CampPair(first, second), out relationType))
            {
                return relationType;
            }

            Log.Warning("Unknown relation between '{0}' and '{1}'.", first.ToString(), second.ToString());
            return RelationType.Unknown;
        }

        /// <summary>
        /// 获取和指定阵营具有特定关系的所有阵营。
        /// </summary>
        public static CampType[] GetCamps(CampType camp, RelationType relation)
        {
            KeyValuePair<CampType, RelationType> key = new KeyValuePair<CampType, RelationType>(camp, relation);
            CampType[] result = null;
            // 1. 先尝试从缓存中获取结果
            if (s_CampAndRelationToCamps.TryGetValue(key, out result))
            {
                return result;
            }

            // 2. 如果缓存中没有，则进行计算
            // TODO: GC Alloc (此处的new List会产生GC，可优化)
            List<CampType> camps = new List<CampType>();
            Array campTypes = Enum.GetValues(typeof(CampType)); // 获取所有阵营类型
            for (int i = 0; i < campTypes.Length; i++)
            {
                CampType campType = (CampType)campTypes.GetValue(i);
                // 逐一检查关系是否匹配
                if (GetRelation(camp, campType) == relation)
                {
                    camps.Add(campType);
                }
            }

            // 3. 将计算结果存入缓存，以备下次使用
            // TODO: GC Alloc (ToArray()会产生GC，可优化)
            result = camps.ToArray();
            s_CampAndRelationToCamps[key] = result;

            return result;
        }

        /// <summary>
        /// 获取两个实体间的距离。
        /// </summary>
        public static float GetDistance(Entity fromEntity, Entity toEntity)
        {
            Transform fromTransform = fromEntity.CachedTransform;
            Transform toTransform = toEntity.CachedTransform;
            return (toTransform.position - fromTransform.position).magnitude;
        }

        /// <summary>
        /// 执行碰撞逻辑处理。这是所有碰撞事件的中央分发器。
        /// </summary>
        public static void PerformCollision(TargetableObject entity, Entity other)
        {
            if (entity == null || other == null) return;

            // --- 分支1: 目标 vs 目标 (如 飞机撞小行星) ---
            TargetableObject target = other as TargetableObject;
            if (target != null)
            {
                ImpactData entityImpactData = entity.GetImpactData(); // 获取自身的战斗数据
                ImpactData targetImpactData = target.GetImpactData(); // 获取对方的战斗数据
                // 如果是友军，则不处理碰撞
                if (GetRelation(entityImpactData.Camp, targetImpactData.Camp) == RelationType.Friendly) return;

                // 根据伤害公式，计算互相造成的伤害
                int entityDamageHP = CalcDamageHP(targetImpactData.Attack, entityImpactData.Defense);
                int targetDamageHP = CalcDamageHP(entityImpactData.Attack, targetImpactData.Defense);

                // 特殊逻辑：处理“同归于尽”的情况，确保能同时死亡
                int delta = Mathf.Min(entityImpactData.HP - entityDamageHP, targetImpactData.HP - targetDamageHP);
                if (delta > 0)
                {
                    entityDamageHP += delta;
                    targetDamageHP += delta;
                }

                // 分别对双方应用伤害
                entity.ApplyDamage(target, entityDamageHP);
                target.ApplyDamage(entity, targetDamageHP);
                return;
            }

            // --- 分支2: 目标 vs 子弹 ---
            Bullet bullet = other as Bullet;
            if (bullet != null)
            {
                ImpactData entityImpactData = entity.GetImpactData(); // 获取目标的战斗数据
                ImpactData bulletImpactData = bullet.GetImpactData(); // 获取子弹的战斗数据
                // 如果是友军子弹，则不处理
                if (GetRelation(entityImpactData.Camp, bulletImpactData.Camp) == RelationType.Friendly) return;
                
                // 计算子弹造成的伤害
                int entityDamageHP = CalcDamageHP(bulletImpactData.Attack, entityImpactData.Defense);

                // 对目标应用伤害
                entity.ApplyDamage(bullet, entityDamageHP);
                // 子弹击中后消失
                GameEntry.Entity.HideEntity(bullet);
                return;
            }
        }

        /// <summary>
        /// 伤害计算公式。
        /// </summary>
        private static int CalcDamageHP(int attack, int defense)
        {
            if (attack <= 0) return 0;
            if (defense < 0) defense = 0;

            // 公式: 伤害 = 攻击² / (攻击 + 防御)
            // 这是一个常见的伤害公式，特点是防御的收益会递减。
            return attack * attack / (attack + defense);
        }

        /// <summary>
        /// 私有结构体，用作阵营关系字典的Key。
        /// </summary>
        [StructLayout(LayoutKind.Auto)]
        private struct CampPair
        {
            private readonly CampType m_First;
            private readonly CampType m_Second;

            public CampPair(CampType first, CampType second)
            {
                m_First = first;
                m_Second = second;
            }

            public CampType First { get { return m_First; } }
            public CampType Second { get { return m_Second; } }
        }
    }
}