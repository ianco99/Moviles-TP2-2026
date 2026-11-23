using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace GunJump.EditorTools
{
    public static class ProjectSetup
    {
        const int MIN_SDK = 26;
        const int TARGET_SDK = 36;

        const string ICON_SOURCE_PATH = "Assets/Proyecto nuevo (20).png";
        const string BRANDING_FOLDER_PARENT = "Assets/_Game/Art";
        const string BRANDING_FOLDER_NAME = "Branding";
        const string ICON_BACKGROUND_PATH = BRANDING_FOLDER_PARENT + "/" + BRANDING_FOLDER_NAME + "/AppIconBackground.png";
        const string ICON_FOREGROUND_PATH = BRANDING_FOLDER_PARENT + "/" + BRANDING_FOLDER_NAME + "/AppIconForeground.png";
        const int ADAPTIVE_ICON_SIZE = 432;
        const float ADAPTIVE_FOREGROUND_SCALE = 0.66f;
        const int ADAPTIVE_LAYER_COUNT = 2;

        static readonly Color ICON_BACKGROUND_COLOR = new Color(0.137f, 0.122f, 0.125f, 1f);

        [MenuItem("GunJump/Apply Player Settings")]
        public static void ApplyPlayerSettings()
        {
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.Android.minSdkVersion = (AndroidSdkVersions)MIN_SDK;
            PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)TARGET_SDK;
            PlayerSettings.SetStaticBatchingForPlatform(BuildTarget.Android, true);
            PlayerSettings.SetDynamicBatchingForPlatform(BuildTarget.Android, true);

            int pipelines = 0;
            foreach (string guid in AssetDatabase.FindAssets("t:UniversalRenderPipelineAsset"))
            {
                UniversalRenderPipelineAsset pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(AssetDatabase.GUIDToAssetPath(guid));
                pipeline.supportsDynamicBatching = true;
                EditorUtility.SetDirty(pipeline);
                pipelines++;
            }

            AssetDatabase.SaveAssets();

            Debug.Log($"GunJump: orientación vertical fija, SDK mínimo {MIN_SDK}, target SDK {TARGET_SDK}, " +
                      $"static y dynamic batching en Android y dynamic batching en {pipelines} URP Assets.");
        }

        [MenuItem("GunJump/Apply App Icon")]
        public static void ApplyAppIcon()
        {
            Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(ICON_SOURCE_PATH);
            if (icon == null)
            {
                EditorUtility.DisplayDialog("GunJump", $"No existe {ICON_SOURCE_PATH}: es la imagen del ícono.", "OK");
                return;
            }

            WriteAdaptiveLayers();
            Texture2D background = AssetDatabase.LoadAssetAtPath<Texture2D>(ICON_BACKGROUND_PATH);
            Texture2D foreground = AssetDatabase.LoadAssetAtPath<Texture2D>(ICON_FOREGROUND_PATH);

            PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new[] { icon }, IconKind.Any);

            int iconCount = 0;
            foreach (PlatformIconKind kind in PlayerSettings.GetSupportedIconKinds(NamedBuildTarget.Android))
            {
                PlatformIcon[] icons = PlayerSettings.GetPlatformIcons(NamedBuildTarget.Android, kind);

                foreach (PlatformIcon platformIcon in icons)
                {
                    if (platformIcon.maxLayerCount >= ADAPTIVE_LAYER_COUNT)
                        platformIcon.SetTextures(background, foreground);
                    else
                        platformIcon.SetTexture(icon);

                    iconCount++;
                }

                PlayerSettings.SetPlatformIcons(NamedBuildTarget.Android, kind, icons);
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"GunJump: ícono de la app desde {ICON_SOURCE_PATH} en {iconCount} tamaños de Android (legacy, redondo y adaptativo).");
        }

        static void WriteAdaptiveLayers()
        {
            if (!AssetDatabase.IsValidFolder(BRANDING_FOLDER_PARENT + "/" + BRANDING_FOLDER_NAME))
                AssetDatabase.CreateFolder(BRANDING_FOLDER_PARENT, BRANDING_FOLDER_NAME);

            Texture2D source = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            source.LoadImage(File.ReadAllBytes(ICON_SOURCE_PATH));

            Texture2D background = new Texture2D(ADAPTIVE_ICON_SIZE, ADAPTIVE_ICON_SIZE, TextureFormat.RGBA32, false);
            Texture2D foreground = new Texture2D(ADAPTIVE_ICON_SIZE, ADAPTIVE_ICON_SIZE, TextureFormat.RGBA32, false);
            Color[] backgroundPixels = new Color[ADAPTIVE_ICON_SIZE * ADAPTIVE_ICON_SIZE];
            Color[] foregroundPixels = new Color[ADAPTIVE_ICON_SIZE * ADAPTIVE_ICON_SIZE];

            float imageSize = ADAPTIVE_ICON_SIZE * ADAPTIVE_FOREGROUND_SCALE;
            float imageStart = (ADAPTIVE_ICON_SIZE - imageSize) * 0.5f;

            for (int y = 0; y < ADAPTIVE_ICON_SIZE; y++)
            {
                for (int x = 0; x < ADAPTIVE_ICON_SIZE; x++)
                {
                    int index = y * ADAPTIVE_ICON_SIZE + x;
                    backgroundPixels[index] = ICON_BACKGROUND_COLOR;

                    float u = (x + 0.5f - imageStart) / imageSize;
                    float v = (y + 0.5f - imageStart) / imageSize;
                    bool isInsideImage = u >= 0f && u <= 1f && v >= 0f && v <= 1f;
                    foregroundPixels[index] = isInsideImage ? source.GetPixelBilinear(u, v) : Color.clear;
                }
            }

            background.SetPixels(backgroundPixels);
            foreground.SetPixels(foregroundPixels);
            File.WriteAllBytes(ICON_BACKGROUND_PATH, background.EncodeToPNG());
            File.WriteAllBytes(ICON_FOREGROUND_PATH, foreground.EncodeToPNG());

            Object.DestroyImmediate(source);
            Object.DestroyImmediate(background);
            Object.DestroyImmediate(foreground);

            AssetDatabase.ImportAsset(ICON_BACKGROUND_PATH);
            AssetDatabase.ImportAsset(ICON_FOREGROUND_PATH);
        }
    }
}
