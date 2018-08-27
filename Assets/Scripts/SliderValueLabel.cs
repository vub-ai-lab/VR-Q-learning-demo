using UnityEngine;
using UnityEngine.UI;

public class SliderValueLabel : MonoBehaviour
{
    public Slider m_Slider;
    public Text m_Text;

    void OnEnable()
    {

        m_Slider.onValueChanged.AddListener(delegate {
            SliderValueChanged();
        });

        //Initialise the Text to say the first value of the Slider
        m_Text.text = m_Slider.value.ToString("n2");
    }

    private void OnDisable()
    {
        m_Slider.onValueChanged.RemoveAllListeners();
    }

    //Output the new state of the Toggle into Text
    void SliderValueChanged()
    {
        float value = m_Slider.value;
        m_Text.text = value.ToString("n2");
    }
}