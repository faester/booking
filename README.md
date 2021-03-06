# Identity Server 4 toy project

## Infrastructure
The 'infrastructrue' directory contains setup of an AWS account. 

It assumes one environment per account (no attempts of being clever with paths in ssm etc). 

Infrastructure is scriptet in terraform. Account number and regions is hard coded. 

It will create one single ECS cluster, an LB and listeners for each service. We have a closed 
vpc with vpc endpoint and rather laxed SGs (no nacl). 

Certificate validation and DNS setup is in the domain mfaester.dk as I did not own a domain 
delegated to AWS. DNS config is manual.  

## Preparations 
IdentityServer need some key material for signing its tokens. This is stored in SecretsManager under `secrets/main_rsa_key` 
- the value must be valid RSAParameters in a json object containing the values as base64 encoded strings.

```yaml
{
   "D": "vD8...",
   "DP": "AXnmC...",
   "DQ": "jtsB7D...",
   "Exponent": "AQAB",
   "InverseQ": "Rp...",
   "Modulus": "zTRTVSd...",
   "P": "+LKuz...",
   "Q": "0zq6F..."
}
```

# Project 
# idp - Identity provider
Standard example project from ID4 with UI example added. Modifications on handling keys (above). We use hard coded configuration to avoid setting up a database. (EF is relatively easy, but still need some preparation and not least some sql server in RDS which is expensive for a toy project. I have used simpledb for user storage and in the other API's, but this non-relational database system is not easily setup for ID4s configuration database.)




# User handling 

# Build 

An old fashioned Makefile is used for creating simple shortcuts for building docker images etc. 

Use 

`make publish` to build and publish new docker images (tagget 'latest'). 
`make terraform-shell` to get an appropriate version of TF inside a docker image. 


Credentials are assumed to be present in `~/.aws/credentials` under the profile `mfaester`.

# Manual tests 
Obtain a token using client credentials: 
```
 curl -H "Content-Type: application/x-www-form-urlencoded" -d "grant_type=client_credentials&client_id=convention-reader&scope=convention-admin" -u "convention-reader:secret01"  https://identity-server.mfaester.dk/connect/token 
```
This emulates server-to-server invokations.
