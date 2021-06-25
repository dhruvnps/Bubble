using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Linq;
using System.Collections.Generic;
using Random = UnityEngine.Random;


public class Game : MonoBehaviour
{
    public Vector3 SmallColliderSize;
    public Vector3 DefaultColliderSize;
    public GameObject tileObject;
    public GameObject pulseObject;
    public GameObject boardObject;
    public GameObject[] spawnPoints;
    public GameObject[] letterObjects;
    public GameObject scoreObject;
    public GameObject infiniteObject;
    public GameObject emptyObject;
    public GameObject fadeObject;

    string[] dice = {
        "AAEEGN", "ABBJOO", "ACHOPS", "AFFKPS",
        "AOOTTW", "CIMOTU", "DEILRX", "DELRVY",
        "DISTTY", "EEGHNW", "EEINSU", "EHRTVW",
        "EIOSST", "ELRTTY", "HIMNUA", "HLNNRZ"
    };

    char[] board = new char[16];
    List<string> playedWords = new List<string>();
    List<GameObject> tiles = new List<GameObject>();
    List<GameObject> letters = new List<GameObject>();
    List<GameObject> highlightedTiles = new List<GameObject>();
    List<String> wordBeingPlayed = new List<String>();
    List<GameObject> previousPoints = new List<GameObject>();
    GameObject tileUnderMouse = null;
    bool wordCompleted = false;
    bool wordValid = false;
    bool defaultColliders = true;
    public Color validColor;
    public Color invalidColor;
    public Color highlightColor;
    public Color borderColor;
    public Color backdropColor;
    Color highlightColorTile;
    Vector3 TileSize;
    List<String> dictionary = new List<String>();
    public TextAsset dictionaryFile;
    public float pulseSpeed;
    public float fadeSpeed;
    public float pulseToScreenSize;
    float pulseSize;
    int pulseIndex = 0;
    Coroutine boardColorsRoutine = null;
    bool gameStarted = false;

    void Start()
    {
        highlightColorTile = backdropColor;
        gameObject.GetComponent<Solver>().enabled = false;
        Application.targetFrameRate = 60;
        ScaleGame();
        NewBoardArray();
        SpawnTheTiles();
        InitializeDictionary();
    }

    public void StartSolver()
    {
        gameObject.GetComponent<Solver>().enabled = true;
    }

    public void StartGame()
    {
        gameStarted = true;
        fadeObject.GetComponent<AdManager>().RequestInterstitial();
    }

    void ScaleGame()
    {
        Debug.Log(Adkeys.adUnitId_ANDROID);
        Debug.Log(Adkeys.adUnitId_IPHONE);
        float aspect = (float)Screen.width / (float)Screen.height;
        Camera.main.orthographicSize /= aspect * 2f;
        pulseSize = Camera.main.orthographicSize * 2 * pulseToScreenSize;
    }

    void InitializeDictionary()
    {
        dictionary = dictionaryFile.text.Split('\n').ToList();
        for (int i = 0; i < dictionary.Count; i++)
        {
            dictionary[i] = dictionary[i].Trim().ToUpper();
        }
    }

    public List<String> GetDictionary()
    {
        return dictionary;
    }

    void NewBoardArray()
    {
        List<int> diceNumbers = Enumerable.Range(0, 16).ToList();
        for (int i = 0; i <= 15; i++)
        {
            int x = Random.Range(0, diceNumbers.Count());
            board[i] = dice[diceNumbers[x]][Random.Range(0, 6)];
            diceNumbers.RemoveAt(x);
        }
    }

    public char[] GetBoard()
    {
        return board;
    }

    void SpawnTheTiles()
    {
        foreach (GameObject point in spawnPoints)
        {
            BoxCollider thisCollider = point.GetComponent<BoxCollider>();
            thisCollider.size = DefaultColliderSize;
        }

        TileSize = tileObject.transform.localScale;
        for (int i = 0; i <= 15; i++)
        {
            GameObject point = spawnPoints[i];
            char character = board[i];
            int characterIndex = (int)character - 65;
            tiles.Add(Instantiate(tileObject, point.transform.position, Quaternion.identity));
            tiles[i].SetActive(true);
            letters.Add((Instantiate(letterObjects[characterIndex], point.transform.position, Quaternion.identity)));
            letters[i].GetComponent<SpriteRenderer>().material.color = Color.black;
            letters[i].transform.parent = tiles[i].transform;
            letters[i].SetActive(true);
        }
    }

