using CurvatureGames.SpaceExtender;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace CurvatureGames.SpaceExtenderEditor
{
    /// <summary>
    /// Responsible for custom editor visualizations specific to the translation redirector.
    /// </summary>
    [CustomEditor(typeof(TranslationRedirector))]
    public class TranslationRedirectorEditor : BaseRedirectorEditor
    {

        TemplateContainer uxml;
        
        protected override void OnEnable()
        {
            base.OnEnable();
        }

        public override VisualElement CreateInspectorGUI()
        {
            var baseUxml = base.CreateInspectorGUI();
            var visualTree = Resources.Load("Space Extender/UXML/TranslationRedirector_Inspector") as VisualTreeAsset;
            uxml = visualTree.CloneTree();
            uxml.styleSheets.Add(Resources.Load("Space Extender/USS/BaseRedirector_Inspector_USS") as StyleSheet);

            uxml.Q("forward-gain-warning").Add(new IMGUIContainer(ShowForwardGainWarning));
            uxml.Q("backward-gain-warning").Add(new IMGUIContainer(ShowBackwardGainWarning));

            uxml.Add(baseUxml);

            return uxml;
        }

        private void ShowForwardGainWarning()
        {
            var objectFieldAmount = uxml.Q("translation-amount-field").Q<FloatField>();
            var objectFieldGain = uxml.Q("forward-translation-gain-field").Q<FloatField>();

            if ((objectFieldAmount.value < 0.0f && objectFieldGain.value >= 0.0f) || objectFieldAmount.value > 0.0f && objectFieldGain.value <= 0.0f)
            {
                EditorGUILayout.HelpBox(
                    "Forward Translation Gain and Translation Amount must show in the same direction (+/+ or -/-).",
                    MessageType.Error, true);
            }
        }

        private void ShowBackwardGainWarning()
        {
            var objectFieldAmount = uxml.Q("translation-amount-field").Q<FloatField>();
            var objectFieldGain = uxml.Q("backward-translation-gain-field").Q<FloatField>();

            if ((objectFieldAmount.value < 0.0f && objectFieldGain.value <= 0.0f) || objectFieldAmount.value > 0.0f && objectFieldGain.value >= 0.0f)
            {
                EditorGUILayout.HelpBox(
                    "Backward Translation Gain and Translation Amount must show in opposite direction (+/- or -/+).",
                    MessageType.Error, true);
            }
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
