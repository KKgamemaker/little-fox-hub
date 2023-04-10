using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    private NavMeshAgent agent;

    private Animator anim;

    private CharacterStats characterStats;

    private GameObject attackTarget;

    private float lastAttackTime;

    private bool isDead;

    private float stopDistance;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        characterStats = GetComponent<CharacterStats>();
        stopDistance = agent.stoppingDistance;
    }

    void Start()
    {
        MouseManager.Instance.OnMouseClicked += MoveToTarget;
        MouseManager.Instance.OnEnemyClicked += EventAttack;
        
        GameManager.Instance.RigisterPlayer(characterStats);
    }


    void Update()
    {
        isDead = characterStats.CurrentHealth == 0;

        if (isDead)
            GameManager.Instance.NotifyObservers();
        SwitchAnimation();
        lastAttackTime -= Time.deltaTime;
    }

    private void SwitchAnimation()
    {
        anim.SetFloat("Speed", agent.velocity.sqrMagnitude);
        anim.SetBool("Death", isDead);
    }

    public void MoveToTarget(Vector3 target)
    {
        StopAllCoroutines();        //少了这个，人物走向怪物的动作在完成之前无法被打断。现在点击别处就可以打断攻击动作了。
        if (isDead) return;
        agent.stoppingDistance = stopDistance;
        agent.isStopped = false;    //少了这个，人物打完怪以后就无法移动到别处了
        agent.destination = target;
    }

    private void EventAttack(GameObject target)
    {
        if (isDead) return;

        if (target != null)
        {
            attackTarget = target;
            characterStats.isCritical = UnityEngine.Random.value < characterStats.attackData.criticalChance;  //攻击触发之前先判断是否是暴击
            StartCoroutine(MoveToAttackTarget());
        }
    }

    IEnumerator MoveToAttackTarget()
    {

        agent.isStopped = false;
        agent.stoppingDistance = characterStats.attackData.attackRange;

        transform.LookAt(attackTarget.transform);

        //TODO:修改攻击范围参数
        while (Vector3.Distance(attackTarget.transform.position, transform.position) > characterStats.attackData.attackRange)  //当玩家与敌人的距离大于玩家的攻击距离时，就执行move的行为
        {
            agent.destination = attackTarget.transform.position;
            yield return null;

        }

        agent.isStopped = true;
        //Attack

        if (lastAttackTime < 0)
        {
            anim.SetBool("Critical", characterStats.isCritical);
            anim.SetTrigger("Attack");
            //重置冷却时间
            lastAttackTime = characterStats.attackData.coolDown;

        }
    }

    //Animation Event

    void Hit()
    {
        if (attackTarget!=null && attackTarget.CompareTag("Attackable"))    //这里的attacktarget指的是，石头   //hit方法触发是animation event里，在人物挥剑的动画里。这种有时间差的要特别注意，经常会出现空引用的报错。所以一定要判断，attacktarget不为null。
        {
            if (attackTarget.GetComponent<Rock>() && attackTarget.GetComponent<Rock>().rockStates == Rock.RockStates.HitNothing)   //盾反石头
            {
                //首先是石头，其次石头是落地的状态
                attackTarget.GetComponent<Rock>().rockStates = Rock.RockStates.HitEnemy;
                attackTarget.GetComponent<Rigidbody>().velocity = Vector3.one;  //因为在rock脚本的fixedupdate（）里，会检测只要石头的速度小于1就会将其state转化为hitnothing。所以要给盾反瞬间的石头一个初速度大于1。否则状态切不回来。
                attackTarget.GetComponent<Rigidbody>().AddForce(transform.forward * 20, ForceMode.Impulse);
                attackTarget.tag = "Untagged";   //将石头的tag锁住，这样就不会调用到mousecontrol里的逻辑了，没有脚本需要获取到它，就不会报错了。

            }
        }
        else
        {
            var targetStats = attackTarget.GetComponent<CharacterStats>();

            targetStats.TakeDamage(characterStats, targetStats);
        }
       
    }
}
