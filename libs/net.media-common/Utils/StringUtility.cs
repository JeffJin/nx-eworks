using System.IO;
using System.Text.RegularExpressions;

namespace adworks.media_common
{
    public class StringHelper
    {
        public static string RemoveInvalidPathChars(string source)
        {
            string invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            invalid = invalid + "\"" + "'" + "!#$%^&*()~:;,+";
            foreach (char c in invalid)
            {
                source = source.Replace(c.ToString(), "");
            }
            source = source.Replace("\r", "").Replace("\n", "").Replace(" ", "");
            
            return source;
        }
        
        public static string CleanupFileName(string fileName)
        {
            fileName = RemoveInvalidPathChars(fileName);
            
            Regex rgx = new Regex("[^.a-zA-Z0-9 -]");
            return rgx.Replace(fileName, "");
        }
        
        public static string GetTitleFromFileName(string fileName)
        {
            fileName = CleanupFileName(fileName);
            return Path.GetFileNameWithoutExtension(fileName);
        }
    }
}
