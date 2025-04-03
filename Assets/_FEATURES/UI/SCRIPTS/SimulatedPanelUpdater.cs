using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class SimulatedPanelUpdater : MonoBehaviour
{
    [Header("TMP References")]
    [SerializeField] private TMP_Text titleTMP;
    [SerializeField] private TMP_Text updateModeTMP;
    [SerializeField] private TMP_Text machineInfoTMP;
    [SerializeField] private TMP_Text orderInfoTMP;

    [Header("UI Controls")]
    [SerializeField] private Slider refreshSlider;

    private int machineID = -1;
    private string lastRFID = "";
    private float refreshTimer = 0f;
    private string lastTimestamp = "";

    private readonly Dictionary<int, string> machineNames = new()
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

    private string[] fakeRFIDs = { "6", "2", "8", "5", "1" };
    private string[] fakeCompanies = { "Festo AG", "Siemens", "ABB", "Bosch", "Schneider" };
    private string[] fakeStates = { "In Progress", "Waiting", "Completed", "Paused" };
    private string[] fakeParts = { "210", "303", "420", "111", "999" };

    private void Start()
    {
        AssignMachineIDFromName();
        UpdateSimulatedPanel();
        StartCoroutine(UpdateLoop());
    }

    private void AssignMachineIDFromName()
    {
        if (gameObject.name.StartsWith("Panel_") && int.TryParse(gameObject.name.Replace("Panel_", ""), out int id))
        {
            machineID = id;
            if (machineNames.TryGetValue(id, out string name))
            {
                titleTMP.text = $"{name} | ID: {id}";
            }
            else
            {
                titleTMP.text = $"Unknown | ID: {id}";
            }
        }
        else
        {
            titleTMP.text = "UNKNOWN MACHINE";
        }
    }

    private IEnumerator UpdateLoop()
    {
        while (true)
        {
            int mode = (int)refreshSlider.value;
            float delay = Mathf.Max(mode - 1, 0);

            if (mode == 0)
            {
                updateModeTMP.text = "DATA UPDATE: OFF";
            }
            else if (mode == 1)
            {
                updateModeTMP.text = "DATA UPDATE: AUTO";
                SimulateIfChanged();
            }
            else
            {
                if (refreshTimer <= 0f)
                {
                    UpdateSimulatedPanel();
                    refreshTimer = delay;
                }

                updateModeTMP.text = $"DATA UPDATE: TIMED {delay}s (update in: {Mathf.CeilToInt(refreshTimer)}s)";
                refreshTimer -= Time.deltaTime;
            }

            yield return null;
        }
    }

    private void SimulateIfChanged()
    {
        string newRFID = GetRandom(fakeRFIDs);
        if (newRFID != lastRFID)
        {
            lastRFID = newRFID;
            UpdateSimulatedPanel();
        }
    }

    private void UpdateSimulatedPanel()
    {
        string rfid = GetRandom(fakeRFIDs);
        string company = GetRandom(fakeCompanies);
        string state = GetRandom(fakeStates);
        string part = GetRandom(fakeParts);
        string timestamp = System.DateTime.Now.ToString("dd/MM/yy HH:mm:ss");

        lastRFID = rfid;
        lastTimestamp = timestamp;

        machineInfoTMP.text =
            $"Data Change Detected : Yes\n" +
            $"Last Change          : {timestamp}\n" +
            $"Node ID              : <i>ns=3;s=\"dbRfidData\".\"ID1\".\"iCarrierID\"</i>\n" +
            $"Node Data            : {rfid}";

        orderInfoTMP.text =
            $"Order Number     : {Random.Range(1000, 9999)}\n" +
            $"Company          : {company}\n" +
            $"Planned Start    : 10:00\n" +
            $"Planned End      : 11:30\n" +
            $"State            : {state}\n" +
            $"Part Number      : {part}\n" +
            $"Carrier ID       : {rfid}";
    }

    private string GetRandom(string[] list) => list[Random.Range(0, list.Length)];
}
