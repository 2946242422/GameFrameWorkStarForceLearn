using GameFramework.Resource;
using UnityGameFramework.Runtime;
// 为状态机持有者类型定义一个别名，方便书写
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace PuddingCat
{
    /// <summary>
    /// 闪屏流程。
    /// 通常用于显示游戏 Logo，并根据不同的资源模式决定下一个流程。
    /// </summary>
    public class ProcedureSplash : ProcedureBase
    {
        /// <summary>
        /// 在此流程中是否使用原生对话框。
        /// </summary>
        public override bool UseNativeDialog
        {
            get
            {
                // 同样，在闪屏阶段，游戏UI系统可能还未完全就绪，使用原生对话框更保险
                return true;
            }
        }

        /// <summary>
        /// 当流程轮询时调用（每帧执行）。
        /// </summary>
        /// <param name="procedureOwner">流程状态机持有者。</param>
        /// <param name="elapseSeconds">自上一帧以来经过的时间（秒）。</param>
        /// <param name="realElapseSeconds">自上一帧以来真实经过的时间（秒）。</param>
        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            // TODO: 这里是播放闪屏动画或显示Logo的理想位置。
            // 例如，可以打开一个UI界面，等待动画播放完毕后再执行后续逻辑。
            // 为了简化示例，这里直接进行模式判断。

            // --- 关键的决策分支 ---
            // 判断当前游戏运行在哪种资源模式下

            if (GameEntry.Base.EditorResourceMode)
            {
                // 1. 编辑器资源模式（在 Unity Editor 中运行，直接读取 Assets 目录）
                Log.Info("Editor resource mode detected.");
                // 无需热更新，直接切换到预加载流程
                ChangeState<ProcedurePreload>(procedureOwner);
            }
            else if (GameEntry.Resource.ResourceMode == ResourceMode.Package)
            {
                // 2. 单机包模式（所有资源都已内置在安装包中，无需联网更新）
                Log.Info("Package resource mode detected.");
                // 切换到初始化本地资源的流程
                // ChangeState<ProcedureInitResources>(procedureOwner);
            }
            else
            {
                // 3. 可更新模式（需要联网检查版本和下载更新资源）
                Log.Info("Updatable resource mode detected.");
                // 切换到检查版本号的流程，开始热更新的第一步
                // ChangeState<ProcedureCheckVersion>(procedureOwner);
            }
        }
    }
}