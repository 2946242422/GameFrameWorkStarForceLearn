using GameFramework;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace PuddingCat
{
    /// <summary>
    /// 武器的逻辑类。
    /// </summary>
    public class Weapon : Entity
    {
        // 约定在父实体模型上的挂点名称
        private const string AttachPoint = "Weapon Point";

        [SerializeField]
        private WeaponData m_WeaponData = null; // 武器的数据

        private float m_NextAttackTime = 0f; // 下一次可以攻击的时间点

#if UNITY_2017_3_OR_NEWER
        protected override void OnShow(object userData)
#else
        protected internal override void OnShow(object userData)
#endif
        {
            base.OnShow(userData);

            m_WeaponData = userData as WeaponData;
            if (m_WeaponData == null)
            {
                Log.Error("Weapon data is invalid.");
                return;
            }

            // 当武器实体被显示时，立刻请求实体管理器将自己附加到主人身上
            GameEntry.Entity.AttachEntity(Entity, m_WeaponData.OwnerId, AttachPoint);
        }

#if UNITY_2017_3_OR_NEWER
        /// <summary>
        /// 当成功附加到父实体上时，此回调被调用。
        /// </summary>
        protected override void OnAttachTo(EntityLogic parentEntity, Transform parentTransform, object userData)
#else
        protected internal override void OnAttachTo(EntityLogic parentEntity, Transform parentTransform, object userData)
#endif
        {
            base.OnAttachTo(parentEntity, parentTransform, userData);

            // 设置一个在编辑器中易于辨认的名字
            Name = Utility.Text.Format("Weapon of {0}", parentEntity.Name);
            // 将自身位置重置，确保紧贴在父实体的挂点上
            CachedTransform.localPosition = Vector3.zero;
        }

        /// <summary>
        /// 尝试攻击。此方法由外部（如玩家飞机的Update）调用。
        /// </summary>
        public void TryAttack()
        {
            // 射速控制：如果当前时间还没到下一次可攻击的时间，则直接返回
            if (Time.time < m_NextAttackTime)
            {
                return;
            }

            // 更新下一次可攻击的时间点
            m_NextAttackTime = Time.time + m_WeaponData.AttackInterval;

            // 请求实体管理器生成一个子弹实体
            GameEntry.Entity.ShowBullet(new BulletData(
                GameEntry.Entity.GenerateSerialId(), // 生成一个新的本地ID
                m_WeaponData.BulletId,          // 使用武器数据中定义的子弹类型ID
                m_WeaponData.OwnerId,           // 告诉子弹它的主人是谁
                m_WeaponData.OwnerCamp,         // 告诉子弹它的主人阵营
                m_WeaponData.Attack,            // 告诉子弹它的攻击力
                m_WeaponData.BulletSpeed)       // 告诉子弹它的速度
            {
                // 设置子弹的初始位置为武器当前的位置
                Position = CachedTransform.position,
            });
            // 播放开火音效
            GameEntry.Sound.PlaySound(m_WeaponData.BulletSoundId);
        }
    }
}