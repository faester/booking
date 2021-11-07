output domain_validation_options {
  value = aws_acm_certificate.cert.domain_validation_options
}

output listener_arn {
  value = aws_lb_listener.front_end.arn
}

output lb_sg_arn {
  description = "Security group for Load Balancer"
  value       = aws_security_group.lb_sg.arn
}