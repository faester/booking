module convention_website {
  source       = "../../modules/ecs-service"
  docker_image = "convention-website"
  vpc_id       = var.vpc_id
  cluster_id   = "booking-main"

  memory = 1024
  cpu    = 1024

  root_domain  = "mfaester.dk"
  port         = 8002
  subdomain    = "convention-website"
  listener_arn = "arn:aws:elasticloadbalancing:eu-west-1:539839626842:listener/app/booking-public-lb/aaeb45b1f270cbf7/46764fad2dd56422"
}
