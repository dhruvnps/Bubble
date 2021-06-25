using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadGame : MonoBehaviour
{
    void Start()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Gameplay");
    }
}
