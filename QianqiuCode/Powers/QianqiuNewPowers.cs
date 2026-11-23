#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using MoeNegiMod.Qianqiu.Cards;

namespace MoeNegiMod.Qianqiu.Powers;

/// <summary>共鸣（能力）：每回合开始时获得 1 点法力，随机给予 1 名敌人 1 层看穿。</summary>
public sealed class QianqiuResonancePower : QianqiuPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != Owner.Side)
        {
            return;
        }
        await PlayerCmd.GainStars(1, Owner.Player);
        var enemies = combatState.HittableEnemies;
        if (enemies != null && enemies.Count > 0)
        {
            Creature target = Owner.Player.PlayerRng.Transformations.NextItem(enemies);
            await PowerCmd.Apply<QianqiuPiercePower>(new ThrowingPlayerChoiceContext(), target, 1, Owner, null);
        }
    }
}

/// <summary>定义（能力）：你每回合打出的前 N(Amount) 张牌获得黑爪附魔。</summary>
public sealed class QianqiuDefinitionPower : QianqiuPower
{
    private int _cardsPlayedThisTurn;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override Task AfterPlayerTurnStartLate(PlayerChoiceContext choiceContext, Player player)
    {
        _cardsPlayedThisTurn = 0;
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.IsAutoPlay || cardPlay.Card.Owner?.Creature != Owner)
        {
            return;
        }
        if (_cardsPlayedThisTurn < (int)Amount)
        {
            QianqiuShadow.Enchant(cardPlay.Card, choiceContext);
            _cardsPlayedThisTurn++;
        }
    }
}

/// <summary>脉动（能力）：每回合结束时恢复自身最大生命值 5%。</summary>
public sealed class QianqiuPulsePower : QianqiuPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != Owner.Side)
        {
            return;
        }
        int heal = (int)Math.Floor(Owner.MaxHp * 0.05m);
        await CreatureCmd.Heal(Owner, heal, false);
    }
}

/// <summary>感知（能力）：每回合开始时对所有敌人施加 2 层看穿，获得 2 点格挡。</summary>
public sealed class QianqiuPerceptionPower : QianqiuPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != Owner.Side)
        {
            return;
        }
        await PowerCmd.Apply<QianqiuPiercePower>(new ThrowingPlayerChoiceContext(), combatState.HittableEnemies, 2, Owner, null);
        await CreatureCmd.GainBlock(Owner, 2, ValueProp.Move, null, false);
    }
}

/// <summary>精通（能力）：每当你将一张牌变为或附魔黑爪时，抽 1 张牌并获得 1 点法力。</summary>
public sealed class QianqiuMasteryPower : QianqiuPower
{
    private bool _subscribed;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override Task BeforeApplied(Creature target, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (!_subscribed)
        {
            QianqiuShadow.Enchanted += OnShadowEnchanted;
            _subscribed = true;
        }
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 黑化形态下，每打出一张（非静态黑爪的）攻击牌即视为"黑爪附魔"。
        if (cardPlay.Card.Owner?.Creature == Owner
            && cardPlay.Card is not IShadowTouched
            && cardPlay.Card.Type == CardType.Attack
            && Owner.HasPower<QianqiuBlackFormPower>())
        {
            await Trigger(choiceContext);
        }
    }

    private async void OnShadowEnchanted(CardModel card, PlayerChoiceContext context)
    {
        if (card.Owner?.Creature != Owner)
        {
            return;
        }
        await Trigger(context);
    }

    private async Task Trigger(PlayerChoiceContext context)
    {
        await CardPileCmd.Draw(context, 1, Owner.Player);
        await PlayerCmd.GainStars(1, Owner.Player);
    }
}

