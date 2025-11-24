using System.Collections.Generic;
using UnityEngine;

public class RadialChoiceTrigger : MonoBehaviour
{
    [Header("References")]
    public RadialChoiceWheel wheel;
    public TimeFreeze freezer;
    public Camera uiCam;
    public Transform wheelAnchor;

    [Header("Options for testing")]
    [TextArea]
    public List<string> testOptions = new List<string>
    {
        "Сказать правду",
        "Промолчать",
        "Солгать"
    };

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (!uiCam)
            uiCam = Camera.main;

        if (wheelAnchor)
        {
            wheel.transform.position = wheelAnchor.position;
            wheel.transform.rotation = Quaternion.LookRotation(uiCam.transform.forward, Vector3.up);
        }

        wheel.BuildAndShow(testOptions);

        if (freezer)
            freezer.Freeze();

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (freezer)
            freezer.Unfreeze();

        if (wheel)
            wheel.Hide();

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void OnChoiceSelected(int index)
    {
        if (index >= 0 && index < testOptions.Count)
            Debug.Log($"Выбран вариант: {index} — {testOptions[index]}");
        else
            Debug.Log($"Выбран вариант: {index}");

        if (freezer)
            freezer.Unfreeze();

        if (wheel)
            wheel.Hide();

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}
