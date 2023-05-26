using Cinemachine;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public static class Extensions
{
    /// <summary>
    /// include root transform
    /// </summary>
    public static Transform FindDeepChild(this Transform trans, string name)
    {
        if (trans.name == name) return trans;
        Queue<Transform> queue = new Queue<Transform>();
        queue.Enqueue(trans);
        while (queue.Count > 0)
        {
            var c = queue.Dequeue();
            if (c.name == name)
                return c;
            foreach (Transform t in c)
                queue.Enqueue(t);
        }

        return null;
    }

    public static int FindDeepChildren(this Transform trans, string name, List<Transform> children)
    {
        children.Clear();
        var queue = new Queue<Transform>();

        queue.Enqueue(trans);
        while (queue.Count > 0)
        {
            var c = queue.Dequeue();
            if (c.name == name)
            {
                children.Add(c);
            }

            foreach (Transform t in c)
            {
                queue.Enqueue(t);
            }
        }

        return children.Count;
    }

    public static T GetComponent<T>(this Component component, string childName)
    {
        var child = FindDeepChild(component.transform, childName);
        return !child ? default : child.GetComponent<T>();
    }


    private static readonly ConditionalWeakTable<GameObject, Dictionary<Type, Component>> m_componentCache =
        new ConditionalWeakTable<GameObject, Dictionary<Type, Component>>();

    public static TComponent GetCachedComponent<TComponent>(this Component component) where TComponent : Component
    {
        if (object.ReferenceEquals(component, null))
        {
            throw new ArgumentNullException("component");
        }

        return component.gameObject.GetCachedComponent<TComponent>();
    }

    public static TComponent GetCachedComponent<TComponent>(this GameObject gameObject) where TComponent : Component
    {
        if (object.ReferenceEquals(gameObject, null))
        {
            throw new ArgumentNullException("gameObject");
        }

        if (typeof(TComponent) == typeof(Transform))
        {
            throw new InvalidOperationException(
                "Shouldn't use GetCachedComponent<>() for Transform components. Use gameObject.transform property instead, which is faster.");
        }

        Dictionary<Type, Component> componentCache = m_componentCache.GetOrCreateValue(gameObject);
        Component component;
        if (!componentCache.TryGetValue(typeof(TComponent), out component))
        {
            component = gameObject.GetComponent<TComponent>();
            componentCache.Add(typeof(TComponent), component);
        }

        return component as TComponent;
    }

    public static void SetLocalScaleX(this Transform transform, float scaleX)
    {
        transform.localScale = new Vector3(scaleX, transform.localScale.y, transform.localScale.z);
    }

    public static Coroutine SetTimeout(this MonoBehaviour mono, Action cb, float time)
    {
        if (time < 0 || cb == null)
        {
            Debug.Log("set timeout param error");
            return null;
        }

        if (Mathf.Approximately(0, time))
        {
            cb.Invoke();
            return null;
        }

        IEnumerator Coroutine()
        {
            yield return new WaitForSeconds(time);
            cb.Invoke();
        }

        return mono.StartCoroutine(Coroutine());
    }

    public static Coroutine SetInterval(this MonoBehaviour mono, Action cb, float time)
    {
        if (time <= 0 || cb == null)
        {
            Debug.Log("set timeout param error");
            return null;
        }

        IEnumerator Coroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(time);
                cb.Invoke();
            }
        }

        return mono.StartCoroutine(Coroutine());
    }

    public static Coroutine CallEndOfTheFrame(this MonoBehaviour mono, Action cb)
    {
        if (cb == null)
        {
            Debug.Log("set timeout param error");
            return null;
        }

        IEnumerator Coroutine()
        {
            yield return new WaitForEndOfFrame();
            cb.Invoke();
        }

        return mono.StartCoroutine(Coroutine());
    }

    public static void SetAlpha(this Image image, float alpha)
    {
        image.color = new Color(image.color.r, image.color.g, image.color.b, alpha);
    }

    public static void SetAlpha(this SpriteRenderer sprite, float alpha)
    {
        sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, alpha);
    }

    public static T Find<T>(this HashSet<T> set, Func<T, bool> indicate)
    {
        foreach (var item in set)
        {
            if (indicate(item)) return item;
        }

        return default;
    }

    public static bool Remove<T>(this IList<T> list, Func<T, bool> indicate)
    {
        for (int i = list.Count - 1; i >= 0; i--)
        {
            if (indicate(list[i]))
            {
                list.RemoveAt(i);
                return true;
            }
        }

        return false;
    }

    public static T Random<T>(this IList<T> list)
    {
        if (list.Count == 0) return default;
        return list[UnityEngine.Random.Range(0, list.Count)];
    }

    /// <summary>
    /// weight必须大于等于0
    /// </summary>
    public static T Random<T>(this IList<T> list, Func<T, int> getWeight)
    {
        var totalWeight = list.Select(i => Mathf.Max(0, getWeight(i))).Sum();

        var curWeight = 0;
        var randomWeight = UnityEngine.Random.Range(1, totalWeight + 1);
        foreach (var item in list)
        {
            curWeight += Mathf.Max(0, getWeight(item));
            if (curWeight >= randomWeight)
                return item;
        }

        Debug.LogWarning("Weighted random failed, use normal random");
        return list.Random();
    }

    public static TSource MaxSource<TSource, TValue>(this IList<TSource> list, Func<TSource, TValue> getValue)
    {
        if (list.Count == 0) return default;
        if (list.Count == 1) return list[0];
        TSource maxSource = list[0];
        TValue maxValue = getValue(maxSource);
        Comparer<TValue> comparer = Comparer<TValue>.Default;

        for (int i = 1; i < list.Count; i++)
        {
            var value = getValue(list[i]);
            if (comparer.Compare(value, maxValue) > 0)
            {
                maxValue = value;
                maxSource = list[i];
            }
        }

        return maxSource;
    }

    private static System.Random rng = new System.Random();

    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    /// <summary>
    /// Check if Value is inside a range (greater equals min && less equals than Max)
    /// </summary>
    /// <param name="value">value to compare</param>
    /// <param name="minMaxRange">range (x min,y max)</param>
    /// <returns></returns>
    public static bool IsInSideRange(this float value, Vector2 minMaxRange)
    {
        return value >= minMaxRange.x && value <= minMaxRange.y;
    }

    public static Vector3[] MakeSmoothCurve(this Vector3[] pts, float smoothFactor = 0.25f)
    {
        smoothFactor = Mathf.Clamp(smoothFactor, 0.1f, 0.9f);
        Vector3[] newPts = new Vector3[(pts.Length - 2) * 2 + 2];
        try
        {
            newPts[0] = pts[0];
            newPts[newPts.Length - 1] = pts[pts.Length - 1];

            int j = 1;
            for (int i = 0; i < pts.Length - 2; i++)
            {
                newPts[j] = pts[i] + (pts[i + 1] - pts[i]) * (1f - smoothFactor);
                newPts[j + 1] = pts[i + 1] + (pts[i + 2] - pts[i + 1]) * smoothFactor;
                j += 2;
            }
        }
        catch
        {
            newPts = pts;
        }

        return newPts;
    }

    public static List<Vector3> MakeSmoothCurve(this List<Vector3> pts, float smoothFactor = 0.25f)
    {
        smoothFactor = Mathf.Clamp(smoothFactor, 0.1f, 0.9f);
        List<Vector3> newPts = new List<Vector3>((pts.Count - 2) * 2 + 2);
        try
        {

            newPts[0] = pts[0];
            newPts[newPts.Count - 1] = pts[pts.Count - 1];

            int j = 1;
            for (int i = 0; i < pts.Count - 2; i++)
            {
                newPts[j] = pts[i] + (pts[i + 1] - pts[i]) * (1f - smoothFactor);
                newPts[j + 1] = pts[i + 1] + (pts[i + 2] - pts[i + 1]) * smoothFactor;
                j += 2;
            }
        }
        catch
        {
            newPts = pts;
        }

        return newPts;
    }

    public static void SetLayerRecursively(this GameObject obj, int layer)
    {
        obj.layer = layer;

        foreach (Transform child in obj.transform)
        {
            child.gameObject.SetLayerRecursively(layer);
        }
    }

    public static bool ContainsLayer(this LayerMask layermask, int layer)
    {
        return layermask == (layermask | (1 << layer));
    }

    public static void SetActiveChildren(this GameObject gameObjet, bool value)
    {
        foreach (Transform child in gameObjet.transform)
        {
            child.gameObject.SetActive(value);
        }
    }


    /// <summary>
    /// Normalized the angle. between -180 and 180 degrees
    /// </summary>
    /// <param Name="eulerAngle">Euler angle.</param>
    public static Vector3 NormalizeAngle(this Vector3 eulerAngle)
    {
        var delta = eulerAngle;

        if (delta.x > 180)
        {
            delta.x -= 360;
        }
        else if (delta.x < -180)
        {
            delta.x += 360;
        }

        if (delta.y > 180)
        {
            delta.y -= 360;
        }
        else if (delta.y < -180)
        {
            delta.y += 360;
        }

        if (delta.z > 180)
        {
            delta.z -= 360;
        }
        else if (delta.z < -180)
        {
            delta.z += 360;
        }

        return new Vector3(delta.x, delta.y, delta.z); //round values to angle;
    }

    public static Vector3 Difference(this Vector3 vector, Vector3 otherVector)
    {
        return otherVector - vector;
    }

    public static Vector3 AngleFormOtherDirection(this Vector3 directionA, Vector3 directionB)
    {
        return Quaternion.LookRotation(directionA).eulerAngles
            .AngleFormOtherEuler(Quaternion.LookRotation(directionB).eulerAngles);
    }

    public static Vector3 AngleFormOtherDirection(this Vector3 directionA, Vector3 directionB, Vector3 up)
    {
        return Quaternion.LookRotation(directionA, up).eulerAngles
            .AngleFormOtherEuler(Quaternion.LookRotation(directionB, up).eulerAngles);
    }

    public static Vector3 AngleFormOtherEuler(this Vector3 eulerA, Vector3 eulerB)
    {
        Vector3 angles = eulerA.NormalizeAngle().Difference(eulerB.NormalizeAngle()).NormalizeAngle();
        return angles;
    }

    public static string ToStringColor(this bool value)
    {
        if (value) return "<color=green>YES</color>";
        else return "<color=red>NO</color>";
    }

    public static float ClampAngle(float angle, float min, float max)
    {
        do
        {
            if (angle < -360)
            {
                angle += 360;
            }

            if (angle > 360)
            {
                angle -= 360;
            }
        } while (angle < -360 || angle > 360);

        return Mathf.Clamp(angle, min, max);
    }

    public static Vector3 BoxSize(this BoxCollider boxCollider)
    {
        var length = boxCollider.transform.lossyScale.x * boxCollider.size.x;
        var width = boxCollider.transform.lossyScale.z * boxCollider.size.z;
        var height = boxCollider.transform.lossyScale.y * boxCollider.size.y;
        return new Vector3(length, height, width);
    }

    public static bool IsClosed(this BoxCollider boxCollider, Vector3 position, Vector3 margin, Vector3 centerOffset)
    {
        var size = boxCollider.BoxSize();
        var marginX = margin.x;
        var marginY = margin.y;
        var marginZ = margin.z;
        var center = boxCollider.center + centerOffset;
        Vector2 rangeX = new Vector2((center.x - (size.x * 0.5f)) - marginX, (center.x + (size.x * 0.5f)) + marginX);
        Vector2 rangeY = new Vector2((center.y - (size.y * 0.5f)) - marginY, (center.y + (size.y * 0.5f)) + marginY);
        Vector2 rangeZ = new Vector2((center.z - (size.z * 0.5f)) - marginZ, (center.z + (size.z * 0.5f)) + marginZ);
        position = boxCollider.transform.InverseTransformPoint(position);

        bool inX = (position.x * boxCollider.transform.lossyScale.x).IsInSideRange(rangeX);
        bool inY = (position.y * boxCollider.transform.lossyScale.y).IsInSideRange(rangeY);
        bool inZ = (position.z * boxCollider.transform.lossyScale.z).IsInSideRange(rangeZ);

        return inX && inY && inZ;
    }

    public static T ToEnum<T>(this string value, bool ignoreCase = true)
    {
        return (T) Enum.Parse(typeof(T), value, ignoreCase);
    }

    public static bool Contains<T>(this Enum value, Enum lookingForFlag) where T : struct
    {
        int intValue = (int) (object) value;
        int intLookingForFlag = (int) (object) lookingForFlag;
        return ((intValue & intLookingForFlag) == intLookingForFlag);
    }

    public static void SetLinearLimit(this ConfigurableJoint joint, float limit)
    {
        var linearLimit = joint.linearLimit;
        linearLimit.limit = limit;
        joint.linearLimit = linearLimit;
    }

    public static Tween DoLinearLimit(this ConfigurableJoint joint, float limit, float duration)
    {
        return DOTween.To(() => joint.linearLimit.limit, joint.SetLinearLimit, limit, duration).SetTarget(joint);
    }
}
