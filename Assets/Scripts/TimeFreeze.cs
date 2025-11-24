using UnityEngine;
public class TimeFreeze : MonoBehaviour
{
    public bool IsFrozen { get; private set; }
    float previousTimeScale = 1f;
    bool previousAudioPause;

    public void Freeze()
    {
        if (IsFrozen) return;
        previousTimeScale = Time.timeScale;
        previousAudioPause = AudioListener.pause;
        AudioListener.pause = true;
        Time.timeScale = 0f;
        IsFrozen = true;
    }

    public void Unfreeze()
    {
        if (!IsFrozen) return;
        AudioListener.pause = previousAudioPause;
        Time.timeScale = previousTimeScale;
        IsFrozen = false;
    }
}
