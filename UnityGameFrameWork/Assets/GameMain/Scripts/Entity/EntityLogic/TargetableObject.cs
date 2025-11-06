using UnityEngine;
using UnityGameFramework.Runtime;

namespace PuddingCat
{
    /// <summary>
    /// 可作为攻击目标的实体逻辑基类。
    /// 所有飞机、小行星等可被攻击的单位都应继承此类。
    /// </summary>
    public abstract class TargetableObject : Entity
    {
        [SerializeField]
        private TargetableObjectData m_TargetableObjectData = null;// 持有的数据对象
        /// <summary>
        /// 获取实体是否已死亡。
        /// </summary>
        public bool IsDead
        {
            get
            {
                // 通过检查数据层的HP来判断
                return m_TargetableObjectData.HP <= 0;
            }
        }

        /// <summary>
        /// 获取碰撞的冲击数据（由子类实现）。
        /// </summary>
        public abstract ImpactData GetImpactData();

        /// <summary>
        /// 应用伤害。这是所有实体受到伤害的统一入口。
        /// </summary>
        /// <param name="attacker">攻击者实体。</param>
        /// <param name="damageHP">伤害数值。</param>
        public void ApplyDamage(Entity attacker, int damageHP)
        {
            float fromHPRatio = m_TargetableObjectData.HPRatio; // 记录受伤前的血量百分比
            m_TargetableObjectData.HP -= damageHP; // 扣血
            float toHPRatio = m_TargetableObjectData.HPRatio; // 记录受伤后的血量百分比
        
            // 如果血量真的减少了，就通知HPBar组件显示血条变化
            if (fromHPRatio > toHPRatio)
            {
                GameEntry.HPBar.ShowHPBar(this, fromHPRatio, toHPRatio);
            }

            // 如果生命值小于等于0，则处理死亡逻辑
            if (m_TargetableObjectData.HP <= 0)
            {
                OnDead(attacker);
            }
        }

#if UNITY_2017_3_OR_NEWER
        protected override void OnInit(object userData)
#else
        protected internal override void OnInit(object userData)
#endif
        {
            base.OnInit(userData);
            gameObject.SetLayerRecursively(Constant.Layer.TargetableObjectLayerId);
        }

#if UNITY_2017_3_OR_NEWER
        protected override void OnShow(object userData)
#else
        protected internal override void OnShow(object userData)
#endif
        {
            base.OnShow(userData);

            m_TargetableObjectData = userData as TargetableObjectData;
            if (m_TargetableObjectData == null)
            {
                Log.Error("Targetable object data is invalid.");
                return;
            }
        }
        /// <summary>
        /// 当实体死亡时调用。
        /// </summary>
        protected virtual void OnDead(Entity attacker)
        {
            // 默认的死亡行为是隐藏自身（回收到对象池）
            GameEntry.Entity.HideEntity(this);
        }

        /// <summary>
        /// 当进入触发器时由Unity物理引擎自动调用。
        /// </summary>
        private void OnTriggerEnter(Collider other)
        {
            Entity entity = other.gameObject.GetComponent<Entity>();
            if (entity == null)
            {
                return;
            }

            // 防重复处理：让ID较小的一方来处理碰撞逻辑，避免同一次碰撞被双方各处理一次
            if (entity is TargetableObject && entity.Id >= Id)
            {
                return;
            }

            // 将具体的碰撞效果计算委托给AI工具类
            AIUtility.PerformCollision(this, entity);
        }
    }
}
