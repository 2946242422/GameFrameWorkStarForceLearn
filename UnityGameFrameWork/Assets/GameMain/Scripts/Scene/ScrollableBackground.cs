using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 可滚动的背景脚本。
/// 适用于经典的飞行射击游戏，通过重复移动背景贴图来营造无限前进的视觉效果。
/// </summary>
public class ScrollableBackground : MonoBehaviour
{
    [SerializeField]
    private float m_ScrollSpeed = -0.25f; // 背景滚动的速度
    [SerializeField]
    private float m_TileSize = 30f; // 背景贴图的尺寸（长度），当移动超过这个距离时会重置
    
    // --- 各种边界定义，通过 BoxCollider 在场景中可视化地编辑 ---
    [SerializeField]
    private BoxCollider m_VisibleBoundary = null; // 摄像机可见区域的边界
    [SerializeField]
    private BoxCollider m_PlayerMoveBoundary = null; // 玩家飞船可移动的区域边界
    [SerializeField]
    private BoxCollider m_EnemySpawnBoundary = null; // 敌人生成的区域边界

    private Transform m_CachedTransform = null; // 缓存自身 Transform 组件，用于优化性能
    private Vector3 m_StartPosition = Vector3.zero; // 记录背景的初始位置

    private void Start()
    {
        // 在 Start 方法中获取并缓存组件和初始位置
        m_CachedTransform = transform;
        m_StartPosition = m_CachedTransform.position;
    }

    // 每帧调用
    void Update()
    {
        // 使用 Mathf.Repeat 函数创建一个循环效果。
        // Time.time * m_ScrollSpeed 计算出随时间推移的总位移。
        // Mathf.Repeat 会将这个位移值限制在 0 到 m_TileSize 之间，形成一个循环。
        float newPosition = Mathf.Repeat(Time.time * m_ScrollSpeed, m_TileSize);
        // 将计算出的循环位移应用到Z轴上，实现背景的无限滚动。
        m_CachedTransform.position = m_StartPosition + Vector3.forward * newPosition;
    }

    /// <summary>
    /// 获取可见区域边界。
    /// </summary>
    public BoxCollider VisibleBoundary
    {
        get
        {
            return m_VisibleBoundary;
        }
    }

    /// <summary>
    /// 获取玩家可移动区域边界。
    /// </summary>
    public BoxCollider PlayerMoveBoundary
    {
        get
        {
            return m_PlayerMoveBoundary;
        }
    }

    /// <summary>
    /// 获取敌人生成区域边界。
    /// </summary>
    public BoxCollider EnemySpawnBoundary
    {
        get
        {
            return m_EnemySpawnBoundary;
        }
    }
}