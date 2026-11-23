using System;
using System.IO;
using GunJump.Core;
using GunJump.Platform;
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
    public static class GameUiSetupTools
    {
        const string ROOT = "Assets/_Game";
        const string ICONS_FOLDER = ROOT + "/Art/Textures/Icons";
        const string SFX_ICON_PATH = ICONS_FOLDER + "/icon_sfx.png";
        const string MUSIC_ICON_PATH = ICONS_FOLDER + "/icon_music.png";
        const string ROUND_BUTTON_PATH = ROOT + "/Art/Textures/Buttons/button_round_flat.png";
        const string WIDE_BUTTON_PATH = ROOT + "/Art/Textures/Buttons/button_rectangle_gloss.png";
        const string BACK_ARROW_PATH = ROOT + "/Art/Textures/Buttons/arrow_decorative_w 1.png";
        const string FONT_PATH = ROOT + "/Art/Fonts/LuckiestGuy-Regular SDF.asset";
        const string BUTTON_ANIMATOR_PATH = ROOT + "/Art/Anims/UI/Button.controller";
        const string PANEL_SPRITE_NAME = "UI/Skin/Background.psd";

        const string PREFAB_FOLDER = ROOT + "/Prefabs/UI";
        const string AUDIO_TOGGLES_PATH = PREFAB_FOLDER + "/AudioToggles.prefab";
        const string LEVEL_MENU_PATH = PREFAB_FOLDER + "/LevelMenu.prefab";
        const string MAIN_MENU_SCENE_PATH = GameSceneBuilder.SCENES_FOLDER + "/MainMenu.unity";
        const string LEVEL_SCENE_PATH = GameSceneBuilder.SCENES_FOLDER + "/Prototype.unity";

        const float CORNER_BUTTON_SIZE = 100f;
        const float CORNER_MARGIN = 50f;
        const float CORNER_BUTTON_GAP = 20f;
        const float TOGGLE_ICON_SIZE = 60f;
        const float BACK_ARROW_SCALE = 0.7f;

        const float CANVAS_MATCH_HEIGHT = 1f;

        const int ICON_SIZE = 128;
        const int ICON_SAMPLES_PER_AXIS = 4;

        static readonly Vector2 SLASH_SIZE = new Vector2(86f, 12f);
        static readonly Color SLASH_COLOR = new Color(0.85f, 0.16f, 0.16f);
        static readonly Color PANEL_COLOR = new Color(0.3137255f, 0.32156864f, 0.33333334f);
        static readonly Color CONFIRM_COLOR = new Color(0.1942594f, 1f, 0f);
        static readonly Color CANCEL_COLOR = new Color(1f, 0.84357387f, 0f);

        static readonly Vector2 PANEL_SIZE = new Vector2(820f, 600f);
        static readonly Vector2 TITLE_POSITION = new Vector2(0f, 140f);
        static readonly Vector2 TITLE_SIZE = new Vector2(720f, 200f);
        static readonly Vector2 CHOICE_SIZE = new Vector2(340f, 170f);
        static readonly Vector2 CHOICE_LABEL_PADDING = new Vector2(70f, 50f);
        const float CHOICE_OFFSET_X = 185f;
        const float CHOICE_OFFSET_Y = -140f;
        const float TITLE_FONT_SIZE = 96f;
        const float CHOICE_FONT_SIZE = 110f;
        const float MIN_FONT_SIZE = 18f;

        const string LOGS_SCREEN_NAME = "LogsScreen";
        const string LOGS_BUTTON_AREA_NAME = "LogsButtonArea";
        const float LOGS_BUTTON_FONT_SIZE = 40f;
        const float LOGS_TITLE_TOP = 40f;
        const float LOGS_PANEL_PADDING = 20f;
        const float LOGS_TEXT_FONT_SIZE = 30f;
        const float LOGS_STATUS_FONT_SIZE = 34f;
        const float LOGS_STATUS_Y = 250f;
        const float LOGS_ROW_Y = 70f;
        const float LOGS_SCROLL_SENSITIVITY = 30f;

        const string VIBRATION_ICON_PATH = ICONS_FOLDER + "/icon_vibration.png";
        const string HAPTICS_TUNING_PATH = ROOT + "/Data/Tuning/HapticsTuning.asset";
        const string PLAY_GAMES_AREA_NAME = "PlayGamesButtonArea";
        const string QUIT_PANEL_NAME = "QuitPanel";
        const string LEVEL_CORE_PREFAB_PATH = ROOT + "/Prefabs/Level/LevelCore.prefab";
        const string PAUSE_BUTTON_NAME = "PauseButton";
        const string RESUME_BUTTON_NAME = "ResumeButton";
        const string NEXT_BUTTON_NAME = "NextButton";
        const string RETRY_BUTTON_NAME = "RetryButton";
        const string BOOTSTRAPPER_TYPE_NAME = "Bootstrapper";
        const int LOGS_SLOT = 0;
        const int ACHIEVEMENTS_SLOT = 1;
        const int LEADERBOARD_SLOT = 2;
        const int VIBRATION_SLOT = 2;
        const int TOGGLE_COUNT = 3;

        const string BUTTON_PREFAB_PATH = ROOT + "/Prefabs/Menu/Button.prefab";
        const string LEVEL_BUTTON_PATH = ROOT + "/Prefabs/Runtime/LevelButton.prefab";
        const string LEVEL_STARS_NAME = "Stars";
        const string LEVELS_BACK_AREA_NAME = "LevelsBackArea";
        const float LEVEL_STAR_SIZE = 26f;
        const float LEVEL_STAR_SPACING = 42f;
        const float LEVEL_STARS_GAP = 12f;

        static readonly Vector2 TOP_CENTER = new Vector2(0.5f, 1f);
        static readonly Vector2 BOTTOM_CENTER = new Vector2(0.5f, 0f);
        static readonly Vector2 LOGS_BUTTON_LABEL_PADDING = new Vector2(24f, 24f);
        static readonly Vector2 LOGS_TITLE_SIZE = new Vector2(600f, 140f);
        static readonly Vector2 LOGS_PANEL_OFFSET_MIN = new Vector2(40f, 380f);
        static readonly Vector2 LOGS_PANEL_OFFSET_MAX = new Vector2(-40f, -200f);
        static readonly Vector2 LOGS_STATUS_SIZE = new Vector2(1000f, 110f);
        static readonly Vector2 LOGS_ROW_BUTTON_SIZE = new Vector2(310f, 160f);
        static readonly Color LOGS_BACKGROUND_COLOR = new Color(0.12f, 0.12f, 0.14f, 1f);
        static readonly Color LOGS_PANEL_COLOR = new Color(0.06f, 0.06f, 0.07f, 1f);
        static readonly Vector2 LEVEL_STARS_SIZE = new Vector2(130f, 40f);
        static readonly HapticPulse[] DEFAULT_PULSES = { HapticPulse.Shot, HapticPulse.Kill, HapticPulse.Multiplier };
        static readonly long[] DEFAULT_PULSE_MILLISECONDS = { 15, 35, 70 };
        static readonly int[] DEFAULT_PULSE_AMPLITUDES = { 80, 180, 255 };

        [MenuItem("GunJump/UI/Setup Menu Buttons")]
        public static void Setup()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("GunJump", "Salí de Play antes de correr Setup Menu Buttons.", "OK");
                return;
            }

            Sprite sfxIcon = EnsureIcon(SFX_ICON_PATH, IsInsideSpeaker);
            Sprite musicIcon = EnsureIcon(MUSIC_ICON_PATH, IsInsideNote);

            GameObject audioToggles = EnsureAudioTogglesPrefab(sfxIcon, musicIcon);
            GameObject levelMenu = EnsureLevelMenuPrefab(audioToggles);
            AssetDatabase.SaveAssets();

            string menu = AddTogglesToMainMenu(audioToggles);
            string level = AddMenuToLevel(levelMenu);
            Debug.Log($"GunJump: botones de audio y de volver al menú listos. MainMenu: {menu} Prototype: {level}");
        }

        [MenuItem("GunJump/UI/Setup Logs Screen")]
        public static void SetupLogsScreen()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("GunJump", "Salí de Play antes de correr Setup Logs Screen.", "OK");
                return;
            }

            if (!File.Exists(MAIN_MENU_SCENE_PATH))
            {
                EditorUtility.DisplayDialog("GunJump", $"No existe {MAIN_MENU_SCENE_PATH}.", "OK");
                return;
            }

            Scene scene = Open(MAIN_MENU_SCENE_PATH, out bool wasOpen);
            string result = AddLogsScreen(scene);
            Close(scene, wasOpen);
            Debug.Log($"GunJump: {result}");
        }

        [MenuItem("GunJump/UI/Restyle Level UI")]
        public static void RestyleLevelUi()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("GunJump", "Salí de Play antes de correr Restyle Level UI.", "OK");
                return;
            }

            if (!File.Exists(LEVEL_CORE_PREFAB_PATH))
            {
                EditorUtility.DisplayDialog("GunJump", $"No existe {LEVEL_CORE_PREFAB_PATH}.", "OK");
                return;
            }

            GameObject contents = PrefabUtility.LoadPrefabContents(LEVEL_CORE_PREFAB_PATH);
            int buttons = ApplyMenuStyle(contents);
            PrefabUtility.SaveAsPrefabAsset(contents, LEVEL_CORE_PREFAB_PATH);
            PrefabUtility.UnloadPrefabContents(contents);
            Debug.Log($"GunJump: UI del nivel con la fuente y los botones del menú ({buttons} botones) en {LEVEL_CORE_PREFAB_PATH}.");
        }

        internal static int ApplyMenuStyle(GameObject levelCoreRoot)
        {
            TMP_FontAsset font = Load<TMP_FontAsset>(FONT_PATH);
            Sprite roundSprite = Load<Sprite>(ROUND_BUTTON_PATH);
            Sprite wideSprite = Load<Sprite>(WIDE_BUTTON_PATH);
            RuntimeAnimatorController buttonAnimator = Load<RuntimeAnimatorController>(BUTTON_ANIMATOR_PATH);

            foreach (TextMeshProUGUI label in levelCoreRoot.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                label.font = font;
            }

            CommandButton[] commandButtons = levelCoreRoot.GetComponentsInChildren<CommandButton>(true);
            foreach (CommandButton commandButton in commandButtons)
            {
                bool isPause = commandButton.name == PAUSE_BUTTON_NAME;

                Image image = commandButton.GetComponent<Image>();
                image.sprite = isPause ? roundSprite : wideSprite;
                image.type = Image.Type.Simple;
                image.color = TintForLevelButton(commandButton.name);

                Button button = commandButton.GetComponent<Button>();
                button.transition = Selectable.Transition.Animation;

                Animator animator = commandButton.GetComponent<Animator>();
                if (animator == null)
                    animator = commandButton.gameObject.AddComponent<Animator>();

                animator.runtimeAnimatorController = buttonAnimator;
                animator.updateMode = AnimatorUpdateMode.UnscaledTime;

                foreach (TMP_Text label in commandButton.GetComponentsInChildren<TMP_Text>(true))
                {
                    label.enableAutoSizing = true;
                    label.fontSizeMin = MIN_FONT_SIZE;
                    label.fontSizeMax = label.fontSize;
                    label.color = Color.white;
                }
            }

            return commandButtons.Length;
        }

        static Color TintForLevelButton(string buttonName)
        {
            if (buttonName == RESUME_BUTTON_NAME || buttonName == NEXT_BUTTON_NAME)
                return CONFIRM_COLOR;

            if (buttonName == RETRY_BUTTON_NAME)
                return CANCEL_COLOR;

            return Color.white;
        }

        [MenuItem("GunJump/UI/Setup Quit Prompt")]
        public static void SetupQuitPrompt()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("GunJump", "Salí de Play antes de correr Setup Quit Prompt.", "OK");
                return;
            }

            if (!File.Exists(MAIN_MENU_SCENE_PATH))
            {
                EditorUtility.DisplayDialog("GunJump", $"No existe {MAIN_MENU_SCENE_PATH}.", "OK");
                return;
            }

            Scene scene = Open(MAIN_MENU_SCENE_PATH, out bool wasOpen);
            string result = AddQuitPrompt(scene);
            Close(scene, wasOpen);
            Debug.Log($"GunJump: {result}");
        }

        static string AddQuitPrompt(Scene scene)
        {
            MainMenuController mainMenu = FindInScene<MainMenuController>(scene);
            if (mainMenu == null)
                return "MainMenu no tiene MainMenuController: no se armó el cartel de salir.";

            Transform oldPanel = mainMenu.transform.Find(QUIT_PANEL_NAME);
            if (oldPanel != null)
            {
                if (!EditorUtility.DisplayDialog("GunJump", "MainMenu ya tiene el cartel de salir. ¿Reconstruirlo?", "Reconstruir", "Dejarlo"))
                    return "el cartel de salir quedó como estaba.";

                Object.DestroyImmediate(oldPanel.gameObject);
            }

            GameObject overlay = UiFactory.CreateOverlay(QUIT_PANEL_NAME, mainMenu.transform);
            overlay.transform.SetAsLastSibling();

            Image box = UiFactory.CreateImage("Box", overlay.transform, PANEL_COLOR, false);
            box.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(PANEL_SPRITE_NAME);
            box.type = Image.Type.Sliced;
            UiFactory.Place(box.rectTransform, UiFactory.CENTER, Vector2.zero, PANEL_SIZE);

            TMP_Text title = CreateMenuLabel("Title", box.transform, "QUIT GAME?", TITLE_FONT_SIZE);
            UiFactory.Place(title.rectTransform, UiFactory.CENTER, TITLE_POSITION, TITLE_SIZE);

            Button confirm = CreateChoice("YesButton", box.transform, "YES", CONFIRM_COLOR, -CHOICE_OFFSET_X);
            Button cancel = CreateChoice("NoButton", box.transform, "NO", CANCEL_COLOR, CHOICE_OFFSET_X);

            GameSceneBuilder.SetReference(mainMenu, "quitPanel", overlay);
            GameSceneBuilder.SetReference(mainMenu, "quitConfirmButton", confirm);
            GameSceneBuilder.SetReference(mainMenu, "quitCancelButton", cancel);
            overlay.SetActive(false);

            Save(scene);
            return "cartel de salir agregado a MainMenu y conectado a MainMenuController.";
        }

        [MenuItem("GunJump/UI/Setup Play Games and Vibration")]
        public static void SetupPlayGamesAndVibration()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("GunJump", "Salí de Play antes de correr Setup Play Games and Vibration.", "OK");
                return;
            }

            if (!File.Exists(MAIN_MENU_SCENE_PATH) || !File.Exists(AUDIO_TOGGLES_PATH))
            {
                EditorUtility.DisplayDialog("GunJump", $"Faltan {MAIN_MENU_SCENE_PATH} o {AUDIO_TOGGLES_PATH}. Corré antes GunJump > UI > Setup Menu Buttons.", "OK");
                return;
            }

            HapticsTuning tuning = EnsureHapticsTuning();
            Sprite vibrationIcon = EnsureIcon(VIBRATION_ICON_PATH, IsInsidePhone);
            string toggles = AddVibrationToggle(vibrationIcon);
            AssetDatabase.SaveAssets();

            Scene scene = Open(MAIN_MENU_SCENE_PATH, out bool wasOpen);
            string menu = AddPlayGamesToMainMenu(scene, tuning);
            Close(scene, wasOpen);
            Debug.Log($"GunJump: {toggles} {menu}");
        }

        static HapticsTuning EnsureHapticsTuning()
        {
            HapticsTuning existing = AssetDatabase.LoadAssetAtPath<HapticsTuning>(HAPTICS_TUNING_PATH);
            if (existing != null)
                return existing;

            HapticsTuning tuning = ScriptableObject.CreateInstance<HapticsTuning>();
            AssetDatabase.CreateAsset(tuning, HAPTICS_TUNING_PATH);

            SerializedObject serialized = new SerializedObject(tuning);
            SerializedProperty pulses = serialized.FindProperty("pulses");
            pulses.arraySize = DEFAULT_PULSES.Length;

            for (int i = 0; i < DEFAULT_PULSES.Length; i++)
            {
                SerializedProperty entry = pulses.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("pulse").enumValueIndex = (int)DEFAULT_PULSES[i];
                entry.FindPropertyRelative("milliseconds").longValue = DEFAULT_PULSE_MILLISECONDS[i];
                entry.FindPropertyRelative("amplitude").intValue = DEFAULT_PULSE_AMPLITUDES[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            return tuning;
        }

        static string AddVibrationToggle(Sprite vibrationIcon)
        {
            GameObject contents = PrefabUtility.LoadPrefabContents(AUDIO_TOGGLES_PATH);
            bool hasToggle = contents.GetComponentInChildren<VibrationToggleButton>(true) != null;

            if (!hasToggle)
            {
                Button button = CreateToggleButton("VibrationButton", contents.transform, vibrationIcon, VIBRATION_SLOT, out Image icon, out GameObject offMark);
                VibrationToggleButton toggle = button.gameObject.AddComponent<VibrationToggleButton>();
                GameSceneBuilder.SetReference(toggle, "icon", icon);
                GameSceneBuilder.SetReference(toggle, "offMark", offMark);
                PrefabUtility.SaveAsPrefabAsset(contents, AUDIO_TOGGLES_PATH);
            }

            PrefabUtility.UnloadPrefabContents(contents);
            return hasToggle ? "AudioToggles ya tenía el botón de vibración." : "Botón de vibración agregado a AudioToggles.";
        }

        static string AddPlayGamesToMainMenu(Scene scene, HapticsTuning tuning)
        {
            MainMenuController mainMenu = FindInScene<MainMenuController>(scene);
            if (mainMenu == null)
                return "MainMenu no tiene MainMenuController: no se agregaron LOGROS ni TOP.";

            GameObject landing = new SerializedObject(mainMenu).FindProperty("landingMenu").objectReferenceValue as GameObject;
            if (landing == null)
                return "MainMenuController no tiene asignado landingMenu.";

            Transform oldArea = landing.transform.Find(PLAY_GAMES_AREA_NAME);
            if (oldArea != null)
            {
                if (!EditorUtility.DisplayDialog("GunJump", "MainMenu ya tiene los botones LOGROS y TOP. ¿Reconstruirlos?", "Reconstruir", "Dejarlos"))
                    return "LOGROS y TOP quedaron como estaban.";

                Object.DestroyImmediate(oldArea.gameObject);
            }

            RectTransform area = UiFactory.CreateSafeArea(landing.transform);
            area.name = PLAY_GAMES_AREA_NAME;
            Button achievements = CreateCornerTextButton("AchievementsButton", area, "AWARDS", ACHIEVEMENTS_SLOT);
            Button leaderboard = CreateCornerTextButton("LeaderboardButton", area, "TOP", LEADERBOARD_SLOT);
            GameSceneBuilder.SetReference(mainMenu, "achievementsButton", achievements);
            GameSceneBuilder.SetReference(mainMenu, "leaderboardButton", leaderboard);

            MoneyCountController money = FindInScene<MoneyCountController>(scene);
            if (money != null)
            {
                SerializedObject serialized = new SerializedObject(money);
                SerializedProperty margin = serialized.FindProperty("screenMargin");
                Vector2 value = margin.vector2Value;
                value.x = CORNER_MARGIN + TOGGLE_COUNT * (CORNER_BUTTON_SIZE + CORNER_BUTTON_GAP);
                margin.vector2Value = value;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }

            Component bootstrapper = FindComponentByTypeName(scene, BOOTSTRAPPER_TYPE_NAME);
            if (bootstrapper != null)
                GameSceneBuilder.SetReference(bootstrapper, "hapticsTuning", tuning);

            Save(scene);
            return bootstrapper != null
                ? "LOGROS y TOP agregados bajo LOGS, contador de monedas corrido y HapticsTuning asignado al Bootstrapper."
                : "LOGROS y TOP agregados, pero MainMenu no tiene Bootstrapper: asigná HapticsTuning a mano.";
        }

        static Button CreateCornerTextButton(string name, Transform parent, string text, int slot)
        {
            Button button = CreateArtButton(name, parent, Load<Sprite>(ROUND_BUTTON_PATH), Color.white);
            float y = -(CORNER_MARGIN + slot * (CORNER_BUTTON_SIZE + CORNER_BUTTON_GAP));
            UiFactory.Place((RectTransform)button.transform, UiFactory.TOP_LEFT, new Vector2(CORNER_MARGIN, y),
                new Vector2(CORNER_BUTTON_SIZE, CORNER_BUTTON_SIZE));

            TMP_Text label = CreateMenuLabel("Label", button.transform, text, LOGS_BUTTON_FONT_SIZE);
            UiFactory.Stretch(label.rectTransform);
            label.rectTransform.sizeDelta = -LOGS_BUTTON_LABEL_PADDING;
            return button;
        }

        static Component FindComponentByTypeName(Scene scene, string typeName)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (MonoBehaviour behaviour in root.GetComponentsInChildren<MonoBehaviour>(true))
                {
                    if (behaviour != null && behaviour.GetType().Name == typeName)
                        return behaviour;
                }
            }

            return null;
        }

        [MenuItem("GunJump/UI/Setup Level Select")]
        public static void SetupLevelSelect()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("GunJump", "Salí de Play antes de correr Setup Level Select.", "OK");
                return;
            }

            if (!File.Exists(MAIN_MENU_SCENE_PATH) || !File.Exists(BUTTON_PREFAB_PATH))
            {
                EditorUtility.DisplayDialog("GunJump", $"Faltan {MAIN_MENU_SCENE_PATH} o {BUTTON_PREFAB_PATH}.", "OK");
                return;
            }

            if (File.Exists(LEVEL_BUTTON_PATH)
                && !EditorUtility.DisplayDialog("GunJump", "La selección de nivel ya está armada. ¿Reconstruir el botón de nivel y el de volver?", "Reconstruir", "Cancelar"))
                return;

            BuildLevelButtonPrefab();

            Scene scene = Open(MAIN_MENU_SCENE_PATH, out bool wasOpen);
            string result = AddLevelSelectBack(scene);
            Close(scene, wasOpen);
            Debug.Log($"GunJump: {result}");
        }

        static void BuildLevelButtonPrefab()
        {
            if (!File.Exists(LEVEL_BUTTON_PATH))
            {
                string folder = Path.GetDirectoryName(LEVEL_BUTTON_PATH).Replace('\\', '/');
                if (!AssetDatabase.IsValidFolder(folder))
                    AssetDatabase.CreateFolder(Path.GetDirectoryName(folder).Replace('\\', '/'), Path.GetFileName(folder));

                AssetDatabase.CopyAsset(BUTTON_PREFAB_PATH, LEVEL_BUTTON_PATH);
            }

            GameObject contents = PrefabUtility.LoadPrefabContents(LEVEL_BUTTON_PATH);
            Button button = contents.GetComponent<Button>();
            TMP_Text numberLabel = contents.GetComponentInChildren<TMP_Text>(true);

            if (button == null || numberLabel == null)
            {
                PrefabUtility.UnloadPrefabContents(contents);
                throw new InvalidOperationException($"{BUTTON_PREFAB_PATH} tiene que tener un Button y un texto TMP para servir de botón de nivel.");
            }

            Transform oldStars = contents.transform.Find(LEVEL_STARS_NAME);
            if (oldStars != null)
                Object.DestroyImmediate(oldStars.gameObject);

            RectTransform starsRow = UiFactory.CreateRect(LEVEL_STARS_NAME, contents.transform);
            UiFactory.Place(starsRow, BOTTOM_CENTER, new Vector2(0f, -(LEVEL_STARS_SIZE.y + LEVEL_STARS_GAP)), LEVEL_STARS_SIZE);

            Object[] stars = new Object[3];
            for (int i = 0; i < stars.Length; i++)
            {
                Image star = UiFactory.CreateImage("Star" + (i + 1), starsRow, Color.white, false);
                UiFactory.Place(star.rectTransform, UiFactory.CENTER, new Vector2((i - 1) * LEVEL_STAR_SPACING, 0f), new Vector2(LEVEL_STAR_SIZE, LEVEL_STAR_SIZE));
                star.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 45f);
                stars[i] = star;
            }

            CanvasGroup canvasGroup = contents.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = contents.AddComponent<CanvasGroup>();

            LevelButtonView view = contents.GetComponent<LevelButtonView>();
            if (view == null)
                view = contents.AddComponent<LevelButtonView>();

            GameSceneBuilder.SetReference(view, "button", button);
            GameSceneBuilder.SetReference(view, "numberLabel", numberLabel);
            GameSceneBuilder.SetReferences(view, "stars", stars);
            GameSceneBuilder.SetReference(view, "canvasGroup", canvasGroup);

            PrefabUtility.SaveAsPrefabAsset(contents, LEVEL_BUTTON_PATH);
            PrefabUtility.UnloadPrefabContents(contents);
        }

        static string AddLevelSelectBack(Scene scene)
        {
            MainMenuController mainMenu = FindInScene<MainMenuController>(scene);
            LevelsMenuController levelsMenu = FindInScene<LevelsMenuController>(scene);
            if (mainMenu == null || levelsMenu == null)
                return "MainMenu no tiene MainMenuController o LevelsMenuController: no se armó la selección de nivel.";

            SerializedObject serializedLevels = new SerializedObject(levelsMenu);
            serializedLevels.FindProperty("levelButtonAssetName").stringValue = Path.GetFileNameWithoutExtension(LEVEL_BUTTON_PATH);
            serializedLevels.ApplyModifiedPropertiesWithoutUndo();

            Transform oldArea = levelsMenu.transform.Find(LEVELS_BACK_AREA_NAME);
            if (oldArea != null)
                Object.DestroyImmediate(oldArea.gameObject);

            RectTransform area = UiFactory.CreateSafeArea(levelsMenu.transform);
            area.name = LEVELS_BACK_AREA_NAME;
            Button backButton = CreateBackButton(area);

            GameSceneBuilder.SetReference(mainMenu, "levelsBackButton", backButton);

            Save(scene);
            return "botón de nivel con estrellas y botón de volver de la selección de nivel listos.";
        }

        static Button CreateBackButton(Transform parent)
        {
            Button backButton = CreateArtButton("BackButton", parent, Load<Sprite>(ROUND_BUTTON_PATH), Color.white);
            UiFactory.Place((RectTransform)backButton.transform, UiFactory.TOP_LEFT, new Vector2(CORNER_MARGIN, -CORNER_MARGIN),
                new Vector2(CORNER_BUTTON_SIZE, CORNER_BUTTON_SIZE));

            Image arrow = UiFactory.CreateImage("Arrow", backButton.transform, Color.white, false);
            arrow.sprite = Load<Sprite>(BACK_ARROW_PATH);
            UiFactory.Stretch(arrow.rectTransform);
            arrow.rectTransform.localScale = Vector3.one * BACK_ARROW_SCALE;
            return backButton;
        }

        static string AddLogsScreen(Scene scene)
        {
            MainMenuController mainMenu = FindInScene<MainMenuController>(scene);
            if (mainMenu == null)
                return "MainMenu no tiene MainMenuController: no se sabe dónde poner la pantalla de Logs.";

            SerializedObject serialized = new SerializedObject(mainMenu);
            GameObject landing = serialized.FindProperty("landingMenu").objectReferenceValue as GameObject;
            if (landing == null)
                return "MainMenuController no tiene asignado landingMenu.";

            LogsMenuController existing = FindInScene<LogsMenuController>(scene);
            if (existing != null)
            {
                if (!EditorUtility.DisplayDialog("GunJump", "MainMenu ya tiene la pantalla de Logs. ¿Reconstruirla?", "Reconstruir", "Cancelar"))
                    return "la pantalla de Logs ya existía y quedó como estaba.";

                Object.DestroyImmediate(existing.gameObject);
            }

            Transform oldButtonArea = landing.transform.Find(LOGS_BUTTON_AREA_NAME);
            if (oldButtonArea != null)
                Object.DestroyImmediate(oldButtonArea.gameObject);

            Button logsButton = CreateLogsButton(landing.transform);
            LogsMenuController logsMenu = CreateLogsScreen(mainMenu.transform, out Button backButton);

            GameSceneBuilder.SetReference(mainMenu, "logsButton", logsButton);
            GameSceneBuilder.SetReference(mainMenu, "logsBackButton", backButton);
            GameSceneBuilder.SetReference(mainMenu, "logsMenu", logsMenu);

            Save(scene);
            return "pantalla de Logs y botón LOGS agregados a MainMenu.";
        }

        static Button CreateLogsButton(Transform landing)
        {
            RectTransform area = UiFactory.CreateSafeArea(landing);
            area.name = LOGS_BUTTON_AREA_NAME;
            return CreateCornerTextButton("LogsButton", area, "LOGS", LOGS_SLOT);
        }

        static LogsMenuController CreateLogsScreen(Transform menu, out Button backButton)
        {
            Image screen = UiFactory.CreateImage(LOGS_SCREEN_NAME, menu, LOGS_BACKGROUND_COLOR, true);
            UiFactory.Stretch(screen.rectTransform);
            screen.transform.SetAsLastSibling();

            RectTransform safeArea = UiFactory.CreateSafeArea(screen.transform);

            backButton = CreateBackButton(safeArea);

            TMP_Text title = CreateMenuLabel("Title", safeArea, "LOGS", CHOICE_FONT_SIZE);
            UiFactory.Place(title.rectTransform, TOP_CENTER, new Vector2(0f, -LOGS_TITLE_TOP), LOGS_TITLE_SIZE);

            ScrollRect scrollRect = CreateLogPanel(safeArea, out TMP_Text logText);

            TMP_Text status = UiFactory.CreateLabel("Status", safeArea, string.Empty, LOGS_STATUS_FONT_SIZE, TextAlignmentOptions.Center, Color.white);
            status.textWrappingMode = TextWrappingModes.Normal;
            UiFactory.Place(status.rectTransform, BOTTOM_CENTER, new Vector2(0f, LOGS_STATUS_Y), LOGS_STATUS_SIZE);

            Button clearButton = CreateRowButton("ClearButton", safeArea, "CLEAR", CANCEL_COLOR, 0f, out _);

            LogsMenuController controller = screen.gameObject.AddComponent<LogsMenuController>();
            GameSceneBuilder.SetReference(controller, "logText", logText);
            GameSceneBuilder.SetReference(controller, "scrollRect", scrollRect);
            GameSceneBuilder.SetReference(controller, "clearButton", clearButton);
            GameSceneBuilder.SetReference(controller, "statusText", status);

            screen.gameObject.SetActive(false);
            return controller;
        }

        static ScrollRect CreateLogPanel(Transform parent, out TMP_Text logText)
        {
            Image panel = UiFactory.CreateImage("LogPanel", parent, LOGS_PANEL_COLOR, true);
            panel.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(PANEL_SPRITE_NAME);
            panel.type = Image.Type.Sliced;
            UiFactory.Stretch(panel.rectTransform);
            panel.rectTransform.offsetMin = LOGS_PANEL_OFFSET_MIN;
            panel.rectTransform.offsetMax = LOGS_PANEL_OFFSET_MAX;

            RectTransform viewport = UiFactory.CreateRect("Viewport", panel.transform);
            UiFactory.Stretch(viewport);
            viewport.offsetMin = Vector2.one * LOGS_PANEL_PADDING;
            viewport.offsetMax = -Vector2.one * LOGS_PANEL_PADDING;
            viewport.gameObject.AddComponent<RectMask2D>();

            logText = UiFactory.CreateLabel("LogText", viewport, string.Empty, LOGS_TEXT_FONT_SIZE, TextAlignmentOptions.TopLeft, Color.white);
            logText.richText = true;
            logText.textWrappingMode = TextWrappingModes.Normal;

            RectTransform content = logText.rectTransform;
            content.anchorMin = UiFactory.TOP_LEFT;
            content.anchorMax = Vector2.one;
            content.pivot = TOP_CENTER;
            content.offsetMin = Vector2.zero;
            content.offsetMax = Vector2.zero;

            ContentSizeFitter fitter = logText.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            ScrollRect scrollRect = panel.gameObject.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.viewport = viewport;
            scrollRect.content = content;
            scrollRect.scrollSensitivity = LOGS_SCROLL_SENSITIVITY;
            return scrollRect;
        }

        static Button CreateRowButton(string name, Transform parent, string text, Color tint, float offsetX, out TMP_Text label)
        {
            Button button = CreateArtButton(name, parent, Load<Sprite>(WIDE_BUTTON_PATH), tint);
            UiFactory.Place((RectTransform)button.transform, BOTTOM_CENTER, new Vector2(offsetX, LOGS_ROW_Y), LOGS_ROW_BUTTON_SIZE);

            label = CreateMenuLabel("Label", button.transform, text, CHOICE_FONT_SIZE);
            UiFactory.Stretch(label.rectTransform);
            label.rectTransform.sizeDelta = -CHOICE_LABEL_PADDING;
            return button;
        }

        static string AddTogglesToMainMenu(GameObject audioToggles)
        {
            if (!File.Exists(MAIN_MENU_SCENE_PATH))
                return "no existe la escena.";

            Scene scene = Open(MAIN_MENU_SCENE_PATH, out bool wasOpen);
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
                    SerializedObject serialized = new SerializedObject(money);
                    RectTransform moneyRect = (RectTransform)serialized.FindProperty("moneyCount").objectReferenceValue;

                    PrefabUtility.InstantiatePrefab(audioToggles, moneyRect.parent);

                    SerializedProperty margin = serialized.FindProperty("screenMargin");
                    Vector2 value = margin.vector2Value;
                    value.x = CORNER_MARGIN + 2f * (CORNER_BUTTON_SIZE + CORNER_BUTTON_GAP);
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
            if (!File.Exists(LEVEL_SCENE_PATH))
                return "no existe la escena.";

            Scene scene = Open(LEVEL_SCENE_PATH, out bool wasOpen);
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

        static ComponentType FindInScene<ComponentType>(Scene scene) where ComponentType : Component
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                ComponentType found = root.GetComponentInChildren<ComponentType>(true);
                if (found != null)
                    return found;
            }

            return null;
        }

        static GameObject EnsureAudioTogglesPrefab(Sprite sfxIcon, Sprite musicIcon)
        {
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(AUDIO_TOGGLES_PATH);
            if (existing != null)
                return existing;

            GameObject root = new GameObject("AudioToggles", typeof(RectTransform));
            RectTransform rect = (RectTransform)root.transform;
            UiFactory.Stretch(rect);
            root.AddComponent<SafeAreaFitter>();

            CreateToggle("SfxButton", rect, AudioChannel.Sfx, sfxIcon, 0);
            CreateToggle("MusicButton", rect, AudioChannel.Music, musicIcon, 1);

            return SavePrefab(root, AUDIO_TOGGLES_PATH);
        }

        static void CreateToggle(string name, Transform parent, AudioChannel channel, Sprite iconSprite, int slot)
        {
            Button button = CreateToggleButton(name, parent, iconSprite, slot, out Image icon, out GameObject offMark);

            AudioToggleButton toggle = button.gameObject.AddComponent<AudioToggleButton>();
            SerializedObject serialized = new SerializedObject(toggle);
            serialized.FindProperty("channel").intValue = (int)channel;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            GameSceneBuilder.SetReference(toggle, "icon", icon);
            GameSceneBuilder.SetReference(toggle, "offMark", offMark);
        }

        static Button CreateToggleButton(string name, Transform parent, Sprite iconSprite, int slot, out Image icon, out GameObject offMark)
        {
            float x = -(CORNER_MARGIN + slot * (CORNER_BUTTON_SIZE + CORNER_BUTTON_GAP));
            Button button = CreateArtButton(name, parent, Load<Sprite>(ROUND_BUTTON_PATH), Color.white);
            UiFactory.Place((RectTransform)button.transform, UiFactory.TOP_RIGHT, new Vector2(x, -CORNER_MARGIN),
                new Vector2(CORNER_BUTTON_SIZE, CORNER_BUTTON_SIZE));

            icon = UiFactory.CreateImage("Icon", button.transform, Color.white, false);
            icon.sprite = iconSprite;
            icon.preserveAspect = true;
            UiFactory.Place(icon.rectTransform, UiFactory.CENTER, Vector2.zero, new Vector2(TOGGLE_ICON_SIZE, TOGGLE_ICON_SIZE));

            Image slash = UiFactory.CreateImage("OffMark", button.transform, SLASH_COLOR, false);
            UiFactory.Place(slash.rectTransform, UiFactory.CENTER, Vector2.zero, SLASH_SIZE);
            slash.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 45f);
            slash.gameObject.SetActive(false);
            offMark = slash.gameObject;
            return button;
        }

        static GameObject EnsureLevelMenuPrefab(GameObject audioToggles)
        {
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(LEVEL_MENU_PATH);
            if (existing != null)
                return existing;

            GameObject canvas = UiFactory.CreateCanvas("LevelMenu", null);
            canvas.GetComponent<CanvasScaler>().matchWidthOrHeight = CANVAS_MATCH_HEIGHT;

            RectTransform safeArea = UiFactory.CreateSafeArea(canvas.transform);
            Button back = CreateArtButton("BackButton", safeArea, Load<Sprite>(ROUND_BUTTON_PATH), Color.white);
            UiFactory.Place((RectTransform)back.transform, UiFactory.TOP_LEFT, new Vector2(CORNER_MARGIN, -CORNER_MARGIN),
                new Vector2(CORNER_BUTTON_SIZE, CORNER_BUTTON_SIZE));

            Image arrow = UiFactory.CreateImage("Arrow", back.transform, Color.white, false);
            arrow.sprite = Load<Sprite>(BACK_ARROW_PATH);
            UiFactory.Stretch(arrow.rectTransform);
            arrow.rectTransform.localScale = Vector3.one * BACK_ARROW_SCALE;

            PrefabUtility.InstantiatePrefab(audioToggles, canvas.transform);

            GameObject overlay = UiFactory.CreateOverlay("ConfirmPanel", canvas.transform);

            Image box = UiFactory.CreateImage("Box", overlay.transform, PANEL_COLOR, false);
            box.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(PANEL_SPRITE_NAME);
            box.type = Image.Type.Sliced;
            UiFactory.Place(box.rectTransform, UiFactory.CENTER, Vector2.zero, PANEL_SIZE);

            TMP_Text title = CreateMenuLabel("Title", box.transform, "BACK TO MENU?", TITLE_FONT_SIZE);
            UiFactory.Place(title.rectTransform, UiFactory.CENTER, TITLE_POSITION, TITLE_SIZE);

            Button confirm = CreateChoice("YesButton", box.transform, "YES", CONFIRM_COLOR, -CHOICE_OFFSET_X);
            Button cancel = CreateChoice("NoButton", box.transform, "NO", CANCEL_COLOR, CHOICE_OFFSET_X);

            BackToMenuPrompt prompt = canvas.AddComponent<BackToMenuPrompt>();
            GameSceneBuilder.SetReference(prompt, "openButton", back);
            GameSceneBuilder.SetReference(prompt, "panel", overlay);
            GameSceneBuilder.SetReference(prompt, "confirmButton", confirm);
            GameSceneBuilder.SetReference(prompt, "cancelButton", cancel);
            overlay.SetActive(false);

            return SavePrefab(canvas, LEVEL_MENU_PATH);
        }

        static Button CreateChoice(string name, Transform parent, string text, Color tint, float offsetX)
        {
            Button button = CreateArtButton(name, parent, Load<Sprite>(WIDE_BUTTON_PATH), tint);
            UiFactory.Place((RectTransform)button.transform, UiFactory.CENTER, new Vector2(offsetX, CHOICE_OFFSET_Y), CHOICE_SIZE);

            TMP_Text label = CreateMenuLabel("Label", button.transform, text, CHOICE_FONT_SIZE);
            UiFactory.Stretch(label.rectTransform);
            label.rectTransform.sizeDelta = -CHOICE_LABEL_PADDING;
            return button;
        }

        static Button CreateArtButton(string name, Transform parent, Sprite sprite, Color tint)
        {
            Image image = UiFactory.CreateImage(name, parent, tint, true);
            image.sprite = sprite;

            Button button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.Animation;

            Animator animator = image.gameObject.AddComponent<Animator>();
            animator.runtimeAnimatorController = Load<RuntimeAnimatorController>(BUTTON_ANIMATOR_PATH);
            animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            return button;
        }

        static TMP_Text CreateMenuLabel(string name, Transform parent, string text, float maxFontSize)
        {
            TMP_Text label = UiFactory.CreateLabel(name, parent, text, maxFontSize, TextAlignmentOptions.Center, Color.white);
            label.font = Load<TMP_FontAsset>(FONT_PATH);
            label.enableAutoSizing = true;
            label.fontSizeMin = MIN_FONT_SIZE;
            label.fontSizeMax = maxFontSize;
            return label;
        }

        static GameObject SavePrefab(GameObject root, string path)
        {
            if (!AssetDatabase.IsValidFolder(PREFAB_FOLDER))
                AssetDatabase.CreateFolder(Path.GetDirectoryName(PREFAB_FOLDER).Replace('\\', '/'), Path.GetFileName(PREFAB_FOLDER));

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab;
        }

        static AssetType Load<AssetType>(string path) where AssetType : Object
        {
            AssetType asset = AssetDatabase.LoadAssetAtPath<AssetType>(path);
            if (asset == null)
                throw new InvalidOperationException("No se pudo cargar " + path);

            return asset;
        }

        static Sprite EnsureIcon(string path, Func<float, float, bool> isInside)
        {
            if (!File.Exists(path))
            {
                Texture2D texture = new Texture2D(ICON_SIZE, ICON_SIZE, TextureFormat.RGBA32, false);
                Color32[] pixels = new Color32[ICON_SIZE * ICON_SIZE];
                int samples = ICON_SAMPLES_PER_AXIS * ICON_SAMPLES_PER_AXIS;

                for (int y = 0; y < ICON_SIZE; y++)
                {
                    for (int x = 0; x < ICON_SIZE; x++)
                    {
                        int hits = 0;
                        for (int sy = 0; sy < ICON_SAMPLES_PER_AXIS; sy++)
                        {
                            for (int sx = 0; sx < ICON_SAMPLES_PER_AXIS; sx++)
                            {
                                float u = (x + (sx + 0.5f) / ICON_SAMPLES_PER_AXIS) / ICON_SIZE;
                                float v = (y + (sy + 0.5f) / ICON_SAMPLES_PER_AXIS) / ICON_SIZE;
                                if (isInside(u, v))
                                    hits++;
                            }
                        }

                        pixels[y * ICON_SIZE + x] = new Color32(255, 255, 255, (byte)(255 * hits / samples));
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

        static bool IsInsidePhone(float x, float y)
        {
            const float CENTER = 0.5f;
            const float BODY_HALF_WIDTH = 0.16f;
            const float BODY_HALF_HEIGHT = 0.32f;
            const float FRAME = 0.05f;
            const float MARK_HALF_WIDTH = 0.025f;
            const float INNER_MARK_X = 0.24f;
            const float OUTER_MARK_X = 0.33f;
            const float INNER_MARK_HALF_HEIGHT = 0.16f;
            const float OUTER_MARK_HALF_HEIGHT = 0.1f;

            float dx = Mathf.Abs(x - CENTER);
            float dy = Mathf.Abs(y - CENTER);

            bool isInsideBody = dx <= BODY_HALF_WIDTH && dy <= BODY_HALF_HEIGHT;
            bool isInsideScreen = dx <= BODY_HALF_WIDTH - FRAME && dy <= BODY_HALF_HEIGHT - FRAME;
            if (isInsideBody && !isInsideScreen)
                return true;

            if (Mathf.Abs(dx - INNER_MARK_X) <= MARK_HALF_WIDTH && dy <= INNER_MARK_HALF_HEIGHT)
                return true;

            return Mathf.Abs(dx - OUTER_MARK_X) <= MARK_HALF_WIDTH && dy <= OUTER_MARK_HALF_HEIGHT;
        }

        static bool IsInsideSpeaker(float x, float y)
        {
            const float CENTER_Y = 0.5f;
            const float BOX_LEFT = 0.12f;
            const float CONE_LEFT = 0.3f;
            const float CONE_RIGHT = 0.52f;
            const float BOX_HALF_HEIGHT = 0.13f;
            const float CONE_HALF_HEIGHT = 0.33f;
            const float WAVE_CENTER_X = 0.5f;
            const float WAVE_HALF_THICKNESS = 0.032f;
            const float WAVE_HALF_ANGLE = 42f;
            const float INNER_WAVE = 0.2f;
            const float OUTER_WAVE = 0.34f;

            float dy = Mathf.Abs(y - CENTER_Y);

            if (x >= BOX_LEFT && x <= CONE_LEFT && dy <= BOX_HALF_HEIGHT)
                return true;

            if (x >= CONE_LEFT && x <= CONE_RIGHT)
            {
                float halfHeight = Mathf.Lerp(BOX_HALF_HEIGHT, CONE_HALF_HEIGHT, (x - CONE_LEFT) / (CONE_RIGHT - CONE_LEFT));
                if (dy <= halfHeight)
                    return true;
            }

            float dx = x - WAVE_CENTER_X;
            if (dx <= 0f || Mathf.Atan2(dy, dx) * Mathf.Rad2Deg > WAVE_HALF_ANGLE)
                return false;

            float distance = Mathf.Sqrt(dx * dx + dy * dy);
            return Mathf.Abs(distance - INNER_WAVE) <= WAVE_HALF_THICKNESS
                   || Mathf.Abs(distance - OUTER_WAVE) <= WAVE_HALF_THICKNESS;
        }

        static bool IsInsideNote(float x, float y)
        {
            const float STEM_WIDTH = 0.06f;
            const float LEFT_STEM_X = 0.34f;
            const float RIGHT_STEM_X = 0.74f;
            const float LEFT_HEAD_Y = 0.2f;
            const float RIGHT_HEAD_Y = 0.28f;
            const float LEFT_TOP_Y = 0.8f;
            const float RIGHT_TOP_Y = 0.88f;
            const float BEAM_THICKNESS = 0.13f;
            const float HEAD_INSET = 0.12f;

            if (IsInsideHead(x, y, LEFT_STEM_X + STEM_WIDTH - HEAD_INSET, LEFT_HEAD_Y)
                || IsInsideHead(x, y, RIGHT_STEM_X + STEM_WIDTH - HEAD_INSET, RIGHT_HEAD_Y))
                return true;

            if (x >= LEFT_STEM_X && x <= LEFT_STEM_X + STEM_WIDTH && y >= LEFT_HEAD_Y && y <= LEFT_TOP_Y)
                return true;

            if (x >= RIGHT_STEM_X && x <= RIGHT_STEM_X + STEM_WIDTH && y >= RIGHT_HEAD_Y && y <= RIGHT_TOP_Y)
                return true;

            if (x < LEFT_STEM_X || x > RIGHT_STEM_X + STEM_WIDTH)
                return false;

            float beamTop = Mathf.Lerp(LEFT_TOP_Y, RIGHT_TOP_Y, (x - LEFT_STEM_X) / (RIGHT_STEM_X + STEM_WIDTH - LEFT_STEM_X));
            return y <= beamTop && y >= beamTop - BEAM_THICKNESS;
        }

        static bool IsInsideHead(float x, float y, float centerX, float centerY)
        {
            const float RADIUS_X = 0.13f;
            const float RADIUS_Y = 0.095f;
            const float TILT = 20f * Mathf.Deg2Rad;

            float dx = x - centerX;
            float dy = y - centerY;
            float alongX = (dx * Mathf.Cos(TILT) + dy * Mathf.Sin(TILT)) / RADIUS_X;
            float alongY = (-dx * Mathf.Sin(TILT) + dy * Mathf.Cos(TILT)) / RADIUS_Y;
            return alongX * alongX + alongY * alongY <= 1f;
        }
    }
}
