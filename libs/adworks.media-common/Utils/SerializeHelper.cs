using System;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace adworks.media_common
{
    public class SerializeHelper
    {
        public static string Stringify(object source, JsonSerializerSettings settings)
        {
            try
            {
                if (source == null)
                {
                    return "";
                }
                return JsonConvert.SerializeObject(source, settings);
            }
            catch (Exception ex)
            {
                throw new SerializationException(string.Format("Unable to serialize :: {0}", ex.Message));
            }

        }
        
        public static string Stringify(object source)
        {
            try
            {
                if (source == null)
                {
                    return "";
                }
                return JsonConvert.SerializeObject(source);
            }
            catch (Exception ex)
            {
                throw new SerializationException(string.Format("Unable to serialize :: {0}", ex.Message));
            }

        }
        
        public static byte[] Serialize(object source)
        {
            try
            {
                string value = JsonConvert.SerializeObject(source);
                return Encoding.UTF8.GetBytes(value);
            }
            catch (Exception ex)
            {
                throw new SerializationException(string.Format("Unable to serialize :: {0}", ex.Message));
            }
        }
        
        public static T Deserialize<T>(byte[] source)
        {
            try
            {
                var value = Encoding.UTF8.GetString(source);
                return Deserialize<T>(value);
            }
            catch (Exception ex)
            {
                throw new SerializationException(string.Format("Unable to deserialize :: {0}", ex.Message));
            }
        }
        
        public static T Deserialize<T>(string value)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(value);
            }
            catch (Exception ex)
            {
                throw new SerializationException(string.Format("Unable to deserialize :: {0}", ex.Message));
            }
        }
    }
}