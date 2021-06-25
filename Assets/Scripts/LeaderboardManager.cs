using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeaderboardManager : MonoBehaviour
{
    // NOTE: Leaderboards have NOT been implemented

    void Start()
    {
#if UNITY_ANDROID
        // initialize android lb
#elif UNITY_IPHONE
        // initialize iphone lb
#endif
    }

    public void ShowLeaderboard()
    {
#if UNITY_ANDROID
        // do android lb stuff here
#elif UNITY_IPHONE
        // do iphone lb stuff here
#endif
    }
}
