using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Golem :EnemyController
{

    [Header("Skill")]

    public float kickForce = 10;

    public GameObject rockPrefab;

    public Transform handPos;

    //Animation Event
    public void KickOff()
    {
        if (attackTarget != null && transform.IsFacingTarget(attackTarget.transform))   //这里是因为，hit本身是一个关键帧的判断，很有可能当到了这一帧的时候，玩家已经跑开了，那么久获取不到attacktarget了，那就会报错
        {
            var targetStats = attackTarget.GetComponent<CharacterStats>();

            Vector3 direction = attackTarget.transform.position - transform.position;
            direction.Normalize();

            targetStats.GetComponent<NavMeshAgent>().isStopped = true;
            targetStats.GetComponent<NavMeshAgent>().velocity = direction * kickForce;
            targetStats.GetComponent<NavMeshAgent>().ResetPath();
            //base on personal taste
            targetStats.GetComponent<Animator>().SetTrigger("Dizzy");
            targetStats.TakeDamage(characterStats, targetStats);
        }
    }

    //Animation Event

    public void ThrowRock()
    {
        //if (attackTarget != null)
        //{

        

        if (attackTarget == null)
        {
            attackTarget = FindObjectOfType<PlayerController>().gameObject; //因为从判定要扔石头--石头生成的逻辑之间有一段时间，可能这期间玩家已经用飞雷神之术溜走了。所以会存在
            var rock = Instantiate(rockPrefab, handPos.position, Quaternion.identity);   //在石巨人手部生成一個石頭
            rock.GetComponent<Rock>().target = attackTarget;

        }                                       //attacktarget为空的问题，当为空时，我直接把石头的目标指定为玩家。不为空时，那就把石头人获取到的attacktarget赋值给玩家。
        else
        {
            var rock = Instantiate(rockPrefab, handPos.position, Quaternion.identity);
            rock.GetComponent<Rock>().target = attackTarget;  //生成的石頭再去獲取身上的脚本rock里的target
        }
            
           
        //}



    }


}
