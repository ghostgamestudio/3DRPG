using StarterAssets;
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

    private bool isAttack;


    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        characterStats = GetComponent<CharacterStats>();
    }

    private void OnEnable()
    {
        //MouseManager.Instance.OnMouseClicked += MoveToTarget;
        MouseManager.Instance.OnMouseClicked += EventAttack;
        GameManager.Instance.RigisterPlayer(characterStats);
        PlayerPrefs.DeleteAll();
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
        //MouseManager.Instance.OnMouseClicked -= MoveToTarget;
        MouseManager.Instance.OnMouseClicked -= EventAttack;
    }
    private void Update()
    {
        isDead = characterStats.CurrentHealth == 0;
        SwitchAnimation();

        if (isDead)
        {
            //MouseManager.Instance.OnMouseClicked -= MoveToTarget;
            MouseManager.Instance.OnMouseClicked -= EventAttack;
            GameManager.Instance.NotifyObservers();
        }

        lastattackTime -= Time.deltaTime;
    }

    private void SwitchAnimation()
    {
        anim.SetFloat("Speed",agent.velocity.sqrMagnitude);
        anim.SetBool("Death", isDead);
    }

    //public void MoveToTarget(Vector3 target)
    //{
    //    StopAllCoroutines();
    //    if (isDead) return;
    //    agent.stoppingDistance = stopDistance;
    //    agent.isStopped = false;
    //    agent.destination = target;
    //}

    private void EventAttack()
    {
        if (attackTarget != null)
        {
            characterStats.isCritical = UnityEngine.Random.value < characterStats.attackData.criticalChance;
        }
        StartCoroutine(Attack());
    }

    IEnumerator Attack()
    {
        //Attack
        if (lastattackTime < 0 && ThirdPersonController.Instance.Grounded)
        {
            anim.SetBool("Critical", characterStats.isCritical);
            anim.SetTrigger("Attack");
            //ÖØÖÃÀäÈ´Ê±¼ä
            lastattackTime = characterStats.CoolDown;
        }
        yield return null;
    }

    //Animation Event
    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("onTrigger");
        if (other != null && other.gameObject.CompareTag("Enemy") && isAttack) 
        {
            attackTarget = other.gameObject;
            characterStats.isCritical = UnityEngine.Random.value < characterStats.attackData.criticalChance;
            Debug.Log("attack");
        }
        if (attackTarget != null && isAttack)
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
        isAttack = false;

    }

    void Hit()
    {
        isAttack = true;
    }
}
