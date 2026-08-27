using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.PotionPools;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MoeNegiMod.Qianqiu.Cards;
using MoeNegiMod.Qianqiu.Extensions;
using MoeNegiMod.Qianqiu.Relics;
using System;

namespace MoeNegiMod.Qianqiu.Character;

public class Qianqiu : PlaceholderCharacterModel
{
    public const string CharacterId = "Qianqiu";

    //public override string CustomCharacterSelectBg => "res://Selphina/Scenes/Char_Select/char_select_bg_selphina.tscn";

    public override string PlaceholderID => "necrobinder";

    public static readonly Color Color = new Color("c4278a");

    public override Color NameColor => Color;
    public override CharacterGender Gender => CharacterGender.Feminine;
    public override int StartingHp => 70;

    // 显示法力（辉星机制）计数器
    public override bool ShouldAlwaysShowStarCounter => true;

    public override IEnumerable<CardModel> StartingDeck => [
        ModelDb.Card<QianqiuAttack>(),
        ModelDb.Card<QianqiuAttack>(),
        ModelDb.Card<QianqiuAttack>(),
        ModelDb.Card<QianqiuAttack>(),
        ModelDb.Card<QianqiuBlock>(),
        ModelDb.Card<QianqiuBlock>(),
        ModelDb.Card<QianqiuBlock>(),
        ModelDb.Card<QianqiuBlock>(),
        ModelDb.Card<QianqiuRead>(),
        ModelDb.Card<QianqiuTransformation>()
    ];

    public override IReadOnlyList<RelicModel> StartingRelics => [ModelDb.Relic<QianqiuToy>()];

    public override CardPoolModel CardPool => ModelDb.CardPool<QianqiuCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<QianqiuRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<SharedPotionPool>();

    /*  PlaceholderCharacterModel will utilize placeholder basegame assets for most of your character assets until you
		override all the other methods that define those assets.
		These are just some of the simplest assets, given some placeholders to differentiate your character with.
		You don't have to, but you're suggested to rename these images. */
    // CustomVisualPath is intentionally not overridden: the necrobinder placeholder visual is used
    // until a Qianqiu character scene exists in this mod's pck.
    public override string CustomIconTexturePath => "character_icon_char_name.png".CharacterUiPath();
    public override string CustomCharacterSelectIconPath => "char_select_char_name.png".CharacterUiPath();
    public override string CustomCharacterSelectLockedIconPath => "char_select_char_name_locked.png".CharacterUiPath();
    public override string CustomMapMarkerPath => "map_marker_char_name.png".CharacterUiPath();
}
