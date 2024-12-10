using UnityEngine;

public class IgnoreCollision : MonoBehaviour
{
    [Header("Layer Settings")]
    public string ignoredLayerName = "Enemy";

    private void Start()
    {
        int currentLayer = gameObject.layer;
        int ignoredLayer = LayerMask.NameToLayer(ignoredLayerName);

        if (ignoredLayer != -1)
        {
            Physics2D.IgnoreLayerCollision(currentLayer, ignoredLayer, true);
        }
        else
        {
            Debug.LogError($"Layer '{ignoredLayerName}' không tồn tại! Vui lòng kiểm tra lại.");
        }
    }
}
