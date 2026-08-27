#nullable enable
using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using MoeNegiMod.Qianqiu.Api;

namespace MoeNegiMod.Qianqiu.Relics;

/// <summary>
/// 血灵剑（稀有通用遗物）：每当有敌人死亡时，你获得其最大生命值 5% 的最大生命值。
/// </summary>
public class QianqiuBloodSword : QianqiuSharedRelic
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
    {
        // 瀑布巨兽爆炸阶段会先把最大生命值改成 999999999 再自爆；
        // 此时死亡不结算，避免拿到爆炸态的巨大生命值（SteamEruptionPower 爆炸前已被移除，故改用 MaxHp 判断）。
        if (creature.Side != Owner.Creature.Side && !creature.IsPlayer
            && !creature.HasPower<SteamEruptionPower>()
            && creature.MaxHp < 999_999_999m)
        {
            int gain = (int)Math.Floor(creature.MaxHp * 0.05m);
            await CreatureCmd.GainMaxHp(Owner.Creature, gain);
        }
    }
}

/// <summary>
/// 龙的粪便（通用遗物）：你的再生不会在你的回合开始时减少。
/// </summary>
public class QianqiuDragonFeces : QianqiuSharedRelic
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    public override bool TryModifyPowerAmountReceived(PowerModel canonicalPower, Creature target, decimal amount, Creature? applier, out decimal modifiedAmount)
    {
        modifiedAmount = amount;
        if (canonicalPower is RegenPower && target == Owner.Creature && amount < 0)
        {
            modifiedAmount = 0; // 再生不再减少
            return true;
        }
        return false;
    }
}

/// <summary>灵果一瓣（稀有通用遗物）：每场战斗结束时恢复 10 点生命。</summary>
public class QianqiuSpiritFruit : QianqiuSharedRelic
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    public override async Task AfterCombatVictory(CombatRoom room)
    {
        if (!Owner.Creature.IsDead)
        {
            await CreatureCmd.Heal(Owner.Creature, 10, false);
        }
    }
}

/// <summary>肉丸冬瓜汤（罕见通用遗物）：战斗开始时获得 3 层再生与 3 点敏捷。</summary>
public class QianqiuMeatballSoup : QianqiuSharedRelic
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override async Task BeforeCombatStartLate()
    {
        var ctx = new ThrowingPlayerChoiceContext();
        await PowerCmd.Apply<RegenPower>(ctx, Owner.Creature, 3, Owner.Creature, null);
        await PowerCmd.Apply<DexterityPower>(ctx, Owner.Creature, 3, Owner.Creature, null);
    }
}

/// <summary>仙参（稀有通用遗物）：进入商店时获得 433 金币，然后此遗物失效。</summary>
public class QianqiuImmortalGinseng : QianqiuSharedRelic
{
    private bool _used;

    public override RelicRarity Rarity => RelicRarity.Rare;

    public override bool IsUsedUp => _used;

    public override async Task AfterRoomEntered(AbstractRoom room)
    {
        if (!_used && room is MerchantRoom)
        {
            Flash();
            await PlayerCmd.GainGold(433, Owner, false);
            _used = true;
        }
    }
}

/// <summary>初级潜行斗篷（商店通用遗物）：战斗开始时获得 1 点力量与 1 点敏捷。</summary>
public class QianqiuCloak : QianqiuSharedRelic
{
    public override RelicRarity Rarity => RelicRarity.Shop;

    public override async Task BeforeCombatStartLate()
    {
        var ctx = new ThrowingPlayerChoiceContext();
        await PowerCmd.Apply<StrengthPower>(ctx, Owner.Creature, 1, Owner.Creature, null);
        await PowerCmd.Apply<DexterityPower>(ctx, Owner.Creature, 1, Owner.Creature, null);
    }
}

/// <summary>蛙蛙的头（罕见通用遗物）：法力上限 +2；每回合开始时恢复 1 点法力。</summary>
public class QianqiuFrogHead : QianqiuSharedRelic
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override async Task AfterPlayerTurnStartEarly(PlayerChoiceContext choiceContext, Player player)
    {
        if (player == Owner)
        {
            // 每回合恢复 1 点法力，但不会超过法力上限。
            int max = Mana.GetMaxFor(Owner);
            int stars = Owner.PlayerCombatState.Stars;
            if (stars < max)
            {
                await PlayerCmd.GainStars(1, Owner);
            }
        }
    }
}
