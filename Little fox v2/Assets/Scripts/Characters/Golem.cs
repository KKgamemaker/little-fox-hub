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
        if (attackTarget != null && transform.IsFacingTarget(attackTarget.transform))   //��������Ϊ��hit������һ���ؼ�֡���жϣ����п��ܵ�������һ֡��ʱ������Ѿ��ܿ��ˣ���ô�û�ȡ����attacktarget�ˣ��Ǿͻᱨ��
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
            attackTarget = FindObjectOfType<PlayerController>().gameObject; //��Ϊ���ж�Ҫ��ʯͷ--ʯͷ���ɵ��߼�֮����һ��ʱ�䣬�������ڼ�����Ѿ��÷�����֮�������ˡ����Ի����
            var rock = Instantiate(rockPrefab, handPos.position, Quaternion.identity);   //��ʯ�����ֲ�����һ��ʯ�^
            rock.GetComponent<Rock>().target = attackTarget;

        }                                       //attacktargetΪ�յ����⣬��Ϊ��ʱ����ֱ�Ӱ�ʯͷ��Ŀ��ָ��Ϊ��ҡ���Ϊ��ʱ���ǾͰ�ʯͷ�˻�ȡ����attacktarget��ֵ����ҡ�
        else
        {
            var rock = Instantiate(rockPrefab, handPos.position, Quaternion.identity);
            rock.GetComponent<Rock>().target = attackTarget;  //���ɵ�ʯ�^��ȥ�@ȡ���ϵĽű�rock���target
        }
            
           
        //}



    }


}
