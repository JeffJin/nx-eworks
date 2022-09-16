using System;

namespace adworks.media_common
{
    public static class FileTypeHelper
    {
        private static string UnquoteFileName(string fileName)
        {
            if (fileName.StartsWith("\"", StringComparison.Ordinal) &&
                fileName.EndsWith("\"", StringComparison.Ordinal) && fileName.Length > 1)
            {
                return fileName.Substring(1, fileName.Length - 2);
            }

            return fileName.ToLower();
        }

        public static bool IsVideo(string fileName)
        {
            fileName = UnquoteFileName(fileName);  
            
            if (fileName.EndsWith("mp4") ||
                fileName.EndsWith("mov") ||
                fileName.EndsWith("avi") ||
                fileName.EndsWith("m4v") ||
                fileName.EndsWith("wmv") ||
                fileName.EndsWith("mpg"))
            {
                return true;
            }

            return false;
        }
        public static bool IsAudio(string fileName)
        {
            fileName = UnquoteFileName(fileName);  

            if (fileName.EndsWith("mp3") ||
                fileName.EndsWith("wav") ||
                fileName.EndsWith("wma"))
            {
                return true;
            }

            return false;   
        }
        public static bool IsImage(string fileName)
        {
            fileName = UnquoteFileName(fileName);  

            if (fileName.EndsWith("jpg") ||
                fileName.EndsWith("gif") ||
                fileName.EndsWith("bmp") ||
                fileName.EndsWith("tif") ||
                fileName.EndsWith("png"))
            {
                return true;
            }

            return false;
        }

        public static bool IsMediaFile(string fileName)
        {
            return IsImage(fileName) || IsAudio(fileName) || IsVideo(fileName);
        }

        public static string GetMediaType(string fileName)
        {
            if (IsImage(fileName))
            {
                return "Image";
            }

            if (IsAudio(fileName))
            {
                return "Audio";
            }

            if (IsVideo(fileName))
            {
                return "Video";
            }

            return "";
        }
    }
}