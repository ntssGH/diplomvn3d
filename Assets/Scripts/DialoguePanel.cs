using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class DialoguePanel : MonoBehaviour
{
    public TextMeshProUGUI text;
    public float fadeTime = 0.15f;
    public float charsPerSecond = 45f; // скорость печати
    CanvasGroup cg;
    Coroutine typeCo;

    void Awake() => cg = GetComponent<CanvasGroup>();

    public void Show(string msg, bool typewriter = true)
    {
        if (typeCo != null) StopCoroutine(typeCo);
        gameObject.SetActive(true);
        if (typewriter) typeCo = StartCoroutine(TypeRoutine(msg));
        else text.text = msg;
        StartCoroutine(FadeTo(1f));
    }

    public void Hide() => StartCoroutine(FadeTo(0f, deactivate:true));

    IEnumerator FadeTo(float target, bool deactivate = false)
    {
        float start = cg.alpha;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / fadeTime;
            cg.alpha = Mathf.Lerp(start, target, t);
            yield return null;
        }
        cg.alpha = target;
        if (deactivate && Mathf.Approximately(target, 0f))
            gameObject.SetActive(false);
    }

    IEnumerator TypeRoutine(string msg)
    {
        text.text = "";
        float t = 0f;
        int i = 0;
        while (i < msg.Length)
        {
            t += Time.unscaledDeltaTime * charsPerSecond;
            int to = Mathf.Clamp(Mathf.FloorToInt(t), 0, msg.Length);
            if (to != i)
            {
                i = to;
                text.text = msg.Substring(0, i);
            }
            yield return null;
        }
        text.text = msg;
        typeCo = null;
    }
}
