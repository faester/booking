using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Newtonsoft.Json;

public static class SecretsRetriever {

    /// <summary>
    ///  Assume secret is a json blob containing a dictionary with keys and base64 strings for 
    /// D, DP, DQ, Exponent, InverseQ, Modulus, P and Q
    /// </summary>
    public static RSAParameters GetRSAParameters(string secretName)
    {
        var secretString = GetSecret(secretName);
        var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(secretString);
        var result = new RSAParameters();

        result.D = Convert.FromBase64String(dict["D"]);
        result.DP = Convert.FromBase64String(dict["DP"]);
        result.DQ = Convert.FromBase64String(dict["DQ"]);
        result.Exponent = Convert.FromBase64String(dict["Exponent"]);
        result.InverseQ = Convert.FromBase64String(dict["InverseQ"]);
        result.Modulus = Convert.FromBase64String(dict["Modulus"]);
        result.P = Convert.FromBase64String(dict["P"]);
        result.Q = Convert.FromBase64String(dict["Q"]);

        return result;
    }

    public static AWSCredentials GetCredentials() {
        var chain = new CredentialProfileStoreChain();
        return chain.TryGetAWSCredentials("mfaester", out var credentials) 
            ? credentials
            : new ECSTaskCredentials();
    }

    public static string GetSecret(string secretName)
    {
        string secret = "";

        IAmazonSecretsManager client = new AmazonSecretsManagerClient(GetCredentials(), Region);

        GetSecretValueRequest request = new GetSecretValueRequest();
        request.SecretId = secretName;
        request.VersionStage = "AWSCURRENT"; // VersionStage defaults to AWSCURRENT if unspecified.

        Task<GetSecretValueResponse> responseTask = client.GetSecretValueAsync(request);
        responseTask.Wait();
        var response = responseTask.Result;

        // Decrypts secret using the associated KMS CMK.
        // Depending on whether the secret is a string or binary, one of these fields will be populated.
        if (response.SecretString != null)
        {
            secret = response.SecretString;
        }
        else
        {
            var memoryStream = response.SecretBinary;
            StreamReader reader = new StreamReader(memoryStream);
            string decodedBinarySecret = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(reader.ReadToEnd()));
        }

        // Your code goes here.
        return secret;
    }

    public static RegionEndpoint Region => RegionEndpoint.EUWest1;
}