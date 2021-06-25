using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;

#if UNITY_IOS
using Unity.Advertisement.IosSupport;
#endif

public class AdManager : MonoBehaviour
{
    private InterstitialAd interstitial;

#if UNITY_IOS
    void Start()
    {
        if(ATTrackingStatusBinding.GetAuthorizationTrackingStatus() == 
        ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
        {
            ATTrackingStatusBinding.RequestAuthorizationTracking();
        }
    }
#endif

    public void RequestInterstitial()
    {
        MobileAds.Initialize(initStatus => { });

#if UNITY_ANDROID
        string adUnitId = Adkeys.adUnitId_ANDROID;
#elif UNITY_IPHONE
        string adUnitId = Adkeys.adUnitId_IPHONE;
#else
        string adUnitId = "unexpected_platform";
#endif

        // Initialize an InterstitialAd.
        this.interstitial = new InterstitialAd(adUnitId);

        // Called when the ad is closed.
        this.interstitial.OnAdClosed += HandleOnAdClosed;

        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
        // Load the interstitial with the request.
        this.interstitial.LoadAd(request);
    }

    public void HandleOnAdClosed(object sender, EventArgs args)
    {
        this.interstitial.Destroy();
        gameObject.GetComponent<BackHome>().ActivateScene();
    }

    public void ShowInterstitial()
    {
        if (this.interstitial.IsLoaded())
        {
            this.interstitial.Show();
            if (Application.isEditor)
            {
                gameObject.GetComponent<BackHome>().ActivateScene();
            }
        }
        else
        {
            this.interstitial.Destroy();
            gameObject.GetComponent<BackHome>().ActivateScene();
        }
    }
}
