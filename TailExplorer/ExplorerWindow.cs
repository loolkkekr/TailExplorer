// ExplorerWindow.cs
using System;
using System.Collections.Generic;
using System.IO;
using MelonLoader;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SceneExplorerMod
{
    public class ExplorerWindow
    {
        public Rect WindowRect = new Rect(20, 70, 400, 660);

        private Vector2 _scrollPosition;

        // --- Resize State ---
        private bool _isResizing = false;
        private ResizeDirection _currentResizeDir = ResizeDirection.None;
        private const float ResizeBorder = 10f;
        private const float MinWidth = 300f;
        private const float MinHeight = 400f;
        private enum ResizeDirection { None, Left, Right, Top, Bottom, TopLeft, TopRight, BottomLeft, BottomRight }

        // --- Logic State ---
        private HashSet<int> _expandedObjects = new HashSet<int>();
        private HashSet<int> _selectedInstanceIDs = new HashSet<int>();
        private int _selectedSceneMode = 0;
        private int _lastClickedID = -1;
        private List<int> _visibleItems = new List<int>();

        // --- Context Menu State ---
        private bool _showContextMenu = false;
        private Rect _contextMenuRect;
        private int _contextMenuTargetID = -1;

        // --- Scene Loader State ---
        private List<string> _buildScenes = new List<string>();
        private bool _scenesCollected = false;
        private int _selectedSceneIndex = -1;
        private bool _showSceneSelector = false;
        private Vector2 _sceneSelectorScroll;
        private Rect _sceneSelectorRect;

        // --- Rename State ---
        private bool _isRenaming = false;
        private int _renamingID = -1;
        private string _renameBuffer = "";
        private bool _shouldFocusRename = false;

        private Vector2 _mouseInWindow;

        public void Draw(int id)
        {
            HandleResize();
            GUI.backgroundColor = Color.white;
            WindowRect = GUI.Window(id, WindowRect, DrawContent, "", StyleManager.Window);
        }

        private void DrawContent(int id)
        {
            Event e = Event.current;
            _mouseInWindow = e.mousePosition;

            HandleOutsideClicks(e);

            // HEADER
            GUILayout.BeginHorizontal();
            GUILayout.Label("SCENE EXPLORER", StyleManager.Label);
            GUILayout.EndHorizontal();
            GUILayout.Space(10);

            // TABS
            GUILayout.BeginHorizontal();
            string activeName = SceneManager.GetActiveScene().name;
            if (string.IsNullOrEmpty(activeName)) activeName = "Active Scene";
            DrawSceneTabButton(0, activeName);
            DrawSceneTabButton(1, "DontDestroyOnLoad");
            GUILayout.EndHorizontal();
            GUILayout.Space(5);

            // TREE VIEW
            GUI.backgroundColor = StyleManager.Colors.HeaderBg;
            GUILayout.BeginVertical("box");
            GUI.backgroundColor = Color.white;

            if (e.type == EventType.Repaint) _visibleItems.Clear();

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
            if (_selectedSceneMode == 0) DrawActiveScene();
            else DrawDontDestroyOnLoad();
            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            // SCENE LOADER
            GUILayout.Space(5);
            DrawSceneLoaderUI();

            // OVERLAYS
            if (_showSceneSelector) DrawSceneSelectorOverlay();
            if (_showContextMenu) DrawContextMenuOverlay();

            if (!_isResizing) GUI.DragWindow(new Rect(ResizeBorder, 0, WindowRect.width - ResizeBorder * 2, 30));
        }

        private void HandleOutsideClicks(Event e)
        {
            if (_showContextMenu && e.type == EventType.MouseDown && !_contextMenuRect.Contains(_mouseInWindow))
            {
                _showContextMenu = false;
                _isRenaming = false;
            }
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

        // ... SCENE LOGIC ...
        private void DrawSceneTabButton(int modeIndex, string label)
        {
            GUI.backgroundColor = (_selectedSceneMode == modeIndex) ? StyleManager.Colors.Accent : StyleManager.Colors.HeaderBg;
            if (GUILayout.Button(label, StyleManager.ButtonMenu))
            {
                _selectedSceneMode = modeIndex;
                _scrollPosition = Vector2.zero;
                _showContextMenu = false;
                _showSceneSelector = false;
                _selectedInstanceIDs.Clear();
                _lastClickedID = -1;
            }
        }

        private void DrawActiveScene()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (!activeScene.IsValid()) return;
            foreach (var go in activeScene.GetRootGameObjects()) DrawNode(go, 0);
        }

        private void DrawDontDestroyOnLoad()
        {
            var allTransforms = UnityEngine.Object.FindObjectsOfType<Transform>();
            List<GameObject> ddolObjects = new List<GameObject>();
            foreach (var t in allTransforms)
            {
                if (t.parent == null && t.gameObject.scene.buildIndex == -1) ddolObjects.Add(t.gameObject);
            }
            foreach (var go in ddolObjects) DrawNode(go, 0);
        }

        private void DrawNode(GameObject go, int indent)
        {
            if (go == null) return;

            int id = go.GetInstanceID();

            if (Event.current.type == EventType.Repaint) _visibleItems.Add(id);

            bool isExpanded = _expandedObjects.Contains(id);
            bool isSelected = _selectedInstanceIDs.Contains(id);
            Component[] components = go.GetComponents<Component>();
            int childCount = go.transform.childCount;
            bool hasChildren = childCount > 0 || components.Length > 1;

            Rect rowRect = GUILayoutUtility.GetRect(GUIContent.none, StyleManager.Row, GUILayout.Height(22));
            bool isMouseOverMenu = (_showContextMenu && _contextMenuRect.Contains(_mouseInWindow)) ||
                                   (_showSceneSelector && _sceneSelectorRect.Contains(_mouseInWindow));

            HandleNodeClick(go, id, rowRect, isMouseOverMenu);

            if (isSelected) GUI.DrawTexture(rowRect, StyleManager.TexHighlight);

            GUILayout.Space(-22);
            GUILayout.BeginHorizontal(StyleManager.Row);
            GUILayout.Space(indent * 15);

            if (hasChildren)
            {
                string arrow = isExpanded ? "▼" : "▶";
                if (GUILayout.Button(arrow, StyleManager.Arrow))
                {
                    if (isExpanded) _expandedObjects.Remove(id);
                    else _expandedObjects.Add(id);
                }
            }
            else GUILayout.Space(20);

            bool isActive = go.activeSelf;
            if (DrawCustomCheckbox(isActive))
            {
                if (!isMouseOverMenu) go.SetActive(!isActive);
            }
            GUILayout.Space(5);

            if (_isRenaming && _renamingID == id)
            {
                DrawRenameField(go);
            }
            else
            {
                GUIStyle labelStyle = new GUIStyle(StyleManager.Label);
                if (!isActive) labelStyle.normal.textColor = StyleManager.Colors.TextDim;
                GUILayout.Label(go.name, labelStyle);
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            if (_isRenaming && _renamingID == id && Event.current.type == EventType.Repaint)
            {
                if (GUI.GetNameOfFocusedControl() != "RenameField" && !_shouldFocusRename)
                {
                    go.name = _renameBuffer;
                    _isRenaming = false;
                }
            }

            if (isExpanded)
            {
                foreach (var comp in components) if (comp != null && !(comp is Transform)) DrawComponent(comp, indent + 1, isMouseOverMenu);
                for (int i = 0; i < childCount; i++) DrawNode(go.transform.GetChild(i).gameObject, indent + 1);
            }
        }

        private void HandleNodeClick(GameObject go, int id, Rect rowRect, bool isMouseOverMenu)
        {
            if (!isMouseOverMenu && Event.current.type == EventType.MouseDown && rowRect.Contains(Event.current.mousePosition))
            {
                if (Event.current.button == 0)
                {
                    if (!(_isRenaming && _renamingID == id))
                    {
                        if (Event.current.shift && _lastClickedID != -1)
                        {
                            int currentIdx = _visibleItems.IndexOf(id);
                            int prevIdx = _visibleItems.IndexOf(_lastClickedID);

                            if (currentIdx != -1 && prevIdx != -1)
                            {
                                _selectedInstanceIDs.Clear();
                                int start = Mathf.Min(currentIdx, prevIdx);
                                int end = Mathf.Max(currentIdx, prevIdx);
                                for (int i = start; i <= end; i++) _selectedInstanceIDs.Add(_visibleItems[i]);
                            }
                            else
                            {
                                _selectedInstanceIDs.Clear();
                                _selectedInstanceIDs.Add(id);
                                _lastClickedID = id;
                            }
                        }
                        else if (Event.current.control)
                        {
                            if (_selectedInstanceIDs.Contains(id)) _selectedInstanceIDs.Remove(id);
                            else
                            {
                                _selectedInstanceIDs.Add(id);
                                _lastClickedID = id;
                            }
                        }
                        else
                        {
                            _selectedInstanceIDs.Clear();
                            _selectedInstanceIDs.Add(id);
                            _lastClickedID = id;
                        }
                        _isRenaming = false;
                        _showSceneSelector = false;
                    }
                }
                else if (Event.current.button == 1)
                {
                    if (!_selectedInstanceIDs.Contains(id))
                    {
                        _selectedInstanceIDs.Clear();
                        _selectedInstanceIDs.Add(id);
                        _lastClickedID = id;
                    }
                    _contextMenuTargetID = id;
                    _showContextMenu = true;
                    _showSceneSelector = false;
                    _isRenaming = false;

                    bool isMultiSelect = _selectedInstanceIDs.Count > 1;
                    float menuW = 120;
                    float menuH = isMultiSelect ? 32 : 62;
                    float x = _mouseInWindow.x;
                    float y = _mouseInWindow.y;
                    if (x + menuW > WindowRect.width) x -= menuW;
                    if (y + menuH > WindowRect.height) y -= menuH;
                    _contextMenuRect = new Rect(x, y, menuW, menuH);
                    Event.current.Use();
                }
            }
        }

        private void DrawRenameField(GameObject go)
        {
            GUI.SetNextControlName("RenameField");
            _renameBuffer = GUILayout.TextField(_renameBuffer, StyleManager.TextField);
            if (_shouldFocusRename) { GUI.FocusControl("RenameField"); _shouldFocusRename = false; }
            if (Event.current.isKey && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter))
            {
                go.name = _renameBuffer;
                _isRenaming = false;
                Event.current.Use();
            }
        }

        private void DrawComponent(Component comp, int indent, bool isBlocked)
        {
            GUILayout.BeginHorizontal(StyleManager.Row);
            GUILayout.Space(indent * 15 + 20);
            bool canDisable = false;
            bool isEnabled = true;

            if (comp is Behaviour beh) { canDisable = true; isEnabled = beh.enabled; }
            else if (comp is Renderer rend) { canDisable = true; isEnabled = rend.enabled; }
            else if (comp is Collider col) { canDisable = true; isEnabled = col.enabled; }

            if (canDisable)
            {
                if (DrawCustomCheckbox(isEnabled) && !isBlocked)
                {
                    if (comp is Behaviour b) b.enabled = !isEnabled;
                    else if (comp is Renderer r) r.enabled = !isEnabled;
                    else if (comp is Collider c) c.enabled = !isEnabled;
                }
            }
            else GUILayout.Space(18);

            GUILayout.Space(5);
            GUIStyle compStyle = new GUIStyle(StyleManager.Label);
            compStyle.normal.textColor = new Color(0.6f, 0.8f, 1f);
            GUILayout.Label(comp.GetType().Name, compStyle);
            GUILayout.EndHorizontal();
        }

        private bool DrawCustomCheckbox(bool value)
        {
            GUI.backgroundColor = value ? StyleManager.Colors.Accent : StyleManager.Colors.CheckboxEmpty;
            bool clicked = GUILayout.Button("", StyleManager.Checkbox, GUILayout.Width(16), GUILayout.Height(16));
            GUI.backgroundColor = Color.white;
            return clicked;
        }

        // --- CONTEXT MENU & LOADER ---
        private void DrawContextMenuOverlay()
        {
            GUI.DrawTexture(new Rect(_contextMenuRect.x - 1, _contextMenuRect.y - 1, _contextMenuRect.width + 2, _contextMenuRect.height + 2), StyleManager.TexBorder);
            GUI.DrawTexture(_contextMenuRect, StyleManager.TexContext);

            bool isMultiSelect = _selectedInstanceIDs.Count > 1;
            float currentY = _contextMenuRect.y;
            float btnHeight = 30f;

            if (!isMultiSelect)
            {
                Rect btnRename = new Rect(_contextMenuRect.x, currentY, _contextMenuRect.width, btnHeight);
                if (GUI.Button(btnRename, "Rename", StyleManager.ContextButton)) { PerformRename(); Event.current.Use(); }
                currentY += btnHeight;
            }

            Rect btnDelete = new Rect(_contextMenuRect.x, currentY, _contextMenuRect.width, btnHeight);
            string delText = isMultiSelect ? $"Delete ({_selectedInstanceIDs.Count})" : "Delete";
            if (GUI.Button(btnDelete, delText, StyleManager.ContextButton)) { PerformDelete(); Event.current.Use(); }
        }

        private void PerformRename()
        {
            GameObject go = FindObjectByInstanceID(_contextMenuTargetID);
            if (go != null) { _isRenaming = true; _renamingID = _contextMenuTargetID; _renameBuffer = go.name; _shouldFocusRename = true; }
            _showContextMenu = false;
        }

        private void PerformDelete()
        {
            var allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            List<GameObject> toDelete = new List<GameObject>();
            foreach (var go in allObjects) if (_selectedInstanceIDs.Contains(go.GetInstanceID())) toDelete.Add(go);
            foreach (var go in toDelete) if (go != null) UnityEngine.Object.Destroy(go);
            _selectedInstanceIDs.Clear();
            _showContextMenu = false;
        }

        private GameObject FindObjectByInstanceID(int id)
        {
            var all = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (var go in all) if (go.GetInstanceID() == id) return go;
            return null;
        }

        private void CollectBuildScenes()
        {
            if (_scenesCollected) return;
            _buildScenes.Clear();
            int count = SceneManager.sceneCountInBuildSettings;
            for (int i = 0; i < count; i++)
            {
                string path = SceneUtility.GetScenePathByBuildIndex(i);
                string name = Path.GetFileNameWithoutExtension(path);
                _buildScenes.Add(name);
            }
            _scenesCollected = true;
        }

        private void DrawSceneLoaderUI()
        {
            if (!_scenesCollected) CollectBuildScenes();

            Rect sepRect = GUILayoutUtility.GetRect(100, 2);
            GUI.DrawTexture(sepRect, StyleManager.TexSeparator);
            GUILayout.Space(5);

            string btnText = (_selectedSceneIndex >= 0 && _selectedSceneIndex < _buildScenes.Count)
                ? _buildScenes[_selectedSceneIndex]
                : "Select Scene...";

            if (GUILayout.Button(btnText, StyleManager.DropdownBtn))
            {
                _showSceneSelector = !_showSceneSelector;
                if (_showSceneSelector) _showContextMenu = false;
            }

            if (Event.current.type == EventType.Repaint)
            {
                Rect btnRect = GUILayoutUtility.GetLastRect();
                float itemHeight = 25f;
                float listHeight = Mathf.Min(_buildScenes.Count * itemHeight + 10, 200f);
                _sceneSelectorRect = new Rect(btnRect.x, btnRect.y - listHeight - 5, btnRect.width, listHeight);
            }

            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Load (Single)", StyleManager.LoaderBtn)) LoadSelectedScene(LoadSceneMode.Single);
            GUILayout.Space(5);
            if (GUILayout.Button("Load (Multiple)", StyleManager.LoaderBtn)) LoadSelectedScene(LoadSceneMode.Additive);
            GUILayout.EndHorizontal();
        }

        private void DrawSceneSelectorOverlay()
        {
            Event e = Event.current;
            if (e.type == EventType.ScrollWheel && _sceneSelectorRect.Contains(e.mousePosition))
            {
                _sceneSelectorScroll.y += e.delta.y * 20f;
                e.Use();
            }

            GUI.DrawTexture(new Rect(_sceneSelectorRect.x - 1, _sceneSelectorRect.y - 1, _sceneSelectorRect.width + 2, _sceneSelectorRect.height + 2), StyleManager.TexBorder);
            GUI.DrawTexture(_sceneSelectorRect, StyleManager.TexContext);

            GUILayout.BeginArea(_sceneSelectorRect);
            _sceneSelectorScroll = GUILayout.BeginScrollView(_sceneSelectorScroll);

            for (int i = 0; i < _buildScenes.Count; i++)
            {
                bool isSelected = (i == _selectedSceneIndex);
                GUIStyle itemStyle = new GUIStyle(StyleManager.ContextButton);
                if (isSelected) itemStyle.normal.textColor = StyleManager.Colors.Accent;

                if (GUILayout.Button(_buildScenes[i], itemStyle, GUILayout.Height(25)))
                {
                    _selectedSceneIndex = i;
                    _showSceneSelector = false;
                }
            }

            GUILayout.EndScrollView();
            GUILayout.EndArea();

            if (e.type == EventType.MouseDown && !_sceneSelectorRect.Contains(_mouseInWindow))
            {
                if (_mouseInWindow.y < _sceneSelectorRect.y || _mouseInWindow.x < _sceneSelectorRect.x || _mouseInWindow.x > _sceneSelectorRect.xMax)
                {
                    _showSceneSelector = false;
                    e.Use();
                }
            }
        }

        private void LoadSelectedScene(LoadSceneMode mode)
        {
            if (_selectedSceneIndex >= 0 && _selectedSceneIndex < _buildScenes.Count)
            {
                string sceneName = _buildScenes[_selectedSceneIndex];
                try { SceneManager.LoadScene(sceneName, mode); }
                catch (Exception e) { MelonLogger.Error($"Failed to load scene {sceneName}: {e.Message}"); }
            }
        }
    }
}