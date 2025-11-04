using System;
using UnityEngine;

namespace PuddingCat
{
    /// <summary>
    /// 实体数据的抽象基类。
    /// 所有具体的实体数据类（如玩家数据、怪物数据）都应从此类继承。
    /// 它定义了一个实体在被创建时所需要的最基本信息。
    /// </summary>
    [Serializable] // [Serializable] 特性允许这个类的实例被Unity序列化，方便在Inspector中查看或存盘
    public abstract class EntityData
    {
        // [SerializeField] 使得私有字段能在Unity Inspector面板中显示和配置
        [SerializeField] private int m_Id = 0; // 实体的唯一ID

        [SerializeField] private int m_TypeId = 0; // 实体的类型ID

        [SerializeField] private Vector3 m_Position = Vector3.zero; // 实体在世界中的初始位置

        [SerializeField] private Quaternion m_Rotation = Quaternion.identity; // 实体在世界中的初始旋转

        /// <summary>
        /// 构造函数，在创建实体数据实例时调用。
        /// </summary>
        /// <param name="entityId">实体编号。</param>
        /// <param name="typeId">实体类型编号。</param>
        public EntityData(int entityId, int typeId)
        {
            m_Id = entityId;
            m_TypeId = typeId;
        }

        /// <summary>
        /// 获取实体编号。
        /// </summary>
        public int Id
        {
            get { return m_Id; }
        }

        /// <summary>
        /// 获取实体类型编号。
        /// </summary>
        public int TypeId
        {
            get { return m_TypeId; }
        }

        /// <summary>
        /// 获取或设置实体位置。
        /// </summary>
        public Vector3 Position
        {
            get { return m_Position; }
            set { m_Position = value; }
        }

        /// <summary>
        /// 获取或设置实体朝向。
        /// </summary>
        public Quaternion Rotation
        {
            get { return m_Rotation; }
            set { m_Rotation = value; }
        }
    }
}