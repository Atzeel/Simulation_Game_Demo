using System.Collections.Generic;
using UnityEngine;

public class Spore : MonoBehaviour
{
    public enum SporeState
    {
        Drifting,
        Seeking,
        Blooming
    }

    [Header("State")]
    public SporeState currentState;

    [Header("Food")]
    public List<Transform> nutrients = new List<Transform>();

    [Header("Energy")]
    public float energy = 50f;
    public float energyDrain = 1f;
    public float energyFromFood = 60f;

    [Header("Movement")]
    public float driftSpeed = 1f;
    public float seekSpeed = 2f;
    public float eatDistance = 0.7f;

    [Header("Blooming")]
    public float bloomDuration = 2f;
    public float bloomEnergyCost = 40f;

    [Header("Boundary")]
    public float boundaryMargin = 0.5f;

    private Vector2 driftDirection;
    private float directionTimer;

    private Transform targetNutrient;

    private float bloomTimer;
    private bool bloomingStarted = false;


    void Start()
    {
        PickRandomDirection();
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

        CheckState();

        if (currentState == SporeState.Drifting)
        {
            Drift();
        }
        else if (currentState == SporeState.Seeking)
        {
            Seek();
        }
        else if (currentState == SporeState.Blooming)
        {
            Bloom();
        }

        StayOnScreen();
    }


    
    // STATE
    

    void CheckState()
    {
        if (bloomingStarted)
        {
            currentState = SporeState.Blooming;
            return;
        }

        if (energy < 30f)
        {
            currentState = SporeState.Seeking;
        }
        else if (energy > 70f)
        {
            currentState = SporeState.Blooming;
        }
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
            directionTimer = 2f;
        }

        transform.position +=
            (Vector3)(driftDirection * driftSpeed * Time.deltaTime);
    }


    void PickRandomDirection()
    {
        driftDirection = Random.insideUnitCircle.normalized;
    }


    // SEEKING


    void Seek()
    {
        // If current target disappeared, find another one
        if (targetNutrient == null)
        {
            targetNutrient = FindClosestNutrient();
        }

        // No food left
        if (targetNutrient == null)
        {
            Drift();
            return;
        }

        float distance = Vector2.Distance(
            transform.position,
            targetNutrient.position
        );

        if (distance <= eatDistance)
        {
            Eat();
            return;
        }

        Vector2 direction =
            (targetNutrient.position - transform.position).normalized;

        transform.position +=
            (Vector3)(direction * seekSpeed * Time.deltaTime);
    }


    Transform FindClosestNutrient()
    {
        Transform closest = null;
        float closestDistance = Mathf.Infinity;

        // Remove nutrients that have already been destroyed
        nutrients.RemoveAll(item => item == null);

        foreach (Transform nutrient in nutrients)
        {
            float distance = Vector2.Distance(
                transform.position,
                nutrient.position
            );

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = nutrient;
            }
        }

        return closest;
    }


    void Eat()
    {
        if (targetNutrient == null)
        {
            return;
        }

        Debug.Log("Spore ate Nutrient!");

        energy += energyFromFood;

        // Remove it from the list
        nutrients.Remove(targetNutrient);

        // Destroy food
        Destroy(targetNutrient.gameObject);

        targetNutrient = null;
    }


   
    // BLOOMING
    

    void Bloom()
    {
        if (!bloomingStarted)
        {
            bloomingStarted = true;
            bloomTimer = bloomDuration;
        }

        bloomTimer -= Time.deltaTime;

        if (bloomTimer <= 0)
        {
            energy -= bloomEnergyCost;

            bloomingStarted = false;

            PickRandomDirection();
        }
    }


    
    // BOUNDARY
    // =========================

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