using MacroTools.Localization;
using MacroTools.Save;

namespace WarcraftLegacies.Source.GameLogic;

/// <summary>
/// Provides a set of dummy quests that provide information to players.
/// </summary>
public static class InfoQuests
{
  // WL 5.0.4's thank-you list (bundle 4, 0.75.0); the names stay as they are in every language
  private const string Thanks = @"Previous Azeroth Wars developers: Augur, Crusader793, Bhaal_Spawn., EagleMan, Avrion, Railen, Thurr, Rhemar, SteakonSpear, Talinn, LuneLune, Dave_Rolf, Richardik, and HerrDave,
the founders of Warcraft Legacies, Lordsebas and YakaryBovine,
the Warcraft Legacies contributors, Naowsx, Savantic, Headhunter, Tracy, Deathlord, zbovo, Chunky, and TheG,
and to the artists who have created the swathe of custom icons and models that we use: Ujimasa Hojo, General Frank, Shiv, Malvodion, nGy, UgoUgo, JetFangInferno, -Grendel, SpasMaster, Eagle XI, Mythic, Hamsta, assasin_lord, Hayate, R.A.N.G.I.T., Stefan.K, Eusira, 00110000, Pyritie, Sunchips, Dojo, PeeKay, nhocklanhox6, dhguardianes, HappyTauren, HerrDave, Shyster, takakenji, JesusHipster, Boogles, Kwaliti, Tauer, WhiteDeath, Darkfang, Mechanical Man, Vinz, Sin'dorei300, Palaslayer, Panda, Moy, MangakaDark, JollyD, Mc !, BLazeKraze, apaka, Taur, Pinachet, Kitabatake, johnwar, r.ace613, bakr, 8512590215848, -Berz-, Villagerino, HuanJuan, Sapprine, Chuchurute, Anachron, Darky29, CRAZYRUSSIAN, Exarch, alfredx_sotn, Nasrudin, JacKThERiPPeR, BaiyuGalan, Aldeia, ChevronSeven, PrinceYaser, Hate, Sun gate, RvzerBro, and Epsilon";

  /// <summary>
  /// Creates a series of dummy quests that provide information to players.
  /// </summary>
  public static void Setup()
  {
    // [SL] (0.75.0): how the fork plays (the maintainer's text), first in the list
    var guideQuest = quest.Create();
    guideQuest.SetIcon(@"ReplaceableTextures\CommandButtons\BTNSpellBookBLS.blp");
    guideQuest.IsRequired = false;

    var creditsQuest = quest.Create();
    creditsQuest.SetIcon(@"ReplaceableTextures\CommandButtons\BTNManual3.blp");
    creditsQuest.IsRequired = false;

    // the system language now, the player's chosen language once the settings are loaded (as Objective does)
    Describe(guideQuest, creditsQuest);
    SaveManager.RunWhenLocalPlayerSettingsReady(() => Describe(guideQuest, creditsQuest));
  }

  private static void Describe(quest guideQuest, quest creditsQuest)
  {
    guideQuest.SetTitle(Loc.Get(SlText.GuideTitle));
    guideQuest.SetDescription(Loc.Get(SlText.GuideHistory) + "\n\n"
                              + Loc.Get(SlText.GuideFallbacks) + "\n"
                              + Loc.Get(SlText.GuideBetrayals) + "\n"
                              + Loc.Get(SlText.GuideHourglass) + "\n"
                              + Loc.Get(SlText.GuideDefeat) + "\n"
                              + Loc.Get(SlText.GuideBots) + "\n\n"
                              + Loc.Get(SlText.GuideInDevelopment));

    // [SL] (0.39.9): first paragraph = fork notice (requested by the original maintainer)
    creditsQuest.SetTitle(Loc.Get(SlText.CreditsTitle));
    creditsQuest.SetDescription(Loc.Get(SlText.CreditsNotice) + "\n\n" + Loc.Get(SlText.CreditsThanks) + "\n\n" + Thanks);
  }
}
