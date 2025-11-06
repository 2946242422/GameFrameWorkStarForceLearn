using UnityEngine;
using UnityGameFramework.Runtime;

namespace PuddingCat
{
    /// <summary>
    /// 玩家飞机的具体逻辑类。
    /// </summary>
    public class MyAircraft : Aircraft
    {
        [SerializeField]
        private MyAircraftData m_MyAircraftData = null; // 玩家飞机专属数据

        private Rect m_PlayerMoveBoundary = default(Rect); // 玩家的可移动区域
        private Vector3 m_TargetPosition = Vector3.zero; // 飞机要飞向的目标位置

#if UNITY_2017_3_OR_NEWER
        protected override void OnInit(object userData)
#else
        protected internal override void OnInit(object userData)
#endif
        {
            base.OnInit(userData);
        }

#if UNITY_2017_3_OR_NEWER
        protected override void OnShow(object userData)
#else
        protected internal override void OnShow(object userData)
#endif
        {
            base.OnShow(userData);

            m_MyAircraftData = userData as MyAircraftData;
            if (m_MyAircraftData == null)
            {
                Log.Error("My aircraft data is invalid.");
                return;
            }

            // 从场景背景中获取玩家可移动的边界信息
            ScrollableBackground sceneBackground = FindObjectOfType<ScrollableBackground>();
            if (sceneBackground == null)
            {
                Log.Warning("Can not find scene background.");
                return;
            }

            // 将3D的BoxCollider边界转换为2D的Rect，方便后续计算
            m_PlayerMoveBoundary = new Rect(sceneBackground.PlayerMoveBoundary.bounds.min.x, sceneBackground.PlayerMoveBoundary.bounds.min.z,
                sceneBackground.PlayerMoveBoundary.bounds.size.x, sceneBackground.PlayerMoveBoundary.bounds.size.z);
        }

#if UNITY_2017_3_OR_NEWER
        protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
#else
        protected internal override void OnUpdate(float elapseSeconds, float realElapseSeconds)
#endif
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);

            // --- 玩家输入与行为控制 ---

            // 如果鼠标左键被按住
            if (Input.GetMouseButton(0))
            {
                // 将鼠标的2D屏幕坐标转换为3D世界坐标
                Vector3 point = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                // 更新飞机的目标位置（Y轴固定为0）
                m_TargetPosition = new Vector3(point.x, 0f, point.z);

                // 遍历所有武器，并尝试开火
                for (int i = 0; i < m_Weapons.Count; i++)
                {
                    m_Weapons[i].TryAttack();
                }
            }

            // --- 移动逻辑 ---

            // 计算当前位置到目标位置的方向向量
            Vector3 direction = m_TargetPosition - CachedTransform.localPosition;
            if (direction.sqrMagnitude <= Vector3.kEpsilon)
            {
                // 如果已经非常接近目标，则不移动
                return;
            }

            // 根据速度和时间，计算本帧应该移动的距离向量
            Vector3 speed = Vector3.ClampMagnitude(direction.normalized * m_MyAircraftData.Speed * elapseSeconds, direction.magnitude);
            
            // 更新飞机位置，并使用Mathf.Clamp将新位置严格限制在移动边界内
            CachedTransform.localPosition = new Vector3
            (
                Mathf.Clamp(CachedTransform.localPosition.x + speed.x, m_PlayerMoveBoundary.xMin, m_PlayerMoveBoundary.xMax),
                0f,
                Mathf.Clamp(CachedTransform.localPosition.z + speed.z, m_PlayerMoveBoundary.yMin, m_PlayerMoveBoundary.yMax)
            );
        }
    }
}