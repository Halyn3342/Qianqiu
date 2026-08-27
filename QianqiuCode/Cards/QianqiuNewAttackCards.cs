#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
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

/// <summary>生命回转（普通攻击）：1 费。造成 4 点伤害，恢复 1 点生命（升级 6 伤/2 生命）。</summary>
public class QianqiuLifeCycle() : QianqiuCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(4, ValueProp.Move),
        new HealVar(1m)
    ];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target).Execute(choiceContext);
        await CreatureCmd.Heal(Owner.Creature, DynamicVars.Heal.BaseValue, false);
    }
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
        DynamicVars.Heal.UpgradeValueBy(1m);
    }
}

/// <summary>血刃（普通攻击）：1 费。造成 6 点伤害，失去 2 点生命，额外造成已损失生命值 10% 的伤害（升级 15%）。</summary>
public class QianqiuBloodBlade() : QianqiuCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(6m),
        new ExtraDamageVar(1m),
        new CalculatedDamageVar(ValueProp.Move)
            .WithMultiplier((CardModel card, Creature? _) =>
                card.Owner?.Creature is Creature c ? Math.Floor((c.MaxHp - c.CurrentHp) * (card.DynamicVars["Percent"].BaseValue / 100m)) : 0),
        new DynamicVar("Percent", 10m)
    ];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 失去 2 点生命（无视格挡）
        await CreatureCmd.Damage(choiceContext, Owner.Creature, 2, ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, this, cardPlay);
        await DamageCmd.Attack(DynamicVars.CalculatedDamage).FromCard(this, cardPlay).Targeting(cardPlay.Target).Execute(choiceContext);
    }
    protected override void OnUpgrade() => DynamicVars["Percent"].UpgradeValueBy(5m); // 10 → 15
}

/// <summary>血怒斩（罕见攻击）：2 费。造成 10 点伤害；若当前生命值低于 60% 则额外 +8（升级 +12）。</summary>
public class QianqiuBloodFury() : QianqiuCard(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(10m),
        new ExtraDamageVar(8m),
        new CalculatedDamageVar(ValueProp.Move)
            .WithMultiplier((CardModel card, Creature? _) =>
                card.Owner?.Creature is Creature c && c.CurrentHp < c.MaxHp * 0.6m ? 1 : 0)
    ];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.CalculatedDamage).FromCard(this, cardPlay).Targeting(cardPlay.Target).Execute(choiceContext);
    }
    protected override void OnUpgrade() => DynamicVars.ExtraDamage.UpgradeValueBy(4m); // 8 → 12
}

/// <summary>血袭（罕见攻击）：1 费。造成 5 点伤害，恢复最大生命值 5%（升级 8%）的生命值。</summary>
public class QianqiuBloodStrike() : QianqiuCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(5, ValueProp.Move),
        new DynamicVar("Percent", 5m)
    ];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target).Execute(choiceContext);
        int heal = (int)Math.Floor(Owner.Creature.MaxHp * DynamicVars["Percent"].BaseValue / 100m);
        await CreatureCmd.Heal(Owner.Creature, heal, false);
    }
    protected override void OnUpgrade() => DynamicVars["Percent"].UpgradeValueBy(3m); // 5 → 8
}

/// <summary>破限（罕见攻击）：2 费。造成 8 点伤害；每拥有 20 点最大生命值额外 +2（升级 +3）。</summary>
public class QianqiuBreakLimit() : QianqiuCard(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(8m),
        new ExtraDamageVar(2m),
        new CalculatedDamageVar(ValueProp.Move)
            .WithMultiplier((CardModel card, Creature? _) =>
                card.Owner?.Creature is Creature c ? Math.Floor(c.MaxHp / 20m) : 0)
    ];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.CalculatedDamage).FromCard(this, cardPlay).Targeting(cardPlay.Target).Execute(choiceContext);
    }
    protected override void OnUpgrade() => DynamicVars.ExtraDamage.UpgradeValueBy(1m); // 2 → 3
}

