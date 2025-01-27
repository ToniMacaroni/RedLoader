using System;
using Alt.Json;
using UnityEngine;

namespace RedLoader.JsonConverters
{
    internal class QuaternionConverter : JsonConverter<Quaternion>
    {
        public override void WriteJson(JsonWriter writer, Quaternion value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, new[] { value.x, value.y, value.z, value.w });
        }

        public override Quaternion ReadJson(JsonReader reader, Type objectType, Quaternion existingValue, bool hasExistingValue,
                                            JsonSerializer serializer)
        {
            var fArr = serializer.Deserialize<float[]>(reader);
            return new Quaternion(fArr[0], fArr[1], fArr[2], fArr[3]);
        }
    }
}
