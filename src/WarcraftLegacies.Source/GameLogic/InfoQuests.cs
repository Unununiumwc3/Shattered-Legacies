namespace WarcraftLegacies.Source.GameLogic;

/// <summary>
/// Provides a set of dummy quests that provide information to players.
/// </summary>
public static class InfoQuests
{
  /// <summary>
  /// Creates a series of dummy quests that provide information to players.
  /// </summary>
  public static void Setup()
  {
    var creditsQuest = quest.Create();
    creditsQuest.SetTitle("Credits");
    creditsQuest.SetIcon(@"ReplaceableTextures\CommandButtons\BTNManual3.blp");
    creditsQuest.SetDescription(@"Shattered Legacies: Botified Realm is an unofficial exploration/prototype based on Warcraft Legacies. It is not the official version and is not affiliated with the Warcraft Legacies team. It experiments with a graph-based bot system built with the help of LLMs.

The Warcraft Legacies team sends an enormous thank you to...

Previous Azeroth Wars developers: Augur, Crusader793, Bhaal_Spawn., EagleMan, Avrion, Railen, Thurr, Rhemar, SteakonSpear, Talinn, LuneLune, Dave_Rolf, Richardik, and HerrDave,
the founders of Warcraft Legacies, Lordsebas and YakaryBovine,
the Warcraft Legacies contributors, Naowsx, Savantic, Headhunter, Tracy, Deathlord, zbovo, Chunky, and TheG,
and to the artists who have created the swathe of custom icons and models that we use: Ujimasa Hojo, General Frank, Shiv, Malvodion, nGy, UgoUgo, JetFangInferno, -Grendel,  SpasMaster, Eagle XI, Mythic, Hamsta, assasin_lord, Hayate, R.A.N.G.I.T.,  Stefan.K,  Eusira, 00110000, Pyritie, Sunchips, Dojo, PeeKay, nhocklanhox6, dhguardianes, HappyTauren, HerrDave, Shyster, takakenji, JesusHipster, Boogles, Kwaliti, Tauer, WhiteDeath, and Mechanical Man");
    creditsQuest.IsRequired = false;
  }
}
