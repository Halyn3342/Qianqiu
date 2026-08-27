#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.ValueProps;

namespace MoeNegiMod.Qianqiu.Cards;

/// <summary>灵光（罕见攻击）：0 能量 + 1 法力，造成 4 点伤害；本回合你每打出 1 张牌，此牌伤害 +1（升级后 +2）。</summary>
public class QianqiuGlint() : QianqiuCard(0,
    CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    public override int CanonicalStarCost => 1;
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(4m),
        new ExtraDamageVar(1m),
        new CalculatedDamageVar(ValueProp.Move)
            .WithMultiplier((CardModel card, Creature? _) =>
                card.CombatState == null || CombatManager.Instance?.History == null
                    ? 0
                    : CombatManager.Instance.History.CardPlaysFinished.Count(e => e.HappenedThisTurn(card.CombatState) && e.CardPlay.Player == card.Owner))
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await DamageCmd.Attack(DynamicVars.CalculatedDamage).FromCard(this, cardPlay).Targeting(cardPlay.Target).Execute(choiceContext);
    }

    protected override void OnUpgrade() => DynamicVars.ExtraDamage.UpgradeValueBy(1m);
}
