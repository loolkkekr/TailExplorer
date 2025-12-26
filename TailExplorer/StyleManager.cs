using System.IO;
using System.Reflection;
using UnityEngine;

namespace SceneExplorerMod
{
    public static class StyleManager
    {
        public static bool IsInitialized = false;

        // --- Styles ---
        public static GUIStyle Window, ButtonMenu, Label, Row, Checkbox, Arrow, ContextButton, TextField;
        public static GUIStyle DropdownBtn, LoaderBtn;
        public static GUIStyle TaskbarBtn, LogoBold, LogoLight, Clock;

        // --- Icons ---
        public static Texture2D IconYoutube;
        public static Texture2D IconDiscord;
        public static Texture2D IconGithub;
        public static Texture2D IconGamebanana;

        // --- Textures ---
        public static Texture2D TexWindow, TexHighlight, TexAccent, TexEmpty, TexContext, TexHover, TexBorder;
        public static Texture2D TexScrollTrack, TexScrollThumb, TexScrollThumbHover;
        public static Texture2D TexTaskbarBg, TexSeparator;

        // --- Colors ---
        public static class Colors
        {
            public static Color WindowBg = new Color(0.11f, 0.11f, 0.11f, 1f);
            public static Color HeaderBg = new Color(0.18f, 0.18f, 0.18f, 1f);
            public static Color ContextBg = new Color(0.15f, 0.15f, 0.15f, 1f);
            public static Color ContextBorder = new Color(0.35f, 0.35f, 0.35f, 1f);
            public static Color Accent = new Color(0.0f, 0.55f, 1.0f, 1f);
            public static Color SelectionBg = new Color(0.0f, 0.55f, 1.0f, 0.25f);
            public static Color Text = new Color(0.92f, 0.92f, 0.92f, 1f);
            public static Color TextDim = new Color(0.6f, 0.6f, 0.6f, 1f);
            public static Color CheckboxEmpty = new Color(0.2f, 0.2f, 0.2f, 1f);
            public static Color ButtonHover = new Color(1f, 1f, 1f, 0.05f);
            public static Color ScrollTrack = new Color(0.14f, 0.14f, 0.14f, 1f);
            public static Color ScrollThumb = new Color(0.3f, 0.3f, 0.3f, 1f);
            public static Color ScrollThumbHover = new Color(0.4f, 0.4f, 0.4f, 1f);
            public static Color TaskbarBg = new Color(0.05f, 0.05f, 0.05f, 0.98f);
            public static Color Separator = new Color(0.3f, 0.3f, 0.3f, 1f);
        }

