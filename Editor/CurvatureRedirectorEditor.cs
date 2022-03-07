using CurvatureGames.SpaceExtender;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace CurvatureGames.SpaceExtenderEditor
{
    /// <summary>
    /// Responsible for custom editor visualizations specific to the curvature redirector.
    /// </summary>
    [CustomEditor(typeof(CurvatureRedirector))]
    public class CurvatureRedirectorEditor : BaseRedirectorEditor
    {
        protected static Color realEndPlayAreaColorNotSelected = new Color(0, 1, 0, 0.3f);
        protected static Color realEndPlayAreaColorSelected = new Color(0, 1, 0, 0.7f);

        protected override void OnEnable()
        {
            base.OnEnable();
        }

        public override VisualElement CreateInspectorGUI()
        {
            var baseUxml = base.CreateInspectorGUI();
            var visualTree = Resources.Load("Space Extender/UXML/CurvatureRedirector_Inspector") as VisualTreeAsset;
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

            serializedObject.ApplyModifiedProperties();
        }

        [DrawGizmo(GizmoType.NonSelected)]
        protected static void DrawGizmosNonSelected(CurvatureRedirector redirector, GizmoType gizmoType)
        {
            // draw virtual path
            var prevZTest = Handles.zTest;
            Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;

            Handles.color = endPlayAreaColorNotSelected;
            Handles.DrawLine(redirector.StartPlayAreaPosition, redirector.EndPlayAreaPosition, 10.0f);

            Handles.zTest = prevZTest;

            // draw real end playarea
            Gizmos.color = realEndPlayAreaColorNotSelected;
            DrawPlayArea(redirector, redirector.RealEndPlayAreaPosition, redirector.RealEndPlayAreaRotation);

            // draw real path
            prevZTest = Handles.zTest;
            Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;

            Handles.color = realEndPlayAreaColorNotSelected;

            Vector3[] pathPositions = redirector.RealPathPositions;
            for (int i = 0; i < pathPositions.Length - 1; ++i)
            {
                Handles.DrawLine(pathPositions[i], pathPositions[i + 1], 10.0f);
            }

            Handles.zTest = prevZTest;
        }

        [DrawGizmo(GizmoType.InSelectionHierarchy)]
        protected static void DrawGizmosSelected(CurvatureRedirector redirector, GizmoType gizmoType)
        {
            // draw real end playarea
            Gizmos.color = realEndPlayAreaColorSelected;
            DrawPlayArea(redirector, redirector.RealEndPlayAreaPosition, redirector.RealEndPlayAreaRotation);

            // draw virtual path
            var prevZTest = Handles.zTest;
            Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;

            Handles.color = endPlayAreaColorSelected;
            Handles.DrawLine(redirector.StartPlayAreaPosition, redirector.EndPlayAreaPosition, 10.0f);

            // draw real path
            Handles.color = realEndPlayAreaColorSelected;
            Vector3[] pathPositions = redirector.RealPathPositions;
            for (int i = 0; i < pathPositions.Length - 1; ++i)
            {
                Handles.DrawLine(pathPositions[i], pathPositions[i + 1], 10.0f);
            }

            Handles.zTest = prevZTest;
        }
    }
}
