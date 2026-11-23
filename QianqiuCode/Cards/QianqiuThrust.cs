#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.ValueProps;
using MoeNegiMod.Qianqiu.Powers;

namespace MoeNegiMod.Qianqiu.Cards;

/// <summary>突刺（罕见攻击）：1 能量 + 1 法力，造成 8 点伤害；若目标有看穿额外 +3（升级 +5），并给予 1 层看穿。</summary>
public class QianqiuThrust() : QianqiuCard(1,
    CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    public override int CanonicalStarCost => 1;
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(8m),
        new ExtraDamageVar(3m),
        new CalculatedDamageVar(ValueProp.Move)
            .WithMultiplier((CardModel card, Creature? target) => target != null && target.HasPower<QianqiuPiercePower>() ? 1 : 0),
        new PowerVar<QianqiuPiercePower>("Pierce", 1m)
    ];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<QianqiuPiercePower>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await DamageCmd.Attack(DynamicVars.CalculatedDamage).FromCard(this, cardPlay).Targeting(cardPlay.Target).Execute(choiceContext);
        await PowerCmd.Apply<QianqiuPiercePower>(choiceContext, cardPlay.Target, DynamicVars["Pierce"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars.ExtraDamage.UpgradeValueBy(2m);
}
