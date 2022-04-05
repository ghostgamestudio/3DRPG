using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    private NavMeshAgent agent;

    private Animator anim;

    private GameObject attackTarget;

    private float lastattackTime;


    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
    }

    private void Start()
    {
        MouseManager.instance.OnMouseClicked += MoveToTarget;
        MouseManager.instance.OnEnemyClicked += EventAttack;
    }


    private void Update()
    {
        SwitchAniamtion();

        lastattackTime -= Time.deltaTime;
    }

    private void SwitchAniamtion()
    {
        anim.SetFloat("Speed",agent.velocity.sqrMagnitude);
    }

    public void MoveToTarget(Vector3 target)
    {
        StopAllCoroutines();
        agent.isStopped = false;
        agent.destination = target;
    }

    private void EventAttack(GameObject target)
    {
        if (target != null)
        {
            attackTarget = target;
            StartCoroutine(MoveToAttackTarget());
        }
    }

    IEnumerator MoveToAttackTarget()
    {
        agent.isStopped = false;

        transform.LookAt(attackTarget.transform);

        //TODO:ÐÞ¸Ä¹¥»÷·¶Î§
        while(Vector3.Distance(attackTarget.transform.position,transform.position) > 1)
        {
            agent.destination = attackTarget.transform.position;
            yield return null;
        }

        agent.isStopped = true;
        //Attack
        if (lastattackTime < 0)
        {
            anim.SetTrigger("Attack");
            //ÖØÖÃÀäÈ´Ê±¼ä
            lastattackTime = 0.5f;
        }
    }
}
