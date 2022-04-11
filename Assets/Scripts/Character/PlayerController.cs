using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    public float HitForce;

    private NavMeshAgent agent;

    private Animator anim;

    private GameObject attackTarget;

    private CharacterStats characterStats;

    private float lastattackTime;

    private bool isDead;

    private float stopDistance;


    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        characterStats = GetComponent<CharacterStats>();

        stopDistance = agent.stoppingDistance;
    }

    private void OnEnable()
    {
        MouseManager.Instance.OnMouseClicked += MoveToTarget;
        MouseManager.Instance.OnEnemyClicked += EventAttack;
        GameManager.Instance.RigisterPlayer(characterStats);
    }

    private void Start()
    {
        SaveManager.Instance.LoadPlayerData();
    }

    private void OnDisable()
    {
        if (!MouseManager.IsInitialized)
        {
            return;
        }
        MouseManager.Instance.OnEnemyClicked -= EventAttack;
        MouseManager.Instance.OnMouseClicked -= MoveToTarget;
    }
    private void Update()
    {
        isDead = characterStats.CurrentHealth == 0;
        SwitchAniamtion();

        if (isDead)
        {
            MouseManager.Instance.OnMouseClicked -= MoveToTarget;
            MouseManager.Instance.OnEnemyClicked -= EventAttack;
            GameManager.Instance.NotifyObservers();
        }

        lastattackTime -= Time.deltaTime;
    }

    private void SwitchAniamtion()
    {
        anim.SetFloat("Speed",agent.velocity.sqrMagnitude);
        anim.SetBool("Death", isDead);
    }

    public void MoveToTarget(Vector3 target)
    {
        StopAllCoroutines();
        if (isDead) return;
        agent.stoppingDistance = stopDistance;
        agent.isStopped = false;
        agent.destination = target;
    }

    private void EventAttack(GameObject target)
    {
        if (target != null)
        {
            attackTarget = target;
            characterStats.isCritical = UnityEngine.Random.value < characterStats.attackData.criticalChance;
            StartCoroutine(MoveToAttackTarget());
        }
    }

    IEnumerator MoveToAttackTarget()
    {
        agent.isStopped = false;
        agent.stoppingDistance = characterStats.AttackRange;

        transform.LookAt(attackTarget.transform);

        //TODO:ÐÞ¸Ä¹¥»÷·¶Î§
        while(Vector3.Distance(attackTarget.transform.position,transform.position) > characterStats.AttackRange)
        {
            agent.destination = attackTarget.transform.position;
            yield return null;
        }

        agent.isStopped = true;
        //Attack
        if (lastattackTime < 0)
        {
            anim.SetBool("Critical", characterStats.isCritical);
            anim.SetTrigger("Attack");
            //ÖØÖÃÀäÈ´Ê±¼ä
            lastattackTime = characterStats.CoolDown;
        }
    }

    //Animation Event
    void Hit()
    {
        if (attackTarget.CompareTag("Attackable"))
        {
            if (attackTarget.GetComponent<Rock>() && attackTarget.GetComponent<Rock>().rockStates == Rock.RockStates.HitNothing && attackTarget.gameObject != null)
            {
                attackTarget.GetComponent<Rock>().rockStates = Rock.RockStates.HitEnemy;
                attackTarget.GetComponent<Rigidbody>().velocity = Vector3.one;
                attackTarget.GetComponent<Rigidbody>().AddForce(transform.forward * HitForce, ForceMode.Impulse);
            }
        }
        else
        {
            var targetStats = attackTarget.GetComponent<CharacterStats>();

            targetStats.TakeDamage(characterStats, targetStats);
        }
    }
}
