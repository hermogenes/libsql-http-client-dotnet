using System.Reflection;
using System.Text.Json;
using Xunit.Sdk;

namespace LibSql.Http.Client.Tests.Shared.Attributes;

public class JsonFileDataAttribute(string filePath, bool mapByProps = false) : DataAttribute
{
    public override IEnumerable<object?[]> GetData(MethodInfo testMethod)
    {
        ArgumentNullException.ThrowIfNull(testMethod);

        var parameters = testMethod.GetParameters();

        // Get the absolute path to the JSON file
        var path = Path.IsPathRooted(filePath)
            ? filePath
            : Path.GetRelativePath(Directory.GetCurrentDirectory(), filePath);

        if (!File.Exists(path)) throw new ArgumentException($"Could not find file at path: {path}");

        // Load the file
        var fileData = File.ReadAllText(filePath);

        return mapByProps ? GetMappingByProps(fileData, parameters) : GetAsArray(fileData, parameters);
    }

    private IEnumerable<object?[]> GetAsArray(string fileData, ParameterInfo[] parameters)
    {
        var deserialized = JsonSerializer.Deserialize<List<JsonElement[]>>(fileData);

        return deserialized?.Select(
            c => c.Select((el, idx) => idx >= parameters.Length ? el : el.Deserialize(parameters[idx].ParameterType))
                .ToArray()) ?? [];
    }

    private IEnumerable<object?[]> GetMappingByProps(string fileData, ParameterInfo[] parameters)
    {
        var deserialized = JsonSerializer.Deserialize<JsonDocument>(fileData);

        if (deserialized is null) return [];

        return
        [
            parameters.Select(
                p => p.Name is not null && deserialized.RootElement.TryGetProperty(p.Name, out var val)
                    ? val.Deserialize(p.ParameterType)
                    : null).ToArray()
        ];
    }
}
