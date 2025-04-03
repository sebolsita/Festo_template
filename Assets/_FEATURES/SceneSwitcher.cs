using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string arSceneName = "MX_MRUK";
    [SerializeField] private string vrSceneName = "VR_MRUK";

    [Header("Passthrough Root Object (optional)")]
    [SerializeField] private GameObject passthroughObject;

    /// <summary>
    /// Called from a UI Button. Switches between AR and Digital Twin scenes
    /// based on whether passthrough is active.
    /// </summary>
    public void SwitchScene()
    {
        string targetScene;

        if (passthroughObject != null && passthroughObject.activeInHierarchy)
        {
            // Currently in AR scene switch to VR
            targetScene = vrSceneName;
        }
        else
        {
            // Currently in VR scene witch to AR
            targetScene = arSceneName;
        }

        Debug.Log($"[SceneSwitcher] Switching to: {targetScene}");
        SceneManager.LoadScene(targetScene);
    }
}
