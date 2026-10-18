using UnityEngine;

public class Predator : MonoBehaviour
{
    public enum PredatorState
    {
        Patrolling,
        Chasing,
        Digesting
    }

    [Header("State Sprites")]
    public Sprite patrollingSprite;
    public Sprite chasingSprite;
    public Sprite digestingSprite;

    private SpriteRenderer spriteRenderer;

    [Header("State")]
    public PredatorState currentState = PredatorState.Patrolling;

    [Header("Hunger")]
    public float hunger = 0f;
    public float hungerRate = 4f;
    public float maxHunger = 100f;
    public float huntThreshold = 30f;
    public float foodValue = 70f;

    [Header("Movement")]
    public float patrolSpeed = 0.8f;
    public float chaseSpeed = 3.5f;
    public float directionChangeTime = 3f;

    [Header("Hunting")]
    public float detectRange = 6f;
    public float eatDistance = 0.7f;

    [Header("Digesting")]
    public float digestTime = 3f;

    [Header("Reproduction")]
    public int grazersEaten = 0;
    public int grazersNeededToReproduce = 2;
    public GameObject predatorPrefab;
    public int babiesPerBirth = 1;

    [Header("Boundary")]
    public float boundaryMargin = 0.5f;

    private Vector2 moveDirection;
    private float directionTimer;

    private Grazer targetGrazer;

    private float digestTimer;

    private BoxCollider2D playArea;


    void Start()
    {
        PickRandomDirection();
        directionTimer = directionChangeTime;

        spriteRenderer = GetComponent<SpriteRenderer>();

        GameObject areaObject = GameObject.Find("PlayArea");

        if (areaObject != null)
        {
            playArea = areaObject.GetComponent<BoxCollider2D>();
        }

        UpdateSprite();
    }


    void Update()
    {
        // Hunger increases over time
        hunger += hungerRate * Time.deltaTime;

        // Starvation
        if (hunger >= maxHunger)
        {
            Destroy(gameObject);
            return;
        }

        CheckState();

        if (currentState == PredatorState.Patrolling)
        {
            Patrol();
        }
        else if (currentState == PredatorState.Chasing)
        {
            Chase();
        }
        else if (currentState == PredatorState.Digesting)
        {
            Digest();
        }

        // Update art AFTER behavior/state changes
        UpdateSprite();

        StayInPlayArea();
    }


    // STATE


    void CheckState()
    {
        // Digesting cannot be interrupted
        if (currentState == PredatorState.Digesting)
        {
            return;
        }

        // Only hunt when hungry enough
        if (hunger >= huntThreshold)
        {
            targetGrazer = FindClosestGrazer();

            if (targetGrazer != null)
            {
                float distance = Vector2.Distance(
                    transform.position,
                    targetGrazer.transform.position
                );

                if (distance <= detectRange)
                {
                    currentState = PredatorState.Chasing;
                    return;
                }
            }
        }

        currentState = PredatorState.Patrolling;
    }



    // PATROLLING


    void Patrol()
    {
        directionTimer -= Time.deltaTime;

        if (directionTimer <= 0)
        {
            PickRandomDirection();
            directionTimer = directionChangeTime;
        }

        transform.position +=
            (Vector3)(moveDirection * patrolSpeed * Time.deltaTime);
    }


    void PickRandomDirection()
    {
        moveDirection = Random.insideUnitCircle.normalized;
    }



    // CHASING


    void Chase()
    {
        if (targetGrazer == null)
        {
            currentState = PredatorState.Patrolling;
            return;
        }

        Vector2 direction =
            (targetGrazer.transform.position - transform.position).normalized;

        transform.position +=
            (Vector3)(direction * chaseSpeed * Time.deltaTime);

        float distance = Vector2.Distance(
            transform.position,
            targetGrazer.transform.position
        );

        if (distance <= eatDistance)
        {
            EatGrazer();
        }
    }


    void EatGrazer()
    {
        if (targetGrazer == null)
        {
            return;
        }

        Debug.Log("Predator ate Grazer!");

        Destroy(targetGrazer.gameObject);
        targetGrazer = null;

        hunger -= foodValue;

        hunger = Mathf.Clamp(
            hunger,
            0,
            maxHunger
        );

        grazersEaten++;

        // Reproduce after eating enough grazers
        if (grazersEaten >= grazersNeededToReproduce)
        {
            Reproduce();
            grazersEaten = 0;
        }

        currentState = PredatorState.Digesting;
        digestTimer = digestTime;
    }



    // REPRODUCTION
  

    void Reproduce()
    {
        if (predatorPrefab == null)
        {
            Debug.LogWarning("Predator Prefab is missing!");
            return;
        }

        Debug.Log("Predator reproduced!");

        for (int i = 0; i < babiesPerBirth; i++)
        {
            Vector2 offset =
                Random.insideUnitCircle.normalized *
                Random.Range(0.5f, 1f);

            GameObject baby = Instantiate(
                predatorPrefab,
                transform.position + (Vector3)offset,
                Quaternion.identity
            );

            Predator babyPredator =
                baby.GetComponent<Predator>();

            if (babyPredator != null)
            {
                babyPredator.hunger = 0f;
                babyPredator.grazersEaten = 0;
                
                // Make sure the baby can reproduce too
                babyPredator.predatorPrefab = predatorPrefab;
            }
        }
    }


    // =====================================
    // DIGESTING
    // =====================================

    void Digest()
    {
        digestTimer -= Time.deltaTime;

        if (digestTimer <= 0)
        {
            currentState = PredatorState.Patrolling;
            PickRandomDirection();
        }
    }


    // =====================================
    // FIND GRAZER
    // =====================================

    Grazer FindClosestGrazer()
    {
        Grazer[] grazers = FindObjectsByType<Grazer>(
            FindObjectsSortMode.None
        );

        Grazer closest = null;
        float closestDistance = Mathf.Infinity;

        foreach (Grazer grazer in grazers)
        {
            float distance = Vector2.Distance(
                transform.position,
                grazer.transform.position
            );

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = grazer;
            }
        }

        return closest;
    }


    // =====================================
    // SPRITE
    // =====================================

    void UpdateSprite()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        if (currentState == PredatorState.Patrolling)
        {
            spriteRenderer.sprite = patrollingSprite;
        }
        else if (currentState == PredatorState.Chasing)
        {
            spriteRenderer.sprite = chasingSprite;
        }
        else if (currentState == PredatorState.Digesting)
        {
            spriteRenderer.sprite = digestingSprite;
        }
    }


    // =====================================
    // PLAY AREA BOUNDARY
    // =====================================

    void StayInPlayArea()
    {
        if (playArea == null)
        {
            return;
        }

        Bounds bounds = playArea.bounds;

        Vector3 pos = transform.position;

        pos.x = Mathf.Clamp(
            pos.x,
            bounds.min.x + boundaryMargin,
            bounds.max.x - boundaryMargin
        );

        pos.y = Mathf.Clamp(
            pos.y,
            bounds.min.y + boundaryMargin,
            bounds.max.y - boundaryMargin
        );

        transform.position = pos;
    }
}