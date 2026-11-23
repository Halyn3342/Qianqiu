#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.ValueProps;

namespace MoeNegiMod.Qianqiu.Cards;

/// <summary>吸取（罕见攻击）：1 费，造成 3 点伤害 2 次（升级 3 次）；每段实际造成伤害则恢复 1 点法力。</summary>
public class QianqiuSiphon() : QianqiuCard(1,
    CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(3, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        int hits = IsUpgraded ? 3 : 2;
        var cmd = await DamageCmd.Attack(DynamicVars.Damage.BaseValue).WithHitCount(hits)
            .FromCard(this, cardPlay).Targeting(cardPlay.Target).Execute(choiceContext);
        // 每段实际造成生命值伤害则恢复 1 点法力
        int healed = cmd.Results.SelectMany(x => x).Count(r => r.UnblockedDamage > 0);
        if (healed > 0)
        {
            await PlayerCmd.GainStars(healed, Owner);
        }
    }

    protected override void OnUpgrade() { }
}
