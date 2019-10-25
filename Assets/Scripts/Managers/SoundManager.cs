using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public bool m_musicEnabled = true;

    public bool m_sfxEnabled = true;

    [Range(0,1)]
    public float m_musicVolume = 1.0f;

    [Range(0,1)]
    public float m_sfxVolume = 1.0f;

    public AudioClip m_clearRowSound;
    public AudioClip m_tetrisSound;
    public AudioClip m_moveSound;
    public AudioClip m_rotateSound;
    public AudioClip m_landSound;
    public AudioClip m_snapSound;
    public AudioClip m_holdSound;
    public AudioClip m_errorSound;
    public AudioClip m_gameOverSound;
    public AudioClip[] m_musicClips;
    public AudioSource m_musicSource;

    public IconToggle m_musicIconToggle;
    public IconToggle m_sfxIconToggle;

    // Start is called before the first frame update
    void Start()
    {
        PlayBackgroundMusic(GetRandomClip(m_musicClips));
        UpdateMusic();
    }

    public AudioClip GetRandomClip(AudioClip[] clips)
    {
        AudioClip activeMusicClip = clips[Random.Range(0, clips.Length)];
        return activeMusicClip;
    }
    public void PlayBackgroundMusic(AudioClip musicClip)
    {
        // Quit command if music is disabled or if source or clip are missing
        if (!m_musicEnabled || !m_musicSource || !musicClip)
        {
            return;
        }

        // if music is playing, stop it
        m_musicSource.Stop();

        m_musicSource.clip = musicClip;

        // Set the music volume
        m_musicSource.volume = m_musicVolume;

        // Music repeats forever
        m_musicSource.loop = true;

        // Start playing
        m_musicSource.Play();
    }

    public void ToggleMusic()
    {
        m_musicEnabled = !m_musicEnabled;
        UpdateMusic();

        if (m_musicIconToggle)
        {
            m_musicIconToggle.ToggleIcon(m_musicEnabled);
        }
    }

    void UpdateMusic()
    {
        m_musicSource.mute = !m_musicEnabled;
    }

    public void ToggleSFX()
    {
        m_sfxEnabled = !m_sfxEnabled;

        if (m_sfxIconToggle)
        {
            m_sfxIconToggle.ToggleIcon(m_sfxEnabled);
        }
    }
}
