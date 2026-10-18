using UnityEngine;

public class Spore : MonoBehaviour
{
    public enum SporeState
    {
        Drifting,
        Seeking,
        Blooming
    }
    [Header("State Sprites")]
    public Sprite driftingSprite;
    public Sprite seekingSprite;
    public Sprite bloomingSprite;

    private SpriteRenderer spriteRenderer;
    [Header("State")]
    public SporeState currentState = SporeState.Drifting;

    [Header("Energy")]
    public float energy = 50f;
    public float maxEnergy = 100f;
    public float energyDrain = 2f;
    public float energyFromFood = 35f;

    [Header("Movement")]
    public float driftSpeed = 1f;
    public float seekSpeed = 2f;
    public float directionChangeTime = 2f;

    [Header("Food")]
    public float eatDistance = 0.7f;

    [Header("Reproduction")]
    public int nutrientsEaten = 0;
    public int nutrientsNeededToReproduce = 3;

    public GameObject sporePrefab;

    public int babiesPerBloom = 2;
    public float bloomDuration = 2f;
    public float reproductionEnergyCost = 30f;

    [Header("Boundary")]
    public float boundaryMargin = 0.5f;

    private Vector2 driftDirection;
    private float directionTimer;

    private GameObject targetNutrient;

    private bool isBlooming = false;
    private float bloomTimer;
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
        // Energy slowly decreases
        energy -= energyDrain * Time.deltaTime;

        // Death
        if (energy <= 0)
        {
            Destroy(gameObject);
            return;
        }

        DecideState();

        if (currentState == SporeState.Drifting)
        {
            Drift();
        }
        else if (currentState == SporeState.Seeking)
        {
            SeekFood();
        }
        else if (currentState == SporeState.Blooming)
        {
            Bloom();
        }

        UpdateSprite();
        StayInPlayArea();
    }



    // STATE DECISION


    void DecideState()
    {
        // Once blooming begins, don't interrupt it
        if (isBlooming)
        {
            currentState = SporeState.Blooming;
            return;
        }

        // Ate enough food -> reproduce
        if (nutrientsEaten >= nutrientsNeededToReproduce)
        {
            currentState = SporeState.Blooming;
        }

        // Hungry -> look for food
        else if (energy < 50f)
        {
            currentState = SporeState.Seeking;
        }

        // Otherwise wander
        else
        {
            currentState = SporeState.Drifting;
        }
    }



    // DRIFTING


    void Drift()
    {
        directionTimer -= Time.deltaTime;

        if (directionTimer <= 0)
        {
            PickRandomDirection();
            directionTimer = directionChangeTime;
        }

        transform.position +=
            (Vector3)(driftDirection * driftSpeed * Time.deltaTime);
    }


    void PickRandomDirection()
    {
        driftDirection =
            Random.insideUnitCircle.normalized;
    }



    // SEEKING FOOD


    void SeekFood()
    {
        if (targetNutrient == null)
        {
            targetNutrient = FindClosestNutrient();
        }

        // No food exists
        if (targetNutrient == null)
        {
            Drift();
            return;
        }

        float distance = Vector2.Distance(
            transform.position,
            targetNutrient.transform.position
        );

        // Close enough -> eat
        if (distance <= eatDistance)
        {
            EatNutrient();
            return;
        }

        Vector2 direction =
            (targetNutrient.transform.position -
             transform.position).normalized;

        transform.position +=
            (Vector3)(direction * seekSpeed * Time.deltaTime);
    }


    GameObject FindClosestNutrient()
    {
        GameObject[] nutrients =
            GameObject.FindGameObjectsWithTag("Nutrient");

        GameObject closest = null;
        float closestDistance = Mathf.Infinity;

        foreach (GameObject nutrient in nutrients)
        {
            float distance = Vector2.Distance(
                transform.position,
                nutrient.transform.position
            );

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = nutrient;
            }
        }

        return closest;
    }


    void EatNutrient()
    {
        if (targetNutrient == null)
        {
            return;
        }

        Debug.Log("Spore ate Nutrient!");

        energy += energyFromFood;

        energy = Mathf.Clamp(
            energy,
            0,
            maxEnergy
        );

        nutrientsEaten++;

        Destroy(targetNutrient);

        targetNutrient = null;
    }



    // BLOOMING / REPRODUCTION


    void Bloom()
    {
        if (!isBlooming)
        {
            isBlooming = true;
            bloomTimer = bloomDuration;

            Debug.Log("Spore started blooming!");
        }

        bloomTimer -= Time.deltaTime;

        if (bloomTimer <= 0)
        {
            Reproduce();

            nutrientsEaten = 0;

            energy -= reproductionEnergyCost;

            isBlooming = false;

            PickRandomDirection();
        }
    }


    void Reproduce()
    {
        if (sporePrefab == null)
        {
            Debug.LogWarning(
                "Spore Prefab has not been assigned!"
            );

            return;
        }

        Debug.Log("Spore reproduced!");

        for (int i = 0; i < babiesPerBloom; i++)
        {
            Vector2 offset =
                Random.insideUnitCircle.normalized *
                Random.Range(0.5f, 1f);

            GameObject baby = Instantiate(
                sporePrefab,
                transform.position + (Vector3)offset,
                Quaternion.identity
            );
            
            baby.transform.localScale = transform.localScale;

            Spore babySpore =
                baby.GetComponent<Spore>();

            if (babySpore != null)
            {
                babySpore.energy = 50f;
                babySpore.nutrientsEaten = 0;
            }
        }
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

        if (currentState == SporeState.Drifting)
        {
            spriteRenderer.sprite = driftingSprite;
        }
        else if (currentState == SporeState.Seeking)
        {
            spriteRenderer.sprite = seekingSprite;
        }
        else if (currentState == SporeState.Blooming)
        {
            spriteRenderer.sprite = bloomingSprite;
        }
    }
}