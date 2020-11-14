﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [SerializeField] AudioClip success;
    [SerializeField] AudioClip startSound;
    [SerializeField] AudioClip endSound;
    [SerializeField] AudioClip mudSound;
    [SerializeField] int recoveryTime = 5;
    [SerializeField] float normalSpeed = 6.0f;
    [SerializeField] float mudSpeed = 3.0f;
    [SerializeField] double energyLossRateNormal = 1.0f;
    [SerializeField] double energyLossRateMud = 4.0f;
    [SerializeField] string levelName = "Level 1";
    [SerializeField] string levelSubtitle = "Getting Lost With Andy";
    [SerializeField] string levelRules = "Collect Watchtowers and Race To The End";
    [SerializeField] string loseTitle = "You Lose";
    [SerializeField] string loseSubtitle = "You Have Not Found Enough Watchtowers";
    [SerializeField] string winTitle = "Excellent";
    [SerializeField] string winSubtitle = "You Have Found The Watchtowers";
    [SerializeField] string gameOverTitle = "Game Over";
    [SerializeField] string gameOverSubtitle = "Click to Replay";
    [SerializeField] string endGameSuccessTitle = "Well Done!";
    [SerializeField] string endGameSuccessSubtitle = "Take a Screenshot and Share Your Result";
    [SerializeField] string buttonText = "Play";
    [SerializeField] int energyStraightOn = 25;
    [SerializeField] int numberOfWatchTowersToFind = 1;
    [SerializeField] int energyTurnLeft = 20;
    [SerializeField] int energyTurnRight = 20;
    [SerializeField] int energyDeadEnd = 40;
    [SerializeField] float levelLoadDelay = 2.0f;
    [SerializeField] float turnSpeed = 100.0f;

    private int currentLevel = 0;
    private AudioSource audioSource;
    private CharacterController controller;
    private Animator anim;
    private Vector3 moveDirection = Vector3.zero;
    public float gravity = 20.0f;
    public float jumpForce = 10.0f;
    private float speed = 0.0f;
    private double lossRate = 0.0f;
    private ParticleSystem sweat;
    private GameObject runFaceVar;
    private GameObject jumpFaceVar;
    private GameObject idleFaceVar;
    private Text gameMessageSubTitle;
    private Text gameMessageTitle;
    private Text gameMessageRules;
    private Button gameMessageButton;
    private Canvas buttonWrapper;
    private ProgressBar energyProgress;
    private GameTimer timer;
    const int MAX_ENERGY = 100;
    private int energy = 100;
    private double energyFrame = 0;
    private float savedVolume = 0;

    enum State { Transcending, Playing, Restarting, Waiting }
    enum AnimationState { ToIdle, ToRun, ToJump, ToTired }

    enum MoveState { Idle, Running, Jumping, Tired, Recovering }
    private MoveState moveState = MoveState.Idle;
    enum GroundState { Grass, Mud }
    private GroundState groundState = GroundState.Grass;
   

    void Start()
    {
        DeerWatch.numWatchTowers = 0;
        currentLevel = SceneManager.GetActiveScene().buildIndex;
        audioSource = GetComponent<AudioSource>();
        savedVolume = audioSource.volume;
        controller = GetComponent<CharacterController>();
        anim = gameObject.GetComponentInChildren<Animator>();
        runFaceVar = GameObject.Find("/ChaseHarrier/Armature/Root/Spine1/Spine2/Neck/Head/GEOFaceRun");
        sweat = GameObject.Find("/ChaseHarrier/Sweat").GetComponent<ParticleSystem>();
        sweat.Stop();
        jumpFaceVar = GameObject.Find("/ChaseHarrier/Armature/Root/Spine1/Spine2/Neck/Head/GEOFaceJump");
        idleFaceVar = GameObject.Find("/ChaseHarrier/Armature/Root/Spine1/Spine2/Neck/Head/GEOFaceIdle");
        energyProgress = GameObject.Find("/UIOverlay/EnergyBar").GetComponent<ProgressBar>();
        timer = GameObject.Find("/UIOverlay/TimerText").GetComponent<GameTimer>();
        gameMessageSubTitle = GameObject.Find("/UIOverlay/GameMessageSubTitle").GetComponent<Text>();
        gameMessageTitle = GameObject.Find("/UIOverlay/GameMessageTitle").GetComponent<Text>();
        gameMessageRules = GameObject.Find("/UIOverlay/GameMessageRules").GetComponent<Text>();
        gameMessageButton = GameObject.Find("/UIOverlay/ButtonWrapper/GameMessageButton").GetComponent<Button>();
        gameMessageButton.onClick.AddListener(delegate { btnClicked(""); });
        buttonWrapper = GameObject.Find("/UIOverlay/ButtonWrapper").GetComponent<Canvas>();
        timer.init();
        energyProgress.BarValue = 100;
        runFaceVar.SetActive(false);
        jumpFaceVar.SetActive(false);
        idleFaceVar.SetActive(true);
        initTerrainDependantVars();
        LevelLoadCode();
    }


    private void initTerrainDependantVars()
    {
        if (groundState == GroundState.Grass)
        {
            speed = normalSpeed;
            lossRate = energyLossRateNormal;
            StopMudSound();
        }
        else if (groundState == GroundState.Mud)
        {
            speed = mudSpeed;
            lossRate = energyLossRateMud;
            PlayMudSound();
        }
    }

    private void PlayStartSound()
    {
        audioSource.Stop();
        audioSource.PlayOneShot(startSound);
    }

    private void PlayEndSound()
    {
        audioSource.Stop();
        audioSource.PlayOneShot(endSound);
    }

    private void PlayMudSound()
    {
        if (audioSource.clip == mudSound && audioSource.isPlaying)
            return;
        audioSource.clip = mudSound;
        audioSource.loop = true;
        audioSource.Play();
    }

    private void pauseGroundSound()
    {
        if (audioSource.clip == mudSound && audioSource.isPlaying)
            audioSource.volume = 0;
    }
    private void restartGroundSound()
    {
        if (audioSource.clip == mudSound && audioSource.isPlaying)
            audioSource.volume = savedVolume;
    }


    private void StopMudSound()
    {
        audioSource.loop = false;
        audioSource.Stop();
    }

    private void LoadLevel()
    {
        audioSource.PlayOneShot(success);
        SceneManager.LoadScene(currentLevel);
        LevelLoadCode();
    }

    private void LevelLoadCode()
    {
        gameMessageTitle.text = levelName;
        gameMessageSubTitle.text = levelSubtitle;
        gameMessageButton.GetComponentInChildren<Text>().text = buttonText;
        gameMessageRules.text = levelRules;
        buttonWrapper.enabled = false;
    }

    void btnClicked(string param)
    {
        LoadNextLevel();
    }

    private void LoadNextLevel()
    {
        DeerWatch.numWatchTowers = 0;
        currentLevel++;
        if (currentLevel == SceneManager.sceneCountInBuildSettings)
            currentLevel = 0;
        LoadLevel();
    }

    private void start()
    {
        DeerWatch.numWatchTowersFound = 0;
        timer.StartTimer();
        PlayStartSound();
        gameMessageTitle.enabled = false;
        gameMessageSubTitle.enabled = false;
        buttonWrapper.enabled = false;
        gameMessageRules.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "start_trigger" || other.name == "start_beam")
        {
            if (!timer.go)
            {
                start();
            }
        }
        else if (other.name == "end_trigger" || other.name == "end_beam")
        {
            if (timer.go)
            {
                timer.StopTimer();
                PlayEndSound();
                if(DeerWatch.numWatchTowersFound >= numberOfWatchTowersToFind)
                {
                    showEndScreen(true);
                }
                else
                {
                    showEndScreen(false);
                }
            }
        }
        else if (other.name == "StraightOn")
        {
            addEnergy(energyStraightOn);
        }
        else if (other.name == "RightTurn")
        {
            addEnergy(energyTurnRight);
        }
        else if (other.name == "LeftTurn")
        {
            addEnergy(energyTurnLeft);
        }
        else if (other.name == "DeadEnd")
        {
            addEnergy(energyDeadEnd);
        }
        else if (other.name.ToUpper().Contains("MUD"))
        {
            groundState = GroundState.Mud;
            initTerrainDependantVars();
        }
    }

    private void showEndScreen(Boolean success)
    {
        gameMessageTitle.enabled = true;
        gameMessageSubTitle.enabled = true;
        if (success)
        {
            gameMessageTitle.text = winTitle;
            gameMessageSubTitle.text = winSubtitle;
            if (currentLevel == SceneManager.sceneCountInBuildSettings)
            {
                Invoke("EndGameInstructions", levelLoadDelay);
            }
            else
            {
                Invoke("LoadNextLevel", levelLoadDelay);
            }
        }
        else
        {
            gameMessageTitle.text = loseTitle;
            gameMessageSubTitle.text = loseSubtitle;
            Invoke("GameOver", levelLoadDelay);
        } 
    }

    private void EndGameInstructions()
    {
        gameMessageTitle.text = endGameSuccessTitle;
        gameMessageSubTitle.text = endGameSuccessSubtitle;
        buttonWrapper.enabled = true;
        currentLevel = 0;
    }

    private void GameOver()
    {
        gameMessageTitle.text = gameOverTitle;
        gameMessageSubTitle.text = gameOverSubtitle;
        buttonWrapper.enabled = true;
        currentLevel =0;
    }


    private void OnTriggerExit(Collider other)
    {

        if (other.name.ToUpper().Contains("MUD"))
        {
            groundState = GroundState.Grass;
            initTerrainDependantVars();
        }
    }


    void addEnergy(int amount)
    {
        energy += amount;
        if (energy > 100)
            energy = 100;
        energyProgress.BarValue = energy;
    }

    void updateEnergy()
    {
        energyFrame += lossRate * Time.deltaTime;
        if (energyFrame >= 1.0)
        {
            energy -= 1;
            energyFrame = 0.0;
            energyProgress.BarValue = energy;
        }
    }
    void Recover()
    {
        sweat.Stop();
        energy = MAX_ENERGY;
        energyProgress.BarValue = 100;
        anim.SetInteger("AnimPar", (int)AnimationState.ToIdle);
        moveState = MoveState.Recovering;
        Invoke("SetIdle", recoveryTime);
    }
    void SetIdle()
    {
        moveState = MoveState.Idle;
    }

    // Update is called once per frame
    void Update()
    {
        if (moveState == MoveState.Tired || moveState == MoveState.Recovering)
        {
            return;
        }
        else if (energy == 0)
        {
            sweat.Play();
            anim.SetInteger("AnimPar", (int)AnimationState.ToTired);
            moveDirection = transform.forward * Input.GetAxis("Vertical") * 0;
            float turn = Input.GetAxis("Horizontal");
            transform.Rotate(0, turn * turnSpeed * Time.deltaTime, 0);
            runFaceVar.SetActive(false);
            jumpFaceVar.SetActive(false);
            idleFaceVar.SetActive(true);
            Invoke("Recover", 2.0f);
            pauseGroundSound();
            return;
        }
        else if (controller.isGrounded && Input.GetKey("up"))
        {
            anim.SetInteger("AnimPar", (int)AnimationState.ToRun);
            moveDirection = transform.forward * Input.GetAxis("Vertical") * speed;
            float turn = Input.GetAxis("Horizontal");
            transform.Rotate(0, turn * turnSpeed * Time.deltaTime, 0);
            runFaceVar.SetActive(true);
            jumpFaceVar.SetActive(false);
            idleFaceVar.SetActive(false);
            updateEnergy();
            restartGroundSound();
        }
        else if (controller.isGrounded && Input.GetKey("down"))
        {
            anim.SetInteger("AnimPar", (int)AnimationState.ToRun);
            moveDirection = transform.forward * Input.GetAxis("Vertical") * (speed / 2);
            float turn = Input.GetAxis("Horizontal");
            transform.Rotate(0, turn * turnSpeed * Time.deltaTime, 0);
            runFaceVar.SetActive(true);
            jumpFaceVar.SetActive(false);
            idleFaceVar.SetActive(false);
            updateEnergy();
            restartGroundSound();
        }
        else if (controller.isGrounded)
        {
            anim.SetInteger("AnimPar", (int)AnimationState.ToIdle);
            moveDirection = transform.forward * Input.GetAxis("Vertical") * 0;
            float turn = Input.GetAxis("Horizontal");
            transform.Rotate(0, turn * turnSpeed * Time.deltaTime, 0);
            runFaceVar.SetActive(false);
            jumpFaceVar.SetActive(false);
            idleFaceVar.SetActive(true);
            pauseGroundSound();
        }

        if (Input.GetButton("Jump") && controller.isGrounded)
        {
            anim.SetInteger("AnimPar", (int)AnimationState.ToJump);
            moveDirection.y = jumpForce;
            runFaceVar.SetActive(false);
            jumpFaceVar.SetActive(true);
            idleFaceVar.SetActive(false);
            pauseGroundSound();
            updateEnergy();
        }

        controller.Move(moveDirection * Time.deltaTime);
        moveDirection.y -= gravity * Time.deltaTime;
    }
}
