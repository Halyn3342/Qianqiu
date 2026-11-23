using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MoeNegiMod.Qianqiu.Character;

namespace MoeNegiMod.Qianqiu.Relics;

/// <summary>
/// 捏造（商店/精英掉落通用遗物）：战斗开始时获得 2 张随机的黑爪牌（刺击/盾化/长枪/弩箭/斩斧/火枪）。
/// </summary>
public class QianqiuFabrication : QianqiuSharedRelic
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override async Task BeforeCombatStartLate()
    {
        var options = ModelDb.CardPool<QianqiuTokenPool>()
            .GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint)
            .ToList();

        for (int i = 0; i < 2 && options.Count > 0; i++)
        {
            CardModel model = Owner.PlayerRng.Transformations.NextItem(options)!;
            options.Remove(model);
            CardModel card = Owner.RunState.CreateCard(model, Owner);
            await CardPileCmd.Add(card, PileType.Hand);
        }
    }
}
