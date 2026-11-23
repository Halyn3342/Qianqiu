#nullable enable
using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MoeNegiMod.Qianqiu.Cards;

namespace MoeNegiMod.Qianqiu.Patch;

/// <summary>
/// 卡牌说明前置"黑爪"：对任何黑爪牌（静态 IShadowTouched 或运行时附魔）在卡面描述顶部显示 [purple]黑爪[/purple]。
/// 若描述里已经带"黑爪"（老牌自带）则不重复。
/// </summary>
[HarmonyPatch(typeof(CardModel), "GetDescriptionForPile", new Type[] { typeof(PileType), typeof(Creature) })]
public static class QianqiuShadowDescriptionPatch
{
    public static void Postfix(CardModel __instance, ref string __result)
    {
        if (__result == null)
        {
            return;
        }
        if (__instance.Owner?.Creature is Creature owner
            && QianqiuShadow.IsShadowTouched(__instance, owner)
            && !__result.Contains("[purple]黑爪[/purple]"))
        {
            __result = "[purple]黑爪[/purple]。\n" + __result;
        }
    }
}