/// <summary>连招（能力）：每回合第 1 张黑爪牌打出后，加入 Amount 张 0 费随机黑爪牌到手牌。</summary>
public sealed class QianqiuComboPower : QianqiuPower
{
    private bool _triggeredThisTurn;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override Task AfterPlayerTurnStartLate(PlayerChoiceContext choiceContext, Player player)
    {
        _triggeredThisTurn = false;
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (_triggeredThisTurn || cardPlay.IsAutoPlay || !QianqiuShadow.IsShadowTouched(cardPlay.Card, Owner))
        {
            return;
        }
        _triggeredThisTurn = true;

        CardModel[] tokens = [ModelDb.Card<QianqiuStab>(), ModelDb.Card<QianqiuShield>(), ModelDb.Card<QianqiuLance>(), ModelDb.Card<QianqiuCrossbow>(), ModelDb.Card<QianqiuAxe>(), ModelDb.Card<QianqiuMusket>()];
        for (int i = 0; i < (int)Amount; i++)
        {
            CardModel model = Owner.Player.PlayerRng.Transformations.NextItem(tokens);
            // 用战斗卡池创建（带 CardScope），否则手牌无法识别、卡在空中
            CardModel card = Owner.CombatState.CreateCard(model, Owner.Player);
            card.EnergyCost.SetThisCombat(0); // 能量 0，法力需求保持
            card.AddKeyword(CardKeyword.Exhaust); // 带消耗
            await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, Owner.Player);
        }
    }
}

/// <summary>坚韧体魄（能力）：每回合开始时获得自身最大生命值 Amount% 的格挡。</summary>
public sealed class QianqiuToughBodyPower : QianqiuPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != Owner.Side)
        {
            return;
        }
        int block = (int)Math.Floor(Owner.MaxHp * Amount / 100m);
        await CreatureCmd.GainBlock(Owner, block, ValueProp.Move, null, false);
    }
}

/// <summary>攻略书（能力）：每回合开始时对所有敌人施加 Amount 层看穿。</summary>
public sealed class QianqiuStrategyGuidePower : QianqiuPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != Owner.Side)
        {
            return;
        }
        await PowerCmd.Apply<QianqiuPiercePower>(new ThrowingPlayerChoiceContext(), combatState.HittableEnemies, Amount, Owner, null);
    }
}

/// <summary>洞悉弱点（能力）：每回合第 1 张攻击牌对拥有看穿的敌人造成 Amount 倍伤害。</summary>
public sealed class QianqiuInsightWeaknessPower : QianqiuPower
{
    private int _attacksThisTurn;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override Task AfterPlayerTurnStartLate(PlayerChoiceContext choiceContext, Player player)
    {
        _attacksThisTurn = 0;
        return Task.CompletedTask;
    }

    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (dealer == Owner && _attacksThisTurn == 0 && cardSource?.Type == CardType.Attack
            && target != null && target.HasPower<QianqiuPiercePower>())
        {
            return Amount;
        }
        return 1m;
    }

    public override Task AfterAttack(PlayerChoiceContext choiceContext, AttackCommand command)
    {
        _attacksThisTurn++;
        return Task.CompletedTask;
    }
}

/// <summary>知己知彼（能力）：每当你给予看穿时，获得等量的格挡。</summary>
public sealed class QianqiuKnowEnemyPower : QianqiuPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power is QianqiuPiercePower && applier == Owner && amount > 0)
        {
            // Unpowered：绕过脆弱等"获得格挡减少"的效果，始终获得等量格挡。
            await CreatureCmd.GainBlock(Owner, amount, ValueProp.Move | ValueProp.Unpowered, null, false);
        }
    }
}

/// <summary>黑化形态（能力标识）：有它时你的所有攻击牌视为黑爪附魔。</summary>
public sealed class QianqiuBlackFormPower : QianqiuPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
}

/// <summary>黑化形态提供的法力上限加成（Amount = 加成值）。</summary>
public sealed class QianqiuMaxManaPower : QianqiuPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
}

/// <summary>反击姿态：回合结束后，你受到攻击时对攻击者造成 Amount 点伤害（每段），下次我方回合开始时移除。</summary>
public sealed class QianqiuCounterStancePower : QianqiuPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target == Owner && dealer != null && dealer.Side != Owner.Side && props.IsPoweredAttack() && result.TotalDamage > 0)
        {
            await CreatureCmd.Damage(choiceContext, dealer, Amount, ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, null, null);
        }
    }

    public override async Task AfterPlayerTurnStartLate(PlayerChoiceContext choiceContext, Player player)
    {
        await PowerCmd.Remove(this);
    }
}

