using GameFramework.DataTable;
using System;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace PuddingCat
{
    /// <summary>
    /// 数据表相关的扩展方法和实用工具。
    /// </summary>
    public static class DataTableExtension
    {
        // 数据行类（DataRow Class）的固定命名空间和前缀
        // 注意：这里是 StarForce.DR，如果你的项目命名空间不同，需要修改这里
        private const string DataRowClassPrefixName = "PuddingCat.DR";

        // 数据表行内字段的分隔符，这里定义为制表符（Tab）
        internal static readonly char[] DataSplitSeparators = new char[] { '\t' };

        // 数据表字段内容需要移除的首尾字符，这里定义为双引号
        internal static readonly char[] DataTrimSeparators = new char[] { '\"' };

        /// <summary>
        /// 加载数据表的扩展方法。
        /// </summary>
        /// <param name="dataTableComponent">要扩展的 DataTableComponent 实例 (this 关键字表明这是扩展方法)。</param>
        /// <param name="dataTableName">数据表的逻辑名称，需遵循 "类型名_实例名" 或 "类型名" 的格式。</param>
        /// <param name="dataTableAssetName">数据表资源的完整路径。</param>
        /// <param name="userData">用户自定义数据，会传递给加载回调。</param>
        public static void LoadDataTable(this DataTableComponent dataTableComponent, string dataTableName,
            string dataTableAssetName, object userData)
        {
            if (string.IsNullOrEmpty(dataTableName))
            {
                Log.Warning("Data table name is invalid.");
                return;
            }

            // 根据下划线 '_' 分割数据表逻辑名称
            string[] splitedNames = dataTableName.Split('_');
            if (splitedNames.Length > 2)
            {
                Log.Warning("Data table name is invalid.");
                return;
            }

            // 构造数据行类（DataRow）的完整类名
            // 例如，如果 dataTableName 是 "Aircraft_Player", splitedNames[0] 是 "Aircraft",
            // 那么 dataRowClassName 就是 "StarForce.DRAircraft"
            string dataRowClassName = DataRowClassPrefixName + splitedNames[0];
            // 使用 C# 反射，根据类名字符串获取对应的 Type 对象
            Type dataRowType = Type.GetType(dataRowClassName);
            if (dataRowType == null)
            {
                Log.Warning("Can not get data row type with class name '{0}'.", dataRowClassName);
                return;
            }

            // 获取实例名（如果存在的话），例如 "Player"
            string name = splitedNames.Length > 1 ? splitedNames[1] : null;
            // 创建数据表实例
            DataTableBase dataTable = dataTableComponent.CreateDataTable(dataRowType, name);
            // 读取数据表资源文件
            dataTable.ReadData(dataTableAssetName, Constant.AssetPriority.DataTableAsset, userData);
        }

        // --- 以下是一系列将字符串解析为Unity常用数据类型的工具方法 ---
        /// <summary>
        /// 将字符串解析为 Color32。
        /// 字符串格式应为 "R,G,B,A"。
        /// </summary>
        public static Color32 ParseColor32(string value)
        {
            string[] splitedValue = value.Split(',');
            return new Color32(byte.Parse(splitedValue[0]), byte.Parse(splitedValue[1]), byte.Parse(splitedValue[2]),
                byte.Parse(splitedValue[3]));
        }

        /// <summary>
        /// 将字符串解析为 Color。
        /// 字符串格式应为 "R,G,B,A"。
        /// </summary>
        public static Color ParseColor(string value)
        {
            string[] splitedValue = value.Split(',');
            return new Color(float.Parse(splitedValue[0]), float.Parse(splitedValue[1]), float.Parse(splitedValue[2]),
                float.Parse(splitedValue[3]));
        }

        /// <summary>
        /// 将字符串解析为 Quaternion。
        /// 字符串格式应为 "X,Y,Z,W"。
        /// </summary>
        public static Quaternion ParseQuaternion(string value)
        {
            string[] splitedValue = value.Split(',');
            return new Quaternion(float.Parse(splitedValue[0]), float.Parse(splitedValue[1]),
                float.Parse(splitedValue[2]), float.Parse(splitedValue[3]));
        }

        /// <summary>
        /// 将字符串解析为 Rect。
        /// 字符串格式应为 "X,Y,Width,Height"。
        /// </summary>
        public static Rect ParseRect(string value)
        {
            string[] splitedValue = value.Split(',');
            return new Rect(float.Parse(splitedValue[0]), float.Parse(splitedValue[1]), float.Parse(splitedValue[2]),
                float.Parse(splitedValue[3]));
        }

        /// <summary>
        /// 将字符串解析为 Vector2。
        /// 字符串格式应为 "X,Y"。
        /// </summary>
        public static Vector2 ParseVector2(string value)
        {
            string[] splitedValue = value.Split(',');
            return new Vector2(float.Parse(splitedValue[0]), float.Parse(splitedValue[1]));
        }

        /// <summary>
        /// 将字符串解析为 Vector3。
        /// 字符串格式应为 "X,Y,Z"。
        /// </summary>
        public static Vector3 ParseVector3(string value)
        {
            string[] splitedValue = value.Split(',');
            return new Vector3(float.Parse(splitedValue[0]), float.Parse(splitedValue[1]),
                float.Parse(splitedValue[2]));
        }

        /// <summary>
        /// 将字符串解析为 Vector4。
        /// 字符串格式应为 "X,Y,Z,W"。
        /// </summary>
        public static Vector4 ParseVector4(string value)
        {
            string[] splitedValue = value.Split(',');
            return new Vector4(float.Parse(splitedValue[0]), float.Parse(splitedValue[1]), float.Parse(splitedValue[2]),
                float.Parse(splitedValue[3]));
        }
    }
}