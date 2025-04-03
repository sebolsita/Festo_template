using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    [SerializeField] private GameObject passthroughObject; // assign your PassthroughBuildingBlock
    [SerializeField] private string arSceneName = "ARScene";
    [SerializeField] private string vrSceneName = "DigitalTwinScene";

    public void SwitchScene()
    {
        string targetScene;

        if (passthroughObject != null && passthroughObject.activeInHierarchy)
        {
            // You're in AR go to VR
            targetScene = vrSceneName;
        }
        else
        {
            // You're in VR go to AR
            targetScene = arSceneName;
        }

        Debug.Log($"[SceneSwitcher] Switching to: {targetScene}");
        SceneManager.LoadScene(targetScene);
    }
}
