output domain_validation_options {
  value = module.lb.domain_validation_options
}

output listener_arn {
  value = module.lb.listener_arn
}

output "capacity_provider_name" {
  value = aws_ecs_capacity_provider.booking.name
}