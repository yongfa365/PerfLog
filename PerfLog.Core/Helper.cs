using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PerfLog.Core.Extend
{
    public static class Helper
    {
        #region JSON序列化，反序列化 {1AC5559B-3615-4EA6-B15B-B6374B28D7A1}
        /// <summary>
        /// 使用json序列化为字符串
        /// </summary>
        /// <param name="input"></param>
        /// <param name="isNeedFormat">默认false</param>
        /// <param name="isCanCyclicReference">默认true,生成的json每个对象会自动加上类似 "$id":"1","$ref": "1"</param>
        /// <param name="dateTimeFormat">默认null,即使用json.net默认的序列化机制，如："\/Date(1439335800000+0800)\/"</param>
        /// <param name="ignoreNullValue">值为null的不进行序列化（比如某个属性的值为null，序列化后没有此属性存在)</param>
        /// <returns></returns>
        public static string ToJson(this object input, bool isNeedFormat = false, bool isCanCyclicReference = true,
                                    string dateTimeFormat = null, bool ignoreNullValue = true)
        {
            var settings = new JsonSerializerSettings();

            if (ignoreNullValue)
            {
                settings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
            }


            if (isCanCyclicReference)
            {
                settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                settings.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
            }

            if (!string.IsNullOrWhiteSpace(dateTimeFormat))
            {
                var jsonConverter = new List<JsonConverter>()
                {
                    new Newtonsoft.Json.Converters.IsoDateTimeConverter(){ DateTimeFormat = dateTimeFormat }//如： "yyyy-MM-dd HH:mm:ss"
                };
                settings.Converters = jsonConverter;
            }

            var format = isNeedFormat ? Formatting.Indented : Formatting.None;

            var json = JsonConvert.SerializeObject(input, format, settings);
            return json;
        }

        /// <summary>
        /// 从序列化字符串里反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <param name="dateTimeFormat">默认null,即使用json.net默认的序列化机制</param>
        /// <returns></returns>
        public static T FromJson<T>(this string input, string dateTimeFormat = null)
        {
            var settings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            };


            if (!string.IsNullOrWhiteSpace(dateTimeFormat))
            {
                var jsonConverter = new List<JsonConverter>()
                {
                    new Newtonsoft.Json.Converters.IsoDateTimeConverter(){ DateTimeFormat = dateTimeFormat }//如： "yyyy-MM-dd HH:mm:ss"
                };
                settings.Converters = jsonConverter;
            }

            return JsonConvert.DeserializeObject<T>(input, settings);
        }

        #endregion
    }
}
