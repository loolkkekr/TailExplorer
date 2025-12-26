using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(SceneExplorerMod.Main), "TailExplorer", "1.1.1", "loolkkekr")]
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
        public static MelonPreferences_Entry<bool> PrefShowSnow;

        // --- UI Global State ---
        private bool _isOverlayActive = false;

        // Public properties
        public bool ShowExplorerWindow = true;
        public bool ShowSettingsWindow = false;
        public bool ShowAboutWindow = false;

        // --- Components ---
        private Taskbar _taskbar;
        private ExplorerWindow _explorerWindow;
        private SettingsWindow _settingsWindow;
        private AboutWindow _aboutWindow;

        // Ссылка на систему снега (может быть null, если не сезон)
        private SnowSystem _snowSystem;

        // --- Optimized Holiday Check ---
        // Статическая переменная, вычисляется один раз при старте
        public static bool IsHolidaySeason = false;

        // --- Cursor State ---
        private CursorLockMode _originalLockMode;
        private bool _originalVisible;
        private bool _hasAutoOpened = false;

        public override void OnInitializeMelon()
        {
            Instance = this;

            // 1. Проверяем дату ОДИН РАЗ при загрузке мода
            DateTime d = DateTime.Now;
            IsHolidaySeason = (d.Month == 12 && d.Day >= 24) || (d.Month == 1 && d.Day <= 2);

            PrefsCategory = MelonPreferences.CreateCategory("TailExplorer");
            PrefAutoOpen = PrefsCategory.CreateEntry("AutoOpen", false, "Open automatically on game start");
            PrefForceCursor = PrefsCategory.CreateEntry("ForceCursor", true, "Force cursor unlock when menu is open");
            PrefMenuKey = PrefsCategory.CreateEntry("MenuKey", KeyCode.F7, "Menu Toggle Key");
            PrefShowSnow = PrefsCategory.CreateEntry("ShowSnow", true, "Enable holiday snow effect");

            _taskbar = new Taskbar();
            _explorerWindow = new ExplorerWindow();
            _settingsWindow = new SettingsWindow();
            _aboutWindow = new AboutWindow();

            // 2. Инициализируем снег ТОЛЬКО если сейчас праздники
            if (IsHolidaySeason)
            {
                _snowSystem = new SnowSystem();
                MelonLogger.Msg("Happy Christmas! :D");
            }
        }

        public override void OnUpdate()
        {
            if (!_hasAutoOpened && Time.time > 1f)
            {
                _hasAutoOpened = true;
                if (PrefAutoOpen.Value) ToggleInterface(true);
            }

            bool isJustRebound = (Time.unscaledTime - _settingsWindow.LastRebindTime) < 0.25f;

            if (Input.GetKeyDown(PrefMenuKey.Value) && !_settingsWindow.IsRebinding && !isJustRebound)
            {
                ToggleInterface();
            }

            if (_isOverlayActive)
            {
                if (PrefForceCursor.Value)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }

                // 3. Обновляем снег, только если система существует (не null) и включена в настройках
                // Проверка на null очень быстрая, нагрузки на CPU нет
                if (_snowSystem != null && PrefShowSnow.Value)
                {
                    _snowSystem.Update();
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
                Cursor.lockState = _originalLockMode;
                Cursor.visible = _originalVisible;
            }
        }

        public override void OnGUI()
        {
            if (!_isOverlayActive) return;

            StyleManager.Init();

            // 4. Рисуем снег, только если он существует
            if (_snowSystem != null && PrefShowSnow.Value)
            {
                _snowSystem.Draw();
            }

            _taskbar.Draw();

            if (ShowExplorerWindow) _explorerWindow.Draw(0);
            if (ShowSettingsWindow) _settingsWindow.Draw(1);
            if (ShowAboutWindow) _aboutWindow.Draw(2);
        }
    }
}