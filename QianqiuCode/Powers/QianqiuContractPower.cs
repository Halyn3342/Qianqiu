#nullable enable
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using System.Threading.Tasks;

namespace MoeNegiMod.Qianqiu.Powers;

/// <summary>
/// 契约（挂在敌人身上）：该敌人每损失 Amount 点生命值（HP 实际减少），打出契约的玩家恢复 1 点生命。
/// Amount = 卡传入的阈值（未升级 15，升级 10）。
/// 多人模式：按施加者独立实例（InstancedPerApplier），同一敌人身上的多个契约分开计算；
/// 治疗只作用于打出契约的玩家（Applier）。
/// </summary>
public sealed class QianqiuContractPower : QianqiuPower
{
    private class Data
    {
        public decimal accumulated;
    }

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    // 每个施加者（玩家）独立实例：同一敌人身上多个契约各自累计、各自回血
    public override PowerInstanceType InstanceType => PowerInstanceType.InstancedPerApplier;

    protected override object InitInternalData() => new Data();

    public override async Task AfterCurrentHpChanged(Creature creature, decimal delta)
    {
        if (creature != Owner || delta >= 0m)
        {
            return;
        }
        Data data = GetInternalData<Data>();
        data.accumulated += -delta;
        int healCount = (int)(data.accumulated / Amount);
        if (healCount > 0)
        {
            data.accumulated -= healCount * Amount;
            Flash();
            // 只治疗打出契约的玩家
            if (Applier?.Player?.Creature != null)
            {
                await CreatureCmd.Heal(Applier.Player.Creature, healCount);
            }
        }
    }
}
