using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyStates { GUARD,PATROL,CHASE,DEAD}
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(CharacterStats))]
public class EnemyController : MonoBehaviour,IEndGameObserver
{
    private EnemyStates enemyStates;
    private NavMeshAgent agent;
    private Animator anim;
    protected CharacterStats characterStats;
    private Collider coll;


    [Header("Basic Settings")]

    public float sightRadius;

    public bool isGuard;

    private float speed;

    protected GameObject attackTarget;

    public float lookAtTime;

    private float remainLookAtTime;

    private float lastAttackTime;

    private Quaternion guardRotation;

    [Header("Patrol State")]
    public float patrolRange;
    private Vector3 wayPoint;
    protected Vector3 guardPos;

    //bool配合动画
    protected bool isWalk;
    bool isChase;
    bool isFollow;
    bool isDead;
    bool playerDead;

    private void Awake()
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

    private void Start()
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
        //FIXME:场景切换后修改
        GameManager.Instance.AddObeserver(this);
    }
    //切换场景时使用
    //void OnEnable()
    //{
    //    GameManager.Instance.AddObeserver(this);
    //}

    void OnDisable()
    {
        if (!GameManager.IsInitialized)
        {
            return;
        }
        GameManager.Instance.RemoveObserver(this);
    }


    private void Update()
    {
        if (characterStats.CurrentHealth == 0)
        {
            isDead = true;
        }
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
        {
            enemyStates = EnemyStates.DEAD;
        }
        //如果发现Player 切换到CHASE
        else if (FoundPlayer())
        {
            enemyStates = EnemyStates.CHASE;
        }

        switch (enemyStates)
        {
            case EnemyStates.GUARD:
                isChase = false;

                if (transform.position != guardPos)
                {
                    isWalk = true;
                    agent.isStopped = false;
                    agent.destination = guardPos;

                    if (Vector3.SqrMagnitude(guardPos - transform.position) <= Math.Pow(agent.stoppingDistance,2.0))
                    {
                        isWalk = false;
                        transform.rotation = Quaternion.Lerp(transform.rotation, guardRotation, 0.1f);
                    }
                }
                break;
            case EnemyStates.PATROL:
                isChase = false;
                agent.speed = speed * 0.5f;
                //判断是否到随机巡逻点
                if (Vector3.SqrMagnitude(wayPoint - transform.position) < Math.Pow(agent.stoppingDistance,2.0))
                //if(Vector3.Distance(wayPoint,transform.position) < agent.stoppingDistance)
                {
                    StopAllCoroutines();
                    isWalk = false;
                    if (remainLookAtTime > 0)
                    {
                        remainLookAtTime -= Time.deltaTime;
                    }
                    else
                        GetNewWayPoint();
                }
                else
                {
                    isWalk=true;
                    agent.destination = wayPoint;
                    StartCoroutine(CheckPath());
                }
                break;
            case EnemyStates.CHASE:
                
                //TODO:配合动画
                isWalk = false;
                isChase = true;

                agent.speed = speed;
                if (!FoundPlayer())
                {
                    //目标超出返回上一个状态
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
                    agent.isStopped = false;
                    agent.destination = attackTarget.transform.position;
                }
                //在攻击范围内则攻击
                if (TargetInAttackRange() || TargetInSkillRange())
                {
                    isFollow=false;
                    agent.isStopped = true;

                    if (lastAttackTime < 0)
                    {
                        lastAttackTime = characterStats.CoolDown;

                        //暴击判断
                        characterStats.isCritical = UnityEngine.Random.value < characterStats.CriticalChance;
                        //执行攻击
                        Attack();
                    }
                }

                break;
            case EnemyStates.DEAD:
                coll.enabled = false;
                //agent.enabled = false;
                agent.radius = 0;
                Destroy(gameObject, 2f);
                break;

        }
    }

     void Attack()
    {
        transform.LookAt(attackTarget.transform);
        if(TargetInAttackRange())
        {
            //近身攻击动画
            anim.SetTrigger("Attack");
        }
        if(TargetInSkillRange())
        {
            //技能攻击动画
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
            return Vector3.SqrMagnitude(attackTarget.transform.position - transform.position) <= Math.Pow(characterStats.AttackRange, 2.0);
        else
            return false;
    }

    bool TargetInSkillRange()
    {
        if (attackTarget != null)
            return Vector3.SqrMagnitude(attackTarget.transform.position - transform.position) <= Math.Pow(characterStats.SkillRange, 2.0);
        else
            return false;
    }

    void GetNewWayPoint()
    {
        remainLookAtTime = lookAtTime;
        float randomX = UnityEngine.Random.Range(-patrolRange, patrolRange);
        float randomZ = UnityEngine.Random.Range(-patrolRange, patrolRange);

        Vector3 randomPoint = new Vector3(guardPos.x + randomX, transform.position.y, guardPos.z + randomZ);

        NavMeshHit hit;
        wayPoint = NavMesh.SamplePosition(randomPoint, out hit, patrolRange, 1)? hit.position : transform.position;
    }

    IEnumerator CheckPath()
    {
        yield return null;
        if (!agent.hasPath)
        {
            GetNewWayPoint();
        }
        else
            yield return null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, sightRadius);
    }

    //Animation Event
    void Hit()
    {
        if (attackTarget != null && transform.IsFacingTarget(attackTarget.transform))
        {
            var targetStats = attackTarget.GetComponent<CharacterStats>();
            targetStats.TakeDamage(characterStats, targetStats);
        }
    }

    public void EndNotify()
    {
        anim.SetBool("Win", true);
        playerDead = true;
        isChase = false;
        isWalk = false;
        attackTarget = null;
    }
}
