using GameFramework.DataTable;
using GameFramework.Event;
using UnityGameFramework.Runtime;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace PuddingCat
{
    /// <summary>
    /// 切换场景流程。
    /// 负责清理旧场景、加载新场景，并在加载完成后切换到对应的游戏逻辑流程。
    /// </summary>
    public class ProcedureChangeScene : ProcedureBase
    {
        private const int MenuSceneId = 1; // 约定主菜单场景的ID为1

        private bool m_ChangeToMenu = false; // 标志位：是否要切换到主菜单流程
        private bool m_IsChangeSceneComplete = false; // 标志位：场景是否已加载完成
        private int m_BackgroundMusicId = 0; // 目标场景需要播放的背景音乐ID

        /// <summary>
        /// 在此流程中是否使用原生对话框。
        /// </summary>
        public override bool UseNativeDialog
        {
            get
            {
                // 此时游戏已初始化完毕，可以使用游戏内自己的UI对话框了
                return false;
            }
        }

        /// <summary>
        /// 当进入此流程时调用。
        /// </summary>
        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            m_IsChangeSceneComplete = false; // 初始化完成标志

            // 1. 订阅与场景加载相关的事件
            GameEntry.Event.Subscribe(LoadSceneSuccessEventArgs.EventId, OnLoadSceneSuccess);
            GameEntry.Event.Subscribe(LoadSceneFailureEventArgs.EventId, OnLoadSceneFailure);
            GameEntry.Event.Subscribe(LoadSceneUpdateEventArgs.EventId, OnLoadSceneUpdate);
            GameEntry.Event.Subscribe(LoadSceneDependencyAssetEventArgs.EventId, OnLoadSceneDependencyAsset);

            // --- 2. 清理当前环境，为加载新场景做准备 ---
            // 停止所有声音
            GameEntry.Sound.StopAllLoadingSounds();
            GameEntry.Sound.StopAllLoadedSounds();
            // 隐藏所有实体
            GameEntry.Entity.HideAllLoadingEntities();
            GameEntry.Entity.HideAllLoadedEntities();
            // 卸载所有已加载的场景
            string[] loadedSceneAssetNames = GameEntry.Scene.GetLoadedSceneAssetNames();
            for (int i = 0; i < loadedSceneAssetNames.Length; i++)
            {
                GameEntry.Scene.UnloadScene(loadedSceneAssetNames[i]);
            }
            // 还原游戏速度
            GameEntry.Base.ResetNormalGameSpeed();

            // --- 3. 开始加载新场景 ---
            // 从流程状态机中获取上一个流程传递过来的 "NextSceneId"
            int sceneId = procedureOwner.GetData<VarInt32>("NextSceneId");
            // 判断目标场景是否是主菜单
            m_ChangeToMenu = sceneId == MenuSceneId;

            // 从数据表中读取场景配置
            IDataTable<DRScene> dtScene = GameEntry.DataTable.GetDataTable<DRScene>();
            DRScene drScene = dtScene.GetDataRow(sceneId);
            if (drScene == null)
            {
                Log.Warning("Can not load scene '{0}' from data table.", sceneId.ToString());
                return;
            }

            // 发起异步加载场景的请求
            GameEntry.Scene.LoadScene(AssetUtility.GetSceneAsset(drScene.AssetName), Constant.AssetPriority.SceneAsset, this);
            // 记录该场景需要播放的背景音乐ID
            m_BackgroundMusicId = drScene.BackgroundMusicId;
        }

        /// <summary>
        /// 当离开此流程时调用。
        /// </summary>
        protected override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            // 取消订阅所有事件，好习惯
            GameEntry.Event.Unsubscribe(LoadSceneSuccessEventArgs.EventId, OnLoadSceneSuccess);
            GameEntry.Event.Unsubscribe(LoadSceneFailureEventArgs.EventId, OnLoadSceneFailure);
            GameEntry.Event.Unsubscribe(LoadSceneUpdateEventArgs.EventId, OnLoadSceneUpdate);
            GameEntry.Event.Unsubscribe(LoadSceneDependencyAssetEventArgs.EventId, OnLoadSceneDependencyAsset);

            base.OnLeave(procedureOwner, isShutdown);
        }



        /// <summary>
        /// 流程轮询（每帧调用）。
        /// </summary>
        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            // 持续检查场景是否加载完成
            if (!m_IsChangeSceneComplete)
            {
                return; // 未完成则等待
            }

            // 加载完成后，根据目标场景类型，切换到对应的游戏逻辑流程
            if (m_ChangeToMenu)
            {
                // ChangeState<ProcedureMenu>(procedureOwner);
            }
            else
            {
                // ChangeState<ProcedureMain>(procedureOwner); // 例如，切换到主战斗流程
            }
        }

        // --- 以下是事件回调处理 ---

        private void OnLoadSceneSuccess(object sender, GameEventArgs e)
        {
            LoadSceneSuccessEventArgs ne = (LoadSceneSuccessEventArgs)e;
            // 校验 UserData，确保是本流程发起的加载
            if (ne.UserData != this)
            {
                return;
            }

            Log.Info("Load scene '{0}' OK.", ne.SceneAssetName);

            // 如果配置了背景音乐，则播放
            if (m_BackgroundMusicId > 0)
            {
                GameEntry.Sound.PlayMusic(m_BackgroundMusicId);
            }

            // 关键：将完成标志位设为 true，OnUpdate 将在下一帧检测到并切换流程
            m_IsChangeSceneComplete = true;
        }

        private void OnLoadSceneFailure(object sender, GameEventArgs e)
        {
            LoadSceneFailureEventArgs ne = (LoadSceneFailureEventArgs)e;
            if (ne.UserData != this)
            {
                return;
            }

            Log.Error("Load scene '{0}' failure, error message '{1}'.", ne.SceneAssetName, ne.ErrorMessage);
        }

        private void OnLoadSceneUpdate(object sender, GameEventArgs e)
        {
            LoadSceneUpdateEventArgs ne = (LoadSceneUpdateEventArgs)e;
            if (ne.UserData != this)
            {
                return;
            }

            // 这里可以用来更新加载界面的进度条
            Log.Info("Load scene '{0}' update, progress '{1}'.", ne.SceneAssetName, ne.Progress.ToString("P2"));
        }

        private void OnLoadSceneDependencyAsset(object sender, GameEventArgs e)
        {
            LoadSceneDependencyAssetEventArgs ne = (LoadSceneDependencyAssetEventArgs)e;
            if (ne.UserData != this)
            {
                return;
            }

            // 这里可以用来显示正在加载的具体资源名称
            Log.Info("Load scene '{0}' dependency asset '{1}', count '{2}/{3}'.", ne.SceneAssetName, ne.DependencyAssetName, ne.LoadedCount.ToString(), ne.TotalCount.ToString());
        }
    }
}