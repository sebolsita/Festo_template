using System.Collections.Generic;
using UnityEngine;
using realvirtual;

public class MachineDataManager : MonoBehaviour
{
    public static MachineDataManager Instance { get; private set; }

    [Header("Machine Readers")]
    public List<NodeReader> machineReaders = new List<NodeReader>();

    [Header("MES Order Data")]
    public CurrentOrders currentOrders;

    private Dictionary<int, string> machineNames = new Dictionary<int, string>()
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

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    public string GetMachineName(int machineID)
    {
        return machineNames.TryGetValue(machineID, out var name) ? name : "Unknown";
    }

    public NodeReader GetReaderForMachine(int machineID)
    {
        return machineReaders.Find(r => r.factoryMachineID == machineID);
    }

    public CurrentOrderJSON GetOrderForRFID(string rfid)
    {
        if (string.IsNullOrEmpty(rfid) || currentOrders == null || currentOrders.currentOrdersObjectArray == null)
            return null;

        foreach (var order in currentOrders.currentOrdersObjectArray)
        {
            if (order.CarrierID == rfid)
                return order;
        }

        return null;
    }
}
