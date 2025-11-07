//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using GameFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PuddingCat.Editor.DataTableTools
{
    /// <summary>
    /// 数据表处理器。
    /// 这个类的核心职责是加载并解析一个TXT格式的数据表文件，
    /// 并提供将其内容生成为二进制数据文件和C#代码文件的功能。
    /// 它是数据表生成工具的核心引擎。
    /// </summary>
    public sealed partial class DataTableProcessor
    {
        // TXT文件中，约定以'#'开头的行为注释行
        private const string CommentLineSeparator = "#";
        // TXT文件中，约定使用制表符('\t')作为列的分隔符
        private static readonly char[] DataSplitSeparators = new char[] { '\t' };
        // 在解析每个单元格数据时，需要移除的首尾字符（通常是双引号）
        private static readonly char[] DataTrimSeparators = new char[] { '\"' };

        // --- 从TXT文件中抽取的关键“元数据”行 ---
        private readonly string[] m_NameRow; // 字段名称行
        private readonly string[] m_TypeRow; // 字段类型行
        private readonly string[] m_DefaultValueRow; // 默认值行（可选）
        private readonly string[] m_CommentRow; // 注释行（可选）
        private readonly int m_ContentStartRow; // 正式数据内容开始的行号
        private readonly int m_IdColumn; // “ID”列所在的列号

        // --- 核心处理数据 ---
        private readonly DataProcessor[] m_DataProcessor; // 存储每一列对应的数据处理器实例（如IntProcessor, StringProcessor）
        private readonly string[][] m_RawValues; // 存储从TXT文件读取的、完整的二维字符串数组
        private readonly string[] m_Strings; // 存储数据表中所有不重复的字符串，用于优化

        // --- 代码生成相关 ---
        private string m_CodeTemplate; // 加载的C#代码模板文件内容
        private DataTableCodeGenerator m_CodeGenerator; // 外部注入的代码生成逻辑委托

        /// <summary>
        /// DataTableProcessor 的构造函数，负责加载和解析整个TXT数据表。
        /// </summary>
        /// <param name="dataTableFileName">要处理的TXT数据表文件路径。</param>
        /// <param name="encoding">文件编码。</param>
        /// <param name="nameRow">字段名所在的行号。</param>
        /// <param name="typeRow">字段类型所在的行号。</param>
        /// <param name="defaultValueRow">默认值所在的行号（可选）。</param>
        /// <param name="commentRow">注释所在的行号（可选）。</param>
        /// <param name="contentStartRow">数据内容开始的行号。</param>
        /// <param name="idColumn">ID列的列号。</param>
        public DataTableProcessor(string dataTableFileName, Encoding encoding, int nameRow, int typeRow, int? defaultValueRow, int? commentRow, int contentStartRow, int idColumn)
        {
            // --- 1. 参数校验 ---
            if (string.IsNullOrEmpty(dataTableFileName))
            {
                throw new GameFrameworkException("Data table file name is invalid.");
            }

            if (!dataTableFileName.EndsWith(".txt", StringComparison.Ordinal))
            {
                throw new GameFrameworkException(Utility.Text.Format("Data table file '{0}' is not a txt.", dataTableFileName));
            }

            if (!File.Exists(dataTableFileName))
            {
                throw new GameFrameworkException(Utility.Text.Format("Data table file '{0}' is not exist.", dataTableFileName));
            }

            // --- 2. 读取文件并解析为二维字符串数组 ---
            string[] lines = File.ReadAllLines(dataTableFileName, encoding);
            int rawRowCount = lines.Length;

            int rawColumnCount = 0;
            List<string[]> rawValues = new List<string[]>();
            for (int i = 0; i < lines.Length; i++)
            {
                string[] rawValue = lines[i].Split(DataSplitSeparators);
                for (int j = 0; j < rawValue.Length; j++)
                {
                    // 清理每个单元格数据首尾的双引号
                    rawValue[j] = rawValue[j].Trim(DataTrimSeparators);
                }

                // 以第一行的列数为标准，验证后续每一行的列数是否一致
                if (i == 0)
                {
                    rawColumnCount = rawValue.Length;
                }
                else if (rawValue.Length != rawColumnCount)
                {
                    throw new GameFrameworkException(Utility.Text.Format("Data table file '{0}', raw Column is '{2}', but line '{1}' column is '{3}'.", dataTableFileName, i, rawColumnCount, rawValue.Length));
                }

                rawValues.Add(rawValue);
            }

            m_RawValues = rawValues.ToArray(); // 将List转换为二维数组并保存

            // --- 3. 再次进行参数校验，确保所有指定的行号和列号都是有效的 ---
            if (nameRow < 0)
            {
                throw new GameFrameworkException(Utility.Text.Format("Name row '{0}' is invalid.", nameRow));
            }

            if (typeRow < 0)
            {
                throw new GameFrameworkException(Utility.Text.Format("Type row '{0}' is invalid.", typeRow));
            }

            if (contentStartRow < 0)
            {
                throw new GameFrameworkException(Utility.Text.Format("Content start row '{0}' is invalid.", contentStartRow));
            }

            if (idColumn < 0)
            {
                throw new GameFrameworkException(Utility.Text.Format("Id column '{0}' is invalid.", idColumn));
            }

            if (nameRow >= rawRowCount)
            {
                throw new GameFrameworkException(Utility.Text.Format("Name row '{0}' >= raw row count '{1}' is not allow.", nameRow, rawRowCount));
            }

            if (typeRow >= rawRowCount)
            {
                throw new GameFrameworkException(Utility.Text.Format("Type row '{0}' >= raw row count '{1}' is not allow.", typeRow, rawRowCount));
            }

            if (defaultValueRow.HasValue && defaultValueRow.Value >= rawRowCount)
            {
                throw new GameFrameworkException(Utility.Text.Format("Default value row '{0}' >= raw row count '{1}' is not allow.", defaultValueRow.Value, rawRowCount));
            }

            if (commentRow.HasValue && commentRow.Value >= rawRowCount)
            {
                throw new GameFrameworkException(Utility.Text.Format("Comment row '{0}' >= raw row count '{1}' is not allow.", commentRow.Value, rawRowCount));
            }

            if (contentStartRow > rawRowCount)
            {
                throw new GameFrameworkException(Utility.Text.Format("Content start row '{0}' > raw row count '{1}' is not allow.", contentStartRow, rawRowCount));
            }

            if (idColumn >= rawColumnCount)
            {
                throw new GameFrameworkException(Utility.Text.Format("Id column '{0}' >= raw column count '{1}' is not allow.", idColumn, rawColumnCount));
            }

            // --- 4. 抽取出元数据行 ---
            m_NameRow = m_RawValues[nameRow];
            m_TypeRow = m_RawValues[typeRow];
            m_DefaultValueRow = defaultValueRow.HasValue ? m_RawValues[defaultValueRow.Value] : null;
            m_CommentRow = commentRow.HasValue ? m_RawValues[commentRow.Value] : null;
            m_ContentStartRow = contentStartRow;
            m_IdColumn = idColumn;

            // --- 5. 为每一列创建对应的 DataProcessor ---
            m_DataProcessor = new DataProcessor[rawColumnCount];
            for (int i = 0; i < rawColumnCount; i++)
            {
                if (i == IdColumn)
                {
                    // ID列使用特殊的"id"处理器
                    m_DataProcessor[i] = DataProcessorUtility.GetDataProcessor("id");
                }
                else
                {
                    // 其他列则根据类型行中的字符串（如"int", "string"）获取对应的处理器
                    m_DataProcessor[i] = DataProcessorUtility.GetDataProcessor(m_TypeRow[i]);
                }
            }

            // --- 6. 提取所有字符串并进行排序，用于字符串池优化 ---
            Dictionary<string, int> strings = new Dictionary<string, int>(StringComparer.Ordinal);
            for (int i = contentStartRow; i < rawRowCount; i++)
            {
                if (IsCommentRow(i))
                {
                    continue;
                }

                for (int j = 0; j < rawColumnCount; j++)
                {
                    // 只处理类型为 string 的列
                    if (m_DataProcessor[j].LanguageKeyword != "string")
                    {
                        continue;
                    }

                    string str = m_RawValues[i][j];
                    // 统计每个字符串出现的次数
                    if (strings.ContainsKey(str))
                    {
                        strings[str]++;
                    }
                    else
                    {
                        strings[str] = 1;
                    }
                }
            }

            // 使用LINQ对字符串进行排序：首先按出现频率降序，其次按字母顺序升序
            m_Strings = strings.OrderBy(value => value.Key).OrderByDescending(value => value.Value).Select(value => value.Key).ToArray();

            // 初始化代码生成相关字段
            m_CodeTemplate = null;
            m_CodeGenerator = null;
        }

        /// <summary>
        /// 获取数据表原始行数。
        /// </summary>
        public int RawRowCount
        {
            get
            {
                return m_RawValues.Length;
            }
        }

        /// <summary>
        /// 获取数据表原始列数。
        /// </summary>
        public int RawColumnCount
        {
            get
            {
                return m_RawValues.Length > 0 ? m_RawValues[0].Length : 0;
            }
        }

        /// <summary>
        /// 获取数据表中不重复字符串的数量。
        /// </summary>
        public int StringCount
        {
            get
            {
                return m_Strings.Length;
            }
        }

        /// <summary>
        /// 获取内容起始行号。
        /// </summary>
        public int ContentStartRow
        {
            get
            {
                return m_ContentStartRow;
            }
        }

        /// <summary>
        /// 获取 ID 列的列号。
        /// </summary>
        public int IdColumn
        {
            get
            {
                return m_IdColumn;
            }
        }

        /// <summary>
        /// 判断指定列是否是 ID 列。
        /// </summary>
        public bool IsIdColumn(int rawColumn)
        {
            if (rawColumn < 0 || rawColumn >= RawColumnCount)
            {
                throw new GameFrameworkException(Utility.Text.Format("Raw column '{0}' is out of range.", rawColumn));
            }

            return m_DataProcessor[rawColumn].IsId;
        }

        /// <summary>
        /// 判断指定行是否是注释行。
        /// </summary>
        public bool IsCommentRow(int rawRow)
        {
            if (rawRow < 0 || rawRow >= RawRowCount)
            {
                throw new GameFrameworkException(Utility.Text.Format("Raw row '{0}' is out of range.", rawRow));
            }

            return GetValue(rawRow, 0).StartsWith(CommentLineSeparator, StringComparison.Ordinal);
        }

        /// <summary>
        /// 判断指定列是否是注释列。
        /// </summary>
        public bool IsCommentColumn(int rawColumn)
        {
            if (rawColumn < 0 || rawColumn >= RawColumnCount)
            {
                throw new GameFrameworkException(Utility.Text.Format("Raw column '{0}' is out of range.", rawColumn));
            }

            return string.IsNullOrEmpty(GetName(rawColumn)) || m_DataProcessor[rawColumn].IsComment;
        }

        /// <summary>
        /// 获取指定列的字段名称。
        /// </summary>
        public string GetName(int rawColumn)
        {
            if (rawColumn < 0 || rawColumn >= RawColumnCount)
            {
                throw new GameFrameworkException(Utility.Text.Format("Raw column '{0}' is out of range.", rawColumn));
            }

            if (IsIdColumn(rawColumn))
            {
                return "Id"; // ID列的名称固定为"Id"
            }

            return m_NameRow[rawColumn];
        }

        /// <summary>
        /// 判断指定列的类型是否是 C# 系统内置类型。
        /// </summary>
        public bool IsSystem(int rawColumn)
        {
            if (rawColumn < 0 || rawColumn >= RawColumnCount)
            {
                throw new GameFrameworkException(Utility.Text.Format("Raw column '{0}' is out of range.", rawColumn));
            }

            return m_DataProcessor[rawColumn].IsSystem;
        }

        /// <summary>
        /// 获取指定列的 C# 类型。
        /// </summary>
        public System.Type GetType(int rawColumn)
        {
            if (rawColumn < 0 || rawColumn >= RawColumnCount)
            {
                throw new GameFrameworkException(Utility.Text.Format("Raw column '{0}' is out of range.", rawColumn));
            }

            return m_DataProcessor[rawColumn].Type;
        }

        /// <summary>
        /// 获取指定列类型的 C# 语言关键字（如 "int", "string"）。
        /// </summary>
        public string GetLanguageKeyword(int rawColumn)
        {
            if (rawColumn < 0 || rawColumn >= RawColumnCount)
            {
                throw new GameFrameworkException(Utility.Text.Format("Raw column '{0}' is out of range.", rawColumn));
            }

            return m_DataProcessor[rawColumn].LanguageKeyword;
        }

        /// <summary>
        /// 获取指定列的默认值。
        /// </summary>
        public string GetDefaultValue(int rawColumn)
        {
            if (rawColumn < 0 || rawColumn >= RawColumnCount)
            {
                throw new GameFrameworkException(Utility.Text.Format("Raw column '{0}' is out of range.", rawColumn));
            }

            return m_DefaultValueRow != null ? m_DefaultValueRow[rawColumn] : null;
        }

        /// <summary>
        /// 获取指定列的注释。
        /// </summary>
        public string GetComment(int rawColumn)
        {
            if (rawColumn < 0 || rawColumn >= RawColumnCount)
            {
                throw new GameFrameworkException(Utility.Text.Format("Raw column '{0}' is out of range.", rawColumn));
            }

            return m_CommentRow != null ? m_CommentRow[rawColumn] : null;
        }

        /// <summary>
        /// 获取指定单元格的原始字符串值。
        /// </summary>
        public string GetValue(int rawRow, int rawColumn)
        {
            if (rawRow < 0 || rawRow >= RawRowCount)
            {
                throw new GameFrameworkException(Utility.Text.Format("Raw row '{0}' is out of range.", rawRow));
            }

            if (rawColumn < 0 || rawColumn >= RawColumnCount)
            {
                throw new GameFrameworkException(Utility.Text.Format("Raw column '{0}' is out of range.", rawColumn));
            }

            return m_RawValues[rawRow][rawColumn];
        }

        /// <summary>
        /// 根据索引获取字符串池中的字符串。
        /// </summary>
        public string GetString(int index)
        {
            if (index < 0 || index >= StringCount)
            {
                throw new GameFrameworkException(Utility.Text.Format("String index '{0}' is out of range.", index));
            }

            return m_Strings[index];
        }

        /// <summary>
        /// 获取字符串在字符串池中的索引。
        /// </summary>
        public int GetStringIndex(string str)
        {
            for (int i = 0; i < StringCount; i++)
            {
                if (m_Strings[i] == str)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// 生成二进制数据文件 (.bytes)。
        /// </summary>
        public bool GenerateDataFile(string outputFileName)
        {
            if (string.IsNullOrEmpty(outputFileName))
            {
                throw new GameFrameworkException("Output file name is invalid.");
            }

            try
            {
                // 使用 using 语句确保文件流被正确关闭
                using (FileStream fileStream = new FileStream(outputFileName, FileMode.Create, FileAccess.Write))
                {
                    using (BinaryWriter binaryWriter = new BinaryWriter(fileStream, Encoding.UTF8))
                    {
                        // 遍历所有内容行
                        for (int rawRow = ContentStartRow; rawRow < RawRowCount; rawRow++)
                        {
                            if (IsCommentRow(rawRow))
                            {
                                continue; // 跳过注释行
                            }

                            // 将一整行的数据转换为二进制字节数组
                            byte[] bytes = GetRowBytes(outputFileName, rawRow);
                            binaryWriter.Write7BitEncodedInt32(bytes.Length); // 写入行数据的长度（7-bit编码以节省空间）
                            binaryWriter.Write(bytes); // 写入行数据本身
                        }
                    }
                }

                Debug.Log(Utility.Text.Format("Parse data table '{0}' success.", outputFileName));
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError(Utility.Text.Format("Parse data table '{0}' failure, exception is '{1}'.", outputFileName, exception));
                return false;
            }
        }

        /// <summary>
        /// 加载并设置代码模板文件内容。
        /// </summary>
        public bool SetCodeTemplate(string codeTemplateFileName, Encoding encoding)
        {
            try
            {
                m_CodeTemplate = File.ReadAllText(codeTemplateFileName, encoding);
                Debug.Log(Utility.Text.Format("Set code template '{0}' success.", codeTemplateFileName));
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError(Utility.Text.Format("Set code template '{0}' failure, exception is '{1}'.", codeTemplateFileName, exception));
                return false;
            }
        }

        /// <summary>
        /// 设置外部代码生成器委托。
        /// </summary>
        public void SetCodeGenerator(DataTableCodeGenerator codeGenerator)
        {
            m_CodeGenerator = codeGenerator;
        }

        /// <summary>
        /// 生成C#代码文件 (.cs)。
        /// </summary>
        public bool GenerateCodeFile(string outputFileName, Encoding encoding, object userData = null)
        {
            if (string.IsNullOrEmpty(m_CodeTemplate))
            {
                throw new GameFrameworkException("You must set code template first.");
            }

            if (string.IsNullOrEmpty(outputFileName))
            {
                throw new GameFrameworkException("Output file name is invalid.");
            }

            try
            {
                // 使用模板内容初始化 StringBuilder
                StringBuilder stringBuilder = new StringBuilder(m_CodeTemplate);
                // 如果设置了代码生成器委托，则调用它，让它根据数据表信息来修改/填充 StringBuilder
                if (m_CodeGenerator != null)
                {
                    m_CodeGenerator(this, stringBuilder, userData);
                }

                // 将最终生成的代码字符串写入.cs文件
                using (FileStream fileStream = new FileStream(outputFileName, FileMode.Create, FileAccess.Write))
                {
                    using (StreamWriter stream = new StreamWriter(fileStream, encoding))
                    {
                        stream.Write(stringBuilder.ToString());
                    }
                }

                Debug.Log(Utility.Text.Format("Generate code file '{0}' success.", outputFileName));
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError(Utility.Text.Format("Generate code file '{0}' failure, exception is '{1}'.", outputFileName, exception));
                return false;
            }
        }

        /// <summary>
        /// 将指定的一行数据转换为二进制字节数组。
        /// </summary>
        private byte[] GetRowBytes(string outputFileName, int rawRow)
        {
            // 使用内存流来写入二进制数据，最后一次性转换为字节数组
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream, Encoding.UTF8))
                {
                    // 遍历该行的每一列
                    for (int rawColumn = 0; rawColumn < RawColumnCount; rawColumn++)
                    {
                        if (IsCommentColumn(rawColumn))
                        {
                            continue; // 跳过注释列
                        }

                        try
                        {
                            // 调用该列对应的数据处理器，将字符串值解析并写入二进制流
                            m_DataProcessor[rawColumn].WriteToStream(this, binaryWriter, GetValue(rawRow, rawColumn));
                        }
                        catch
                        {
                            // 如果解析失败
                            if (m_DataProcessor[rawColumn].IsId || string.IsNullOrEmpty(GetDefaultValue(rawColumn)))
                            {
                                // 如果是ID列，或者该列没有配置默认值，则这是一个严重错误，必须报错
                                Debug.LogError(Utility.Text.Format("Parse raw value failure. OutputFileName='{0}' RawRow='{1}' RowColumn='{2}' Name='{3}' Type='{4}' RawValue='{5}'", outputFileName, rawRow, rawColumn, GetName(rawColumn), GetLanguageKeyword(rawColumn), GetValue(rawRow, rawColumn)));
                                return null;
                            }
                            else
                            {
                                // 如果有默认值，则打印警告并尝试使用默认值
                                Debug.LogWarning(Utility.Text.Format("Parse raw value failure, will try default value. OutputFileName='{0}' RawRow='{1}' RowColumn='{2}' Name='{3}' Type='{4}' RawValue='{5}'", outputFileName, rawRow, rawColumn, GetName(rawColumn), GetLanguageKeyword(rawColumn), GetValue(rawRow, rawColumn)));
                                try
                                {
                                    m_DataProcessor[rawColumn].WriteToStream(this, binaryWriter, GetDefaultValue(rawColumn));
                                }
                                catch
                                {
                                    // 如果连默认值都解析失败，则报严重错误
                                    Debug.LogError(Utility.Text.Format("Parse default value failure. OutputFileName='{0}' RawRow='{1}' RowColumn='{2}' Name='{3}' Type='{4}' RawValue='{5}'", outputFileName, rawRow, rawColumn, GetName(rawColumn), GetLanguageKeyword(rawColumn), GetComment(rawColumn)));
                                    return null;
                                }
                            }
                        }
                    }

                    // 返回内存流中的完整字节数组
                    return memoryStream.ToArray();
                }
            }
        }
    }
}