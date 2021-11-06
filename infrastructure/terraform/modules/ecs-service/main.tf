
resource aws_ecs_task_definition service {
  family = "booking"
  container_definitions = jsonencode([
    {
      name      = "first"
      image     = var.docker_image
      cpu       = var.cpu
      memory    = var.memory
      essential = true
      portMappings = [
        {
          containerPort = 80
          hostPort      = var.port
        }
      ]
    }
  ])
}

resource aws_ecs_service mongo {
  name            = var.docker_image
  cluster         = var.cluster_id
  task_definition = aws_ecs_task_definition.service.arn
  desired_count   = 3
  iam_role        = aws_iam_role.foo.arn
  depends_on      = [aws_iam_role_policy.foo]

  ordered_placement_strategy {
    type  = "binpack"
    field = "cpu"
  }

  load_balancer {
    target_group_arn = aws_lb_target_group.foo.arn
    container_name   = var.docker_image
    container_port   = var.port
  }

  lifecycle {
    ignore_changes = [desired_count]
  }
}

aws_iam_role_policy service_policy {

}

resource aws_lb_target_group tg {
  name        = "tg-${var.docker_image}"
  port        = var.port
  protocol    = "http"
  vpc_id      = var.vpc_id
  target_type = "instance"
}
