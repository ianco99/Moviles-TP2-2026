using UnityEditor;
using UnityEngine;

namespace GunJump.EditorTools
{
    public static class ProjectSetup
    {
        const int MIN_SDK = 26;
        const int TARGET_SDK = 36;

        [MenuItem("GunJump/Apply Player Settings")]
        public static void ApplyPlayerSettings()
        {
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.Android.minSdkVersion = (AndroidSdkVersions)MIN_SDK;
            PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)TARGET_SDK;
            AssetDatabase.SaveAssets();

            Debug.Log($"GunJump: orientación vertical fija, SDK mínimo {MIN_SDK}, target SDK {TARGET_SDK}.");
        }
    }
}
