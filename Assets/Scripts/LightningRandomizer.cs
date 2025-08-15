using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningRandomizer : MonoBehaviour
{
    [Header("The positions that outline the path of the lightning")]
    [SerializeField] private Transform[] nodes;

    [SerializeField] private float boltHeight = 5f;
    [SerializeField] private float boltRadius = 0.5f;

    void Awake()
    {
        if (nodes == null || nodes.Length == 0)
        {
            Debug.LogWarning("No nodes assigned to LightningRandomizer.");
            return;
        }

        Vector3 startPos = transform.position;

        // Distance in Y between each node
        float interval = boltHeight / (nodes.Length - 1);

        for (int i = 0; i < nodes.Length; i++)
        {
            // Even Y position
            float y = startPos.y + (interval * i);

            // Random XZ offset inside a circle
            Vector2 randomXZ = Random.insideUnitCircle * boltRadius;

            nodes[i].position = new Vector3(
                startPos.x + randomXZ.x,
                y,
                startPos.z + randomXZ.y
            );
        }
    }
}
