using GameFramework;

namespace PuddingCat
{
    /// <summary>
    /// </summary>
    public static class AssetUtility
    {
        /// <summary>
        /// 获取配置文件的资源路径。
        /// </summary>
        /// <param name="assetName">资源名称（不带后缀）。</param>
        /// <param name="fromBytes">是否从二进制文件（.bytes）加载，否则为文本文件（.txt）。</param>
        /// <returns>完整的资源路径。</returns>
        public static string GetConfigAsset(string assetName, bool fromBytes)
        {
            // Utility.Text.Format 是 Game Framework 提供的字符串格式化工具，功能类似 string.Format
            return Utility.Text.Format("Assets/GameMain/Configs/{0}.{1}", assetName, fromBytes ? "bytes" : "txt");
        }

        /// <summary>
        /// 获取数据表的资源路径。
        /// </summary>
        /// <param name="assetName">资源名称（不带后缀）。</param>
        /// <param name="fromBytes">是否从二进制文件（.bytes）加载，否则为文本文件（.txt）。</param>
        /// <returns>完整的资源路径。</returns>
        public static string GetDataTableAsset(string assetName, bool fromBytes)
        {
            return Utility.Text.Format("Assets/GameMain/DataTables/{0}.{1}", assetName, fromBytes ? "bytes" : "txt");
        }

        /// <summary>
        /// 获取本地化字典的资源路径。
        /// </summary>
        /// <param name="assetName">资源名称（不带后缀）。</param>
        /// <param name="fromBytes">是否从二进制文件（.bytes）加载，否则为XML文件（.xml）。</param>
        /// <returns>完整的资源路径。</returns>
        public static string GetDictionaryAsset(string assetName, bool fromBytes)
        {
            // 这个方法会根据当前游戏的语言设置，动态地构建到对应语言文件夹下的字典路径
            return Utility.Text.Format("Assets/GameMain/Localization/{0}/Dictionaries/{1}.{2}",
                GameEntry.Localization.Language, assetName, fromBytes ? "bytes" : "xml");
        }

        /// <summary>
        /// 获取字体的资源路径。
        /// </summary>
        /// <param name="assetName">字体资源名称（不带后缀）。</param>
        /// <returns>完整的资源路径。</returns>
        public static string GetFontAsset(string assetName)
        {
            return Utility.Text.Format("Assets/GameMain/Fonts/{0}.ttf", assetName);
        }

        /// <summary>
        /// 获取场景的资源路径。
        /// </summary>
        /// <param name="assetName">场景资源名称（不带后缀）。</param>
        /// <returns>完整的资源路径。</returns>
        public static string GetSceneAsset(string assetName)
        {
            return Utility.Text.Format("Assets/GameMain/Scenes/{0}.unity", assetName);
        }

        /// <summary>
        /// 获取音乐的资源路径。
        /// </summary>
        /// <param name="assetName">音乐资源名称（不带后缀）。</param>
        /// <returns>完整的资源路径。</returns>
        public static string GetMusicAsset(string assetName)
        {
            return Utility.Text.Format("Assets/GameMain/Music/{0}.mp3", assetName);
        }

        /// <summary>
        /// 获取音效的资源路径。
        /// </summary>
        /// <param name="assetName">音效资源名称（不带后缀）。</param>
        /// <returns>完整的资源路径。</returns>
        public static string GetSoundAsset(string assetName)
        {
            return Utility.Text.Format("Assets/GameMain/Sounds/{0}.wav", assetName);
        }

        /// <summary>
        /// 获取实体的资源路径（通常是Prefab）。
        /// </summary>
        /// <param name="assetName">实体资源名称（不带后缀）。</param>
        /// <returns>完整的资源路径。</returns>
        public static string GetEntityAsset(string assetName)
        {
            return Utility.Text.Format("Assets/GameMain/Entities/{0}.prefab", assetName);
        }

        /// <summary>
        /// 获取UI界面的资源路径（通常是Prefab）。
        /// </summary>
        /// <param name="assetName">UI界面资源名称（不带后缀）。</param>
        /// <returns>完整的资源路径。</returns>
        public static string GetUIFormAsset(string assetName)
        {
            return Utility.Text.Format("Assets/GameMain/UI/UIForms/{0}.prefab", assetName);
        }

        /// <summary>
        /// 获取UI音效的资源路径。
        /// </summary>
        /// <param name="assetName">UI音效资源名称（不带后缀）。</param>
        /// <returns>完整的资源路径。</returns>
        public static string GetUISoundAsset(string assetName)
        {
            return Utility.Text.Format("Assets/GameMain/UI/UISounds/{0}.wav", assetName);
        }
    }
}