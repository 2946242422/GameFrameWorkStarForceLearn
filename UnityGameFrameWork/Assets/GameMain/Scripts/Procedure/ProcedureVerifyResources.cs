using GameFramework.Event;
using UnityGameFramework.Runtime;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace PuddingCat
{
    /// <summary>
    /// 校验资源流程。
    /// 负责将本地存在的资源与最新的资源版本列表进行比对，检查文件的完整性和正确性。
    /// </summary>
    public class ProcedureVerifyResources : ProcedureBase
    {
        private bool m_VerifyResourcesComplete = false; // 校验是否完成的标志位

        public override bool UseNativeDialog
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// 当进入此流程时调用。
        /// </summary>
        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            // 订阅与资源校验相关的事件，用于在UI上显示详细进度
            GameEntry.Event.Subscribe(ResourceVerifyStartEventArgs.EventId, OnResourceVerifyStart);
            GameEntry.Event.Subscribe(ResourceVerifySuccessEventArgs.EventId, OnResourceVerifySuccess);
            GameEntry.Event.Subscribe(ResourceVerifyFailureEventArgs.EventId, OnResourceVerifyFailure);

            m_VerifyResourcesComplete = false; // 重置完成标志
            
            // 调用资源组件的接口，开始异步校验本地资源。
            // OnVerifyResourcesComplete 是一个回调函数，在整个校验过程结束后被调用。
            GameEntry.Resource.VerifyResources(OnVerifyResourcesComplete);
        }

        /// <summary>
        /// 当离开此流程时调用。
        /// </summary>
        protected override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);

            // 取消订阅事件
            GameEntry.Event.Unsubscribe(ResourceVerifyStartEventArgs.EventId, OnResourceVerifyStart);
            GameEntry.Event.Unsubscribe(ResourceVerifySuccessEventArgs.EventId, OnResourceVerifySuccess);
            GameEntry.Event.Unsubscribe(ResourceVerifyFailureEventArgs.EventId, OnResourceVerifyFailure);
        }

        /// <summary>
        /// 流程轮询（每帧调用）。
        /// </summary>
        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            // 持续检查校验是否已全部完成
            if (!m_VerifyResourcesComplete)
            {
                return; // 未完成则等待
            }

            // 校验完成后，切换到“检查（需要下载的）资源”流程
            ChangeState<ProcedureCheckResources>(procedureOwner);
        }

        /// <summary>
        /// 当整个资源校验过程完成时，由框架调用的回调函数。
        /// </summary>
        private void OnVerifyResourcesComplete(bool result)
        {
            // 将完成标志位置为 true，OnUpdate 将在下一帧检测到并切换流程
            m_VerifyResourcesComplete = true;
            Log.Info("Verify resources complete, result is '{0}'.", result);
        }

        // --- 以下是用于显示详细进度的事件回调 ---

        /// <summary>
        /// 当资源校验开始时，由事件系统触发。
        /// </summary>
        private void OnResourceVerifyStart(object sender, GameEventArgs e)
        {
            ResourceVerifyStartEventArgs ne = (ResourceVerifyStartEventArgs)e;
            // 打印总共需要校验的文件数量和总大小
            Log.Info("Start verify resources, verify resource count '{0}', verify resource total length '{1}'.", ne.Count, ne.TotalLength);
        }

        /// <summary>
        /// 当一个资源文件校验成功时，由事件系统触发。
        /// </summary>
        private void OnResourceVerifySuccess(object sender, GameEventArgs e)
        {
            ResourceVerifySuccessEventArgs ne = (ResourceVerifySuccessEventArgs)e;
            Log.Info("Verify resource '{0}' success.", ne.Name);
        }

        /// <summary>
        /// 当一个资源文件校验失败时（文件损坏或不匹配），由事件系统触发。
        /// 框架会自动删除这个损坏的文件。
        /// </summary>
        private void OnResourceVerifyFailure(object sender, GameEventArgs e)
        {
            ResourceVerifyFailureEventArgs ne = (ResourceVerifyFailureEventArgs)e;
            Log.Warning("Verify resource '{0}' failure.", ne.Name);
        }
    }
}