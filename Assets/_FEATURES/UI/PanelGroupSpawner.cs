using UnityEngine;
using Meta.XR.MRUtilityKit;
using System.Collections;

public class PanelGroupSpawner : MonoBehaviour
{
    [Header("Panel Group Prefab")]
    [SerializeField] private GameObject panelGroupPrefab;

    [Header("Offset from wall surface (local space, meters)")]
    [SerializeField] private Vector3 localOffset = new Vector3(0f, 1.8f, 0.5f); // X = right, Y = up, Z = forward

    [Header("Rotation Offset (degrees)")]
    [SerializeField] private Vector3 rotationOffset = Vector3.zero;

    private void Start()
    {
        StartCoroutine(SpawnWhenReady());
    }

    private IEnumerator SpawnWhenReady()
    {
        while (MRUK.Instance == null || MRUK.Instance.GetCurrentRoom() == null)
        {
            Debug.Log("[PanelGroupSpawner] Waiting for MRUK room...");
            yield return null;
        }

        SpawnAtKeyWall();
    }

    private void SpawnAtKeyWall()
    {
        MRUKAnchor keyWall = MRUK.Instance.GetCurrentRoom().GetKeyWall(out Vector2 wallScale);

        if (keyWall == null)
        {
            Debug.LogWarning("[PanelGroupSpawner] No key wall found.");
            return;
        }

        Transform wall = keyWall.transform;

        // Offset from center of wall, adjusted by its actual scale
        Vector3 spawnPosition =
            wall.position
            + wall.right * (Mathf.Sign(localOffset.x) * (wall.localScale.x / 2 + Mathf.Abs(localOffset.x)))
            + wall.up * (Mathf.Sign(localOffset.y) * (wall.localScale.y / 2 + Mathf.Abs(localOffset.y)))
            + wall.forward * (Mathf.Sign(localOffset.z) * (wall.localScale.z / 2 + Mathf.Abs(localOffset.z)));

        // Face away from wall, plus rotation offset
        Quaternion baseRotation = Quaternion.LookRotation(-wall.forward, wall.up);
        Quaternion finalRotation = baseRotation * Quaternion.Euler(rotationOffset);

        Instantiate(panelGroupPrefab, spawnPosition, finalRotation);
        Debug.Log($"[PanelGroupSpawner] Group spawned at {spawnPosition} with rotation {finalRotation.eulerAngles}");
    }
}
