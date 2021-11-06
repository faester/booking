resource "aws_lb" "lb" {
  name               = var.name
  internal           = false
  load_balancer_type = "application"
  security_groups    = [aws_security_group.lb_sg.id]
  subnets            = var.subnet_ids

  access_logs {
    bucket  = aws_s3_bucket.lb_logs.bucket
    prefix  = var.name
    enabled = true
  }
}

data "aws_elb_service_account" "main" {}


resource aws_security_group lb_sg {
  vpc_id = var.vpc_id
}

resource aws_s3_bucket lb_logs {
  bucket = "lb-logs-${var.name}"

  policy = <<POLICY
{
  "Id": "Policy",
  "Version": "2012-10-17",
  "Statement": [
    {
      "Action": [
        "s3:PutObject"
      ],
      "Effect": "Allow",
      "Resource": "arn:aws:s3:::lb-logs-${var.name}/${var.name}/*",
      "Principal": {
        "AWS": [
          "${data.aws_elb_service_account.main.arn}"
        ]
      }
    }
  ]
}
POLICY

  lifecycle_rule {
    id      = "log"
    enabled = true

    prefix = var.name

    tags = {
      rule      = "log"
      autoclean = "true"
    }

    transition {
      days          = 30
      storage_class = "ONEZONE_IA"
    }

    transition {
      days          = 60
      storage_class = "GLACIER"
    }

    expiration {
      days = 90
    }
  }
}

resource "aws_acm_certificate" "cert" {
  domain_name       = "*.${var.root_domain}"
  validation_method = "DNS"

  lifecycle {
    create_before_destroy = true
  }
}
