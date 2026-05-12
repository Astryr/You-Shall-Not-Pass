using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("BGM Details")]
    [SerializeField] private bool playBgm;
    [SerializeField] private AudioSource[] bgm;
    private int currentBgmIndex;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);

        InvokeRepeating(nameof(PlayMusicIfNeeded), 0, 2);
    }


    public void PlaySFX(AudioSource audioToPlay,bool randomPitch = false)
    {
        if (audioToPlay.clip == null)
        {
            Debug.Log("Could not play " + audioToPlay.gameObject.name + ". There is no audio Clip assigned!");
            return;
        }

        if (audioToPlay.isPlaying)
            audioToPlay.Stop();

        audioToPlay.pitch = randomPitch ? Random.Range(.9f, 1.1f) : 1;
        audioToPlay.Play();
    }

    private void PlayMusicIfNeeded()
    {
        if (bgm.Length <= 0)
        {

            Debug.Log("You trying to play music, but you did not assign any!");
            return;
        }

        if (playBgm == false)
            return;

        if (bgm[currentBgmIndex].isPlaying == false)
            PlayRandomBGM();
    }

    [ContextMenu("Play Random Music")]
    public void PlayRandomBGM()
    {
        currentBgmIndex = Random.Range(0, bgm.Length);
        PlayBGM(currentBgmIndex);
    }

    public void PlayBGM(int bgmToPlay)
    {
        if (bgm.Length <= 0)
        {
            Debug.Log("You trying to play music, but you did not assign any!");
            return;
        }

        StopAllBGM();

        currentBgmIndex = bgmToPlay;
        bgm[bgmToPlay].Play();
    }

    [ContextMenu("Stop All Music")]
    public void StopAllBGM()
    {
        for (int i = 0; i < bgm.Length; i++)
        {
            bgm[i].Stop();
        }
    }

}
