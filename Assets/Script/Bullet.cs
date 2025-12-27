using UnityEngine;
using System.Collections.Generic;

public class Bullet : MonoBehaviour
{
    private Transform target;
    private int damage;
    private DiceType type;
    private float speed = 10f;

    [Header("Lightning Settings")]
    private int maxTargets = 3;    
    private float chainRange = 4.0f;

    [Header("Effects")]
    public GameObject lightningVisualPrefab;

    public void Init(Transform _target, int _damage, DiceType _type)
    {
        target = _target;
        damage = _damage;
        type = _type;
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
            primaryTarget.ApplySlow(0.7f);
        }
        else if (type == DiceType.Lightning)
        {
            HashSet<int> visitedEnemies = new HashSet<int>();
            visitedEnemies.Add(primaryTarget.gameObject.GetInstanceID());
            ChainDamage(primaryTarget.transform.position, maxTargets - 1, visitedEnemies);
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
}
