using UnityEngine;
using UnityEngine.Events; // 引入 UnityEvent 命名空间
using UnityEngine.EventSystems; // 引入事件系统接口的命名空间

namespace PuddingCat
{
    /// <summary>
    /// 通用按钮组件。
    /// 实现了鼠标悬停、点击等状态的视觉反馈，并提供了对应的事件接口。
    /// </summary>
    // 实现多个事件接口，以便能接收到Unity事件系统的回调
    public class CommonButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        private const float FadeTime = 0.3f; // 淡入淡出动画的时长
        private const float OnHoverAlpha = 0.7f; // 鼠标悬停时的透明度
        private const float OnClickAlpha = 0.6f; // 鼠标按下时的透明度

        [SerializeField]
        private UnityEvent m_OnHover = null; // 在 Inspector 中配置的“悬停”事件

        [SerializeField]
        private UnityEvent m_OnClick = null; // 在 Inspector 中配置的“点击”事件

        // 对 CanvasGroup 组件的引用，用于控制整体的 Alpha 透明度
        private CanvasGroup m_CanvasGroup = null;

        private void Awake()
        {
            // 获取或自动添加 CanvasGroup 组件，确保其一定存在
            m_CanvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();
        }

        /// <summary>
        /// 当组件被禁用时由Unity调用。
        /// </summary>
        private void OnDisable()
        {
            // 将透明度重置为1，防止按钮在半透明状态下被禁用，下次激活时状态不正确
            m_CanvasGroup.alpha = 1f;
        }

        /// <summary>
        /// 当鼠标指针进入此对象的区域时，由事件系统调用。
        /// </summary>
        public void OnPointerEnter(PointerEventData eventData)
        {
            // 过滤掉非左键的交互
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            StopAllCoroutines(); // 停止所有正在进行的淡入淡出，防止效果冲突
            // 开始一个协程，平滑地将Alpha过渡到悬停值
            StartCoroutine(m_CanvasGroup.FadeToAlpha(OnHoverAlpha, FadeTime));
            // 触发在Inspector中设置的 OnHover 事件（常用于播放悬停音效）
            m_OnHover.Invoke();
        }

        /// <summary>
        /// 当鼠标指针离开此对象的区域时，由事件系统调用。
        /// </summary>
        public void OnPointerExit(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            StopAllCoroutines();
            // 平滑地将Alpha恢复到完全不透明
            StartCoroutine(m_CanvasGroup.FadeToAlpha(1f, FadeTime));
        }

        /// <summary>
        /// 当鼠标指针在此对象上按下时，由事件系统调用。
        /// </summary>
        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            // 立即将Alpha设置为点击值，提供即时反馈
            m_CanvasGroup.alpha = OnClickAlpha;
            // 触发在Inspector中设置的 OnClick 事件（核心功能）
            m_OnClick.Invoke();
        }

        /// <summary>
        /// 当鼠标指针在此对象上抬起时，由事件系统调用。
        /// </summary>
        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            // 鼠标抬起后，立即将Alpha恢复到悬停值
            // （假设此时鼠标仍在按钮上，如果已移开，OnPointerExit会处理后续的淡出）
            m_CanvasGroup.alpha = OnHoverAlpha;
        }
    }
}