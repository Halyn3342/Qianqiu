using BaseLib.Abstracts;
using Godot;
using MoeNegiMod.Qianqiu.Cards;
using MoeNegiMod.Qianqiu.Extensions;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Unlocks;

namespace MoeNegiMod.Qianqiu.Character;

public class QianqiuCardPool : CustomCardPoolModel
{
    public override string Title => Qianqiu.CharacterId; //This is not a display name.

    public override string BigEnergyIconPath => "Charui/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "Charui/text_energy.png".ImagePath();

    /// <summary>
    /// 游戏结束！虽在本池中，但排除出"可获得"列表：不进奖励/商店/尘封魔典，
    /// 只能通过"启示录"直接创建获得。
    /// </summary>
    protected override IEnumerable<CardModel> FilterThroughEpochs(UnlockState unlockState, IEnumerable<CardModel> cards)
        => cards.Where(c => c is not QianqiuGameOver);

    /* These HSV values will determine the color of your card back.
	They are applied as a shader onto an already colored image,
	so it may take some experimentation to find a color you like.
	Generally they should be values between 0 and 1. */
    public override float H => 0.95f;
    public override float S => 0.98f;
    public override float V => 0.7f;

    //Alternatively, leave these values at 1 and provide a custom frame image.
    /*public override Texture2D CustomFrame(CustomCardModel card)
	{
		//This will attempt to load Oddmelt/images/cards/frame.png
		return PreloadManager.Cache.GetTexture2D("cards/frame.png".ImagePath());
	}*/

    //Color of small card icons
    public override Color DeckEntryCardColor => new("840240");
    public override Color EnergyOutlineColor => new("651565");

    public override bool IsColorless => false;
}
