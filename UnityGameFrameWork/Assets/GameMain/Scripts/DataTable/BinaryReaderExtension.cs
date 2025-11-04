using System;
using System.IO; // 引入 System.IO 命名空间，BinaryReader 在这里定义
using UnityEngine;

namespace PuddingCat
{
    /// <summary>
    /// BinaryReader 类的扩展方法。
    /// 这个静态类为 System.IO.BinaryReader 添加了一系列新方法，
    /// 使其能够直接从二进制流中读取 Unity 的常用数据类型。
    /// </summary>
    public static class BinaryReaderExtension
    {
        /// <summary>
        /// 从二进制流中读取 Color32。
        /// </summary>
        /// <param name="binaryReader">要扩展的 BinaryReader 实例 (this 关键字表明这是扩展方法)。</param>
        /// <returns>读取到的 Color32 对象。</returns>
        public static Color32 ReadColor32(this BinaryReader binaryReader)
        {
            // Color32 由 4 个 byte (R, G, B, A) 组成
            return new Color32(binaryReader.ReadByte(), binaryReader.ReadByte(), binaryReader.ReadByte(),
                binaryReader.ReadByte());
        }

        /// <summary>
        /// 从二进制流中读取 Color。
        /// </summary>
        public static Color ReadColor(this BinaryReader binaryReader)
        {
            // Color 由 4 个 float (R, G, B, A) 组成
            // ReadSingle() 用于读取单精度浮点数 (float)
            return new Color(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle(),
                binaryReader.ReadSingle());
        }

        /// <summary>
        /// 从二进制流中读取 DateTime。
        /// </summary>
        public static DateTime ReadDateTime(this BinaryReader binaryReader)
        {
            // DateTime 通常被序列化为一个长整型 (long / Int64)，代表 Ticks (时间刻度)
            return new DateTime(binaryReader.ReadInt64());
        }

        /// <summary>
        /// 从二进制流中读取 Quaternion。
        /// </summary>
        public static Quaternion ReadQuaternion(this BinaryReader binaryReader)
        {
            // Quaternion 由 4 个 float (x, y, z, w) 组成
            return new Quaternion(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle(),
                binaryReader.ReadSingle());
        }

        /// <summary>
        /// 从二进制流中读取 Rect。
        /// </summary>
        public static Rect ReadRect(this BinaryReader binaryReader)
        {
            // Rect 由 4 个 float (x, y, width, height) 组成
            return new Rect(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle(),
                binaryReader.ReadSingle());
        }

        /// <summary>
        /// 从二进制流中读取 Vector2。
        /// </summary>
        public static Vector2 ReadVector2(this BinaryReader binaryReader)
        {
            // Vector2 由 2 个 float (x, y) 组成
            return new Vector2(binaryReader.ReadSingle(), binaryReader.ReadSingle());
        }

        /// <summary>
        /// 从二进制流中读取 Vector3。
        /// </summary>
        public static Vector3 ReadVector3(this BinaryReader binaryReader)
        {
            // Vector3 由 3 个 float (x, y, z) 组成
            return new Vector3(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
        }

        /// <summary>
        /// 从二进制流中读取 Vector4。
        /// </summary>
        public static Vector4 ReadVector4(this BinaryReader binaryReader)
        {
            // Vector4 由 4 个 float (x, y, z, w) 组成
            return new Vector4(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle(),
                binaryReader.ReadSingle());
        }
    }
}