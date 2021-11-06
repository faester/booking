resource aws_ecr_repository repos {
  count = length(var.repositories)
  name  = var.repositories[count.index]
}
