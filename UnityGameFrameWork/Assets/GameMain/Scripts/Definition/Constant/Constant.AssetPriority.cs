namespace PuddingCat
{
    // partial 关键字表示这是一个分部类，允许将 Constant 类的定义分散在多个文件中。
    public static partial class Constant
    {
        /// <summary>
        /// 资源优先级定义。
        /// 资源加载管理器会根据此优先级决定加载顺序，数值越大，优先级越高。
        /// </summary>
        public static class AssetPriority
        {
            // --- 核心资源 ---
            public const int ConfigAsset = 100; // 配置文件
            public const int DataTableAsset = 100; // 数据表
            public const int DictionaryAsset = 100; // 本地化字典
            public const int FontAsset = 50; // 字体
            public const int MusicAsset = 20; // 音乐
            public const int SceneAsset = 0; // 场景（优先级最低）
            public const int SoundAsset = 30; // 音效
            public const int UIFormAsset = 50; // UI界面
            public const int UISoundAsset = 30; // UI音效

            // --- 游戏逻辑相关资源 (飞机大战示例) ---
            public const int MyAircraftAsset = 90; // 玩家飞机
            public const int AircraftAsset = 80; // 普通飞机
            public const int ThrusterAsset = 30; // 推进器特效
            public const int WeaponAsset = 30; // 武器
            public const int ArmorAsset = 30; // 装甲
            public const int BulletAsset = 80; // 子弹
            public const int AsteroiAsset = 80; // 小行星
            public const int EffectAsset = 80; // 特效
        }
    }
}