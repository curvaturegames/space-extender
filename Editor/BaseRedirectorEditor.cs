using System.Collections;
using System.Collections.Generic;
using CurvatureGames.SpaceExtender;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;

namespace CurvatureGames.SpaceExtenderEditor
{

    /// <summary>
    /// Responsible for custom editor visualizations that all Redirectors share.
    /// Draws start and end playarea, start playarea outline and some inspector variables.
    /// </summary>
    [CanEditMultipleObjects]
    public class BaseRedirectorEditor : UnityEditor.Editor
    {
        // predefined colors
        protected static Color startPlayAreaColorNotSelected = new Color(1, 0, 0, 0.3f);
        protected static Color endPlayAreaColorNotSelected = new Color(0, 0, 1, 0.3f);
        protected static Color startPlayAreaColorSelected = new Color(1, 0, 0, 0.7f);
        protected static Color endPlayAreaColorSelected = new Color(0, 0, 1, 0.7f);
        protected static Color outlineColor = Color.red;

        TemplateContainer uxml;

        protected virtual void OnEnable()
        {

        }

        public override VisualElement CreateInspectorGUI()
        {
            var visualTree = Resources.Load("Space Extender/UXML/BaseRedirector_Inspector") as VisualTreeAsset;
            uxml = visualTree.CloneTree();
            uxml.styleSheets.Add(Resources.Load("Space Extender/USS/BaseRedirector_Inspector_USS") as StyleSheet);

            //show warning if redirection-object-field == null
            uxml.Q("redirection-object-warning").Add(new IMGUIContainer(ShowRedirectionWarning));

            return uxml;
        }

        private void ShowRedirectionWarning()
        {
            var objField = uxml.Q("redirection-object-field").Q<ObjectField>();
            if (objField != null && objField.value == null)
            {
                EditorGUILayout.HelpBox(
                    "Please set Redirection Object! \nFor redirection to work, it must have the VR playarea as one of it's children.",
                    MessageType.Error, true);
            }
        }

        protected virtual void OnSceneGUI()
        {

        }

        [DrawGizmo(GizmoType.NonSelected)]
        protected static void DrawGizmosNonSelected(BaseRedirector redirector, GizmoType gizmoType)
        {
            // draw both playareas
            Gizmos.color = startPlayAreaColorNotSelected;
            DrawPlayArea(redirector, redirector.StartPlayAreaPosition, redirector.StartPlayAreaRotation);
            Gizmos.color = endPlayAreaColorNotSelected;
            DrawPlayArea(redirector, redirector.EndPlayAreaPosition, redirector.EndPlayAreaRotation);
        }


        [DrawGizmo(GizmoType.InSelectionHierarchy)]
        protected static void DrawGizmosSelected(BaseRedirector redirector, GizmoType gizmoType)
        {
            // get mesh's vertex-positions in order to draw the outline
            Vector3[] linePoints = new Vector3[5];
            Vector3[] gizmoVerts = redirector.GizmoPlayAreaMesh.vertices;
            // put line points in correct order
            linePoints[0] = gizmoVerts[0];
            linePoints[1] = gizmoVerts[1];
            linePoints[2] = gizmoVerts[3];
            linePoints[3] = gizmoVerts[2];
            linePoints[4] = gizmoVerts[0];
            // scale points correctly
            for (int i = 0; i < linePoints.Length; i++)
            {
                linePoints[i] = redirector.transform.TransformPoint(new Vector3(
                    linePoints[i].x * redirector.PlayAreaDimensions.x, linePoints[i].y,
                    linePoints[i].z * redirector.PlayAreaDimensions.y));
            }

            Handles.color = outlineColor;
            Handles.DrawAAPolyLine(10f, linePoints);

            //Draw both playareas
            Gizmos.color = endPlayAreaColorSelected;
            DrawPlayArea(redirector, redirector.EndPlayAreaPosition, redirector.EndPlayAreaRotation);
            Gizmos.color = startPlayAreaColorSelected;
            DrawPlayArea(redirector, redirector.StartPlayAreaPosition, redirector.StartPlayAreaRotation);
        }

        /// <summary>
        /// Draws the playarea as a gizmo inside the scene view.
        /// </summary>
        /// <param name="redirector"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        protected static void DrawPlayArea(BaseRedirector redirector, Vector3 position, Quaternion rotation)
        {
            Gizmos.DrawMesh(redirector.GizmoPlayAreaMesh, position, rotation,
                new Vector3(redirector.PlayAreaDimensions.x, 0f, redirector.PlayAreaDimensions.y));
        }
    }
}
