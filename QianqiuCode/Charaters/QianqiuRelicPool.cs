using BaseLib.Abstracts;
using Godot;
using MoeNegiMod.Qianqiu.Character;
using System;

namespace MoeNegiMod.Qianqiu.Character;

public partial class QianqiuRelicPool : CustomRelicPoolModel
{
    public override string EnergyColorName => Qianqiu.CharacterId;

    public override Color LabOutlineColor => Qianqiu.Color;
}
