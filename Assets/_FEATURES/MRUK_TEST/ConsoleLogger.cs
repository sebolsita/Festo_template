using UnityEngine;
using System.Collections.Generic;
using realvirtual; // Ensure this namespace is correct

/// <summary>
/// Test script to log OPC UA data from machines in the console.
/// </summary>
public class MachineConsoleLogger : MonoBehaviour
{
    #region Public Properties
    [Header("Machine Readers")]
    [SerializeField] private List<NodeReader> machineReaders = new List<NodeReader>(); // List of NodeReader (machines)
    #endregion

    #region Unity Methods
    private void Start()
    {
        if (machineReaders.Count == 0)
        {
            Debug.LogError("[MachineConsoleLogger] No NodeReader components assigned.");
            return;
        }

        foreach (var reader in machineReaders)
        {
            if (reader == null)
            {
                Debug.LogError("[MachineConsoleLogger] One or more NodeReader references are missing.");
                continue;
            }

            // Add listeners for connection events
            reader.oPCUAinterface.EventOnConnected.AddListener(() => OnConnected(reader));
            reader.oPCUAinterface.EventOnDisconnected.AddListener(() => OnDisconnected(reader));
        }

        // Run data check every 5 seconds
        InvokeRepeating(nameof(LogMachineData), 2f, 5f);
    }
    #endregion

    #region Event Handlers
    /// <summary>
    /// Called when an OPC UA interface connects successfully.
    /// </summary>
    private void OnConnected(NodeReader reader)
    {
        Debug.Log($"[MachineConsoleLogger] Connected to {reader.gameObject.name}.");
    }

    /// <summary>
    /// Called when an OPC UA interface disconnects.
    /// </summary>
    private void OnDisconnected(NodeReader reader)
    {
        Debug.LogWarning($"[MachineConsoleLogger] Disconnected from {reader.gameObject.name}.");
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Logs machine data to the console for testing purposes.
    /// </summary>
    private void LogMachineData()
    {
        foreach (var reader in machineReaders)
        {
            if (reader == null || reader.oPCUAinterface == null)
                continue;

            // Output the data from the OPC UA node to the console
            string machineName = reader.gameObject.name;
            string nodeData = reader.dataFromOPCUANode;
            Debug.Log($"[MachineConsoleLogger] {machineName} - Node Data: {nodeData}");
        }
    }
    #endregion
}
