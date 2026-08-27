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

/// <summary>
/// 千段击：稀有攻击，1 能量 + X 法力（消耗全部法力，0 法力也可打出）。
/// 造成 12 点伤害（1 段）；你每拥有 1 点法力，额外造成 5 点伤害 1 次（多段，升级后 6）。
/// </summary>
public class QianqiuThousandStrike() : QianqiuCard(1,
    CardType.Attack, CardRarity.Rare,
    TargetType.AnyEnemy)
{
    // X 法力费用：打出时消耗当前全部法力；X 卡在 0 法力时仍可打出
    public override bool HasStarCostX => true;

    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(12, ValueProp.Move),
        new ExtraDamageVar(5m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        // 第一段：12 点
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
        // 每 1 点法力：额外 5（升级 6）点伤害 1 次
        int stars = ResolveStarXValue();
        if (stars > 0)
        {
            await DamageCmd.Attack(DynamicVars.ExtraDamage.BaseValue).FromCard(this, cardPlay)
                .Targeting(cardPlay.Target)
                .WithHitCount(stars)
                .Execute(choiceContext);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.ExtraDamage.UpgradeValueBy(1m); // 5 → 6
    }
}
