module identity {
  source       = "../../modules/ecs-service"
  docker_image = "identity-server"
  vpc_id       = var.vpc_id
  cluster_id   = "booking-main"

  memory = 1024
  cpu    = 1024

  root_domain  = "mfaester.dk"
  port         = 8000
  subdomain    = "identity-server"
  listener_arn = "arn:aws:elasticloadbalancing:eu-west-1:539839626842:listener/app/booking-public-lb/aaeb45b1f270cbf7/46764fad2dd56422"
}


resource aws_iam_role_policy_attachment service_role {
  role       = module.identity.task_role_name
  policy_arn = aws_iam_policy.ecs_role_policy.arn
}

resource aws_iam_policy ecs_role_policy {
  name        = "service-identity-server-simpledb-policy"
  path        = "/"
  description = "Policy for service identity-server allowing "

  policy = data.aws_iam_policy_document.ecs_role_policy.json
}

data "aws_iam_policy_document" "ecs_role_policy" {
  statement {
    actions = [
                "sdb:DeleteAttributes",
                "sdb:GetAttributes",
                "sdb:BatchDeleteAttributes",
                "sdb:PutAttributes",
                "ssm:GetParametersByPath",
                "sdb:Select",
                "sdb:BatchPutAttributes",
                "sdb:DomainMetadata"
    ]
    resources = ["arn:aws:sdb:*:${var.account_id}:domain/users"]
  }
  statement {
    actions = [
                "sdb:ListDomains",
    ]
    resources = ["*"]
  }
  statement {
    actions = [
                "sqs:SendMessage",
                "sqs:DeleteMessage",
    ]
    resources = [aws_sqs_queue.events.arn]
  }

}

resource "aws_simpledb_domain" "users" {
  name = "users"
}

resource "aws_sqs_queue" "events" {
  name = "booking-audit-events"
  message_retention_seconds = 86400 * 7
}
