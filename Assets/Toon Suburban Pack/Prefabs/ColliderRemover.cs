using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class ColliderRemoverEditor
{
    [MenuItem("Component/Remove All Colliders From Selection")]
    public static void RemoveAllColliders()
    {
        foreach (GameObject obj in Selection.gameObjects)
        {
            Collider[] colliders = obj.GetComponentsInChildren<Collider>(true);
            foreach (Collider col in colliders)
            {
                Object.DestroyImmediate(col, true);
            }
        }
    }
}
