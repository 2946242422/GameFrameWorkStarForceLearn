namespace PuddingCat
{
    public static partial class Constant
    {
        /// <summary>
        /// 玩家设置相关的常量。
        /// 这些字符串用作本地存储（如 PlayerPrefs）的键（Key）。
        /// </summary>
        public static class Setting
        {
            // 语言设置的键
            public const string Language = "Setting.Language";

            // 声音组静音状态的键（{0} 是一个占位符，用于替换具体的声音组名称，如 Music, Sound）
            public const string SoundGroupMuted = "Setting.{0}Muted";

            // 声音组音量大小的键
            public const string SoundGroupVolume = "Setting.{0}Volume";

            // --- 为了方便直接使用，下面也为具体的声音组定义了常量 ---
            public const string MusicMuted = "Setting.MusicMuted";
            public const string MusicVolume = "Setting.MusicVolume";
            public const string SoundMuted = "Setting.SoundMuted";
            public const string SoundVolume = "Setting.SoundVolume";
            public const string UISoundMuted = "Setting.UISoundMuted";
            public const string UISoundVolume = "Setting.UISoundVolume";
        }
    }
}