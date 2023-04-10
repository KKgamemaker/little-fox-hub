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
        StopAllCoroutines();        //��������������������Ķ��������֮ǰ�޷�����ϡ����ڵ���𴦾Ϳ��Դ�Ϲ��������ˡ�
        if (isDead) return;
        agent.stoppingDistance = stopDistance;
        agent.isStopped = false;    //������������������Ժ���޷��ƶ�������
        agent.destination = target;
    }

    private void EventAttack(GameObject target)
    {
        if (isDead) return;

        if (target != null)
        {
            attackTarget = target;
            characterStats.isCritical = UnityEngine.Random.value < characterStats.attackData.criticalChance;  //��������֮ǰ���ж��Ƿ��Ǳ���
            StartCoroutine(MoveToAttackTarget());
        }
    }

    IEnumerator MoveToAttackTarget()
    {

        agent.isStopped = false;
        agent.stoppingDistance = characterStats.attackData.attackRange;

        transform.LookAt(attackTarget.transform);

        //TODO:�޸Ĺ�����Χ����
        while (Vector3.Distance(attackTarget.transform.position, transform.position) > characterStats.attackData.attackRange)  //���������˵ľ��������ҵĹ�������ʱ����ִ��move����Ϊ
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
            //������ȴʱ��
            lastAttackTime = characterStats.attackData.coolDown;

        }
    }

    //Animation Event

    void Hit()
    {
        if (attackTarget!=null && attackTarget.CompareTag("Attackable"))    //�����attacktargetָ���ǣ�ʯͷ   //hit����������animation event�������ӽ��Ķ����������ʱ����Ҫ�ر�ע�⣬��������ֿ����õı�������һ��Ҫ�жϣ�attacktarget��Ϊnull��
        {
            if (attackTarget.GetComponent<Rock>() && attackTarget.GetComponent<Rock>().rockStates == Rock.RockStates.HitNothing)   //�ܷ�ʯͷ
            {
                //������ʯͷ�����ʯͷ����ص�״̬
                attackTarget.GetComponent<Rock>().rockStates = Rock.RockStates.HitEnemy;
                attackTarget.GetComponent<Rigidbody>().velocity = Vector3.one;  //��Ϊ��rock�ű���fixedupdate���������ֻҪʯͷ���ٶ�С��1�ͻὫ��stateת��Ϊhitnothing������Ҫ���ܷ�˲���ʯͷһ�����ٶȴ���1������״̬�в�������
                attackTarget.GetComponent<Rigidbody>().AddForce(transform.forward * 20, ForceMode.Impulse);
                attackTarget.tag = "Untagged";   //��ʯͷ��tag��ס�������Ͳ�����õ�mousecontrol����߼��ˣ�û�нű���Ҫ��ȡ�������Ͳ��ᱨ���ˡ�

            }
        }
        else
        {
            var targetStats = attackTarget.GetComponent<CharacterStats>();

            targetStats.TakeDamage(characterStats, targetStats);
        }
       
    }
}
