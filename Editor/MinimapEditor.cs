using CurvatureGames.SpaceExtender;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CurvatureGames.SpaceExtenderEditor
{
    [CustomEditor(typeof(Minimap))]
    public class MinimapEditor : UnityEditor.Editor
    {

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
        }

        public override VisualElement CreateInspectorGUI()
        {
            var visualTree = Resources.Load("Space Extender/UXML/Minimap_Inspector") as VisualTreeAsset;
            var uxml = visualTree.CloneTree();
            uxml.Q<Button>("regenerate-map").clickable.clicked += () => { (target as Minimap).RegenerateMap(); };

            return uxml;
        }
    }
}
