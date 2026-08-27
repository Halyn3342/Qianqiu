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

public class QianqiuRead() : QianqiuCard(1,
    CardType.Skill, CardRarity.Basic,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<QianqiuReadStrengthUpPower>("StrengthUp", 3m),
        new PowerVar<QianqiuReadStrengthDownPower>("StrengthDown", 2m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<QianqiuReadStrengthUpPower>(),
        HoverTipFactory.FromPower<QianqiuOverdraftPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 本回合 +3 真实力量（蓄力，回合结束回滚）
        await PowerCmd.Apply<QianqiuReadStrengthUpPower>(choiceContext,
            Owner.Creature, DynamicVars["StrengthUp"].BaseValue, Owner.Creature, this);
        // 获得透支：下回合转化为乏力（乏力在本回合减少力量）
        await PowerCmd.Apply<QianqiuOverdraftPower>(choiceContext,
            Owner.Creature, DynamicVars["StrengthDown"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["StrengthDown"].UpgradeValueBy(-1m); // 透支层数 2 → 1
    }
}
