using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace PuddingCat
{
    /// <summary>
    /// 主游戏流程。
    /// 负责根据不同的游戏模式，创建并管理对应的游戏玩法逻辑实例。
    /// </summary>
    public class ProcedureMain : ProcedureBase
    {
        private const float GameOverDelayedSeconds = 2f; // 游戏结束后，延迟多久返回主菜单
        
        // 一个字典，用于存储不同游戏模式对应的玩法逻辑实例
        private readonly Dictionary<GameMode, GameBase> m_Games = new Dictionary<GameMode, GameBase>();
        private GameBase m_CurrentGame = null; // 当前正在运行的玩法逻辑实例

        private bool m_GotoMenu = false; // 是否需要返回主菜单的标志位
        private float m_GotoMenuDelaySeconds = 0f; // 返回主菜单前的延迟计时器

        public override bool UseNativeDialog
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// 提供给外部（如UI）调用的方法，用于触发返回主菜单的逻辑。
        /// </summary>
        public void GotoMenu()
        {
            m_GotoMenu = true;
        }

        /// <summary>
        /// 当流程初始化时调用（仅在第一次进入此流程时调用一次）。
        /// </summary>
        protected override void OnInit(ProcedureOwner procedureOwner)
        {
            base.OnInit(procedureOwner);

            // 在这里，我们将所有支持的游戏模式逻辑都实例化并添加到字典中
            m_Games.Add(GameMode.Survival, new SurvivalGame());
            // 如果未来有新的模式，如 RaceGame，只需在这里添加一行：
            // m_Games.Add(GameMode.Race, new RaceGame());
        }

        /// <summary>
        /// 当流程被销毁时调用。
        /// </summary>
        protected override void OnDestroy(ProcedureOwner procedureOwner)
        {
            base.OnDestroy(procedureOwner);

            m_Games.Clear();
        }

        /// <summary>
        /// 当进入此流程时调用（每次进入都会调用）。
        /// </summary>
        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            m_GotoMenu = false; // 重置返回菜单标志位
            
            // 从流程状态机中获取之前流程传递过来的“游戏模式”数据
            GameMode gameMode = (GameMode)procedureOwner.GetData<VarByte>("GameMode").Value;
            // 从字典中根据游戏模式，找出对应的玩法逻辑实例
            m_CurrentGame = m_Games[gameMode];
            // 初始化选中的玩法逻辑
            m_CurrentGame.Initialize();
        }

        /// <summary>
        /// 当离开此流程时调用。
        /// </summary>
        protected override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            // 在离开流程前，确保当前的游戏玩法逻辑被正确关闭
            if (m_CurrentGame != null)
            {
                m_CurrentGame.Shutdown();
                m_CurrentGame = null;
            }

            base.OnLeave(procedureOwner, isShutdown);
        }

        /// <summary>
        /// 流程的轮询方法（每帧调用）。
        /// </summary>
        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            // 如果当前游戏正在进行中且尚未结束
            if (m_CurrentGame != null && !m_CurrentGame.GameOver)
            {
                // 则持续调用当前玩法逻辑的Update方法
                m_CurrentGame.Update(elapseSeconds, realElapseSeconds);
                return; // 后续逻辑不执行
            }

            // --- 如果游戏已经结束 (m_CurrentGame.GameOver 为 true) ---

            // 如果还未触发“返回菜单”逻辑，则在这里触发
            if (!m_GotoMenu)
            {
                m_GotoMenu = true;
                m_GotoMenuDelaySeconds = 0; // 重置延迟计时器
            }

            // 开始延迟计时
            m_GotoMenuDelaySeconds += elapseSeconds;
            // 当延迟时间达到预设值后
            if (m_GotoMenuDelaySeconds >= GameOverDelayedSeconds)
            {
                // 设置下一个场景为主菜单场景
                procedureOwner.SetData<VarInt32>("NextSceneId", GameEntry.Config.GetInt("Scene.Menu"));
                // 切换到“切换场景”流程
                ChangeState<ProcedureChangeScene>(procedureOwner);
            }
        }
    }
}