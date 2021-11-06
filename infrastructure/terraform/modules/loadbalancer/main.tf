resource "aws_lb" "lb" {
  name               = var.name
  internal           = false
  load_balancer_type = "application"
  security_groups    = [aws_security_group.lb_sg.id]
  subnets            = var.subnet_ids

  enable_deletion_protection = true

  access_logs {
    bucket  = aws_s3_bucket.lb_logs.bucket
    prefix  = var.name
    enabled = true
  }
}


resource aws_security_group lb_sg {
  vpc_id = var.vpc_id
}

resource aws_s3_bucket lb_logs {
  bucket = "lb-logs-${var.name}"


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
