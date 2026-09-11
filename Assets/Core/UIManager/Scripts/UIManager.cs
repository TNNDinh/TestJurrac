using System.Collections.Generic;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    [Tooltip("Parent for spawned features (usually a Canvas). Falls back to this transform when empty.")]
    [SerializeField] private Transform root;
    [SerializeField] private FeatureBase[] featurePrefabs;

    private readonly Dictionary<FeatureType, FeatureBase> prefabs = new();
    private readonly Dictionary<FeatureType, FeatureBase> opened = new();

    protected override void Awake()
    {
        base.Awake();
        if (root == null) root = transform;

        foreach (FeatureBase prefab in featurePrefabs)
        {
            prefabs[prefab.FeatureType] = prefab;
        }
    }

    public T Show<T>(FeatureType type) where T : FeatureBase
    {
        return Show(type) as T;
    }

    public FeatureBase Show(FeatureType type)
    {
        if (opened.TryGetValue(type, out FeatureBase feature) && feature != null && feature.IsShowing)
        {
            return feature;
        }

        if (!prefabs.TryGetValue(type, out FeatureBase prefab))
        {
            Debug.LogError($"[UIManager] No prefab registered for {type}");
            return null;
        }

        feature = Instantiate(prefab, root);
        opened[type] = feature;
        feature.Show();
        return feature;
    }

    public void Close(FeatureType type)
    {
        if (opened.TryGetValue(type, out FeatureBase feature) && feature != null)
        {
            feature.Close();
        }

        opened.Remove(type);
    }

    public bool IsShowing(FeatureType type)
    {
        return opened.TryGetValue(type, out FeatureBase feature) && feature != null && feature.IsShowing;
    }
}
