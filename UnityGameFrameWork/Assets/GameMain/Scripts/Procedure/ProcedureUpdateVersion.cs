using GameFramework.Resource;
using UnityGameFramework.Runtime;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace PuddingCat
{
    /// <summary>
    /// 更新版本列表流程。
    /// 负责从服务器下载最新的资源版本列表文件（VersionList）。
    /// </summary>
    public class ProcedureUpdateVersion : ProcedureBase
    {
        private bool m_UpdateVersionComplete = false; // 更新是否完成的标志位
        private UpdateVersionListCallbacks m_UpdateVersionListCallbacks = null; // 存储更新回调函数的对象

        public override bool UseNativeDialog
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// 当流程初始化时调用（仅在第一次进入此流程时调用一次）。
        /// </summary>
        protected override void OnInit(ProcedureOwner procedureOwner)
        {
            base.OnInit(procedureOwner);

            // 提前创建好回调函数集，避免在OnEnter中重复创建
            m_UpdateVersionListCallbacks = new UpdateVersionListCallbacks(OnUpdateVersionListSuccess, OnUpdateVersionListFailure);
        }

        /// <summary>
        /// 当进入此流程时调用。
        /// </summary>
        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            m_UpdateVersionComplete = false; // 重置完成标志

            // 从上一个流程(ProcedureCheckVersion)传递过来的数据中，获取资源版本列表的四项校验信息
            int versionListLength = procedureOwner.GetData<VarInt32>("VersionListLength");
            int versionListHashCode = procedureOwner.GetData<VarInt32>("VersionListHashCode");
            int versionListCompressedLength = procedureOwner.GetData<VarInt32>("VersionListCompressedLength");
            int versionListCompressedHashCode = procedureOwner.GetData<VarInt32>("VersionListCompressedHashCode");

            // 调用资源组件的接口，开始异步更新版本列表
            // 框架会根据这些校验信息，安全地下载并校验文件
            GameEntry.Resource.UpdateVersionList(versionListLength, versionListHashCode, versionListCompressedLength, versionListCompressedHashCode, m_UpdateVersionListCallbacks);
            
            // 数据使用完毕后，立即从流程状态机中移除，保持状态机干净
            procedureOwner.RemoveData("VersionListLength");
            procedureOwner.RemoveData("VersionListHashCode");
            procedureOwner.RemoveData("VersionListCompressedLength");
            procedureOwner.RemoveData("VersionListCompressedHashCode");
        }

        /// <summary>
        /// 流程轮询（每帧调用）。
        /// </summary>
        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            // 持续检查更新是否完成
            if (!m_UpdateVersionComplete)
            {
                return; // 未完成则等待
            }

            // 更新完成后，切换到“校验资源”流程
            ChangeState<ProcedureVerifyResources>(procedureOwner);
        }

        /// <summary>
        /// 当版本列表更新成功时，由框架调用的回调函数。
        /// </summary>
        private void OnUpdateVersionListSuccess(string downloadPath, string downloadUri)
        {
            // 将完成标志位置为 true，OnUpdate 将在下一帧检测到并切换流程
            m_UpdateVersionComplete = true;
            Log.Info("Update version list from '{0}' success.", downloadUri);
        }

        /// <summary>
        /// 当版本列表更新失败时，由框架调用的回调函数。
        /// </summary>
        private void OnUpdateVersionListFailure(string downloadUri, string errorMessage)
        {
            // 在实际项目中，这里应该弹出一个包含重试按钮的对话框
            Log.Warning("Update version list from '{0}' failure, error message is '{1}'.", downloadUri, errorMessage);
        }
    }
}