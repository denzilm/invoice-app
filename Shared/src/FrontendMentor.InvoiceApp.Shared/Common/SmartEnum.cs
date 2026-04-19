using System.Collections.Concurrent;
using System.Reflection;

namespace FrontendMentor.InvoiceApp.Shared.Common;

/// <summary>
/// Base class for creating type-safe enumerations with additional properties
/// </summary>
/// <typeparam name="T">The derived type inherits from SmartEnum</typeparam>
public abstract class SmartEnum<T> : IEquatable<T> where T : SmartEnum<T>
{
    private static readonly ConcurrentDictionary<Type, IReadOnlyList<T>> Cache = new();

    protected SmartEnum(int value, string name, string displayName)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException($"{nameof(Name)} cannot be null or whitespace.", nameof(name));
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException($"{nameof(DisplayName)} cannot be null or whitespace.", nameof(displayName));

        Name = name;
        Value = value;
        DisplayName = displayName;
    }

    public string Name { get; }
    public int Value { get; }
    public string DisplayName { get; }

    /// <summary>
    /// Lists all instances of the SmartEnum type T
    /// </summary>
    /// <returns></returns>
    public static IReadOnlyList<T> Enumerate()
    {
        return Cache.GetOrAdd(typeof(T), static type =>
        {
            return type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Where(f => f.FieldType == typeof(T))
                .Select(f => (T)f.GetValue(null)!)
                .ToList();
        });
    }

    /// <summary>
    /// Attempts to get the SmartEnum instance that matches the provided value
    /// </summary>
    /// <param name="value">The value to search for</param>
    /// <returns>The matching SmartEnum instance</returns>
    /// <exception cref="ArgumentException">Thrown when no matching instance is found</exception>
    public static T FromValue(int value)
    {
        var matchingItem = Enumerate().FirstOrDefault(item => item.Value == value);
        if (matchingItem is null)
            throw new ArgumentException($"Value '{value}' is not a valid value for '{typeof(T).Name}'", nameof(value));

        return matchingItem;
    }

    /// <summary>
    /// Attempts to get the SmartEnum instance that matches the provided value
    /// </summary>
    /// <param name="value">The value to search for</param>
    /// <param name="result">When this method returns, contains the instance if found, otherwise null</param>
    /// <returns>true if a matching instance was found, otherwise false</returns>
    public static bool TryGetFromValue(int value, out T? result)
    {
        result = Enumerate().FirstOrDefault(item => item.Value == value);
        return result is not null;
    }

    /// <summary>
    /// Gets the SmartEnum instance that matches the provided name
    /// </summary>
    /// <param name="name">The name to search for</param>
    /// <returns>The matching SmartEnum instance</returns>
    /// <exception cref="ArgumentException">Thrown when no matching instance is found</exception>
    public static T FromName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException($"{nameof(Name)} cannot be null or whitespace.", nameof(name));

        var matchingItem = Enumerate().FirstOrDefault(item => item.Name == name);
        if (matchingItem is null)
            throw new ArgumentException($"'{name}' is not a valid name in {typeof(T)}", nameof(name));

        return matchingItem;
    }

    /// <summary>
    /// Attempts to get the SmartEnum instance with the specified name
    /// </summary>
    /// <param name="name">The name to search for</param>
    /// <param name="result">When this match returns, contains the matching instance if found otherwise null</param>
    /// <returns>true if a matching instance was found; otherwise false</returns>
    public static bool TryGetFromName(string name, out T? result)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            result = null;
            return false;
        }

        result = Enumerate().FirstOrDefault(item => item.Name == name);
        return result is not null;
    }

    public bool Equals(T? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return GetType() == other.GetType() && Value == other.Value;
    }

    public override bool Equals(object? obj) => obj is T other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(GetType(), Value);

    public static bool operator ==(SmartEnum<T>? left, SmartEnum<T>? right) => Equals(left, right);

    public static bool operator !=(SmartEnum<T>? left, SmartEnum<T>? right) => !(left == right);

    public override string ToString() => DisplayName;
}
