using UnityEngine;
using UnityGameFramework.Runtime;

namespace PuddingCat
{
    /// <summary>
    /// 特效的逻辑类。
    /// 负责播放完毕后自动回收。
    /// </summary>
    public class Effect : Entity
    {
        [SerializeField]
        private EffectData m_EffectData = null; // 特效数据

        private float m_ElapseSeconds = 0f; // 计时器，记录特效已显示的时间

#if UNITY_2017_3_OR_NEWER
        protected override void OnShow(object userData)
#else
        protected internal override void OnShow(object userData)
#endif
        {
            base.OnShow(userData);

            m_EffectData = userData as EffectData;
            if (m_EffectData == null)
            {
                Log.Error("Effect data is invalid.");
                return;
            }

            // 每次显示时，重置计时器
            m_ElapseSeconds = 0f;
        }

#if UNITY_2017_3_OR_NEWER
        protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
#else
        protected internal override void OnUpdate(float elapseSeconds, float realElapseSeconds)
#endif
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);

            // 累加时间
            m_ElapseSeconds += elapseSeconds;
            // 如果显示时间达到了数据中定义的持续时间
            if (m_ElapseSeconds >= m_EffectData.KeepTime)
            {
                // 则请求实体管理器隐藏自身（回收到对象池）
                GameEntry.Entity.HideEntity(this);
            }
        }
    }
}