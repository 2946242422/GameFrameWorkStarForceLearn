using UnityEngine;

namespace PuddingCat
{
    public static partial class Constant
    {
        /// <summary>
        /// Unity 层（Layer）的定义。
        /// 用于物理检测、渲染筛选等。
        /// </summary>
        public static class Layer
        {
            // -- Default 层 --
            public const string DefaultLayerName = "Default";

            // LayerMask.NameToLayer 是一个耗性能的操作，因此在启动时获取一次并缓存起来。
            public static readonly int DefaultLayerId = LayerMask.NameToLayer(DefaultLayerName);

            // -- UI 层 --
            public const string UILayerName = "UI";
            public static readonly int UILayerId = LayerMask.NameToLayer(UILayerName);

            // -- 可锁定目标层 (例如：敌人、可破坏物) --
            public const string TargetableObjectLayerName = "Targetable Object";
            public static readonly int TargetableObjectLayerId = LayerMask.NameToLayer(TargetableObjectLayerName);
        }
    }
}