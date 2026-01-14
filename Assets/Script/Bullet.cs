using UnityEngine;
using System.Collections.Generic;

public class Bullet : MonoBehaviour
{
    private Transform target;
    private int damage;
    private DiceType type;
    private float speed = 10f;
    private float specialValue;

    [Header("Lightning Settings")]
    //private int maxTargets = 3;    
    private float chainRange = 4.0f;

    [Header("Effects")]
    public GameObject lightningVisualPrefab;
    public GameObject explosionVisualPrefab;

    public void Init(Transform _target, int _damage, DiceType _type, float _specialValue)
    {
        target = _target;
        damage = _damage;
        type = _type;
        specialValue = _specialValue;
    }

    void Update()
    {
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            PoolManager.Instance.ReturnToPool(gameObject);
            return;
        }
        Vector3 dir = (target.position - transform.position).normalized;
        transform.Translate(dir * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null && enemy.gameObject.activeInHierarchy)
            {
                ApplyDamageEffect(enemy);
            }
            PoolManager.Instance.ReturnToPool(gameObject);
        }
    }
    void ApplyDamageEffect(Enemy primaryTarget)
    {
        primaryTarget.TakeDamage(damage);

        if (type == DiceType.Ice)
        {
            primaryTarget.ApplySlow(specialValue);
        }
        else if (type == DiceType.Fire)
        {
            Explode(primaryTarget.transform.position, specialValue);
        }
        else if (type == DiceType.Lightning)
        {
            HashSet<int> visited = new HashSet<int>();
            visited.Add(primaryTarget.gameObject.GetInstanceID());
            ChainDamage(primaryTarget.transform.position, (int)specialValue - 1, visited);
        }
    }

    void ChainDamage(Vector3 startPos, int remainingCount, HashSet<int> visited)
    {
        if (remainingCount <= 0) return;
        Collider2D[] hits = Physics2D.OverlapCircleAll(startPos, chainRange);
        Enemy nextTarget = null;
        float minDistance = float.MaxValue;
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                int id = hit.gameObject.GetInstanceID();
                if (visited.Contains(id)) continue;
                Enemy enemyScript = hit.GetComponent<Enemy>();
                if (enemyScript != null && enemyScript.gameObject.activeInHierarchy)
                {
                    float dist = Vector3.Distance(startPos, hit.transform.position);
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        nextTarget = enemyScript;
                    }
                }
            }
        }

        if (nextTarget != null)
        {
            nextTarget.TakeDamage(damage);
            visited.Add(nextTarget.gameObject.GetInstanceID());
            if (lightningVisualPrefab != null && PoolManager.Instance != null)
            {
                GameObject visualObj = PoolManager.Instance.Spawn(lightningVisualPrefab, Vector3.zero, Quaternion.identity);
                LightningEffect effectScript = visualObj.GetComponent<LightningEffect>();

                if (effectScript != null)
                {
                    effectScript.Show(startPos, nextTarget.transform.position);
                }
            }
            ChainDamage(nextTarget.transform.position, remainingCount - 1, visited);
        }
    }

    void Explode(Vector3 center, float radius)
    {
        if (explosionVisualPrefab != null && PoolManager.Instance != null)
        {
            GameObject visualObj = PoolManager.Instance.Spawn(explosionVisualPrefab, center, Quaternion.identity);
            ExplosionEffect effectScript = visualObj.GetComponent<ExplosionEffect>();
            if (effectScript != null)
            {
                effectScript.Show(center, radius);
            }
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius);

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                Enemy enemy = hit.GetComponent<Enemy>();
                if (enemy != null && enemy.gameObject.activeInHierarchy)
                {
                    enemy.TakeDamage(damage);
                }
            }
        }
    }
}
