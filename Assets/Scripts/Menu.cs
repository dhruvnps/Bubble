using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Menu : MonoBehaviour
{
    public GameObject dividerObject;
    public GameObject scoreObject;
    public GameObject clockObject;
    public GameObject exitObject;
    public GameObject infiniteObject;
    public GameObject shakeObject;
    public GameObject coverObject;
    public Game gameScript;
    public Timer timerScript;
    public Animator canvasAnimator;
    public float shuffle_u;
    public float shuffle_a;
    public int shuffleCount;
    public GameObject bestObject;
    static TextMeshProUGUI bestText;

    void Start()
    {
        bestText = bestObject.GetComponent<TMPro.TextMeshProUGUI>();
        Menu.UpdateBestText();

        dividerObject.SetActive(false);
        scoreObject.SetActive(false);
        clockObject.SetActive(false);
        exitObject.SetActive(false);
        infiniteObject.SetActive(false);

        gameObject.SetActive(true);
        shakeObject.SetActive(true);
        coverObject.SetActive(true);
        bestObject.SetActive(true);
    }

    public static void UpdateBestText()
    {
        int best = PlayerPrefs.GetInt("Best", 0);
        string text = best.ToString();
        while (text.Length < 3) { text = "0" + text; }
        bestText.text = text;
    }

    public void InitTransitionToGame()
    {
        StartCoroutine(TransitionToGame());
    }

    IEnumerator TransitionToGame()
    {
        gameObject.GetComponent<Animator>().Play("Menu Exit Animation");

        float speed = shuffle_u;
        for (int i = 0; i < shuffleCount; i++)
        {
            speed += shuffle_a;
            float time = 1f / speed;
            yield return new WaitForSeconds(time);
            gameScript.InitResetBoard(false);
        }

        shakeObject.SetActive(false);
        bestObject.SetActive(false);

        yield return new WaitForSeconds(0.5f);

        coverObject.GetComponent<Animator>().Play("Cover Exit Animation");
        yield return new WaitForSeconds(0.5f);

        coverObject.SetActive(false);

        canvasAnimator.Play("Canvas Start Animation");
        dividerObject.SetActive(true);
        scoreObject.SetActive(true);
        clockObject.SetActive(true);
        exitObject.SetActive(true);

        infiniteObject.SetActive(true);
        infiniteObject.GetComponent<Animator>().Play("Infinite Start Animation");

        gameScript.StartGame();
        timerScript.StartTimer();

        gameObject.SetActive(false);
    }

}
