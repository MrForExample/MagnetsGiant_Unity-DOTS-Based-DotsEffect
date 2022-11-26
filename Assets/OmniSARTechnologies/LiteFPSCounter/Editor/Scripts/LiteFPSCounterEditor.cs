//
// Lite FPS Counter Editor
//
// Author     : Alex Tuduran
// Copyright  : OmniSAR Technologies
//

using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using OmniSARTechnologies.Helper;

namespace OmniSARTechnologies.LiteFPSCounter {
    [CustomEditor(typeof(LiteFPSCounter))]
    [CanEditMultipleObjects]
    public class LiteFPSCounterEditor : Editor {
        private const string PackageName = "Lite FPS Counter";
        private const string GameObjectMenuRoot = "GameObject/OmniSAR Technologies/" + PackageName + "/";
        private const string HeaderGraphicPath = "Assets/OmniSARTechnologies/LiteFPSCounter/Editor/Resources/UI/Header/lite-fps-counter-header.png";
        private const float HeaderGraphicTileWidth = 5.0f;

        private SerializedProperty m_DynamicInfoText = null;
        private SerializedProperty m_StaticInfoText = null;

        private bool m_GUIOptionsFolded = true;

        private static Texture m_HeaderTex = null;
        private static Rect m_HeaderTileTexCoords;

        private void OnEnable() {
            m_DynamicInfoText = serializedObject.FindProperty("dynamicInfoText");
            m_StaticInfoText = serializedObject.FindProperty("staticInfoText");

            m_HeaderTex = (Texture2D)AssetDatabase.LoadMainAssetAtPath(HeaderGraphicPath);
            if (null == m_HeaderTex) {
                return;
            }

            m_HeaderTileTexCoords = new Rect(0, 0, HeaderGraphicTileWidth / (m_HeaderTex.width - 1), 1);
        }

        public static void DrawComponentHeader() {
            if (!m_HeaderTex) {
                return;
            }

            GUILayout.BeginHorizontal(); {
                Rect drawingAreaRect = EditorGUILayout.GetControlRect(GUILayout.MaxHeight(34));

                Rect headerRect = new Rect(
                    drawingAreaRect.xMin - 13,
                    drawingAreaRect.y + 2,
                    drawingAreaRect.xMax + 3,
                    m_HeaderTex.height
                );

                Rect headerImageRect = new Rect(headerRect) { width = m_HeaderTex.width };
                headerImageRect.width = m_HeaderTex.width;
                GUI.DrawTexture(headerImageRect, m_HeaderTex);

                Rect headerImageTileRect = new Rect(headerRect);
                headerImageTileRect.x = m_HeaderTex.width + 1;
                headerImageTileRect.width = headerRect.width - headerImageRect.width;
                GUI.DrawTextureWithTexCoords(headerImageTileRect, m_HeaderTex, m_HeaderTileTexCoords);
            } GUILayout.EndHorizontal();
        }

        private void DrawUIOptions<T>(SerializedProperty headerProperty) {
            HeaderAttribute header = EditorGUIHelper.Attributes.GetSerializedPropertyFirstAttribute<T, HeaderAttribute>(headerProperty);
            if (null != header) {
                m_GUIOptionsFolded = EditorGUILayout.Foldout(
                    m_GUIOptionsFolded,
                    header.header,
                    m_GUIOptionsFolded ? EditorGUIHelper.Styles.boldFoldout : EditorStyles.foldout
                );

                if (!m_GUIOptionsFolded) {
                    return;
                }
            }

            EditorGUIHelper.Drawing.DrawMultiValueObjectField<T>(m_DynamicInfoText);
            EditorGUIHelper.Drawing.DrawMultiValueObjectField<T>(m_StaticInfoText);

            if (null != header) {
                EditorGUILayout.Separator();
            }
        }

        public override void OnInspectorGUI() {
            DrawComponentHeader();
            DrawUIOptions<LiteFPSCounter>(m_DynamicInfoText);
        }

        private static bool CreateGameObject(string prefabName, string commandName, string packageName) {
            string[] assets = AssetDatabase.FindAssets(prefabName + " t:Prefab");

            if (null == assets) {
                Debug.LogWarning(
                    ColorHelper.ColorText(
                        string.Format(
                            "Could not create {0}: " +
                            "Prefab \"{1}\" could not be found in the project: " +
                            "Please re-install the {2} package and try again",
                            commandName,
                            prefabName,
                            packageName
                        ),
                        Color.red
                    )
                );
                return false;
            }

            if (assets.Length < 1) {
                Debug.LogWarning(
                    ColorHelper.ColorText(
                        string.Format(
                            "Could not create {0}: " +
                            "Prefab \"{1}\" could not be found in the project: " +
                            "Please re-install the {2} package and try again",
                            commandName,
                            prefabName,
                            packageName
                        ),
                        Color.red
                    )
                );
                return false;
            }

            string prefabPath = AssetDatabase.GUIDToAssetPath(assets[0]);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (!prefab) {
                Debug.LogWarning(
                    ColorHelper.ColorText(
                        string.Format(
                            "Could not create {0}: " +
                            "Prefab \"{1}\" could not be found in the project: " +
                            "Please re-install the {2} package and try again",
                            commandName,
                            prefabName,
                            packageName
                        ),
                        Color.red
                    )
                );
                return false;
            }

            GameObject go = Instantiate(prefab, Selection.activeGameObject ? Selection.activeGameObject.transform : null);
            go.name = prefabName;

            if (!go) {
                Debug.LogWarning(
                    ColorHelper.ColorText(
                        string.Format(
                            "Could not create {0}: " +
                            "Prefab \"{1}\" coult not be instantiated: " +
                            "Instantiate() returned NULL: " +
                            "Please manually add the prefab into the scene: " +
                            "Path: \"{2}\"",
                            commandName,
                            prefabName,
                            prefabPath
                        ),
                        Color.red
                    )
                );
                return false;
            }

            Selection.activeGameObject = go;
            Debug.Log(
                ColorHelper.ColorText(
                    string.Format(
                        "Game Object \"{0}\" added to scene (based on the \"{1}\" prefab)",
                        go.name,
                        prefabPath
                    ),
                    ColorHelper.HexStrToColor("#0C4366")
                )
            );
            return true;
        }

        [MenuItem(GameObjectMenuRoot + "Lite FPS Counter", priority = 10)] 
        private static bool CreateLiteFPSCounterGameObject() {
            return CreateGameObject(
                "LiteFPSCounter",
                "Lite FPS Counter",
                PackageName
            );
        }
    }
}