using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioSourceController : MonoBehaviour
{
    [Header("Inscribed")]
    public AudioClip[] audioClips;

    AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayRandomAudioClip()
    {
        if (audioClips.Length > 0)
            audioSource.clip = audioClips[Random.Range(0, audioClips.Length)];
        if (audioSource.clip != null)
            audioSource.Play();
    }
}
