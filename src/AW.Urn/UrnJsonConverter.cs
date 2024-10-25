using System;
using System.Text.Json.Serialization;

namespace AW.Identifiers;

public class UrnJsonConverter : JsonConverter<Urn>
{
    public override Urn Read(ref System.Text.Json.Utf8JsonReader reader, Type typeToConvert, System.Text.Json.JsonSerializerOptions options)
        {
            var str = reader.GetString();

            return string.IsNullOrWhiteSpace(str)
                ? Urn.Empty
                : Urn.Parse(str);
        }

    public override void Write(System.Text.Json.Utf8JsonWriter writer, Urn value, System.Text.Json.JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString());
}
