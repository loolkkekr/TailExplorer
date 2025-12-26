using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(SceneExplorerMod.Main), "TailExplorer", "1.1.0", "loolkkekr")]
[assembly: MelonGame(null, null)]

namespace SceneExplorerMod
{
    public class Main : MelonMod
    {
        public static Main Instance { get; private set; }

        // --- Preferences ---
        public static MelonPreferences_Category PrefsCategory;
        public static MelonPreferences_Entry<bool> PrefAutoOpen;
        public static MelonPreferences_Entry<bool> PrefForceCursor;
        public static MelonPreferences_Entry<KeyCode> PrefMenuKey;

        // --- UI Global State ---
        private bool _isOverlayActive = false;

        // Public properties to be accessed by Taskbar
        public bool ShowExplorerWindow = true;
        public bool ShowSettingsWindow = false;
        public bool ShowAboutWindow = false; // Новое окно

        // --- Components ---
        private Taskbar _taskbar;
        private ExplorerWindow _explorerWindow;
        private SettingsWindow _settingsWindow;
        private AboutWindow _aboutWindow; // Компонент нового окна

        // --- Cursor State ---
        private CursorLockMode _originalLockMode;
        private bool _originalVisible;
        private bool _hasAutoOpened = false;

        public override void OnInitializeMelon()
        {
            Instance = this;

            // Инициализация настроек MelonLoader
            PrefsCategory = MelonPreferences.CreateCategory("TailExplorer");
            PrefAutoOpen = PrefsCategory.CreateEntry("AutoOpen", false, "Open automatically on game start");
            PrefForceCursor = PrefsCategory.CreateEntry("ForceCursor", true, "Force cursor unlock when menu is open");
            PrefMenuKey = PrefsCategory.CreateEntry("MenuKey", KeyCode.F7, "Menu Toggle Key");

            _taskbar = new Taskbar();
            _explorerWindow = new ExplorerWindow();
            _settingsWindow = new SettingsWindow();
            _aboutWindow = new AboutWindow(); // Инициализация
        }

        public override void OnUpdate()
        {
            // Проверка автозапуска
            if (!_hasAutoOpened && Time.time > 1f)
            {
                _hasAutoOpened = true;
                if (PrefAutoOpen.Value)
                {
                    ToggleInterface(true);
                }
            }

            // Открытие меню (блокируем, если идет ребинд клавиш)
            if (Input.GetKeyDown(PrefMenuKey.Value) && !_settingsWindow.IsRebinding)
            {
                ToggleInterface();
            }

            // Логика курсора
            if (_isOverlayActive)
            {
                if (PrefForceCursor.Value)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
            }
        }

        public void ToggleInterface(bool forceOpen = false)
        {
            if (forceOpen) _isOverlayActive = true;
            else _isOverlayActive = !_isOverlayActive;

            if (_isOverlayActive)
            {
                _originalLockMode = Cursor.lockState;
                _originalVisible = Cursor.visible;
            }
            else
            {
                // При закрытии восстанавливаем как было
                Cursor.lockState = _originalLockMode;
                Cursor.visible = _originalVisible;
            }
        }

        public override void OnGUI()
        {
            if (!_isOverlayActive) return;

            StyleManager.Init();

            _taskbar.Draw();

            if (ShowExplorerWindow)
            {
                _explorerWindow.Draw(0);
            }

            if (ShowSettingsWindow)
            {
                _settingsWindow.Draw(1);
            }

            if (ShowAboutWindow)
            {
                _aboutWindow.Draw(2); // ID окна = 2
            }
        }
    }
}