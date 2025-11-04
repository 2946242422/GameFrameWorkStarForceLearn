using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

namespace PuddingCat
{
    /// <summary>
    /// uGUI 界面组辅助器。
    /// 此脚本应挂载在场景中代表每个 UI 组的 GameObject 上。
    /// 它的核心作用是管理整个界面组的基础渲染深度。
    /// </summary>
    public class UGuiGroupHelper : UIGroupHelperBase
    {
        /// <summary>
        /// 每个界面组之间的深度因子。
        /// 这个巨大的数值确保了不同组之间的 sortingOrder 不会重叠。
        /// </summary>
        public const int DepthFactor = 10000;

        // 当前界面组的深度值（由UI框架进行设置）
        private int m_Depth = 0;

        // 缓存的Canvas组件
        private Canvas m_CachedCanvas = null;

        /// <summary>
        /// 设置界面组深度。此方法由 Game Framework 的 UI 组件自动调用。
        /// </summary>
        /// <param name="depth">框架分配的界面组深度。</param>
        public override void SetDepth(int depth)
        {
            m_Depth = depth;
            // 开启 overrideSorting，让我们可以手动控制 Canvas 的渲染顺序
            m_CachedCanvas.overrideSorting = true;
            // 根据深度值和深度因子，计算出此 Canvas 的最终 sortingOrder
            m_CachedCanvas.sortingOrder = DepthFactor * depth;
        }

        // 在对象第一次被创建时调用
        private void Awake()
        {
            // 获取或添加 Canvas 组件，作为此 UI 组的根画布
            m_CachedCanvas = gameObject.GetOrAddComponent<Canvas>();
            // 获取或添加 GraphicRaycaster 组件，使此 UI 组能够接收UI事件（如点击）
            gameObject.GetOrAddComponent<GraphicRaycaster>();
        }

        // 在所有 Awake 调用后，对象被激活时调用
        private void Start()
        {
            // 再次确保 Canvas 的排序设置正确
            m_CachedCanvas.overrideSorting = true;
            m_CachedCanvas.sortingOrder = DepthFactor * m_Depth;

            // 将此 UI 组的 RectTransform 设置为全屏拉伸，使其铺满整个屏幕
            RectTransform transform = GetComponent<RectTransform>();
            transform.anchorMin = Vector2.zero;
            transform.anchorMax = Vector2.one;
            transform.anchoredPosition = Vector2.zero;
            transform.sizeDelta = Vector2.zero;
        }
    }
}