using TMPro;
using UnityEngine;
using System.Collections;

public enum DiceState
{
    Idle,       // 대기 
    Attack,     // 공격 (연사 중)
    Cooldown    // 휴식 
}

public enum TargetMode
{
    Closest,    // 디폴트
    Front,      // 맨앞
    MaxHP       // 체력
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
    public TextMeshPro levelText;

    [Header("Combat")]
    public GameObject projectilePrefab;
    public float range = 3f;

    [Header("Targeting")]
    public TargetMode targetMode = TargetMode.Closest;

    [Header("Synergy")]
    public float synergyMultiplier = 1.0f; // 기본 1배
    public GameObject synergyEffectObj;
    private Coroutine twinkleCoroutine;

    [Tooltip("공격 한 사이클(연사)이 끝나고 쉬는 시간")]
    public float attackInterval = 1f;

    [Tooltip("연사 속도 (총알 사이의 간격)")]
    public float burstInterval = 1f; 

    // 내부 로직 변수
    private float cooldownTimer = 0f;
    private float burstTimer = 0f;     // 연사 타이머
    private int shotsFired = 0;        // 현재 몇 발 쐈는지 체크
    private Transform currentTarget;
    private float specialValue = 0f;

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
        float damageFloat = (baseDmg * dotCount) * synergyMultiplier;
        int finalDamage = Mathf.RoundToInt(damageFloat);
        bulletScript.Init(currentTarget, finalDamage, type, specialValue);
    }

    float GetAttackInterval()
    {
        if (type == DiceType.Wind) return attackInterval * 0.5f;
        return attackInterval;
    }

    public void Init(DiceType newType)
    {
        type = newType;

        if (DataManager.Instance.diceDict.TryGetValue(type, out DiceStat stat))
        {
            attackInterval = stat.attackSpeed;
            range = stat.range;
            specialValue = stat.specialValue;
        }

        currentState = DiceState.Idle;
        UpdateColor();
        SetDotCount(1);
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

        if (levelText != null)
        {
            levelText.text = dotCount.ToString();
        }
    }

    void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos, Vector2.zero);

            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider.gameObject == gameObject)
                {
                    isDragging = true;
                    if (spriteRenderer != null) spriteRenderer.sortingOrder = 100;
                    if (levelText != null) levelText.sortingOrder = 101;
                    break;
                }
            }
        }

        if (isDragging && Input.GetMouseButton(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = -1f; 
            transform.position = mousePos;
        }

        if (isDragging && Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            CheckDrop();
        }
    }

    void CheckDrop()
    {
        Collider2D myCollider = GetComponent<Collider2D>();
        if (myCollider != null) myCollider.enabled = false;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos, Vector2.zero);

        Slot targetSlot = null;

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider.gameObject == gameObject) continue;

            Slot s = hit.collider.GetComponent<Slot>();
            if (s != null)
            {
                targetSlot = s;
                break; 
            }
        }

        if (targetSlot != null && targetSlot.currentDice != null)
        {
            Dice targetDice = targetSlot.currentDice;
            if (targetDice != this &&
                type == targetDice.type &&
                dotCount == targetDice.dotCount &&
                dotCount < 7)
            {
                currentSlot.RemoveDice();
                targetSlot.RemoveDice();

                PoolManager.Instance.ReturnToPool(gameObject);
                PoolManager.Instance.ReturnToPool(targetDice.gameObject);
                if (BoardManager.Instance != null)
                {
                    BoardManager.Instance.SpawnMergedDiceRandom(dotCount + 1);
                }

                return; 
            }
        }
        ReturnToSlot();
    }

    void ReturnToSlot()
    {
        if (currentSlot != null)
        {
            Vector3 targetPos = currentSlot.transform.position;
            targetPos.z = -0.1f;
            transform.position = targetPos;
            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = 5; 
                if (levelText != null) levelText.sortingOrder = 6; 
            }
        }
    }

    void FindTarget()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, range);

        Enemy bestTarget = null;
        float maxScore = -1f;     
        float minDistance = Mathf.Infinity; 

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                Enemy enemy = hit.GetComponent<Enemy>();
                if (enemy == null || !enemy.gameObject.activeInHierarchy) continue;
                switch (targetMode)
                {
                    case TargetMode.Closest:
                        float dist = Vector3.Distance(transform.position, enemy.transform.position);
                        if (dist < minDistance)
                        {
                            minDistance = dist;
                            bestTarget = enemy;
                        }
                        break;

                    case TargetMode.Front: 
                        float progress = enemy.GetProgress();
                        if (progress > maxScore)
                        {
                            maxScore = progress;
                            bestTarget = enemy;
                        }
                        break;

                    case TargetMode.MaxHP:
                        if (enemy.currentHp > maxScore)
                        {
                            maxScore = enemy.currentHp;
                            bestTarget = enemy;
                        }
                        break;
                }
            }
        }

        currentTarget = (bestTarget != null) ? bestTarget.transform : null;
    }

    public void ToggleTargetMode()
    {
        int current = (int)targetMode;
        int next = (current + 1) % 3;
        targetMode = (TargetMode)next;
        Debug.Log($"주사위 타겟 모드: {targetMode}");
        FindTarget();
    }

    public void SetTargetMode(TargetMode mode)
    {
        targetMode = mode;
        FindTarget();
    }

    public void SetSynergy(float multiplier)
    {
        synergyMultiplier = multiplier;
        if (synergyMultiplier > 1.01f)
        {
            if (!synergyEffectObj.activeSelf)
            {
                synergyEffectObj.SetActive(true);
                if (twinkleCoroutine != null) StopCoroutine(twinkleCoroutine);
                twinkleCoroutine = StartCoroutine(TwinkleEffectRoutine());
            }
        }
        else
        {
            if (synergyEffectObj.activeSelf)
            {
                synergyEffectObj.SetActive(false);
                if (twinkleCoroutine != null) StopCoroutine(twinkleCoroutine);
            }
        }
    }

    IEnumerator TwinkleEffectRoutine()
    {
        SpriteRenderer glowSR = synergyEffectObj.GetComponent<SpriteRenderer>();
        Color baseColor = glowSR.color;

        Vector3 originalScale = Vector3.one * 1.3f; 

        float speed = 3.0f;

        while (true)
        {
            float time = Mathf.PingPong(Time.time * speed, 1f);
            float alpha = Mathf.Lerp(0.4f, 1.0f, time);
            glowSR.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
            synergyEffectObj.transform.localScale = Vector3.Lerp(originalScale, originalScale * 1.15f, time);

            yield return null;
        }
    }
}