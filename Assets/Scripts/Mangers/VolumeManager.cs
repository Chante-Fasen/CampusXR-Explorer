using UnityEngine;
using UnityEngine.UI;
public class VolumeManager : MonoBehaviour
{
    [SerializeField] Slider volumeSlider; 

    public void changeVolume()
    {
        AudioListener.volume = volumeSlider.value;
    }
}
