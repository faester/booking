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

resource aws_subnet booking-a {
  vpc_id            = aws_vpc.booking.id
  cidr_block        = "10.0.0.0/24"
  availability_zone = "eu-west-1a"

  tags = {
    Name = "booking-a"
  }
}

resource aws_subnet booking-b {
  vpc_id            = aws_vpc.booking.id
  cidr_block        = "10.0.1.0/24"
  availability_zone = "eu-west-1b"

  tags = {
    Name = "booking-b"
  }
}

resource aws_vpc booking {
  cidr_block = "10.0.0.0/23"

  enable_dns_support   = true
  enable_dns_hostnames = true
  tags = {
    Name = "booking"
  }
}

module ecr {
  source       = "../../modules/ecr"
  repositories = local.ecr_repositories
}

resource aws_internet_gateway gw {
  vpc_id = aws_vpc.booking.id

  tags = {
    Name = "main"
  }
}

resource aws_route_table booking_public {
  vpc_id = aws_vpc.booking.id


  route {
    cidr_block = "0.0.0.0/0"
    gateway_id = aws_internet_gateway.gw.id
  }

  tags = {
    Name = "igw_to_subnets_booking"
  }
}

resource aws_route_table_association public_subnet_a {
  subnet_id      = aws_subnet.booking-a.id
  route_table_id = aws_route_table.booking_public.id
}

resource aws_route_table_association public_subnet_b {
  subnet_id      = aws_subnet.booking-b.id
  route_table_id = aws_route_table.booking_public.id
}
