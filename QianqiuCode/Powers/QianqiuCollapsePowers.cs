#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace MoeNegiMod.Qianqiu.Powers;

/// <summary>
/// 崩坏（debuff）：目标回合开始时，失去 最大生命 × 层数% 点生命（无视格挡），且不低于层数点。
/// </summary>
public sealed class QianqiuCollapsePower : QianqiuPower
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (participants.Contains(Owner) && side == Owner.Side)
        {
            int n = (int)Amount;
            int maxHpLoss = (int)Math.Max(Amount / 100m * Owner.MaxHp, Amount);
            // 扣除目标的生命上限（LoseMaxHp），而非造成伤害
            await CreatureCmd.LoseMaxHp(new ThrowingPlayerChoiceContext(), Owner, maxHpLoss, false);
        }
    }
}

/// <summary>
/// 崩坏触发（挂在玩家）：你的每段攻击命中敌人时，使其获得 1 层崩坏。
/// </summary>
public sealed class QianqiuCollapseAmplifyPower : QianqiuPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterAttack(PlayerChoiceContext choiceContext, AttackCommand command)
    {
        foreach (List<DamageResult> hitList in command.Results)
        {
            foreach (DamageResult r in hitList)
            {
                if (r.UnblockedDamage > 0 && r.Receiver != null && r.Receiver != Owner)
                {
                    await PowerCmd.Apply<QianqiuCollapsePower>(choiceContext, r.Receiver, 1, Owner, command.ModelSource as MegaCrit.Sts2.Core.Models.CardModel);
                }
            }
        }
    }
}
