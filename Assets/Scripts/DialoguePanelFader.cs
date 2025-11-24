using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class DialoguePanelFader : MonoBehaviour
{
    [Header("Refs")]
    public TMP_Text text;                       // твой TextMeshProUGUI
    [Tooltip("Оставь пустым — подхватит все Image/Graphic/TMP ниже по иерархии")]
    public bool autoCollect = true;

    [Header("FX")]
    public float fadeTime = 0.15f;              // скорость фейда
    public float charsPerSecond = 45f;          // тайпрайтер

    List<Graphic> graphics = new List<Graphic>();
    List<TMP_Text> tmps = new List<TMP_Text>();
    Coroutine fadeCo, typeCo;

    void Awake()
    {
        if (autoCollect)
        {
            graphics.AddRange(GetComponentsInChildren<Graphic>(true));    // Image, RawImage, etc.
            tmps.AddRange(GetComponentsInChildren<TMP_Text>(true));       // TMP тексты
        }
        if (text == null) text = GetComponentInChildren<TMP_Text>(true);
        // стартово делаем видимым
        SetAlpha(1f);
        SetRaycast(true);
    }

    // Публичные методы
    public void Show(string msg, bool typewriter = true)
    {
        if (!text)
        {
            Debug.LogWarning($"{nameof(DialoguePanelFader)} on {name} has no text assigned.");
            return;
        }

        if (typeCo != null) StopCoroutine(typeCo);
        gameObject.SetActive(true);
        SetRaycast(true);

        if (typewriter) typeCo = StartCoroutine(TypeRoutine(msg));
        else if (text) text.text = msg;

        StartFade(1f);
    }

    public void Hide()
    {
        if (typeCo != null)
        {
            StopCoroutine(typeCo);
            typeCo = null;
        }

        SetRaycast(false);
        StartFade(0f, deactivate:true);
    }

    void StartFade(float target, bool deactivate = false)
    {
        if (fadeCo != null) StopCoroutine(fadeCo);
        fadeCo = StartCoroutine(FadeRoutine(target, deactivate));
    }

    IEnumerator FadeRoutine(float target, bool deactivate)
    {
        float start = GetAlpha();
        float t = 0f;
        while (t < 1f)
        {
            t += (fadeTime <= 0f ? 1f : Time.unscaledDeltaTime / fadeTime);
            float a = Mathf.Lerp(start, target, t);
            SetAlpha(a);
            yield return null;
        }
        SetAlpha(target);
        if (deactivate && target <= 0.001f)
            gameObject.SetActive(false);
        fadeCo = null;
    }

    IEnumerator TypeRoutine(string msg)
    {
        if (!text) yield break;
        msg ??= string.Empty;
        text.text = "";
        float shown = 0f;
        while (shown < msg.Length)
        {
            shown += Time.unscaledDeltaTime * charsPerSecond;
            int count = Mathf.Clamp(Mathf.FloorToInt(shown), 0, msg.Length);
            text.text = msg.Substring(0, count);
            yield return null;
        }
        text.text = msg;
        typeCo = null;
    }

    void OnDisable()
    {
        if (fadeCo != null) StopCoroutine(fadeCo);
        if (typeCo != null) StopCoroutine(typeCo);
        fadeCo = typeCo = null;
    }

    // Утилиты
    void SetRaycast(bool on)
    {
        foreach (var g in graphics) g.raycastTarget = on;
        // TMP_Text наследует от MaskableGraphic, значит у него тоже есть raycastTarget
        foreach (var t in tmps) t.raycastTarget = on;
        if (text) text.raycastTarget = on;
    }

    float GetAlpha()
    {
        // берём альфу первого попавшегося графика/текста
        if (graphics.Count > 0) return graphics[0].color.a;
        if (tmps.Count > 0) return tmps[0].color.a;
        if (text) return text.color.a;
        return 1f;
    }

    void SetAlpha(float a)
    {
        Color c;
        foreach (var g in graphics)
        {
            c = g.color; c.a = a; g.color = c;
        }
        foreach (var t in tmps)
        {
            c = t.color; c.a = a; t.color = c;
        }
        if (text)
        {
            c = text.color; c.a = a; text.color = c;
        }
    }
}
