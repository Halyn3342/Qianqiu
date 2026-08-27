#nullable enable
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.ValueProps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoeNegiMod.Qianqiu.Cards;

/// <summary>
/// 变形：选择手牌中的一张牌变化——攻击牌变为刺击，技能牌变为盾化，
/// 状态/诅咒/能力牌随机变为刺击或盾化。升级后获得保留。
/// </summary>
public class QianqiuTransformation() : QianqiuCard(0,
    CardType.Skill, CardRarity.Basic,
    TargetType.Self)
{
    // 法力费用 1（原版辉星机制：卡面左下角槽位显示，法力不够自动不可打）
    public override int CanonicalStarCost => 1;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.Static(StaticHoverTip.Transform)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 玩家选择手牌中的一张（仅可变换的）
        CardModel? card = (await CardSelectCmd.FromHand(
            prefs: new CardSelectorPrefs(SelectionScreenPrompt, 1),
            context: choiceContext, player: Owner,
            filter: c => c.IsTransformable, source: this)).FirstOrDefault();
        if (card == null)
        {
            return;
        }

        CardModel replacement;
        if (card.Type == CardType.Attack)
        {
            replacement = card.CardScope.CreateCard(ModelDb.Card<QianqiuStab>(), card.Owner);
        }
        else
        {
            // 技能 / 状态 / 诅咒 / 能力：变为盾化
            replacement = card.CardScope.CreateCard(ModelDb.Card<QianqiuShield>(), card.Owner);
        }

        await CardCmd.Transform(card, replacement);
        // 变为黑爪牌：通知"精通"等监听（变为黑爪）。
        QianqiuShadow.Enchant(replacement, choiceContext);
    }

    protected override void OnUpgrade()
    {
        // CanonicalKeywords 只在卡创建时求值一次，升级后不会重算，
        // 所以用 AddKeyword 在升级时动态添加"保留"。
        AddKeyword(CardKeyword.Retain);
    }
}
