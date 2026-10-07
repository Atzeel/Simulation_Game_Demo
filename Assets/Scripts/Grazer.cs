using UnityEngine;

public class Grazer : MonoBehaviour
{
    public enum GrazerState
    {
        Searching,
        Hunting,
        Resting
    }

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

    [Header("Boundary")]
    public float boundaryMargin = 0.5f;

    private Vector2 moveDirection;
    private float directionTimer;

    private Spore targetSpore;

    private float restTimer;


    void Start()
    {
        PickRandomDirection();
        directionTimer = directionChangeTime;
    }


    void Update()
    {
        // Hunger constantly increases
        hunger += hungerRate * Time.deltaTime;

        // Die from starvation
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

        StayOnScreen();
    }


    
    // STATE CHECK
    

    void CheckState()
    {
        // Resting cannot be interrupted
        if (currentState == GrazerState.Resting)
        {
            return;
        }

        // Only hunt when actually hungry
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

        // Eating makes the Grazer less hungry
        hunger -= foodValue;

        hunger = Mathf.Clamp(
            hunger,
            0,
            maxHunger
        );

        // Rest after eating
        currentState = GrazerState.Resting;
        restTimer = restDuration;
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


   
    // SCREEN BOUNDARY
    

    void StayOnScreen()
    {
        Camera cam = Camera.main;

        if (cam == null)
        {
            return;
        }

        Vector3 bottomLeft =
            cam.ViewportToWorldPoint(new Vector3(0, 0, 0));

        Vector3 topRight =
            cam.ViewportToWorldPoint(new Vector3(1, 1, 0));

        Vector3 pos = transform.position;

        pos.x = Mathf.Clamp(
            pos.x,
            bottomLeft.x + boundaryMargin,
            topRight.x - boundaryMargin
        );

        pos.y = Mathf.Clamp(
            pos.y,
            bottomLeft.y + boundaryMargin,
            topRight.y - boundaryMargin
        );

        transform.position = pos;
    }
}