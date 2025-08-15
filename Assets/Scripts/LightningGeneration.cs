using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class LightningGeneration : MonoBehaviour
{
    [Header("Lightning Settings")]
    [Tooltip("The lightning prefab with a Visual Effect component.")]
    public GameObject lightningPrefab;

    [Tooltip("Minimum number of lightning strikes to spawn.")]
    public int minStrikes = 1;

    [Tooltip("Maximum number of lightning strikes to spawn.")]
    public int maxStrikes = 3;

    [Tooltip("How far from this object lightning can spawn.")]
    public float spawnRadius = 5f;

    [Tooltip("Frequency of lightning bursts.")]
    public float spawnInterval = 2f;

    [Tooltip("Range of randomness for the spawning interval.")]
    public float descrepency;

    private float timer = 0;

    void Update()
    {
        if (timer <= 0)
        {
            GenerateLightning();
            timer = spawnInterval;
        }
        timer -= Time.deltaTime;
    }

    /// <summary>
    /// Call this method to generate lightning.
    /// </summary>
    private void GenerateLightning()
    {
        StartCoroutine(SpawnLightningRoutine());
    }

    private IEnumerator SpawnLightningRoutine()
    {
        if (lightningPrefab == null)
        {
            Debug.LogWarning("No lightning prefab assigned to LightningGeneration.");
            yield break;
        }

        int strikeCount = Random.Range(minStrikes, maxStrikes + 1);
        List<GameObject> spawned = new List<GameObject>();

        for (int i = 0; i < strikeCount; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPos = new Vector3(
                transform.position.x + randomCircle.x,
                transform.position.y, // keep same Y level
                transform.position.z + randomCircle.y
            );

            // Random Z rotation
            Quaternion randomRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

            GameObject lightning = Instantiate(lightningPrefab, spawnPos, randomRotation);
            spawned.Add(lightning);

            // Play the VFX Graph effect immediately
            VisualEffect vfx = lightning.GetComponent<VisualEffect>();
            if (vfx != null)
            {
                vfx.Play();
            }
        }

        yield return new WaitForSeconds(spawnInterval + Random.Range(-descrepency, descrepency));

        foreach (var obj in spawned)
        {
            if (obj != null)
                Destroy(obj);
        }
    }
}
