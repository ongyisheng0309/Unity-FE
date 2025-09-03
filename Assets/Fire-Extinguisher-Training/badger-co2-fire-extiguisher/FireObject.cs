using UnityEngine;

public class FireObject : MonoBehaviour
{
    [Header("Fire Settings")]
    public float fireHealth = 100f;           // How much "health" the fire has
    public float extinguishRate = 20f;        // How fast the fire is extinguished
    public ParticleSystem fireParticles;      // Fire particle effect
    public Light fireLight;                   // Fire light component

    private float originalIntensity;
    private bool isExtinguished = false;

    void Start()
    {
        // Store original light intensity
        if (fireLight != null)
            originalIntensity = fireLight.intensity;
    }

    void OnParticleCollision(GameObject other)
    {
        // Check if hit by extinguisher foam
        if (other.CompareTag("ExtinguisherFoam") && !isExtinguished)
        {
            ExtinguishFire();
        }
    }

    void OnTriggerStay(Collider other)
    {
        // Alternative detection method using trigger
        if (other.CompareTag("ExtinguisherFoam") && !isExtinguished)
        {
            ExtinguishFire();
        }
    }

    void ExtinguishFire()
    {
        fireHealth -= extinguishRate * Time.deltaTime;

        // Scale down fire effects based on health
        if (fireParticles != null)
        {
            var emission = fireParticles.emission;
            emission.rateOverTime = Mathf.Lerp(0, 50, fireHealth / 100f);
        }

        if (fireLight != null)
        {
            fireLight.intensity = Mathf.Lerp(0, originalIntensity, fireHealth / 100f);
        }

        // Check if fire is completely extinguished
        if (fireHealth <= 0 && !isExtinguished)
        {
            isExtinguished = true;
            Debug.Log("Fire extinguished successfully!");

            if (fireParticles != null)
                fireParticles.Stop();

            if (fireLight != null)
                fireLight.enabled = false;

            // Could trigger success event here
            OnFireExtinguished();
        }
    }

    void OnFireExtinguished()
    {
        // This is where you'd trigger PASS condition
        Debug.Log("TEST PASSED: Fire successfully extinguished!");

        // You could send this to a test manager
        TestManager testManager = FindObjectOfType<TestManager>();
        if (testManager != null)
            testManager.OnTestPassed();
    }
}