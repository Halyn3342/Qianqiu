#nullable enable
using BaseLib.Utils;
using Godot;
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
/// 终焉：稀有 攻击牌，3 能量。造成 12 点伤害 + 目标最大生命值 20%（升级 25%）的额外伤害，
/// 然后失去自己当前生命值的 10%（无视格挡）。
/// </summary>
public class QianqiuFinality() : QianqiuCard(3,
    CardType.Attack, CardRarity.Rare,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(12m),
        new ExtraDamageVar(1m),
        // 12 + 自己最大生命 × 20%（升级 25%），向下取整
        new CalculatedDamageVar(ValueProp.Move)
            .WithMultiplier((CardModel card, Creature? _) =>
            {
                decimal pct = card.IsUpgraded ? 0.25m : 0.2m;
                return card.Owner?.Creature?.MaxHp is int maxHp ? Math.Floor(maxHp * pct) : 0;
            })
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await DamageCmd.Attack(DynamicVars.CalculatedDamage).FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

        // 失去自己当前生命值的 10%（无视格挡）
        int hpLoss = (int)Math.Floor(Owner.Creature.CurrentHp * 0.1m);
        await CreatureCmd.Damage(choiceContext, Owner.Creature, hpLoss,
            ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, this, cardPlay);
    }

    protected override void OnUpgrade()
    {
        // 升级效果由 CalculatedDamageVar 的 multiplier 内 IsUpgraded 判断（20% → 25%）
    }
}
