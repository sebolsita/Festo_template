using UnityEngine;
using Meta.XR.MRUtilityKit;
using System.Collections;
using System.Collections.Generic;

public class SpawnUIPanelsOnMachines : MonoBehaviour
{
    [Header("UI Panel Prefab")]
    [SerializeField] private GameObject panelPrefab;

    [Header("Panel Spawn Offset")]
    [SerializeField] private Vector3 defaultOffset = new Vector3(0.5f, 0.3f, 0f); // Offset from anchor

    private void Start()
    {
        StartCoroutine(WaitForRoomAndSpawn());
    }

    private IEnumerator WaitForRoomAndSpawn()
    {
        while (MRUK.Instance == null || MRUK.Instance.GetCurrentRoom() == null)
        {
            Debug.Log("[SpawnUIPanelsOnMachines] Waiting for MRUK room to load...");
            yield return null;
        }

        SpawnPanels();
    }

    private void SpawnPanels()
    {
        MRUKRoom currentRoom = MRUK.Instance.GetCurrentRoom();
        if (currentRoom == null)
        {
            Debug.LogError("[SpawnUIPanelsOnMachines] No MRUK room detected.");
            return;
        }

        List<MRUKAnchor> allAnchors = currentRoom.Anchors;

        if (allAnchors.Count == 0)
        {
            Debug.LogError("[SpawnUIPanelsOnMachines] No anchors found in the scanned space.");
            return;
        }

        int panelIndex = 1;  // Start the suffix from 1

        foreach (MRUKAnchor anchor in allAnchors)
        {
            if (!anchor.Label.ToString().ToLower().Contains("other"))
                continue;

            Vector3 anchorPos = anchor.transform.position;

            // Calculate the directional offset based on panelIndex
            Vector3 worldOffset = CalculateDirectionalOffset(anchorPos, panelIndex);
            Vector3 spawnPos = anchorPos + worldOffset;

            // Instantiate the panel
            GameObject panel = Instantiate(panelPrefab, spawnPos, Quaternion.identity);

            // Set a unique name with a suffix (e.g., Panel_1, Panel_2, etc.)
            panel.name = panelPrefab.name + "_" + panelIndex++;

            // Make the panel face the camera
            if (Camera.main != null)
            {
                panel.transform.LookAt(Camera.main.transform);
                panel.transform.Rotate(0, 180f, 0); // Flip to face the user
            }

            Debug.Log("[SpawnUIPanelsOnMachines] Panel spawned at: " + spawnPos + " with name: " + panel.name);
        }
    }

    // Adjust offset based on panelIndex
    private Vector3 CalculateDirectionalOffset(Vector3 anchorPos, int panelIndex)
    {
        // Customize the offset logic based on the panel index
        if (panelIndex == 1)
            return Vector3.left * 0.4f + Vector3.up * 0.3f;  // Example: first panel, offset left and up
        if (panelIndex == 2)
            return Vector3.right * 0.4f + Vector3.up * 0.3f; // Example: second panel, offset right and up
        if (panelIndex == 3)
            return Vector3.forward * 0.4f + Vector3.up * 0.3f; // Example: third panel, offset forward
                                                               // Adjust as needed for more panels

        // Default offset
        return Vector3.up * 0.5f;  // Default offset: just move it up
    }

}
