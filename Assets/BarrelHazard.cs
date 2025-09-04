using UnityEngine;

public class BarrelHazard : MonoBehaviour
{
    [Header("Assign your Fire prefab here")]
    public GameObject firePrefab;
    
    [Header("Drag the FirePoint child here")]
    public Transform firePoint;
    
    private bool hasIgnited = false;
    private GameObject spawnedFire;

    private void OnTriggerEnter(Collider other)
    {
        if (hasIgnited) return;
        
        if (other.CompareTag("Spark")) // lighter must have the Spark tag
        {
            Debug.Log("🔥 Barrel ignited by Spark!");
            
            // Spawn fire
            spawnedFire = Instantiate(firePrefab, firePoint.position, Quaternion.identity);
            spawnedFire.transform.SetParent(transform);
            
            // Register with test manager
            DynamicTestManager testManager = FindObjectOfType<DynamicTestManager>();
            if (testManager != null)
            {
                // Just register the fire GameObject directly
                testManager.RegisterNewFire(spawnedFire.GetComponent<FireObject>());
                Debug.Log("✅ Fire registered with TestManager");
            }
            
            hasIgnited = true;
        }
    }
}