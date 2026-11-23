#nullable enable
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using System.Threading.Tasks;

namespace MoeNegiMod.Qianqiu.Powers;

/// <summary>
/// 绽放（临时生命）：Amount = 临时生命值。
/// 生命值要减少时优先扣除临时生命（ModifyHpLost 吸收），状态栏显示剩余临时生命（血量），数字颜色 #b7dec2。
/// 临时生命耗尽（Amount ≤ 0）后自动移除（状态栏与血条显示消失）。
/// </summary>
public sealed class QianqiuBloomPower : QianqiuPower
{
    private static readonly Color _tempHpColor = new Color("3ec5ce");

    private bool _shouldRemove;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    // 状态栏数字颜色
    public override Color AmountLabelColor => _tempHpColor;

    public override decimal ModifyHpLostAfterOstyLate(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner || amount <= 0m)
        {
            return amount;
        }
        decimal absorbed = Math.Min(Amount, amount);
        if (absorbed > 0m)
        {
            Flash();
            SetAmount((int)(Amount - absorbed), silent: false);
            if (Amount <= 0m)
            {
                _shouldRemove = true;
            }
        }
        return amount - absorbed;
    }

    public override async Task AfterModifyingHpLostAfterOsty()
    {
        if (_shouldRemove)
        {
            _shouldRemove = false;
            await PowerCmd.Remove(this);
        }
    }
}
