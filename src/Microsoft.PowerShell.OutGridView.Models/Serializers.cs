// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Text;
using Newtonsoft.Json;

// TODO: switch to JSON.NET
// BUGBUG: This appears to be unused code. Consider removing it.

namespace Microsoft.PowerShell.OutGridView.Models
{
    public class Serializers
    {
        private static readonly JsonSerializerSettings JSON_SERIALIZER_SETTINGS = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.All
        };
        public static string ObjectToJson<T>(T obj)
        {
            var jsonString = JsonConvert.SerializeObject(obj, JSON_SERIALIZER_SETTINGS);

            return ToBase64String(jsonString);
        }

        public static T? ObjectFromJson<T>(string base64Json)
        {
            var jsonString = FromBase64String(base64Json);

            return JsonConvert.DeserializeObject<T>(jsonString, JSON_SERIALIZER_SETTINGS);
        }


        private static string FromBase64String(string base64string)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(base64string));
        }

        private static string ToBase64String(string str)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(str));
        }
    }
}
