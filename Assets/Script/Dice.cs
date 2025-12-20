using TMPro;
using UnityEngine;

public enum DiceState
{
    Idle,       // 대기 
    Attack,     // 공격 (연사 중)
    Cooldown    // 휴식 
}

public enum DiceType { Basic, Ice, Fire, Lightning, Wind }

public class Dice : MonoBehaviour
{
    private bool isDragging = false;

    [Header("State")]
    public DiceState currentState = DiceState.Idle;

    [Header("Data")]
    public DiceType type;      // 주사위 종류
    public int dotCount = 1;   // 주사위 눈 개수 (레벨)
    public Slot currentSlot;

    [Header("Prefab")]
    public SpriteRenderer spriteRenderer;

    [Header("Combat")]
    public GameObject projectilePrefab;
    public float range = 3f;

    [Tooltip("공격 한 사이클(연사)이 끝나고 쉬는 시간")]
    public float attackInterval = 1f;

    [Tooltip("연사 속도 (총알 사이의 간격)")]
    public float burstInterval = 1f; 

    // 내부 로직 변수
    private float cooldownTimer = 0f;
    private float burstTimer = 0f;     // 연사 타이머
    private int shotsFired = 0;        // 현재 몇 발 쐈는지 체크
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
        if (isDragging)
        {
            HandleInput();
            return;
        }

        switch (currentState) // FSM
        {
            case DiceState.Idle:
                UpdateIdle();
                break;
            case DiceState.Attack:
                UpdateAttack();
                break;
            case DiceState.Cooldown:
                UpdateCooldown();
                break;
        }

        HandleInput();
    }

    void UpdateIdle()
    {
        if (currentTarget == null || !currentTarget.gameObject.activeInHierarchy ||
            Vector3.Distance(transform.position, currentTarget.position) > range)
        {
            FindTarget();
        }

        if (currentTarget != null)
        {
            ChangeState(DiceState.Attack);
        }
    }

    void UpdateAttack()
    {
        if (currentTarget == null || !currentTarget.gameObject.activeInHierarchy)
        {
            ChangeState(DiceState.Idle);
            return;
        }

        burstTimer += Time.deltaTime;

        if (burstTimer >= burstInterval)
        {
            burstTimer = 0f;
            FireProjectile();
            shotsFired++; // 발사 횟수 증가

            if (shotsFired >= dotCount)
            {
                ChangeState(DiceState.Cooldown);
            }
        }
    }

    void UpdateCooldown()
    {
        cooldownTimer += Time.deltaTime;

        if (cooldownTimer >= GetAttackInterval())
        {
            cooldownTimer = 0f;
            ChangeState(DiceState.Idle);
        }
    }

    void ChangeState(DiceState newState)
    {
        currentState = newState;

        if (newState == DiceState.Attack)
        {
            shotsFired = 0;
            burstTimer = burstInterval;
        }
    }

    void FireProjectile()
    {
        if (PoolManager.Instance == null) return;

        GameObject bulletObj = PoolManager.Instance.Spawn(projectilePrefab, transform.position, Quaternion.identity);
        Bullet bulletScript = bulletObj.GetComponent<Bullet>();

        
        int baseDmg = 10;
        if (DiceUpgradeManager.Instance != null)
        {
            baseDmg = DiceUpgradeManager.Instance.GetTotalDamage(type);
        }

        
        int finalDamage = baseDmg * dotCount;

        bulletScript.Init(currentTarget, finalDamage, type);
    }

    float GetAttackInterval()
    {
        if (type == DiceType.Wind) return attackInterval * 0.5f;
        return attackInterval;
    }

    public void Init(DiceType newType)
    {
        type = newType;
        dotCount = 1;
        currentState = DiceState.Idle; 
        UpdateColor();
    }

    public void UpdateColor() // 임시 색상
    {
        if (spriteRenderer == null) return;
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
        dotCount = count;
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
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 0.5f);
        Slot targetSlot = null;
        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject == gameObject) continue;

            Slot s = hit.GetComponent<Slot>();
            if (s != null)
            {
                targetSlot = s;
                break;
            }
        }
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

                if (type == targetDice.type && dotCount == targetDice.dotCount && dotCount < 6)
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
}