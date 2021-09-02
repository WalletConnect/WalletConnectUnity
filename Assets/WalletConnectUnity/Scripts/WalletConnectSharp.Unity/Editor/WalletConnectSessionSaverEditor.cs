

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using WalletConnectSharp.Unity;

[CustomEditor(typeof(WalletConnectSessionSaver))]
public class WalletConnectSessionSaverEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var saver = (WalletConnectSessionSaver) target;

        if (GUILayout.Button("Clear Session"))
        {
            saver.CLearSession();
        }
    }
}

#endif