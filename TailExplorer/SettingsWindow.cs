using MelonLoader;
using UnityEngine;

namespace SceneExplorerMod
{
    public class SettingsWindow
    {
        // Увеличил высоту по умолчанию с 240 до 270, чтобы влезла новая опция
        public Rect WindowRect = new Rect(440, 70, 300, 270);
        public bool IsRebinding { get; private set; } = false;

        // --- Resize State ---
        private bool _isResizing = false;
        private ResizeDirection _currentResizeDir = ResizeDirection.None;
        private const float ResizeBorder = 10f;
        private const float MinWidth = 280f;
        private const float MinHeight = 200f;
        private enum ResizeDirection { None, Left, Right, Top, Bottom, TopLeft, TopRight, BottomLeft, BottomRight }

        public void Draw(int id)
        {
            HandleResize();
            GUI.backgroundColor = Color.white;
            WindowRect = GUI.Window(id, WindowRect, DrawContent, "", StyleManager.Window);
        }

        private void DrawContent(int id)
        {
            Event e = Event.current;

            // HEADER
            GUILayout.BeginHorizontal();
            GUILayout.Label("SETTINGS", StyleManager.Label);
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            // --- SECTION: GENERAL ---
            GUILayout.Label("General", StyleManager.Label);

            GUI.backgroundColor = StyleManager.Colors.HeaderBg;
            GUILayout.BeginVertical("box");
            GUI.backgroundColor = Color.white;
            {
                // Option 1: Auto Open
                GUILayout.BeginHorizontal();
                GUILayout.Label("Auto Open on Start", StyleManager.Label);
                GUILayout.FlexibleSpace();
                bool autoOpen = Main.PrefAutoOpen.Value;
                if (DrawCustomCheckbox(autoOpen))
                {
                    Main.PrefAutoOpen.Value = !autoOpen;
                    Main.PrefsCategory.SaveToFile(false);
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(5);

                // Option 2: Force Unlock Cursor (Новая опция)
                GUILayout.BeginHorizontal();
                GUILayout.Label("Force Unlock Cursor", StyleManager.Label);
                GUILayout.FlexibleSpace();
                bool forceCursor = Main.PrefForceCursor.Value;
                if (DrawCustomCheckbox(forceCursor))
                {
                    Main.PrefForceCursor.Value = !forceCursor;
                    Main.PrefsCategory.SaveToFile(false);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();

            GUILayout.Space(10);

            // --- SECTION: CONTROLS ---
            GUILayout.Label("Controls", StyleManager.Label);

            GUI.backgroundColor = StyleManager.Colors.HeaderBg;
            GUILayout.BeginVertical("box");
            GUI.backgroundColor = Color.white;
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Menu Toggle Key", StyleManager.Label, GUILayout.Height(24));
                GUILayout.FlexibleSpace();

                string btnText = IsRebinding ? "Press Any Key..." : Main.PrefMenuKey.Value.ToString();

                GUIStyle btnStyle = new GUIStyle(StyleManager.ButtonMenu);
                btnStyle.alignment = TextAnchor.MiddleCenter;

                Color originalColor = GUI.color;
                if (IsRebinding)
                {
                    float t = Mathf.PingPong(Time.unscaledTime * 3f, 1f);
                    GUI.color = Color.Lerp(Color.white, new Color(1f, 0.3f, 0.3f), t);
                }

                if (GUILayout.Button(btnText, btnStyle, GUILayout.Width(120), GUILayout.Height(24)))
                {
                    IsRebinding = !IsRebinding;
                }

                GUI.color = originalColor;

                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();

            // --- Info Footer ---
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUIStyle dimLabel = new GUIStyle(StyleManager.Label);
            dimLabel.normal.textColor = new Color(1, 1, 1, 0.3f);
            dimLabel.fontSize = 10;
            GUILayout.Label($"v{Main.Instance.Info.Version}", dimLabel);
            GUILayout.EndHorizontal();

            // Rebind Logic
            if (IsRebinding && e.isKey && e.type == EventType.KeyDown)
            {
                if (e.keyCode != KeyCode.None && e.keyCode != KeyCode.Escape)
                {
                    Main.PrefMenuKey.Value = e.keyCode;
                    Main.PrefsCategory.SaveToFile(false);
                    IsRebinding = false;
                    e.Use();
                }
                else if (e.keyCode == KeyCode.Escape)
                {
                    IsRebinding = false;
                    e.Use();
                }
            }

            if (IsRebinding && e.type == EventType.MouseDown && !WindowRect.Contains(GUIUtility.GUIToScreenPoint(e.mousePosition)))
            {
                IsRebinding = false;
            }

            if (!_isResizing) GUI.DragWindow(new Rect(ResizeBorder, 0, WindowRect.width - ResizeBorder * 2, 30));
        }

        private bool DrawCustomCheckbox(bool value)
        {
            GUI.backgroundColor = value ? StyleManager.Colors.Accent : StyleManager.Colors.CheckboxEmpty;
            bool clicked = GUILayout.Button("", StyleManager.Checkbox, GUILayout.Width(18), GUILayout.Height(18));
            GUI.backgroundColor = Color.white;
            return clicked;
        }

        private void HandleResize()
        {
            Event e = Event.current;
            Vector2 m = e.mousePosition;

            if (_isResizing)
            {
                if (e.type == EventType.MouseUp)
                {
                    _isResizing = false;
                    _currentResizeDir = ResizeDirection.None;
                }
                else if (e.type == EventType.MouseDrag)
                {
                    Vector2 delta = e.delta;
                    Rect r = WindowRect;

                    if (_currentResizeDir == ResizeDirection.Right || _currentResizeDir == ResizeDirection.BottomRight || _currentResizeDir == ResizeDirection.TopRight)
                        r.width = Mathf.Max(MinWidth, r.width + delta.x);

                    if (_currentResizeDir == ResizeDirection.Bottom || _currentResizeDir == ResizeDirection.BottomRight || _currentResizeDir == ResizeDirection.BottomLeft)
                        r.height = Mathf.Max(MinHeight, r.height + delta.y);

                    if (_currentResizeDir == ResizeDirection.Left || _currentResizeDir == ResizeDirection.TopLeft || _currentResizeDir == ResizeDirection.BottomLeft)
                    {
                        float oldMaxX = r.xMax;
                        r.x += delta.x;
                        r.width = oldMaxX - r.x;
                        if (r.width < MinWidth) { r.x = oldMaxX - MinWidth; r.width = MinWidth; }
                    }

                    if (_currentResizeDir == ResizeDirection.Top || _currentResizeDir == ResizeDirection.TopLeft || _currentResizeDir == ResizeDirection.TopRight)
                    {
                        float oldMaxY = r.yMax;
                        r.y += delta.y;
                        r.height = oldMaxY - r.y;
                        if (r.height < MinHeight) { r.y = oldMaxY - MinHeight; r.height = MinHeight; }
                    }

                    WindowRect = r;
                    e.Use();
                }
            }
            else
            {
                float b = ResizeBorder;
                Rect r = WindowRect;
                bool nearWindow = m.x >= r.x - b && m.x <= r.xMax + b && m.y >= r.y - b && m.y <= r.yMax + b;

                if (nearWindow)
                {
                    bool inLeft = m.x >= r.x - b / 2 && m.x <= r.x + b;
                    bool inRight = m.x >= r.xMax - b && m.x <= r.xMax + b / 2;
                    bool inTop = m.y >= r.y - b / 2 && m.y <= r.y + b;
                    bool inBottom = m.y >= r.yMax - b && m.y <= r.yMax + b / 2;

                    if (inLeft || inRight || inTop || inBottom)
                    {
                        ResizeDirection dir = ResizeDirection.None;
                        if (inTop && inLeft) dir = ResizeDirection.TopLeft;
                        else if (inTop && inRight) dir = ResizeDirection.TopRight;
                        else if (inBottom && inLeft) dir = ResizeDirection.BottomLeft;
                        else if (inBottom && inRight) dir = ResizeDirection.BottomRight;
                        else if (inTop) dir = ResizeDirection.Top;
                        else if (inBottom) dir = ResizeDirection.Bottom;
                        else if (inLeft) dir = ResizeDirection.Left;
                        else if (inRight) dir = ResizeDirection.Right;

                        if (dir != ResizeDirection.None && e.type == EventType.MouseDown && e.button == 0)
                        {
                            _isResizing = true;
                            _currentResizeDir = dir;
                            e.Use();
                        }
                    }
                }
            }
        }
    }
}