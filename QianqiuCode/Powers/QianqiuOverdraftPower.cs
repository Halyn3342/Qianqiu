#nullable enable
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using System.Threading.Tasks;

namespace MoeNegiMod.Qianqiu.Powers;

/// <summary>
/// 透支（负面效果）：打出读取/回溯后获得。下回合开始时转化为等量层数的乏力
/// （乏力在本回合减少力量），随后移除。
/// </summary>
public sealed class QianqiuOverdraftPower : QianqiuPower
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPlayerTurnStartLate(PlayerChoiceContext choiceContext, Player player)
    {
        // 下回合开始：转化为乏力（乏力在本回合减少力量）
        await PowerCmd.Apply<QianqiuReadStrengthDownPower>(choiceContext, Owner, Amount, Owner, null);
        await PowerCmd.Remove(this);
    }
}
