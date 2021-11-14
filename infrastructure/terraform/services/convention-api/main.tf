module convention_api {
  source       = "../../modules/ecs-service"
  docker_image = "convention-api"
  vpc_id       = var.vpc_id
  cluster_id   = "booking-main"

  memory = 1024
  cpu    = 1024

  root_domain  = "mfaester.dk"
  port         = 8001
  subdomain    = "convention-api"
  listener_arn = "arn:aws:elasticloadbalancing:eu-west-1:539839626842:listener/app/booking-public-lb/aaeb45b1f270cbf7/46764fad2dd56422"
}


resource aws_iam_role_policy_attachment service_role {
  role       = module.convention_api.task_role_name
  policy_arn = aws_iam_policy.ecs_role_policy.arn
}

resource aws_iam_policy ecs_role_policy {
  name        = "service-convention-api-simpledb-policy"
  path        = "/"
  description = "Policy for service convention api"

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
    resources = [
      "arn:aws:sdb:*:${var.account_id}:domain/${aws_simpledb_domain.conventions.name}",
      "arn:aws:sdb:*:${var.account_id}:domain/${aws_simpledb_domain.events.name}",
      "arn:aws:sdb:*:${var.account_id}:domain/${aws_simpledb_domain.talks.name}",
    ]
  }
  statement {
    actions = [
      "sdb:ListDomains",
    ]
    resources = ["*"]
  }
}

resource "aws_simpledb_domain" "conventions" {
  name = "conventions"
}

resource "aws_simpledb_domain" "events" {
  name = "events"
}

resource "aws_simpledb_domain" "talks" {
  name = "talks"
}
