using UnityEngine;
using UnityEngine.UI;

public class HotkeyButtons : MonoBehaviour
{
    public Button button1;
    public Button button2;
    public bool alsoNumPad = true;

    void Update()
    {
        if (Down(KeyCode.Alpha1, KeyCode.Keypad1)) Press(button1);
        if (Down(KeyCode.Alpha2, KeyCode.Keypad2)) Press(button2);
    }

    bool Down(KeyCode a, KeyCode np)
        => Input.GetKeyDown(a) || (alsoNumPad && Input.GetKeyDown(np));

    void Press(Button b)
    {
        if (!b || !b.interactable || !b.gameObject.activeInHierarchy) return;
        b.onClick.Invoke();
        var pulse = b.GetComponent<UIButtonPulse>();
        if (pulse) pulse.PressOnce();   // <-- правильный вызов
    }
}
