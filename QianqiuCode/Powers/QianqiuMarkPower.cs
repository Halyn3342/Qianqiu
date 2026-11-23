#nullable enable
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using MoeNegiMod.Qianqiu.Cards;

namespace MoeNegiMod.Qianqiu.Powers;

/// <summary>
/// 印记（正面效果）：你拥有影触附魔的牌造成伤害 +Amount、获得格挡 +Amount。
/// </summary>
public sealed class QianqiuMarkPower : QianqiuPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (cardSource != null && QianqiuShadow.IsShadowTouched(cardSource, Owner))
        {
            return Amount;
        }
        return 0m;
    }

    public override decimal ModifyBlockAdditive(Creature target, decimal block, ValueProp props, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (cardSource != null && QianqiuShadow.IsShadowTouched(cardSource, Owner))
        {
            return Amount;
        }
        return 0m;
    }
}
