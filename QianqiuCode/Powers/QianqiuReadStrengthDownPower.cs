#nullable enable
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoeNegiMod.Qianqiu.Powers;

/// <summary>
/// 读取（乏力）：由蓄力在回合结束时施加，本回合（下个回合）真实失去 -amount 力量。
/// 该回合结束时移除并回滚力量。
/// </summary>
public sealed class QianqiuReadStrengthDownPower : QianqiuPower
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task BeforeApplied(Creature target, decimal amount, Creature? applier, CardModel? cardSource)
    {
        // 真实减力量（amount 为正数，施加负数）
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), target, -amount, applier, cardSource, silent: true);
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (participants.Contains(Owner))
        {
            Flash();
            await PowerCmd.Remove(this);
            await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, Amount, Owner, null); // 回滚乏力的 -力量
        }
    }
}
