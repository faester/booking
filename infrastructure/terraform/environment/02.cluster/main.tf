locals {
  ecr_repositories = ["identity-server"]
}

terraform {
  required_version = "~> 0.13.6"
  backend "s3" {
    encrypt        = true
    bucket         = "mfaester-booking-state"
    dynamodb_table = "mfaester-booking-state-locks"
    key            = "booking/02.cluster/terraform.tfstate"
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

data aws_subnet subnet_a {
  filter {
    name   = "tag:Name"
    values = ["booking-a"]
  }
}

data aws_subnet subnet_b {
  filter {
    name   = "tag:Name"
    values = ["booking-b"]
  }
}

data aws_ssm_parameter ecs_ami {
	name = "/aws/service/ecs/optimized-ami/amazon-linux-2/recommended/image_id"
}

resource aws_launch_template booking {
  name_prefix = "booking"
  image_id    = data.aws_ssm_parameter.ecs_ami.value
}

resource aws_autoscaling_group booking {
  capacity_rebalance  = true
  desired_capacity    = 0
  max_size            = 6
  min_size            = 0
  vpc_zone_identifier = [data.aws_subnet.subnet_a.id, data.aws_subnet.subnet_b.id]

  mixed_instances_policy {
    instances_distribution {
      on_demand_base_capacity                  = 0
      on_demand_percentage_above_base_capacity = 0
      spot_allocation_strategy                 = "capacity-optimized"
    }

    launch_template {
      launch_template_specification {
        launch_template_id = aws_launch_template.booking.id
      }

      override {
        instance_type     = "t3.nano"
        weighted_capacity = "1"
      }

      override {
        instance_type     = "t3a.nano"
        weighted_capacity = "1"
      }
    }
  }

  lifecycle {
	ignore_changes = [desired_capacity]
  }
}
