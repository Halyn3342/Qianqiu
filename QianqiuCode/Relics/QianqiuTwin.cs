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
/// 血灵双生（欧洛巴斯之触精炼后的血灵花）：每回合获得 2 点法力，法力上限 +3，
/// 每 5 场战斗胜利获得 4 点生命值上限。
/// </summary>
public class QianqiuTwin : QianqiuRelics
{
    private int _victories;

    public override RelicRarity Rarity => RelicRarity.Starter;

    public override bool ShowCounter => true;

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
        [new StarsVar(2)];

    /// <summary>战斗开始时恢复法力至上限（Late 时机：在开始阶段之后生效）。</summary>
    public override async Task BeforeCombatStartLate()
    {
        await PlayerCmd.SetStars(Mana.GetMaxFor(Owner), Owner);
    }

    public override async Task AfterPlayerTurnStartEarly(PlayerChoiceContext choiceContext, Player player)
    {
        int max = Mana.GetMaxFor(Owner); // 已包含血灵双生 +3
        int stars = Owner.PlayerCombatState.Stars;
        if (stars > max)
        {
            await PlayerCmd.SetStars(max, Owner);
        }
        else if (stars < max)
        {
            // 只加"最多到上限"的量，避免差 1 时 +2 溢出到上限之上。
            int gain = Math.Min((int)DynamicVars.Stars.BaseValue, max - stars);
            if (gain > 0)
            {
                await PlayerCmd.GainStars(gain, Owner);
            }
        }
    }

    public override async Task AfterCombatVictory(CombatRoom room)
    {
        Victories++;
        if (Victories % 5 == 0)
        {
            Flash();
            await CreatureCmd.GainMaxHp(Owner.Creature, 4);
        }
    }
}
