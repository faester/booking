using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Amazon.KeyManagementService.Model;
using Amazon.SecretsManager;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Microsoft.AspNetCore.DataProtection.Repositories;

namespace IdentityServer.DataProtection
{
    public class SsmDataprotection : IXmlRepository
    {
        private readonly string _ssmPrefix;
        private readonly IAmazonSimpleSystemsManagement _ssmClient;

        public SsmDataprotection(string ssmPrefix, IAmazonSimpleSystemsManagement ssmClient)
        {
            _ssmPrefix = ssmPrefix;
            if (!_ssmPrefix.EndsWith("/"))
            {
                _ssmPrefix += "/";
            }

            _ssmClient = ssmClient;
        }

        public IReadOnlyCollection<XElement> GetAllElements()
        {
            var elements = new List<XElement>();
            var getRequest = new GetParametersByPathRequest
            {
                Path = _ssmPrefix,
                Recursive = true,
                WithDecryption = true
            };

            do
            {
                var ssmResponse = _ssmClient.GetParametersByPathAsync(getRequest);
                ssmResponse.Wait();
                getRequest.NextToken = ssmResponse.Result.NextToken;
                ssmResponse.Result.Parameters.ForEach(
                    z => elements.Add(XElement.Parse(z.Value)));
            } while (!string.IsNullOrEmpty(getRequest.NextToken));

            return elements;
        }

        public void StoreElement(XElement element, string friendlyName)
        {
            var putRequest = new PutParameterRequest
            {
                Value = element.ToString(),
                Name = _ssmPrefix + friendlyName
            };
            var put = _ssmClient.PutParameterAsync(putRequest);
            put.Wait();
        }
    }
}
