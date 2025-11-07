using GameFramework;
using UnityEditor; // 引入Unity编辑器命名空间
using UnityEngine;

namespace PuddingCat.Editor.DataTableTools
{
    /// <summary>
    /// 数据表生成器的菜单项。
    /// </summary>
    public sealed class DataTableGeneratorMenu
    {
        /// <summary>
        /// 定义一个菜单项，路径为 "Star Force/Generate DataTables"。
        /// </summary>
        [MenuItem("Star Force/Generate DataTables")]
        private static void GenerateDataTables()
        {
            // 遍历在运行时预加载流程中定义的所有数据表名称
            foreach (string dataTableName in ProcedurePreload.DataTableNames)
            {
                // 为每个数据表创建一个处理器实例
                DataTableProcessor dataTableProcessor = DataTableGenerator.CreateDataTableProcessor(dataTableName);
                // 检查原始数据（TXT文件）是否合法
                if (!DataTableGenerator.CheckRawData(dataTableProcessor, dataTableName))
                {
                    Debug.LogError(Utility.Text.Format("Check raw data failure. DataTableName='{0}'", dataTableName));
                    break;
                }

                // 生成二进制数据文件 (.bytes)
                DataTableGenerator.GenerateDataFile(dataTableProcessor, dataTableName);
                // 生成C#代码文件 (.cs)
                DataTableGenerator.GenerateCodeFile(dataTableProcessor, dataTableName);
            }

            // 刷新Unity的资产数据库，使新生成的文件在编辑器中可见
            AssetDatabase.Refresh();
        }
    }
}