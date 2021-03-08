
using CurvatureGames.SpaceExtender;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace CurvatureGames.SpaceExtenderEditor
{
    [CustomEditor(typeof(OverlappingRedirector))]
    public class OverlappingRedirectorEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var baseUxml = base.CreateInspectorGUI();
            var visualTree = Resources.Load("Space Extender/UXML/OverlappingRedirector_Inspector") as VisualTreeAsset;
            var uxml = visualTree.CloneTree();

            uxml.Add(baseUxml);

            return uxml;
        }
    }
}
