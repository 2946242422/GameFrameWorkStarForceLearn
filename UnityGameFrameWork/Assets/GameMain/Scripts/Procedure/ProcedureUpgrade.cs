using System.Collections;
using System.Collections.Generic;
using GameFramework.DataTable;
using UnityEngine;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;
namespace PuddingCat
{
    public class ProcedureUpgrade : ProcedureBase
    {
        private IDataTable<DRUpgrade> m_UpgradeTable;
        private List<DRUpgrade> m_UpgradeChoices;
        public override bool UseNativeDialog { get; }
        
       
        protected override void OnInit(ProcedureOwner procedureOwner)
        {
            base.OnInit(procedureOwner);
            // 预先获取数据表，避免每次都获取
            m_UpgradeTable = GameEntry.DataTable.GetDataTable<DRUpgrade>();
        }
        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            // 1. 暂停游戏
            GameEntry.Base.PauseGame();

            // 2. 从“卡池”中随机抽取三个不重复的升级
            m_UpgradeChoices = ChooseRandomUpgrades(3);

            // 3. 打开升级选择界面，并将“三个选项”和“自身引用”传递过去
            object[] openData = new object[] { m_UpgradeChoices, this };
            GameEntry.UI.OpenUIForm(UIFormId.UpgradeForm, openData);
        }
         /// <summary>
        /// 这个方法由 UpgradeForm 调用
        /// </summary>
        public void OnUpgradeSelected(int upgradeId)
        {
            DRUpgrade selectedUpgrade = m_UpgradeChoices.Find(u => u.Id == upgradeId);

            // 4. 应用升级效果 (暂时只打印日志，下一步实现)
            ApplyUpgrade(selectedUpgrade);

            // 5. 关闭UI界面
            GameEntry.UI.CloseUIForm((int)UIFormId.UpgradeForm);
            
            // 6. 恢复游戏并回到主游戏流程
            GameEntry.Base.ResumeGame();
            ChangeState<ProcedureMain>(procedureOwner);
        }
        
        /// <summary>
        /// 从数据表中随机选择指定数量的不重复升级。
        /// </summary>
        private List<DRUpgrade> ChooseRandomUpgrades(int count)
        {
            DRUpgrade[] allUpgrades = m_UpgradeTable.GetAllDataRows();
            List<DRUpgrade> validUpgrades = allUpgrades.ToList(); // 转换为列表方便操作

            // TODO: 在这里可以实现更复杂的逻辑，比如排除已获得的升级、根据稀有度权重抽卡等
            
            List<DRUpgrade> choices = new List<DRUpgrade>();
            if (validUpgrades.Count <= count)
            {
                return validUpgrades;
            }

            // 简单的随机不重复抽取
            for (int i = 0; i < count; i++)
            {
                int randomIndex = GameFramework.Utility.Random.GetRandom(0, validUpgrades.Count);
                choices.Add(validUpgrades[randomIndex]);
                validUpgrades.RemoveAt(randomIndex);
            }

            return choices;
        }

        /// <summary>
        /// 应用升级效果。
        /// </summary>
        private void ApplyUpgrade(DRUpgrade upgrade)
        {
            // TODO: 这是我们下一步要实现的核心逻辑
            Log.Info("Player selected upgrade: '{0}', Type='{1}', Param1='{2}'", upgrade.Name, upgrade.Type, upgrade.Param1);

            // 获取玩家飞机的数据对象
            var myAircraft = (MyAircraft)GameEntry.Entity.GetGameEntity(10000); // 假设玩家ID是10000
            if (myAircraft == null) return;

            MyAircraftData aircraftData = myAircraft.GetData<MyAircraftData>();
            
            // 将获得的升级记录下来
            aircraftData.AcquiredUpgrades.Add(upgrade);
        }
    }

}
