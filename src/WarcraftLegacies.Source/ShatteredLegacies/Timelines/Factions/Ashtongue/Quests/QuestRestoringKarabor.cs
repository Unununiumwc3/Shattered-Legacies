using MacroTools.Factions;
using MacroTools.Quests;
using WarcraftLegacies.Source.Ai;
using WarcraftLegacies.Source.Objectives.QuestBased;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Ashtongue.Quests
{
  // 0.63, the Ashtongue's final goal (maintainer): Outland cleansed of fel energy -- all four sites purified. The
  // Temple of the Deathsworn becomes the Temple of Karabor (a type of the fork's: WL's Black Temple is never renamed),
  // Xi'ri the naaru can be trained there, Ishanah is free to move. The faction's NAME stays "Ashtongue" for now: WL
  // and the fork find factions by name (TODO.md).
  public sealed class QuestRestoringKarabor : QuestData
  {
    public QuestRestoringKarabor(QuestData[] sites) : base("Restoring Karabor to the Light",
      "Before Illidan, before the orcs, the Black Temple was Karabor, the draenei's holiest place. Cleanse Outland of "
      + "the fel and it will be again.",
      @"ReplaceableTextures\CommandButtons\BTNDranaiAkama.blp")
    {
      for (var i = 0; i < sites.Length; i++)
      {
        AddObjective(new ObjectiveQuestComplete(sites[i]));
      }

      ResearchId = AshtongueConfig.KaraborTech;
    }

    protected override string RewardDescription =>
      "The Temple of the Deathsworn becomes the Temple of Karabor; Xi'ri the naaru can be trained there; Ishanah "
      + "can leave Shattrath";

    protected override void OnComplete(Faction whichFaction)
    {
      var p = whichFaction.Player;
      if (p == null)
      {
        return;
      }

      ReplaceTemple(p);
      AshtonguePurification.Release();
      for (var j = 0; j < 24; j++)
      {
        DisplayTimedTextToPlayer(Player(j), 0, 0, 20, "|cffffcc00Karabor is restored to the Light.|r The fel is "
                                                      + "burned out of Outland; the Ashtongue's temple shines again.");
      }

      AiApi.LogPin("Ashtongue: Karabor restored");
    }

    // the Temple of the Deathsworn -> the Temple of Karabor, same spot, same share of hit points
    private static void ReplaceTemple(player p)
    {
      var g = CreateGroup();
      GroupEnumUnitsOfPlayer(g, p, null);
      unit u;
      while ((u = FirstOfGroup(g)) != null)
      {
        GroupRemoveUnit(g, u);
        if (GetUnitTypeId(u) != AshtongueConfig.TempleOfTheDeathsworn)
        {
          continue;
        }

        var x = GetUnitX(u);
        var y = GetUnitY(u);
        RemoveUnit(u);
        var k = CreateUnit(p, AshtongueConfig.TempleOfKarabor, x, y, 270f);
        SetUnitInvulnerable(k, true);
      }

      DestroyGroup(g);
    }
  }
}
