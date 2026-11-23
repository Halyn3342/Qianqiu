#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.ValueProps;

namespace MoeNegiMod.Qianqiu.Cards;

/// <summary>风暴（稀有攻击）：2 能量 + 5 法力，对所有敌人造成 6 点伤害 6 次（升级 7 次）。</summary>
public class QianqiuStorm() : QianqiuCard(2,
    CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
{
    public override int CanonicalStarCost => 5;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(6, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int hits = IsUpgraded ? 7 : 6;
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).WithHitCount(hits)
            .FromCard(this, cardPlay).TargetingAllOpponents(CombatState!).Execute(choiceContext);
    }

    protected override void OnUpgrade() { }
}
