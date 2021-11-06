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

resource "aws_subnet" "booking-a" {
  vpc_id            = aws_vpc.booking.id
  cidr_block        = "10.0.0.0/24"
  availability_zone = "eu-west-1a"

  tags = {
    Name = "booking-a"
  }
}

resource "aws_subnet" "booking-b" {
  vpc_id            = aws_vpc.booking.id
  cidr_block        = "10.0.1.0/24"
  availability_zone = "eu-west-1b"

  tags = {
    Name = "booking-b"
  }
}

resource "aws_vpc" "booking" {
  cidr_block = "10.0.0.0/23"
}

module ecr {
  source       = "../../modules/ecr"
  repositories = local.ecr_repositories
}

resource "aws_internet_gateway" "gw" {
  vpc_id = aws_vpc.booking.id

  tags = {
    Name = "main"
  }
}

