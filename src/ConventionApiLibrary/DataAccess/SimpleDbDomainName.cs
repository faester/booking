namespace ConventionApiLibrary.DataAccess
{
    public class SimpleDbDomainName : ISimpleDbDomainName
    {
        public SimpleDbDomainName(string domainName)
        {
            DomainName = domainName;
        }

        public string DomainName { get; }
    }
}