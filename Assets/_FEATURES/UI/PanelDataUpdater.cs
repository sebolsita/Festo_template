using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PanelDataUpdater : MonoBehaviour
{
    [System.Serializable]
    public class PanelMachineMapping
    {
        public string panelName;
        public int machineID;
    }

    [Header("Text Fields")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text detailsText;
    [SerializeField] private TMP_Text modeLabel;

    [Header("Manual Mapping")]
    [SerializeField] private List<PanelMachineMapping> panelMappings = new List<PanelMachineMapping>();

    [Header("Optional Refresh Slider")]
    [SerializeField] private Slider refreshSlider;

    private int machineID = -1;
    private string lastNodeData = "";
    private float timer = 0f;

    private void Start()
    {
        AssignMachineIDFromName();

        if (refreshSlider != null)
        {
            refreshSlider.value = 6; // TIMED: 5s
        }

        StartCoroutine(RefreshRoutine());
    }

    private void AssignMachineIDFromName()
    {
        string myName = gameObject.name;

        foreach (var map in panelMappings)
        {
            if (map.panelName == myName)
            {
                machineID = map.machineID;
                return;
            }
        }

        machineID = -1;
        titleText.text = "Unknown Machine";
        detailsText.text = "No mapping found for this panel.";
    }

    private IEnumerator RefreshRoutine()
    {
        while (true)
        {
            float refreshMode = refreshSlider != null ? refreshSlider.value : 6f;
            float delay = Mathf.Max(refreshMode - 1f, 0f);

            switch ((int)refreshMode)
            {
                case 0: // NO UPDATES
                    modeLabel.text = "NO UPDATES";
                    break;

                case 1: // AUTO REFRESH
                    modeLabel.text = "AUTO REFRESH";
                    UpdateIfChanged();
                    break;

                default: // TIMED
                    modeLabel.text = "TIMED: " + (int)delay + "s";
                    timer += Time.deltaTime;
                    if (timer >= delay)
                    {
                        UpdateText();
                        timer = 0f;
                    }
                    else
                    {
                        int secondsLeft = Mathf.CeilToInt(delay - timer);
                        modeLabel.text = "TIMED: " + secondsLeft + "s";
                    }
                    break;
            }

            yield return null;
        }
    }

    private void UpdateIfChanged()
    {
        if (machineID == -1 || MachineDataManager.Instance == null)
            return;

        NodeReader reader = MachineDataManager.Instance.GetReaderForMachine(machineID);
        if (reader == null)
            return;

        string currentData = reader.dataFromOPCUANode;
        if (currentData != lastNodeData)
        {
            lastNodeData = currentData;
            UpdateText();
        }
    }

    private void UpdateText()
    {
        if (titleText == null || detailsText == null || machineID == -1 || MachineDataManager.Instance == null)
            return;

        string machineName = MachineDataManager.Instance.GetMachineName(machineID);
        titleText.text = machineName;

        NodeReader reader = MachineDataManager.Instance.GetReaderForMachine(machineID);
        if (reader == null)
        {
            detailsText.text = "Machine data not available.";
            return;
        }

        string nodeID = reader.nodeID;
        string nodeData = string.IsNullOrEmpty(reader.dataFromOPCUANode) ? "(no data)" : reader.dataFromOPCUANode.Trim();
        lastNodeData = nodeData;

        CurrentOrderJSON order = MachineDataManager.Instance.GetOrderForRFID(nodeData);

        string orderDetails = order != null
            ? $"Order Number      : {order.ONo}\n" +
              $"Company           : {order.Company}\n" +
              $"Planned Start     : {order.PlannedStart}\n" +
              $"Planned End       : {order.PlannedEnd}\n" +
              $"State             : {order.State}\n" +
              $"Part Number       : {order.PartNumber}\n" +
              $"Carrier ID        : {order.CarrierID}"
            : "Order Number      : NO ORDER ASSIGNED\n" +
              "Company           : n/a\n" +
              "Planned Start     : n/a\n" +
              "Planned End       : n/a\n" +
              "State             : n/a\n" +
              "Part Number       : n/a\n" +
              "Carrier ID        : n/a";

        detailsText.text = $"Machine ID        : {machineID}\n" +
                           $"Node ID           : {nodeID}\n" +
                           $"Node Data         : {nodeData}\n\n" +
                           orderDetails;
    }
}
