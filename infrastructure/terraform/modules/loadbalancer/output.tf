output domain_validation_options {
  value = aws_acm_certificate.cert.domain_validation_options
}

output listener_arn {
  value = aws_lb_listener.front_end.arn
}