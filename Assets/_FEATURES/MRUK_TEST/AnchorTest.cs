using UnityEngine;
using Meta.XR.MRUtilityKit;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Spawns cubes on all objects tagged with "Other" in the MRUK-scanned space.
/// </summary>
public class SpawnCubesOnMachines : MonoBehaviour
{
    #region Public Properties

    [Header("Cube Prefab")]
    [SerializeField] private GameObject cubePrefab;

    #endregion

    #region Unity Methods

    private void Start()
    {
        StartCoroutine(WaitForRoomAndSpawn());
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Waits for MRUK to load the scene, then spawns cubes on objects labeled "Other".
    /// </summary>
    private IEnumerator WaitForRoomAndSpawn()
    {
        while (MRUK.Instance == null || MRUK.Instance.GetCurrentRoom() == null)
        {
            Debug.Log("[SpawnCubesOnMachines] Waiting for MRUK room to load...");
            yield return null;
        }

        SpawnCubes();
    }

    /// <summary>
    /// Spawns a cube at each object tagged as "Other".
    /// </summary>
    private void SpawnCubes()
    {
        MRUKRoom currentRoom = MRUK.Instance.GetCurrentRoom();
        if (currentRoom == null)
        {
            Debug.LogError("[SpawnCubesOnMachines] No MRUK room detected.");
            return;
        }

        // Get all anchors in the current room
        List<MRUKAnchor> allAnchors = currentRoom.Anchors;

        if (allAnchors.Count == 0)
        {
            Debug.LogError("[SpawnCubesOnMachines] No anchors found in the scanned space.");
            return;
        }

        foreach (MRUKAnchor anchor in allAnchors)
        {
            // Check if the anchor name contains "Other" (case-insensitive)
            if (anchor.Label.ToString().ToLower().Contains("other"))
            {
                Vector3 spawnPosition = anchor.transform.position;
                Instantiate(cubePrefab, spawnPosition, Quaternion.identity);
                Debug.Log($"[SpawnCubesOnMachines] Cube spawned at: {spawnPosition}");
            }
        }
    }

    #endregion
}

