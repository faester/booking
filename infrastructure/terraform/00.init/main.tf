terraform {
  required_version = "~> 0.13.6"
}

# Configure the AWS Provider
provider "aws" {
  version = "~> 3.0"
  region  = "eu-west-1"
  profile = "mfaester"
}

terraform {
  backend "s3" {
    encrypt = true    
    bucket = "mfaester-booking"
    dynamodb_table = "terraform-state-lock-dynamo"
    key    = "terraform.tfstate"
    region = "us-east-1"
  }
}

