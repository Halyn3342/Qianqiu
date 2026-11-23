#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.ValueProps;
using MoeNegiMod.Qianqiu.Powers;

namespace MoeNegiMod.Qianqiu.Cards;

/// <summary>回响斩（普通攻击）：0 能量 + 1 法力，造成 4 点伤害，给予 2 层看穿（升级 5 伤 + 3 层看穿）。</summary>
public class QianqiuEchoSlash() : QianqiuCard(0,
    CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    public override int CanonicalStarCost => 1;
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(4, ValueProp.Move), new PowerVar<QianqiuPiercePower>("Pierce", 2m)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<QianqiuPiercePower>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target).Execute(choiceContext);
        await PowerCmd.Apply<QianqiuPiercePower>(choiceContext, cardPlay.Target, DynamicVars["Pierce"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1m);
        DynamicVars["Pierce"].UpgradeValueBy(1m);
    }
}
