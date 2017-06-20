using System;
using UnityEngine;

[Serializable]
public struct SynchronizedMaterial
{
    public Material Source, Destination;

    public void SyncProperties()
    {
        if (Source != null && Destination != null)
        {
            Destination.CopyPropertiesFromMaterial(Source);
        }
    }

    public SynchronizedMaterial(
        Material source,
        Material destination)
    {
        Source = source;
        Destination = destination;
    }
}