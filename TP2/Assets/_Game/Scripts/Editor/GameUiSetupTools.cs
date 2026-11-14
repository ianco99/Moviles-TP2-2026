using System;
using System.IO;
using GunJump.Core;
using GunJump.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace GunJump.EditorTools
{
    // Arma, con el mismo arte del menú principal, los botones de efectos y música y el botón de volver
    // al menú de los niveles, y los pone en MainMenu y en Prototype. Solo crea lo que falta: los íconos,
    // los prefabs y las escenas que ya los tienen quedan como estén, así se pueden retocar a mano.
    public static class GameUiSetupTools
    {
        const string Root = "Assets/_Game";
        const string IconsFolder = Root + "/Art/Textures/Icons";
        const string SfxIconPath = IconsFolder + "/icon_sfx.png";
        const string MusicIconPath = IconsFolder + "/icon_music.png";
        const string RoundButtonPath = Root + "/Art/Textures/Buttons/button_round_flat.png";
        const string WideButtonPath = Root + "/Art/Textures/Buttons/button_rectangle_gloss.png";
        const string BackArrowPath = Root + "/Art/Textures/Buttons/arrow_decorative_w 1.png";
        const string FontPath = Root + "/Art/Fonts/LuckiestGuy-Regular SDF.asset";
        const string ButtonAnimatorPath = Root + "/Art/Anims/UI/Button.controller";
        const string PanelSpriteName = "UI/Skin/Background.psd";

        const string PrefabFolder = Root + "/Prefabs/UI";
        const string AudioTogglesPath = PrefabFolder + "/AudioToggles.prefab";
        const string LevelMenuPath = PrefabFolder + "/LevelMenu.prefab";
        const string MainMenuScenePath = GameSceneBuilder.ScenesFolder + "/MainMenu.unity";
        const string LevelScenePath = GameSceneBuilder.ScenesFolder + "/Prototype.unity";

        // Las mismas medidas que el botón de volver de la tienda.
        const float CornerButtonSize = 100f;
        const float CornerMargin = 50f;
        const float CornerButtonGap = 20f;
        const float ToggleIconSize = 60f;
        const float BackArrowScale = 0.7f;

        // Con el alto como referencia, igual que el canvas del menú.
        const float CanvasMatchHeight = 1f;

        const int IconSize = 128;
        const int IconSamplesPerAxis = 4;

        static readonly Vector2 SlashSize = new Vector2(86f, 12f);
        static readonly Color SlashColor = new Color(0.85f, 0.16f, 0.16f);
        static readonly Color PanelColor = new Color(0.3137255f, 0.32156864f, 0.33333334f);
        static readonly Color ConfirmColor = new Color(0.1942594f, 1f, 0f);
        static readonly Color CancelColor = new Color(1f, 0.84357387f, 0f);

        static readonly Vector2 PanelSize = new Vector2(820f, 600f);
        static readonly Vector2 TitlePosition = new Vector2(0f, 140f);
        static readonly Vector2 TitleSize = new Vector2(720f, 200f);
        static readonly Vector2 ChoiceSize = new Vector2(340f, 170f);
        static readonly Vector2 ChoiceLabelPadding = new Vector2(70f, 50f);
        const float ChoiceOffsetX = 185f;
        const float ChoiceOffsetY = -140f;
        const float TitleFontSize = 96f;
        const float ChoiceFontSize = 110f;
        const float MinFontSize = 18f;

        [MenuItem("GunJump/UI/Setup Menu Buttons")]
        public static void Setup()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("GunJump", "Salí de Play antes de correr Setup Menu Buttons.", "OK");
                return;
            }

            Sprite sfxIcon = EnsureIcon(SfxIconPath, IsInsideSpeaker);
            Sprite musicIcon = EnsureIcon(MusicIconPath, IsInsideNote);

            GameObject audioToggles = EnsureAudioTogglesPrefab(sfxIcon, musicIcon);
            GameObject levelMenu = EnsureLevelMenuPrefab(audioToggles);
            AssetDatabase.SaveAssets();

            string menu = AddTogglesToMainMenu(audioToggles);
            string level = AddMenuToLevel(levelMenu);
            Debug.Log($"GunJump: botones de audio y de volver al menú listos. MainMenu: {menu} Prototype: {level}");
        }

        // ---------------------------------------------------------------- Escenas

        // Los botones van en la esquina de arriba a la derecha, así que el contador de monedas se corre a su izquierda.
        static string AddTogglesToMainMenu(GameObject audioToggles)
        {
            if (!File.Exists(MainMenuScenePath))
                return "no existe la escena.";

            Scene scene = Open(MainMenuScenePath, out bool wasOpen);
            string result;

            if (FindInScene<AudioToggleButton>(scene) != null)
            {
                result = "ya tenía los botones de audio.";
            }
            else
            {
                MoneyCountController money = FindInScene<MoneyCountController>(scene);
                if (money == null)
                {
                    result = "no tiene MoneyCountController: no se sabe dónde poner los botones.";
                }
                else
                {
                    var serialized = new SerializedObject(money);
                    var moneyRect = (RectTransform)serialized.FindProperty("moneyCount").objectReferenceValue;

                    // Último hijo: se dibuja encima de todas las pantallas del menú.
                    PrefabUtility.InstantiatePrefab(audioToggles, moneyRect.parent);

                    SerializedProperty margin = serialized.FindProperty("screenMargin");
                    Vector2 value = margin.vector2Value;
                    value.x = CornerMargin + 2f * (CornerButtonSize + CornerButtonGap);
                    margin.vector2Value = value;
                    serialized.ApplyModifiedPropertiesWithoutUndo();

                    Save(scene);
                    result = "botones de audio agregados y contador de monedas corrido a su izquierda.";
                }
            }

            Close(scene, wasOpen);
            return result;
        }

        static string AddMenuToLevel(GameObject levelMenu)
        {
            if (!File.Exists(LevelScenePath))
                return "no existe la escena.";

            Scene scene = Open(LevelScenePath, out bool wasOpen);
            string result;

            if (FindInScene<BackToMenuPrompt>(scene) != null)
            {
                result = "ya tenía el menú del nivel.";
            }
            else
            {
                PrefabUtility.InstantiatePrefab(levelMenu, scene);

                if (FindInScene<EventSystem>(scene) == null)
                {
                    EventSystem eventSystem = UiFactory.CreateEventSystem();
                    SceneManager.MoveGameObjectToScene(eventSystem.gameObject, scene);
                }

                Save(scene);
                result = "menú del nivel agregado.";
            }

            Close(scene, wasOpen);
            return result;
        }

        static Scene Open(string path, out bool wasOpen)
        {
            Scene scene = SceneManager.GetSceneByPath(path);
            wasOpen = scene.IsValid() && scene.isLoaded;
            return wasOpen ? scene : EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
        }

        static void Save(Scene scene)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        static void Close(Scene scene, bool wasOpen)
        {
            if (!wasOpen)
                EditorSceneManager.CloseScene(scene, true);
        }

        static T FindInScene<T>(Scene scene) where T : Component
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                T found = root.GetComponentInChildren<T>(true);
                if (found != null)
                    return found;
            }

            return null;
        }

        // ---------------------------------------------------------------- Prefabs

        // Un panel ajustado al área segura con los dos botones en su esquina de arriba a la derecha:
        // efectos en la esquina y música a su izquierda. Sirve igual en el menú y en los niveles.
        static GameObject EnsureAudioTogglesPrefab(Sprite sfxIcon, Sprite musicIcon)
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(AudioTogglesPath);
            if (existing != null)
                return existing;

            var root = new GameObject("AudioToggles", typeof(RectTransform));
            var rect = (RectTransform)root.transform;
            UiFactory.Stretch(rect);
            root.AddComponent<SafeAreaFitter>();

            CreateToggle("SfxButton", rect, AudioChannel.Sfx, sfxIcon, 0);
            CreateToggle("MusicButton", rect, AudioChannel.Music, musicIcon, 1);

            return SavePrefab(root, AudioTogglesPath);
        }

        static void CreateToggle(string name, Transform parent, AudioChannel channel, Sprite iconSprite, int slot)
        {
            float x = -(CornerMargin + slot * (CornerButtonSize + CornerButtonGap));
            Button button = CreateArtButton(name, parent, Load<Sprite>(RoundButtonPath), Color.white);
            UiFactory.Place((RectTransform)button.transform, UiFactory.TopRight, new Vector2(x, -CornerMargin),
                new Vector2(CornerButtonSize, CornerButtonSize));

            Image icon = UiFactory.CreateImage("Icon", button.transform, Color.white, false);
            icon.sprite = iconSprite;
            icon.preserveAspect = true;
            UiFactory.Place(icon.rectTransform, UiFactory.Center, Vector2.zero, new Vector2(ToggleIconSize, ToggleIconSize));

            Image slash = UiFactory.CreateImage("OffMark", button.transform, SlashColor, false);
            UiFactory.Place(slash.rectTransform, UiFactory.Center, Vector2.zero, SlashSize);
            slash.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 45f);
            slash.gameObject.SetActive(false);

            var toggle = button.gameObject.AddComponent<AudioToggleButton>();
            var serialized = new SerializedObject(toggle);
            serialized.FindProperty("channel").intValue = (int)channel;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            GameSceneBuilder.SetReference(toggle, "icon", icon);
            GameSceneBuilder.SetReference(toggle, "offMark", slash.gameObject);
        }

        // Lo que se le agrega a cada escena de nivel: botón de volver, botones de audio y el panel que
        // pregunta antes de salir al menú.
        static GameObject EnsureLevelMenuPrefab(GameObject audioToggles)
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(LevelMenuPath);
            if (existing != null)
                return existing;

            GameObject canvas = UiFactory.CreateCanvas("LevelMenu", null);
            canvas.GetComponent<CanvasScaler>().matchWidthOrHeight = CanvasMatchHeight;

            RectTransform safeArea = UiFactory.CreateSafeArea(canvas.transform);
            Button back = CreateArtButton("BackButton", safeArea, Load<Sprite>(RoundButtonPath), Color.white);
            UiFactory.Place((RectTransform)back.transform, UiFactory.TopLeft, new Vector2(CornerMargin, -CornerMargin),
                new Vector2(CornerButtonSize, CornerButtonSize));

            Image arrow = UiFactory.CreateImage("Arrow", back.transform, Color.white, false);
            arrow.sprite = Load<Sprite>(BackArrowPath);
            UiFactory.Stretch(arrow.rectTransform);
            arrow.rectTransform.localScale = Vector3.one * BackArrowScale;

            PrefabUtility.InstantiatePrefab(audioToggles, canvas.transform);

            // Fuera del área segura: el fondo oscuro llega hasta los bordes de la pantalla.
            GameObject overlay = UiFactory.CreateOverlay("ConfirmPanel", canvas.transform);

            Image box = UiFactory.CreateImage("Box", overlay.transform, PanelColor, false);
            box.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(PanelSpriteName);
            box.type = Image.Type.Sliced;
            UiFactory.Place(box.rectTransform, UiFactory.Center, Vector2.zero, PanelSize);

            TMP_Text title = CreateMenuLabel("Title", box.transform, "BACK TO MENU?", TitleFontSize);
            UiFactory.Place(title.rectTransform, UiFactory.Center, TitlePosition, TitleSize);

            Button confirm = CreateChoice("YesButton", box.transform, "YES", ConfirmColor, -ChoiceOffsetX);
            Button cancel = CreateChoice("NoButton", box.transform, "NO", CancelColor, ChoiceOffsetX);

            var prompt = canvas.AddComponent<BackToMenuPrompt>();
            GameSceneBuilder.SetReference(prompt, "openButton", back);
            GameSceneBuilder.SetReference(prompt, "panel", overlay);
            GameSceneBuilder.SetReference(prompt, "confirmButton", confirm);
            GameSceneBuilder.SetReference(prompt, "cancelButton", cancel);
            overlay.SetActive(false);

            return SavePrefab(canvas, LevelMenuPath);
        }

        static Button CreateChoice(string name, Transform parent, string text, Color tint, float offsetX)
        {
            Button button = CreateArtButton(name, parent, Load<Sprite>(WideButtonPath), tint);
            UiFactory.Place((RectTransform)button.transform, UiFactory.Center, new Vector2(offsetX, ChoiceOffsetY), ChoiceSize);

            TMP_Text label = CreateMenuLabel("Label", button.transform, text, ChoiceFontSize);
            UiFactory.Stretch(label.rectTransform);
            label.rectTransform.sizeDelta = -ChoiceLabelPadding;
            return button;
        }

        // Un botón como los del menú: sprite del pack, y la animación de Button.controller en vez de un tinte.
        // El Animator corre con tiempo sin escalar para que el botón responda con el juego congelado.
        static Button CreateArtButton(string name, Transform parent, Sprite sprite, Color tint)
        {
            Image image = UiFactory.CreateImage(name, parent, tint, true);
            image.sprite = sprite;

            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.Animation;

            var animator = image.gameObject.AddComponent<Animator>();
            animator.runtimeAnimatorController = Load<RuntimeAnimatorController>(ButtonAnimatorPath);
            animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            return button;
        }

        // Texto con la fuente del menú, que se achica si no entra en su rectángulo.
        static TMP_Text CreateMenuLabel(string name, Transform parent, string text, float maxFontSize)
        {
            TMP_Text label = UiFactory.CreateLabel(name, parent, text, maxFontSize, TextAlignmentOptions.Center, Color.white);
            label.font = Load<TMP_FontAsset>(FontPath);
            label.enableAutoSizing = true;
            label.fontSizeMin = MinFontSize;
            label.fontSizeMax = maxFontSize;
            return label;
        }

        static GameObject SavePrefab(GameObject root, string path)
        {
            if (!AssetDatabase.IsValidFolder(PrefabFolder))
                AssetDatabase.CreateFolder(Path.GetDirectoryName(PrefabFolder).Replace('\\', '/'), Path.GetFileName(PrefabFolder));

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab;
        }

        static T Load<T>(string path) where T : Object
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
                throw new InvalidOperationException("No se pudo cargar " + path);

            return asset;
        }

        // ---------------------------------------------------------------- Íconos
        // Blancos sobre transparente, como la flecha del pack. Cada forma se describe con una función que dice
        // si un punto del cuadrado de lado 1 (y hacia arriba) cae adentro; el borde se suaviza tomando varias
        // muestras por píxel.

        static Sprite EnsureIcon(string path, Func<float, float, bool> isInside)
        {
            if (!File.Exists(path))
            {
                var texture = new Texture2D(IconSize, IconSize, TextureFormat.RGBA32, false);
                var pixels = new Color32[IconSize * IconSize];
                int samples = IconSamplesPerAxis * IconSamplesPerAxis;

                for (int y = 0; y < IconSize; y++)
                {
                    for (int x = 0; x < IconSize; x++)
                    {
                        int hits = 0;
                        for (int sy = 0; sy < IconSamplesPerAxis; sy++)
                        {
                            for (int sx = 0; sx < IconSamplesPerAxis; sx++)
                            {
                                float u = (x + (sx + 0.5f) / IconSamplesPerAxis) / IconSize;
                                float v = (y + (sy + 0.5f) / IconSamplesPerAxis) / IconSize;
                                if (isInside(u, v))
                                    hits++;
                            }
                        }

                        pixels[y * IconSize + x] = new Color32(255, 255, 255, (byte)(255 * hits / samples));
                    }
                }

                texture.SetPixels32(pixels);
                File.WriteAllBytes(path, texture.EncodeToPNG());
                Object.DestroyImmediate(texture);
                AssetDatabase.ImportAsset(path);
            }

            if (AssetImporter.GetAtPath(path) is TextureImporter importer && importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
            }

            return Load<Sprite>(path);
        }

        // Parlante mirando a la derecha, con dos ondas.
        static bool IsInsideSpeaker(float x, float y)
        {
            const float centerY = 0.5f;
            const float boxLeft = 0.12f;
            const float coneLeft = 0.3f;
            const float coneRight = 0.52f;
            const float boxHalfHeight = 0.13f;
            const float coneHalfHeight = 0.33f;
            const float waveCenterX = 0.5f;
            const float waveHalfThickness = 0.032f;
            const float waveHalfAngle = 42f;
            const float innerWave = 0.2f;
            const float outerWave = 0.34f;

            float dy = Mathf.Abs(y - centerY);

            if (x >= boxLeft && x <= coneLeft && dy <= boxHalfHeight)
                return true;

            if (x >= coneLeft && x <= coneRight)
            {
                float halfHeight = Mathf.Lerp(boxHalfHeight, coneHalfHeight, (x - coneLeft) / (coneRight - coneLeft));
                if (dy <= halfHeight)
                    return true;
            }

            float dx = x - waveCenterX;
            if (dx <= 0f || Mathf.Atan2(dy, dx) * Mathf.Rad2Deg > waveHalfAngle)
                return false;

            float distance = Mathf.Sqrt(dx * dx + dy * dy);
            return Mathf.Abs(distance - innerWave) <= waveHalfThickness
                   || Mathf.Abs(distance - outerWave) <= waveHalfThickness;
        }

        // Dos corcheas unidas por una barra.
        static bool IsInsideNote(float x, float y)
        {
            const float stemWidth = 0.06f;
            const float leftStemX = 0.34f;
            const float rightStemX = 0.74f;
            const float leftHeadY = 0.2f;
            const float rightHeadY = 0.28f;
            const float leftTopY = 0.8f;
            const float rightTopY = 0.88f;
            const float beamThickness = 0.13f;
            // Cuánto queda el centro de la cabeza a la izquierda del borde derecho de su palito.
            const float headInset = 0.12f;

            if (IsInsideHead(x, y, leftStemX + stemWidth - headInset, leftHeadY)
                || IsInsideHead(x, y, rightStemX + stemWidth - headInset, rightHeadY))
                return true;

            if (x >= leftStemX && x <= leftStemX + stemWidth && y >= leftHeadY && y <= leftTopY)
                return true;

            if (x >= rightStemX && x <= rightStemX + stemWidth && y >= rightHeadY && y <= rightTopY)
                return true;

            if (x < leftStemX || x > rightStemX + stemWidth)
                return false;

            float beamTop = Mathf.Lerp(leftTopY, rightTopY, (x - leftStemX) / (rightStemX + stemWidth - leftStemX));
            return y <= beamTop && y >= beamTop - beamThickness;
        }

        // La cabeza de la nota: una elipse inclinada.
        static bool IsInsideHead(float x, float y, float centerX, float centerY)
        {
            const float radiusX = 0.13f;
            const float radiusY = 0.095f;
            const float tilt = 20f * Mathf.Deg2Rad;

            float dx = x - centerX;
            float dy = y - centerY;
            float alongX = (dx * Mathf.Cos(tilt) + dy * Mathf.Sin(tilt)) / radiusX;
            float alongY = (-dx * Mathf.Sin(tilt) + dy * Mathf.Cos(tilt)) / radiusY;
            return alongX * alongX + alongY * alongY <= 1f;
        }
    }
}
