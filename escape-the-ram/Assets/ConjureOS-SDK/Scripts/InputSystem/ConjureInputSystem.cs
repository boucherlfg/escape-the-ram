using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem;

namespace ConjureOS.Input
{
#if UNITY_EDITOR
    [InitializeOnLoad] // Call static class constructor in editor.
#endif
    static public class ConjureInputSystem
    {
        static ConjureInputSystem()
        {
            Debug.Log("Mamamiiiiia");
#if UNITY_EDITOR
            InitializeInEditor();
#endif
        }
        
        static void InitializeInEditor()
        {
#if UNITY_EDITOR && ENABLE_INPUT_SYSTEM
            InputSystem.RegisterLayout<ConjureArcadeController>(
                matches: new InputDeviceMatcher()
                    .WithInterface("ConjureArcadeController"));
#endif
        }
    }
}