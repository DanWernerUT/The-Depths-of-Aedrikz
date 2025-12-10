using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float impactDuration = 2f;
    [SerializeField] private float speed = 100f;
    [SerializeField] private float manaCost = 10f;
    [SerializeField] private GameObject impactVFX;
    [SerializeField] private AudioClip impactSFX;
    [SerializeField] private float impactVolume = .5f;
    [SerializeField] private GameObject lightPrefab;
    [SerializeField] private float impactLightMultiplier = 3f;
    [SerializeField] private float lightFadeSpeed = 10f;
    [SerializeField] private float impactDamage = 20f;
    [SerializeField] private float impactRadius = 5f;

    // Optional: set this from your shooting script
    public Vector3 direction;
    private ManaScript playerMana;
    private GameObject lightInstance;

    void Start()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        playerMana = FindFirstObjectByType<ManaScript>();

        if (GameState.paused)
        {
            Destroy(gameObject);
            return;
        }

        if (playerMana != null)
        {
            if (playerMana.GetMana() > 0)
                playerMana.SpendMana(manaCost);
            else
                Destroy(gameObject);
        }

        if (direction != Vector3.zero)
            rb.linearVelocity = direction.normalized * speed;
        else
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
                rb.linearVelocity = mainCam.transform.forward * speed;
            else
                rb.linearVelocity = transform.forward * speed;
        }

        // Instantiate light as a child so it moves with the projectile
        if (lightPrefab != null)
        {
            lightInstance = Instantiate(lightPrefab, transform.position, Quaternion.identity);
            lightInstance.transform.SetParent(transform);
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        Debug.Log("Projectile hit: " + other.gameObject.name);

        // Store the impact position
        Vector3 impactPosition = transform.position;

        // Multiply light intensity on impact and start fading
        if (lightInstance != null)
        {
            Light light = lightInstance.GetComponent<Light>();
            if (light != null)
            {
                light.intensity *= impactLightMultiplier;
            }

            // Unparent light so it stays at impact position
            lightInstance.transform.SetParent(null);

            // Add a component to handle the fade
            LightFader fader = lightInstance.AddComponent<LightFader>();
            fader.fadeSpeed = lightFadeSpeed;
        }

        Collider[] hits = Physics.OverlapSphere(impactPosition, impactRadius);

        foreach (Collider c in hits)
        {
            HealthScript health = c.GetComponent<HealthScript>();
            if (health != null)
            {
                health.LoseHealth(impactDamage);
                Debug.Log($"Damaged {c.gameObject.name} for {impactDamage}");
            }
        }

        // Spawn VFX
        GameObject fx = Instantiate(impactVFX, impactPosition, Quaternion.identity);
        Destroy(fx, impactDuration);

        GameObject soundPosition = new GameObject("ImpactSound");
        soundPosition.transform.position = impactPosition;
        SoundFXManager.instance.PlaySoundFXClip(impactSFX, soundPosition.transform, impactVolume);
        Destroy(soundPosition, impactDuration);

        Destroy(gameObject);
    }
}

public class LightFader : MonoBehaviour
{
    public float fadeSpeed = 1000f;
    private Light lightComponent;

    void Start()
    {
        lightComponent = GetComponent<Light>();
    }

    void Update()
    {
        if (lightComponent != null)
        {
            lightComponent.intensity *= Mathf.Pow(0.1f, fadeSpeed * Time.deltaTime);
            if (lightComponent.intensity <= 0)
            {
                Destroy(gameObject);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
}