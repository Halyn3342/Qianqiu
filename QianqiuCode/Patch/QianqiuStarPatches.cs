using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MoeNegiMod.Qianqiu.Api;
using MoeNegiMod.Qianqiu.Cards;

namespace MoeNegiMod.Qianqiu.Patch;

/// <summary>
/// 九千秋：法力计数器显示为 "x/3"（上限 3）。
/// </summary>
[HarmonyPatch(typeof(NStarCounter), "SetStarCountText")]
public static class NStarCounterDisplayPatch
{
    public static void Postfix(NStarCounter __instance, int stars)
    {
        Player? player = (Player?)AccessTools.Field(typeof(NStarCounter), "_player").GetValue(__instance);
        if (player == null || !Mana.IsQianqiu(player))
        {
            return;
        }
        MegaRichTextLabel label = (MegaRichTextLabel)AccessTools.Field(typeof(NStarCounter), "_label").GetValue(__instance);
        label.Text = $"[center]{stars}/{Mana.GetMaxFor(player)}[/center]";
    }
}

/// <summary>
/// 九千秋：法力计数器 hover 提示显示"法力"（其他角色保持原版"辉星"）。
/// 每次 hover 前替换提示，避免依赖 Initialize 的时序。
/// </summary>
[HarmonyPatch(typeof(NStarCounter), "OnHovered")]
public static class NStarCounterHoverPatch
{
    public static void Prefix(NStarCounter __instance)
    {
        Player? player = (Player?)AccessTools.Field(typeof(NStarCounter), "_player").GetValue(__instance);
        if (player == null || !Mana.IsQianqiu(player))
        {
            return;
        }
        LocString description = new LocString("static_hover_tips", "QIANQIU_MANA_COUNT.description");
        description.Add("singleStarIcon", "[img]res://images/packed/sprite_fonts/star_icon.png[/img]");
        HoverTip tip = new HoverTip(new LocString("static_hover_tips", "QIANQIU_MANA_COUNT.title"), description);
        AccessTools.Field(typeof(NStarCounter), "_hoverTip").SetValue(__instance, tip);
        MegaCrit.Sts2.Core.Logging.Log.Info(">>>[Qianqiu] Mana hover tip applied for Qianqiu");
    }
}

/// <summary>
/// 突刺（QianqiuThrust）：法力不足时仍可打出（只扣实际能付的法力）。
/// 原版星费用卡法力不够会不可打，这里把 StarCostTooHigh 从不可打原因中移除。
/// </summary>
[HarmonyPatch(typeof(PlayerCombatState), "HasEnoughResourcesFor")]
public static class QianqiuThrustPlayablePatch
{
    public static void Postfix(PlayerCombatState __instance, CardModel card, ref UnplayableReason reason)
    {
        if (card is QianqiuThrust)
        {
            reason &= ~UnplayableReason.StarCostTooHigh;
        }
    }
}
