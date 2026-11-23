#nullable enable
using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MoeNegiMod.Qianqiu.Powers;

namespace MoeNegiMod.Qianqiu.Cards;

/// <summary>
/// 运行时"黑爪附魔"登记。静态实现 <see cref="IShadowTouched"/> 的牌天然是黑爪；
/// 这里额外支持在战斗中把任意牌实例动态标记为黑爪（弱引用，不跨战斗泄漏）。
/// </summary>
public static class QianqiuShadow
{
    private static readonly ConditionalWeakTable<CardModel, object> _enchanted = new();

    /// <summary>当某张牌被动态附魔黑爪时触发（供"精通"等监听）。</summary>
    public static event Action<CardModel, PlayerChoiceContext>? Enchanted;

    public static void Enchant(CardModel card, PlayerChoiceContext context)
    {
        _enchanted.GetValue(card, _ => new object());
        Enchanted?.Invoke(card, context);
    }

    /// <summary>
    /// 该牌是否算作"黑爪"：静态实现 IShadowTouched，或运行时被附魔，
    /// 或（在"黑化形态"下的）攻击牌。
    /// </summary>
    public static bool IsShadowTouched(CardModel? card, Creature owner) =>
        card != null && (
            card is IShadowTouched ||
            _enchanted.TryGetValue(card, out _) ||
            (card.Type == CardType.Attack && owner.HasPower<QianqiuBlackFormPower>()));

    /// <summary>是否为"运行时"附魔的黑爪（区别于静态实现 IShadowTouched 的固定黑爪牌）。</summary>
    public static bool IsRuntimeShadowTouched(CardModel? card, Creature owner) =>
        card != null && card is not IShadowTouched && (
            _enchanted.TryGetValue(card, out _) ||
            (card.Type == CardType.Attack && owner.HasPower<QianqiuBlackFormPower>()));
}
