using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

namespace PuddingCat
{
    /// <summary>
    /// UGUI 界面的抽象基类。
    /// 项目中所有 UI Form 的逻辑类都应继承自此类。
    /// </summary>
    public abstract class UGuiForm : UIFormLogic
    {
        // 每个 Canvas 之间 sortingOrder 的固定间隔
        public const int DepthFactor = 100;

        // 默认的淡入淡出时间
        private const float FadeTime = 0.3f;

        // 全局静态的主字体
        private static Font s_MainFont = null;

        // 缓存的此界面的主 Canvas
        private Canvas m_CachedCanvas = null;

        // 缓存的此界面的 CanvasGroup，用于控制整体 Alpha
        private CanvasGroup m_CanvasGroup = null;

        // 用于临时存储子 Canvas 的列表容器
        private List<Canvas> m_CachedCanvasContainer = new List<Canvas>();

        /// <summary>
        /// 获取界面在 Prefab 中原始的层级深度。
        /// </summary>
        public int OriginalDepth { get; private set; }

        /// <summary>
        /// 获取界面当前的层级深度。
        /// </summary>
        public int Depth
        {
            get { return m_CachedCanvas.sortingOrder; }
        }

        /// <summary>
        /// 关闭界面（带淡出动画）。
        /// </summary>
        public void Close()
        {
            Close(false);
        }

        /// <summary>
        /// 关闭界面。
        /// </summary>
        /// <param name="ignoreFade">是否忽略淡出动画，立即关闭。</param>
        public void Close(bool ignoreFade)
        {
            StopAllCoroutines();

            if (ignoreFade)
            {
                // 直接通知UI组件关闭
                GameEntry.UI.CloseUIForm(this);
            }
            else
            {
                // 启动带淡出动画的关闭协程
                StartCoroutine(CloseCo(FadeTime));
            }
        }

        /// <summary>
        /// 播放UI音效。
        /// </summary>
        /// <param name="uiSoundId">要播放的UI音效ID。</param>
        public void PlayUISound(int uiSoundId)
        {
            GameEntry.Sound.PlayUISound(uiSoundId);
        }

        /// <summary>
        /// 设置全局主字体。
        /// </summary>
        /// <param name="mainFont">主字体。</param>
        public static void SetMainFont(Font mainFont)
        {
            if (mainFont == null)
            {
                Log.Error("Main font is invalid.");
                return;
            }

            s_MainFont = mainFont;
        }

#if UNITY_2017_3_OR_NEWER
        /// <summary>
        /// 界面初始化。
        /// </summary>
        protected override void OnInit(object userData)
#else
        protected internal override void OnInit(object userData)
#endif
        {
            base.OnInit(userData);

            // --- 自动化组件配置 ---
            m_CachedCanvas = gameObject.GetOrAddComponent<Canvas>();
            m_CachedCanvas.overrideSorting = true; // 必须开启，由框架管理排序
            OriginalDepth = m_CachedCanvas.sortingOrder;

            m_CanvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();

            // --- 自动化布局 ---
            // 将 RectTransform 设置为全屏拉伸模式
            RectTransform transform = GetComponent<RectTransform>();
            transform.anchorMin = Vector2.zero;
            transform.anchorMax = Vector2.one;
            transform.anchoredPosition = Vector2.zero;
            transform.sizeDelta = Vector2.zero;

            gameObject.GetOrAddComponent<GraphicRaycaster>();

            // --- 自动化字体和本地化 ---
            Text[] texts = GetComponentsInChildren<Text>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                // 替换为全局主字体
                texts[i].font = s_MainFont;
                // 如果文本内容不为空，则认为它是本地化Key，进行翻译
                if (!string.IsNullOrEmpty(texts[i].text))
                {
                    texts[i].text = GameEntry.Localization.GetString(texts[i].text);
                }
            }
        }

#if UNITY_2017_3_OR_NEWER
        /// <summary>
        /// 界面打开。
        /// </summary>
        protected override void OnOpen(object userData)
#else
        protected internal override void OnOpen(object userData)
#endif
        {
            base.OnOpen(userData);

            m_CanvasGroup.alpha = 0f;
            StopAllCoroutines();
            // 启动淡入动画
            StartCoroutine(m_CanvasGroup.FadeToAlpha(1f, FadeTime));
        }

#if UNITY_2017_3_OR_NEWER
        /// <summary>
        /// 界面关闭。
        /// </summary>
        protected override void OnClose(bool isShutdown, object userData)
#else
        protected internal override void OnClose(bool isShutdown, object userData)
#endif
        {
            base.OnClose(isShutdown, userData);
        }

#if UNITY_2017_3_OR_NEWER
        /// <summary>
        /// 界面暂停。
        /// </summary>
        protected override void OnPause()
#else
        protected internal override void OnPause()
#endif
        {
            base.OnPause();
        }

#if UNITY_2017_3_OR_NEWER
        /// <summary>
        /// 界面恢复。
        /// </summary>
        protected override void OnResume()
#else
        protected internal override void OnResume()
#endif
        {
            base.OnResume();

            m_CanvasGroup.alpha = 0f;
            StopAllCoroutines();
            // 启动淡入动画
            StartCoroutine(m_CanvasGroup.FadeToAlpha(1f, FadeTime));
        }

#if UNITY_2017_3_OR_NEWER
        /// <summary>
        /// 界面深度改变。
        /// </summary>
        protected override void OnDepthChanged(int uiGroupDepth, int depthInUIGroup)
#else
        protected internal override void OnDepthChanged(int uiGroupDepth, int depthInUIGroup)
#endif
        {
            int oldDepth = Depth;
            base.OnDepthChanged(uiGroupDepth, depthInUIGroup);
            // 计算新的深度与旧深度的差值
            int deltaDepth = UGuiGroupHelper.DepthFactor * uiGroupDepth + DepthFactor * depthInUIGroup - oldDepth +
                             OriginalDepth;
            // 遍历所有子Canvas，统一应用深度差值
            GetComponentsInChildren(true, m_CachedCanvasContainer);
            for (int i = 0; i < m_CachedCanvasContainer.Count; i++)
            {
                m_CachedCanvasContainer[i].sortingOrder += deltaDepth;
            }

            m_CachedCanvasContainer.Clear();
        }

        /// <summary>
        /// 带淡出动画的关闭协程。
        /// </summary>
        private IEnumerator CloseCo(float duration)
        {
            yield return m_CanvasGroup.FadeToAlpha(0f, duration);
            GameEntry.UI.CloseUIForm(this);
        }
    }
}