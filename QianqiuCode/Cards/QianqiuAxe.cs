#nullable enable
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.ValueProps;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoeNegiMod.Qianqiu.Cards;

/// <summary>
/// 斩斧（黑爪 Token）：2 费，造成 15 点伤害；对生命值低于 50% 的敌人额外造成 5 点伤害（升级后 10）。
/// 只能通过变牌/产牌获得。
/// </summary>
public class QianqiuAxe() : QianqiuCard(2,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AnyEnemy), IShadowTouched
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(15m),
        new ExtraDamageVar(5m),
        new CalculatedDamageVar(ValueProp.Move)
            .WithMultiplier((CardModel card, Creature? target) =>
                target != null && target.CurrentHp < target.MaxHp * 0.5m ? 1 : 0)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await DamageCmd.Attack(DynamicVars.CalculatedDamage).FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.ExtraDamage.UpgradeValueBy(5m); // 5 → 10
    }
}
