using GameFramework;
using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

namespace PuddingCat
{
    /// <summary>
    /// 对话框界面的逻辑脚本。
    /// </summary>
    public class DialogForm : UGuiForm
    {
        // --- 在 Unity Inspector 中拖拽赋值的UI控件 ---
        [SerializeField] private Text m_TitleText = null; // 标题文本

        [SerializeField] private Text m_MessageText = null; // 消息文本

        [SerializeField] private GameObject[] m_ModeObjects = null; // 模式对象数组，[0]对应1按钮模式, [1]对应2按钮模式, ...

        [SerializeField] private Text[] m_ConfirmTexts = null; // 所有模式下的“确认”按钮文本

        [SerializeField] private Text[] m_CancelTexts = null; // 所有模式下的“取消”按钮文本

        [SerializeField] private Text[] m_OtherTexts = null; // 所有模式下的“其他”按钮文本

        // --- 内部状态变量 ---
        private int m_DialogMode = 1;
        private bool m_PauseGame = false;

        private object m_UserData = null;

        // 保存从 DialogParams 传来的回调函数
        private GameFrameworkAction<object> m_OnClickConfirm = null;
        private GameFrameworkAction<object> m_OnClickCancel = null;
        private GameFrameworkAction<object> m_OnClickOther = null;

        /// <summary>
        /// 当“确认”按钮被点击时调用（此方法在Unity Editor中关联到按钮的OnClick事件）。
        /// </summary>
        public void OnConfirmButtonClick()
        {
            Close(); // 关闭当前界面

            // 如果存在确认回调，则执行它
            if (m_OnClickConfirm != null)
            {
                m_OnClickConfirm(m_UserData);
            }
        }

        /// <summary>
        /// 当“取消”按钮被点击时调用。
        /// </summary>
        public void OnCancelButtonClick()
        {
            Close();

            if (m_OnClickCancel != null)
            {
                m_OnClickCancel(m_UserData);
            }
        }

        /// <summary>
        /// 当“其他”按钮被点击时调用。
        /// </summary>
        public void OnOtherButtonClick()
        {
            Close();

            if (m_OnClickOther != null)
            {
                m_OnClickOther(m_UserData);
            }
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

            // 将传入的 userData 转换为 DialogParams
            DialogParams dialogParams = (DialogParams)userData;
            if (dialogParams == null)
            {
                Log.Warning("DialogParams is invalid.");
                return;
            }

            // --- 从 DialogParams 读取数据并应用到UI ---
            m_DialogMode = dialogParams.Mode;
            RefreshDialogMode(); // 根据模式显示/隐藏不同的按钮组合

            m_TitleText.text = dialogParams.Title;
            m_MessageText.text = dialogParams.Message;

            m_PauseGame = dialogParams.PauseGame;
            RefreshPauseGame(); // 根据参数决定是否暂停游戏

            m_UserData = dialogParams.UserData; // 保存用户数据

            // 刷新按钮文本并保存回调函数
            RefreshConfirmText(dialogParams.ConfirmText);
            m_OnClickConfirm = dialogParams.OnClickConfirm;

            RefreshCancelText(dialogParams.CancelText);
            m_OnClickCancel = dialogParams.OnClickCancel;

            RefreshOtherText(dialogParams.OtherText);
            m_OnClickOther = dialogParams.OnClickOther;
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
            // 如果之前暂停了游戏，则现在恢复
            if (m_PauseGame)
            {
                GameEntry.Base.ResumeGame();
            }

            // --- 重置所有状态，为UI复用做准备 ---
            m_DialogMode = 1;
            m_TitleText.text = string.Empty;
            m_MessageText.text = string.Empty;
            m_PauseGame = false;
            m_UserData = null;
            m_OnClickConfirm = null;
            m_OnClickCancel = null;
            m_OnClickOther = null;

            base.OnClose(isShutdown, userData);
        }

        // 根据 m_DialogMode 激活对应的 GameObject
        private void RefreshDialogMode()
        {
            for (int i = 1; i <= m_ModeObjects.Length; i++)
            {
                m_ModeObjects[i - 1].SetActive(i == m_DialogMode);
            }
        }

        // 如果需要，暂停游戏
        private void RefreshPauseGame()
        {
            if (m_PauseGame)
            {
                GameEntry.Base.PauseGame();
            }
        }

        // 更新所有确认按钮的文本
        private void RefreshConfirmText(string confirmText)
        {
            // 如果传入的文本为空，则使用本地化文件中的默认文本
            if (string.IsNullOrEmpty(confirmText))
            {
                confirmText = GameEntry.Localization.GetString("Dialog.ConfirmButton");
            }

            // 更新所有可能的确认按钮
            for (int i = 0; i < m_ConfirmTexts.Length; i++)
            {
                m_ConfirmTexts[i].text = confirmText;
            }
        }

        // 更新所有取消按钮的文本
        private void RefreshCancelText(string cancelText)
        {
            if (string.IsNullOrEmpty(cancelText))
            {
                cancelText = GameEntry.Localization.GetString("Dialog.CancelButton");
            }

            for (int i = 0; i < m_CancelTexts.Length; i++)
            {
                m_CancelTexts[i].text = cancelText;
            }
        }

        // 更新所有其他按钮的文本
        private void RefreshOtherText(string otherText)
        {
            if (string.IsNullOrEmpty(otherText))
            {
                otherText = GameEntry.Localization.GetString("Dialog.OtherButton");
            }

            for (int i = 0; i < m_OtherTexts.Length; i++)
            {
                m_OtherTexts[i].text = otherText;
            }
        }
    }
}