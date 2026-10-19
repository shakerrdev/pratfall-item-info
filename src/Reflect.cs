using System;
using System.Collections.Generic;
using System.Reflection;

namespace ItemInfo;

internal static class Reflect
{
    private static readonly Dictionary<(Type, string), MemberInfo> Cache = new();

    public static object Get(object target, string name)
    {
        if (target == null)
            return null;

        var member = Find(target.GetType(), name);
        return member switch
        {
            FieldInfo f => f.GetValue(target),
            PropertyInfo p => p.GetValue(target),
            _ => null
        };
    }

    public static T Get<T>(object target, string name, T fallback = default) =>
        Get(target, name) is T value ? value : fallback;

    public static T NetVar<T>(object target, string name, T fallback = default)
    {
        var netVar = Get(target, name);
        return netVar == null ? fallback : Get(netVar, "Value", fallback);
    }

    private static MemberInfo Find(Type type, string name)
    {
        if (Cache.TryGetValue((type, name), out var cached))
            return cached;

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        MemberInfo member = null;
        for (var t = type; t != null && member == null; t = t.BaseType)
            member = (MemberInfo)t.GetField(name, flags | BindingFlags.DeclaredOnly)
                     ?? t.GetProperty(name, flags | BindingFlags.DeclaredOnly);

        Cache[(type, name)] = member;
        return member;
    }
}
