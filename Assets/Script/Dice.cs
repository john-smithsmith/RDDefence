using TMPro;
using UnityEngine;


public enum DiceType { Basic, Ice, Fire, Lightning, Wind }
public class Dice : MonoBehaviour
{
    private bool isDragging = false;
    

    [Header("Data")]
    public DiceType type;      // 주사위 종류
    public int dotCount = 1;   // 주사위 눈 개수
    public Slot currentSlot;

    [Header("Prefab")]
    public SpriteRenderer spriteRenderer;
    public SpriteRenderer dotRenderer;

    [Header("Combat")]
    public GameObject projectilePrefab;
    public float range = 3f;            // 사거리
    public float attackSpeed = 1f;      // 공격 속도
    private float attackTimer = 0f;
    private Transform currentTarget;

    void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    void Update()
    {
        attackTimer += Time.deltaTime;
        if (currentTarget == null || Vector3.Distance(transform.position, currentTarget.position) > range)
        {
            FindTarget();
        }
        if (currentTarget != null && attackTimer >= AttackSpeed())
        {
            Attack();
            attackTimer = 0f;
        }
        HandleInput();
    }

    public void Init(DiceType newType)
    {
        type = newType;
        dotCount = 1;
        UpdateColor();
    }

    public void UpdateColor()//임시색상
    {
        switch (type)
        {
            case DiceType.Basic: spriteRenderer.color = Color.white; break;
            case DiceType.Ice: spriteRenderer.color = Color.cyan; break;
            case DiceType.Fire: spriteRenderer.color = Color.red; break;
            case DiceType.Lightning: spriteRenderer.color = Color.yellow; break;
            case DiceType.Wind: spriteRenderer.color = Color.green; break;
        }
    }
    public void SetDotCount(int count)
    {
        
    }

    void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                isDragging = true;
                GetComponent<SpriteRenderer>().sortingOrder = 100;
            }
        }

        if (isDragging && Input.GetMouseButton(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            transform.position = mousePos;
        }

        if (isDragging && Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            GetComponent<SpriteRenderer>().sortingOrder = 0;
            CheckDrop();
        }
    }

    void CheckDrop()
    {
        Collider2D hit = Physics2D.OverlapPoint(transform.position);

        if (hit != null)
        {
            Slot targetSlot = hit.GetComponent<Slot>();
            if (targetSlot != null)
            {
                if (targetSlot.IsEmpty())
                {
                    currentSlot.RemoveDice();
                    targetSlot.SetDice(this);
                    return;
                }

                if (targetSlot.currentDice != null && targetSlot.currentDice != this)
                {
                    Dice targetDice = targetSlot.currentDice;

                    if (type == targetDice.type && dotCount == targetDice.dotCount && dotCount < 7)
                    {
                        currentSlot.RemoveDice();
                        targetSlot.RemoveDice();
                        PoolManager.Instance.ReturnToPool(gameObject);
                        PoolManager.Instance.ReturnToPool(targetDice.gameObject);
                        BoardManager.Instance.SpawnMergedDiceAtRandom(dotCount + 1);
                        return;
                    }
                }
            }
        }

        ReturnToSlot();
    }

    void ReturnToSlot()
    {
        if (currentSlot != null)
        {
            transform.position = currentSlot.transform.position;
        }
    }


void FindTarget()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, range);
        float shortestDistance = Mathf.Infinity;
        GameObject nearestEnemy = null;

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                float dist = Vector3.Distance(transform.position, hit.transform.position);
                if (dist < shortestDistance)
                {
                    shortestDistance = dist;
                    nearestEnemy = hit.gameObject;
                }
            }
        }

        if (nearestEnemy != null)
        {
            currentTarget = nearestEnemy.transform;
        }
        else
        {
            currentTarget = null;
        }
    }

    void Attack()
    {
        GameObject bulletObj = PoolManager.Instance.Spawn(projectilePrefab, transform.position, Quaternion.identity);
        Bullet bulletScript = bulletObj.GetComponent<Bullet>();
        int finalDamage = 10 + (dotCount * 10);
        bulletScript.Init(currentTarget, finalDamage, type);
    }
   
    float AttackSpeed()
    {
        if (type == DiceType.Wind) return attackSpeed * 0.5f;
        return attackSpeed;
    }
}