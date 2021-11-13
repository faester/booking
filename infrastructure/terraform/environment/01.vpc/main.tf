locals {
  ecr_repositories = ["identity-server"]
  vpc_endpoints = ["ecs-agent",
    "ecs-telemetry",
    "ecs",
    "ecr.api",
    "ecr.dkr",
    "ssm",
    "ec2messages",
    "ssmmessages",
    "logs",
    "sts",
    "ec2",
    "sns",
    "sqs",
  "s3"]

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
  vpc_id                  = aws_vpc.booking.id
  cidr_block              = "10.0.0.0/24"
  availability_zone       = "eu-west-1a"
  map_public_ip_on_launch = true


  tags = {
    Name = "booking-a"
  }
}

resource aws_subnet booking-b {
  vpc_id                  = aws_vpc.booking.id
  cidr_block              = "10.0.1.0/24"
  availability_zone       = "eu-west-1b"
  map_public_ip_on_launch = true

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



resource "aws_security_group" "vpc_endpoint_sg" {
  name        = "booking main vpc endpoint - allow all"
  description = "Used for VPC endpoint - allowing incoming and outgoing traffic"
  vpc_id      = aws_vpc.booking.id

  ingress {
    description = "Allow incoming traffic"
    from_port   = 443
    to_port     = 443
    protocol    = "TCP"
    cidr_blocks = ["0.0.0.0/0"]
  }

  egress {
    description = "Allow all outgoing traffic"
    from_port   = 0
    to_port     = 65535
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

}

resource "aws_vpc_endpoint" "vpc_endpoints" {
  for_each     = toset(local.vpc_endpoints)
  vpc_id       = aws_vpc.booking.id
  service_name = "com.amazonaws.eu-west-1.${each.value}"

  vpc_endpoint_type   = each.value == "s3" ? "Gateway" : "Interface"
  private_dns_enabled = each.value != "s3"

  security_group_ids = each.value == "s3" ? [] : [aws_security_group.vpc_endpoint_sg.id]

  subnet_ids = each.value == "s3" ? [] : [aws_subnet.booking-a.id, aws_subnet.booking-b.id]
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

resource "aws_eip" "nateip" {
  vpc = true
  tags = {
    Name = "booking-main-eip-natgw"
  }
  depends_on = [aws_internet_gateway.gw, ]
}

resource "aws_nat_gateway" "natgw" {
  allocation_id = aws_eip.nateip.id
  subnet_id     = aws_subnet.booking-a.id
  tags = {
    Name = "booking-main-natgw"
  }
  depends_on = [aws_internet_gateway.gw, ]
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
