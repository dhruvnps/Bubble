// Source: https://alanyeats.com/post/unityapptrackingtransparencypopup/

#if UNITY_IOS
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;

public class PostBuildStep
{
    /// <summary>
    /// Description for IDFA request notification 
    /// [sets NSUserTrackingUsageDescription]
    /// </summary>
    const string TrackingDescription =
        "This identifier will be used to deliver personalized ads to you. ";

    [PostProcessBuild(0)]
    public static void OnPostprocessBuild(BuildTarget buildTarget, string pathToXcode)
    {
        if (buildTarget == BuildTarget.iOS)
        {
            AddPListValues(pathToXcode);
        }
    }

    static void AddPListValues(string pathToXcode)
    {
        // Get Plist from Xcode project 
        string plistPath = pathToXcode + "/Info.plist";

        // Read in Plist 
        PlistDocument plistObj = new PlistDocument();
        plistObj.ReadFromString(File.ReadAllText(plistPath));

        // set values from the root obj
        PlistElementDict plistRoot = plistObj.root;

        // Set value in plist
        plistRoot.SetString("NSUserTrackingUsageDescription", TrackingDescription);

        // save
        File.WriteAllText(plistPath, plistObj.WriteToString());
    }

}
#endif