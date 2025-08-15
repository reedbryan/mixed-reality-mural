using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneObjectManager : MonoBehaviour
{
    [Header("List of GameObjects that change from OSC messages")]
    [SerializeField] private List<GameObject> objectList;

    [Header("Index of the currently displayed object")]
    [SerializeField] private int activeObject;

    [Header("Time each object has before it can be changed")]
    [SerializeField] private float coolDown;
    private float timer;
    private bool cooldownActive = false;

    [Header("Shrink/Grow settings")]
    [SerializeField] private float transitionTime = 1f; // seconds to shrink or grow

    // Store original scales
    private List<Vector3> originalScales = new List<Vector3>();

    void Awake()
    {
        // Save original scales for all objects
        foreach (var obj in objectList)
        {
            if (obj != null)
                originalScales.Add(obj.transform.localScale);
            else
                originalScales.Add(Vector3.one);
        }

        // Configure objects
        int i = 0;
        foreach (GameObject obj in objectList)
        {
            if (i == 0) obj.SetActive(true);
            else obj.SetActive(false);
            i++;
        }

        activeObject = 0;
    }


    void Update()
    {
        if (cooldownActive)
        {
            if (coolDown < timer){
                cooldownActive = false;
            }
            else {
                coolDown -= Time.deltaTime;
            }
        }
    }
    public void ChangeSceneObject(int index)
    {
        if (cooldownActive == false){
            cooldownActive = true;
        } else {
            return;
        }

        if (index < 0 || index >= objectList.Count || index == activeObject)
            return;

        // Stop any existing transitions
        StopAllCoroutines();
        StartCoroutine(SwapObjectsRoutine(activeObject, index));
    }

    private IEnumerator SwapObjectsRoutine(int fromIndex, int toIndex)
    {
        // Shrink old object
        yield return StartCoroutine(ScaleOverTime(objectList[fromIndex], Vector3.zero, transitionTime));
        objectList[fromIndex].SetActive(false);

        // Grow new object
        objectList[toIndex].SetActive(true);
        objectList[toIndex].transform.localScale = Vector3.zero;
        yield return StartCoroutine(ScaleOverTime(objectList[toIndex], originalScales[toIndex], transitionTime));

        activeObject = toIndex;
    }

    private IEnumerator ScaleOverTime(GameObject target, Vector3 targetScale, float duration)
    {
        if (target == null) yield break;

        Vector3 startScale = target.transform.localScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            target.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        target.transform.localScale = targetScale;
    }
}
