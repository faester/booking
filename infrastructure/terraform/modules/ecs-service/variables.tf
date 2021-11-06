variable subdomain {
  type = string
}

variable root_domain {
  type = string
}

variable docker_image {
  type = string
}

variable port {
  type = numeric
}

variable cpu {
  type    = numeric
  default = 256
}

variable memory {
  type    = numeric
  default = 512
}
