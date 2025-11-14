using GameFramework;
using GameFramework.Event;
using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace PuddingCat
{
    /// <summary>
    /// 更新资源流程。
    /// 负责从服务器下载所有需要更新的资源，并向玩家展示下载进度。
    /// </summary>
    public class ProcedureUpdateResources : ProcedureBase
    {
        private bool m_UpdateResourcesComplete = false; // 所有资源是否更新完成的标志位
        private int m_UpdateCount = 0; // 需要更新的资源总数
        private long m_UpdateTotalCompressedLength = 0L; // 需要更新的资源压缩后总大小
        private int m_UpdateSuccessCount = 0; // 已成功更新的资源数量
        private List<UpdateLengthData> m_UpdateLengthData = new List<UpdateLengthData>(); // 存储每个正在下载资源的进度信息
        private UpdateResourceForm m_UpdateResourceForm = null; // 对更新界面UI的引用

        public override bool UseNativeDialog
        {
            get
            {
                return true; // 在更新阶段，依然使用原生对话框
            }
        }

        /// <summary>
        /// 当进入此流程时调用。
        /// </summary>
        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            // --- 1. 初始化状态 ---
            m_UpdateResourcesComplete = false;
            // 从上一个流程获取需要更新的资源总数和总大小
            m_UpdateCount = procedureOwner.GetData<VarInt32>("UpdateResourceCount");
            procedureOwner.RemoveData("UpdateResourceCount");
            m_UpdateTotalCompressedLength = procedureOwner.GetData<VarInt64>("UpdateResourceTotalCompressedLength");
            procedureOwner.RemoveData("UpdateResourceTotalCompressedLength");
            m_UpdateSuccessCount = 0;
            m_UpdateLengthData.Clear();
            m_UpdateResourceForm = null;

            // --- 2. 订阅事件 ---
            GameEntry.Event.Subscribe(ResourceUpdateStartEventArgs.EventId, OnResourceUpdateStart);
            GameEntry.Event.Subscribe(ResourceUpdateChangedEventArgs.EventId, OnResourceUpdateChanged);
            GameEntry.Event.Subscribe(ResourceUpdateSuccessEventArgs.EventId, OnResourceUpdateSuccess);
            GameEntry.Event.Subscribe(ResourceUpdateFailureEventArgs.EventId, OnResourceUpdateFailure);

            // --- 3. 检查网络环境并决策 ---
            // 如果是移动数据网络
            if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork)
            {
                // 弹出一个对话框，询问玩家是否继续
                GameEntry.UI.OpenDialog(new DialogParams
                {
                    Mode = 2,
                    Title = GameEntry.Localization.GetString("UpdateResourceViaCarrierDataNetwork.Title"),
                    Message = GameEntry.Localization.GetString("UpdateResourceViaCarrierDataNetwork.Message"),
                    ConfirmText = GameEntry.Localization.GetString("UpdateResourceViaCarrierDataNetwork.UpdateButton"),
                    OnClickConfirm = StartUpdateResources, // 玩家点确认后，才开始更新
                    CancelText = GameEntry.Localization.GetString("UpdateResourceViaCarrierDataNetwork.QuitButton"),
                    OnClickCancel = delegate (object userData) { UnityGameFramework.Runtime.GameEntry.Shutdown(ShutdownType.Quit); }, // 玩家点取消，则退出游戏
                });
                return; // 等待玩家选择
            }

            // 如果是WiFi或其他网络，直接开始更新
            StartUpdateResources(null);
        }

        /// <summary>
        /// 当离开此流程时调用。
        /// </summary>
        protected override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            // 销毁更新界面
            if (m_UpdateResourceForm != null)
            {
                Object.Destroy(m_UpdateResourceForm.gameObject);
                m_UpdateResourceForm = null;
            }

            // 取消订阅所有事件
            GameEntry.Event.Unsubscribe(ResourceUpdateStartEventArgs.EventId, OnResourceUpdateStart);
            GameEntry.Event.Unsubscribe(ResourceUpdateChangedEventArgs.EventId, OnResourceUpdateChanged);
            GameEntry.Event.Unsubscribe(ResourceUpdateSuccessEventArgs.EventId, OnResourceUpdateSuccess);
            GameEntry.Event.Unsubscribe(ResourceUpdateFailureEventArgs.EventId, OnResourceUpdateFailure);

            base.OnLeave(procedureOwner, isShutdown);
        }

        /// <summary>
        /// 流程轮询（每帧调用）。
        /// </summary>
        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            // 等待所有资源更新完成
            if (!m_UpdateResourcesComplete)
            {
                return;
            }

            // 更新完成后，切换到预加载流程，准备进入游戏
            ChangeState<ProcedurePreload>(procedureOwner);
        }

        /// <summary>
        /// 开始更新资源。
        /// </summary>
        private void StartUpdateResources(object userData)
        {
            // 如果更新界面还未创建，则实例化一个
            if (m_UpdateResourceForm == null)
            {
                m_UpdateResourceForm = Object.Instantiate(GameEntry.BuiltinData.UpdateResourceFormTemplate);
            }

            Log.Info("Start update resources...");
            // 调用资源组件的接口，开始异步更新所有资源
            GameEntry.Resource.UpdateResources(OnUpdateResourcesComplete);
        }

        /// <summary>
        /// 刷新进度条显示。
        /// </summary>
        private void RefreshProgress()
        {
            long currentTotalUpdateLength = 0L;
            // 累加所有正在下载的文件的当前已下载长度
            for (int i = 0; i < m_UpdateLengthData.Count; i++)
            {
                currentTotalUpdateLength += m_UpdateLengthData[i].Length;
            }

            // 计算总进度百分比
            float progressTotal = (float)currentTotalUpdateLength / m_UpdateTotalCompressedLength;
            // 格式化一个详细的描述字符串
            string descriptionText = GameEntry.Localization.GetString("UpdateResource.Tips", m_UpdateSuccessCount.ToString(), m_UpdateCount.ToString(), GetByteLengthString(currentTotalUpdateLength), GetByteLengthString(m_UpdateTotalCompressedLength), progressTotal, GetByteLengthString((int)GameEntry.Download.CurrentSpeed));
            // 更新UI显示
            m_UpdateResourceForm.SetProgress(progressTotal, descriptionText);
        }

        /// <summary>
        /// 将字节长度转换为可读的字符串（如 KB, MB）。
        /// </summary>
        private string GetByteLengthString(long byteLength)
        {
            if (byteLength < 1024L) return Utility.Text.Format("{0} Bytes", byteLength);
            if (byteLength < 1048576L) return Utility.Text.Format("{0:F2} KB", byteLength / 1024f);
            // ... (其他单位转换)
            return Utility.Text.Format("{0:F2} EB", byteLength / 1152921504606846976f);
        }

        /// <summary>
        /// 当所有资源更新完成时，由框架调用的回调。
        /// </summary>
        private void OnUpdateResourcesComplete(GameFramework.Resource.IResourceGroup resourceGroup, bool result)
        {
            if (result)
            {
                // 如果结果为 true，表示所有资源都成功更新
                m_UpdateResourcesComplete = true;
                Log.Info("Update resources complete with no errors.");
            }
            else
            {
                // 如果结果为 false，表示有部分或全部资源更新失败
                Log.Error("Update resources complete with errors.");
                // 在实际项目中，这里应该弹出一个重试或退出游戏的对话框
            }
        }

        // --- 以下是用于实时更新进度的事件回调 ---

        private void OnResourceUpdateStart(object sender, GameEventArgs e)
        {
            ResourceUpdateStartEventArgs ne = (ResourceUpdateStartEventArgs)e;
            // 当一个新文件开始下载时，添加到进度跟踪列表中
            m_UpdateLengthData.Add(new UpdateLengthData(ne.Name));
        }

        private void OnResourceUpdateChanged(object sender, GameEventArgs e)
        {
            ResourceUpdateChangedEventArgs ne = (ResourceUpdateChangedEventArgs)e;
            // 当一个文件下载进度变化时，更新其已下载长度，并刷新UI
            for (int i = 0; i < m_UpdateLengthData.Count; i++)
            {
                if (m_UpdateLengthData[i].Name == ne.Name)
                {
                    m_UpdateLengthData[i].Length = ne.CurrentLength;
                    RefreshProgress();
                    return;
                }
            }
        }

        private void OnResourceUpdateSuccess(object sender, GameEventArgs e)
        {
            ResourceUpdateSuccessEventArgs ne = (ResourceUpdateSuccessEventArgs)e;
            Log.Info("Update resource '{0}' success.", ne.Name);
            // 当一个文件下载成功时，将其长度标记为最终大小，并增加成功计数，然后刷新UI
            for (int i = 0; i < m_UpdateLengthData.Count; i++)
            {
                if (m_UpdateLengthData[i].Name == ne.Name)
                {
                    m_UpdateLengthData[i].Length = ne.CompressedLength;
                    m_UpdateSuccessCount++;
                    RefreshProgress();
                    return;
                }
            }
        }

        private void OnResourceUpdateFailure(object sender, GameEventArgs e)
        {
            ResourceUpdateFailureEventArgs ne = (ResourceUpdateFailureEventArgs)e;
            // 当一个文件下载失败时
            if (ne.RetryCount >= ne.TotalRetryCount)
            {
                // 如果已达到最大重试次数，则报告严重错误
                Log.Error("Update resource '{0}' failure from '{1}' with error message '{2}', retry count '{3}'.", ne.Name, ne.DownloadUri, ne.ErrorMessage, ne.RetryCount.ToString());
                // 在此可以弹出对话框让玩家重试整个更新流程
                return;
            }
            else
            {
                // 否则，只打印普通日志，框架会自动进行下一次重试
                Log.Info("Update resource '{0}' failure from '{1}' with error message '{2}', retry count '{3}'.", ne.Name, ne.DownloadUri, ne.ErrorMessage, ne.RetryCount.ToString());
            }
        }

        /// <summary>
        /// 用于存储单个正在下载的资源进度的数据结构。
        /// </summary>
        private class UpdateLengthData
        {
            public UpdateLengthData(string name) { m_Name = name; }
            public string Name { get { return m_Name; } }
            public int Length { get; set; }
            private readonly string m_Name;
        }
    }
}