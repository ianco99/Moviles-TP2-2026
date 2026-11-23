using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;

namespace GunJump.EditorTools
{
    public static class OptimizationTools
    {
        const string ART_FOLDER = "Assets/_Game/Art";
        const string ATLAS_FOLDER_NAME = "Atlases";
        const string UI_ATLAS_PATH = ART_FOLDER + "/" + ATLAS_FOLDER_NAME + "/UI.spriteatlasv2";
        const string ANDROID_PLATFORM = "Android";
        const int ATLAS_MAX_SIZE = 2048;
        const int ATLAS_PADDING = 4;
        const int ATLAS_COMPRESSION_QUALITY = 50;

        static readonly string[] UI_SPRITE_FOLDERS =
        {
            ART_FOLDER + "/Textures/Buttons",
            ART_FOLDER + "/Textures/Icons",
            ART_FOLDER + "/Textures/GunPreviews"
        };

        [MenuItem("GunJump/Optimization/Create UI Sprite Atlas")]
        public static void CreateUiSpriteAtlas()
        {
            if (File.Exists(UI_ATLAS_PATH))
            {
                if (!EditorUtility.DisplayDialog("GunJump", $"Ya existe {UI_ATLAS_PATH}. ¿Reemplazarlo?", "Reemplazar", "Cancelar"))
                    return;

                AssetDatabase.DeleteAsset(UI_ATLAS_PATH);
            }

            EditorSettings.spritePackerMode = SpritePackerMode.SpriteAtlasV2;

            List<Object> folders = new List<Object>();
            foreach (string folder in UI_SPRITE_FOLDERS)
            {
                DefaultAsset folderAsset = AssetDatabase.LoadAssetAtPath<DefaultAsset>(folder);
                if (folderAsset == null)
                {
                    Debug.LogWarning($"GunJump: no existe {folder}; queda fuera del atlas de UI.");
                    continue;
                }

                folders.Add(folderAsset);
            }

            if (!AssetDatabase.IsValidFolder(ART_FOLDER + "/" + ATLAS_FOLDER_NAME))
                AssetDatabase.CreateFolder(ART_FOLDER, ATLAS_FOLDER_NAME);

            SpriteAtlasAsset atlas = new SpriteAtlasAsset();
            atlas.Add(folders.ToArray());
            SpriteAtlasAsset.Save(atlas, UI_ATLAS_PATH);
            AssetDatabase.ImportAsset(UI_ATLAS_PATH);

            SpriteAtlasImporter importer = (SpriteAtlasImporter)AssetImporter.GetAtPath(UI_ATLAS_PATH);
            importer.includeInBuild = true;
            importer.packingSettings = new SpriteAtlasPackingSettings
            {
                blockOffset = 1,
                padding = ATLAS_PADDING,
                enableRotation = false,
                enableTightPacking = false
            };
            importer.textureSettings = new SpriteAtlasTextureSettings
            {
                readable = false,
                generateMipMaps = false,
                sRGB = true,
                filterMode = FilterMode.Bilinear,
                anisoLevel = 1
            };
            importer.SetPlatformSettings(new TextureImporterPlatformSettings
            {
                name = ANDROID_PLATFORM,
                overridden = true,
                maxTextureSize = ATLAS_MAX_SIZE,
                format = TextureImporterFormat.ASTC_6x6,
                compressionQuality = ATLAS_COMPRESSION_QUALITY
            });
            importer.SaveAndReimport();

            Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(UI_ATLAS_PATH);
            Debug.Log($"GunJump: atlas {UI_ATLAS_PATH} creado con {folders.Count} carpetas de sprites de UI; Sprite Packer en Sprite Atlas V2.");
        }
    }
}
