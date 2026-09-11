using System.Collections.Generic;
using UnityEngine;

public class Pool<T> where T : Component
{
    private readonly T prefab;
    private readonly Transform parent;
    private readonly Stack<T> inactive = new();
    private readonly HashSet<T> inPool = new();

    public int CountInactive => inactive.Count;

    public Pool(T prefab, Transform parent = null, int preload = 0)
    {
        this.prefab = prefab;
        this.parent = parent;

        for (int i = 0; i < preload; i++)
        {
            Release(Object.Instantiate(prefab, parent));
        }
    }

    public T Get()
    {
        T item = TakeInactive();
        if (item == null) return Object.Instantiate(prefab, parent);

        item.gameObject.SetActive(true);
        return item;
    }

    public T Get(Vector3 position, Quaternion rotation)
    {
        T item = TakeInactive();
        if (item == null) return Object.Instantiate(prefab, position, rotation, parent);

        item.transform.SetPositionAndRotation(position, rotation);
        item.gameObject.SetActive(true);
        return item;
    }

    public void Release(T item)
    {
        if (item == null || !inPool.Add(item)) return;

        item.gameObject.SetActive(false);
        inactive.Push(item);
    }

    public void Clear()
    {
        foreach (T item in inactive)
        {
            if (item != null) Object.Destroy(item.gameObject);
        }

        inactive.Clear();
        inPool.Clear();
    }

    private T TakeInactive()
    {
        while (inactive.Count > 0)
        {
            T item = inactive.Pop();
            inPool.Remove(item);

            // Skip items destroyed from outside while sitting in the pool.
            if (item != null) return item;
        }

        return null;
    }
}
