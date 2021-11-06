locals {
  ecr_repositories = ["identity-server"]
}

terraform {
  required_version = "~> 0.13.6"
  backend "s3" {
    encrypt        = true
    bucket         = "mfaester-booking-state"
    dynamodb_table = "mfaester-booking-state-locks"
    key            = "booking/01.vpc/terraform.tfstate"
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

resource "aws_subnet" "back-a" {
  vpc_id            = aws_vpc.front.id
  cidr_block        = "10.0.0.0/24"
  availability_zone = "eu-west-1a"

  tags = {
    Name = "Back"
  }
}

resource "aws_subnet" "back-b" {
  vpc_id            = aws_vpc.front.id
  cidr_block        = "10.0.1.0/24"
  availability_zone = "eu-west-1b"

  tags = {
    Name = "Back"
  }
}

resource "aws_vpc" "front" {
  cidr_block = "10.0.0.0/23"
}

module ecr {
  source       = "../../modules/ecr"
  repositories = local.ecr_repositories
}
