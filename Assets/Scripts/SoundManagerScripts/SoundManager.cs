using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public static class SoundManager
{
    public enum Sound
    {
        Bobbing_SpaceBar,
        Casting_Splash,
        Fish_Catch,
        Got_Line,
        Got_Reel,
        Reel,
        Got_Bad_Item,
        Game_Over
    }

    public static void PlaySound(Sound sound)
    {
        GameObject soundGameObject = new GameObject("Sound");
        AudioSource audioSource = soundGameObject.AddComponent<AudioSource>();
        AudioClip playClip = GetAudioClip(sound);

        float clipTime = playClip.length + 2f;

        RemoveSoundObject rmSound = soundGameObject.AddComponent<RemoveSoundObject>();
        soundGameObject.GetComponent<RemoveSoundObject>().SelfDestruct(clipTime);

        audioSource.PlayOneShot(playClip);
    }

    private static AudioClip GetAudioClip(Sound sound)
    {
        foreach (GameAssets.SoundAudioClip soundAudioClip in GameAssets.Instance.soundAudioClipArray)
        {
            if (soundAudioClip.sound == sound)
                return soundAudioClip.audioClip;
        }

        Debug.LogError("Sound " + sound + " not found!");

        return null;
    }
}
