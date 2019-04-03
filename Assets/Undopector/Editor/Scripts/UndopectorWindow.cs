﻿using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace HK.Undopector
{
    public class UndopectorWindow : EditorWindow
    {
        private List<int> instanceIds = new List<int>();

        private int currentIndex = 0;

        private bool registerSelection = true;

        private Vector2 scrollPosition;

        private static readonly Vector2 WindowMinSize = new Vector2(80, 24);

        private static readonly GUILayoutOption[] ButtonGUILayoutOptions =
        {
            GUILayout.Width(24),
            GUILayout.Height(18),
        };

        [MenuItem("Window/Undopector Floating Window")]
        private static void OpenAsUtility()
        {
            var instance = GetWindow(true);
            instance.minSize = WindowMinSize;
            var position = instance.position;
            instance.position = new Rect(120, 120, WindowMinSize.x, WindowMinSize.y);
            instance.ShowUtility();
        }

        [MenuItem("Window/Undopector")]
        private static void Open()
        {
            GetWindow(false).Show();
        }

        private static UndopectorWindow GetWindow(bool utility)
        {
            // MEMO: すでに存在するウィンドウを明示的に閉じる
            GetWindow<UndopectorWindow>().Close();

            return GetWindow<UndopectorWindow>(utility, "Undopector", true);
        }

        void OnGUI()
        {
            this.DrawUndoRedoButtons();
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
            this.DrawSelectedInstanceIdList();
        }

        void OnSelectionChange()
        {
            var instanceId = Selection.activeInstanceID;
            if (this.registerSelection && instanceId != 0)
            {
                if (this.currentIndex < this.instanceIds.Count - 1)
                {
                    this.instanceIds = new List<int>(this.instanceIds.GetRange(0, this.currentIndex + 1));
                }

                if (this.instanceIds.Count <= 0 || this.instanceIds[this.instanceIds.Count - 1] != instanceId)
                {
                    this.instanceIds.Add(instanceId);
                    this.currentIndex = this.instanceIds.Count - 1;
                }
            }

            this.registerSelection = true;
            this.Repaint();
        }

        private void DrawUndoRedoButtons()
        {
            using (new GUILayout.HorizontalScope())
            {
                EditorGUI.BeginDisabledGroup(this.currentIndex <= 0);
                if (GUILayout.Button("<", ButtonGUILayoutOptions))
                {
                    this.registerSelection = false;
                    this.currentIndex--;
                    Selection.activeInstanceID = this.instanceIds[this.currentIndex];
                }
                EditorGUI.EndDisabledGroup();

                EditorGUI.BeginDisabledGroup(this.currentIndex >= this.instanceIds.Count - 1);
                if (GUILayout.Button(">", ButtonGUILayoutOptions))
                {
                    this.registerSelection = false;
                    this.currentIndex++;
                    Selection.activeInstanceID = this.instanceIds[this.currentIndex];
                }
                EditorGUI.EndDisabledGroup();
            }
        }

        private void DrawSelectedInstanceIdList()
        {
            if (!this.CanDrawSelectedInstanceIdList)
            {
                return;
            }

            using (var scrollView = new GUILayout.ScrollViewScope(this.scrollPosition))
            {
                using (var iconSize = new EditorGUIUtility.IconSizeScope(Vector2.one * 14))
                {
                    for (var i = 0; i < this.instanceIds.Count; i++)
                    {
                        this.DrawChangeSelectionButton(i);
                    }
                }

                this.scrollPosition = scrollView.scrollPosition;
            }
        }

        private bool CanDrawSelectedInstanceIdList
        {
            get
            {
                return this.position.width > WindowMinSize.x || this.position.height > WindowMinSize.y;
            }
        }

        private void DrawChangeSelectionButton(int instanceIdsIndex)
        {
            var instanceId = this.instanceIds[instanceIdsIndex];

            using (new EditorGUILayout.HorizontalScope())
            {
                if (this.currentIndex == instanceIdsIndex)
                {
                    GUILayout.Box("", GUI.skin.GetStyle("Foldout"), GUILayout.Width(18));
                }
                using (new EditorGUI.DisabledGroupScope(this.currentIndex == instanceIdsIndex))
                {
                    var obj = EditorUtility.InstanceIDToObject(instanceId);
                    if (GUILayout.Button(EditorGUIUtility.ObjectContent(obj, obj.GetType()), GUI.skin.GetStyle("GUIEditor.BreadcrumbLeft")))
                    {
                        Selection.activeInstanceID = instanceId;
                        this.currentIndex = instanceIdsIndex;
                        this.registerSelection = false;
                    }
                }
            }
        }
    }
}
