using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MoeNegiMod.Qianqiu.Powers;

namespace MoeNegiMod.Qianqiu.Relics;

/// <summary>
/// 洞察（商店/精英掉落通用遗物）：战斗开始时对随机一名敌人施加 3 层看穿。
/// </summary>
public class QianqiuInsight : QianqiuSharedRelic
{
    public override RelicRarity Rarity => RelicRarity.Common;

    public override async Task BeforeCombatStartLate()
    {
        var enemies = Owner.Creature.CombatState?.HittableEnemies;
        if (enemies is not { Count: > 0 })
        {
            return;
        }
        Creature? enemy = Owner.RunState.Rng.CombatTargets.NextItem(enemies);
        if (enemy != null)
        {
            await PowerCmd.Apply<QianqiuPiercePower>(new ThrowingPlayerChoiceContext(), enemy, 3, Owner.Creature, null);
        }
    }
}
