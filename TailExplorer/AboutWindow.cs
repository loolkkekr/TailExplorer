using UnityEngine;

namespace SceneExplorerMod
{
    public class AboutWindow
    {
        public Rect WindowRect = new Rect(Screen.width / 2 - 140, Screen.height / 2 - 10, 280, 205);

        public void Draw(int id)
        {
            GUI.backgroundColor = Color.white;
            WindowRect = GUI.Window(id, WindowRect, DrawContent, "", StyleManager.Window);
        }

        private void DrawContent(int id)
        {
            // --- ЗАГОЛОВОК ---
            GUILayout.BeginHorizontal();
            GUILayout.Label("ABOUT", StyleManager.Label);
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            // --- БЛОК ИНФОРМАЦИИ ---
            GUI.backgroundColor = StyleManager.Colors.HeaderBg;
            GUILayout.BeginVertical("box");
            GUI.backgroundColor = Color.white;
            {
                GUILayout.Space(5);

                // Логотип
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label("TAIL", StyleManager.LogoBold, GUILayout.Width(45));
                GUILayout.Label("EXPLORER", StyleManager.LogoLight);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                // Версия
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUIStyle dimLabel = new GUIStyle(StyleManager.Label);
                dimLabel.normal.textColor = StyleManager.Colors.TextDim;
                dimLabel.fontSize = 12;
                dimLabel.alignment = TextAnchor.MiddleCenter;
                GUILayout.Label($"v{Main.Instance.Info.Version}", dimLabel);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.Space(5);

                // Автор
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUIStyle authorStyle = new GUIStyle(StyleManager.Label);
                authorStyle.alignment = TextAnchor.MiddleCenter;
                GUILayout.Label("Created by loolkkekr", authorStyle);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.Space(10);
            }
            GUILayout.EndVertical();

            GUILayout.Space(15);

            // --- ИКОНКИ СОЦ СЕТЕЙ (Стилизованные) ---
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            float btnSize = 40f; // Размер кнопок
            float spacing = 5f;  // Отступ

            // Youtube
            if (DrawStyledIconButton(StyleManager.IconYoutube, btnSize))
            {
                Application.OpenURL("https://www.youtube.com/@loolkkekr");
            }
            GUILayout.Space(spacing);

            // Discord
            if (DrawStyledIconButton(StyleManager.IconDiscord, btnSize))
            {
                Application.OpenURL("https://discord.gg/pAwJf7WYsG");
            }
            GUILayout.Space(spacing);

            // Github
            if (DrawStyledIconButton(StyleManager.IconGithub, btnSize))
            {
                Application.OpenURL("https://github.com/loolkkekr/TailExplorer");
            }
            GUILayout.Space(spacing);

            // Gamebanana
            if (DrawStyledIconButton(StyleManager.IconGamebanana, btnSize))
            {
                Application.OpenURL("https://gamebanana.com/mods/642044");
            }

            // Gamejolt
            //if (DrawStyledIconButton(StyleManager.IconGamejolt, btnSize))
            //{
            //    Application.OpenURL("https://gamejolt.com/");
            //}

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUI.DragWindow(new Rect(0, 0, WindowRect.width, 30));
        }

        private bool DrawStyledIconButton(Texture2D icon, float size)
        {
            // Создаем копию стиля ButtonMenu, чтобы изменить высоту только для этих кнопок
            GUIStyle iconStyle = new GUIStyle(StyleManager.ButtonMenu);

            // В ButtonMenu стоит fixedHeight = 24, это сплющит иконку. 
            // Ставим 0, чтобы кнопка слушалась GUILayout.Height(size)
            iconStyle.fixedHeight = 0;

            // Если нужно поправить положение картинки внутри кнопки
            iconStyle.padding = new RectOffset(4, 4, 4, 4);
            iconStyle.imagePosition = ImagePosition.ImageOnly; // Рисовать только картинку

            GUIContent content = icon ? new GUIContent(icon) : new GUIContent("?");

            // Рисуем кнопку с нашим исправленным темным стилем
            return GUILayout.Button(content, iconStyle, GUILayout.Width(size), GUILayout.Height(size));
        }
    }
}