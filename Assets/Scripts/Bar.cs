using UnityEngine;

public class Bar : MonoBehaviour
{
    private float value = 0f;
    private float minValue = 0f;
    private float maxValue = 1f;

    private float maxWidth;

    private RectTransform barTransform;

    public void SetMinMax(float min, float max)
    {
        minValue = min;
        maxValue = max;
        SetValue(minValue);
    }

    public void SetValue(float value)
    {
        this.value = Mathf.Clamp(value, minValue, maxValue);
        OnValueChange();
    }

    private void OnValueChange()
    {
        float newWidth = maxWidth * (value - minValue) / (maxValue - minValue);

        Vector2 size = barTransform.sizeDelta;
        size.x = newWidth;
        barTransform.sizeDelta = size;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        maxWidth = gameObject.transform.parent.GetComponent<RectTransform>().rect.width;
        barTransform = gameObject.GetComponent<RectTransform>();
    }

    void Start()
    {
        SetValue(value);
    }
}
