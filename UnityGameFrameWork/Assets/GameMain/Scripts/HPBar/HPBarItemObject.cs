using GameFramework;
using GameFramework.ObjectPool;
using UnityEngine;

namespace PuddingCat
{
    /// <summary>
    /// 血条项对象，用于 GameFramework 对象池管理的包装类。
    /// </summary>
    public class HPBarItemObject : ObjectBase
    {
        /// <summary>
        /// 创建一个血条项对象实例。
        /// </summary>
        /// <param name="target">要被包装的实际目标对象，这里通常是 HPBarItem 实例。</param>
        /// <returns>创建的血条项对象。</returns>
        public static HPBarItemObject Create(object target)
        {
            // 从引用池中获取一个 HPBarItemObject 实例，避免频繁 new 对象
            HPBarItemObject hpBarItemObject = ReferencePool.Acquire<HPBarItemObject>();
            // 调用父类的 Initialize 方法，将实际的 HPBarItem 实例存起来
            hpBarItemObject.Initialize(target);
            return hpBarItemObject;
        }

        /// <summary>
        /// 释放对象。
        /// 在对象被对象池销毁时调用。
        /// </summary>
        /// <param name="isShutdown">对象池是否正在关闭。</param>
        protected override void Release(bool isShutdown)
        {
            // 从 Target 属性获取被包装的 HPBarItem 实例
            HPBarItem hpBarItem = (HPBarItem)Target;
            if (hpBarItem == null)
            {
                return;
            }

            // 销毁血条项的GameObject，清理场景资源
            Object.Destroy(hpBarItem.gameObject);
        }
    }
}