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
/// 源流：罕见 能力牌，1 能量 + 2 法力，法力上限 +2（升级后 1 能量 + 1 法力）。
/// </summary>
public class QianqiuFountain() : QianqiuCard(1,
    CardType.Power, CardRarity.Uncommon,
    TargetType.Self)
{
    // 法力费用 2（升级后 -1）
    public override int CanonicalStarCost => 2;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<QianqiuFountainPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 每层 +2 法力上限（Mana.GetMaxFor 按层数计算）
        await PowerCmd.Apply<QianqiuFountainPower>(choiceContext,
            Owner.Creature, 1m, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        UpgradeStarCostBy(-1); // 法力费用 2 → 1
    }
}
