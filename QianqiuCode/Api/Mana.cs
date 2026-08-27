using MegaCrit.Sts2.Core.Entities.Players;
using MoeNegiMod.Qianqiu.Powers;
using MoeNegiMod.Qianqiu.Relics;
using System.Linq;

namespace MoeNegiMod.Qianqiu.Api;

/// <summary>
/// 九千秋的法力（辉星机制）相关常量与判断。
/// </summary>
public static class Mana
{
    /// <summary>基础法力上限 3（3/3）。</summary>
    public const int BaseMax = 3;

    /// <summary>每张"源流"（QianqiuFountainPower 层数）额外提供的法力上限。</summary>
    public const int FountainMaxBonus = 2;

    /// <summary>"血灵双生"额外提供的法力上限。</summary>
    public const int TwinMaxBonus = 3;

    /// <summary>"蛙蛙的头"额外提供的法力上限。</summary>
    public const int FrogHeadMaxBonus = 2;

    /// <summary>当前法力上限 = 基础 3 + 源流层数 × 2 + 黑化形态 +3（血灵双生 +3，蛙蛙的头 +2）。</summary>
    public static int GetMaxFor(Player player)
    {
        int max = BaseMax + FountainMaxBonus * (int)player.Creature.GetPowerAmount<QianqiuFountainPower>();
        max += (int)player.Creature.GetPowerAmount<QianqiuMaxManaPower>();
        if (player.Relics.Any(r => r is QianqiuTwin))
        {
            max += TwinMaxBonus;
        }
        if (player.Relics.Any(r => r is QianqiuFrogHead))
        {
            max += FrogHeadMaxBonus;
        }
        return max;
    }

    public static bool IsQianqiu(Player player) => player.Character is MoeNegiMod.Qianqiu.Character.Qianqiu;
}
