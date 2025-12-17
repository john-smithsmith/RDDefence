using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    public float baseSpeed = 2f;
    public float speed;
    public float maxHp = 100f;
    private float currentHp;

    [Header("Pathfinding")]
    private Transform[] waypoints; 
    private int targetIndex = 0; 

    public void Init(Transform[] points, float hpBuff)
    {
        waypoints = points;
        targetIndex = 0; 
        speed = baseSpeed;
        maxHp *= hpBuff;
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
        Destroy(gameObject);
    }

    void ReachEndPoint()
    {
        Debug.Log("적이 기지에 도착했습니다!");
        Destroy(gameObject); // 일단은 삭제
    }
}