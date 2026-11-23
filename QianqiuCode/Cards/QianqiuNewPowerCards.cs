#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MoeNegiMod.Qianqiu.Powers;

namespace MoeNegiMod.Qianqiu.Cards;

/// <summary>共鸣（罕见能力）：1 费。每回合开始时获得 1 点法力，随机给予 1 名敌人 1 层看穿（升级获得固有）。</summary>
public class QianqiuResonance() : QianqiuCard(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        => await PowerCmd.Apply<QianqiuResonancePower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
    protected override void OnUpgrade() => AddKeyword(CardKeyword.Innate);
}

/// <summary>定义（稀有能力）：1 费。你每回合打出的前 1 张牌获得黑爪附魔（升级后前 2 张）。</summary>
public class QianqiuDefinition() : QianqiuCard(1, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Count", 1m)];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        => await PowerCmd.Apply<QianqiuDefinitionPower>(choiceContext, Owner.Creature, DynamicVars["Count"].BaseValue, Owner.Creature, this);
    protected override void OnUpgrade() => DynamicVars["Count"].UpgradeValueBy(1m);
}

/// <summary>脉动（罕见能力）：2 费。每回合结束时恢复自身最大生命值 5%（升级后 1 费）。</summary>
public class QianqiuPulse() : QianqiuCard(2, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        => await PowerCmd.Apply<QianqiuPulsePower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>感知（稀有能力）：2 费，虚无。每回合开始时对所有敌人施加 2 层看穿，获得 2 点格挡（升级后虚无移除）。</summary>
public class QianqiuPerception() : QianqiuCard(2, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Ethereal];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        => await PowerCmd.Apply<QianqiuPerceptionPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
    protected override void OnUpgrade() => RemoveKeyword(CardKeyword.Ethereal);
}

/// <summary>精通（稀有能力）：2 费。每当你将一张牌变为或附魔黑爪时，抽 1 张牌并获得 1 点法力（升级后 1 费）。</summary>
public class QianqiuMastery() : QianqiuCard(2, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        => await PowerCmd.Apply<QianqiuMasteryPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>连招（罕见能力）：1 费。每回合第 1 张黑爪牌打出后，加入 1 张 0 费随机黑爪牌到手牌（升级后 2 张）。</summary>
public class QianqiuCombo() : QianqiuCard(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Count", 1m)];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        => await PowerCmd.Apply<QianqiuComboPower>(choiceContext, Owner.Creature, DynamicVars["Count"].BaseValue, Owner.Creature, this);
    protected override void OnUpgrade() => DynamicVars["Count"].UpgradeValueBy(1m);
}

/// <summary>玉竹笛（稀有能力）：3 费，虚无。所有玩家获得 5 层再生（升级后 2 费）。</summary>
public class QianqiuJadeFlute() : QianqiuCard(3, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Ethereal];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        => await PowerCmd.Apply<RegenPower>(choiceContext, Owner.Creature, 5, Owner.Creature, this);
    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>强化药剂（罕见能力）：1 费。获得 1 点力量、1 点敏捷（升级后 2/2）。</summary>
public class QianqiuFortifyPotion() : QianqiuCard(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<StrengthPower>("Strength", 1m),
        new PowerVar<DexterityPower>("Dexterity", 1m)
    ];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, DynamicVars["Strength"].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<DexterityPower>(choiceContext, Owner.Creature, DynamicVars["Dexterity"].BaseValue, Owner.Creature, this);
    }
    protected override void OnUpgrade()
    {
        DynamicVars["Strength"].UpgradeValueBy(1m);
        DynamicVars["Dexterity"].UpgradeValueBy(1m);
    }
}

/// <summary>坚韧体魄（稀有能力）：2 费。每回合开始时获得自身最大生命值 10% 的格挡（升级后 15%）。</summary>
public class QianqiuToughBody() : QianqiuCard(2, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Percent", 10m)];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        => await PowerCmd.Apply<QianqiuToughBodyPower>(choiceContext, Owner.Creature, DynamicVars["Percent"].BaseValue, Owner.Creature, this);
    protected override void OnUpgrade() => DynamicVars["Percent"].UpgradeValueBy(5m);
}

/// <summary>攻略书（稀有能力）：1 费。每回合开始时对所有敌人施加 3 层看穿（升级后固有）。</summary>
public class QianqiuStrategyGuide() : QianqiuCard(1, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Pierce", 3m)];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        => await PowerCmd.Apply<QianqiuStrategyGuidePower>(choiceContext, Owner.Creature, DynamicVars["Pierce"].BaseValue, Owner.Creature, this);
    protected override void OnUpgrade() => AddKeyword(CardKeyword.Innate);
}

/// <summary>洞悉弱点（稀有能力）：2 费。每回合第 1 张攻击牌对拥有看穿的敌人造成 1.5 倍伤害（升级后 2 倍）。</summary>
public class QianqiuInsightWeakness() : QianqiuCard(2, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Power", 1.5m)];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        => await PowerCmd.Apply<QianqiuInsightWeaknessPower>(choiceContext, Owner.Creature, DynamicVars["Power"].BaseValue, Owner.Creature, this);
    protected override void OnUpgrade() => DynamicVars["Power"].UpgradeValueBy(0.5m);
}

/// <summary>知己知彼（罕见能力）：1 费。每当你给予看穿时获得等量的格挡（升级后保留）。</summary>
public class QianqiuKnowEnemy() : QianqiuCard(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        => await PowerCmd.Apply<QianqiuKnowEnemyPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
    protected override void OnUpgrade() => AddKeyword(CardKeyword.Retain);
}

/// <summary>黑化形态（稀有能力）：3 费。获得 5 点力量、3 点法力上限，你的任何攻击牌都被附魔黑爪（升级后保留）。</summary>
public class QianqiuBlackForm() : QianqiuCard(3, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<QianqiuBlackFormPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
        await PowerCmd.Apply<QianqiuMaxManaPower>(choiceContext, Owner.Creature, 3, Owner.Creature, this);
        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, 5, Owner.Creature, this);
    }
    protected override void OnUpgrade() => AddKeyword(CardKeyword.Retain);
}
