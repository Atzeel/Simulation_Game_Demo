using UnityEngine;

public class Grazer : MonoBehaviour
{
    public enum GrazerState
    {
        Searching,
        Hunting,
        Resting
    }

    [Header("State Sprites")]
    public Sprite searchingSprite;
    public Sprite huntingSprite;
    public Sprite restingSprite;

    private SpriteRenderer spriteRenderer;
    [Header("State")]
    public GrazerState currentState = GrazerState.Searching;

    [Header("Hunger")]
    public float hunger = 0f;
    public float hungerRate = 5f;
    public float maxHunger = 100f;
    public float hungryThreshold = 30f;
    public float foodValue = 60f;

    [Header("Movement")]
    public float searchSpeed = 1f;
    public float huntSpeed = 3f;
    public float directionChangeTime = 1.5f;

    [Header("Hunting")]
    public float detectRange = 5f;
    public float eatDistance = 0.6f;

    [Header("Resting")]
    public float restDuration = 3f;

    [Header("Reproduction")]
    public int sporesEaten = 0;
    public int sporesNeededToReproduce = 3;
    public GameObject grazerPrefab;
    public int babiesPerBirth = 1;

    [Header("Boundary")]
    public float boundaryMargin = 0.5f;

    private Vector2 moveDirection;
    private float directionTimer;

    private Spore targetSpore;

    private float restTimer;
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
    }


    void Update()
    {
        // Hunger constantly increases
        hunger += hungerRate * Time.deltaTime;

        // Starvation
        if (hunger >= maxHunger)
        {
            Destroy(gameObject);
            return;
        }

        CheckState();

        if (currentState == GrazerState.Searching)
        {
            Search();
        }
        else if (currentState == GrazerState.Hunting)
        {
            Hunt();
        }
        else if (currentState == GrazerState.Resting)
        {
            Rest();
        }
        UpdateSprite();

        StayInPlayArea();
    }



    // STATE


    void CheckState()
    {
        // Resting cannot be interrupted
        if (currentState == GrazerState.Resting)
        {
            return;
        }

        // Only hunt if hungry
        if (hunger >= hungryThreshold)
        {
            targetSpore = FindClosestSpore();

            if (targetSpore != null)
            {
                float distance = Vector2.Distance(
                    transform.position,
                    targetSpore.transform.position
                );

                if (distance <= detectRange)
                {
                    currentState = GrazerState.Hunting;
                    return;
                }
            }
        }

        currentState = GrazerState.Searching;
    }



    // SEARCHING


    void Search()
    {
        directionTimer -= Time.deltaTime;

        if (directionTimer <= 0)
        {
            PickRandomDirection();
            directionTimer = directionChangeTime;
        }

        transform.position +=
            (Vector3)(moveDirection * searchSpeed * Time.deltaTime);
    }


    void PickRandomDirection()
    {
        moveDirection = Random.insideUnitCircle.normalized;
    }


 
    // HUNTING


    void Hunt()
    {
        if (targetSpore == null)
        {
            currentState = GrazerState.Searching;
            return;
        }

        Vector2 direction =
            (targetSpore.transform.position - transform.position).normalized;

        transform.position +=
            (Vector3)(direction * huntSpeed * Time.deltaTime);

        float distance = Vector2.Distance(
            transform.position,
            targetSpore.transform.position
        );

        if (distance <= eatDistance)
        {
            EatSpore();
        }
    }


    void EatSpore()
    {
        if (targetSpore == null)
        {
            return;
        }

        Debug.Log("Grazer ate a Spore!");

        Destroy(targetSpore.gameObject);
        targetSpore = null;

        // Reduce hunger
        hunger -= foodValue;

        hunger = Mathf.Clamp(
            hunger,
            0,
            maxHunger
        );

        // Count food
        sporesEaten++;

        // Reproduce after eating enough spores
        if (sporesEaten >= sporesNeededToReproduce)
        {
            Reproduce();
            sporesEaten = 0;
        }

        // Rest after eating
        currentState = GrazerState.Resting;
        restTimer = restDuration;
    }


    // REPRODUCTION


    void Reproduce()
    {
        if (grazerPrefab == null)
        {
            Debug.LogWarning("Grazer Prefab is missing!");
            return;
        }

        Debug.Log("Grazer reproduced!");

        for (int i = 0; i < babiesPerBirth; i++)
        {
            Vector2 offset =
                Random.insideUnitCircle.normalized *
                Random.Range(0.5f, 1f);

            GameObject baby = Instantiate(
                grazerPrefab,
                transform.position + (Vector3)offset,
                Quaternion.identity
            );

            Grazer babyGrazer =
                baby.GetComponent<Grazer>();

            if (babyGrazer != null)
            {
                babyGrazer.hunger = 0f;
                babyGrazer.sporesEaten = 0;
            }
        }
    }


  
    // RESTING
    

    void Rest()
    {
        restTimer -= Time.deltaTime;

        if (restTimer <= 0)
        {
            currentState = GrazerState.Searching;
            PickRandomDirection();
        }
    }


    
    // FIND SPORE
   

    Spore FindClosestSpore()
    {
        Spore[] spores = FindObjectsByType<Spore>(
            FindObjectsSortMode.None
        );

        Spore closest = null;
        float closestDistance = Mathf.Infinity;

        foreach (Spore spore in spores)
        {
            float distance = Vector2.Distance(
                transform.position,
                spore.transform.position
            );

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = spore;
            }
        }

        return closest;
    }


    // BOUNDARY
   

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
    
    void UpdateSprite()
    {
        if (spriteRenderer == null)
            return;

        if (currentState == GrazerState.Searching)
        {
            spriteRenderer.sprite = searchingSprite;
        }
        else if (currentState == GrazerState.Hunting)
        {
            spriteRenderer.sprite = huntingSprite;
        }
        else if (currentState == GrazerState.Resting)
        {
            spriteRenderer.sprite = restingSprite;
        }
    }
}