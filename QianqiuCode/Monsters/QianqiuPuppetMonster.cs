#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace MoeNegiMod.Qianqiu.Monsters;

/// <summary>
/// 化形·傀儡 的专属怪物，独立于原版 Osty。
/// 外观目前先复用原版 Osty 场景，保证立即可用；以后放自己的傀儡美术场景时，
/// 只需把 <see cref="VisualsPath"/> 改成你的场景路径（并确保场景含 idle_loop/cast/attack/attack_poke/hurt/die/dead_loop/revive 这些动画状态）。
/// </summary>
public sealed class QianqiuPuppetMonster : MonsterModel
{
    // 名称用默认 Title（= L10NMonsterLookup(base.Id.Entry + ".name")），
    // 本地化键在 monsters.json；已同时提供 QIANQIU_PUPPET_MONSTER.name 与 MOENEGIMOD-QIANQIU_PUPPET_MONSTER.name 两种。

    // TODO(美术): 换成自己的傀儡场景。例如 SceneHelper.GetScenePath("creature_visuals/qianqiu_puppet")
    protected override string VisualsPath => SceneHelper.GetScenePath("creature_visuals/osty");

    public override int MinInitialHp => 1;

    public override int MaxInitialHp => 1;

    // 血条/状态显示面板能否显示由该属性决定；这里固定为 true，确保傀儡的血条一直可见，
    // 而不依赖 base.Creature（否则在节点 _Ready 时可能求值异常导致整个状态显示面板被隐藏）。
    public override bool IsHealthBarVisible => true;

    public override string DeathSfx => "event:/sfx/characters/osty/osty_die";

    public override bool HasDeathSfx => true;

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        // 傀儡不采取任何行动，只站着替你挡伤。
        List<MonsterState> list = new List<MonsterState>();
        MoveState moveState = new MoveState("NOTHING", NothingMove, new HiddenIntent());
        moveState.FollowUpState = moveState;
        list.Add(moveState);
        return new MonsterMoveStateMachine(list, moveState);
    }

    private Task NothingMove(IReadOnlyList<Creature> targets)
    {
        return Task.CompletedTask;
    }

    // 复用原版 Osty 场景时，要用跟 Osty 一致的动画器（含 dead_loop / revive），
    // 否则死亡/复活不会走对应动画、傀儡会卡在受击/死亡状态直到被打才恢复。
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState animState = new AnimState("idle_loop", isLooping: true);
        AnimState animState2 = new AnimState("cast");
        AnimState animState3 = new AnimState("attack");
        AnimState animState4 = new AnimState("attack_poke");
        AnimState animState5 = new AnimState("hurt");
        AnimState animState6 = new AnimState("die");
        AnimState nextState = new AnimState("dead_loop", isLooping: true);
        AnimState animState7 = new AnimState("revive");
        animState.AddBranch("Hit", animState5);
        animState2.NextState = animState;
        animState2.AddBranch("Hit", animState5);
        animState3.NextState = animState;
        animState3.AddBranch("Hit", animState5);
        animState4.NextState = animState;
        animState4.AddBranch("Hit", animState5);
        animState5.NextState = animState;
        animState5.AddBranch("Hit", animState5);
        animState6.NextState = nextState;
        animState7.NextState = animState;
        CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
        creatureAnimator.AddAnyState("Attack", animState3);
        creatureAnimator.AddAnyState("Cast", animState2);
        creatureAnimator.AddAnyState("Dead", animState6);
        creatureAnimator.AddAnyState("attack_poke", animState4);
        creatureAnimator.AddAnyState("Revive", animState7);
        return creatureAnimator;
    }
}