        private static Texture2D LoadEmbeddedImage(string resourceName)
        {
            Texture2D tex = new Texture2D(2, 2);
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    byte[] buffer = new byte[stream.Length];
                    stream.Read(buffer, 0, buffer.Length);
                    tex.LoadImage(buffer);
                    tex.filterMode = FilterMode.Bilinear;
                    return tex;
                }
                else
                {
                    MelonLoader.MelonLogger.Error($"Failed to load texture: {resourceName}");
                    return null;
                }
            }
        }

        public static void Init()
        {
            if (IsInitialized) return;

            // --- ЗАГРУЗКА ИКОНОК (Убедитесь, что имена совпадают с вашими ресурсами!) ---
            IconYoutube = LoadEmbeddedImage("TailExplorer.Resources.youtube.png");
            IconDiscord = LoadEmbeddedImage("TailExplorer.Resources.discord.png");
            IconGithub = LoadEmbeddedImage("TailExplorer.Resources.github.png");
            IconGamebanana = LoadEmbeddedImage("TailExplorer.Resources.gamebanana.png");

            TexWindow = MakeTex(Colors.WindowBg);
            TexHighlight = MakeTex(Colors.SelectionBg);
            TexAccent = MakeTex(Colors.Accent);
            TexEmpty = MakeTex(Colors.CheckboxEmpty);
            TexContext = MakeTex(Colors.ContextBg);
            TexBorder = MakeTex(Colors.ContextBorder);
            TexHover = MakeTex(Colors.ButtonHover);
            TexScrollTrack = MakeTex(Colors.ScrollTrack);
            TexScrollThumb = MakeTex(Colors.ScrollThumb);
            TexScrollThumbHover = MakeTex(Colors.ScrollThumbHover);
            TexTaskbarBg = MakeTex(Colors.TaskbarBg);
            TexSeparator = MakeTex(Colors.Separator);

            Window = new GUIStyle(GUI.skin.window);
            Window.normal.background = TexWindow;
            Window.onNormal.background = TexWindow;
            Window.padding = new RectOffset(10, 10, 10, 10);

            ButtonMenu = new GUIStyle(GUI.skin.button);
            ButtonMenu.normal.background = TexEmpty;
            ButtonMenu.normal.textColor = Colors.Text;
            ButtonMenu.hover.background = TexHighlight;
            ButtonMenu.active.background = TexAccent;
            ButtonMenu.alignment = TextAnchor.MiddleCenter;
            ButtonMenu.fontSize = 12;
            ButtonMenu.fixedHeight = 24;
            ButtonMenu.border = new RectOffset(0, 0, 0, 0);

            Label = new GUIStyle(GUI.skin.label);
            Label.normal.textColor = Colors.Text;
            Label.alignment = TextAnchor.MiddleLeft;
            Label.padding = new RectOffset(2, 0, 0, 0);

            TextField = new GUIStyle(GUI.skin.textField);
            TextField.normal.textColor = Color.white;
            TextField.normal.background = TexAccent;
            TextField.focused.background = TexAccent;
            TextField.alignment = TextAnchor.MiddleLeft;

            Row = new GUIStyle();
            Row.fixedHeight = 22;
            Row.padding = new RectOffset(2, 2, 2, 2);

            Arrow = new GUIStyle(GUI.skin.label);
            Arrow.normal.textColor = Color.white;
            Arrow.alignment = TextAnchor.MiddleCenter;
            Arrow.fixedWidth = 20;
            Arrow.fontSize = 10;

            Checkbox = new GUIStyle(GUI.skin.box);
            Checkbox.border = new RectOffset(0, 0, 0, 0);
            Checkbox.normal.background = Texture2D.whiteTexture;

            ContextButton = new GUIStyle(GUI.skin.button);
            ContextButton.normal.background = TexContext;
            ContextButton.normal.textColor = Colors.Text;
            ContextButton.hover.background = TexHover;
            ContextButton.hover.textColor = Color.white;
            ContextButton.active.background = TexAccent;
            ContextButton.alignment = TextAnchor.MiddleLeft;
            ContextButton.padding = new RectOffset(10, 0, 0, 0);
            ContextButton.border = new RectOffset(0, 0, 0, 0);

            LogoBold = new GUIStyle(GUI.skin.label);
            LogoBold.normal.textColor = Color.white;
            LogoBold.fontStyle = FontStyle.Bold;
            LogoBold.fontSize = 18;
            LogoBold.alignment = TextAnchor.MiddleRight;

            LogoLight = new GUIStyle(GUI.skin.label);
            LogoLight.normal.textColor = Colors.Accent;
            LogoLight.fontStyle = FontStyle.Normal;
            LogoLight.fontSize = 18;
            LogoLight.alignment = TextAnchor.MiddleLeft;

            TaskbarBtn = new GUIStyle(GUI.skin.button);
            TaskbarBtn.normal.background = Texture2D.blackTexture;
            TaskbarBtn.hover.background = Texture2D.blackTexture;
            TaskbarBtn.active.background = Texture2D.blackTexture;
            TaskbarBtn.alignment = TextAnchor.MiddleCenter;
            TaskbarBtn.border = new RectOffset(0, 0, 0, 0);
            TaskbarBtn.fontSize = 13;
            TaskbarBtn.fontStyle = FontStyle.Bold;
            TaskbarBtn.contentOffset = new Vector2(0, -3f);

            GUIStyle scrollBar = new GUIStyle(GUI.skin.verticalScrollbar);
            scrollBar.normal.background = TexScrollTrack;
            scrollBar.fixedWidth = 10;
            scrollBar.border = new RectOffset(0, 0, 0, 0);

            GUIStyle scrollThumb = new GUIStyle(GUI.skin.verticalScrollbarThumb);
            scrollThumb.normal.background = TexScrollThumb;
            scrollThumb.hover.background = TexScrollThumbHover;
            scrollThumb.active.background = TexAccent;
            scrollThumb.border = new RectOffset(0, 0, 0, 0);

            GUI.skin.verticalScrollbar = scrollBar;
            GUI.skin.verticalScrollbarThumb = scrollThumb;
            GUI.skin.horizontalScrollbar = scrollBar;
            GUI.skin.horizontalScrollbarThumb = scrollThumb;

            Clock = new GUIStyle(GUI.skin.label);
            Clock.normal.textColor = Colors.Text;
            Clock.fontSize = 14;
            Clock.fontStyle = FontStyle.Normal;
            Clock.alignment = TextAnchor.MiddleRight;
            Clock.padding = new RectOffset(0, 5, 0, 0);

            DropdownBtn = new GUIStyle(GUI.skin.button);
            DropdownBtn.normal.background = TexContext;
            DropdownBtn.normal.textColor = Colors.Text;
            DropdownBtn.hover.background = TexHover;
            DropdownBtn.active.background = TexAccent;
            DropdownBtn.alignment = TextAnchor.MiddleLeft;
            DropdownBtn.padding = new RectOffset(10, 10, 5, 5);
            DropdownBtn.border = new RectOffset(0, 0, 0, 0);
            DropdownBtn.fixedHeight = 30;

            LoaderBtn = new GUIStyle(ButtonMenu);
            LoaderBtn.fixedHeight = 28;
            LoaderBtn.fontSize = 11;

            IsInitialized = true;
        }

        private static Texture2D MakeTex(Color col)
        {
            Texture2D result = new Texture2D(1, 1);
            result.SetPixel(0, 0, col);
            result.Apply();
            return result;
        }
    }
}