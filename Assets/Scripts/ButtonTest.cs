using UnityEngine;
using UnityEngine.UI;

public class ButtonTest : MonoBehaviour
{
    public Button button1;
    public Button button2;

    void Start()
    {
        button1.onClick.AddListener(() => Debug.Log("Нажата кнопка 1!"));
        button2.onClick.AddListener(() => Debug.Log("Нажата кнопка 2!"));
    }
}
