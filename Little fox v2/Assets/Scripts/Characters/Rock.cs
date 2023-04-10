using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Rock : MonoBehaviour
{

    public enum RockStates { HitPlayer, HitEnemy, HitNothing }; 

    private Rigidbody rb;

    public RockStates rockStates;

    [Header("Basic Settings")]

    public float force;

    public int damage;

    public GameObject target;

    private Vector3 direction;

    public GameObject breakEffect;

     void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.velocity = Vector3.one;        //rock�����ɵ�˲������ٶ���0�������ͻᵼ��fixedupdate��Ĺ��ܱ������ǵ������ͼ������ÿ��ʯͷ���������ж�Ϊhitnothing���������Ǹ���һ����ʼ���ٶȡ�

        rockStates = RockStates.HitPlayer;
        FlyToTarget();
    }

     void FixedUpdate()
    {
        if (rb.velocity.sqrMagnitude < 1f)     //�����ȥ���ʯͷ����ķ����ٶȣ��趨�У���ʯͷ����Ժ����ȥ�����Ǿͻ�����ͬ���ٶ�С��1�������滻��������
        {
            rockStates = RockStates.HitNothing;

        }
        //Debug.Log(rb.velocity.sqrMagnitude);
    }

    public void FlyToTarget()
    {
        direction = (target.transform.position - transform.position + Vector3.up).normalized;
        rb.AddForce(direction * force, ForceMode.Impulse);


    }

    private void OnCollisionEnter(Collision other)
    {
        switch (rockStates)
        {
            case RockStates.HitPlayer:
                if (other.gameObject.CompareTag("Player"))
                {
                    other.gameObject.GetComponent<NavMeshAgent>().isStopped = true;
                    other.gameObject.GetComponent<NavMeshAgent>().velocity = direction * force;

                    other.gameObject.GetComponent<Animator>().SetTrigger("Dizzy");
                    other.gameObject.GetComponent<CharacterStats>().TakeDamage(damage, other.gameObject.GetComponent<CharacterStats>());

                    rockStates = RockStates.HitNothing;
                }
                break;

            case RockStates.HitEnemy:
                if (other.gameObject.GetComponent<Golem>())
                {
                    var otherStats = other.gameObject.GetComponent<CharacterStats>();
                    otherStats.TakeDamage(damage, otherStats);
                    Instantiate(breakEffect, transform.position, Quaternion.identity);
                    Destroy(gameObject);
                }

                break;
        }
    }







}
