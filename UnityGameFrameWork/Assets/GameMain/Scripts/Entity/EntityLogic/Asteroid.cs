using UnityEngine;
using UnityGameFramework.Runtime;

namespace PuddingCat
{
    /// <summary>
    /// 小行星的逻辑类。
    /// 继承自 TargetableObject，是一个具体的敌人类型。
    /// </summary>
    public class Asteroid : TargetableObject
    {
        [SerializeField]
        private AsteroidData m_AsteroidData = null; // 存储小行星的数据

        // 用于存储一个随机的旋转轴，让每个小行星的翻滚看起来都独一无二
        private Vector3 m_RotateSphere = Vector3.zero;

#if UNITY_2017_3_OR_NEWER
        /// <summary>
        /// 实体初始化。在实体第一次被创建时调用。
        /// </summary>
        protected override void OnInit(object userData)
#else
        protected internal override void OnInit(object userData)
#endif
        {
            // 调用基类的初始化方法
            base.OnInit(userData);
        }

#if UNITY_2017_3_OR_NEWER
        /// <summary>
        /// 当实体显示时调用。
        /// </summary>
        protected override void OnShow(object userData)
#else
        protected internal override void OnShow(object userData)
#endif
        {
            base.OnShow(userData);

            // 接收并保存传入的小行星数据
            m_AsteroidData = userData as AsteroidData;
            if (m_AsteroidData == null)
            {
                Log.Error("Asteroid data is invalid.");
                return;
            }

            // 在每次显示时，都随机生成一个新的旋转轴
            m_RotateSphere = Random.insideUnitSphere;
        }

#if UNITY_2017_3_OR_NEWER
        /// <summary>
        /// 实体轮询（每帧调用）。
        /// </summary>
        protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
#else
        protected internal override void OnUpdate(float elapseSeconds, float realElapseSeconds)
#endif
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);

            // 根据数据中的速度，沿着世界坐标的Z轴负方向（Vector3.back）移动
            CachedTransform.Translate(Vector3.back * m_AsteroidData.Speed * elapseSeconds, Space.World);
            // 根据数据中的角速度和随机生成的旋转轴，进行自身的翻滚
            CachedTransform.Rotate(m_RotateSphere * m_AsteroidData.AngularSpeed * elapseSeconds, Space.Self);
        }

        /// <summary>
        /// 当实体死亡时调用（重写基类方法以添加额外逻辑）。
        /// </summary>
        protected override void OnDead(Entity attacker)
        {
            // 首先执行基类的死亡逻辑（比如隐藏自身）
            base.OnDead(attacker);

            // 然后根据数据，在当前位置生成一个死亡特效
            GameEntry.Entity.ShowEffect(new EffectData(GameEntry.Entity.GenerateSerialId(), m_AsteroidData.DeadEffectId)
            {
                Position = CachedTransform.localPosition,
            });
            // 并根据数据，播放死亡音效
            GameEntry.Sound.PlaySound(m_AsteroidData.DeadSoundId);
        }

        /// <summary>
        /// 获取小行星的冲击数据，用于碰撞和伤害计算。
        /// </summary>
        public override ImpactData GetImpactData()
        {
            // 将自身战斗相关的属性打包成 ImpactData 格式返回
            return new ImpactData(m_AsteroidData.Camp, m_AsteroidData.HP, m_AsteroidData.Attack, 0);
        }
    }
}