/// <summary>瞬步：你打出的下一张牌费用 -1（本回合内），未使用时回合结束移除。</summary>
public sealed class QianqiuNextCardCostDownPower : QianqiuPower
{
    private CardModel? _source;
    private bool _used;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override Task BeforeApplied(Creature target, decimal amount, Creature? applier, CardModel? cardSource)
    {
        _source = cardSource;
        return Task.CompletedTask;
    }

    public override bool TryModifyEnergyCostInCombatLate(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (!CombatManager.Instance.IsInProgress || card.Owner?.Creature != Owner)
        {
            return false;
        }
        if (card == _source || _used)
        {
            return false; // 不作用于瞬步本身，也只用一次
        }
        if (card.Pile?.Type is not (PileType.Hand or PileType.Play))
        {
            return false;
        }
        modifiedCost = Math.Max(0m, originalCost - 1m);
        return true;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card != _source && cardPlay.Card.Owner?.Creature == Owner)
        {
            _used = true;
            await PowerCmd.Remove(this); // 用掉后移除
        }
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side == Owner.Side)
        {
            await PowerCmd.Remove(this);
        }
    }
}

/// <summary>灵敏黑爪：本回合内每打出一张黑爪牌获得 Amount 点格挡。</summary>
public sealed class QianqiuAgileClawPower : QianqiuPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner?.Creature == Owner && QianqiuShadow.IsShadowTouched(cardPlay.Card, Owner))
        {
            await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Move, null, false);
        }
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side == Owner.Side)
        {
            await PowerCmd.Remove(this);
        }
    }
}

/// <summary>赋能：本回合内下一张黑爪牌造成伤害 +Amount（一次性）。</summary>
public sealed class QianqiuEmpowerPower : QianqiuPower
{
    private bool _consumed;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (!_consumed && cardSource != null && QianqiuShadow.IsShadowTouched(cardSource, Owner))
        {
            _consumed = true;
            return Amount;
        }
        return 0m;
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side == Owner.Side)
        {
            await PowerCmd.Remove(this);
        }
    }
}

/// <summary>庇护：本回合结束时恢复所受到伤害 Amount% 的生命值。</summary>
public sealed class QianqiuShelterPower : QianqiuPower
{
    private decimal _damageTaken;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target == Owner)
        {
            _damageTaken += result.TotalDamage;
        }
        return Task.CompletedTask;
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side == Owner.Side)
        {
            int heal = (int)Math.Floor(_damageTaken * Amount / 100m);
            if (heal > 0)
            {
                await CreatureCmd.Heal(Owner, heal, false);
            }
            await PowerCmd.Remove(this);
        }
    }
}

/// <summary>回响：本回合内每打出 2 张牌恢复 Amount 点生命值。</summary>
public sealed class QianqiuEchoPower : QianqiuPower
{
    private int _cardsPlayed;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner?.Creature != Owner)
        {
            return;
        }
        _cardsPlayed++;
        while (_cardsPlayed >= 2)
        {
            _cardsPlayed -= 2;
            await CreatureCmd.Heal(Owner, Amount, false);
        }
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side == Owner.Side)
        {
            await PowerCmd.Remove(this);
        }
    }
}

/// <summary>顿悟：本回合内看穿效果翻倍（对打出者有效，回合结束移除）。</summary>
public sealed class QianqiuPierceAmplifyPower : QianqiuPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (dealer == Owner && target != null && target.HasPower<QianqiuPiercePower>())
        {
            // 与看穿自身提供的加成等量，达到翻倍
            return Math.Floor(target.GetPowerAmount<QianqiuPiercePower>() * 0.5m);
        }
        return 0m;
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side == Owner.Side)
        {
            await PowerCmd.Remove(this);
        }
    }
}

/// <summary>同化：绑定敌人获得格挡时，你获得等量格挡（本场战斗）。</summary>
public sealed class QianqiuAssimilationPower : QianqiuPower
{
    public Creature? BoundEnemy { get; set; }

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterBlockGained(Creature creature, decimal amount, ValueProp props, CardModel? cardSource)
    {
        if (creature == BoundEnemy && amount > 0)
        {
            await CreatureCmd.GainBlock(Owner, amount, ValueProp.Move, null, false);
        }
    }
}
