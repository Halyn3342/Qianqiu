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
/// 读取（蓄力）：施加时真实获得 +amount 力量（StrengthPower），本回合结束时移除并回滚。
/// 乏力由卡在打出时直接施加"透支"，下回合转化为乏力（不再由本 Power 转）。
/// </summary>
public sealed class QianqiuReadStrengthUpPower : QianqiuPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task BeforeApplied(Creature target, decimal amount, Creature? applier, CardModel? cardSource)
    {
        // 真实加力量（与 TemporaryStrengthPower 同款做法）
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), target, amount, applier, cardSource, silent: true);
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (participants.Contains(Owner))
        {
            Flash();
            await PowerCmd.Remove(this);
            await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, -Amount, Owner, null); // 回滚本回合的 +力量
        }
    }
}
