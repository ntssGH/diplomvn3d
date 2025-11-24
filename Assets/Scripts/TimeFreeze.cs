using UnityEngine;
public class TimeFreeze : MonoBehaviour
{
    public bool IsFrozen { get; private set; }

    public void Freeze()
    {
        if (IsFrozen) return;
        AudioListener.pause = true;
        Time.timeScale = 0f;
        IsFrozen = true;
    }

    public void Unfreeze()
    {
        if (!IsFrozen) return;
        AudioListener.pause = false;
        Time.timeScale = 1f;
        IsFrozen = false;
    }
}
