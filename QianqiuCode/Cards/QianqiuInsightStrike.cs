#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.ValueProps;
using MoeNegiMod.Qianqiu.Powers;

namespace MoeNegiMod.Qianqiu.Cards;

/// <summary>识破（普通攻击）：1 费，造成 6 点伤害；给予 2 层看穿（升级后 3 层）。</summary>
public class QianqiuInsightStrike() : QianqiuCard(1,
    CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(6, ValueProp.Move),
        new PowerVar<QianqiuPiercePower>("Pierce", 2m)
    ];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<QianqiuPiercePower>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target).Execute(choiceContext);
        await PowerCmd.Apply<QianqiuPiercePower>(choiceContext, cardPlay.Target, DynamicVars["Pierce"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars["Pierce"].UpgradeValueBy(1m); // 2 → 3
}
