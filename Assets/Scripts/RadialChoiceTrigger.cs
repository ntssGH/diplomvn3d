using System.Collections.Generic;
using UnityEngine;

public class RadialChoiceTrigger : MonoBehaviour
{
    [Header("References")]
    public RadialChoiceWheel wheel;   // сюда перетащи UI_RadialChoice
    public TimeFreeze freezer;        // сюда объект с TimeFreeze
    public Camera uiCam;              // Main Camera
    public Transform wheelAnchor;     // пустышка в мире, где висит колесо

    [Header("Test options")]
    [TextArea]
    public List<string> testOptions = new List<string>
    {
        "Сказать правду",
        "Промолчать",
        "Солгать"
    };

    // Игрок должен иметь тег "Player"
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (uiCam == null) uiCam = Camera.main;

        // Ставим колесо в нужное место в мире
        if (wheelAnchor != null)
        {
            wheel.transform.position = wheelAnchor.position;
            // Поворачиваем лицом к камере
            wheel.transform.rotation =
                Quaternion.LookRotation(uiCam.transform.forward, Vector3.up);
        }

        // Показываем колесо
        wheel.BuildAndShow(testOptions, uiCam);

        // Фризим время (если задан)
        if (freezer != null)
            freezer.Freeze();

        // Курсор нам не нужен — пусть камера крутится
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Если вышли из зоны — просто прячем колесо и размораживаем
        if (freezer != null)
            freezer.Unfreeze();

        if (wheel != null)
            wheel.Hide();

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Это можно повесить в OnChoice у RadialChoiceWheel (в инспекторе)
    public void OnChoiceSelected(int index)
    {
        Debug.Log($"Выбран вариант: {index} — {testOptions[index]}");

        if (freezer != null)
            freezer.Unfreeze();

        if (wheel != null)
            wheel.Hide();

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        // тут дальше твоя логика ветки диалога
    }
}
