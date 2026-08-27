#nullable enable
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using System.Threading.Tasks;

namespace MoeNegiMod.Qianqiu.Powers;

/// <summary>
/// 源泉（能力）：每回合开始时获得 Amount 点法力（未升级 2 / 升级 3）。
/// 使用 Late 时机（在遗物重置/回蓝之后），获得的法力可以超过上限（超限部分下回合开始时由遗物移除）。
/// </summary>
public sealed class QianqiuSpringPower : QianqiuPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPlayerTurnStartLate(PlayerChoiceContext choiceContext, Player player)
    {
        await PlayerCmd.GainStars(Amount, Owner.Player);
    }
}
