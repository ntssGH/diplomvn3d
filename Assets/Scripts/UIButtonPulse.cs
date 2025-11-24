using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonPulse : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public float hoverScale = 1.04f;
    public float pressScale = 0.94f;
    public float lerpSpeed = 18f;

    Vector3 baseScale, targetScale;
    bool isHover;
    Coroutine pressCo;

    void Awake()
    {
        baseScale = transform.localScale;
        targetScale = baseScale;
    }

    void Update()
    {
        float t = Mathf.Clamp01(Time.unscaledDeltaTime * lerpSpeed);
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, t);
    }

    public void OnPointerEnter(PointerEventData e){ isHover = true;  targetScale = baseScale * hoverScale; }
    public void OnPointerExit (PointerEventData e){ isHover = false; targetScale = baseScale; }
    public void OnPointerDown (PointerEventData e){ targetScale = baseScale * pressScale; }
    public void OnPointerUp   (PointerEventData e){ targetScale = isHover ? baseScale * hoverScale : baseScale; }

    // вызывать из хоткеев
    public void PressOnce()
    {
        if (pressCo != null) StopCoroutine(pressCo);
        pressCo = StartCoroutine(PressOnceCo());
    }
    System.Collections.IEnumerator PressOnceCo()
    {
        var returnScale = isHover ? baseScale * hoverScale : baseScale;
        targetScale = baseScale * pressScale;
        yield return new WaitForSecondsRealtime(0.08f);
        targetScale = returnScale;
        pressCo = null;
    }

    void OnDisable()
    {
        if (pressCo != null) StopCoroutine(pressCo);
        isHover = false;
        transform.localScale = baseScale;
        targetScale = baseScale;
    }
}
