using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SliderController : MonoBehaviour
{
    public Slider slider;
    public float duration = 3f;
    public float targetValue = 1f; // e.g. 1 = full if slider range is 0-1

    void Start()
    {
        StartCoroutine(FillSlider());
    }
    IEnumerator FillSlider()
    {
        yield return new WaitForSeconds(0.5f); // optional delay before starting

        float startValue = slider.value;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            slider.value = Mathf.Lerp(startValue, targetValue, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        slider.value = targetValue;
    }
}
