//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

namespace PuddingCat
{
    /// <summary>
    /// “关于”界面的逻辑脚本。
    /// 实现了无限循环的滚动字幕效果。
    /// </summary>
    public class AboutForm : UGuiForm
    {
        [SerializeField]
        private RectTransform m_Transform = null; // 需要滚动的文本或内容的 RectTransform

        [SerializeField]
        private float m_ScrollSpeed = 1f; // 向上滚动的速度

        // 滚动内容的初始Y坐标，会被动态计算
        private float m_InitPosition = 0f;

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

            // 获取父级的 CanvasScaler，用于进行分辨率自适应计算
            CanvasScaler canvasScaler = GetComponentInParent<CanvasScaler>();
            if (canvasScaler == null)
            {
                Log.Warning("Can not find CanvasScaler component.");
                return;
            }

            // 根据 CanvasScaler 的参考分辨率和当前屏幕的宽高比，
            // 计算出一个恰好在屏幕底部之外的初始Y坐标。
            // 这样做可以确保在任何分辨率下，滚动内容都能从屏幕正下方开始。
            m_InitPosition = -0.5f * canvasScaler.referenceResolution.x * Screen.height / Screen.width;
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

            // 每次打开界面时，都将滚动内容重置到计算好的初始位置
            m_Transform.SetLocalPositionY(m_InitPosition);

            // 切换到“关于”界面的专属背景音乐（ID为3）
            GameEntry.Sound.PlayMusic(3);
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
            base.OnClose(isShutdown, userData);

            // 关闭界面时，将背景音乐还原为主菜单音乐（ID为1）
            GameEntry.Sound.PlayMusic(1);
        }

#if UNITY_2017_3_OR_NEWER
        /// <summary>
        /// 界面轮询（每帧调用）。
        /// </summary>
        protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
#else
        protected internal override void OnUpdate(float elapseSeconds, float realElapseSeconds)
#endif
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);

            // 每帧都让内容向上移动一小段距离
            m_Transform.AddLocalPositionY(m_ScrollSpeed * elapseSeconds);
            
            // 判断是否已经完全滚出屏幕顶部
            if (m_Transform.localPosition.y > m_Transform.sizeDelta.y - m_InitPosition)
            {
                // 如果是，则瞬间传送回屏幕底部的初始位置，形成循环
                m_Transform.SetLocalPositionY(m_InitPosition);
            }
        }
    }
}