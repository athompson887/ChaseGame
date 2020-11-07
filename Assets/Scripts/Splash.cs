using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;




public class Splash : MonoBehaviour
{
    [SerializeField] int splashDelay = 5;
    // Start is called before the first frame update
    void Start()
    {
        Invoke("NextScene", splashDelay);
    }

    void NextScene()
    {
        SceneManager.LoadScene(1);
    }

    // Update is called once per frame
    void Update()
    {
        
    }


}
