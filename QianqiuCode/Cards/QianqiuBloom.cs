#nullable enable
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.ValueProps;
using MoeNegiMod.Qianqiu.Powers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoeNegiMod.Qianqiu.Cards;

/// <summary>
/// 绽放：罕见 能力牌，1 能量 + 2 法力。获得最大生命值 15%（升级 20%）的临时生命值。
/// 生命值减少时优先扣除临时生命。
/// </summary>
public class QianqiuBloom() : QianqiuCard(1,
    CardType.Power, CardRarity.Uncommon,
    TargetType.Self)
{
    // 法力费用 2
    public override int CanonicalStarCost => 2;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<QianqiuBloomPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        decimal pct = IsUpgraded ? 0.20m : 0.15m;
        int tempHp = (int)Math.Floor(Owner.Creature.MaxHp * pct);
        await PowerCmd.Apply<QianqiuBloomPower>(choiceContext,
            Owner.Creature, tempHp, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // 升级效果（15% → 20%）在 OnPlay 的 IsUpgraded 判断中处理
    }
}
