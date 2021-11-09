variable subdomain {
  type = string
}

variable cluster_id {
  type = string
}

variable vpc_id {
  type = string
}

variable root_domain {
  type = string
}

variable docker_image {
  type = string
}

variable listener_arn {
  type    = string
  default = "latest"
}

variable docker_tag {
  type    = string
  default = "latest"
}

variable subnet_ids {
  type = list(string)
}