using System.Text.Json.Serialization;

namespace LibSql.Http.Client.Tests.Shared.Models;

public record HranaNamedArg(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("value")] HranaArgValue Value);

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(HranaFloatValue), "float")]
[JsonDerivedType(typeof(HranaTextValue), "text")]
[JsonDerivedType(typeof(HranaIntegerValue), "integer")]
public record HranaArgValue;

public record HranaFloatValue([property: JsonPropertyName("value")] float Value) : HranaArgValue;

public record HranaTextValue([property: JsonPropertyName("value")] string Value) : HranaArgValue;

public record HranaIntegerValue([property: JsonPropertyName("value")] string Value) : HranaArgValue;

public record HranaStatement(
    [property: JsonPropertyName("sql")] string Sql,
    [property: JsonPropertyName("args")] HranaArgValue?[]? Args = null,
    [property: JsonPropertyName("named_args")]
    HranaNamedArg[]? NamedArgs = null);

public record HranaBatchStepCondition(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("step")] int? Step = null,
    [property: JsonPropertyName("cond")] HranaBatchStepCondition? Condition = null);

public record HranaBatchStep(
    [property: JsonPropertyName("stmt")] HranaStatement Statement,
    [property: JsonPropertyName("condition")]
    HranaBatchStepCondition? Condition = null);

public record HranaBatchRequest([property: JsonPropertyName("steps")] HranaBatchStep[] Steps);

public record HranaPipelineRequest(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("batch")] HranaBatchRequest? Batch = null,
    [property: JsonPropertyName("stmt")] HranaStatement? Statement = null);

public record HranaPipelineRequestBody(
    [property: JsonPropertyName("requests")]
    HranaPipelineRequest[] Requests,
    [property: JsonPropertyName("baton")] string? Baton = null);