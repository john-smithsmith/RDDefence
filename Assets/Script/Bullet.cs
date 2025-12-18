using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Transform target;
    private int damage;
    private DiceType type;
    private float speed = 10f;

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
    void ApplyDamageEffect(Enemy Target)
    {
        Target.TakeDamage(damage);
        if (type == DiceType.Ice)
        {
            Target.ApplySlow(0.7f); // 30% °¨¼Ó
        }
    }
}
