using System.Collections.Generic;
using UnityEngine;
using realvirtual;

public class DetailedMachineConsoleLogger : MonoBehaviour
{
    [Header("Machine Readers")]
    [SerializeField] private List<NodeReader> machineReaders = new List<NodeReader>();

    [Header("MES Order Data")]
    [SerializeField] private CurrentOrders currentOrders;

    private Dictionary<NodeReader, string> previousData = new Dictionary<NodeReader, string>();
    private int logCount = 1;

    // Machine ID to name mapping
    private readonly Dictionary<int, string> machineNames = new Dictionary<int, string>()
    {
        { 1, "Factory" },
        { 2, "RobotArm" },
        { 3, "CameraStation" },
        { 4, "Branch" },
        { 5, "FrontMagazine" },
        { 6, "Measuring" },
        { 7, "Drilling" },
        { 8, "BackMagazine" },
        { 9, "Pressing" }
    };

    private void Start()
    {
        if (machineReaders.Count == 0)
        {
            Debug.LogError("[DetailedMachineConsoleLogger] No NodeReader components assigned.");
            return;
        }

        foreach (var reader in machineReaders)
        {
            if (reader == null)
            {
                Debug.LogError("[DetailedMachineConsoleLogger] One or more NodeReader references are missing.");
                continue;
            }

            reader.oPCUAinterface.EventOnConnected.AddListener(() => OnConnected(reader));
            reader.oPCUAinterface.EventOnDisconnected.AddListener(() => OnDisconnected(reader));
            previousData[reader] = "";
        }

        InvokeRepeating(nameof(LogDetailedMachineData), 2f, 5f);
    }

    private void OnConnected(NodeReader reader)
    {
        Debug.Log($"[DetailedMachineConsoleLogger] Connected to {reader.gameObject.name}.");
    }

    private void OnDisconnected(NodeReader reader)
    {
        Debug.LogWarning($"[DetailedMachineConsoleLogger] Disconnected from {reader.gameObject.name}.");
    }

    private void LogDetailedMachineData()
    {
        string fullLog = $"\n==================== LOG: {logCount++} ====================\n";

        foreach (var reader in machineReaders)
        {
            if (reader == null || reader.oPCUAinterface == null)
                continue;

            int machineID = reader.factoryMachineID;
            string machineName = machineNames.TryGetValue(machineID, out var name) ? name : "Unknown";
            string nodeID = reader.nodeID;
            string rawData = reader.dataFromOPCUANode;
            string cleanedNodeData = string.IsNullOrEmpty(rawData) ? "(no data)" : rawData.Replace("realvirtual.OPCUANodeSubscription", "").Trim();

            bool hasChanged = previousData[reader] != cleanedNodeData;
            previousData[reader] = cleanedNodeData;

            CurrentOrderJSON matchingOrder = null;
            if (currentOrders.currentOrdersObjectArray != null)
            {
                foreach (var order in currentOrders.currentOrdersObjectArray)
                {
                    if (order.CarrierID == cleanedNodeData)
                    {
                        matchingOrder = order;
                        break;
                    }
                }
            }

            string machineHeader = $"-------------------- MACHINE: {machineName} --------------------";
            string machineFooter = new string('-', machineHeader.Length);

            string machineBlock = $"{machineHeader}\n" +
                                  $"Machine ID        : {machineID}\n" +
                                  $"Node ID           : {nodeID}\n" +
                                  $"Node Data         : {cleanedNodeData}\n" +
                                  $"Change Detected   : {(hasChanged ? "Yes" : "No")}";

            if (matchingOrder != null)
            {
                machineBlock += "\n\nMATCHING ORDER FOUND\n" +
                                $"Order Number      : {matchingOrder.ONo}\n" +
                                $"Company           : {matchingOrder.Company}\n" +
                                $"Planned Start     : {matchingOrder.PlannedStart}\n" +
                                $"Planned End       : {matchingOrder.PlannedEnd}\n" +
                                $"State             : {matchingOrder.State}\n" +
                                $"Part Number       : {matchingOrder.PartNumber}\n" +
                                $"Carrier ID        : {matchingOrder.CarrierID}";
            }
            else if (cleanedNodeData != "(no data)")
            {
                machineBlock += $"\n\nNo MES data found for RFID: {cleanedNodeData}";
            }

            machineBlock += $"\n{machineFooter}\n";
            fullLog += machineBlock;
        }

        Debug.Log(fullLog);
    }
}
