#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.ValueProps;
using MoeNegiMod.Qianqiu.Powers;

namespace MoeNegiMod.Qianqiu.Cards;

/// <summary>观察（普通技能）：1 费。获得 3 点格挡，给予所有敌人 2 层看穿（升级 5 格挡/3 层）。</summary>
public class QianqiuObservation() : QianqiuCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(3, ValueProp.Move),
        new PowerVar<QianqiuPiercePower>("Pierce", 2m)
    ];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<QianqiuPiercePower>()];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        await PowerCmd.Apply<QianqiuPiercePower>(choiceContext, CombatState?.HittableEnemies, DynamicVars["Pierce"].BaseValue, Owner.Creature, this);
    }
    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2m);
        DynamicVars["Pierce"].UpgradeValueBy(1m);
    }
}

/// <summary>流影（普通技能）：1 费。获得 2 点法力，抽 1 张牌（升级抽 2）。</summary>
public class QianqiuFlowingShadow() : QianqiuCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new StarsVar(2),
        new CardsVar(1)
    ];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PlayerCmd.GainStars(DynamicVars.Stars.BaseValue, Owner);
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
    }
    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1m);
}

/// <summary>闪避（普通技能）：1 费。获得 6 点格挡；若这是本回合第 1 张打出的牌，抽 1 张牌（升级后 0 费）。</summary>
public class QianqiuDodge() : QianqiuCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(6, ValueProp.Move),
        new CardsVar(1)
    ];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        int playedThisTurn = CombatManager.Instance.History.CardPlaysFinished.Count(e => e.HappenedThisTurn(CombatState) && e.CardPlay.Player == Owner);
        if (playedThisTurn <= 1)
        {
            await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
        }
    }
    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>虚实转换（罕见技能）：1 费。选择 1 张手牌变为随机黑爪牌，获得 1 点法力（升级 2 点）。</summary>
public class QianqiuIllusionShift() : QianqiuCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new StarsVar(1)];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CardModel? card = (await CardSelectCmd.FromHand(
            prefs: new CardSelectorPrefs(SelectionScreenPrompt, 1),
            context: choiceContext, player: Owner,
            filter: c => c.IsTransformable, source: this)).FirstOrDefault();
        if (card == null)
        {
            return;
        }
        CardModel[] options = [ModelDb.Card<QianqiuStab>(), ModelDb.Card<QianqiuShield>(), ModelDb.Card<QianqiuLance>(), ModelDb.Card<QianqiuCrossbow>(), ModelDb.Card<QianqiuAxe>(), ModelDb.Card<QianqiuMusket>()];
        CardModel replacement = card.CardScope.CreateCard(Owner.PlayerRng.Transformations.NextItem(options), card.Owner);
        await CardCmd.Transform(card, replacement);
        QianqiuShadow.Enchant(replacement, choiceContext);
        await PlayerCmd.GainStars(DynamicVars.Stars.BaseValue, Owner);
    }
    protected override void OnUpgrade() => DynamicVars.Stars.UpgradeValueBy(1m);
}

/// <summary>反击姿态（罕见技能）：1 费。获得 4 点格挡；回合结束后，你受到攻击时对攻击者造成 3 点伤害（每段，升级 4）。</summary>
public class QianqiuCounterStance() : QianqiuCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(4, ValueProp.Move),
        new DynamicVar("Power", 3m)
    ];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        await PowerCmd.Apply<QianqiuCounterStancePower>(choiceContext, Owner.Creature, DynamicVars["Power"].BaseValue, Owner.Creature, this);
    }
    protected override void OnUpgrade() => DynamicVars["Power"].UpgradeValueBy(1m);
}

/// <summary>瞬步（罕见技能）：1 能量 + 1 法力。抽 1 张牌，本回合内下一张牌费用 -1（升级抽 2）。</summary>
public class QianqiuFlashStep() : QianqiuCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override int CanonicalStarCost => 1;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1)];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<QianqiuNextCardCostDownPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
    }
    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1m);
}

/// <summary>灵敏黑爪（罕见技能）：1 费。本回合内每打出过 1 张黑爪牌获得 2 点格挡（升级 3）。附魔黑爪也算。</summary>
public class QianqiuAgileClaw() : QianqiuCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Power", 2m)];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 本回合已打出的黑爪牌（含被附魔的）
        int past = CombatManager.Instance.History.CardPlaysFinished.Count(e =>
            e.HappenedThisTurn(CombatState) && e.CardPlay.Player == Owner
            && QianqiuShadow.IsShadowTouched(e.CardPlay.Card, Owner.Creature));
        await CreatureCmd.GainBlock(Owner.Creature, past * DynamicVars["Power"].BaseValue, ValueProp.Move, cardPlay, false);
        // 之后打出的黑爪牌每张也提供格挡
        await PowerCmd.Apply<QianqiuAgileClawPower>(choiceContext, Owner.Creature, DynamicVars["Power"].BaseValue, Owner.Creature, this);
    }
    protected override void OnUpgrade() => DynamicVars["Power"].UpgradeValueBy(1m);
}

