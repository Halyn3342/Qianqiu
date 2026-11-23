using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
using MoeNegiMod.Qianqiu.Api;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoeNegiMod.Qianqiu.Relics;

/// <summary>
/// 血灵花（初始遗物）：每回合获得 1 点法力；战斗开始时恢复法力至上限（最后生效）；
/// 每 5 场战斗胜利获得 2 点生命值上限。
/// 每回合使用 Early 时机：先重置上一回合遗留的超限法力，再回蓝。
/// </summary>
public class QianqiuToy : QianqiuRelics
{
    private int _victories;

    public override RelicRarity Rarity => RelicRarity.Starter;

    public override bool ShowCounter => true;

    /// <summary>距离下一次生命上限提升还差的战斗胜利场数。</summary>
    public override int DisplayAmount => 5 - (Victories % 5);

    [SavedProperty]
    public int Victories
    {
        get => _victories;
        set
        {
            AssertMutable();
            _victories = value;
            InvokeDisplayAmountChanged();
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new StarsVar(1)];

    /// <summary>战斗开始时恢复法力至上限（Late 时机：在开始阶段之后生效）。</summary>
    public override async Task BeforeCombatStartLate()
    {
        await PlayerCmd.SetStars(Mana.GetMaxFor(Owner), Owner);
    }

    public override async Task AfterPlayerTurnStartEarly(PlayerChoiceContext choiceContext, Player player)
    {
        int max = Mana.GetMaxFor(Owner);
        int stars = Owner.PlayerCombatState.Stars;
        if (stars > max)
        {
            await PlayerCmd.SetStars(max, Owner);
        }
        else if (stars < max)
        {
            await PlayerCmd.GainStars(DynamicVars.Stars.BaseValue, Owner);
        }
    }

    public override async Task AfterCombatVictory(CombatRoom room)
    {
        Victories++;
        if (Victories % 5 == 0)
        {
            Flash();
            await CreatureCmd.GainMaxHp(Owner.Creature, 2);
        }
    }
}
