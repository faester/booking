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

data aws_vpc main {

}

module identity {
  source       = "../../modules/ecs-service"
  docker_image = "identity-server"
  vpc_id       = local.vpc_id
  cluster_id   = "booking-main"

  root_domain = "mfaester.dk"
  port        = 8000
  subdomain   = "identity-server"
 listener_arn = "arn:aws:elasticloadbalancing:eu-west-1:539839626842:listener/app/booking-public-lb/aaeb45b1f270cbf7/46764fad2dd56422"
}