    void Update()
    {
        if (gameStarted)
        {
            if (Input.GetMouseButton(0))
            {
                CastRay();
            }
            else if (Input.GetMouseButtonUp(0))
            {
                EndWord();
            }
            UpdateHighlightedTiles();
            UpdateBoxCollider(wordBeingPlayed.Count == 0);
        }
    }

    public void HighlightPath(List<int> path, bool wordGot)
    {
        foreach (UnityEngine.Object obj in GameObject.FindGameObjectsWithTag("EmptyClone"))
        {
            Destroy(obj);
        }
        for (int i = 0; i < 16; i++)
        {
            SpriteRenderer tileRenderer = tiles[i].GetComponent<SpriteRenderer>();
            SpriteRenderer letterRenderer = letters[i].GetComponent<SpriteRenderer>();
            if (path.Contains(i))
            {
                tileRenderer.material.color = wordGot ? validColor : highlightColorTile;
                letterRenderer.material.color = wordGot ? Color.white : highlightColor;
            }
            else
            {
                tileRenderer.material.color = Color.white;
                letterRenderer.material.color = Color.black;
            }
        }
        for (int i = 0; i < path.Count - 1; i++)
        {
            Vector3[] points = new Vector3[]
            {
                spawnPoints[path[i]].transform.position,
                spawnPoints[path[i + 1]].transform.position
            };
            GameObject emptyClone = Instantiate(emptyObject, emptyObject.transform.position, Quaternion.identity);
            emptyClone.tag = "EmptyClone";
            LineRenderer lr = emptyClone.AddComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.sortingOrder = -1;
            lr.widthMultiplier = 0.1f;
            lr.endColor = wordGot ? Color.white : highlightColor;
            lr.startColor = wordGot ? Color.white : highlightColor;
            lr.SetPositions(points);
        }
    }

    void UpdateHighlightedTiles()
    {
        foreach (GameObject tile in highlightedTiles)
        {
            SpriteRenderer tileRenderer = tile.GetComponent<SpriteRenderer>();
            SpriteRenderer letterRenderer = letters[Array.IndexOf(tiles.ToArray(), tile)].GetComponent<SpriteRenderer>();

            if (tileRenderer.material.color != highlightColorTile)
            {
                tileRenderer.material.color = highlightColorTile;
            }
            if (letterRenderer.material.color != highlightColor)
            {
                letterRenderer.material.color = highlightColor;
            }
        }
    }

    void UpdateBoxCollider(bool makeDefault)
    {
        if (makeDefault != defaultColliders)
        {
            foreach (GameObject point in spawnPoints)
            {
                BoxCollider thisCollider = point.GetComponent<BoxCollider>();
                if (makeDefault)
                {
                    defaultColliders = true;
                    thisCollider.size = DefaultColliderSize;
                }
                else
                {
                    defaultColliders = false;
                    thisCollider.size = SmallColliderSize;
                }
            }
        }
    }