/// <summary>血瀑（稀有攻击）：2 费。造成 5 点伤害 3 次，每次附带已损失生命值 6%（升级 10%）的额外伤害。</summary>
public class QianqiuBloodFalls() : QianqiuCard(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
{
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(5, ValueProp.Move),
        new DynamicVar("Percent", 6m)
    ];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int perHit = (int)Math.Floor(DynamicVars.Damage.BaseValue + (Owner.Creature.MaxHp - Owner.Creature.CurrentHp) * DynamicVars["Percent"].BaseValue / 100m);
        await DamageCmd.Attack(perHit).FromCard(this, cardPlay).Targeting(cardPlay.Target).WithHitCount(3).Execute(choiceContext);
    }
    protected override void OnUpgrade() => DynamicVars["Percent"].UpgradeValueBy(4m); // 6 → 10
}

/// <summary>注视（普通攻击）：1 费。对所有敌人造成 4 点伤害，给予 2 层看穿（升级后 0 费）。</summary>
public class QianqiuGaze() : QianqiuCard(1, CardType.Attack, CardRarity.Common, TargetType.AllEnemies)
{
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(4, ValueProp.Move),
        new PowerVar<QianqiuPiercePower>("Pierce", 2m)
    ];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<QianqiuPiercePower>()];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).TargetingAllOpponents(CombatState!).Execute(choiceContext);
        await PowerCmd.Apply<QianqiuPiercePower>(choiceContext, CombatState?.HittableEnemies, DynamicVars["Pierce"].BaseValue, Owner.Creature, this);
    }
    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>攻略之力（稀有攻击）：2 费，消耗。对一名敌人造成 8 点伤害；清空所有敌人的看穿，每清空 1 层造成的伤害 +1（升级 +2）。</summary>
public class QianqiuStrategyPower() : QianqiuCard(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
{
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(8m),
        new ExtraDamageVar(1m),
        new CalculatedDamageVar(ValueProp.Move)
            .WithMultiplier((CardModel card, Creature? _) =>
                card.CombatState?.HittableEnemies?.Sum(e => e.GetPowerAmount<QianqiuPiercePower>()) ?? 0),
        new DynamicVar("Bonus", 1m)
    ];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var enemies = CombatState?.HittableEnemies;
        int total = 0;
        if (enemies != null)
        {
            foreach (Creature e in enemies)
            {
                total += (int)e.GetPowerAmount<QianqiuPiercePower>();
            }
        }
        int dmg = (int)DynamicVars.CalculationBase.BaseValue + total * (int)DynamicVars["Bonus"].BaseValue;
        await DamageCmd.Attack(dmg).FromCard(this, cardPlay).Targeting(cardPlay.Target).Execute(choiceContext);
        if (enemies != null)
        {
            foreach (Creature e in enemies)
            {
                if (e.GetPower<QianqiuPiercePower>() is QianqiuPiercePower p)
                {
                    await PowerCmd.Remove(p);
                }
            }
        }
    }
    protected override void OnUpgrade() => DynamicVars["Bonus"].UpgradeValueBy(1m); // 1 → 2
}

/// <summary>认知收割（罕见攻击）：1 费。造成 6 点伤害；若敌人看穿 ≥4 则伤害翻倍（升级后 7 点伤害）。</summary>
public class QianqiuCognitionHarvest() : QianqiuCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(6m),
        new ExtraDamageVar(6m),
        new CalculatedDamageVar(ValueProp.Move)
            .WithMultiplier((CardModel card, Creature? target) =>
                target != null && target.GetPowerAmount<QianqiuPiercePower>() >= 4 ? 1 : 0)
    ];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<QianqiuPiercePower>()];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.CalculatedDamage).FromCard(this, cardPlay).Targeting(cardPlay.Target).Execute(choiceContext);
    }
    protected override void OnUpgrade() => DynamicVars.CalculationBase.UpgradeValueBy(1m); // 6 → 7
}
