using GameFramework;
using GameFramework.Event;
using GameFramework.Resource;
using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace PuddingCat
{
    /// <summary>
    /// 预加载流程。
    /// 负责加载游戏启动所必需的配置文件、数据表、本地化字典等全局资源。
    /// </summary>
    public class ProcedurePreload : ProcedureBase
    {
        // 定义需要预加载的所有数据表的名称
        public static readonly string[] DataTableNames = new string[]
        {
            "Aircraft", "Armor", "Asteroid", "Entity", "Music", "Scene", "Sound",
            "Thruster", "UIForm", "UISound", "Weapon", "TestConfig",
        };

        // 一个字典，用作加载任务的“清单”。Key是资源名，Value是是否加载完成的标志。
        private Dictionary<string, bool> m_LoadedFlag = new Dictionary<string, bool>();

        public override bool UseNativeDialog
        {
            get { return true; }
        }

        /// <summary>
        /// 当进入此流程时调用。
        /// </summary>
        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            // 订阅各种资源加载成功/失败的事件
            GameEntry.Event.Subscribe(LoadConfigSuccessEventArgs.EventId, OnLoadConfigSuccess);
            GameEntry.Event.Subscribe(LoadConfigFailureEventArgs.EventId, OnLoadConfigFailure);
            GameEntry.Event.Subscribe(LoadDataTableSuccessEventArgs.EventId, OnLoadDataTableSuccess);
            GameEntry.Event.Subscribe(LoadDataTableFailureEventArgs.EventId, OnLoadDataTableFailure);
            GameEntry.Event.Subscribe(LoadDictionarySuccessEventArgs.EventId, OnLoadDictionarySuccess);
            GameEntry.Event.Subscribe(LoadDictionaryFailureEventArgs.EventId, OnLoadDictionaryFailure);

            m_LoadedFlag.Clear(); // 清空上一轮的加载清单

            PreloadResources(); // 开始执行预加载
        }

        /// <summary>
        /// 当离开此流程时调用。
        /// </summary>
        protected override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            // 取消订阅所有之前订阅的事件，防止内存泄漏
            GameEntry.Event.Unsubscribe(LoadConfigSuccessEventArgs.EventId, OnLoadConfigSuccess);
            GameEntry.Event.Unsubscribe(LoadConfigFailureEventArgs.EventId, OnLoadConfigFailure);
            GameEntry.Event.Unsubscribe(LoadDataTableSuccessEventArgs.EventId, OnLoadDataTableSuccess);
            GameEntry.Event.Unsubscribe(LoadDataTableFailureEventArgs.EventId, OnLoadDataTableFailure);
            GameEntry.Event.Unsubscribe(LoadDictionarySuccessEventArgs.EventId, OnLoadDictionarySuccess);
            GameEntry.Event.Unsubscribe(LoadDictionaryFailureEventArgs.EventId, OnLoadDictionaryFailure);

            base.OnLeave(procedureOwner, isShutdown);
        }

        /// <summary>
        /// 流程轮询（每帧调用）。
        /// </summary>
        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            // 遍历加载清单，检查是否所有任务都已完成
            foreach (KeyValuePair<string, bool> loadedFlag in m_LoadedFlag)
            {
                if (!loadedFlag.Value)
                {
                    // 只要有一个任务未完成，就直接返回，等待下一帧再检查
                    return;
                }
            }

            // 如果循环顺利结束，说明所有资源都已加载完毕
            // 从（刚刚加载好的）配置中读取主菜单场景的ID，并存入流程状态机，供下一个流程使用
            procedureOwner.SetData<VarInt32>("NextSceneId", GameEntry.Config.GetInt("Scene.Menu"));
            // 切换到下一个流程：改变场景
             ChangeState<ProcedureChangeScene>(procedureOwner); // 原代码中被注释，这里是其应有的功能
        }

        /// <summary>
        /// 统一发起所有预加载请求。
        /// </summary>
        private void PreloadResources()
        {
            // 加载配置文件
            LoadConfig("DefaultConfig");

            // 遍历列表，加载所有数据表
            foreach (string dataTableName in DataTableNames)
            {
                LoadDataTable(dataTableName);
            }

            // 加载本地化字典
            LoadDictionary("Default");

            // 加载字体
            LoadFont("MainFont");
        }

        // --- 以下是发起具体加载请求的辅助方法 ---

        private void LoadConfig(string configName)
        {
            string configAssetName = AssetUtility.GetConfigAsset(configName, false);
            m_LoadedFlag.Add(configAssetName, false); // 在清单中添加一个“未完成”的任务
            GameEntry.Config.ReadData(configAssetName, this); // 发起异步加载请求，this作为UserData传递
        }

        private void LoadDataTable(string dataTableName)
        {
            string dataTableAssetName = AssetUtility.GetDataTableAsset(dataTableName, false);
            m_LoadedFlag.Add(dataTableAssetName, false);
            GameEntry.DataTable.LoadDataTable(dataTableName, dataTableAssetName, this);
        }

        private void LoadDictionary(string dictionaryName)
        {
            string dictionaryAssetName = AssetUtility.GetDictionaryAsset(dictionaryName, false);
            Debug.Log($"Attempting to load dictionary from path: {dictionaryAssetName}");

            m_LoadedFlag.Add(dictionaryAssetName, false);
            GameEntry.Localization.ReadData(dictionaryAssetName, this);
        }

        private void LoadFont(string fontName)
        {
            string flagName = Utility.Text.Format("Font.{0}", fontName);
            m_LoadedFlag.Add(flagName, false);
            // 字体资源不属于Config/DataTable等，需要通过通用的资源加载接口加载
            GameEntry.Resource.LoadAsset(AssetUtility.GetFontAsset(fontName), Constant.AssetPriority.FontAsset,
                new LoadAssetCallbacks(
                    // 加载成功回调
                    (assetName, asset, duration, userData) =>
                    {
                        m_LoadedFlag[flagName] = true; // 在清单中标记为“已完成”
                        UGuiForm.SetMainFont((Font)asset); // 设置全局UI字体
                        Log.Info("Load font '{0}' OK.", fontName);
                    },
                    // 加载失败回调
                    (assetName, status, errorMessage, userData) =>
                    {
                        Log.Error("Can not load font '{0}' from '{1}' with error message '{2}'.", fontName, assetName,
                            errorMessage);
                    }));
        }

        // --- 以下是处理事件的回调方法 ---

        private void OnLoadConfigSuccess(object sender, GameEventArgs e)
        {
            LoadConfigSuccessEventArgs ne = (LoadConfigSuccessEventArgs)e;
            // 检查这个事件是否是由本流程的加载请求触发的
            if (ne.UserData != this)
            {
                return;
            }

            m_LoadedFlag[ne.ConfigAssetName] = true; // 在清单中标记为“已完成”
            Log.Info("Load config '{0}' OK.", ne.ConfigAssetName);
        }

        private void OnLoadConfigFailure(object sender, GameEventArgs e)
        {
            LoadConfigFailureEventArgs ne = (LoadConfigFailureEventArgs)e;
            if (ne.UserData != this)
            {
                return;
            }

            Log.Error("Can not load config '{0}' from '{1}' with error message '{2}'.", ne.ConfigAssetName,
                ne.ConfigAssetName, ne.ErrorMessage);
        }

        private void OnLoadDataTableSuccess(object sender, GameEventArgs e)
        {
            LoadDataTableSuccessEventArgs ne = (LoadDataTableSuccessEventArgs)e;
            if (ne.UserData != this)
            {
                return;
            }

            m_LoadedFlag[ne.DataTableAssetName] = true;
            Log.Info("Load data table '{0}' OK.", ne.DataTableAssetName);
        }

        private void OnLoadDataTableFailure(object sender, GameEventArgs e)
        {
            LoadDataTableFailureEventArgs ne = (LoadDataTableFailureEventArgs)e;
            if (ne.UserData != this)
            {
                return;
            }

            Log.Error("Can not load data table '{0}' from '{1}' with error message '{2}'.", ne.DataTableAssetName,
                ne.DataTableAssetName, ne.ErrorMessage);
        }

        private void OnLoadDictionarySuccess(object sender, GameEventArgs e)
        {
            LoadDictionarySuccessEventArgs ne = (LoadDictionarySuccessEventArgs)e;
            if (ne.UserData != this)
            {
                return;
            }

            m_LoadedFlag[ne.DictionaryAssetName] = true;
            Log.Info("Load dictionary '{0}' OK.", ne.DictionaryAssetName);
        }

        private void OnLoadDictionaryFailure(object sender, GameEventArgs e)
        {
            LoadDictionaryFailureEventArgs ne = (LoadDictionaryFailureEventArgs)e;
            if (ne.UserData != this)
            {
                return;
            }

            Log.Error("Can not load dictionary '{0}' from '{1}' with error message '{2}'.", ne.DictionaryAssetName,
                ne.DictionaryAssetName, ne.ErrorMessage);
        }
    }
}