#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace MoeNegiMod.Qianqiu.Cards;

/// <summary>
/// 化形术：1 能量 + 1 法力。选择一张手牌，再从长枪/弩箭/斩斧/火枪中选择一张，将该牌变化为所选黑爪牌。
/// </summary>
public class QianqiuMetamorphosis() : QianqiuCard(1,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    // 法力费用 1
    public override int CanonicalStarCost => 1;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 第一步：选择手牌中的一张（可变换的）
        CardModel? card = (await CardSelectCmd.FromHand(
            prefs: new CardSelectorPrefs(SelectionScreenPrompt, 1),
            context: choiceContext, player: Owner,
            filter: c => c.IsTransformable, source: this)).FirstOrDefault();
        if (card == null)
        {
            return;
        }

        // 第二步：从黑爪变化牌中选择一张（网格选择界面）
        CardModel[] options =
        [
            ModelDb.Card<QianqiuLance>(),
            ModelDb.Card<QianqiuCrossbow>(),
            ModelDb.Card<QianqiuAxe>(),
            ModelDb.Card<QianqiuMusket>(),
            ModelDb.Card<QianqiuGreatsword>(),
            ModelDb.Card<QianqiuPuppet>()
        ];
        CardModel? chosen = (await CardSelectCmd.FromSimpleGrid(
            choiceContext, options, Owner, new CardSelectorPrefs(CardSelectorPrefs.TransformSelectionPrompt, 1))).FirstOrDefault();
        if (chosen == null)
        {
            return;
        }

        CardModel replacement = card.CardScope.CreateCard(chosen, card.Owner);
        await CardCmd.Transform(card, replacement);
        // 变为黑爪牌：通知"精通"等监听（变为黑爪）。
        QianqiuShadow.Enchant(replacement, choiceContext);
    }
}
