using System.Collections;
using System.Collections.Generic;
using GameFramework;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace PuddingCat
{
    /// <summary>
    /// 内置数据组件。
    /// 这个组件用于管理游戏内置的数据，比如构建信息、默认字典等。
    /// </summary>
    public class BuiltinDataComponent : GameFrameworkComponent
    {
        // [SerializeField] 使得私有字段能在Unity Inspector面板中进行赋值

        // 用于存储构建信息的文本资源（通常是JSON格式）
        [SerializeField] private TextAsset m_BuildInfoTextAsset = null;

        // 用于存储默认本地化字典的文本资源
        [SerializeField] private TextAsset m_DefaultDictionaryTextAsset = null;

        // 资源更新界面的UI模板（Prefab）
        [SerializeField] private UpdateResourceForm m_UpdateResourceFormTemplate = null;

        // 存储解析后的构建信息对象
        private BuildInfo m_BuildInfo = null;

        /// <summary>
        /// 获取构建信息。
        /// </summary>
        public BuildInfo BuildInfo
        {
            get { return m_BuildInfo; }
        }

        /// <summary>
        /// 获取资源更新界面的模板。
        /// </summary>
        public UpdateResourceForm UpdateResourceFormTemplate
        {
            get { return m_UpdateResourceFormTemplate; }
        }

        /// <summary>
        /// 初始化构建信息。
        /// </summary>
        public void InitBuildInfo()
        {
            // 检查构建信息文本资源是否有效
            if (m_BuildInfoTextAsset == null || string.IsNullOrEmpty(m_BuildInfoTextAsset.text))
            {
                Log.Info("Build info can not be found or empty."); // 日志：找不到构建信息或内容为空
                return;
            }

            // 使用框架的JSON工具将文本反序列化为BuildInfo对象
            m_BuildInfo = Utility.Json.ToObject<BuildInfo>(m_BuildInfoTextAsset.text);
            if (m_BuildInfo == null)
            {
                // 如果解析失败，则发出警告
                Log.Warning("Parse build info failure."); // 日志：解析构建信息失败
                return;
            }
        }

        /// <summary>
        /// 初始化默认字典。
        /// </summary>
        public void InitDefaultDictionary()
        {
            // 检查默认字典文本资源是否有效
            if (m_DefaultDictionaryTextAsset == null || string.IsNullOrEmpty(m_DefaultDictionaryTextAsset.text))
            {
                Log.Info("Default dictionary can not be found or empty."); // 日志：找不到默认字典或内容为空
                return;
            }

            // 调用 Localization 组件的 ParseData 方法来解析字典数据
            // GameEntry 是框架的统一入口，用于访问各个模块
            if (!GameEntry.Localization.ParseData(m_DefaultDictionaryTextAsset.text))
            {
                // 如果解析失败，则发出警告
                Log.Warning("Parse default dictionary failure."); // 日志：解析默认字典失败
                return;
            }
        }
    }
}