using System;
using System.Collections.Generic;
using UnityEngine;

namespace GorillaAchievements;

// Here's an example on how to do extensions
public static class Extensions
{
    /// <summary>
    /// Add a key value pair to a dictionary or return false if it has already been added.
    /// </summary>
    /// <typeparam name="TKey">The key type of the dictionary.</typeparam>
    /// <typeparam name="TValue">The value type of the dictionary.</typeparam>
    /// <param name="dict">The dictionary to add the pair to.</param>
    /// <param name="key">The key to add to the dictionary.</param>
    /// <param name="value">The value to add to the dictionary.</param>
    /// <returns>true if the pair was added, false if the key was already inserted into the dictionary.</returns>
    /// <exception cref="ArgumentNullException">key or value is null.</exception>
    public static bool TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue value)
    {
        if (key is null) throw new ArgumentNullException(nameof(key));
        if (value is null) throw new ArgumentNullException(nameof(value));

        if (dict.ContainsKey(key))
            return false;

        dict.Add(key, value);

        return true;
    }

    /// <summary>
    /// Find a child of a gameobject
    /// </summary>

    public static GameObject Find(this GameObject go, string name)
    {
        return go.transform.Find(name)?.gameObject;
    }
}
