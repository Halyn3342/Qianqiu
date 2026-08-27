#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.ValueProps;
using MoeNegiMod.Qianqiu.Character;

namespace MoeNegiMod.Qianqiu.Cards;

/// <summary>
/// 启示录（先古能力）：3 能量 + 3 法力。将一张"游戏结束！"加入手牌（升级后 2 能量 + 3 法力）。
/// </summary>
public class QianqiuApocalypse() : QianqiuCard(3, CardType.Power, CardRarity.Ancient, TargetType.Self)
{
    public override int CanonicalStarCost => 3;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CardModel go = CombatState!.CreateCard(ModelDb.Card<QianqiuGameOver>(), Owner);
        await CardPileCmd.AddGeneratedCardToCombat(go, PileType.Hand, Owner);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1); // 3 费 → 2 费（法力仍 3）
}

/// <summary>
/// 游戏结束！（稀有技能）：9 费，保留，消耗。给予自身所有基础正面效果各 1 层
/// （不包括难以杀灭、硬化外壳，也不包括能力牌/技能牌给予的正面效果）。
/// 回合结束时若此牌在手牌中则费用 -1（升级后 -2），直到被打出。
/// 在角色卡池中但被 QianqiuCardPool.FilterThroughEpochs 排除出"可获得"列表，
/// 因此不进入奖励/商店/尘封魔典，只能通过"启示录"直接创建获得。
/// </summary>
public class QianqiuGameOver() : QianqiuCard(9, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    // 基础正面效果白名单（经典基础 Buff）
    private static readonly HashSet<Type> _baseBuffs = new()
    {
        typeof(StrengthPower),     // 力量
        typeof(DexterityPower),    // 敏捷
        typeof(IntangiblePower),   // 无实体
        typeof(RegenPower),        // 再生
        typeof(ThornsPower),       // 荆棘
        typeof(VigorPower),        // 威势
        typeof(ArtifactPower),     // 人工制品
        typeof(BufferPower),       // 缓冲
        typeof(FocusPower),        // 集中
        typeof(RitualPower),       // 仪式
        typeof(BlurPower),         // 模糊
        typeof(BarricadePower),    // 堡垒
    };

    // 基础负面效果白名单（经典基础 Debuff）
    private static readonly HashSet<Type> _baseDebuffs = new()
    {
        typeof(VulnerablePower),     // 易伤
        typeof(WeakPower),           // 虚弱
        typeof(FrailPower),          // 脆弱
        typeof(PoisonPower),         // 中毒
        typeof(DoomPower),           // 灾厄
        typeof(DebilitatePower),     // 摧残
        typeof(ConstrictPower),      // 紧缠
        typeof(DemisePower),         // 消亡
        typeof(DisintegrationPower), // 瓦解
        typeof(FlankingPower),       // 夹击
        typeof(HangPower),           // 吊杀
        typeof(ImbalancedPower),     // 失衡
        typeof(KnockdownPower),      // 击倒
        typeof(NeurosurgePower),     // 精神过载
        typeof(OblivionPower),       // 湮灭
        typeof(ShrinkPower),         // 缩小
        typeof(SicEmPower),          // 紧追不放
        typeof(SlowPower),           // 缓慢
        typeof(StranglePower),       // 紧勒
        typeof(TagTeamPower),        // 多人组队
    };

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain, CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await ApplyEffects(choiceContext);
        // 若打出此牌时费用为 0，则重放 1 次（All 包含"直到打出"的临时减费，OnPlay 期间仍有效）
        if (EnergyCost.GetWithModifiers(CostModifiers.All) == 0)
        {
            await ApplyEffects(choiceContext);
        }
    }

    private async Task ApplyEffects(PlayerChoiceContext choiceContext)
    {
        // 自身：基础正面效果 5 层（AllPowers 返回规范模型，先 ToMutable() 再施加）
        foreach (PowerModel p in ModelDb.AllPowers.Where(x => _baseBuffs.Contains(x.GetType())))
        {
            await PowerCmd.Apply(choiceContext, p.ToMutable(), Owner.Creature, 5, Owner.Creature, this);
        }
        // 所有敌人：基础负面效果 5 层，并失去 5 点力量与 5 点敏捷
        var enemies = CombatState?.HittableEnemies;
        if (enemies != null)
        {
            foreach (PowerModel p in ModelDb.AllPowers.Where(x => _baseDebuffs.Contains(x.GetType())))
            {
                foreach (Creature e in enemies)
                {
                    await PowerCmd.Apply(choiceContext, p.ToMutable(), e, 5, Owner.Creature, this);
                }
            }
            foreach (Creature e in enemies)
            {
                await PowerCmd.Apply(choiceContext, ModelDb.Power<StrengthPower>().ToMutable(), e, -5, Owner.Creature, this);
                await PowerCmd.Apply(choiceContext, ModelDb.Power<DexterityPower>().ToMutable(), e, -5, Owner.Creature, this);
            }
        }
    }

    // 用 AfterSideTurnEnd 实现"回合结束时若在手牌中则费用-1（升级-2）"——
    // 不能用 HasTurnEndInHandEffect（那会把牌移出到手牌，保留词条就不生效了）。
    public override Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (participants.Contains(Owner.Creature) && Pile?.Type == PileType.Hand)
        {
            EnergyCost.AddUntilPlayed(IsUpgraded ? -2 : -1); // 每回合在手牌中 -1（升级 -2），直到打出
        }
        return Task.CompletedTask;
    }
}
