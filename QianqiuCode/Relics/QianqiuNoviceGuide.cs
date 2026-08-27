using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace MoeNegiMod.Qianqiu.Relics;

/// <summary>
/// 新手攻略（商店/精英掉落通用遗物）：战斗开始时抽 1 张牌。
/// </summary>
public class QianqiuNoviceGuide : QianqiuSharedRelic
{
    public override RelicRarity Rarity => RelicRarity.Common;

    public override async Task BeforeCombatStartLate()
    {
        await CardPileCmd.Draw(new ThrowingPlayerChoiceContext(), 1, Owner);
    }
}
