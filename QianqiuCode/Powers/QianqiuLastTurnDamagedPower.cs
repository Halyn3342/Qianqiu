#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace MoeNegiMod.Qianqiu.Powers;

/// <summary>
/// 反击标记（挂在玩家）：记录"上回合是否受到过生命值伤害"（供反击查询）。
/// </summary>
public sealed class QianqiuLastTurnDamagedPower : QianqiuPower
{
    private class Data
    {
        public bool thisTurn;
        public bool lastTurn;
    }

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>上回合是否受到过生命值伤害。</summary>
    public bool WasDamagedLastTurn => GetInternalData<Data>().lastTurn;

    public override Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target == Owner && result.UnblockedDamage > 0)
        {
            GetInternalData<Data>().thisTurn = true;
        }
        return Task.CompletedTask;
    }

    public override Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (participants.Contains(Owner))
        {
            Data d = GetInternalData<Data>();
            d.lastTurn = d.thisTurn;
            d.thisTurn = false;
        }
        return Task.CompletedTask;
    }
}
