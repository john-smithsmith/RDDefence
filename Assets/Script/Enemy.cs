using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    public float baseSpeed = 2f;
    public float speed;
    public float maxHp = 100f;
    public float currentHp;

    [Header("Pathfinding")]
    private Transform[] waypoints; 
    public int targetIndex = 0; 

    public void Init(Transform[] points, float hpMultiplier, int enemyID)
    {
        EnemyStat stat = DataManager.Instance.enemyDict[enemyID];
        baseSpeed = stat.speed;
        maxHp = stat.maxHp * hpMultiplier; // 웨이브 배율 적용
        currentHp = maxHp;
        if (waypoints.Length > 0)
        {
            transform.position = waypoints[0].position;
            targetIndex = 1;
        }
    }
    public void ApplySlow(float factor)
    {
        if (speed < baseSpeed * 0.9f) return;
        speed *= factor;
        GetComponent<SpriteRenderer>().color = Color.blue;
    }

    void Update()
    {
        Move();
    }

    void Move()
    {
        if (waypoints == null || targetIndex >= waypoints.Length) return;
        Transform target = waypoints[targetIndex];
        Vector3 dir = (target.position - transform.position).normalized;
        transform.Translate(dir * speed * Time.deltaTime);
        if (Vector3.Distance(transform.position, target.position) < 0.1f)
        {
            targetIndex++;
            if (targetIndex >= waypoints.Length)
            {
                ReachEndPoint();
            }
        }
    }

    public void TakeDamage(float damage)
    {
        currentHp -= damage;
        if (currentHp <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (BoardManager.Instance != null)
        {
            BoardManager.Instance.AddSP(10);
        }
        if (PoolManager.Instance != null)
        {
            PoolManager.Instance.ReturnToPool(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void ReachEndPoint()
    {
        Debug.Log("적이 기지에 도착했습니다!");
        if (PoolManager.Instance != null)
        {
            PoolManager.Instance.ReturnToPool(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public float GetProgress()
    {
        if (waypoints == null || targetIndex >= waypoints.Length) return 0;
        float distToNext = Vector3.Distance(transform.position, waypoints[targetIndex].position);
        return (targetIndex * 1000f) - distToNext;
    }
}