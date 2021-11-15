locals {
  ecr_repositories = ["identity-server"]
  vpc_id           = "vpc-0aeea6469b8e6c87c"
}

terraform {
  required_version = "~> 0.13.6"
  backend "s3" {
    encrypt        = true
    bucket         = "mfaester-booking-state"
    dynamodb_table = "mfaester-booking-state-locks"
    key            = "booking/03.services/terraform.tfstate"
    region         = "eu-west-1"
    profile        = "mfaester"
  }
}

# Configure the AWS Provider
provider "aws" {
  version = "~> 3.0"
  region  = "eu-west-1"
  profile = "mfaester"
  default_tags {
    tags = {
      Source = path.cwd
    }
  }
}

data "aws_caller_identity" "current" {}


module identity_server {
  source     = "../../services/identity-server"
  vpc_id     = local.vpc_id
  account_id = data.aws_caller_identity.current.account_id
}

module convention_api {
  source     = "../../services/convention-api"
  vpc_id     = local.vpc_id
  account_id = data.aws_caller_identity.current.account_id
}

module convention_website {
  source     = "../../services/convention-website"
  vpc_id     = local.vpc_id
  account_id = data.aws_caller_identity.current.account_id
}
