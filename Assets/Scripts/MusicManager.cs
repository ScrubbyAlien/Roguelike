using System.Collections;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager instance;

    [SerializeField]
    private AudioSource ambience, music;

    private float musicVolume;

    private void Awake() {
        if (instance) {
            Destroy(gameObject);
        }
        else {
            instance = this;
            DontDestroyOnLoad(gameObject);
            musicVolume = music.volume;
        }
    }

    public void SetMusic(AudioClip clip) {
        music.clip = clip;
    }

    public void PlayMusic() {
        music.Play();
    }

    public void StopMusic() {
        music.Stop();
    }

    public void FadeInMusic(float time) {
        StopAllCoroutines();
        StartCoroutine(FadeIn(music, time, musicVolume));
    }

    public void FadeOutMusic(float time) {
        StopAllCoroutines();
        StartCoroutine(FadeOut(music, time));
    }

    public void TransitionMusic(AudioClip clip, float time) {
        if (music.clip == clip) return;
        StopAllCoroutines();
        StartCoroutine(FadeInFadeOut(music, time, musicVolume, clip));
    }

    public void TransitionMusic(AudioClip clip, float time, float volume) {
        if (music.clip == clip) return;
        StopAllCoroutines();
        StartCoroutine(FadeInFadeOut(music, time, volume, clip));
    }

    private IEnumerator FadeInFadeOut(AudioSource source, float time, float targetVolume, AudioClip clip) {
        yield return StartCoroutine(FadeOut(source, time));
        SetMusic(clip);
        yield return StartCoroutine(FadeIn(source, time, targetVolume));
    }

    private IEnumerator FadeIn(AudioSource source, float time, float targetVolume) {
        float endTime = Time.time + time;
        float startTime = Time.time;
        while (Time.time < endTime) {
            float t = (Time.time - startTime) / time;
            float volume = Mathf.Lerp(0, targetVolume, t);
            source.volume = volume;
            yield return null;
        }
        source.volume = targetVolume;
        PlayMusic();
    }

    private IEnumerator FadeOut(AudioSource source, float time) {
        float endTime = Time.time + time;
        float startTime = Time.time;
        float originalVolume = source.volume;
        while (Time.time < endTime) {
            float t = (Time.time - startTime) / time;
            float volume = Mathf.Lerp(originalVolume, 0, t);
            source.volume = volume;
            yield return null;
        }
        source.volume = 0;
        StopMusic();
    }
}