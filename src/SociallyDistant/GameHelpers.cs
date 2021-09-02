using System;
using System.Text;

namespace SociallyDistant
{
    public static class GameHelpers
    {
        public static string ToUnixUsername(this string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            var name = text.Trim();

            var unix = new StringBuilder();
            var wasWhitespace = false;
            
            foreach (var ch in name)
            {
                if (char.IsLetterOrDigit(ch))
                {
                    wasWhitespace = false;
                    unix.Append(ch);
                }
                else
                {
                    if (ch == '_' || ch == '-')
                    {
                        if (!wasWhitespace)
                        {
                            wasWhitespace = true;
                            unix.Append(ch);
                        }
                        
                        continue;
                    }
                    else
                    {
                        if (!wasWhitespace)
                        {
                            unix.Append('-');
                            wasWhitespace = true;
                        }

                        continue;
                    }
                }
            }

            return unix.ToString();
        }

        public static Type FindType(string fullName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(fullName);

                if (type != null)
                    return type;
            }

            return null;
        }
    }
}