using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class RockSmasher : MonoBehaviour
{
    [Header("--- [Broken Pieces Settings] ---")]
    public List<GameObject> brokenPieces;
    public float scatterRadius = 0.5f;

    [Header("--- [Physics Settings] ---")]
    [Tooltip("Force applied in the direction of the hit")]
    public float hitPushForce = 8.0f;
    [Tooltip("Random spread amount for flying pieces")]
    public float spreadAmount = 2.0f;

    [Header("--- [Health & Damage Settings] ---")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Tooltip("Damage multiplier based on velocity")]
    public float damageMultiplier = 5.0f;

    [Tooltip("Minimum velocity to register a hit")]
    public float minDamageVelocity = 2.0f;

    [Header("--- [Visual Effects] ---")]
    public Renderer rockRenderer;
    public Material[] crackStages;

    [Header("--- [Sound Settings] ---")]
    public AudioClip hitSound;
    public AudioClip breakSound;

    // ★ NEW: Volume Multipliers
    [Range(0.1f, 5.0f)]
    public float hitVolumeScale = 1.0f;   // Scale up to make hit sound louder
    [Range(0.1f, 5.0f)]
    public float breakVolumeScale = 1.0f; // Scale up to make break sound louder

    private AudioSource audioSource;

    // Internal Variables
    private int currentCrackIndex = -1;
    private Vector3 lastHitDirection;

    void Start()
    {
        currentHealth = maxHealth;
        audioSource = GetComponent<AudioSource>();
        if (rockRenderer == null) rockRenderer = GetComponentInChildren<Renderer>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (currentHealth <= 0) return;

        if (collision.gameObject.CompareTag("BreakingStone"))
        {
            float hitSpeed = collision.relativeVelocity.magnitude;

            if (hitSpeed >= minDamageVelocity)
            {
                lastHitDirection = collision.relativeVelocity.normalized;
                ApplyDamage(hitSpeed);
            }
        }
    }

    void ApplyDamage(float hitSpeed)
    {
        float damage = hitSpeed * damageMultiplier;
        currentHealth -= damage;

        Debug.Log($"[Rock Hit] Speed: {hitSpeed:F1} | Damage: {damage:F1} | HP: {currentHealth:F1}/{maxHealth}");

        // 1. Play Hit Sound (With Volume Scale)
        if (hitSound != null)
        {
            // Base volume depends on speed (0.0 ~ 1.0)
            float baseVolume = Mathf.Clamp01(hitSpeed / 20f);

            // ★ Apply the multiplier (hitVolumeScale)
            float finalVolume = baseVolume * hitVolumeScale;

            audioSource.PlayOneShot(hitSound, finalVolume);
        }

        if (currentHealth <= 0)
        {
            Smash(); // Destroy!
        }
        else
        {
            UpdateCrackVisuals();
        }
    }

    void UpdateCrackVisuals()
    {
        if (crackStages == null || crackStages.Length == 0) return;

        float healthPercent = currentHealth / maxHealth;
        int stageToApply = -1;

        if (healthPercent <= 0.25f && crackStages.Length >= 3) stageToApply = 2;
        else if (healthPercent <= 0.5f && crackStages.Length >= 2) stageToApply = 1;
        else if (healthPercent <= 0.75f && crackStages.Length >= 1) stageToApply = 0;

        if (stageToApply != -1 && stageToApply != currentCrackIndex)
        {
            currentCrackIndex = stageToApply;
            rockRenderer.material = crackStages[currentCrackIndex];
        }
    }

    void Smash()
    {
        StartCoroutine(SmashRoutine());
    }

    IEnumerator SmashRoutine()
    {
        // 1. Play Break Sound (With Volume Scale)
        if (breakSound != null)
        {
            // ★ Apply the multiplier (breakVolumeScale)
            audioSource.PlayOneShot(breakSound, 1.0f * breakVolumeScale);
        }

        if (rockRenderer != null) rockRenderer.enabled = false;
        Collider myCollider = GetComponentInChildren<Collider>();
        if (myCollider != null) myCollider.enabled = false;

        foreach (GameObject piece in brokenPieces)
        {
            if (piece != null)
            {
                Vector3 randomPos = transform.position + (Random.insideUnitSphere * scatterRadius);
                piece.transform.position = randomPos;
                piece.transform.rotation = Random.rotation;

                piece.SetActive(true);
                piece.transform.parent = null;

                Rigidbody rb = piece.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 finalVelocity = (lastHitDirection * hitPushForce) + (Random.insideUnitSphere * spreadAmount);
                    rb.velocity = finalVelocity;
                }

                yield return new WaitForSeconds(0.01f);
            }
        }

        yield return new WaitForSeconds(1.0f);
        gameObject.SetActive(false);
    }
}