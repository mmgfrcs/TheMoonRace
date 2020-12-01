using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Composer : MonoBehaviour
{
    private double secPerBeat, delay, songPosition, songPosBeat, songTimeDsp;

    public double InitialDelay => delay;

    public double SongPositionSec => songPosition;

    public double SongPositionBeat => songPosBeat;
    
    private AudioSource musicSource;

    public void PlaySong(AudioClip clip, double bpm, double delay)
    {
        this.delay = delay;
        secPerBeat = 60 / bpm;
        songTimeDsp = AudioSettings.dspTime;
        musicSource.clip = clip;
        musicSource.Play();
    }
    
    // Start is called before the first frame update
    void Start()
    {
        musicSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (musicSource.isPlaying)
        {
            songPosition = AudioSettings.dspTime - songTimeDsp - delay;
            songPosBeat = songPosition / secPerBeat;
        }
    }
}
