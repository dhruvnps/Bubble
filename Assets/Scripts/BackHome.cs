using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BackHome : MonoBehaviour
{
    public GameObject clockObject;
    public Game gameScript;
    public Animator canvasAnimator;
    public GameObject exitObject;
    public GameObject widgetObject;

    public void ActivateScene()
    {
        SceneManager.LoadScene("Gameplay");
    }

    public void LoadSolve()
    {
        clockObject.GetComponent<Timer>().RemoveElements();
        canvasAnimator.Play("Idle");
        gameScript.StartSolver();
        widgetObject.SetActive(true);
    }
}
