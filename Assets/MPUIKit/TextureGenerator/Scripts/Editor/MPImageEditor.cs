using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.Callbacks;

namespace MPUIKIT
{
    [CustomEditor(typeof(MPImage))]
    public class MPImageEditor : ImageEditor {
        private MPImage mpImage;
        private SerializedProperty m_UseSpriteMesh, m_PreserveAspect;
        private SerializedProperty DrawMethod, Shape, LineWeight, FalloffDistance, SharedMaterial;
        private SerializedProperty CircleRadius, RectangleCornerRadius;
        private SerializedProperty PentagonRectangleRadius, PentagonTriangleRadius, PentagonTriangleSize;
        private SerializedProperty TriangleRadius;
        private SerializedProperty OverrideSize, Size;
        private AnimBool ShowRegular, ShowProcedural, ShowHybrid;

        private bool TestBool;
        private void OnEnable()
        {
            base.OnEnable();
            mpImage = target as MPImage;
            DrawMethod = serializedObject.FindProperty("m_DrawMethod");
            Shape = serializedObject.FindProperty("m_DrawShape");
            LineWeight = serializedObject.FindProperty("m_LineWeight");
            FalloffDistance = serializedObject.FindProperty("m_FalloffDistance");
            SharedMaterial = serializedObject.FindProperty("m_SharedMaterial");
            RectangleCornerRadius = serializedObject.FindProperty("m_RectangleCornerRadius");
            CircleRadius = serializedObject.FindProperty("m_CircleRadius");
            OverrideSize = serializedObject.FindProperty("m_OverrideSize");
            Size = serializedObject.FindProperty("m_Size");
            
            PentagonRectangleRadius = serializedObject.FindProperty("m_PentagonRectangleRadius");
            PentagonTriangleRadius = serializedObject.FindProperty("m_PentagonTriangleRadius");
            PentagonTriangleSize = serializedObject.FindProperty("m_PentagonTriangleSize");

            TriangleRadius = serializedObject.FindProperty("m_TriangleRadius");
            
            m_PreserveAspect = serializedObject.FindProperty("m_PreserveAspect");
            m_UseSpriteMesh = serializedObject.FindProperty("m_UseSpriteMesh");
            
        }
        
        public override void OnInspectorGUI() {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            RaycastControlsGUI();
            EditorGUILayout.PropertyField(m_Color);
            EditorGUILayout.Space();

            mpImage.DrawMethod = (DrawMethod)EditorGUILayout.EnumPopup("Draw Method", (DrawMethod) DrawMethod.enumValueIndex);
            if (mpImage.DrawMethod == MPUIKIT.DrawMethod.Regular || mpImage.DrawMethod == MPUIKIT.DrawMethod.Hybrid) {
                SpriteGUI();
                if (mpImage.sprite != null) {
                    TypeGUI();
                    if (mpImage.DrawMethod == MPUIKIT.DrawMethod.Regular) {
                        EditorGUI.indentLevel++;
                        if (mpImage.type == Image.Type.Simple) {
                            EditorGUILayout.PropertyField(m_UseSpriteMesh);
                            EditorGUILayout.PropertyField(m_PreserveAspect);
                        }

                        if (mpImage.type == Image.Type.Filled) {
                            EditorGUILayout.PropertyField(m_PreserveAspect);
                        }
                        EditorGUI.indentLevel--;
                    }
                }
            }
            
            
            if (mpImage.DrawMethod == MPUIKIT.DrawMethod.Procedural || mpImage.DrawMethod == MPUIKIT.DrawMethod.Hybrid){
                EditorGUILayout.PropertyField(Shape);
                switch (mpImage.DrawShape) {
                    case DrawShape.Circle:
                        CircleGUI();
                        break;
                    case DrawShape.Rectangle:
                        RectangleGUI();
                        break;
                        case DrawShape.Pentagon:
                            PentagonGUI();
                            break;

                        case DrawShape.Triangle:
                            TriangleGUI();
                            break;
                        Default:
                        break;
                }
                mpImage.LineWeight = EditorGUILayout.FloatField("Line Weight", LineWeight.floatValue);
                mpImage.FalloffDistance = EditorGUILayout.FloatField("Falloff", FalloffDistance.floatValue);
            }
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(OverrideSize);
            if (OverrideSize.boolValue) {
                EditorGUILayout.PropertyField(Size);
            }

            bool showNativeSize = (mpImage.DrawMethod != MPUIKIT.DrawMethod.Procedural && mpImage.sprite != null &&
                                   (mpImage.type == Image.Type.Simple || mpImage.type == Image.Type.Filled));
            base.SetShowNativeSize(showNativeSize, false);
            NativeSizeButtonGUI();
            
            mpImage.SharedMaterial = (Material) EditorGUILayout.ObjectField("Material", SharedMaterial.objectReferenceValue, typeof(Material), false);
            if (EditorGUI.EndChangeCheck()) {
                mpImage.UpdateMaterial();
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void PentagonGUI() {
            EditorGUILayout.PropertyField(PentagonRectangleRadius);
            EditorGUILayout.PropertyField(PentagonTriangleRadius);
            EditorGUILayout.PropertyField(PentagonTriangleSize);
        }

        private void CircleGUI() {
            EditorGUILayout.PropertyField(CircleRadius, new GUIContent("Radius"));
        }


        private void RectangleGUI() {
            EditorGUILayout.PropertyField(RectangleCornerRadius, new GUIContent("Corner Radius"));
        }

        private void TriangleGUI() {
            EditorGUILayout.PropertyField(TriangleRadius);
        }

        private void UpdateImage() {
            
                mpImage.UpdateMaterial();
        }
    }
}
