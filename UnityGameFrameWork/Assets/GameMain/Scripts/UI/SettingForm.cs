using GameFramework.Localization;
using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

namespace PuddingCat
{
    /// <summary>
    /// 设置界面的逻辑脚本。
    /// </summary>
    public class SettingForm : UGuiForm
    {
        // --- 音频设置相关的UI控件 ---
        [SerializeField]
        private Toggle m_MusicMuteToggle = null; // 音乐静音开关
        [SerializeField]
        private Slider m_MusicVolumeSlider = null; // 音乐音量滑块
        [SerializeField]
        private Toggle m_SoundMuteToggle = null; // 音效静音开关
        [SerializeField]
        private Slider m_SoundVolumeSlider = null; // 音效音量滑块
        [SerializeField]
        private Toggle m_UISoundMuteToggle = null; // UI音效静音开关
        [SerializeField]
        private Slider m_UISoundVolumeSlider = null; // UI音效音量滑块

        // --- 语言设置相关的UI控件 ---
        [SerializeField]
        private CanvasGroup m_LanguageTipsCanvasGroup = null; // “需要重启”提示的CanvasGroup，用于控制其显隐和透明度
        [SerializeField]
        private Toggle m_EnglishToggle = null; // 英语选项
        [SerializeField]
        private Toggle m_ChineseSimplifiedToggle = null; // 简体中文选项
        [SerializeField]
        private Toggle m_ChineseTraditionalToggle = null; // 繁体中文选项
        [SerializeField]
        private Toggle m_KoreanToggle = null; // 韩语选项

        // 用于临时存储玩家当前选择的语言，可能与游戏中实际应用的语言不同
        private Language m_SelectedLanguage = Language.Unspecified;

        /// <summary>
        /// 当音乐静音Toggle状态改变时调用。
        /// </summary>
        public void OnMusicMuteChanged(bool isOn)
        {
            // Toggle的isOn为true代表“开启”，对应声音系统是“非静音”，所以要取反
            GameEntry.Sound.Mute("Music", !isOn);
            // UX优化：当静音时（isOn为false），隐藏音量调节滑块
            m_MusicVolumeSlider.gameObject.SetActive(isOn);
        }

        /// <summary>
        /// 当音乐音量Slider的值改变时调用。
        /// </summary>
        public void OnMusicVolumeChanged(float volume)
        {
            GameEntry.Sound.SetVolume("Music", volume);
        }

        public void OnSoundMuteChanged(bool isOn)
        {
            GameEntry.Sound.Mute("Sound", !isOn);
            m_SoundVolumeSlider.gameObject.SetActive(isOn);
        }

        public void OnSoundVolumeChanged(float volume)
        {
            GameEntry.Sound.SetVolume("Sound", volume);
        }

        public void OnUISoundMuteChanged(bool isOn)
        {
            GameEntry.Sound.Mute("UISound", !isOn);
            m_UISoundVolumeSlider.gameObject.SetActive(isOn);
        }

        public void OnUISoundVolumeChanged(float volume)
        {
            GameEntry.Sound.SetVolume("UISound", volume);
        }

        /// <summary>
        /// 当英语Toggle被选中时调用。
        /// </summary>
        public void OnEnglishSelected(bool isOn)
        {
            if (!isOn) return; // 只处理被选中的情况

            m_SelectedLanguage = Language.English;
            RefreshLanguageTips(); // 刷新“需要重启”的提示
        }

        public void OnChineseSimplifiedSelected(bool isOn)
        {
            if (!isOn) return;

            m_SelectedLanguage = Language.ChineseSimplified;
            RefreshLanguageTips();
        }

        public void OnChineseTraditionalSelected(bool isOn)
        {
            if (!isOn) return;

            m_SelectedLanguage = Language.ChineseTraditional;
            RefreshLanguageTips();
        }

        public void OnKoreanSelected(bool isOn)
        {
            if (!isOn) return;

            m_SelectedLanguage = Language.Korean;
            RefreshLanguageTips();
        }

        /// <summary>
        /// 当“提交”按钮被点击时调用。
        /// </summary>
        public void OnSubmitButtonClick()
        {
            // 如果玩家选择的语言和当前语言相同，则什么都不做，直接关闭界面
            if (m_SelectedLanguage == GameEntry.Localization.Language)
            {
                Close();
                return;
            }

            // 1. 将新的语言设置保存到本地
            GameEntry.Setting.SetString(Constant.Setting.Language, m_SelectedLanguage.ToString());
            GameEntry.Setting.Save();

            // 2. 停止当前音乐，防止重启时声音突然中断
            GameEntry.Sound.StopMusic();
            // 3. 执行重启游戏的操作，以应用新的语言和资源变体
            UnityGameFramework.Runtime.GameEntry.Shutdown(ShutdownType.Restart);
        }

#if UNITY_2017_3_OR_NEWER
        /// <summary>
        /// 当界面打开时调用，用于初始化UI显示。
        /// </summary>
        protected override void OnOpen(object userData)
#else
        protected internal override void OnOpen(object userData)
#endif
        {
            base.OnOpen(userData);

            // --- 根据当前游戏设置，初始化所有UI控件的状态 ---
            m_MusicMuteToggle.isOn = !GameEntry.Sound.IsMuted("Music");
            m_MusicVolumeSlider.value = GameEntry.Sound.GetVolume("Music");

            m_SoundMuteToggle.isOn = !GameEntry.Sound.IsMuted("Sound");
            m_SoundVolumeSlider.value = GameEntry.Sound.GetVolume("Sound");

            m_UISoundMuteToggle.isOn = !GameEntry.Sound.IsMuted("UISound");
            m_UISoundVolumeSlider.value = GameEntry.Sound.GetVolume("UISound");

            // 根据当前语言，设置对应的Toggle为选中状态
            m_SelectedLanguage = GameEntry.Localization.Language;
            switch (m_SelectedLanguage)
            {
                case Language.English: m_EnglishToggle.isOn = true; break;
                case Language.ChineseSimplified: m_ChineseSimplifiedToggle.isOn = true; break;
                case Language.ChineseTraditional: m_ChineseTraditionalToggle.isOn = true; break;
                case Language.Korean: m_KoreanToggle.isOn = true; break;
                default: break;
            }
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

            // 如果“需要重启”的提示是激活的
            if (m_LanguageTipsCanvasGroup.gameObject.activeSelf)
            {
                // 使用Sin函数创建一个0到1之间平滑变化的“呼吸”效果，以吸引玩家注意
                m_LanguageTipsCanvasGroup.alpha = 0.5f + 0.5f * Mathf.Sin(Mathf.PI * Time.time);
            }
        }

        /// <summary>
        /// 刷新语言提示的显示状态。
        /// </summary>
        private void RefreshLanguageTips()
        {
            // 如果玩家新选择的语言与当前游戏语言不同，则显示提示；否则隐藏
            m_LanguageTipsCanvasGroup.gameObject.SetActive(m_SelectedLanguage != GameEntry.Localization.Language);
        }
    }
}