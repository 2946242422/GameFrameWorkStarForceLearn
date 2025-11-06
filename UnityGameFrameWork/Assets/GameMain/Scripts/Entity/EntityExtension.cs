using GameFramework.DataTable;
using System;
using UnityGameFramework.Runtime;

namespace PuddingCat
{
    /// <summary>
    /// 实体相关的扩展方法。
    /// 为 EntityComponent 提供了更高层、数据驱动的封装。
    /// </summary>
    public static class EntityExtension
    {
        // 关于 EntityId 的约定：
        // 0 为无效
        // 正值用于和服务器通信的实体（如玩家角色、NPC、怪等，服务器只产生正值）
        // 负值用于本地生成的临时实体（如特效、子弹等）
        private static int s_SerialId = 0;

        /// <summary>
        /// 扩展方法：获取实体逻辑类实例。
        /// </summary>
        /// <param name="entityComponent">实体组件。</param>
        /// <param name="entityId">实体序列编号。</param>
        /// <returns>实体逻辑类实例。</returns>
        public static Entity GetGameEntity(this EntityComponent entityComponent, int entityId)
        {
            // 从底层获取实体容器
            UnityGameFramework.Runtime.Entity entity = entityComponent.GetEntity(entityId);
            if (entity == null)
            {
                return null;
            }

            // 返回容器中的逻辑组件，并进行类型转换
            return (Entity)entity.Logic;
        }

        /// <summary>
        /// 扩展方法：隐藏实体。
        /// </summary>
        public static void HideEntity(this EntityComponent entityComponent, Entity entity)
        {
            entityComponent.HideEntity(entity.Entity);
        }

        /// <summary>
        /// 扩展方法：挂接实体。
        /// </summary>
        public static void AttachEntity(this EntityComponent entityComponent, Entity entity, int ownerId, string parentTransformPath = null, object userData = null)
        {
            entityComponent.AttachEntity(entity.Entity, ownerId, parentTransformPath, userData);
        }

        // --- 以下是一系列专用于显示特定类型实体的便捷方法 ---

        public static void ShowMyAircraft(this EntityComponent entityComponent, MyAircraftData data)
        {
            // 调用通用的 ShowEntity 方法，并传入特定参数
            entityComponent.ShowEntity(typeof(MyAircraft), "Aircraft", Constant.AssetPriority.MyAircraftAsset, data);
        }

        public static void ShowAircraft(this EntityComponent entityComponent, AircraftData data)
        {
            entityComponent.ShowEntity(typeof(Aircraft), "Aircraft", Constant.AssetPriority.AircraftAsset, data);
        }

        public static void ShowThruster(this EntityComponent entityComponent, ThrusterData data)
        {
            entityComponent.ShowEntity(typeof(Thruster), "Thruster", Constant.AssetPriority.ThrusterAsset, data);
        }

        public static void ShowWeapon(this EntityComponent entityComponent, WeaponData data)
        {
            entityComponent.ShowEntity(typeof(Weapon), "Weapon", Constant.AssetPriority.WeaponAsset, data);
        }

        public static void ShowArmor(this EntityComponent entityComponent, ArmorData data)
        {
            entityComponent.ShowEntity(typeof(Armor), "Armor", Constant.AssetPriority.ArmorAsset, data);
        }

        public static void ShowBullet(this EntityComponent entityCompoennt, BulletData data)
        {
            entityCompoennt.ShowEntity(typeof(Bullet), "Bullet", Constant.AssetPriority.BulletAsset, data);
        }

        public static void ShowAsteroid(this EntityComponent entityCompoennt, AsteroidData data)
        {
            entityCompoennt.ShowEntity(typeof(Asteroid), "Asteroid", Constant.AssetPriority.AsteroiAsset, data);
        }

        public static void ShowEffect(this EntityComponent entityComponent, EffectData data)
        {
            entityComponent.ShowEntity(typeof(Effect), "Effect", Constant.AssetPriority.EffectAsset, data);
        }

        /// <summary>
        /// 内部通用的显示实体方法（数据驱动的核心）。
        /// </summary>
        /// <param name="entityComponent">实体组件。</param>
        /// <param name="logicType">实体逻辑类型。</param>
        /// <param name="entityGroup">实体组。</param>
        /// <param name="priority">资源加载优先级。</param>
        /// <param name="data">实体数据。</param>
        private static void ShowEntity(this EntityComponent entityComponent, Type logicType, string entityGroup, int priority, EntityData data)
        {
            if (data == null)
            {
                Log.Warning("Data is invalid.");
                return;
            }

            // 从数据表中获取 DREntity 的数据表实例
            IDataTable<DREntity> dtEntity = GameEntry.DataTable.GetDataTable<DREntity>();
            // 根据传入数据中的 TypeId，查找对应的数据行
            DREntity drEntity = dtEntity.GetDataRow(data.TypeId);
            if (drEntity == null)
            {
                Log.Warning("Can not load entity id '{0}' from data table.", data.TypeId.ToString());
                return;
            }

            // 从数据行中读取资源名称，并结合 AssetUtility 转换为完整的资源路径，
            // 最后调用底层的 ShowEntity 方法。
            entityComponent.ShowEntity(data.Id, logicType, AssetUtility.GetEntityAsset(drEntity.AssetName), entityGroup, priority, data);
        }

        /// <summary>
        /// 扩展方法：生成一个本地唯一的实体序列编号（负数）。
        /// </summary>
        public static int GenerateSerialId(this EntityComponent entityComponent)
        {
            // 使用--s_SerialId确保返回的是一个负数且每次都不同
            return --s_SerialId;
        }
    }
}