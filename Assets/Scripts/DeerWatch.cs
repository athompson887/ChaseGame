using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeerWatch : MonoBehaviour
{
    [SerializeField] AudioClip success;
    [SerializeField] ParticleSystem successParticles;
    public static int numWatchTowers;

    AudioSource audioSource;
    public float volume = 0.5f;
    enum State { Found, Hidden }
    State state = State.Hidden;
    public static int numWatchTowersFound = 0;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        Render();
        numWatchTowers++;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void MakeGemNoise()
    {
        audioSource.Stop();
        audioSource.PlayOneShot(success);
        successParticles.Play();
    }

    public void Reset()
    {
        state = State.Hidden;
        numWatchTowersFound = 0;
    }

    private void Render()
    {
        var guiText = transform.Find("/UIOverlay/DeerText").GetComponent<Text>();
        guiText.text = "Watchtowers Found " + numWatchTowersFound.ToString() + "/" + numWatchTowers.ToString();
    }

    public void OnTriggerEnter(Collider other)
    {
        if(state==State.Hidden)
        {
            numWatchTowersFound++;
            MakeGemNoise();
            Render();
            state = State.Found;
        }
    }
}
