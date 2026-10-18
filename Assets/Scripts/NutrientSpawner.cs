using UnityEngine;
using UnityEngine.InputSystem;

public class NutrientSpawner : MonoBehaviour
{
    [Header("Drag these in Inspector")]
    public GameObject nutrientPrefab;
    public Camera worldCamera;

    void Update()
    {
        if (Mouse.current == null)
        {
            return;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            SpawnNutrient();
        }
    }

    void SpawnNutrient()
    {
        if (nutrientPrefab == null)
        {
            Debug.LogWarning("Nutrient Prefab is missing!");
            return;
        }

        if (worldCamera == null)
        {
            Debug.LogWarning("World Camera is missing!");
            return;
        }

        Vector2 mousePosition =
            Mouse.current.position.ReadValue();

        Vector3 worldPosition =
            worldCamera.ScreenToWorldPoint(
                new Vector3(
                    mousePosition.x,
                    mousePosition.y,
                    0f
                )
            );

        worldPosition.z = 0f;

        Instantiate(
            nutrientPrefab,
            worldPosition,
            Quaternion.identity
        );

        Debug.Log("Spawned Nutrient!");
    }
}