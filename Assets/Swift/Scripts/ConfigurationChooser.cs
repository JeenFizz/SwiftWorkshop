using UnityEngine;
using UnityEngine.UI;

public class ConfigurationChooser : MonoBehaviour
{
    public Image screenshot;
    public Text date;
    public bool done = false;

    private ControllerInput.JsonConfig jsonConfig;
    private string path;
    private string configPath;
    private MachineLayoutSaver machineLayoutSaver;

    void Start()
    {
        path = Application.dataPath + "/StreamingAssets/";
        configPath = path + "MachineSaves/";
        machineLayoutSaver = GameObject.FindWithTag("FactoryMap").GetComponent<MachineLayoutSaver>();
    }

    public void SetJsonConfig(ControllerInput.JsonConfig jsonConfig)
    {
        this.jsonConfig = jsonConfig;
        if (jsonConfig.screenshotName != null)
            screenshot.sprite = Resources.Load<Sprite>(configPath + jsonConfig.screenshotName);
        date.text = jsonConfig.name;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("InteractionTool"))
        {
            machineLayoutSaver.LoadFile(configPath + jsonConfig.fileName);
            done = true;
        }
    }
}
