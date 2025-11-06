using GameFramework;
using GameFramework.DataTable;
using UnityEngine;

namespace  PuddingCat
{
    /// <summary>
    /// 生存模式。
    /// 这是 GameBase 的一个具体实现，负责处理生存模式下的独特游戏逻辑。
    /// </summary>
    public class SurvivalGame : GameBase
    {
        private float m_ElapseSeconds = 0f; // 计时器，用于控制生成敌人的频率

        /// <summary>
        /// 获取当前游戏模式为“生存模式”。
        /// </summary>
        public override GameMode GameMode
        {
            get
            {
                return GameMode.Survival;
            }
        }

        /// <summary>
        /// 生存模式的逻辑轮询。
        /// </summary>
        public override void Update(float elapseSeconds, float realElapseSeconds)
        {
            // 首先执行基类中的通用Update逻辑（如检查游戏是否结束）
            base.Update(elapseSeconds, realElapseSeconds);

            m_ElapseSeconds += elapseSeconds; // 累加时间
            // 每隔 1 秒执行一次
            if (m_ElapseSeconds >= 1f)
            {
                m_ElapseSeconds = 0f; // 重置计时器
                IDataTable<DRAsteroid> dtAsteroid = GameEntry.DataTable.GetDataTable<DRAsteroid>();
                // 在敌人生成边界内，随机计算一个X和Z坐标
                float randomPositionX = SceneBackground.EnemySpawnBoundary.bounds.min.x + SceneBackground.EnemySpawnBoundary.bounds.size.x * (float)Utility.Random.GetRandomDouble();
                float randomPositionZ = SceneBackground.EnemySpawnBoundary.bounds.min.z + SceneBackground.EnemySpawnBoundary.bounds.size.z * (float)Utility.Random.GetRandomDouble();
                // 请求实体组件，在计算出的随机位置生成一个随机类型的小行星
                GameEntry.Entity.ShowAsteroid(new AsteroidData(GameEntry.Entity.GenerateSerialId(), 60000 + Utility.Random.GetRandom(dtAsteroid.Count))
                {
                    Position = new Vector3(randomPositionX, 0f, randomPositionZ),
                });
            }
        }
    }
}