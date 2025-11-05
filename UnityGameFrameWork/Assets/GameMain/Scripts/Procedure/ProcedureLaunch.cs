using System;
using GameFramework.Localization;
using PuddingCat; // 引入项目命名空间
using UnityGameFramework.Runtime;
using GameEntry = PuddingCat.GameEntry; // 使用 using 别名，简化代码
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>; // 流程 FSM 的类型别名

/// <summary>
/// 启动流程。
/// 这是游戏启动后的第一个流程，负责进行各种初始化设置。
/// </summary>
public class ProcedureLaunch : ProcedureBase
{
    /// <summary>
    /// 获取在此流程中是否使用原生对话框。
    /// </summary>
    public override bool UseNativeDialog
    {
        get
        {
            // 在启动阶段，UI系统可能尚未初始化，使用原生对话框更可靠
            return true;
        }
    }

    /// <summary>
    /// 当进入此流程时调用。
    /// </summary>
    /// <param name="procedureOwner">流程状态机持有者。</param>
    protected override void OnEnter(ProcedureOwner procedureOwner)
    {
        base.OnEnter(procedureOwner);

        // 1. 初始化构建信息：读取内置的 BuildInfo.txt 文件
        GameEntry.BuiltinData.InitBuildInfo();

        // 2. 初始化语言配置：确定当前游戏要使用的语言
        InitLanguageSettings();

        // 3. 初始化资源变体：根据语言设置，让资源系统加载对应的资源版本
        InitCurrentVariant();

        // 4. 初始化声音配置：加载用户保存的声音设置
        InitSoundSettings();

        // 5. 初始化默认字典：加载用于更新流程的、内置的少量文本
        GameEntry.BuiltinData.InitDefaultDictionary();

        // 注意：在此流程的所有初始化工作完成后，通常会切换到下一个流程
        // 例如：ChangeState<ProcedureSplash>(procedureOwner); (这部分逻辑可能在 OnUpdate 或其他地方)
    }
    protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
    {
        base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

        // 运行一帧即切换到 Splash 展示流程
        ChangeState<ProcedureSplash>(procedureOwner);
    }
    /// <summary>
    /// 初始化语言设置。
    /// </summary>
    private void InitLanguageSettings()
    {
        // 在编辑器模式下，可以方便地从 Inspector 面板强制指定语言，用于测试
        if (GameEntry.Base.EditorResourceMode && GameEntry.Base.EditorLanguage != Language.Unspecified)
        {
            return;
        }

        Language language = GameEntry.Localization.Language;
        // 检查 PlayerPrefs 中是否保存了语言设置
        if (GameEntry.Setting.HasSetting(Constant.Setting.Language))
        {
            try
            {
                // 读取并解析保存的语言字符串
                string languageString = GameEntry.Setting.GetString(Constant.Setting.Language);
                language = (Language)Enum.Parse(typeof(Language), languageString);
            }
            catch
            {
                // 解析失败则忽略
            }
        }

        // 如果解析出的语言不是当前游戏支持的几种语言
        if (language != Language.English
            && language != Language.ChineseSimplified
            && language != Language.ChineseTraditional
            && language != Language.Korean)
        {
            // 则强制设置为英语作为默认语言
            language = Language.English;

            // 并将这个默认设置保存起来
            GameEntry.Setting.SetString(Constant.Setting.Language, language.ToString());
            GameEntry.Setting.Save();
        }

        // 将最终确定的语言设置给本地化组件
        GameEntry.Localization.Language = language;
        Log.Info("Init language settings complete, current language is '{0}'.", language.ToString());
    }

    /// <summary>
    /// 初始化当前资源变体。
    /// </summary>
    private void InitCurrentVariant()
    {
        if (GameEntry.Base.EditorResourceMode)
        {
            // 编辑器资源模式不使用 AssetBundle，也就没有变体（Variant）的概念了
            return;
        }

        string currentVariant = null;
        // 根据当前语言，选择对应的资源变体名称
        switch (GameEntry.Localization.Language)
        {
            case Language.English:
                currentVariant = "en-us";
                break;
            case Language.ChineseSimplified:
                currentVariant = "zh-cn";
                break;
            case Language.ChineseTraditional:
                currentVariant = "zh-tw";
                break;
            case Language.Korean:
                currentVariant = "ko-kr";
                break;
            default:
                // 默认使用简体中文
                currentVariant = "zh-cn";
                break;
        }

        // 将变体名称设置给资源组件，后续加载资源时会自动匹配
        GameEntry.Resource.SetCurrentVariant(currentVariant);
        Log.Info("Init current variant complete.");
    }

    /// <summary>
    /// 初始化声音设置。
    /// </summary>
    private void InitSoundSettings()
    {
        // 从 PlayerPrefs 读取各个声音组的静音和音量设置，如果找不到，则使用提供的默认值
        GameEntry.Sound.Mute("Music", GameEntry.Setting.GetBool(Constant.Setting.MusicMuted, false));
        GameEntry.Sound.SetVolume("Music", GameEntry.Setting.GetFloat(Constant.Setting.MusicVolume, 0.3f));
        GameEntry.Sound.Mute("Sound", GameEntry.Setting.GetBool(Constant.Setting.SoundMuted, false));
        GameEntry.Sound.SetVolume("Sound", GameEntry.Setting.GetFloat(Constant.Setting.SoundVolume, 1f));
        GameEntry.Sound.Mute("UISound", GameEntry.Setting.GetBool(Constant.Setting.UISoundMuted, false));
        GameEntry.Sound.SetVolume("UISound", GameEntry.Setting.GetFloat(Constant.Setting.UISoundVolume, 1f));
        Log.Info("Init sound settings complete.");
    }
}