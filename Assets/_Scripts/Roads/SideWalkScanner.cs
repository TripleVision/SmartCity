#if UNITY_EDITOR
using MBaske.Sensors.Grid;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SideWalkScanner : MonoBehaviour
{
     [SerializeField, Tooltip("Optional settings for shape detection.")]
      private GameObjectShape m_Shape = new GameObjectShape();

    [ContextMenu("ScanSideWalks")]
    public void ScanSideWalks()
    {
        var detectableGameObjects = transform.GetComponentsInChildren<DetectableGameObject>();

        foreach (var item in detectableGameObjects)
        {
            item.m_Shape.m_ScanLOD = m_Shape.ScanLOD;
            item.m_Shape.m_GizmoLOD = m_Shape.m_GizmoLOD;
            item.ScanShape();
        }
        SaveData();
        Debug.Log($"Modified {detectableGameObjects.Length}");
    }

    void SaveData()
    {

        EditorUtility.SetDirty(gameObject);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
#endif
