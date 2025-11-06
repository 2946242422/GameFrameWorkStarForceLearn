using GameFramework.Event;
using UnityGameFramework.Runtime;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace PuddingCat
{
    /// <summary>
    /// 主菜单流程。
    /// 负责管理玩家在主菜单界面时的游戏状态。
    /// </summary>
    public class ProcedureMenu : ProcedureBase
    {
        private bool m_StartGame = false; // "开始游戏"的标志位，由 MenuForm 来触发改变
        private MenuForm m_MenuForm = null; // 对主菜单UI的引用

        public override bool UseNativeDialog
        {
            get
            {
                return false; // 进入主菜单后，使用游戏内UI对话框
            }
        }

        /// <summary>
        /// 这是提供给 MenuForm 调用的公共接口。
        /// 当玩家点击开始按钮时，MenuForm会调用此方法。
        /// </summary>
        public void StartGame()
        {
            m_StartGame = true;
        }

        /// <summary>
        /// 当进入此流程时调用。
        /// </summary>
        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);
            // 订阅UI打开成功的事件
            GameEntry.Event.Subscribe(OpenUIFormSuccessEventArgs.EventId, OnOpenUIFormSuccess);
            
            m_StartGame = false; // 重置标志位，防止从其他场景返回时立即开始游戏
            
            // 打开主菜单界面，并将本流程实例(this)作为用户数据传递过去
            GameEntry.UI.OpenUIForm(UIFormId.MenuForm, this);
        }

        /// <summary>
        /// 当离开此流程时调用。
        /// </summary>
        protected override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);
            
            // 取消订阅事件
            GameEntry.Event.Unsubscribe(OpenUIFormSuccessEventArgs.EventId, OnOpenUIFormSuccess);

            // 如果 MenuForm 仍然打开着，则关闭它
            if (m_MenuForm != null)
            {
                m_MenuForm.Close(isShutdown);
                m_MenuForm = null;
            }
        }

        /// <summary>
        /// 流程轮询（每帧调用）。
        /// </summary>
        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            // 持续检查是否收到了“开始游戏”的信号
            if (m_StartGame)
            {
                // 设置下一个场景的ID和游戏模式，这些数据将传递给 ProcedureChangeScene
                procedureOwner.SetData<VarInt32>("NextSceneId", GameEntry.Config.GetInt("Scene.Main"));
                procedureOwner.SetData<VarByte>("GameMode", (byte)GameMode.Survival);
                
                // 切换到“切换场景”流程
                ChangeState<ProcedureChangeScene>(procedureOwner);
            }
        }

        /// <summary>
        /// 当UI界面成功打开时调用的回调函数。
        /// </summary>
        private void OnOpenUIFormSuccess(object sender, GameEventArgs e)
        {
            OpenUIFormSuccessEventArgs ne = (OpenUIFormSuccessEventArgs)e;
            // 确保这个事件是由本流程打开UI的请求触发的
            if (ne.UserData != this)
            {
                return;
            }

            // 获取并保存对 MenuForm 逻辑组件的引用
            m_MenuForm = (MenuForm)ne.UIForm.Logic;
        }
    }
}