using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.ValueProps;
using MoeNegiMod.Qianqiu.Powers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoeNegiMod.Qianqiu.Cards;

/// <summary>
/// 回溯（先古技能牌，古老牙齿升华读取所得）：1 费。
/// 本回合获得 5 点力量；获得 1 点永久力量（升级后 2）；获得 1 层透支（下回合转为乏力，乏力本回合减少力量）。
/// </summary>
public class QianqiuRetrace() : QianqiuCard(1,
    CardType.Skill, CardRarity.Ancient,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<QianqiuRetraceUpPower>("Retrace", 5m),
        new DynamicVar("Permanent", 1m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<QianqiuRetraceUpPower>(),
        HoverTipFactory.FromPower<QianqiuOverdraftPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 本回合临时 +5 力量（回合结束回滚）
        await PowerCmd.Apply<QianqiuRetraceUpPower>(choiceContext,
            Owner.Creature, DynamicVars["Retrace"].BaseValue, Owner.Creature, this);
        // 本场战斗永久力量 +1（升级 +2）
        await PowerCmd.Apply<StrengthPower>(choiceContext,
            Owner.Creature, DynamicVars["Permanent"].BaseValue, Owner.Creature, this);
        // 获得 1 层透支：下回合转为乏力
        await PowerCmd.Apply<QianqiuOverdraftPower>(choiceContext,
            Owner.Creature, 1m, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Permanent"].UpgradeValueBy(1m); // 1 → 2
    }
}
