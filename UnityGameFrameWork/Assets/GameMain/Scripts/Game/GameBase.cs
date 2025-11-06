using System.Collections;
using System.Collections.Generic;
using GameFramework.Event;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace PuddingCat
{
    /// <summary>
    /// 游戏玩法的抽象基类。
    /// 定义了所有具体游戏模式（如生存模式、竞速模式）都必须具备的通用逻辑和接口。
    /// </summary>
    public abstract class GameBase
    {
        /// <summary>
        /// 获取当前游戏模式的类型（由子类实现）。
        /// </summary>
        public abstract GameMode GameMode
        {
            get;
        }

        /// <summary>
        /// 获取场景中的可滚动背景。
        /// </summary>
        protected ScrollableBackground SceneBackground
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取或设置游戏是否结束。
        /// </summary>
        public bool GameOver
        {
            get;
            protected set;
        }

        // 对玩家飞机的引用
        private MyAircraft m_MyAircraft = null;

        /// <summary>
        /// 游戏玩法初始化。在进入具体玩法时由 ProcedureMain 调用。
        /// </summary>
        public virtual void Initialize()
        {
            // 订阅“显示实体成功/失败”事件，用于获取对玩家飞机的引用
            GameEntry.Event.Subscribe(ShowEntitySuccessEventArgs.EventId, OnShowEntitySuccess);
            GameEntry.Event.Subscribe(ShowEntityFailureEventArgs.EventId, OnShowEntityFailure);

            // 查找并保存场景背景对象的引用
            SceneBackground = Object.FindObjectOfType<ScrollableBackground>();
            if (SceneBackground == null)
            {
                Log.Warning("Can not find scene background.");
                return;
            }

            // 给背景的可见边界添加一个“超出边界即隐藏”的脚本
            SceneBackground.VisibleBoundary.gameObject.GetOrAddComponent<HideByBoundary>();
            
            // 请求实体组件生成“我的飞机”
            GameEntry.Entity.ShowMyAircraft(new MyAircraftData(GameEntry.Entity.GenerateSerialId(), 10000)
            {
                Name = "My Aircraft",
                Position = Vector3.zero,
            });

            // 初始化游戏状态
            GameOver = false;
            m_MyAircraft = null;
        }

        /// <summary>
        /// 游戏玩法关闭。在退出具体玩法时由 ProcedureMain 调用。
        /// </summary>
        public virtual void Shutdown()
        {
            // 取消订阅事件，防止内存泄漏
            GameEntry.Event.Unsubscribe(ShowEntitySuccessEventArgs.EventId, OnShowEntitySuccess);
            GameEntry.Event.Unsubscribe(ShowEntityFailureEventArgs.EventId, OnShowEntityFailure);
        }

        /// <summary>
        /// 游戏玩法逻辑的轮询（每帧调用）。
        /// </summary>
        public virtual void Update(float elapseSeconds, float realElapseSeconds)
        {
            // 检查玩家飞机是否存在且已经死亡
            if (m_MyAircraft != null && m_MyAircraft.IsDead)
            {
                // 如果条件满足，则将游戏结束标志位置为 true
                GameOver = true;
                return;
            }
        }

        /// <summary>
        /// 当“显示实体成功”事件被触发时，此回调被调用。
        /// </summary>
        protected virtual void OnShowEntitySuccess(object sender, GameEventArgs e)
        {
            ShowEntitySuccessEventArgs ne = (ShowEntitySuccessEventArgs)e;
            // 检查成功显示的实体是否是“我的飞机”
            if (ne.EntityLogicType == typeof(MyAircraft))
            {
                // 如果是，则获取并保存对飞机逻辑组件的引用
                m_MyAircraft = (MyAircraft)ne.Entity.Logic;
            }
        }

        /// <summary>
        /// 当“显示实体失败”事件被触发时，此回调被调用。
        /// </summary>
        protected virtual void OnShowEntityFailure(object sender, GameEventArgs e)
        {
            ShowEntityFailureEventArgs ne = (ShowEntityFailureEventArgs)e;
            Log.Warning("Show entity failure with error message '{0}'.", ne.ErrorMessage);
        }
    }
}