#nullable enable
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.ValueProps;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoeNegiMod.Qianqiu.Cards;

/// <summary>
/// 火枪（黑爪 Token）：1 能量 + 2 法力，造成 8 点伤害（升级后 12），无视敌人 50% 的防御。
/// 只能通过变牌/产牌获得。
/// </summary>
public class QianqiuMusket() : QianqiuCard(1,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AnyEnemy), IShadowTouched
{
    // 法力费用 2
    public override int CanonicalStarCost => 2;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(8, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        // 无视敌人 50% 防御（单段）：一半伤害打格挡（超出格挡的部分转移到打血），另一半打血量。
        decimal damage = DynamicVars.Damage.BaseValue;
        int block = cardPlay.Target.Block;
        int halfBlock = (int)Math.Floor(damage * 0.5m);
        int absorbed = Math.Min(block, halfBlock);   // 格挡抵消（最多一半伤害）
        if (absorbed > 0)
        {
            await CreatureCmd.LoseBlock(choiceContext, cardPlay.Target, absorbed, Owner.Creature);
        }
        int hpDamage = (int)damage - absorbed;        // 打血 = 总伤 - 格挡抵消
        await CreatureCmd.Damage(choiceContext, cardPlay.Target, hpDamage,
            ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, this, cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4m); // 8 → 12
    }
}
