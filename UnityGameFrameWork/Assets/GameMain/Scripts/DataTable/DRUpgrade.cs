//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------
// 此文件由工具自动生成，请勿直接修改。
// 生成时间：2025-12-04 16:33:26.129
//------------------------------------------------------------

using GameFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace PuddingCat
{
    /// <summary>
    /// 鍗囩骇閰嶇疆。
    /// </summary>
    public class DRUpgrade : DataRowBase
    {
        private int m_Id = 0;

        /// <summary>
        /// 获取鍗囩骇ID。
        /// </summary>
        public override int Id
        {
            get
            {
                return m_Id;
            }
        }

        /// <summary>
        /// 获取鍗囩骇鍚嶇О。
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取鎻忚堪。
        /// </summary>
        public string Description
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取鍗囩骇鎻忚堪。
        /// </summary>
        public int Rarity
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取绋鏈夊害。
        /// </summary>
        public string TypeS
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取鍙傛暟1。
        /// </summary>
        public string Param1
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取鍙傛暟1。
        /// </summary>
        public string Param2
        {
            get;
            private set;
        }

        public override bool ParseDataRow(string dataRowString, object userData)
        {
            string[] columnStrings = dataRowString.Split(DataTableExtension.DataSplitSeparators);
            for (int i = 0; i < columnStrings.Length; i++)
            {
                columnStrings[i] = columnStrings[i].Trim(DataTableExtension.DataTrimSeparators);
            }

            int index = 0;
            index++;
            m_Id = int.Parse(columnStrings[index++]);
            Name = columnStrings[index++];
            Description = columnStrings[index++];
            Rarity = int.Parse(columnStrings[index++]);
            TypeS = columnStrings[index++];
            Param1 = columnStrings[index++];
            Param2 = columnStrings[index++];

            GeneratePropertyArray();
            return true;
        }

        public override bool ParseDataRow(byte[] dataRowBytes, int startIndex, int length, object userData)
        {
            using (MemoryStream memoryStream = new MemoryStream(dataRowBytes, startIndex, length, false))
            {
                using (BinaryReader binaryReader = new BinaryReader(memoryStream, Encoding.UTF8))
                {
                    m_Id = binaryReader.Read7BitEncodedInt32();
                    Name = binaryReader.ReadString();
                    Description = binaryReader.ReadString();
                    Rarity = binaryReader.Read7BitEncodedInt32();
                    TypeS = binaryReader.ReadString();
                    Param1 = binaryReader.ReadString();
                    Param2 = binaryReader.ReadString();
                }
            }

            GeneratePropertyArray();
            return true;
        }

        private KeyValuePair<int, string>[] m_Param = null;

        public int ParamCount
        {
            get
            {
                return m_Param.Length;
            }
        }

        public string GetParam(int id)
        {
            foreach (KeyValuePair<int, string> i in m_Param)
            {
                if (i.Key == id)
                {
                    return i.Value;
                }
            }

            throw new GameFrameworkException(Utility.Text.Format("GetParam with invalid id '{0}'.", id));
        }

        public string GetParamAt(int index)
        {
            if (index < 0 || index >= m_Param.Length)
            {
                throw new GameFrameworkException(Utility.Text.Format("GetParamAt with invalid index '{0}'.", index));
            }

            return m_Param[index].Value;
        }

        private void GeneratePropertyArray()
        {
            m_Param = new KeyValuePair<int, string>[]
            {
                new KeyValuePair<int, string>(1, Param1),
                new KeyValuePair<int, string>(2, Param2),
            };
        }
    }
}
