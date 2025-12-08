using UnityEngine;

public class PlaySoundOnClick : MonoBehaviour
{
    [SerializeField] private AudioSource[] audioSources; // Just reference the AudioSources

    public void PlaySound()
    {
        if (audioSources != null && audioSources.Length > 0)
        {
            foreach (AudioSource source in audioSources)
            {
                if (source != null)
                {
                    source.Play();
                }
            }
        }
    }
}