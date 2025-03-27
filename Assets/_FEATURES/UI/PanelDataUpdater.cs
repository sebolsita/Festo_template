using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class PanelDataUpdater : MonoBehaviour
{
    [Header("TMP References")]
    [SerializeField] private TMP_Text titleTMP;
    [SerializeField] private TMP_Text updateModeTMP;
    [SerializeField] private TMP_Text machineInfoTMP;
    [SerializeField] private TMP_Text orderInfoTMP;
    [SerializeField] private TMP_Text orderStatusTMP;
    [SerializeField] private TMP_Text partNumberValueTMP;
    [SerializeField] private TMP_Text quantityValueTMP;

    [Header("UI Controls")]
    [SerializeField] private Slider refreshSlider;
    [SerializeField] private Slider partNumberSlider;
    [SerializeField] private Slider quantitySlider;

    private int machineID = -1;
    private string lastNodeData = "";
    private float refreshTimer = 0f;
    private string lastTimestamp = "";

    private void Start()
    {
        AssignMachineIDFromName();

        partNumberSlider.onValueChanged.AddListener(OnPartSliderChanged);
        quantitySlider.onValueChanged.AddListener(OnQtySliderChanged);

        OnPartSliderChanged(partNumberSlider.value);
        OnQtySliderChanged(quantitySlider.value);

        StartCoroutine(RefreshLoop());
    }

    private void AssignMachineIDFromName()
    {
        if (gameObject.name.StartsWith("Panel_") && int.TryParse(gameObject.name.Replace("Panel_", ""), out int id))
        {
            machineID = id;
            string machineName = MachineDataManager.Instance.GetMachineName(machineID);
            titleTMP.text = $"{machineName} | ID: {machineID}";
        }
        else
        {
            titleTMP.text = "UNKNOWN MACHINE";
        }
    }

    private IEnumerator RefreshLoop()
    {
        while (true)
        {
            int refreshMode = (int)refreshSlider.value;
            float delay = Mathf.Max(refreshMode - 1, 0);

            if (refreshMode == 0)
            {
                updateModeTMP.text = "DATA UPDATE: OFF";
            }
            else if (refreshMode == 1)
            {
                updateModeTMP.text = "DATA UPDATE: AUTO";
                UpdateIfChanged();
            }
            else
            {
                if (refreshTimer <= 0)
                {
                    UpdatePanel();
                    refreshTimer = delay;
                }

                updateModeTMP.text = $"DATA UPDATE: TIMED {delay}s (update in: {Mathf.CeilToInt(refreshTimer)}s)";
                refreshTimer -= Time.deltaTime;
            }

            yield return null;
        }
    }

    private void UpdateIfChanged()
    {
        NodeReader reader = MachineDataManager.Instance.GetReaderForMachine(machineID);
        if (reader == null) return;

        string currentData = reader.dataFromOPCUANode;
        if (currentData != lastNodeData)
        {
            lastNodeData = currentData;
            UpdatePanel();
        }
    }

    private void UpdatePanel()
    {
        NodeReader reader = MachineDataManager.Instance.GetReaderForMachine(machineID);
        if (reader == null)
        {
            machineInfoTMP.text = "Machine data unavailable.";
            orderInfoTMP.text = "NO ORDER";
            return;
        }

        string nodeData = string.IsNullOrEmpty(reader.dataFromOPCUANode) ? "(no data)" : reader.dataFromOPCUANode;
        string nodeID = reader.nodeID;
        bool changed = (nodeData != lastNodeData);

        if (changed)
        {
            lastTimestamp = System.DateTime.Now.ToString("dd/MM/yy HH:mm:ss");
        }

        lastNodeData = nodeData;

        machineInfoTMP.text =
            $"Data Change Detected : {(changed ? "Yes" : "No")}\n" +
            $"Last Change          : {lastTimestamp}\n" +
            $"Node ID              : <i>{nodeID}</i>\n" +
            $"Node Data            : {nodeData}";

        CurrentOrderJSON order = MachineDataManager.Instance.GetOrderForRFID(nodeData);

        if (order != null)
        {
            orderInfoTMP.text =
                $"Order Number     : {order.ONo}\n" +
                $"Company          : {order.Company}\n" +
                $"Planned Start    : {order.PlannedStart}\n" +
                $"Planned End      : {order.PlannedEnd}\n" +
                $"State            : {order.State}\n" +
                $"Part Number      : {order.PartNumber}\n" +
                $"Carrier ID       : {order.CarrierID}";
        }
        else
        {
            orderInfoTMP.text =
                "Order Number     : NO ORDER\n" +
                "Company          : n/a\n" +
                "Planned Start    : n/a\n" +
                "Planned End      : n/a\n" +
                "State            : n/a\n" +
                "Part Number      : n/a\n" +
                "Carrier ID       : n/a";
        }
    }

    private void OnPartSliderChanged(float value)
    {
        partNumberValueTMP.text = $"Part No. {Mathf.RoundToInt(value)}";
    }

    private void OnQtySliderChanged(float value)
    {
        quantityValueTMP.text = $"Quantity: {Mathf.RoundToInt(value)}";
    }

    // Hook this manually to the button via Inspector
    public void SendOrder()
    {
        // Placeholder message
        string errorMsg = "<color=red><i>Order system not connected</i></color>";
        orderStatusTMP.text = $"NEW ORDER STATUS: {errorMsg}";
    }
}
