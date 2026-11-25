namespace MacroTools.Localization.Translations;

/// <summary>
/// [SL] (0.75.0) Simplified Chinese for Shattered Legacies' own texts (<see cref="SlText"/>). WL's entries are in
/// ChineseTranslation.cs (from WL 5.0.4); this file only adds the fork's. Terms as in WL's table: 都城 (capital),
/// 控制点 (Control Point), 回合 (turn), 任务（F9）.
/// </summary>
internal sealed partial class ChineseTranslation
{
  private void AddShatteredLegacies()
  {
    Add(SlText.Title, "Shattered Legacies：破碎的传承");
    Add(SlText.IntroNotice, "基于 Warcraft Legacies 的非官方地图，并非官方版本。");
    Add(SlText.IntroTurningPoints, "失去都城并不意味着终结。当转折点出现时，请在命运沙漏（Hourglass of Fate，右上角）中选择你的道路。");
    Add(SlText.IntroNewPlayer, "新玩家？打开任务日志（F9），查看你的目标以及 Shattered Legacies 的玩法。");

    Add(SlText.GuideTitle, "Shattered Legacies 玩法说明");
    Add(SlText.GuideHistory, "在 Shattered Legacies 中，历史是可以改变的。");
    Add(SlText.GuideFallbacks, "退守：阵营失去都城后，可以退守并以新的阵营身份回归。");
    Add(SlText.GuideBetrayals, "背叛：部分阵营可以倒戈攻击盟友，或脱离所在的队伍。");
    Add(SlText.GuideHourglass, "命运沙漏：转折点会出现在命运沙漏中（右上角）。当某个转折点可用时，研究它即可走上这条道路。机器人会自行决定。");
    Add(SlText.GuideDefeat, "失败：只有当你超过 5 个回合未控制任何控制点时才会失败。失去英雄不会导致你失败。");
    Add(SlText.GuideBots, "机器人：机器人会接管空位以及离开游戏的玩家。");
    Add(SlText.GuideInDevelopment, "许多转折点仍在开发中。");

    Add(SlText.CreditsTitle, "致谢");
    Add(SlText.CreditsNotice, "Shattered Legacies: Botified Realm 是基于 Warcraft Legacies 的非官方探索版／原型。它并非官方版本，也与 Warcraft Legacies 团队无关。本地图尝试借助大语言模型（LLM）构建一套基于图结构的机器人系统。");
    Add(SlText.CreditsThanks, "Warcraft Legacies 团队衷心感谢……");

    Add(SlText.HintFallbacks, "失去都城并不意味着终结。许多阵营会退守，并以新的阵营身份回归。");
    Add(SlText.HintHourglass, "转折点会出现在命运沙漏中（右上角）。一旦某个转折点可用，研究它即可选择这条道路。");
    Add(SlText.HintDefeat, "只有当你超过 5 个回合未控制任何控制点时才会被击败。只要拥有一个控制点就能存活。");
    Add(SlText.HintLeavers, "当有玩家离开时，机器人会接管其阵营。");
    Add(SlText.HintBotChoices, "机器人会随机选择转折点，所以你无法预测它们的行动。");
    Add(SlText.HintFork, "本地图是 Warcraft Legacies 的非官方分支。原版地图请访问 {url}");
    Add(SlText.HintPatreon, "你可以在 Patreon 上支持 Warcraft Legacies 原作团队：{url}");
  }
}
