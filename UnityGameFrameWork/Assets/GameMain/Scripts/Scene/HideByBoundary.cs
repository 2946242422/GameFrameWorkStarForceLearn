using UnityEngine;
using UnityGameFramework.Runtime;

namespace PuddingCat
{
    /// <summary>
    /// 通过边界隐藏实体（基于物理触发器）。
    /// 此脚本应挂载在一个勾选了 Is Trigger 的 Collider 上，作为场景的“回收边界”。
    /// </summary>
    public class HideByBoundary : MonoBehaviour
    {
        /// <summary>
        /// 当其他 Collider 离开此对象的触发器范围时，Unity 会自动调用此方法。
        /// </summary>
        /// <param name="other">离开的那个物体的 Collider。</param>
        private void OnTriggerExit(Collider other)
        {
            // 获取离开的那个物体的 GameObject
            GameObject go = other.gameObject;
            // 尝试获取该 GameObject 上的实体逻辑组件
            Entity entity = go.GetComponent<Entity>();
            if (entity == null)
            {
                // 如果获取不到，说明这不是一个由框架管理的实体
                Log.Warning("Unknown GameObject '{0}', you must use entity only.", go.name);
                // 作为保险措施，直接销毁这个未知的对象
                Destroy(go);
                return;
            }

            // 如果是框架实体，则调用 HideEntity 将其安全地回收（通常是回收到对象池）
            GameEntry.Entity.HideEntity(entity);
        }
    }
}