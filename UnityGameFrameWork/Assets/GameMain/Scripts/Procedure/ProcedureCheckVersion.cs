//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using GameFramework;
using GameFramework.Event;
using GameFramework.Resource;
using UnityEngine;
using UnityGameFramework.Runtime;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace PuddingCat
{
    /// <summary>
    /// 检查版本流程。
    /// 热更新流程的起点，负责向服务器请求最新的版本信息。
    /// </summary>
    public class ProcedureCheckVersion : ProcedureBase
    {
        private bool m_CheckVersionComplete = false; // 检查版本是否完成的标志位
        private bool m_NeedUpdateVersion = false; // 是否需要更新版本的标志位
        private VersionInfo m_VersionInfo = null; // 存储从服务器获取的版本信息

        public override bool UseNativeDialog
        {
            get
            {
                // 在这个阶段，UI系统可能还未初始化，使用原生对话框更可靠
                return true;
            }
        }

        /// <summary>
        /// 当进入此流程时调用。
        /// </summary>
        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            // 初始化标志位
            m_CheckVersionComplete = false;
            m_NeedUpdateVersion = false;
            m_VersionInfo = null;

            // 订阅网络请求成功/失败事件
            GameEntry.Event.Subscribe(WebRequestSuccessEventArgs.EventId, OnWebRequestSuccess);
            GameEntry.Event.Subscribe(WebRequestFailureEventArgs.EventId, OnWebRequestFailure);

            // 构建版本检查URL并发起网络请求
            // GetPlatformPath() 会根据当前平台（Windows/Android/IOS）返回对应的字符串
            GameEntry.WebRequest.AddWebRequest(Utility.Text.Format(GameEntry.BuiltinData.BuildInfo.CheckVersionUrl, GetPlatformPath()), this);
        }

        /// <summary>
        /// 当离开此流程时调用。
        /// </summary>
        protected override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            // 取消订阅事件
            GameEntry.Event.Unsubscribe(WebRequestSuccessEventArgs.EventId, OnWebRequestSuccess);
            GameEntry.Event.Unsubscribe(WebRequestFailureEventArgs.EventId, OnWebRequestFailure);

            base.OnLeave(procedureOwner, isShutdown);
        }

        /// <summary>
        /// 流程轮询（每帧调用）。
        /// </summary>
        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            // 等待版本检查完成
            if (!m_CheckVersionComplete)
            {
                return;
            }

            // 版本检查完成后，根据结果切换到下一个流程
            if (m_NeedUpdateVersion)
            {
                // 如果需要更新，将版本列表的校验信息存入流程状态机，供下一流程使用
                procedureOwner.SetData<VarInt32>("VersionListLength", m_VersionInfo.VersionListLength);
                procedureOwner.SetData<VarInt32>("VersionListHashCode", m_VersionInfo.VersionListHashCode);
                procedureOwner.SetData<VarInt32>("VersionListCompressedLength", m_VersionInfo.VersionListCompressedLength);
                procedureOwner.SetData<VarInt32>("VersionListCompressedHashCode", m_VersionInfo.VersionListCompressedHashCode);
                // 切换到“更新版本”流程
                ChangeState<ProcedureUpdateVersion>(procedureOwner);
            }
            else
            {
                // 如果不需要更新，则切换到“校验资源”流程
                ChangeState<ProcedureVerifyResources>(procedureOwner);
            }
        }

        /// <summary>
        /// 跳转到应用商店进行更新的回调方法。
        /// </summary>
        private void GotoUpdateApp(object userData)
        {
            string url = null;
            // 使用宏定义，根据不同平台获取对应的商店URL
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            url = GameEntry.BuiltinData.BuildInfo.WindowsAppUrl;
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            url = GameEntry.BuiltinData.BuildInfo.MacOSAppUrl;
#elif UNITY_IOS
            url = GameEntry.BuiltinData.BuildInfo.IOSAppUrl;
#elif UNITY_ANDROID
            url = GameEntry.BuiltinData.BuildInfo.AndroidAppUrl;
#endif
            if (!string.IsNullOrEmpty(url))
            {
                // 打开URL
                Application.OpenURL(url);
            }
        }

        /// <summary>
        /// 当网络请求成功时调用的回调。
        /// </summary>
        private void OnWebRequestSuccess(object sender, GameEventArgs e)
        {
            WebRequestSuccessEventArgs ne = (WebRequestSuccessEventArgs)e;
            if (ne.UserData != this) return; // 校验是否是本流程发起的请求

            // 解析版本信息
            byte[] versionInfoBytes = ne.GetWebResponseBytes();
            string versionInfoString = Utility.Converter.GetString(versionInfoBytes);
            m_VersionInfo = Utility.Json.ToObject<VersionInfo>(versionInfoString);
            if (m_VersionInfo == null)
            {
                Log.Error("Parse VersionInfo failure.");
                return;
            }

            Log.Info("Latest game version is '{0} ({1})', local game version is '{2} ({3})'.", m_VersionInfo.LatestGameVersion, m_VersionInfo.InternalGameVersion.ToString(), Version.GameVersion, Version.InternalGameVersion.ToString());

            // 决策一：检查是否需要强制更新App
            if (m_VersionInfo.ForceUpdateGame)
            {
                // 打开一个强制更新的对话框
                GameEntry.UI.OpenDialog(new DialogParams
                {
                    Mode = 2, // 两个按钮
                    Title = GameEntry.Localization.GetString("ForceUpdate.Title"),
                    Message = GameEntry.Localization.GetString("ForceUpdate.Message"),
                    ConfirmText = GameEntry.Localization.GetString("ForceUpdate.UpdateButton"),
                    OnClickConfirm = GotoUpdateApp, // 确认按钮跳转到商店
                    CancelText = GameEntry.Localization.GetString("ForceUpdate.QuitButton"),
                    OnClickCancel = delegate (object userData) { UnityGameFramework.Runtime.GameEntry.Shutdown(ShutdownType.Quit); }, // 取消按钮直接退出游戏
                });
                return; // 流程中断，等待玩家操作
            }

            // 如果不需要强更，则设置资源更新的URL前缀
            GameEntry.Resource.UpdatePrefixUri = Utility.Path.GetRegularPath(m_VersionInfo.UpdatePrefixUri);

            // 决策二：检查是否需要热更新资源
            m_CheckVersionComplete = true; // 标记版本检查已完成
            // 调用框架接口，比较服务器资源版本号和本地资源版本号
            m_NeedUpdateVersion = GameEntry.Resource.CheckVersionList(m_VersionInfo.InternalResourceVersion) == CheckVersionListResult.NeedUpdate;
        }

        /// <summary>
        /// 当网络请求失败时调用的回调。
        /// </summary>
        private void OnWebRequestFailure(object sender, GameEventArgs e)
        {
            WebRequestFailureEventArgs ne = (WebRequestFailureEventArgs)e;
            if (ne.UserData != this) return;

            Log.Warning("Check version failure, error message is '{0}'.", ne.ErrorMessage);
            // 在实际项目中，这里应该弹出一个重试对话框
        }

        /// <summary>
        /// 根据当前运行平台获取对应的平台路径字符串。
        /// </summary>
        private string GetPlatformPath()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                    return "Windows";

                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                    return "MacOS";

                case RuntimePlatform.IPhonePlayer:
                    return "IOS";

                case RuntimePlatform.Android:
                    return "Android";

                default:
                    throw new System.NotSupportedException(Utility.Text.Format("Platform '{0}' is not supported.", Application.platform));
            }
        }
    }
}