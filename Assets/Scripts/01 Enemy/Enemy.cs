using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : LivingEntity
{
    public enum State { Idle, Chasing, Attacking };
    private NavMeshAgent navMeshAgent;
    private Transform target;

    #region Attack
    [SerializeField] private float attackDistanceThreshold = 2.5f;
    private float timeBtwAttack = 1.0f;

    private float nextAttackTime;
    public State currentState;
    public float damage = 1.0f;
    #endregion

    [SerializeField] private float updateRate;

    [SerializeField] private bool hasTarget;

    public ParticleSystem deathEffect;

    Material skinMaterial;
    Color originalColor;

    public static event Action onDeathStatic;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();

        if (GameObject.FindGameObjectWithTag("Player") != null)
        {
            hasTarget = true;
            target = GameObject.FindGameObjectWithTag("Player").transform;
        }
    }

    protected override void Start()
    {
        base.Start();

        if (hasTarget)
        {
            currentState = State.Chasing;
            target.GetComponent<LivingEntity>().onDeath += TargetDeath;

            StartCoroutine(UpdatePath());
        }
    }

    #region Attack
    private void Update()
    {
        if (hasTarget)
        {
            if (Time.time > nextAttackTime)
            {
                float sqrtDstToTarget = (target.position - transform.position).sqrMagnitude;
                if (sqrtDstToTarget < Mathf.Pow(attackDistanceThreshold, 2))
                {
                    nextAttackTime = Time.time + timeBtwAttack;
                    StartCoroutine(Attack());
                }
            }
        }
    }

    IEnumerator Attack()
    {
        currentState = State.Attacking;
        navMeshAgent.enabled = false;

        Vector3 originalPos = transform.position;
        Vector3 attackPos = target.position;

        float attackSpeed = 3;
        float percent = 0;

        skinMaterial.color = Color.red;
        bool hasAttacked = false;

        while (percent <= 1)
        {
            if (percent >= 0.5f && !hasAttacked)
            {
                hasAttacked = true;
                target.GetComponent<IDamageable>().TakenDamage(damage);
                FindObjectOfType<GameUI>().UpdateHealth();
                Debug.Log("Attack");
            }

            percent += Time.deltaTime * attackSpeed;

            float interpolation = 4 * (-percent * percent + percent);
            transform.position = Vector3.Lerp(originalPos, attackPos, interpolation);

            yield return null;
        }

        skinMaterial.color = originalColor;
        currentState = State.Chasing;
        navMeshAgent.enabled = true;
    }
    #endregion

    IEnumerator UpdatePath()
    {
        while (hasTarget)
        {
            if (currentState == State.Chasing)
            {
                if (!isDead)
                {
                    Vector3 preTargetPos = new Vector3(target.position.x, 0, target.position.z);
                    navMeshAgent.SetDestination(preTargetPos);
                }
            }
            yield return new WaitForSeconds(updateRate);
        }
    }

    private void TargetDeath()
    {
        hasTarget = false;
        currentState = State.Idle;
    }

    public override void TakeHit(float _damageAmount, Vector3 hitPoint, Vector3 hitDirection)
    {
        if (_damageAmount >= health)
        {
            if (onDeathStatic != null)
            {
                onDeathStatic();
            }
            GameObject spawnEffect = Instantiate(deathEffect.gameObject, hitPoint, Quaternion.FromToRotation(Vector3.forward, hitDirection));
            Destroy(spawnEffect, deathEffect.main.startLifetime.constant);// Use main.startLifetime instead of startLifetime
        }
        base.TakeHit(_damageAmount, hitPoint, hitDirection);
    }

    public void SetDifficulty(float _speed, float _damage, float _health, Color _color)
    {
        navMeshAgent.speed = _speed;

        damage = _damage;
        maxHealth = _health;

        var mainModule = deathEffect.main;
        mainModule.startColor = new Color(_color.r, _color.g, _color.b, 1);

        skinMaterial = GetComponent<MeshRenderer>().material;
        skinMaterial.color = _color;
        originalColor = skinMaterial.color;
    }
}
