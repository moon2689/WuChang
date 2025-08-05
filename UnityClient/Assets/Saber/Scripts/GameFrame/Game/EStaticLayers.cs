using UnityEngine;

public enum EStaticLayers
{
    Default = 0,
    TransparentFX = 1,
    IgnoreRaycast = 2,
    Water = 4,
    UI = 5,

    Actor = 6,
    Collider = 7,
}

public static class StaticLayersHelper
{
    public static int GetLayer(this EStaticLayers layer)
    {
        return LayerMask.NameToLayer(layer.ToString());
    }

    public static LayerMask GetLayerMask(this EStaticLayers layer)
    {
        return 1 << layer.GetLayer();
    }
}