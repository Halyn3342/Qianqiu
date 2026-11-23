#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.ValueProps;
using MoeNegiMod.Qianqiu.Powers;

namespace MoeNegiMod.Qianqiu.Cards;

/// <summary>护佑护符（普通技能）：1 费。恢复 3 点生命，获得 3 点格挡（升级 4/4）。</summary>
public class QianqiuAmulet() : QianqiuCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new HealVar(3m),
        new BlockVar(3, ValueProp.Move)
    ];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.Heal(Owner.Creature, DynamicVars.Heal.BaseValue, false);
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
    }
    protected override void OnUpgrade()
    {
        DynamicVars.Heal.UpgradeValueBy(1m);
        DynamicVars.Block.UpgradeValueBy(1m);
    }
}

/// <summary>献祭（罕见技能）：0 费。失去 4 点生命，获得 3 点法力，抽 1 张牌（升级失去 3 点生命）。</summary>
public class QianqiuSacrifice() : QianqiuCard(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new StarsVar(3),
        new CardsVar(1),
        new DynamicVar("Loss", 4m)
    ];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.Damage(choiceContext, Owner.Creature, DynamicVars["Loss"].BaseValue, ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, this, cardPlay);
        await PlayerCmd.GainStars(DynamicVars.Stars.BaseValue, Owner);
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
    }
    protected override void OnUpgrade() => DynamicVars["Loss"].UpgradeValueBy(-1m); // 4 → 3
}

/// <summary>庇护（罕见技能）：1 能量 + 1 法力。本回合结束时恢复所受到伤害 20%（升级 30%）的生命值。</summary>
public class QianqiuShelter() : QianqiuCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override int CanonicalStarCost => 1;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Power", 20m)];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        => await PowerCmd.Apply<QianqiuShelterPower>(choiceContext, Owner.Creature, DynamicVars["Power"].BaseValue, Owner.Creature, this);
    protected override void OnUpgrade() => DynamicVars["Power"].UpgradeValueBy(10m); // 20 → 30
}

/// <summary>回响（罕见技能）：1 能量 + 1 法力。本回合内每打出 2 张牌恢复 1 点生命（升级 2 点）。</summary>
public class QianqiuEcho() : QianqiuCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override int CanonicalStarCost => 1;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Power", 1m)];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        => await PowerCmd.Apply<QianqiuEchoPower>(choiceContext, Owner.Creature, DynamicVars["Power"].BaseValue, Owner.Creature, this);
    protected override void OnUpgrade() => DynamicVars["Power"].UpgradeValueBy(1m);
}

/// <summary>共鸣（稀有技能）：0 能量 + 3 法力，消耗。选择 2 张弃牌堆中的牌消耗，获得 2 点力量与 2 点敏捷（升级获得保留）。</summary>
public class QianqiuResonanceRitual() : QianqiuCard(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    public override int CanonicalStarCost => 3;
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var selected = (await CardSelectCmd.FromCombatPile(
            choiceContext, Owner.PlayerCombatState.DiscardPile, Owner,
            new CardSelectorPrefs(SelectionScreenPrompt, 2), _ => true)).ToList();
        foreach (CardModel c in selected)
        {
            await CardCmd.Exhaust(choiceContext, c);
        }
        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, 2, Owner.Creature, this);
        await PowerCmd.Apply<DexterityPower>(choiceContext, Owner.Creature, 2, Owner.Creature, this);
    }
    protected override void OnUpgrade() => AddKeyword(CardKeyword.Retain);
}

/// <summary>漆黑风暴（稀有技能）：1 能量 + 3 法力。打出你消耗牌堆中的所有黑爪牌（它们仍留在消耗牌堆）（升级后 1 能量 + 2 法力）。</summary>
public class QianqiuDarkStorm() : QianqiuCard(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    public override int CanonicalStarCost => 3;
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var cards = Owner.PlayerCombatState.ExhaustPile.Cards
            .Where(c => !c.Keywords.Contains(CardKeyword.Unplayable) && QianqiuShadow.IsShadowTouched(c, Owner.Creature))
            .ToList();
        foreach (CardModel c in cards)
        {
            await CardCmd.AutoPlay(choiceContext, c, null, AutoPlayType.Default, false, true);
        }
    }
    protected override void OnUpgrade() => UpgradeStarCostBy(-1); // 3 法力 → 2 法力
}

