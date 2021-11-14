namespace ConventionApiLibrary.DataAccess
{
    public class SimpleDbDomainName<T> : ISimpleDbDomainName<T>
    {
        public SimpleDbDomainName(string domainName)
        {
            DomainName = domainName;
        }

        public string DomainName { get; }
    }
}