#nullable enable
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoeNegiMod.Qianqiu.Powers;

/// <summary>
/// 看穿（debuff）：对该目标造成的伤害提升 floor(层数 × 50%)。
/// 双向生效：玩家对拥有看穿的敌人、敌人对拥有看穿的玩家。
/// 该目标实际受到生命值伤害（未格挡部分 &gt; 0）后，层数 -1。
/// hover 描述通过 DamageBoost 变量直接显示具体提升数值。
/// </summary>
public sealed class QianqiuPiercePower : QianqiuPower
{
    private const string BoostKey = "DamageBoost";

    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar(BoostKey, 0m)];

    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (Owner != target)
        {
            return 0m;
        }
        // 每层 +50%，向下取整：2 层 → +1，5 层 → +2
        return Math.Floor(Amount * 0.5m);
    }

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power == this)
        {
            DynamicVars[BoostKey].BaseValue = Math.Floor(Amount * 0.5m);
            InvokeDisplayAmountChanged();
        }
    }

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        // 受到生命值伤害（未格挡部分 > 0）时层数 -1；纯被格挡不触发
        if (target == Owner && result.UnblockedDamage > 0m)
        {
            Flash();
            await PowerCmd.Decrement(this);
        }
    }
}
