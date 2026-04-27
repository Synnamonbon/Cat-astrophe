#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(Transform)), CanEditMultipleObjects]
public class CenterTransformEditor : Editor
{
    private Vector3 cachedFirstCenter;
    private Dictionary<Transform, Vector3> cachedCenters = new Dictionary<Transform, Vector3>();
    private bool centersCached = false;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (Event.current.type == EventType.Layout)
        {
            cachedCenters.Clear();
            foreach (var obj in targets)
            {
                Transform t = (Transform)obj;
                Renderer r = t.GetComponentInChildren<Renderer>();
                if (r != null)
                    cachedCenters[t] = r.bounds.center;
            }

            if (cachedCenters.Count > 0)
            {
                bool first = true;
                foreach (var c in cachedCenters.Values)
                {
                    if (first) { cachedFirstCenter = c; first = false; }
                }
                centersCached = true;
            }
            else
            {
                centersCached = false;
            }
        }

        if (!centersCached || cachedCenters.Count == 0) return;

        bool mixX = false, mixY = false, mixZ = false;
        bool f = true;
        foreach (var c in cachedCenters.Values)
        {
            if (f) { f = false; continue; }
            if (!Mathf.Approximately(c.x, cachedFirstCenter.x)) mixX = true;
            if (!Mathf.Approximately(c.y, cachedFirstCenter.y)) mixY = true;
            if (!Mathf.Approximately(c.z, cachedFirstCenter.z)) mixZ = true;
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Center Position");

        // Track each axis independently
        EditorGUI.BeginChangeCheck();
        EditorGUI.showMixedValue = mixX;
        float newX = EditorGUILayout.FloatField(cachedFirstCenter.x);
        bool changedX = EditorGUI.EndChangeCheck();

        EditorGUI.BeginChangeCheck();
        EditorGUI.showMixedValue = mixY;
        float newY = EditorGUILayout.FloatField(cachedFirstCenter.y);
        bool changedY = EditorGUI.EndChangeCheck();

        EditorGUI.BeginChangeCheck();
        EditorGUI.showMixedValue = mixZ;
        float newZ = EditorGUILayout.FloatField(cachedFirstCenter.z);
        bool changedZ = EditorGUI.EndChangeCheck();

        EditorGUI.showMixedValue = false;
        EditorGUILayout.EndHorizontal();

        if (changedX || changedY || changedZ)
        {
            // Snapshot the cached data so it can't shift during modification
            var snapshot = new Dictionary<Transform, Vector3>(cachedCenters);

            var toModify = new List<Transform>(snapshot.Keys);
            Undo.RecordObjects(toModify.ToArray(), "Move Centers");

            foreach (var t in toModify)
            {
                Vector3 ownCenter = snapshot[t];
                Vector3 delta = Vector3.zero;

                if (changedX) delta.x = newX - ownCenter.x;
                if (changedY) delta.y = newY - ownCenter.y;
                if (changedZ) delta.z = newZ - ownCenter.z;

                t.position += delta;
            }

            foreach (var t in toModify)
            {
                if (PrefabUtility.IsPartOfPrefabInstance(t))
                    PrefabUtility.RecordPrefabInstancePropertyModifications(t);
                EditorUtility.SetDirty(t);
            }

            // Force cache refresh next frame
            centersCached = false;
        }
    }
}
#endif