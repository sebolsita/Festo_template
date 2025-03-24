using UnityEngine;
using Meta.XR.MRUtilityKit;
using System.Collections;
using System.Collections.Generic;

public class SpawnNumberedCubesOnAnchors : MonoBehaviour
{
    [Header("Cube Prefab")]
    [SerializeField] private GameObject cubePrefab;

    private void Start()
    {
        StartCoroutine(WaitForRoomAndSpawn());
    }

    private IEnumerator WaitForRoomAndSpawn()
    {
        // Wait for MRUK to load the scene
        while (MRUK.Instance == null || MRUK.Instance.GetCurrentRoom() == null)
        {
            Debug.Log("[SpawnNumberedCubesOnAnchors] Waiting for MRUK room to load...");
            yield return null;
        }

        SpawnCubes();
    }

    private void SpawnCubes()
    {
        MRUKRoom currentRoom = MRUK.Instance.GetCurrentRoom();
        if (currentRoom == null)
        {
            Debug.LogError("[SpawnNumberedCubesOnAnchors] No MRUK room detected.");
            return;
        }

        List<MRUKAnchor> allAnchors = currentRoom.Anchors;
        if (allAnchors.Count == 0)
        {
            Debug.LogError("[SpawnNumberedCubesOnAnchors] No anchors found in the scanned space.");
            return;
        }

        int index = 1; // Start the numbering from 1

        foreach (MRUKAnchor anchor in allAnchors)
        {
            if (!anchor.Label.ToString().ToLower().Contains("other"))
                continue;

            // Spawn a cube at the anchor's position
            Vector3 spawnPos = anchor.transform.position;
            GameObject cube = Instantiate(cubePrefab, spawnPos, Quaternion.identity);

            // Assign the unique number to the cube
            cube.name = "MachineCube_" + index;
            // Optionally, we can add a label to the cube's UI for identification purposes
            cube.GetComponentInChildren<TextMesh>().text = index.ToString();

            Debug.Log($"[SpawnNumberedCubesOnAnchors] Cube spawned at: {spawnPos}, with number: {index}");

            index++;
        }
    }
}
