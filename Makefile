.PHONY: all build_dotnet_core_projects build_docker_images

all: build_dotnet_core_projects build_docker_images

build_dotnet_core_projects: 
	@echo "Build .NET core solution"
	dotnet build src

build_docker_images: 
	docker build . -f docker/Dockerfile.identityserver -t identityserver:local

run: build_docker_images
	docker run -d --rm -p 8000:80 --name identity_server identityserver:local

stop: 
	docker stop identity_server

logs: 
	docker logs identity_server

terraform_docker:
	docker build . -f docker/Dockerfile.terraform -t booking_terraform:local

terraform-shell: terraform_docker
	docker run -ti --mount type=bind,source=$(shell pwd),target=/home/terraform/booking --mount type=bind,source=$(shell echo ~)/.ssh/,target=/home/terraform/.ssh --mount type=bind,source=$(shell echo ~)/.aws/,target=/home/terraform/.aws  booking_terraform:local bash
