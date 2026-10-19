#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEditorInternal;

namespace LaResidencia
{
    /// <summary>
    /// Selecciona la puerta (el objeto con el MeshFilter / Door) y usa el menú La Residencia > Puerta.
    /// Crea un pivote vacío en el borde de la bisagra, reparenta la puerta y mueve el componente Door al pivote.
    /// Colocar este archivo dentro de una carpeta llamada "Editor".
    /// </summary>
    public static class DoorPivotTool
    {
        [MenuItem("La Residencia/Puerta/Crear pivote (bisagra en el borde -X local)")]
        private static void HingeMinX() => Create(false);

        [MenuItem("La Residencia/Puerta/Crear pivote (bisagra en el borde +X local)")]
        private static void HingeMaxX() => Create(true);

        private static void Create(bool maxSide)
        {
            Transform doorT = Selection.activeTransform;
            if (doorT == null) { Debug.LogWarning("[DoorPivotTool] Selecciona primero la malla de la puerta."); return; }

            MeshFilter mf = doorT.GetComponentInChildren<MeshFilter>();
            if (mf == null || mf.sharedMesh == null) { Debug.LogWarning("[DoorPivotTool] No se encontró MeshFilter."); return; }

            Bounds b = mf.sharedMesh.bounds;
            Vector3 local = new Vector3(maxSide ? b.max.x : b.min.x, b.center.y, b.center.z);
            Vector3 worldPoint = mf.transform.TransformPoint(local);
            // Si la bisagra queda en +X, giramos el pivote 180° para que el panel siga extendiéndose hacia +X del pivote.
            Quaternion rot = doorT.rotation * (maxSide ? Quaternion.Euler(0f, 180f, 0f) : Quaternion.identity);

            var pivot = new GameObject(doorT.name + "_Pivot");
            Undo.RegisterCreatedObjectUndo(pivot, "Crear pivote de puerta");
            pivot.transform.SetParent(doorT.parent, true);
            pivot.transform.SetPositionAndRotation(worldPoint, rot);
            pivot.transform.localScale = Vector3.one;

            Undo.SetTransformParent(doorT, pivot.transform, "Reparentar puerta");

            Door oldDoor = doorT.GetComponent<Door>();
            if (oldDoor != null)
            {
                ComponentUtility.CopyComponent(oldDoor);
                ComponentUtility.PasteComponentAsNew(pivot);
                Undo.DestroyObjectImmediate(oldDoor);

                Door newDoor = pivot.GetComponent<Door>();
                var so = new SerializedObject(newDoor);
                so.FindProperty("hinge").objectReferenceValue = null;   // el pivote ES la bisagra
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            Selection.activeGameObject = pivot;
            Debug.Log($"[DoorPivotTool] Pivote creado en {worldPoint}. Selecciónalo para ver los gizmos (verde = bisagra, rojo = panel, azul = normal).");
        }
    }
}
#endif
