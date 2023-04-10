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
        rb.velocity = Vector3.one;        //rock在生成的瞬间可能速度是0，这样就会导致fixedupdate里的功能背离我们的设计意图，导致每个石头出来就是判定为hitnothing。所以我们给它一个初始的速度。

        rockStates = RockStates.HitPlayer;
        FlyToTarget();
    }

     void FixedUpdate()
    {
        if (rb.velocity.sqrMagnitude < 1f)     //这个是去监控石头本身的飞行速度，设定中，当石头落地以后就能去击打，那就基本等同于速度小于1，等量替换了属于是
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
