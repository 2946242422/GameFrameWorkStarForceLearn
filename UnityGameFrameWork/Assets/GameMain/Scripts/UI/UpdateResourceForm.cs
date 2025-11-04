using UnityEngine;
using UnityEngine.UI;

namespace PuddingCat
{
    /// <summary>
    /// 更新资源界面。
    /// 这个脚本挂载在一个UI Prefab上，用于向玩家展示资源更新的进度。
    /// </summary>
    public class UpdateResourceForm : MonoBehaviour
    {
        // [SerializeField] 使得私有字段能在Unity Inspector面板中进行赋值

        // 用于显示描述文本的UI Text组件，例如 "正在检查更新..."
        [SerializeField] private Text m_DescriptionText = null;

        // 用于显示进度的UI Slider组件（进度条）
        [SerializeField] private Slider m_ProgressSlider = null;

        // Unity的生命周期函数，当对象被激活时调用一次
        private void Start()
        {
            // 目前为空，没有需要在开始时执行的逻辑
        }

        // Unity的生命周期函数，每一帧都会调用
        private void Update()
        {
            // 目前为空，没有需要每帧更新的逻辑
        }

        /// <summary>
        /// 设置进度条和描述文本。
        /// 这个方法由外部逻辑（如资源更新流程）调用。
        /// </summary>
        /// <param name="progress">进度值，通常范围是 0 到 1。</param>
        /// <param name="description">要显示的描述性文字。</param>
        public void SetProgress(float progress, string description)
        {
            // 更新进度条的显示值
            m_ProgressSlider.value = progress;
            // 更新文本内容
            m_DescriptionText.text = description;
        }
    }
}