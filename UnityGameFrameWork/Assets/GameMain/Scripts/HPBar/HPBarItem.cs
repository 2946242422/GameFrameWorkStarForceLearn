//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

namespace PuddingCat
{
    /// <summary>
    /// 血条项，负责单个血条UI的显示和行为。
    /// </summary>
    public class HPBarItem : MonoBehaviour
    {
        // 定义动画的各个阶段时长
        private const float AnimationSeconds = 0.3f; // 血量变化动画的时长
        private const float KeepSeconds = 0.4f;      // 动画结束后保持显示的时长
        private const float FadeOutSeconds = 0.3f;   // 淡出效果的时长

        // 血条的Slider组件，用于显示血量
        [SerializeField]
        private Slider m_HPBar = null;

        // 该血条UI所在的父Canvas
        private Canvas m_ParentCanvas = null;
        // 缓存自身的RectTransform组件，用于位置变换
        private RectTransform m_CachedTransform = null;
        // 缓存自身的CanvasGroup组件，用于控制整体的淡入淡出
        private CanvasGroup m_CachedCanvasGroup = null;
        // 该血条所属的实体（Entity），即显示谁的血条
        private Entity m_Owner = null;
        // 缓存实体的ID，用于判断实体是否已被回收
        private int m_OwnerId = 0;

        /// <summary>
        /// 获取血条所属的实体。
        /// </summary>
        public Entity Owner
        {
            get
            {
                return m_Owner;
            }
        }

        /// <summary>
        /// 初始化血条。
        /// </summary>
        /// <param name="owner">所属实体。</param>
        /// <param name="parentCanvas">父Canvas。</param>
        /// <param name="fromHPRatio">血量变化的起始比例。</param>
        /// <param name="toHPRatio">血量变化的目标比例。</param>
        public void Init(Entity owner, Canvas parentCanvas, float fromHPRatio, float toHPRatio)
        {
            if (owner == null)
            {
                Log.Error("Owner is invalid.");
                return;
            }

            m_ParentCanvas = parentCanvas;

            gameObject.SetActive(true); // 激活UI对象
            StopAllCoroutines(); // 停止之前可能在运行的所有协程

            m_CachedCanvasGroup.alpha = 1f; // 确保UI是完全不透明的
            // 如果是新的实体，或者实体ID不匹配，则重置血条的初始值
            if (m_Owner != owner || m_OwnerId != owner.Id)
            {
                m_HPBar.value = fromHPRatio;
                m_Owner = owner;
                m_OwnerId = owner.Id;
            }

            Refresh(); // 立即刷新一次位置

            // 启动血条动画的协程
            StartCoroutine(HPBarCo(toHPRatio, AnimationSeconds, KeepSeconds, FadeOutSeconds));
        }

        /// <summary>
        /// 刷新血条位置。
        /// </summary>
        /// <returns>如果血条仍然可见，则返回true；否则返回false。</returns>
        public bool Refresh()
        {
            // 如果已经完全透明，则认为不再需要刷新
            if (m_CachedCanvasGroup.alpha <= 0f)
            {
                return false;
            }
            
            // 确保实体仍然有效且存在
            if (m_Owner != null && Owner.Available && Owner.Id == m_OwnerId)
            {
                // 计算实体在世界中的位置（稍微向上偏移一点），并转换为屏幕坐标
                Vector3 worldPosition = m_Owner.CachedTransform.position + Vector3.up; // 向上偏移1米
                Vector3 screenPosition = GameEntry.Scene.MainCamera.WorldToScreenPoint(worldPosition);

                Vector2 position;
                // 将屏幕坐标转换为父Canvas下的本地坐标
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)m_ParentCanvas.transform, screenPosition,
                    m_ParentCanvas.worldCamera, out position))
                {
                    m_CachedTransform.localPosition = position; // 设置UI位置
                }
            }

            return true;
        }

        /// <summary>
        /// 重置血条状态，为回收到对象池做准备。
        /// </summary>
        public void Reset()
        {
            StopAllCoroutines();
            m_CachedCanvasGroup.alpha = 1f; // 恢复不透明
            m_HPBar.value = 1f; // 血条值恢复为1
            m_Owner = null; // 解除与实体的关联
            gameObject.SetActive(false); // 隐藏UI对象
        }
        
        // 在对象被创建时调用
        private void Awake()
        {
            // 获取并缓存必要的组件引用
            m_CachedTransform = GetComponent<RectTransform>();
            if (m_CachedTransform == null)
            {
                Log.Error("RectTransform is invalid.");
                return;
            }

            m_CachedCanvasGroup = GetComponent<CanvasGroup>();
            if (m_CachedCanvasGroup == null)
            {
                Log.Error("CanvasGroup is invalid.");
                return;
            }
        }

        /// <summary>
        /// 血条动画的协程。
        /// </summary>
        private IEnumerator HPBarCo(float value, float animationDuration, float keepDuration, float fadeOutDuration)
        {
            // 第一步：平滑地将血条值变化到目标值
            yield return m_HPBar.SmoothValue(value, animationDuration);
            // 第二步：等待一段时间，让玩家能看清血量变化
            yield return new WaitForSeconds(keepDuration);
            // 第三步：将血条整体淡出，直至完全透明
            yield return m_CachedCanvasGroup.FadeToAlpha(0f, fadeOutDuration);
        }
    }
}