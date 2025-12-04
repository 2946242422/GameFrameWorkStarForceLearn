using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PuddingCat
{
    // 用于方便地管理一个升级选项的UI元素
    [System.Serializable]
    public class UpgradeOptionUI
    {
        public GameObject ButtonObject;
        public Text NameText;
        public Text DescriptionText;
        private DRUpgrade m_UpgradeData;
        public void SetData(DRUpgrade data, System.Action<int> onClickCallback)
        {
            m_UpgradeData = data;
            NameText.text = data.Name;
            DescriptionText.text = data.Description;

            // 移除旧监听，添加新监听
            ButtonObject.GetComponent<Button>().onClick.RemoveAllListeners();
            ButtonObject.GetComponent<Button>().onClick.AddListener(() =>
            {
                onClickCallback(m_UpgradeData.Id);
            });

            ButtonObject.SetActive(true);
        }
    }

    public class UpgradeForm : UGuiForm
    {
        [SerializeField]
        private List<UpgradeOptionUI> m_UpgradeOptions = new List<UpgradeOptionUI>();
        // private ProcedureUpgrade m_ProcedureUpgrade;
    }
}

