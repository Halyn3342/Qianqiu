#nullable enable
using MegaCrit.Sts2.Core.Entities.Powers;

namespace MoeNegiMod.Qianqiu.Powers;

/// <summary>
/// 源流（能力标记）：每层使法力上限 +2（由 Mana.GetMaxFor 计算）。本身无回合效果。
/// </summary>
public sealed class QianqiuFountainPower : QianqiuPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;
}
