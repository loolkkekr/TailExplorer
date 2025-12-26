using System;
using UnityEngine;

namespace SceneExplorerMod
{
    public class Taskbar
    {
        public void Draw()
        {
            float tbHeight = 46f;
            Rect tbRect = new Rect(0, 0, Screen.width, tbHeight);

            GUI.DrawTexture(tbRect, StyleManager.TexTaskbarBg);
            GUI.DrawTexture(new Rect(0, tbHeight - 1, Screen.width, 1), StyleManager.TexBorder);

            GUILayout.BeginArea(new Rect(20, 0, Screen.width - 40, tbHeight));
            GUILayout.BeginHorizontal();

            // LOGO
            GUILayout.Space(5);
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.Label("TAIL", StyleManager.LogoBold, GUILayout.Width(45));
            GUILayout.Label("EXPLORER", StyleManager.LogoLight);
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();

            GUILayout.Space(25);
            GUI.DrawTexture(new Rect(175, 12, 1, 22), StyleManager.TexSeparator);
            GUILayout.Space(25);

            // BUTTONS
            if (DrawModernTaskbarButton("EXPLORER", Main.Instance.ShowExplorerWindow, tbHeight))
            {
                Main.Instance.ShowExplorerWindow = !Main.Instance.ShowExplorerWindow;
                if (Main.Instance.ShowExplorerWindow) GUI.BringWindowToFront(0);
            }

            GUILayout.Space(5);

            if (DrawModernTaskbarButton("SETTINGS", Main.Instance.ShowSettingsWindow, tbHeight))
            {
                Main.Instance.ShowSettingsWindow = !Main.Instance.ShowSettingsWindow;
                if (Main.Instance.ShowSettingsWindow) GUI.BringWindowToFront(1);
            }

            GUILayout.Space(5);

            // --- НОВАЯ КНОПКА ABOUT ---
            if (DrawModernTaskbarButton("ABOUT", Main.Instance.ShowAboutWindow, tbHeight))
            {
                Main.Instance.ShowAboutWindow = !Main.Instance.ShowAboutWindow;
                if (Main.Instance.ShowAboutWindow) GUI.BringWindowToFront(2);
            }
            // --------------------------

            GUILayout.FlexibleSpace();

            // CLOCK
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            GUILayout.Label(DateTime.Now.ToString("HH:mm:ss"), StyleManager.Clock);
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private bool DrawModernTaskbarButton(string text, bool isActive, float barHeight)
        {
            float btnWidth = 100f;

            GUIStyle currentStyle = new GUIStyle(StyleManager.TaskbarBtn);
            if (isActive) currentStyle.normal.textColor = Color.white;
            else currentStyle.normal.textColor = StyleManager.Colors.TextDim;

            bool clicked = GUILayout.Button(text, currentStyle, GUILayout.Width(btnWidth), GUILayout.Height(barHeight));
            Rect r = GUILayoutUtility.GetLastRect();

            if (isActive)
            {
                GUI.DrawTexture(new Rect(r.x, barHeight - 2, r.width, 2), StyleManager.TexAccent);
                GUI.DrawTexture(new Rect(r.x, 0, r.width, barHeight), StyleManager.TexHover);
            }
            else if (r.Contains(Event.current.mousePosition))
            {
                GUI.DrawTexture(new Rect(r.x, 0, r.width, barHeight), StyleManager.TexHover);
            }

            return clicked;
        }
    }
}