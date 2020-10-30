using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flower : MonoBehaviour
{
    [SerializeField] AudioClip success;
    [SerializeField] ParticleSystem successParticles;
    AudioSource audioSource;
    public float volume = 0.5f;
    enum State { Left, Right, Straight, DeadEnd }
    [SerializeField] State state;


    void Start()
    {
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

    private void Render()
    {
      //var guiText = transform.Find("/UIOverlay/DeerText").GetComponent<Text>();
      //guiText.text = "WATCHTOWERS FOUND " + numWatchTowersFound.ToString() + "/" + numWatchTowers.ToString();
    }

    public void OnTriggerEnter(Collider other)
    {
        if (state == State.Left)
        {
            MakeGemNoise();
        }
        else if (state == State.Right)
        {
            MakeGemNoise();
        }
        else if (state == State.DeadEnd)
        {
            MakeGemNoise();
        }
       else if (state == State.Straight)
        {
            MakeGemNoise();
        }
    }
}
