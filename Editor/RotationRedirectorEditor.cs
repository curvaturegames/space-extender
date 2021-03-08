using System.Collections;
using System.Collections.Generic;
using CurvatureGames.SpaceExtender;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace CurvatureGames.SpaceExtenderEditor
{
    /// <summary>
    /// Responsible for custom editor visualizations specific to the rotation redirector.
    /// Draws rotationpoint and its handles, rotation icon, inspector
    /// </summary>
    [CustomEditor(typeof(RotationRedirector))]
    public class RotationRedirectorEditor : BaseRedirectorEditor
    {
        // used to access properties of the RotationRedirector object.
        SerializedProperty rotationPointProp; // vector3
        SerializedProperty rotationDegreesProp; // float

        protected override void OnEnable()
        {
            base.OnEnable();
            // init properties for sceneGUI
            rotationPointProp = serializedObject.FindProperty("rotationPoint");
            rotationDegreesProp = serializedObject.FindProperty("rotationDegrees");
        }

        public override VisualElement CreateInspectorGUI()
        {
            var baseUxml = base.CreateInspectorGUI();
            var visualTree = Resources.Load("Space Extender/UXML/RotationRedirector_Inspector") as VisualTreeAsset;
            var uxml = visualTree.CloneTree();
            uxml.styleSheets.Add(Resources.Load("Space Extender/USS/BaseRedirector_Inspector_USS") as StyleSheet);

            uxml.Add(baseUxml);

            return uxml;
        }

        // UIElements does not yet support sceneGUI properly. Until then we still use the old system.
        protected override void OnSceneGUI()
        {
            // init
            base.OnSceneGUI();
            serializedObject.Update();
            RotationRedirector rotator = target as RotationRedirector;
            Vector3 worldRotationPoint = rotator.transform.TransformPoint(rotationPointProp.vector3Value);

            // custom position handle for rotation point that is restricted to xz plane
            Handles.color = Handles.xAxisColor;
            worldRotationPoint =
                Handles.Slider(worldRotationPoint, rotator.transform.TransformDirection(Vector3.right));
            Handles.color = Handles.zAxisColor;
            worldRotationPoint =
                Handles.Slider(worldRotationPoint, rotator.transform.TransformDirection(Vector3.forward));
            Handles.color = Handles.yAxisColor;
            worldRotationPoint = Handles.Slider2D(worldRotationPoint, rotator.transform.TransformDirection(Vector3.up),
                rotator.transform.TransformDirection(Vector3.right),
                rotator.transform.TransformDirection(Vector3.forward),
                HandleUtility.GetHandleSize(worldRotationPoint) * 0.15f, Handles.RectangleHandleCap, 0.0f);

            // restrict rotationPoint to playarea
            Vector3 localRotationPoint = rotator.transform.InverseTransformPoint(worldRotationPoint);
            localRotationPoint.x = Mathf.Clamp(localRotationPoint.x, -rotator.PlayAreaDimensions.x / 2,
                +rotator.PlayAreaDimensions.x / 2);
            localRotationPoint.z = Mathf.Clamp(localRotationPoint.z, -rotator.PlayAreaDimensions.y / 2,
                +rotator.PlayAreaDimensions.y / 2);

            // update rotationpoint property
            rotationPointProp.vector3Value = localRotationPoint;

            // set final world space rotation point
            worldRotationPoint = rotator.transform.TransformPoint(localRotationPoint);
            // Draw rotation point up-axis
            Handles.DrawLine(worldRotationPoint,
                worldRotationPoint + rotator.transform.TransformDirection(Vector3.up) * 7);

            // rotation handle around rotationpoint-axis for setting the desired rotation degrees
            rotationDegreesProp.floatValue = Handles.Disc(Quaternion.Euler(0.0f, rotationDegreesProp.floatValue, 0f),
                worldRotationPoint, rotator.transform.TransformDirection(Vector3.up),
                HandleUtility.GetHandleSize(worldRotationPoint) * 0.5f, false, 5.0f).eulerAngles.y;

            serializedObject.ApplyModifiedProperties();
        }

        [DrawGizmo(GizmoType.NonSelected)]
        static void DrawGizmosNonSelected(RotationRedirector rotator, GizmoType gizmoType)
        {
            Gizmos.DrawIcon(rotator.transform.position, "SpaceExtenderTool/rotationIcon", true);
            // visualize rotation point
            Gizmos.color = new Color(Handles.yAxisColor.r, Handles.yAxisColor.g, Handles.yAxisColor.b, 0.2f);
            Vector3 worldRotationPoint = rotator.transform.TransformPoint(rotator.RotationPoint);
            Gizmos.DrawLine(worldRotationPoint,
                worldRotationPoint + rotator.transform.TransformDirection(Vector3.up) * 7);
        }

        [DrawGizmo(GizmoType.InSelectionHierarchy)]
        static void DrawGizmosSelected(RotationRedirector rotator, GizmoType gizmoType)
        {
            Gizmos.DrawIcon(rotator.transform.TransformPoint(rotator.RotationPoint), "SpaceExtenderTool/rotationIcon",
                true);
        }


    }
}
