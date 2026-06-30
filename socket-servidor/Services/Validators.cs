
namespace Validators
{
    public static class Validators
    {
        public static bool IsValidUserName(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
                return false;

            if (userName.StartsWith(".") || userName.Contains("/") || userName.Contains("\\"))
                return false;

            foreach (char c in userName)
            {
                if (!char.IsLetterOrDigit(c))
                    return false;
            }

            return true;
        }

        public static bool IsValidFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return false;

            if (fileName.Contains("/") || fileName.Contains("\\"))
                return false;

            return true;
        }
    }
}