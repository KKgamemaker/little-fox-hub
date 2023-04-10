using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyStates { GUARD, PATROL, CHASE, DEAD}

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(CharacterStats))]

public class EnemyController : MonoBehaviour,IEndGameObserver
{
    private EnemyStates enemyStates;

    private NavMeshAgent agent;

    private Animator anim;

    private Collider coll;

    protected CharacterStats characterStats;

    [Header("Basic Settings")]

    public float sightRadius;

    public bool isGuard;

    private float speed;

    protected GameObject attackTarget;   //��������protected֮�󣬾Ϳ���������������з����ˣ������Ƿ��ʲ�����

    public float lookAtTime;

    private float remainLookAtTime;

    private float lastAttackTime;

    private Quaternion guardRotation;

    [Header("Patrol State")]

    public float patrolRange;

    private Vector3 wayPoint; //��ʼ�������û�и�ֵ��

    private Vector3 guardPos;

    //bool��϶���

    bool isWalk;

    bool isChase;

    bool isFollow;

    bool isDead;

    bool playerDead;


    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        characterStats = GetComponent<CharacterStats>();
        coll = GetComponent<Collider>();

        speed = agent.speed;
        guardPos = transform.position;
        guardRotation = transform.rotation;
        remainLookAtTime = lookAtTime;
    }

    void Start()
    {
        if (isGuard)
        {
            enemyStates = EnemyStates.GUARD;
        }
        else
        {
            enemyStates = EnemyStates.PATROL;
            GetNewWayPoint();
        }

        GameManager.Instance.AddObserver(this);
    }

    //void OnEnable()
    //{
    //    GameManager.Instance.AddObserver(this);
    //}

    void OnDisable()
    {
        if (!GameManager.IsInitialized) return;
        GameManager.Instance.RemoveObserver(this);
    }


    void Update()
    {
        if (characterStats.CurrentHealth == 0)
            isDead = true;

        if (!playerDead)
        {
            SwitchStates();
            SwitchAnimation();
            lastAttackTime -= Time.deltaTime;
        }
       
    }

    void SwitchAnimation()
    {
        anim.SetBool("Walk", isWalk);
        anim.SetBool("Chase", isChase);
        anim.SetBool("Follow", isFollow);
        anim.SetBool("Critical", characterStats.isCritical);
        anim.SetBool("Death", isDead);
    }



    void SwitchStates()
    {
         if (isDead)
            enemyStates = EnemyStates.DEAD;

            //�������player���л���CHASE
         else if (FoundPlayer())
         {
                enemyStates = EnemyStates.CHASE;
                Debug.Log("�ҵ�player��");
                
         }

            switch (enemyStates)
            {
                
            case EnemyStates.GUARD:
                isChase = false;   //���������״̬����ô����Ҫֹͣ׷��

                if (transform.position != guardPos)
                {
                    isWalk = true;   //��������Ϊ��·
                    agent.isStopped = false; //transform����Ϊ�ɶ�
                    agent.destination = guardPos; //λ�ø�Ϊ�����ص�

                    if (Vector3.SqrMagnitude(guardPos - transform.position) <= agent.stoppingDistance)  //���Ҫ������λ����ͬ����Ϊ����ԭ�����������ģ���һֱ�޷������������ͻ�һֱwalk
                    {
                        isWalk = false;
                        transform.rotation = Quaternion.Lerp(transform.rotation, guardRotation, 0.01f);
                    }
                }
                    break;
                
            
            
            
            
            
            case EnemyStates.PATROL:

                isChase = false;
                agent.speed = speed * 0.5f;

                //�ж��Ƿ������Ѳ�ߵ�
                if (Vector3.Distance(wayPoint, transform.position) <= agent.stoppingDistance)
                {
                    isWalk = false;
                    if (remainLookAtTime > 0)
                        remainLookAtTime -= Time.deltaTime;
                    else
                        GetNewWayPoint();
                }
                else
                {
                    isWalk = true;
                    agent.destination = wayPoint;
                }

                break;
               
            
            
            
            
            
            
            case EnemyStates.CHASE:
                //todo:׷player


                //todo:��϶���
                isWalk = false;
                isChase = true;

                agent.speed = speed;

                if (!FoundPlayer())
                {
                    //    //todo:���ѻص���һ��״̬
                    isFollow = false;
                    if (remainLookAtTime > 0)
                    {
                        agent.destination = transform.position;
                        remainLookAtTime -= Time.deltaTime;
                    }

                    else if (isGuard)
                        enemyStates = EnemyStates.GUARD;
                    else
                        enemyStates = EnemyStates.PATROL;

                }
                else
                {
                    isFollow = true;
                    agent.isStopped = false;//����Ҵ�ʷ��ķ�Ĺ�����Χ����֮��ʷ��ķ�Ͳ�Ӧ����stay��
                    agent.destination = attackTarget.transform.position;
                }

                //todo:�ڹ�����Χ���򹥻�
                if (TargetInAttackRange() || TargetInSkillRange())
                {
                    isFollow = false;
                    agent.isStopped = true;

                    if (lastAttackTime < 0)
                    {
                        lastAttackTime = characterStats.attackData.coolDown;  

                        //�жϱ���
                        characterStats.isCritical = Random.value < characterStats.attackData.criticalChance;
                        //ִ�й���
                        Attack();

                    }
                }

                break;
               
            
            
            
            
            
            
            case EnemyStates.DEAD:
                coll.enabled = false;
                //agent.enabled = false;     //����ڵ�������ʱagentΪfalse����ôstopagent�Ľű��е�update��ĺ��������ͻ�ȡ����agent�ˣ��ͻᱨ��
                agent.radius = 0;
                Destroy(gameObject, 2f);
                    break;
            }
    }

    void Attack()
    {
        transform.LookAt(attackTarget.transform);
        if (TargetInAttackRange())
        {
            //��������
            anim.SetTrigger("Attack");
        }
        if (TargetInSkillRange())
        {
            //���ܹ�������
            anim.SetTrigger("Skill");
        }
    }

    bool FoundPlayer()
        {
            var colliders = Physics.OverlapSphere(transform.position, sightRadius);

            foreach (var target in colliders)
            {
                if (target.CompareTag("Player"))
                {
                attackTarget = target.gameObject;
                return true;
                }

            }
        attackTarget = null;
        return false;
        }

    bool TargetInAttackRange()
    {
        if (attackTarget != null)
            return Vector3.Distance(attackTarget.transform.position, transform.position) <= characterStats.attackData.attackRange;
        else
            return false;

    }

    bool TargetInSkillRange()
    {
        if (attackTarget != null)
            return Vector3.Distance(attackTarget.transform.position, transform.position) <= characterStats.attackData.skillRange;
        else
            return false;
    }


    void GetNewWayPoint()
    {
        remainLookAtTime = lookAtTime;

        float randomX = Random.Range(-patrolRange, patrolRange);
        float randomZ = Random.Range(-patrolRange, patrolRange);

        Vector3 randomPoint = new Vector3(guardPos.x + randomX, transform.position.y, guardPos.z + randomZ); //y����transform����Ϊ��������������ܻ���ɸ���

        //wayPoint = randomPoint;     �������ܻ���������λ�������ϰ����У�Ȼ������һֱ���Ž���ȥ�������
        NavMeshHit hit;
        wayPoint = NavMesh.SamplePosition(randomPoint, out hit, patrolRange, 1) ? hit.position : transform.position;


    }


    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, sightRadius);
    }


    //Animation Event

    void Hit()
    {
        if (attackTarget != null && transform.IsFacingTarget(attackTarget.transform))   //��������Ϊ��hit������һ���ؼ�֡���жϣ����п��ܵ�������һ֡��ʱ������Ѿ��ܿ��ˣ���ô�û�ȡ����attacktarget�ˣ��Ǿͻᱨ��
        {
            var targetStats = attackTarget.GetComponent<CharacterStats>();
            targetStats.TakeDamage(characterStats, targetStats);
        }                                                                       
    }

    public void EndNotify()
    {
        //��ʤ����
        //ֹͣ�����ƶ�
        //ֹͣAgent
        anim.SetBool("Win", true);
        playerDead = true;
        isChase = false;
        isWalk = false;
        attackTarget = null;

    }
}
