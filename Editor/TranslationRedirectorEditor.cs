using CurvatureGames.SpaceExtender;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace CurvatureGames.SpaceExtenderEditor
{
    /// <summary>
    /// Responsible for custom editor visualizations specific to the translation redirector.
    /// </summary>
    [CustomEditor(typeof(TranslationRedirector))]
    public class TranslationRedirectorEditor : BaseRedirectorEditor
    {
        protected override void OnEnable()
        {
            base.OnEnable();
        }

        public override VisualElement CreateInspectorGUI()
        {
            var baseUxml = base.CreateInspectorGUI();
            var visualTree = Resources.Load("Space Extender/UXML/TranslationRedirector_Inspector") as VisualTreeAsset;
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
    }
}