/// <summary>法力灌注（普通技能）：1 费。获得 2 点法力（升级 3）。</summary>
public class QianqiuManaInfusion() : QianqiuCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new StarsVar(2)];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        => await PlayerCmd.GainStars(DynamicVars.Stars.BaseValue, Owner);
    protected override void OnUpgrade() => DynamicVars.Stars.UpgradeValueBy(1m);
}

/// <summary>化盾（罕见技能）：1 能量 + 1 法力。选择 1 张手牌变为盾化（升级后 0 费）。</summary>
public class QianqiuShieldify() : QianqiuCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override int CanonicalStarCost => 1;
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CardModel? card = (await CardSelectCmd.FromHand(
            prefs: new CardSelectorPrefs(SelectionScreenPrompt, 1),
            context: choiceContext, player: Owner,
            filter: c => c.IsTransformable, source: this)).FirstOrDefault();
        if (card == null)
        {
            return;
        }
        CardModel replacement = card.CardScope.CreateCard(ModelDb.Card<QianqiuShield>(), card.Owner);
        await CardCmd.Transform(card, replacement);
        QianqiuShadow.Enchant(replacement, choiceContext);
    }
    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>赋能（罕见技能）：1 能量 + 1 法力。本回合内下一张黑爪牌伤害 +5（升级 +6）。</summary>
public class QianqiuEmpower() : QianqiuCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override int CanonicalStarCost => 1;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Power", 5m)];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        => await PowerCmd.Apply<QianqiuEmpowerPower>(choiceContext, Owner.Creature, DynamicVars["Power"].BaseValue, Owner.Creature, this);
    protected override void OnUpgrade() => DynamicVars["Power"].UpgradeValueBy(1m);
}

/// <summary>镜像（稀有技能）：2 能量 + 1 法力，消耗。复制手牌中的 1 张牌，本回合免费打出，复制牌将带有消耗（升级后移除消耗，费用不变）。</summary>
public class QianqiuMirror() : QianqiuCard(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    public override int CanonicalStarCost => 1;
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CardModel? chosen = (await CardSelectCmd.FromHand(
            prefs: new CardSelectorPrefs(SelectionScreenPrompt, 1),
            context: choiceContext, player: Owner,
            filter: _ => true, source: this)).FirstOrDefault();
        if (chosen != null)
        {
            CardModel copy = chosen.CreateClone();
            copy.SetToFreeThisTurn();
            copy.AddKeyword(CardKeyword.Exhaust); // 消耗
            await CardPileCmd.AddGeneratedCardToCombat(copy, PileType.Hand, Owner);
        }
    }
    protected override void OnUpgrade() => RemoveKeyword(CardKeyword.Exhaust); // 费用不变，仅移除消耗
}

/// <summary>千变万化（稀有技能）：2 能量 + 2 法力。选择抽牌堆中的任意张牌变为免费的盾化（升级后 1 法力）。</summary>
public class QianqiuTransformative() : QianqiuCard(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    public override int CanonicalStarCost => 2;
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var selected = (await CardSelectCmd.FromCombatPile(
            choiceContext, Owner.PlayerCombatState.DrawPile, Owner,
            new CardSelectorPrefs(SelectionScreenPrompt, 0, 999),
            c => c.IsTransformable)).ToList();
        foreach (CardModel card in selected)
        {
            CardModel replacement = card.CardScope.CreateCard(ModelDb.Card<QianqiuShield>(), card.Owner);
            replacement.EnergyCost.SetThisCombat(0); // 免费
            replacement.AddKeyword(CardKeyword.Ethereal); // 虚无
            await CardCmd.Transform(card, replacement);
            QianqiuShadow.Enchant(replacement, choiceContext);
        }
    }
    protected override void OnUpgrade() => UpgradeStarCostBy(-1); // 2 法力 → 1 法力
}

/// <summary>共振（稀有技能）：1 能量 + 3 法力，消耗。获得 2 点力量，所有敌人失去 1 点力量（升级 3 力/敌失 2 力）。</summary>
public class QianqiuCoResonance() : QianqiuCard(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    public override int CanonicalStarCost => 3;
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<StrengthPower>("Strength", 2m),
        new DynamicVar("EnemyLoss", 1m)
    ];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, DynamicVars["Strength"].BaseValue, Owner.Creature, this);
        var enemies = CombatState?.HittableEnemies;
        if (enemies != null)
        {
            foreach (Creature e in enemies)
            {
                await PowerCmd.Apply<StrengthPower>(choiceContext, e, -DynamicVars["EnemyLoss"].BaseValue, Owner.Creature, this);
            }
        }
    }
    protected override void OnUpgrade()
    {
        DynamicVars["Strength"].UpgradeValueBy(1m);
        DynamicVars["EnemyLoss"].UpgradeValueBy(1m);
    }
}
