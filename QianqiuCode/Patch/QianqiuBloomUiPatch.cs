#nullable enable
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MoeNegiMod.Qianqiu.Powers;
using System;

namespace MoeNegiMod.Qianqiu.Patch;

/// <summary>
/// 绽放：血条上显示临时生命值（"HP/HP +N"，位于生命值右侧，颜色 #3ec5ce）。
/// </summary>
[HarmonyPatch(typeof(NHealthBar), "RefreshValues")]
public static class BloomHpLabelPatch
{
    private static readonly Color _tempHpColor = new Color("3ec5ce");

    public static void Postfix(NHealthBar __instance)
    {
        Creature? creature = (Creature?)AccessTools.Field(typeof(NHealthBar), "_creature").GetValue(__instance);
        int temp = creature?.GetPowerAmount<QianqiuBloomPower>() ?? 0;
        MegaLabel hpLabel = (MegaLabel)AccessTools.Field(typeof(NHealthBar), "_hpLabel").GetValue(__instance);

        Label? label = __instance.GetNodeOrNull<Label>("TempHpLabel");
        if (temp > 0)
        {
            if (label == null)
            {
                label = new Label();
                label.Name = "TempHpLabel";
                __instance.AddChild(label);
            }
            label.Visible = true;
            label.Text = $"+{temp}";
            label.AddThemeColorOverride("font_color", _tempHpColor);
            label.AddThemeColorOverride("font_outline_color", new Color("003322"));
            label.AddThemeConstantOverride("outline_size", 4);
            label.GlobalPosition = hpLabel.GlobalPosition + new Vector2(hpLabel.Size.X + 6f, hpLabel.Size.Y * 0.5f - label.Size.Y * 0.5f);
        }
        else if (label != null)
        {
            label.Visible = false;
        }
    }
}

/// <summary>
/// 绽放：临时生命值超过生命条长度时，按比例额外扩展血条容器长度。
/// </summary>
[HarmonyPatch(typeof(NHealthBar), "SetHpBarContainerSizeWithOffsetsImmediately")]
public static class BloomHpBarExtendPatch
{
    public static void Postfix(NHealthBar __instance)
    {
        Creature? creature = (Creature?)AccessTools.Field(typeof(NHealthBar), "_creature").GetValue(__instance);
        int temp = creature?.GetPowerAmount<QianqiuBloomPower>() ?? 0;
        if (creature == null || temp <= 0 || temp <= creature.MaxHp)
        {
            return;
        }
        float baseWidth = __instance.HpBarContainer.Size.X;
        float maxFgWidth = baseWidth - 10f;
        float extra = (float)(temp - creature.MaxHp) / Math.Max(1, creature.MaxHp) * maxFgWidth;
        __instance.HpBarContainer.Size = new Vector2(baseWidth + extra, __instance.HpBarContainer.Size.Y);
    }
}

/// <summary>
/// 绽放：临时生命段——复制原版毒前景节点（poisonForeground.Duplicate），复用其贴图/九宫格/垂直布局；
/// 宽度用原版 GetFgWidth 算法；用 Offset 从血条末端向左填充。仿照海克斯「灼烧」。
/// 宿主节点每帧驱动（临时生命吸收伤害时 HP 不变、血条不刷新，需每帧更新）。
/// </summary>
public sealed partial class TempHpBarNode : Control
{
    public NHealthBar? Bar;
    public Creature? Creature;

    private Control? _clone;
    private static FieldInfo? _poisonField;
    private static MethodInfo? _getFgWidth;
    private static readonly Color _color = new Color("3ec5ce");

    public override void _Process(double delta)
    {
        if (Bar == null || Creature == null || !IsInsideTree())
        {
            return;
        }
        try
        {
            if (_clone == null || !GodotObject.IsInstanceValid(_clone))
            {
                _poisonField ??= AccessTools.Field(typeof(NHealthBar), "_poisonForeground");
                _getFgWidth ??= typeof(NHealthBar).GetMethod("GetFgWidth", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, new[] { typeof(int) }, null);
                if (_poisonField?.GetValue(Bar) is not Control poison)
                {
                    return;
                }
                _clone = (Control)poison.Duplicate();
                _clone.Name = "QianqiuTempHpBar";
                _clone.SelfModulate = _color;
                poison.GetParent().AddChild(_clone);
                _clone.Visible = false;
            }

            int temp = Creature.GetPowerAmount<QianqiuBloomPower>();
            if (temp <= 0)
            {
                _clone.Visible = false;
                return;
            }
            float tempWidth = (float)(_getFgWidth?.Invoke(Bar, new object[] { temp }) ?? 0);
            var parent = (Control)_clone.GetParent();
            _clone.OffsetLeft = Math.Max(0f, parent.Size.X - tempWidth);
            _clone.OffsetRight = 0f;
            _clone.Visible = true;
        }
        catch
        {
            // 血条尚未就绪时静默跳过
        }
    }
}

/// <summary>
/// 绽放：把临时生命段宿主节点挂到每个血条上（最底层），并绑定生物。
/// </summary>
[HarmonyPatch(typeof(NHealthBar), "RefreshForeground")]
public static class BloomHpForegroundPatch
{
    public static void Postfix(NHealthBar __instance)
    {
        Creature? creature = (Creature?)AccessTools.Field(typeof(NHealthBar), "_creature").GetValue(__instance);
        if (creature == null)
        {
            return;
        }

        TempHpBarNode? node = __instance.GetNodeOrNull<TempHpBarNode>("QianqiuTempHpHost");
        if (node == null)
        {
            node = new TempHpBarNode();
            node.Name = "QianqiuTempHpHost";
            __instance.AddChild(node);
            __instance.MoveChild(node, 0);
        }
        node.Bar = __instance;
        node.Creature = creature;
    }
}
