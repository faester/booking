terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 3.0"
    }
  }
}

# Configure the AWS Provider
provider "aws" {
  version = "~> 3.0"
  region  = "eu-west-1"
  profile = "mfaester"
}

resource aws_s3_bucket state_bucket {
  bucket = "mfaester-booking-state"
  acl    = "private"
}

resource aws_dynamodb_table state_locks {
  name           = "mfaester-booking-state-locks"
  billing_mode   = "PROVISIONED"
  read_capacity  = 20
  write_capacity = 20
  hash_key       = "LockID"

  attribute {
    name = "LockID"
    type = "S"
  }
}
