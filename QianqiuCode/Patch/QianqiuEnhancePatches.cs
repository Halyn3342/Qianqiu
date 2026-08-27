using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Saves.Runs;
using MoeNegiMod.Qianqiu.Cards;
using MoeNegiMod.Qianqiu.Relics;

namespace MoeNegiMod.Qianqiu.Patch;

/// <summary>
/// 古老牙齿：注册九千秋的卡牌升华——读取 → 回溯。
/// </summary>
[HarmonyPatch(typeof(ArchaicTooth), "get_TranscendenceUpgrades")]
public static class ArchaicToothUpgradePatch
{
    public static void Postfix(ref Dictionary<ModelId, CardModel> __result)
    {
        __result[ModelDb.Card<QianqiuRead>().Id] = ModelDb.Card<QianqiuRetrace>();
    }
}

/// <summary>
/// 欧洛巴斯之触：注册九千秋的起始遗物精炼——血灵花 → 血灵双生。
/// </summary>
[HarmonyPatch(typeof(TouchOfOrobas), "get_RefinementUpgrades")]
public static class TouchOfOrobasUpgradePatch
{
    public static void Postfix(ref Dictionary<ModelId, RelicModel> __result)
    {
        __result[ModelDb.Relic<QianqiuToy>().Id] = ModelDb.Relic<QianqiuTwin>();
    }
}

/// <summary>
/// 尘封魔典：若 AncientCard 为空（例如通过 FreeLoadout/控制台直接添加、未走 SetupForPlayer），
/// 回退为九千秋的先古牌"启示录"，避免 AfterObtained 空引用崩溃，并保证能拿到启示录。
/// </summary>
[HarmonyPatch(typeof(DustyTome), "AfterObtained")]
public static class DustyTomeAfterObtainedPatch
{
    public static void Prefix(DustyTome __instance)
    {
        if (__instance.AncientCard == null)
        {
            AccessTools.Property(typeof(DustyTome), "AncientCard").SetValue(__instance, ModelDb.Card<QianqiuApocalypse>().Id);
        }
    }
}
