using System.Collections;
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
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (playBgm)
            PlayMenuMusic();
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

    public void PlayMenuMusic()
    {
        if (!playBgm || bgm.Length <= 0) return;
        PlayBGM(0);
    }

    public void PlayLevelMusic()
    {
        if (!playBgm || bgm.Length <= 0) return;
        int index = bgm.Length > 1 ? 1 : 0;
        PlayBGM(index);
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
