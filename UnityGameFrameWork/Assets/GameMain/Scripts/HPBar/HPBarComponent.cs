using GameFramework.ObjectPool;
using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace PuddingCat
{
    /// <summary>
    /// 血条组件，负责管理场景中所有血条的显示、隐藏和回收。
    /// </summary>
    public class HPBarComponent : GameFrameworkComponent
    {
        // 血条UI的Prefab模板
        [SerializeField]
        private HPBarItem m_HPBarItemTemplate = null;

        // 所有血条UI实例的父节点，通常是一个Canvas
        [SerializeField]
        private Transform m_HPBarInstanceRoot = null;

        // 对象池的容量
        [SerializeField]
        private int m_InstancePoolCapacity = 16;
        
        // 血条项对象池
        private IObjectPool<HPBarItemObject> m_HPBarItemObjectPool = null;
        // 当前正在显示的血条列表
        private List<HPBarItem> m_ActiveHPBarItems = null;
        // 缓存父节点的Canvas组件
        private Canvas m_CachedCanvas = null;

        // 组件启动时调用
        private void Start()
        {
            if (m_HPBarInstanceRoot == null)
            {
                Log.Error("You must set HP bar instance root first.");
                return;
            }

            m_CachedCanvas = m_HPBarInstanceRoot.GetComponent<Canvas>();
            // 创建一个单次生成的对象池，用于管理 HPBarItemObject
            m_HPBarItemObjectPool = GameEntry.ObjectPool.CreateSingleSpawnObjectPool<HPBarItemObject>("HPBarItem", m_InstancePoolCapacity);
            m_ActiveHPBarItems = new List<HPBarItem>();
        }
        
        // 组件销毁时调用（在此脚本中为空）
        private void OnDestroy()
        {
        }

        // 每帧调用
        private void Update()
        {
            // 从后向前遍历，因为可能会在循环中移除元素
            for (int i = m_ActiveHPBarItems.Count - 1; i >= 0; i--)
            {
                HPBarItem hpBarItem = m_ActiveHPBarItems[i];
                // 刷新血条位置，如果返回false，说明血条已淡出，需要隐藏
                if (hpBarItem.Refresh())
                {
                    continue; // 如果还在显示，则继续处理下一个
                }

                // 隐藏血条并回收到对象池
                HideHPBar(hpBarItem);
            }
        }

        /// <summary>
        /// 显示血条。
        /// </summary>
        /// <param name="entity">需要显示血条的实体。</param>
        /// <param name="fromHPRatio">血量变化的起始比例。</param>
        /// <param name="toHPRatio">血量变化的目标比例。</param>
        public void ShowHPBar(Entity entity, float fromHPRatio, float toHPRatio)
        {
            if (entity == null)
            {
                Log.Warning("Entity is invalid.");
                return;
            }

            // 检查这个实体是否已经有一个正在显示的血条
            HPBarItem hpBarItem = GetActiveHPBarItem(entity);
            if (hpBarItem == null)
            {
                // 如果没有，则创建一个新的血条并加入到活动列表
                hpBarItem = CreateHPBarItem(entity);
                m_ActiveHPBarItems.Add(hpBarItem);
            }

            // 初始化血条，开始动画
            hpBarItem.Init(entity, m_CachedCanvas, fromHPRatio, toHPRatio);
        }

        /// <summary>
        /// 隐藏血条并将其归还对象池。
        /// </summary>
        private void HideHPBar(HPBarItem hpBarItem)
        {
            hpBarItem.Reset(); // 重置血条状态
            m_ActiveHPBarItems.Remove(hpBarItem); // 从活动列表中移除
            m_HPBarItemObjectPool.Unspawn(hpBarItem); // 将其包装对象归还到对象池
        }

        /// <summary>
        /// 获取指定实体当前正在显示的血条项。
        /// </summary>
        private HPBarItem GetActiveHPBarItem(Entity entity)
        {
            if (entity == null)
            {
                return null;
            }

            foreach (HPBarItem hpBarItem in m_ActiveHPBarItems)
            {
                if (hpBarItem.Owner == entity)
                {
                    return hpBarItem;
                }
            }

            return null;
        }
        
        /// <summary>
        /// 创建一个新的血条项。
        /// </summary>
        private HPBarItem CreateHPBarItem(Entity entity)
        {
            HPBarItem hpBarItem = null;
            // 从对象池中取出一个包装对象
            HPBarItemObject hpBarItemObject = m_HPBarItemObjectPool.Spawn();
            if (hpBarItemObject != null)
            {
                // 如果成功取出，就获取它包裹的血条实例
                hpBarItem = (HPBarItem)hpBarItemObject.Target;
            }
            else
            {
                // 如果对象池为空，则实例化一个新的血条Prefab
                hpBarItem = Instantiate(m_HPBarItemTemplate);
                Transform transform = hpBarItem.GetComponent<Transform>();
                transform.SetParent(m_HPBarInstanceRoot);
                transform.localScale = Vector3.one;
                // 将新创建的血条实例用 HPBarItemObject 包装后，注册到对象池中
                m_HPBarItemObjectPool.Register(HPBarItemObject.Create(hpBarItem), true);
            }

            return hpBarItem;
        }
    }
}