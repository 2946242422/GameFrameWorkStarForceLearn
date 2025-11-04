using GameFramework;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace PuddingCat
{
    /// <summary>
    /// 实体逻辑的抽象基类。
    /// 游戏中所有具体的实体（如玩家、怪物、NPC）都应继承此类。
    /// 它处理了实体在Game Framework框架下的整个生命周期。
    /// </summary>
    public abstract class Entity : EntityLogic
    {
        // 用于存储该实体的数据
        [SerializeField] private EntityData m_EntityData = null;

        /// <summary>
        /// 获取实体编号。
        /// 注意：这里获取的是 Game Framework 框架层面的实体ID，
        /// 它与 m_EntityData.Id 在逻辑上应该是相同的。
        /// </summary>
        public int Id
        {
            get
            {
                // this.Entity 是从基类 EntityLogic 继承的属性，指向框架的实体容器
                return Entity.Id;
            }
        }

        /// <summary>
        /// 获取缓存的动画组件。
        /// </summary>
        public Animation CachedAnimation { get; private set; }

        // 预处理指令，用于处理不同Unity版本的API兼容性
#if UNITY_2017_3_OR_NEWER
        /// <summary>
        /// 实体初始化。
        /// 在实体第一次被创建（或从对象池取出）时调用。
        /// </summary>
        protected override void OnInit(object userData)
#else
        protected internal override void OnInit(object userData)
#endif
        {
            base.OnInit(userData);
            // 在初始化时获取并缓存Animation组件，提高后续访问性能
            CachedAnimation = GetComponent<Animation>();
        }

#if UNITY_2017_3_OR_NEWER
        /// <summary>
        /// 实体回收。
        /// 在实体被归还到对象池时调用。
        /// </summary>
        protected override void OnRecycle()
#else
        protected internal override void OnRecycle()
#endif
        {
            base.OnRecycle();
        }

#if UNITY_2017_3_OR_NEWER
        /// <summary>
        /// 实体显示。
        /// 在实体被激活、在场景中显示时调用。
        /// 这是设置实体初始状态的关键地方。
        /// </summary>
        /// <param name="userData">用户自定义数据，这里通常传入 EntityData 对象。</param>
        protected override void OnShow(object userData)
#else
        protected internal override void OnShow(object userData)
#endif
        {
            base.OnShow(userData);

            // 将传入的 userData 转换为 EntityData
            m_EntityData = userData as EntityData;
            if (m_EntityData == null)
            {
                Log.Error("Entity data is invalid.");
                return;
            }

            // 根据数据设置实体的各种属性
            Name = Utility.Text.Format("[Entity {0}]", Id); // 设置游戏对象在Hierarchy中的名字
            CachedTransform.localPosition = m_EntityData.Position; // 设置位置
            CachedTransform.localRotation = m_EntityData.Rotation; // 设置旋转
            CachedTransform.localScale = Vector3.one; // 默认缩放为1
        }

#if UNITY_2017_3_OR_NEWER
        /// <summary>
        /// 实体隐藏。
        /// 在实体被隐藏、从场景中移除（但未销毁）时调用。
        /// </summary>
        protected override void OnHide(bool isShutdown, object userData)
#else
        protected internal override void OnHide(bool isShutdown, object userData)
#endif
        {
            base.OnHide(isShutdown, userData);
        }

#if UNITY_2017_3_OR_NEWER
        /// <summary>
        /// 实体附加子实体。
        /// 当一个实体成为本实体的子实体时调用。
        /// </summary>
        protected override void OnAttached(EntityLogic childEntity, Transform parentTransform, object userData)
#else
        protected internal override void OnAttached(EntityLogic childEntity, Transform parentTransform, object userData)
#endif
        {
            base.OnAttached(childEntity, parentTransform, userData);
        }

#if UNITY_2017_3_OR_NEWER
        /// <summary>
        /// 实体解除子实体。
        /// 当一个子实体与本实体解除关系时调用。
        /// </summary>
        protected override void OnDetached(EntityLogic childEntity, object userData)
#else
        protected internal override void OnDetached(EntityLogic childEntity, object userData)
#endif
        {
            base.OnDetached(childEntity, userData);
        }

#if UNITY_2017_3_OR_NEWER
        /// <summary>
        /// 实体附加到父实体。
        /// 当本实体附加到另一个父实体上时调用。
        /// </summary>
        protected override void OnAttachTo(EntityLogic parentEntity, Transform parentTransform, object userData)
#else
        protected internal override void OnAttachTo(EntityLogic parentEntity, Transform parentTransform, object userData)
#endif
        {
            base.OnAttachTo(parentEntity, parentTransform, userData);
        }

#if UNITY_2017_3_OR_NEWER
        /// <summary>
        /// 实体从父实体解除。
        /// 当本实体从父实体上解除关系时调用。
        /// </summary>
        protected override void OnDetachFrom(EntityLogic parentEntity, object userData)
#else
        protected internal override void OnDetachFrom(EntityLogic parentEntity, object userData)
#endif
        {
            base.OnDetachFrom(parentEntity, userData);
        }

#if UNITY_2017_3_OR_NEWER
        /// <summary>
        /// 实体轮询。
        /// 在实体处于激活状态时，每帧都会调用。
        /// </summary>
        protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
#else
        protected internal override void OnUpdate(float elapseSeconds, float realElapseSeconds)
#endif
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);
        }
    }
}