    void CastRay()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (
            Physics.Raycast(ray, out hit, Mathf.Infinity)
            && (
                !previousPoints.Contains(hit.collider.gameObject)
                || previousPoints[previousPoints.Count - 1] == hit.collider.gameObject
            )
        )
        {
            if (hit.collider.gameObject == boardObject)
            {
                tileUnderMouse = null;
            }
            foreach (GameObject point in spawnPoints)
            {
                if (hit.collider.gameObject == point && !previousPoints.Contains(point))
                {
                    float dist;
                    if (previousPoints.Count > 0)
                    {
                        dist = Mathf.Abs(Vector3.Distance(
                            previousPoints[previousPoints.Count - 1].transform.position,
                            point.transform.position
                        ));
                    }
                    else
                    {
                        dist = 0;
                    }
                    if (dist < 1.5)
                    {
                        previousPoints.Add(point);
                        tileUnderMouse = tiles[Array.IndexOf(spawnPoints, point)];
                        StartCoroutine(TileAnimation(tileUnderMouse, point));
                        char charcterPressed = board[Array.IndexOf(spawnPoints, point)];
                        wordBeingPlayed.Add(charcterPressed.ToString());
                    }
                    else
                    {
                        EndWord();
                    }
                }
            }
        }
        else
        {
            EndWord();
        }
    }

    IEnumerator TileAnimation(GameObject tile, GameObject point)
    {
        highlightedTiles.Add(tile);
        Vector3 originalPosition = point.transform.position;
        tile.transform.localScale += new Vector3(0.03f, 0.03f, 0);
        SpriteRenderer tileRenderer = tile.GetComponent<SpriteRenderer>();
        SpriteRenderer letterRenderer = letters[Array.IndexOf(tiles.ToArray(), tile)].GetComponent<SpriteRenderer>();

        while (tileUnderMouse == tile)
        {
            Vector3 mouse = Input.mousePosition;
            Ray castPoint = Camera.main.ScreenPointToRay(mouse);
            RaycastHit hit;
            if (Physics.Raycast(castPoint, out hit, Mathf.Infinity))
            {
                tile.transform.position = hit.point;
            }

            tileRenderer.GetComponent<Renderer>().sortingOrder = 2;
            letterRenderer.GetComponent<Renderer>().sortingOrder = 3;

            yield return null;
        }

        tileRenderer.GetComponent<Renderer>().sortingOrder = 0;
        letterRenderer.GetComponent<Renderer>().sortingOrder = 1;
        StartCoroutine(OriginalVectors(tile, originalPosition, TileSize));

        while (!wordCompleted)
        {
            yield return null;
        }

        for (int i = 0; i <= 15; i++)
        {
            if (highlightedTiles.Contains(tiles[i]))
            {
                StartCoroutine(OriginalColors(tiles[i], letters[i], wordValid));
            }
        }

        wordValid = false;
        wordCompleted = false;
    }

    IEnumerator OriginalVectors(GameObject tile, Vector3 pos, Vector3 size)
    {
        float dist = Mathf.Abs(Vector3.Distance(pos, tile.transform.position));
        while (dist > 0.01f)
        {
            dist = Mathf.Abs(Vector3.Distance(pos, tile.transform.position));
            tile.transform.position = Vector3.Lerp(tile.transform.position, pos, Time.deltaTime * 10f);
            tile.transform.localScale = Vector3.Lerp(tile.transform.localScale, size, Time.deltaTime * 5f);
            yield return null;
        }

        tile.transform.position = pos;
        tile.transform.localScale = size;
        yield return null;
    }

    IEnumerator OriginalColors(GameObject tile, GameObject letter, bool valid)
    {
        StartCoroutine(LetterColors(tile, letter, valid ? validColor : invalidColor));
        if (boardColorsRoutine != null)
        {
            StopCoroutine(boardColorsRoutine);
        }
        boardColorsRoutine = StartCoroutine(BoardColors(valid ? validColor : invalidColor));
        for (float i = 0; i < 1; i += Time.deltaTime)
        {
            if (previousPoints.Contains(spawnPoints[Array.IndexOf(tiles.ToArray(), tile)])) { yield break; }

            highlightedTiles.Remove(tile);
            tile.GetComponent<SpriteRenderer>().material.color = Color.Lerp(
                tile.GetComponent<SpriteRenderer>().material.color, Color.white, 0.2f
            );
            yield return null;
        }
        tile.GetComponent<SpriteRenderer>().material.color = Color.white;
    }

    IEnumerator LetterColors(GameObject tile, GameObject letter, Color newColor)
    {
        Renderer letterRenderer = letter.GetComponent<SpriteRenderer>();
        for (float i = 0; i < 1; i += Time.deltaTime)
        {
            if (previousPoints.Contains(spawnPoints[Array.IndexOf(tiles.ToArray(), tile)])) { yield break; }

            letterRenderer.material.color = Color.Lerp(
                letterRenderer.material.color, newColor, fadeSpeed * Time.deltaTime
            );
            yield return null;
        }
        for (float i = 0; i < 1; i += Time.deltaTime)
        {
            if (previousPoints.Contains(spawnPoints[Array.IndexOf(tiles.ToArray(), tile)])) { yield break; }

            letterRenderer.material.color = Color.Lerp(
                letterRenderer.material.color, Color.black, fadeSpeed * Time.deltaTime
            );
            yield return null;
        }
        letterRenderer.material.color = Color.black;
    }

    IEnumerator BoardColors(Color newColor)
    {
        SpriteRenderer boardRenderer = boardObject.GetComponent<SpriteRenderer>();
        for (float i = 0; i < 1; i += Time.deltaTime)
        {
            boardRenderer.color = Color.Lerp(boardRenderer.color, newColor, fadeSpeed * Time.deltaTime);
            yield return null;
        }
        for (float i = 0; i < 1; i += Time.deltaTime)
        {
            boardRenderer.color = Color.Lerp(boardRenderer.color, borderColor, fadeSpeed * Time.deltaTime);
            yield return null;
        }
        boardRenderer.color = borderColor;
    }

    IEnumerator Pulsate(Color pulseColor, int layer)
    {
        GameObject pulseClone;
        pulseClone = Instantiate(pulseObject, new Vector3(0.5f, 0.5f, 1f), Quaternion.identity);
        pulseClone.GetComponent<SpriteRenderer>().color = pulseColor;
        pulseClone.GetComponent<SpriteRenderer>().GetComponent<Renderer>().sortingOrder = layer;
        for (float i = 0; i < 1; i += Time.deltaTime)
        {
            pulseClone.transform.localScale = Vector3.Lerp(
                pulseClone.transform.localScale, new Vector3(pulseSize, pulseSize, 1), pulseSpeed * Time.deltaTime
            );
            yield return null;
        }
        Color newColor;
        for (float i = 0; i < 1; i += Time.deltaTime)
        {
            newColor = pulseClone.GetComponent<SpriteRenderer>().color;
            newColor.a = Mathf.Lerp(newColor.a, 0f, fadeSpeed * Time.deltaTime);
            pulseClone.GetComponent<SpriteRenderer>().color = newColor;
            yield return null;
        }
        GameObject.Destroy(pulseClone);
    }

    public void EndWord()
    {
        tileUnderMouse = null;
        previousPoints.Clear();
        string finalWord = string.Join("", wordBeingPlayed.ToArray());
        if (finalWord.Length > 0)
        {
            SubmitWord(finalWord.ToUpper());
        }
        wordBeingPlayed.Clear();
    }

    void SubmitWord(string word)
    {
        wordCompleted = true;
        pulseIndex++;
        wordValid = word.Length > 2 && dictionary.Contains(word) && !playedWords.Contains(word) ? true : false;
        if (wordValid)
        {
            StartCoroutine(Pulsate(validColor, pulseIndex));
            playedWords.Add(word);
            scoreObject.GetComponent<Score>().CorrectWord(word);
        }
        else
        {
            StartCoroutine(Pulsate(invalidColor, pulseIndex));
            scoreObject.GetComponent<Score>().IncorrectWord();
        }
        if (infiniteObject.GetComponent<RectTransform>().anchoredPosition.y > -240)
        {
            infiniteObject.GetComponent<Animator>().Play("Infinite Animation");
        }
    }

    public List<string> GetPlayedWords()
    {
        return playedWords;
    }

    public void InitResetBoard(bool resetScore)
    {
        StartCoroutine(ResetBoard(resetScore));
    }

    IEnumerator ResetBoard(bool resetScore)
    {
        if (resetScore)
        {
            scoreObject.GetComponent<Score>().ResetScore();
        }
        playedWords.Clear();
        foreach (GameObject tile in tiles)
        {
            GameObject.DestroyImmediate(tile);
        }
        tiles.Clear();
        foreach (GameObject letter in letters)
        {
            GameObject.DestroyImmediate(letter);
        }
        letters.Clear();
        NewBoardArray();
        SpawnTheTiles();
        pulseIndex++;
        yield return null;
    }

}