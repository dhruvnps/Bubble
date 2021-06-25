using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LettersLoad : MonoBehaviour
{
    static LettersLoad instance;
    public LettersLoad GetInstance()
    {
        if (instance == null)
        {
            Object prefab = Resources.Load("Path/To/Prefab");
            Object go = Instantiate(prefab);
            instance = GetComponent<LettersLoad>();
            DontDestroyOnLoad(go);
        }
        return instance;
    }
}