/// <summary>协查任务（普通技能）：1 费。给予所有敌人 1 层看穿，抽 1 张牌（升级 2 层）。</summary>
public class QianqiuJointTask() : QianqiuCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<QianqiuPiercePower>("Pierce", 1m),
        new CardsVar(1)
    ];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<QianqiuPiercePower>()];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<QianqiuPiercePower>(choiceContext, CombatState?.HittableEnemies, DynamicVars["Pierce"].BaseValue, Owner.Creature, this);
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
    }
    protected override void OnUpgrade() => DynamicVars["Pierce"].UpgradeValueBy(1m);
}

/// <summary>人物壁垒（罕见技能）：1 费。敌人每拥有 2 点看穿获得 1 点格挡（所有敌人合计，升级 2 点格挡）。</summary>
public class QianqiuPersonnelBarrier() : QianqiuCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Power", 1m)];
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
        int block = (int)(Math.Floor(total / 2m) * DynamicVars["Power"].BaseValue);
        await CreatureCmd.GainBlock(Owner.Creature, block, ValueProp.Move, cardPlay, false);
    }
    protected override void OnUpgrade() => DynamicVars["Power"].UpgradeValueBy(1m); // 1 → 2
}

/// <summary>认知屏障（罕见技能）：2 费。给予所有敌人 6 层看穿，每实际给予 1 层获得 1 点格挡（人工制品抵消的层数不计，升级 7 层）。</summary>
public class QianqiuCognitiveBarrier() : QianqiuCard(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Pierce", 6m)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<QianqiuPiercePower>()];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var enemies = CombatState?.HittableEnemies;
        int total = 0;
        if (enemies != null)
        {
            foreach (Creature e in enemies)
            {
                int before = (int)e.GetPowerAmount<QianqiuPiercePower>();
                await PowerCmd.Apply<QianqiuPiercePower>(choiceContext, e, DynamicVars["Pierce"].BaseValue, Owner.Creature, this);
                int after = (int)e.GetPowerAmount<QianqiuPiercePower>();
                total += Math.Max(0, after - before);
            }
        }
        await CreatureCmd.GainBlock(Owner.Creature, total, ValueProp.Move, cardPlay, false);
    }
    protected override void OnUpgrade() => DynamicVars["Pierce"].UpgradeValueBy(1m); // 6 → 7
}

/// <summary>顿悟（稀有技能）：3 费，虚无，消耗。本回合内看穿效果翻倍（升级后虚无移除）。</summary>
public class QianqiuEpiphany() : QianqiuCard(3, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Ethereal, CardKeyword.Exhaust];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        => await PowerCmd.Apply<QianqiuPierceAmplifyPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
    protected override void OnUpgrade() => RemoveKeyword(CardKeyword.Ethereal);
}

/// <summary>通晓（稀有技能）：3 费，消耗。消耗所有敌人的看穿层数，每层获得 1 点格挡（升级后 2 费）。</summary>
public class QianqiuComprehension() : QianqiuCard(3, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var enemies = CombatState?.HittableEnemies;
        int total = 0;
        if (enemies != null)
        {
            foreach (Creature e in enemies)
            {
                total += (int)e.GetPowerAmount<QianqiuPiercePower>();
                if (e.GetPower<QianqiuPiercePower>() is QianqiuPiercePower p)
                {
                    await PowerCmd.Remove(p);
                }
            }
        }
        await CreatureCmd.GainBlock(Owner.Creature, total, ValueProp.Move, cardPlay, false);
    }
    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1); // 3 → 2
}

/// <summary>同化（稀有技能）：2 能量 + 2 法力。选择 1 名敌人，本场战斗对方获得格挡时你获得等量格挡（升级后 1 法力）。</summary>
public class QianqiuAssimilation() : QianqiuCard(2, CardType.Skill, CardRarity.Rare, TargetType.AnyEnemy)
{
    public override int CanonicalStarCost => 2;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<QianqiuAssimilationPower>()];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null)
        {
            return;
        }
        QianqiuAssimilationPower? power = await PowerCmd.Apply<QianqiuAssimilationPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
        if (power != null)
        {
            power.BoundEnemy = cardPlay.Target;
        }
    }
    protected override void OnUpgrade() => UpgradeStarCostBy(-1); // 2 法力 → 1 法力
}
