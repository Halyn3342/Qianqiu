using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.ValueProps;
using MoeNegiMod.Qianqiu.Powers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoeNegiMod.Qianqiu.Cards;

/// <summary>
/// 契约：稀有 技能牌，2 能量 + 2 法力，消耗。选择一名敌人：
/// 这场战斗中该敌人每损失 15 点生命值（升级后 10），玩家恢复 1 点生命。
/// </summary>
public class QianqiuContract() : QianqiuCard(2,
    CardType.Skill, CardRarity.Rare,
    TargetType.AnyEnemy)
{
    // 法力费用 2
    public override int CanonicalStarCost => 2;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Threshold", 15m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<QianqiuContractPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null)
        {
            return;
        }
        // Amount = 每损失多少生命回 1（15 → 升级 10）
        await PowerCmd.Apply<QianqiuContractPower>(choiceContext,
            cardPlay.Target, DynamicVars["Threshold"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Threshold"].UpgradeValueBy(-5m); // 15 → 10
    }
}
