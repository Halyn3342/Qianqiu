#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace MoeNegiMod.Qianqiu.Relics;

/// <summary>
/// 精炼之石（商店/精英掉落通用遗物）：你每回合打出的第一张牌费用-1。
/// </summary>
public class QianqiuRefinementStone : QianqiuSharedRelic
{
    private int _cardsPlayedThisTurn;

    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override bool TryModifyEnergyCostInCombatLate(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (!ShouldModifyCost(card))
        {
            return false;
        }
        modifiedCost = Math.Max(0m, originalCost - 1m);
        return true;
    }

    public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (participants.Contains(Owner.Creature))
        {
            _cardsPlayedThisTurn = 0;
        }
        return Task.CompletedTask;
    }

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!CombatManager.Instance.IsInProgress || cardPlay.IsAutoPlay || cardPlay.Card.Owner != Owner)
        {
            return Task.CompletedTask;
        }
        _cardsPlayedThisTurn++;
        return Task.CompletedTask;
    }

    private bool ShouldModifyCost(CardModel card)
    {
        if (!CombatManager.Instance.IsInProgress || card.Owner.Creature != Owner.Creature)
        {
            return false;
        }
        if (_cardsPlayedThisTurn != 0)
        {
            return false; // 只作用于每回合第一张牌
        }
        return card.Pile?.Type is PileType.Hand or PileType.Play;
    }
}
