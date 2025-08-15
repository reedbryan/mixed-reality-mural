using UnityEngine;
using System.Collections;

public class ScaleEffect : MonoBehaviour
{
    [Header("Effect Settings")]
    public float duration = 1f; // seconds for shrink/grow
    public float minScale = 0.01f; // how small it shrinks before disabling

    private Vector3 originalScale;
    private Coroutine currentRoutine;

    void Awake()
    {
        originalScale = transform.localScale;
    }

    /// <summary>
    /// Triggers the scaling effect.
    /// Pass true to grow in, false to shrink out.
    /// </summary>
    public void ToggleViewable(bool grow)
    {
        // Stop any running animation
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        // Start new animation
        currentRoutine = StartCoroutine(ScaleRoutine(grow));
    }

    private IEnumerator ScaleRoutine(bool grow)
    {
        if (grow)
        {
            gameObject.SetActive(true); // ensure visible
        }

        Vector3 startScale = transform.localScale;
        Vector3 targetScale = grow ? originalScale : Vector3.one * minScale;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        transform.localScale = targetScale; // snap to final

        if (!grow)
        {
            gameObject.SetActive(false); // disable after shrinking
        }
    }
}
