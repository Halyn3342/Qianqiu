using System;
using System.Linq;
using System.Reflection;
using BaseLib.Utils;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Nodes.Screens.Timeline;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Managers;
using MegaCrit.Sts2.Core.Timeline;

namespace Qianqiu;

/**
 * Ideas
 * 
 * Self Bind
 * 
 * Bind effect - square texture based on model size, lines random generated (amount equal to bind amount)
 * shader of transparency of line based on average of point spread of the model
 * colored
 * 
 * Bind... rename? Necrobinder kinda overlaps.
 * */

[ModInitializer(nameof(Initialize))]
public class MainFile
{
    public const string ModId = "Qianqiu"; //At the moment, this is used only for the Logger and harmony names.

    public static Logger Logger { get; } =
        new(ModId, LogType.Generic);

    public static void Initialize()
    {
        Harmony harmony = new(ModId);

        // 注意：不再手动 ModelDb.Inject 怪物——游戏会扫描并自动注册本模组的所有 AbstractModel
        // 子类（卡/遗物/能力/怪物），手动 Inject 会导致重复注册（DuplicateModelException）而无法启动。

        // 逐个应用补丁：单个补丁失败只记录日志，不阻断其余补丁（PatchAll 会因一个失败而中断后面的补丁）。
        foreach (Type type in typeof(MainFile).Assembly.GetTypes()
            .Where(t => t.GetCustomAttributes(typeof(HarmonyPatch), false).Length > 0))
        {
            try
            {
                harmony.CreateClassProcessor(type).Patch();
            }
            catch (Exception ex)
            {
                Log.Info($"Failed to apply patch on {type.FullName}: {ex.Message}", 2);
            }
        }
    }
}
