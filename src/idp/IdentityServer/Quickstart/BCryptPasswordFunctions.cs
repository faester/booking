using System;
using BCrypt.Net;

namespace IdentityServer.Quickstart
{
    public class BCryptPasswordFunctions : IPasswordFunctions
    {
        private int defaultWorkFactor = 12;
        private const string IdentifyingPrefix = "bcrypt#:";

        public string CreateHash(string clearTextPassword)
        {
            return BCrypt.Net.BCrypt.HashPassword(clearTextPassword, defaultWorkFactor);
        }

        public bool CompareHashedPassword(string clearTextPassword, string hashedPassword)
        {
            if (clearTextPassword == null || hashedPassword == null)
            {
                return false;
            }
            return BCrypt.Net.BCrypt.Verify(clearTextPassword, hashedPassword);
        }

        public bool IsHashedWithThisFunction(string hashedPassword)
        {
            try
            {
                BCrypt.Net.BCrypt.InterrogateHash(hashedPassword);
                return true;
            }
            catch (HashInformationException)
            {
                return false;
            }
        }
    }
}