locals {
  ecr_repositories   = ["identity-server"]
  cluster_identifier = "booking-main"
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

resource "aws_security_group" "ecs_cluster_member_sg" {
  name        = "${local.cluster_identifier}-allow-outgoing"
  description = "SG for cluster ec2 instances"
  vpc_id      = data.aws_subnet.subnet_a.vpc_id

  ingress {
    description     = "Allow traffic on internally exposed ports"
    from_port       = 8000
    to_port         = 9999
    protocol        = "tcp"
    security_groups = [] #[module.lb.lb_sg_arn]
    cidr_blocks     = ["0.0.0.0/0"]
  }

  egress {
    description = "Allow all outgoing traffic. - We should perhaps limit this later on."
    from_port   = 0
    to_port     = 65535
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }
}

resource aws_launch_template booking {
  name_prefix = "booking"
  image_id    = data.aws_ssm_parameter.ecs_ami.value

  vpc_security_group_ids = [aws_security_group.ecs_cluster_member_sg.id]

  iam_instance_profile {
    arn = aws_iam_instance_profile.ecs_ec2_instance_profile.arn
  }

  update_default_version = true

  user_data = filebase64("userdata.sh")
}

resource "aws_iam_instance_profile" "ecs_ec2_instance_profile" {
  name = "ecs_ec2_instance_profile-${local.cluster_identifier}"
  role = aws_iam_role.ecs_ec2_instance_profile.name
}

resource "aws_iam_role_policy_attachment" "session_manager_role_attach" {
  policy_arn = "arn:aws:iam::aws:policy/AmazonSSMManagedInstanceCore"
  role       = aws_iam_role.ecs_ec2_instance_profile.name
}

resource "aws_iam_role_policy_attachment" "cloudwatchagent_role_attach" {
  policy_arn = "arn:aws:iam::aws:policy/CloudWatchAgentServerPolicy"
  role       = aws_iam_role.ecs_ec2_instance_profile.name
}


resource "aws_iam_role_policy" "ecs_ec2_instance_policy" {
  name = "ecs_ec2_instance_policy"
  role = aws_iam_role.ecs_ec2_instance_profile.id

  policy = data.aws_iam_policy_document.ecs_ec2_instance_policy.json
}

data aws_iam_policy_document ecs_ec2_instance_policy {
  statement {

    actions = [
      "ec2:AttachVolume",
      "ec2:DetachVolume",
      "ec2:DescribeTags",
      "ecs:CreateCluster",
      "ecs:DeregisterContainerInstance",
      "ecs:DiscoverPollEndpoint",
      "ecs:Poll",
      "ecs:RegisterContainerInstance",
      "ecs:StartTelemetrySession",
      "ecs:UpdateContainerInstancesState",
      "ecs:Submit*",
      "ecr:GetAuthorizationToken",
      "ecr:BatchCheckLayerAvailability",
      "ecr:GetDownloadUrlForLayer",
      "ecr:BatchGetImage",
      "logs:CreateLogStream",
      "logs:PutLogEvents"
    ]
    resources = ["*"]
  }

  statement {
    actions = [
      "sts:assumerole"
    ]
    resources = ["*"]
  }
}

resource "aws_iam_role" "ecs_ec2_instance_profile" {
  name = "ecs_ec2_instance_profile-${local.cluster_identifier}"
  path = "/"

  assume_role_policy = <<EOF
{
    "Version": "2012-10-17",
    "Statement": [
        {   
            "Action": "sts:AssumeRole",
            "Principal": {
               "Service": ["ec2.amazonaws.com"]
            },  
            "Effect": "Allow",
            "Sid": ""
        }   
    ]   
}
EOF
}

resource aws_autoscaling_group booking {
  capacity_rebalance   = true
  desired_capacity     = 0
  max_size             = 3
  min_size             = 0
  vpc_zone_identifier  = [data.aws_subnet.subnet_a.id, data.aws_subnet.subnet_b.id]
  termination_policies = ["OldestInstance"]

  health_check_type         = "EC2"
  health_check_grace_period = 300

  enabled_metrics = ["GroupDesiredCapacity", "GroupInServiceCapacity", "GroupPendingCapacity", "GroupMinSize", "GroupMaxSize", "GroupInServiceInstances", "GroupPendingInstances", "GroupStandbyInstances", "GroupStandbyCapacity", "GroupTerminatingCapacity", "GroupTerminatingInstances", "GroupTotalCapacity", "GroupTotalInstances"]


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

resource aws_ecs_capacity_provider booking {
  name = "booking-main-capacity-provider"

  auto_scaling_group_provider {
    auto_scaling_group_arn         = aws_autoscaling_group.booking.arn
    managed_termination_protection = "DISABLED"

    managed_scaling {
      maximum_scaling_step_size = 1000
      minimum_scaling_step_size = 1
      status                    = "ENABLED"
      target_capacity           = 100
    }
  }
}

resource aws_iam_service_linked_role ecs {
  aws_service_name = "ecs.amazonaws.com"
}

resource aws_ecs_cluster booking {
  name               = "booking-main"
  capacity_providers = [aws_ecs_capacity_provider.booking.name]
  default_capacity_provider_strategy {
    capacity_provider = aws_ecs_capacity_provider.booking.name
    weight            = 1
  }

  setting {
    name  = "containerInsights"
    value = "enabled"
  }
}


######################################################################
# IAM ROLES FOR ECS
######################################################################
resource "aws_iam_role" "ecs_role" {
  name = "${local.cluster_identifier}-ecs-role"

  assume_role_policy = jsonencode({
    "Version" : "2012-10-17",
    "Statement" : [
      {
        "Action" : "sts:AssumeRole",
        "Principal" : {
          "Service" : "ecs.amazonaws.com"
        },
        "Effect" : "Allow",
        "Sid" : "",
      }
    ]

  })

  tags = {
    Cluster = local.cluster_identifier
  }
}

resource "aws_iam_role_policy_attachment" "ecs_role" {
  role       = aws_iam_role.ecs_role.name
  policy_arn = aws_iam_policy.ecs_role_policy.arn
}

resource "aws_iam_policy" "ecs_role_policy" {
  name        = "${local.cluster_identifier}-ecs-policy"
  path        = "/"
  description = "Policy for ECS in cluster ${local.cluster_identifier}"

  policy = data.aws_iam_policy_document.ecs_role_policy.json
}

data "aws_iam_policy_document" "ecs_role_policy" {
  statement {
    actions = [
      "ec2:*",
      "s3:*",
      "ecs:*",
      "cloudwatch:*",
      "sts:assumerole",
      "logs:*"
    ]
    resources = ["*"]
  }
}



module lb {
  source      = "../../modules/loadbalancer"
  subnet_ids  = [data.aws_subnet.subnet_a.id, data.aws_subnet.subnet_b.id]
  name        = "booking-public-lb"
  root_domain = "mfaester.dk"
  vpc_id      = data.aws_subnet.subnet_a.vpc_id
}
