using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{

    public Animator canvasAnimator;
    TextMeshProUGUI clockText;
    public int totalTime;
    int timeLeft;
    Coroutine highlightTextRoutine = null;
    public float warpSpeed;
    public float bubbleDelay;
    public float buttonsDelay;
    public float bubbleSpeed;
    public float shockShrinkSpeed;
    public GameObject scoreObject;
    public GameObject solveObject;
    public GameObject warpObject;
    public GameObject infiniteObject;
    public GameObject scoreGhostObject;
    public GameObject finalScoreObject;
    public GameObject spawnManagerObject;
    public GameObject dividerObject;
    public Game gameScript;
    bool newBest = false;

    void Start()
    {
        solveObject.SetActive(false);
        clockText = gameObject.GetComponent<TMPro.TextMeshProUGUI>();
    }

    public void StartTimer()
    {
        StartCoroutine(Countdown(totalTime));
    }

    void UpdateClockText()
    {
        string text = timeLeft.ToString();
        clockText.text = text.Length == 1 ? "0" + text : text;
    }

    IEnumerator Countdown(int val)
    {
        timeLeft = val;
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(Shock(2.0f, true));
        while (timeLeft > 0)
        {
            yield return new WaitForSeconds(1.0f);
            timeLeft--;
            if (timeLeft > 10 || timeLeft == 0) { UpdateClockText(); }
            else { StartCoroutine(Shock(1.0f, false)); }
        }
        yield return new WaitForSeconds(0.05f);
        if (gameObject.GetComponent<Timer>().enabled == true)
        {
            Transition();
        }
    }

    IEnumerator Shock(float time, bool revert)
    {
        clockText.fontSize += 8;
        UpdateClockText();
        float newSize = clockText.fontSize - 8;
        for (float i = 0; i < time; i += Time.deltaTime)
        {
            clockText.fontSize = Mathf.Lerp(clockText.fontSize, newSize, shockShrinkSpeed * Time.deltaTime);
            UpdateClockText();
            yield return null;
        }
        clockText.fontSize = newSize;
    }

    public void InitHighlightText(float fadeSpeed)
    {
        if (highlightTextRoutine != null)
        {
            StopCoroutine(highlightTextRoutine);
        }
        highlightTextRoutine = StartCoroutine(HighlightText(fadeSpeed));
    }

    IEnumerator HighlightText(float fadeSpeed)
    {
        if (timeLeft > 10) { yield break; }
        for (float i = 0; i < 1; i += Time.deltaTime)
        {
            clockText.color = Color32.Lerp(clockText.color, Color.white, fadeSpeed * Time.deltaTime);
            UpdateClockText();
            yield return null;
        }
        for (float i = 0; i < 1; i += Time.deltaTime)
        {
            clockText.color = Color32.Lerp(clockText.color, Color.black, fadeSpeed * Time.deltaTime);
            UpdateClockText();
            yield return null;
        }
    }

    void Transition()
    {
        gameScript.EndWord();
        scoreObject.GetComponent<Score>().TimeFinished();
        spawnManagerObject.GetComponent<Game>().enabled = false;
        StartCoroutine(Warp());
        StartCoroutine(ButtonsIn());
        StartCoroutine(ScoreBubble());
        scoreGhostObject.SetActive(false);
        if (infiniteObject.GetComponent<RectTransform>().anchoredPosition.y > -240)
        {
            infiniteObject.GetComponent<Animator>().Play("Infinite Animation");
        }
        canvasAnimator.Play("Canvas Timeup Animation");
    }

    public void NewBest()
    {
        newBest = true;
    }

    IEnumerator ButtonsIn()
    {
        yield return new WaitForSeconds(buttonsDelay);
        solveObject.SetActive(true);
        canvasAnimator.Play("Canvas Buttons Animation");
    }

    IEnumerator ScoreBubble()
    {
        yield return new WaitForSeconds(bubbleDelay);
        finalScoreObject.SetActive(true);
        TextMeshProUGUI finalScoreText = finalScoreObject.GetComponent<TMPro.TextMeshProUGUI>();
        if (finalScoreText.text[0] == '1' && finalScoreText.text[finalScoreText.text.Length - 1] != '1')
        {
            Vector3 newPos = finalScoreObject.GetComponent<RectTransform>().anchoredPosition;
            newPos.x -= 2;
            finalScoreObject.GetComponent<RectTransform>().anchoredPosition = newPos;
        }
        if (newBest)
        {
            // new best visuals and stuff
        }
        GameObject bubbleClone;
        bubbleClone = Instantiate(warpObject, new Vector3(0.5f, 0.5f, 1f), Quaternion.identity);
        bubbleClone.gameObject.tag = "ToRemove";
        bubbleClone.transform.localScale = new Vector3(0f, 0f, 1f);
        SpriteRenderer bubbleRenderer = bubbleClone.GetComponent<SpriteRenderer>();
        bubbleRenderer.color = Color.black;
        bubbleRenderer.GetComponent<Renderer>().sortingLayerName = "Transition";
        bubbleRenderer.GetComponent<Renderer>().sortingOrder = 2;
        for (float i = 0; i < 1; i += Time.deltaTime)
        {
            bubbleClone.transform.localScale = Vector3.Lerp(
                bubbleClone.transform.localScale, new Vector3(2f, 2f, 1), bubbleSpeed * Time.deltaTime
            );
            yield return null;
        }
    }

    IEnumerator Warp()
    {
        float warpSize = Camera.main.orthographicSize * 2;
        GameObject warpClone;
        warpClone = Instantiate(warpObject, new Vector3(0.5f, 0.5f, 1f), Quaternion.identity);
        warpClone.gameObject.tag = "ToRemove";
        warpClone.transform.localScale = new Vector3(0f, 0f, 1f);
        warpClone.GetComponent<SpriteRenderer>().color = gameScript.backdropColor;
        warpClone.GetComponent<SpriteRenderer>().GetComponent<Renderer>().sortingLayerName = "Transition";
        warpClone.GetComponent<SpriteRenderer>().GetComponent<Renderer>().sortingOrder = 0;
        for (float i = 0; i < 2; i += Time.deltaTime)
        {
            warpClone.transform.localScale = Vector3.Lerp(
                warpClone.transform.localScale, new Vector3(warpSize, warpSize, 1), warpSpeed * Time.deltaTime
            );
            yield return null;
        }
    }

    public void RemoveElements()
    {
        foreach (Object obj in GameObject.FindGameObjectsWithTag("ToRemove"))
        {
            Destroy(obj);
        }
        finalScoreObject.SetActive(false);
        solveObject.SetActive(false);
        infiniteObject.SetActive(false);
        scoreObject.SetActive(false);
        dividerObject.SetActive(false);
        gameObject.SetActive(false);
    }

}
