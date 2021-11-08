resource aws_ecs_task_definition service {
  family = "booking"
  container_definitions = jsonencode([
    {
      name      = var.subdomain
      image     = "539839626842.dkr.ecr.eu-west-1.amazonaws.com/${var.docker_image}:${var.docker_tag}"
      cpu       = var.cpu
      memory    = var.memory
      essential = true
      logConfiguration = {
        logDriver = "awslogs"
        "options" = {
             "awslogs-group"         = aws_cloudwatch_log_group.task_log.name,
             "awslogs-region"        = "eu-west-1",
             "awslogs-stream-prefix" = var.subdomain
           }
      }
      portMappings = [
        {
          containerPort = 80
          hostPort      = var.port
        }
      ]
    }
  ])
}

resource "aws_cloudwatch_log_group" "task_log" {
  name = "booking/services/${var.subdomain}"
}

resource aws_ecs_service service {
  name            = var.docker_image
  cluster         = var.cluster_id
  task_definition = aws_ecs_task_definition.service.arn
  desired_count   = 3

  ordered_placement_strategy {
    type  = "binpack"
    field = "cpu"
  }

  load_balancer {
    target_group_arn = aws_lb_target_group.tg.arn
    container_name   = var.subdomain
    container_port   = 80
  }

  capacity_provider_strategy {
    capacity_provider = "${var.cluster_id}-capacity-provider"
    weight = 1
  }

  lifecycle {
    ignore_changes = [desired_count]
  }
}

resource aws_iam_role_policy_attachment ecs_service {
  role       = aws_iam_role.service_role.name
  policy_arn = aws_iam_policy.ecs_role_policy.arn
  #policy_arn = "arn:aws:iam::aws:policy/aws-service-role/AmazonECSServiceRolePolicy" 
}

resource "aws_lb_listener_rule" "connect" {
  listener_arn = var.listener_arn
  priority     = 100

  action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.tg.arn
  }

  condition {
    host_header {
      values = ["${var.subdomain}.${var.root_domain}"]
    }
  }
}

resource aws_lb_target_group tg {
  name        = "tg-${var.docker_image}"
  port        = var.port
  protocol    = "HTTP"
  vpc_id      = var.vpc_id
  target_type = "instance"
  health_check {
    interval = 10
    matcher = "200,404"
    timeout = 9
  }
}

data "aws_iam_role" "ecs_role" {
  name = "${var.cluster_id}-ecs-role"
}

resource aws_iam_role service_role {
  name = "ecs-${var.docker_image}-role"

  assume_role_policy = jsonencode({
    "Version" : "2012-10-17",
    "Statement" : [
      {
        "Action" : "sts:AssumeRole",
        "Principal" : {
          "Service" : ["ecs.amazonaws.com", "ecs-tasks.amazonaws.com"]
          "AWS" : data.aws_iam_role.ecs_role.arn
        },
        "Effect" : "Allow",
        "Sid" : "",
      }
    ]
  })

}

resource aws_iam_role_policy_attachment service_role {
  role       = aws_iam_role.service_role.name
  policy_arn = aws_iam_policy.ecs_role_policy.arn
}

resource aws_iam_policy ecs_role_policy {
  name        = "service-${var.docker_image}-policy"
  path        = "/"
  description = "Policy for service ${var.docker_image}"

  policy = data.aws_iam_policy_document.ecs_role_policy.json
}

data "aws_iam_policy_document" "ecs_role_policy" {
  statement {
    actions = [
      "s3:ListBucket",
      "s3:GetObject",
      "s3:PutObject",
      "s3:DeleteObject",
      "logs:CreateLogStream",
      "logs:PutLogEvents",
      "sts:assumerole",
      "ssm:DescribeParameters",
    ]
    resources = ["*"]
  }
  statement {
    actions = [
      "ssm:GetParametersByPath",
      "ssm:GetParameters",
      "ssm:GetParameter",
    ]
    resources = ["arn:aws:ssm:*:*:parameter/*"]
  }
  statement {
    actions = [
      "sqs:ListQueues"
    ]
    resources = ["*"]
  }
  statement {
    actions = [
      "secretsmanager:GetResourcePolicy",
      "secretsmanager:GetSecretValue",
      "secretsmanager:DescribeSecret",
      "secretsmanager:ListSecretVersionIds"
    ]
    resources = ["arn:aws:secretsmanager:*:*:secret:*"]
  }
}

######################################
# Scaling
######################################

resource "aws_appautoscaling_target" "ecs_target" {
  min_capacity       = 1 # TODO: Two, if I did not pay :)
  max_capacity       = 4
  resource_id        = "service/${var.cluster_id}/${var.docker_image}"
  scalable_dimension = "ecs:service:DesiredCount"
  service_namespace  = "ecs"
}

resource "aws_appautoscaling_policy" "ecs_policy" {
  name               = "Autoscaling"
  policy_type        = "TargetTrackingScaling"
  resource_id        = aws_appautoscaling_target.ecs_target.resource_id
  scalable_dimension = aws_appautoscaling_target.ecs_target.scalable_dimension
  service_namespace  = aws_appautoscaling_target.ecs_target.service_namespace

 target_tracking_scaling_policy_configuration {
    target_value = 60

    customized_metric_specification {
      metric_name = "CPUUtilization"
      namespace   = "MyNamespace"
      statistic   = "Average"
      unit        = "Percent"

      dimensions {
        name  = "MyOptionalMetricDimensionName"
        value = "MyOptionalMetricDimensionValue"
      }
    }
  }
}