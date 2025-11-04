using GameFramework;

namespace PuddingCat
{
    /// <summary>
    /// 对话框显示数据。
    /// 这是一个数据容器类，用于在打开对话框界面时传递所有必要的参数。
    /// </summary>
    public class DialogParams
    {
        /// <summary>
        /// 获取或设置模式，即按钮数量。通常取值为 1 (确认)、2 (确认/取消)、3 (确认/取消/其他)。
        /// </summary>
        public int Mode { get; set; }

        /// <summary>
        /// 获取或设置标题。
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 获取或设置消息内容。
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 获取或设置弹出窗口时是否暂停游戏。
        /// </summary>
        public bool PauseGame { get; set; }

        /// <summary>
        /// 获取或设置确认按钮的文本。如果为空，则会使用默认文本。
        /// </summary>
        public string ConfirmText { get; set; }

        /// <summary>
        /// 获取或设置点击确定按钮时的回调函数。
        /// </summary>
        public GameFrameworkAction<object> OnClickConfirm { get; set; }

        /// <summary>
        /// 获取或设置取消按钮的文本。
        /// </summary>
        public string CancelText { get; set; }

        /// <summary>
        /// 获取或设置点击取消按钮时的回调函数。
        /// </summary>
        public GameFrameworkAction<object> OnClickCancel { get; set; }

        /// <summary>
        /// 获取或设置中立/其他按钮的文本。
        /// </summary>
        public string OtherText { get; set; }

        /// <summary>
        /// 获取或设置点击其它按钮时的回调函数。
        /// </summary>
        public GameFrameworkAction<object> OnClickOther { get; set; }

        /// <summary>
        /// 获取或设置用户自定义数据，这个数据会在回调时原样传回。
        /// </summary>
        public object UserData // 这里原脚本是 string，改为 object 更通用
        {
            get;
            set;
        }
    }
}