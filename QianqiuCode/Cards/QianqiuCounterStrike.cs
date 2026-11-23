#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.ValueProps;
using MoeNegiMod.Qianqiu.Powers;

namespace MoeNegiMod.Qianqiu.Cards;

/// <summary>反击（罕见攻击）：1 费，造成 5 点伤害；若上回合受到过伤害则额外 +5（升级后 6 伤）。</summary>
public class QianqiuCounterStrike() : QianqiuCard(1,
    CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(5, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        // 确保标记 Power 存在
        if (Owner.Creature.GetPower<QianqiuLastTurnDamagedPower>() == null)
        {
            await PowerCmd.Apply<QianqiuLastTurnDamagedPower>(choiceContext, Owner.Creature, 0, Owner.Creature, this);
        }
        bool damagedLastTurn = Owner.Creature.GetPower<QianqiuLastTurnDamagedPower>()?.WasDamagedLastTurn == true;
        decimal dmg = DynamicVars.Damage.BaseValue + (damagedLastTurn ? 5m : 0m);
        await DamageCmd.Attack(dmg).FromCard(this, cardPlay).Targeting(cardPlay.Target).Execute(choiceContext);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(1m);
}
