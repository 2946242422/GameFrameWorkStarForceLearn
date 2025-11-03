namespace PuddingCat // 注意这里的命名空间是小写开头
{
    /// <summary>
    /// 构建信息类。
    /// 这是一个数据结构类，不继承MonoBehaviour，用于存储游戏的版本和更新相关信息。
    /// 通常用于解析本地或服务器上的版本配置文件（如 a.json）。
    /// </summary>
    public class BuildInfo
    {
        /// <summary>
        /// 游戏版本号（对外显示的字符串，如 "1.0.0"）。
        /// </summary>
        public string GameVersion
        {
            get; // 定义了get访问器，允许外部读取
            set; // 定义了set访问器，允许外部写入
        }

        /// <summary>
        /// 内部游戏版本号（用于程序逻辑判断的整数，会随每次打包递增）。
        /// </summary>
        public int InternalGameVersion
        {
            get;
            set;
        }

        /// <summary>
        /// 检查版本更新的服务器地址。
        /// </summary>
        public string CheckVersionUrl
        {
            get;
            set;
        }

        /// <summary>
        /// Windows平台整包下载地址。
        /// </summary>
        public string WindowsAppUrl
        {
            get;
            set;
        }

        /// <summary>
        /// macOS平台整包下载地址。
        /// </summary>
        public string MacOSAppUrl
        {
            get;
            set;
        }

        /// <summary>
        /// iOS平台整包下载地址（通常是App Store链接）。
        /// </summary>
        public string IOSAppUrl
        {
            get;
            set;
        }

        /// <summary>
        /// Android平台整包下载地址（.apk文件链接）。
        /// </summary>
        public string AndroidAppUrl
        {
            get;
            set;
        }
    }
}