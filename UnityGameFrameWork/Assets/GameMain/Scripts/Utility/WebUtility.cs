//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;

namespace PuddingCat
{
    /// <summary>
    /// Web 相关的实用工具类。
    /// 提供 URL 编码和解码的功能。
    /// </summary>
    public static class WebUtility
    {
        /// <summary>
        /// 对字符串进行 URL 编码（也称为“转义”）。
        /// 这会将字符串中的特殊字符（如空格, &, ?, =）转换为 %XX 的安全格式，
        /// 以便可以安全地将其作为 URL 的一部分进行传输。
        /// </summary>
        /// <param name="stringToEscape">要进行编码的原始字符串。</param>
        /// <returns>经过 URL 编码后的安全字符串。</returns>
        public static string EscapeString(string stringToEscape)
        {
            // 内部调用 .NET 框架提供的标准 URL 数据编码方法。
            return Uri.EscapeDataString(stringToEscape);
        }

        /// <summary>
        /// 对经过 URL 编码的字符串进行解码（也称为“反转义”）。
        /// 这是 EscapeString 的逆向操作。
        /// </summary>
        /// <param name="stringToUnescape">要进行解码的、已经过 URL 编码的字符串。</param>
        /// <returns>解码后的原始字符串。</returns>
        public static string UnescapeString(string stringToUnescape)
        {
            // 内部调用 .NET 框架提供的标准 URL 数据解码方法。
            return Uri.UnescapeDataString(stringToUnescape);
        }
    }
}