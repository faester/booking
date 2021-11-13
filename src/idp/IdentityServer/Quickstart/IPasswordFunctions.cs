namespace IdentityServer.Quickstart
{
    /// <summary>
    /// Describes a way of storing a password
    /// with a one-way hash function.
    /// <para>
    /// Everything is simple strings to avoid
    /// changing other classes. 
    /// </para>
    /// </summary>
    public interface IPasswordFunctions
    {
        /// <summary>
        /// Create a hash representation of a password.
        /// <para>
        /// Compare using CompareHashedPassword
        /// </para>
        /// </summary>
        /// <param name="clearTextPassword"></param>
        /// <returns></returns>
        string CreateHash(string clearTextPassword);

        /// <summary>
        /// Compare a cleartext password with a hashed representation.
        /// </summary>
        /// <param name="clearTextPassword"></param>
        /// <param name="hashedPassword"></param>
        /// <returns></returns>
        bool CompareHashedPassword(string clearTextPassword, string hashedPassword);

        /// <summary>
        /// Must return true if string is a hash created with this function and
        /// can be compared with a cleartext password using CompareHashedPassword.
        /// </summary>
        /// <param name="hashedPassword"></param>
        /// <returns></returns>
        bool IsHashedWithThisFunction(string hashedPassword);
    }
}