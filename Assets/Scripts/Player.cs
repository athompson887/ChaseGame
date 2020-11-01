using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [SerializeField] AudioClip success;
    [SerializeField] AudioClip startSound;
    [SerializeField] AudioClip endSound;
    [SerializeField] AudioClip mudSound;
    [SerializeField] float normalSpeed = 6.0f;
    [SerializeField] float mudSpeed = 3.0f;
    [SerializeField] double energyLossRateNormal = 1.0f;
    [SerializeField] double energyLossRateMud = 4.0f;
    [SerializeField] int energyStraightOn = 25;
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
    private GameObject runFaceVar;
    private GameObject jumpFaceVar;
    private GameObject idleFaceVar;
    private ProgressBar energyProgress;
    const int MAX_ENERGY = 100;
    private int energy = 100;
    private double energyFrame = 0;
    private float savedVolume = 0;

    enum State { Transcending, Playing,Restarting,Waiting}
    private State state = State.Waiting;

    enum GroundState { Grass, Mud }
    private GroundState groundState = GroundState.Grass;

    // Start is called before the first frame update
    void Start()
    {
        currentLevel = SceneManager.GetActiveScene().buildIndex;
        audioSource = GetComponent<AudioSource>();
        savedVolume = audioSource.volume;
        controller = GetComponent<CharacterController>(); 
        anim = gameObject.GetComponentInChildren<Animator>();
        runFaceVar = GameObject.Find("/ChaseHarrier/Armature/Root/Spine1/Spine2/Neck/Head/GEOFaceRun");
        jumpFaceVar = GameObject.Find("/ChaseHarrier/Armature/Root/Spine1/Spine2/Neck/Head/GEOFaceJump");
        idleFaceVar = GameObject.Find("/ChaseHarrier/Armature/Root/Spine1/Spine2/Neck/Head/GEOFaceIdle");
        energyProgress = GameObject.Find("/UIOverlay/EnergyBar").GetComponent<ProgressBar>();
        energyProgress.BarValue = 100;
        runFaceVar.SetActive(false);
        jumpFaceVar.SetActive(false);
        idleFaceVar.SetActive(true);
        initTerrainDependantVars();
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
        state = State.Waiting;
        audioSource.PlayOneShot(success);
        SceneManager.LoadScene(currentLevel);
        if (currentLevel == 0)
        {
            LevelOneCode();
        }
    }
    
    private void LevelOneCode()
    {

    }

    private void LoadNextLevel()
    {
        currentLevel++;
        if (currentLevel == SceneManager.sceneCountInBuildSettings)
            currentLevel = 0;
        LoadLevel();
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.name=="start_trigger" || other.name == "start_beam")
        {
            GameTimer timer = GameObject.Find("/UIOverlay/TimerText").GetComponent<GameTimer>();
            if (!timer.go)
            {
                timer.StartTimer();
                PlayStartSound();
            }
        }
        else if (other.name == "end_trigger" || other.name == "end_beam")
        {
            GameTimer timer = GameObject.Find("/UIOverlay/TimerText").GetComponent<GameTimer>();
            if (timer.go)
            {
                timer.StopTimer();
                PlayEndSound();
                state = State.Transcending;
                Invoke("LoadNextLevel", levelLoadDelay);
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
        else if (other.name == "Mud")
        {
            groundState = GroundState.Mud;
            initTerrainDependantVars();
        }
    }

   

    private void OnTriggerExit(Collider other)
    {
      
        if (other.name == "Mud")
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
        if(energyFrame >= 1.0)
        {
            energy -= 1;
            energyFrame = 0.0;
            energyProgress.BarValue = energy;
        }
    }
    void Recover()
    {
        energy = MAX_ENERGY;
        energyProgress.BarValue = 100;
        print("Player recovered");
    }

    // Update is called once per frame
    void Update()
    {
        if(energy==0)
        {
            anim.SetInteger("AnimPar", 0);
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
        if(controller.isGrounded && Input.GetKey("up"))
        {
            anim.SetInteger("AnimPar", 1);
            moveDirection = transform.forward * Input.GetAxis("Vertical") * speed;
            float turn = Input.GetAxis("Horizontal");
            transform.Rotate(0, turn * turnSpeed * Time.deltaTime,0);
            runFaceVar.SetActive(true);
            jumpFaceVar.SetActive(false);
            idleFaceVar.SetActive(false);
            updateEnergy();
            restartGroundSound();
        }
        else if (controller.isGrounded && Input.GetKey("down"))
        {
            anim.SetInteger("AnimPar", 1);
            moveDirection = transform.forward * Input.GetAxis("Vertical") * (speed/2);
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
            anim.SetInteger("AnimPar", 0);
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
            anim.SetInteger("AnimPar", 2);
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
