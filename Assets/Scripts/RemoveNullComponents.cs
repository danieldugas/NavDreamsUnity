
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class RemoveNullComponents : MonoBehaviour
{
    public void Clean() {
        Clean(this.transform);
    }

    public void Clean(Transform obj) {
        GameObjectUtility.RemoveMonoBehavioursWithMissingScript(obj.gameObject);

        //Debug.Log(obj.childCount);
        for (int i = 0; i < obj.childCount; i++) {
            Clean(obj.GetChild(i));

        }

    }
}