using UnityEngine;
using UnityEngine.UI;

public class MenuCreator : MonoBehaviour
{

    public Canvas menu;

    private Slider LRslider;
    private Slider DFslider;
    private Slider TDslider;
    private Slider Eslider;
    private Slider Tslider;
    private Toggle Ptoggle;
    

    void Awake()
    {
        // Find sliders
        LRslider = GameObject.Find("AlphaSlider").GetComponent<Slider>();
        DFslider = GameObject.Find("GammaSlider").GetComponent<Slider>();
        TDslider = GameObject.Find("LambdaSlider").GetComponent<Slider>();
        Eslider = GameObject.Find("EpsylonSlider").GetComponent<Slider>();
        Tslider = GameObject.Find("TemperatureSlider").GetComponent<Slider>();
        Ptoggle = GameObject.Find("PolicyIndicationToggle").GetComponent<Toggle>();

        // Deactivate all components
        LRslider.gameObject.SetActive(false);
        DFslider.gameObject.SetActive(false);
        TDslider.gameObject.SetActive(false);
        Eslider.gameObject.SetActive(false);
        Tslider.gameObject.SetActive(false);
        Ptoggle.gameObject.SetActive(false);

        Debug.Log("MenuCreator started");
    }

    public void InitializeMenu(string[] sliders)
    {
        Debug.Log("Initializing menu");

        //The y position on the menu
        float position = 180;

        foreach (string slider in sliders)
        {
            switch (slider)
            {
                case "LRslider":
                    LRslider.gameObject.SetActive(true);
                    LRslider.gameObject.transform.localPosition = new Vector3(LRslider.gameObject.transform.localPosition.x, position, LRslider.gameObject.transform.localPosition.z);
                    position -= 50;
                    break;
                case "DFslider":
                    DFslider.gameObject.SetActive(true);
                    DFslider.gameObject.transform.localPosition = new Vector3(DFslider.gameObject.transform.localPosition.x, position, DFslider.gameObject.transform.localPosition.z);
                    position -= 50;
                    break;
                case "TDslider":
                    TDslider.gameObject.SetActive(true);
                    TDslider.gameObject.transform.localPosition = new Vector3(TDslider.gameObject.transform.localPosition.x, position, TDslider.gameObject.transform.localPosition.z);
                    position -= 50;
                    break;
                case "Eslider":
                    Eslider.gameObject.SetActive(true);
                    Eslider.gameObject.transform.localPosition = new Vector3(Eslider.gameObject.transform.localPosition.x, position, Eslider.gameObject.transform.localPosition.z);
                    position -= 50;
                    break;
                case "Tslider":
                    Tslider.gameObject.SetActive(true);
                    Tslider.gameObject.transform.localPosition = new Vector3(Tslider.gameObject.transform.localPosition.x, position, Tslider.gameObject.transform.localPosition.z);
                    position -= 50;
                    break;
                case "Ptoggle":
                    Ptoggle.gameObject.SetActive(true);
                    Ptoggle.gameObject.transform.localPosition = new Vector3(Ptoggle.gameObject.transform.localPosition.x, position, Ptoggle.gameObject.transform.localPosition.z);
                    position -= 50;
                    break;

            }

        }

    }

    
}

