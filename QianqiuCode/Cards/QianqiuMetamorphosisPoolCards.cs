#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using MoeNegiMod.Qianqiu.Monsters;

namespace MoeNegiMod.Qianqiu.Cards;

/// <summary>化形·大剑（罕见攻击）：2 能量 + 1 法力。对所有敌人造成 15 点伤害，给予 2 层易伤（升级 20 伤）。</summary>
public class QianqiuGreatsword() : QianqiuCard(2, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies), IShadowTouched
{
    public override int CanonicalStarCost => 1;
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(15, ValueProp.Move),
        new PowerVar<VulnerablePower>("Vuln", 2m)
    ];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<VulnerablePower>()];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay)
            .TargetingAllOpponents(CombatState!).Execute(choiceContext);
        await PowerCmd.Apply<VulnerablePower>(choiceContext, CombatState?.HittableEnemies, DynamicVars["Vuln"].BaseValue, Owner.Creature, this);
    }
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(5m); // 15 → 20
}

/// <summary>化形·傀儡（罕见技能）：2 能量 + 2 法力。召唤一个专属傀儡（血量为自身最大生命值 20%）为你阻挡伤害。场上最多一个，已有傀儡时再次召唤会将其生命恢复至上限（升级后 1 能量 + 2 法力）。</summary>
public class QianqiuPuppet() : QianqiuCard(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self), IShadowTouched
{
    public override int CanonicalStarCost => 2;
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int hp = (int)Math.Floor(Owner.Creature.MaxHp * 0.2m);
        // 场上最多一个专属傀儡：已有傀儡（无论死活）一律复用——存活则提高上限、死亡则复活，绝不再新建。
        // 注意：傀儡因 DieForYou 死亡后不会从 _pets 移除，所以必须复用而不是"看 IsAlive 才新建"，否则会越召越多。
        Creature? existing = Owner.PlayerCombatState?.GetPet<QianqiuPuppetMonster>();
        if (existing != null)
        {
            bool wasDead = !existing.IsAlive;
            if (wasDead)
            {
                // 原版召唤（死亡）：设为上限并治疗，触发复活。
                await CreatureCmd.SetMaxHp(existing, hp);
                await CreatureCmd.Heal(existing, hp);
            }
            else
            {
                // 原版召唤（已有存活）：提升其生命上限（不再锁死，可随召唤次数增长）。
                await CreatureCmd.GainMaxHp(existing, hp);
            }
            var node = NCombatRoom.Instance?.GetCreatureNode(existing);
            // 时长 0：立即到位（不产生平移），并触发 UpdateBounds -> 刷新血量条。
            node?.OstyScaleToSize((float)existing.MaxHp, 0.0);
            // NCombatRoom 对"非 Osty 玩家宠物"会调 ToggleIsInteractable(false) 隐藏血量显示面板，
            // 这里重新设为 true 让血条显示出来。
            node?.ToggleIsInteractable(true);
            if (wasDead)
            {
                node?.StartReviveAnim(); // 死亡后召唤：播放复活动画（否则傀儡卡在死亡动画直到被打）
            }
            return;
        }
        // 新建专属傀儡：先建实体并在 AddPet 创建节点*之前*设好血量，
        // 保证血条从一开始就显示正确值（否则节点会先绑定 1/1，且血量变更未必触发刷新）。
        Creature pet = CombatState!.CreateCreature(
            ModelDb.Monster<QianqiuPuppetMonster>().ToMutable(), Owner.Creature.Side, null);
        pet.SetMaxHpInternal(hp);
        pet.SetCurrentHpInternal(hp);
        await PlayerCmd.AddPet(pet, Owner);
        await PowerCmd.Apply<DieForYouPower>(new ThrowingPlayerChoiceContext(), pet, 1m, null, null);
        var petNode = NCombatRoom.Instance?.GetCreatureNode(pet);
        petNode?.OstyScaleToSize((float)pet.MaxHp, 0.0);
        petNode?.ToggleIsInteractable(true); // 同上：重新显示血量显示面板
    }
    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1); // 2 费 → 1 费（法力仍 2）
}
