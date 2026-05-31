using System.Collections.Generic;
using UnityEngine;

namespace PromisedEigong.Gameplay.AttackFactories;

using static PromisedEigongModGlobalSettings.EigongAttacks;
using static PromisedEigongModGlobalSettings.EigongSpeed;

public class _25SlashUpCrimsonPokeChainFactory : BaseAttackFactory
{
    public override string AttackToBeCopied => ATTACK12_SLASH_UP_CRIMSON;
    public override string AttackToBeCreated => ATTACK25_NEW_CHAIN_SLASH_UP_CRIMSON;

    public override void CopyAttack (BossGeneralState bossGeneralState)
    {
        var newAttack = SetupAttack(bossGeneralState);
        var attack1NextMove = GameObject.Find(ATTACK5_TELEPORT_TO_BACK).GetComponent<BossGeneralState>();
        var attack2NextMove = GameObject.Find(ATTACK2_TELEPORT_TO_TOP).GetComponent<BossGeneralState>();
        newAttack.AnimationSpeed = ATTACK25_SLASH_UP_CRIMSON_SPEED;
        newAttack.IsFromAChain = true;
        newAttack.StartOffset = new Vector3(-45, 0, 0);
        
        var phase2Weights = new List<AttackWeight>
        {
            CreateAttackWeight(attack1NextMove, 2, 7),
            CreateAttackWeight(attack2NextMove, 2, 3)
        };
        
        newAttack.Phase2Weights = phase2Weights;
        newAttack.SubscribeSource(ATTACK29_NEW_CHAIN_TELEPORT_TO_BACK_FIRST);
        newAttack.SubscribeSource(ATTACK37_NEW_CHAIN_ALTERNATE_JUMP_BACK);
    }
}
