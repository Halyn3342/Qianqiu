using BaseLib.Abstracts;
using Godot;
using MoeNegiMod.Qianqiu.Extensions;

namespace MoeNegiMod.Qianqiu.Character;

/// <summary>
/// 刺击/盾化等"变色牌"专属池（Token 稀有度）。
/// 不进角色池/无色池，因此不会出现在奖励、商店、无色奖励中；
/// 只能通过变形、遗物、产牌等效果直接创建获得。
/// </summary>
public class QianqiuTokenPool : CustomCardPoolModel
{
    public override string Title => "QianqiuTokens";

    // 中性灰白卡背，近似"无色"视觉
    public override float H => 0f;
    public override float S => 0f;
    public override float V => 0.85f;

    public override Color DeckEntryCardColor => new("888888");
    public override Color EnergyOutlineColor => new("555555");

    public override bool IsColorless => true;
}
