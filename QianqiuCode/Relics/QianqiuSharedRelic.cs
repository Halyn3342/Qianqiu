using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MoeNegiMod.Qianqiu.Extensions;

namespace MoeNegiMod.Qianqiu.Relics;

/// <summary>
/// 九千秋的通用遗物基类（商店可购买 / 精英掉落）：进共享遗物池（SharedRelicPool），
/// 所有角色都能获得。图标 fallback 到 relic.png。
/// </summary>
[Pool(typeof(SharedRelicPool))]
public abstract class QianqiuSharedRelic : CustomRelicModel
{
    public override string PackedIconPath
    {
        get
        {
            var path = $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".RelicImagePath();
            return ResourceLoader.Exists(path) ? path : "relic.png".RelicImagePath();
        }
    }

    protected override string PackedIconOutlinePath
    {
        get
        {
            var path = $"{Id.Entry.RemovePrefix().ToLowerInvariant()}_outline.png".RelicImagePath();
            return ResourceLoader.Exists(path) ? path : "relic_outline.png".RelicImagePath();
        }
    }

    protected override string BigIconPath
    {
        get
        {
            var path = $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigRelicImagePath();
            return ResourceLoader.Exists(path) ? path : "relic.png".BigRelicImagePath();
        }
    }
}
