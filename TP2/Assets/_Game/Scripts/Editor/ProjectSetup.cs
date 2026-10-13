using UnityEditor;
using UnityEngine;

namespace GunJump.EditorTools
{
    public static class ProjectSetup
    {
        // VibrationEffect existe desde la API 26; Google Play exige apuntar a la 36 en apps nuevas.
        const int MinSdk = 26;
        const int TargetSdk = 36;

        [MenuItem("GunJump/Apply Player Settings")]
        public static void ApplyPlayerSettings()
        {
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.Android.minSdkVersion = (AndroidSdkVersions)MinSdk;
            PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)TargetSdk;
            AssetDatabase.SaveAssets();

            Debug.Log($"GunJump: orientación vertical fija, SDK mínimo {MinSdk}, target SDK {TargetSdk}.");
        }
    }
}
