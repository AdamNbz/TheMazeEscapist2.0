using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

#if UNITY_EDITOR

[RequireComponent(typeof(CompositeCollider2D))]
public class ShadowCaster2DCreator : MonoBehaviour
{
    [SerializeField]
    private bool selfShadows = true;

    private CompositeCollider2D tilemapCollider;

    // Unity 6: ch? c?n 2 field n�y
    static readonly FieldInfo shapePathField =
        typeof(ShadowCaster2D).GetField("m_ShapePath", BindingFlags.NonPublic | BindingFlags.Instance);

    static readonly FieldInfo shapePathHashField =
        typeof(ShadowCaster2D).GetField("m_ShapePathHash", BindingFlags.NonPublic | BindingFlags.Instance);

    public void Create()
    {
        DestroyOldShadowCasters();

        tilemapCollider = GetComponent<CompositeCollider2D>();
        if (tilemapCollider == null)
        {
            Debug.LogError("CompositeCollider2D not found!");
            return;
        }

        for (int i = 0; i < tilemapCollider.pathCount; i++)
        {
            Vector2[] pathVertices = new Vector2[tilemapCollider.GetPathPointCount(i)];
            tilemapCollider.GetPath(i, pathVertices);

            GameObject shadowCaster = new GameObject("shadow_caster_" + i);
            shadowCaster.transform.SetParent(transform, false);

            ShadowCaster2D caster = shadowCaster.AddComponent<ShadowCaster2D>();
            caster.selfShadows = selfShadows;

            // Convert Vector2 -> Vector3
            Vector3[] shapePath = new Vector3[pathVertices.Length];
            for (int j = 0; j < pathVertices.Length; j++)
            {
                shapePath[j] = pathVertices[j];
            }

            // Unity 6: set shape l� ??, mesh t? rebuild
            shapePathField.SetValue(caster, shapePath);
            shapePathHashField.SetValue(caster, Random.Range(int.MinValue, int.MaxValue));
        }
    }

    public void DestroyOldShadowCasters()
    {
        var children = transform.Cast<Transform>().ToList();
        foreach (var child in children)
        {
            DestroyImmediate(child.gameObject);
        }
    }
}

[CustomEditor(typeof(ShadowCaster2DCreator))]
public class ShadowCaster2DTileMapEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Create"))
        {
            ((ShadowCaster2DCreator)target).Create();
        }

        if (GUILayout.Button("Remove Shadows"))
        {
            ((ShadowCaster2DCreator)target).DestroyOldShadowCasters();
        }

        EditorGUILayout.EndHorizontal();
    }
}

#endif