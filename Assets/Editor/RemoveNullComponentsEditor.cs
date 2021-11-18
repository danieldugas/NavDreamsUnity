using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof (RemoveNullComponents))]
public class RemoveNullComponentsEditor : Editor {

	public override void OnInspectorGUI () {
		DrawDefaultInspector ();

		RemoveNullComponents rmc = (RemoveNullComponents) target;

		if (GUILayout.Button ("Remove null Components")) {
			rmc.Clean ();
		}
	}
}