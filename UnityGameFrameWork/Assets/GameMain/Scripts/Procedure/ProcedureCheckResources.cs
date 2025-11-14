using UnityGameFramework.Runtime;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace PuddingCat
{
    /// <summary>
    /// 检查资源流程。
    /// 负责比对最新的资源列表和本地资源，生成需要更新的资源列表。
    /// </summary>
    public class ProcedureCheckResources : ProcedureBase
    {
        private bool m_CheckResourcesComplete = false; // 检查是否完成的标志位
        private bool m_NeedUpdateResources = false; // 是否需要更新资源的标志位
        private int m_UpdateResourceCount = 0; // 需要更新的资源数量
        private long m_UpdateResourceTotalCompressedLength = 0L; // 需要更新的资源压缩后总大小

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

            // 初始化所有状态变量
            m_CheckResourcesComplete = false;
            m_NeedUpdateResources = false;
            m_UpdateResourceCount = 0;
            m_UpdateResourceTotalCompressedLength = 0L;

            // 调用资源组件的接口，开始异步检查需要更新的资源。
            // 这是一个后台操作，框架会比对版本列表，计算出差异。
            // OnCheckResourcesComplete 是在整个检查过程结束后被调用的回调函数。
            GameEntry.Resource.CheckResources(OnCheckResourcesComplete);
        }

        /// <summary>
        /// 流程轮询（每帧调用）。
        /// </summary>
        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            // 持续检查比对工作是否已完成
            if (!m_CheckResourcesComplete)
            {
                return; // 未完成则等待
            }

            // 比对完成后，根据结果进行决策
            if (m_NeedUpdateResources)
            {
                // 如果有资源需要更新
                // 将需要更新的资源数量和总大小存入流程状态机，供下一个流程(ProcedureUpdateResources)使用
                procedureOwner.SetData<VarInt32>("UpdateResourceCount", m_UpdateResourceCount);
                procedureOwner.SetData<VarInt64>("UpdateResourceTotalCompressedLength", m_UpdateResourceTotalCompressedLength);
                // 切换到“更新资源”流程
                ChangeState<ProcedureUpdateResources>(procedureOwner);
            }
            else
            {
                // 如果没有任何资源需要更新，说明本地已是最新版本
                // 直接跳过更新阶段，切换到“预加载”流程，准备进入游戏
                ChangeState<ProcedurePreload>(procedureOwner);
            }
        }

        /// <summary>
        /// 当整个资源检查过程完成时，由框架调用的回调函数。
        /// </summary>
        /// <param name="movedCount">已移动的资源数量。</param>
        /// <param name="removedCount">已移除的资源数量。</param>
        /// <param name="updateCount">需要更新的资源数量。</param>
        /// <param name="updateTotalLength">需要更新的资源未压缩总大小。</param>
        /// <param name="updateTotalCompressedLength">需要更新的资源压缩后总大小。</param>
        private void OnCheckResourcesComplete(int movedCount, int removedCount, int updateCount, long updateTotalLength, long updateTotalCompressedLength)
        {
            // 标记检查工作已完成
            m_CheckResourcesComplete = true;
            // 根据需要更新的资源数量，设置更新标志位
            m_NeedUpdateResources = updateCount > 0;
            // 保存统计结果
            m_UpdateResourceCount = updateCount;
            m_UpdateResourceTotalCompressedLength = updateTotalCompressedLength;
            Log.Info("Check resources complete, '{0}' resources need to update, compressed length is '{1}', uncompressed length is '{2}'.", updateCount.ToString(), updateTotalCompressedLength.ToString(), updateTotalLength.ToString());
        }
    }
}