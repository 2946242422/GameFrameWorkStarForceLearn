using UnityEngine;
using UnityGameFramework.Runtime;

namespace PuddingCat
{
    /// <summary>
    /// 子弹的逻辑类。
    /// </summary>
    public class Bullet : Entity
    {
        [SerializeField]
        private BulletData m_BulletData = null; // 子弹的数据

        /// <summary>
        /// 获取子弹的冲击数据，用于碰撞和伤害计算。
        /// </summary>
        public ImpactData GetImpactData()
        {
            // 将子弹数据打包成通用的 ImpactData 格式
            return new ImpactData(m_BulletData.OwnerCamp, 0, m_BulletData.Attack, 0);
        }

#if UNITY_2017_3_OR_NEWER
        protected override void OnShow(object userData)
#else
        protected internal override void OnShow(object userData)
#endif
        {
            base.OnShow(userData);

            // 接收并保存传入的子弹数据
            m_BulletData = userData as BulletData;
            if (m_BulletData == null)
            {
                Log.Error("Bullet data is invalid.");
                return;
            }
        }

#if UNITY_2017_3_OR_NEWER
        protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
#else
        protected internal override void OnUpdate(float elapseSeconds, realElapseSeconds)
#endif
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);

            // 每帧沿着自己的Z轴正方向（Vector3.forward）移动
            // 使用 Space.World 确保是按世界坐标系的方向移动，而不是自身旋转后的方向
            CachedTransform.Translate(Vector3.forward * m_BulletData.Speed * elapseSeconds, Space.World);
        }
    }
}