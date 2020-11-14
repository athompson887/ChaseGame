using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeerWatch : MonoBehaviour
{
    [SerializeField] AudioClip success;
    [SerializeField] ParticleSystem successParticles;

    public static int numWatchTowers = 0;
    public static int numWatchTowersFound = 0;
    public static Text deerText;

    AudioSource audioSource;
    public float volume = 0.5f;
    enum State { Found, Hidden }
    State state = State.Hidden;
   

    void Start()
    {
        if(deerText==null)
            deerText = transform.Find("/UIOverlay/DeerText").GetComponent<Text>();

        audioSource = GetComponent<AudioSource>();
        Render();
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

    public static void Render()
    {
        deerText.text = "Watchtowers Found " + numWatchTowersFound.ToString() + "/" + numWatchTowers.ToString();
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