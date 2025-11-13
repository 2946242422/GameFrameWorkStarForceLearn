//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

namespace PuddingCat
{
    /// <summary>
    /// 版本信息。
    /// 这是一个数据容器类，用于存储从服务器获取的版本信息。
    /// </summary>
    public class VersionInfo
    {
        /// <summary>
        /// 获取或设置是否需要强制更新游戏应用。
        /// 如果为 true，则必须引导玩家去应用商店下载新包。
        /// </summary>
        public bool ForceUpdateGame
        {
            get;
            set;
        }

        /// <summary>
        /// 获取或设置最新的游戏版本号（如 "1.0.1"）。
        /// 主要用于UI显示。
        /// </summary>
        public string LatestGameVersion
        {
            get;
            set;
        }

        /// <summary>
        /// 获取或设置最新的游戏内部版本号。
        /// 用于程序比较，判断App版本是否过旧。
        /// </summary>
        public int InternalGameVersion
        {
            get;
            set;
        }

        /// <summary>
        /// 获取或设置最新的资源内部版本号。
        /// 用于程序比较，判断本地资源是否需要热更新。
        /// </summary>
        public int InternalResourceVersion
        {
            get;
            set;
        }

        /// <summary>
        /// 获取或设置资源更新下载地址的根URL。
        /// </summary>
        public string UpdatePrefixUri
        {
            get;
            set;
        }

        /// <summary>
        /// 获取或设置资源版本列表文件的长度（解压后）。
        /// 用于下载后的文件校验。
        /// </summary>
        public int VersionListLength
        {
            get;
            set;
        }

        /// <summary>
        /// 获取或设置资源版本列表文件的哈希值（解压后）。
        /// 用于下载后的文件校验。
        /// </summary>
        public int VersionListHashCode
        {
            get;
            set;
        }

        /// <summary>
        /// 获取或设置资源版本列表文件的长度（压缩后）。
        /// 用于下载后的文件校验。
        /// </summary>
        public int VersionListCompressedLength
        {
            get;
            set;
        }

        /// <summary>
        /// 获取或设置资源版本列表文件的哈希值（压缩后）。
        /// 用于下载后的文件校验。
        /// </summary>
        public int VersionListCompressedHashCode
        {
            get;
            set;
        }
    }
}