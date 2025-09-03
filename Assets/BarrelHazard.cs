using UnityEngine;

public class BarrelHazard : MonoBehaviour
{
    [Header("Assign your Fire prefab here")]
    public GameObject firePrefab;

    [Header("Drag the FirePoint child here")]
    public Transform firePoint;

    private bool hasIgnited = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasIgnited) return;

        if (other.CompareTag("Spark")) // lighter must have the Spark tag
        {
            Debug.Log("Barrel ignited by Spark!");

            // Spawn fire at FirePoint and attach it to the barrel
            GameObject fire = Instantiate(firePrefab, firePoint.position, Quaternion.identity);
            fire.transform.SetParent(transform);

            hasIgnited = true;
        }
    }
}

