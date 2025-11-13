using UnityEngine;
using UnityGameFramework.Runtime;

namespace PuddingCat
{
    /// <summary>
    /// 主菜单界面的逻辑脚本。
    /// </summary>
    public class MenuForm : UGuiForm
    {
        [SerializeField]
        private GameObject m_QuitButton = null; // 对“退出按钮”的引用
        
        // 对打开此界面的流程的引用
        private ProcedureMenu m_ProcedureMenu = null;

        /// <summary>
        /// 当“开始”按钮被点击时调用。
        /// </summary>
        public void OnStartButtonClick()
        {
            // 通过持有的引用，通知主菜单流程：“我要开始游戏了！”
            Log.Info("StartGame");
            m_ProcedureMenu.StartGame();
        }

        /// <summary>
        /// 当“设置”按钮被点击时调用。
        /// </summary>
        public void OnSettingButtonClick()
        {
            // 打开设置界面
            GameEntry.UI.OpenUIForm(UIFormId.SettingForm);
        }

        /// <summary>
        /// 当“关于”按钮被点击时调用。
        /// </summary>
        public void OnAboutButtonClick()
        {
            // 打开关于界面
            GameEntry.UI.OpenUIForm(UIFormId.AboutForm);
        }

        /// <summary>
        /// 当“退出”按钮被点击时调用。
        /// </summary>
        public void OnQuitButtonClick()
        {
            // 打开一个确认对话框
            GameEntry.UI.OpenDialog(new DialogParams()
            {
                Mode = 2, // 两个按钮（确认/取消）
                Title = GameEntry.Localization.GetString("AskQuitGame.Title"), // 标题
                Message = GameEntry.Localization.GetString("AskQuitGame.Message"), // 消息
                // 定义点击“确认”按钮后要执行的操作
                OnClickConfirm = delegate (object userData) { UnityGameFramework.Runtime.GameEntry.Shutdown(ShutdownType.Quit); },
            });
        }

#if UNITY_2017_3_OR_NEWER
        /// <summary>
        /// 当界面打开时调用。
        /// </summary>
        protected override void OnOpen(object userData)
#else
        protected internal override void OnOpen(object userData)
#endif
        {
            base.OnOpen(userData);

            // 接收并保存打开此界面的 ProcedureMenu 实例
            m_ProcedureMenu = (ProcedureMenu)userData;
            if (m_ProcedureMenu == null)
            {
                Log.Warning("ProcedureMenu is invalid when open MenuForm.");
                return;
            }

            // 平台特定逻辑：iOS平台不应显示退出按钮
            m_QuitButton.SetActive(Application.platform != RuntimePlatform.IPhonePlayer);
        }

#if UNITY_2017_3_OR_NEWER
        /// <summary>
        /// 当界面关闭时调用。
        /// </summary>
        protected override void OnClose(bool isShutdown, object userData)
#else
        protected internal override void OnClose(bool isShutdown, object userData)
#endif
        {
            // 清理对流程的引用，防止内存泄漏
            m_ProcedureMenu = null;

            base.OnClose(isShutdown, userData);
        }
    }
}