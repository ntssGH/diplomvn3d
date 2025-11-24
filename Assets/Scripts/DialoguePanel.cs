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
    Coroutine typeCo, fadeCo;

    void Awake() => cg = GetComponent<CanvasGroup>();

    public void Show(string msg, bool typewriter = true)
    {
        if (text == null)
        {
            Debug.LogWarning($"{nameof(DialoguePanel)} on {name} has no text assigned.");
            return;
        }

        if (typeCo != null) StopCoroutine(typeCo);
        if (fadeCo != null) StopCoroutine(fadeCo);

        gameObject.SetActive(true);
        typeCo = typewriter ? StartCoroutine(TypeRoutine(msg)) : null;
        if (!typewriter) text.text = msg;

        fadeCo = StartCoroutine(FadeTo(1f));
    }

    public void Hide()
    {
        if (typeCo != null)
        {
            StopCoroutine(typeCo);
            typeCo = null;
        }

        if (fadeCo != null) StopCoroutine(fadeCo);
        fadeCo = StartCoroutine(FadeTo(0f, deactivate:true));
    }

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
        msg ??= string.Empty;
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

    void OnDisable()
    {
        if (typeCo != null) StopCoroutine(typeCo);
        if (fadeCo != null) StopCoroutine(fadeCo);
        typeCo = fadeCo = null;
    }
}
