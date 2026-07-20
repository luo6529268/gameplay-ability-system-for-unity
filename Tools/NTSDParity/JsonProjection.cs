using System.Collections;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace NTSDParity;

internal static class JsonProjection
{
    public static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
    };

    public static object? Project(
        object? value,
        ISet<string>? excludedMembers = null,
        int depth = 0,
        ISet<string>? normalizedPathMembers = null)
    {
        if (value is null)
            return null;
        if (depth > 32)
            throw new InvalidOperationException($"Projection depth exceeded for {value.GetType().FullName}.");

        Type type = value.GetType();
        if (type.IsEnum)
            return Convert.ToInt64(value);
        if (value is string || value is char || value is bool ||
            value is byte || value is sbyte || value is short || value is ushort ||
            value is int || value is uint || value is long || value is ulong ||
            value is float || value is double || value is decimal)
        {
            return value;
        }

        if (value is Array array)
            return ProjectArray(array, excludedMembers, depth + 1, 0, [], normalizedPathMembers);

        if (value is IDictionary dictionary)
        {
            SortedDictionary<string, object?> result = new(StringComparer.Ordinal);
            foreach (DictionaryEntry entry in dictionary)
                result[Convert.ToString(entry.Key) ?? string.Empty] = Project(entry.Value, excludedMembers, depth + 1, normalizedPathMembers);
            return result;
        }

        if (value is IEnumerable enumerable)
        {
            List<object?> result = [];
            foreach (object? item in enumerable)
                result.Add(Project(item, excludedMembers, depth + 1, normalizedPathMembers));
            return result;
        }

        SortedDictionary<string, object?> projected = new(StringComparer.Ordinal);
        foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.Public).OrderBy(item => item.Name, StringComparer.Ordinal))
        {
            if (ShouldExclude(type, field.Name, excludedMembers))
                continue;
            object? fieldValue = field.GetValue(value);
            projected[field.Name] = NormalizeMember(type, field.Name, fieldValue, normalizedPathMembers)
                ?? Project(fieldValue, excludedMembers, depth + 1, normalizedPathMembers);
        }

        foreach (PropertyInfo property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public).OrderBy(item => item.Name, StringComparer.Ordinal))
        {
            if (!property.CanRead || property.GetIndexParameters().Length != 0 || projected.ContainsKey(property.Name))
                continue;
            if (ShouldExclude(type, property.Name, excludedMembers))
                continue;
            object? propertyValue = property.GetValue(value);
            projected[property.Name] = NormalizeMember(type, property.Name, propertyValue, normalizedPathMembers)
                ?? Project(propertyValue, excludedMembers, depth + 1, normalizedPathMembers);
        }
        return projected;
    }

    public static JsonNode? ToNode(object? value)
        => JsonSerializer.SerializeToNode(value, SerializerOptions);

    private static bool ShouldExclude(Type type, string memberName, ISet<string>? exclusions)
        => exclusions?.Contains(type.Name + "." + memberName) == true;

    private static object? NormalizeMember(
        Type type,
        string memberName,
        object? value,
        ISet<string>? normalizedPathMembers)
    {
        if (value is not string text || normalizedPathMembers?.Contains(type.Name + "." + memberName) != true)
            return null;
        return CanonicalJson.NormalizePureAssetPath(text);
    }

    private static object ProjectArray(
        Array array,
        ISet<string>? excludedMembers,
        int depth,
        int dimension,
        int[] indices,
        ISet<string>? normalizedPathMembers)
    {
        int length = array.GetLength(dimension);
        List<object?> values = new(length);
        for (int i = 0; i < length; i++)
        {
            int[] next = new int[indices.Length + 1];
            Array.Copy(indices, next, indices.Length);
            next[^1] = i;
            if (dimension + 1 == array.Rank)
                values.Add(Project(array.GetValue(next), excludedMembers, depth + 1, normalizedPathMembers));
            else
                values.Add(ProjectArray(array, excludedMembers, depth + 1, dimension + 1, next, normalizedPathMembers));
        }
        return values;
    }
}
