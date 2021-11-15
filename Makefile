.PHONY: all build_dotnet_core_projects build_docker_images

all: build_dotnet_core_projects build_docker_images

build_dotnet_core_projects: 
	@echo "Build .NET core solution"
	dotnet build src

build_docker_images: 
	docker build . -f docker/Dockerfile.build -t convention-api-build:local
	docker build . -f docker/Dockerfile.identityserver -t identity-server:local
	docker build . -f docker/Dockerfile.convention-api -t convention-api:local
	docker build . -f docker/Dockerfile.convention-website -t convention-website:local

build: build_dotnet_core_projects

run: build
	dotnet ./src/idp/IdentityServer/bin/Debug/netcoreapp3.1/IdentityServer.dll
	

docker_run: build_docker_images
	docker run -d --rm -p 8000:80 --name identity_server identity-server:local

stop: 
	docker stop identity_server

logs: 
	docker logs identity_server

terraform_docker:
	docker build . -f docker/Dockerfile.terraform -t booking_terraform:local

terraform-shell: terraform_docker
	docker run -ti --mount type=bind,source=$(shell pwd),target=/home/terraform/booking --mount type=bind,source=$(shell echo ~)/.ssh/,target=/home/terraform/.ssh --mount type=bind,source=$(shell echo ~)/.aws/,target=/home/terraform/.aws  booking_terraform:local bash

tag_for_ecr: build_docker_images
	docker tag identity-server:local 539839626842.dkr.ecr.eu-west-1.amazonaws.com/identity-server:latest
	docker tag convention-api:local 539839626842.dkr.ecr.eu-west-1.amazonaws.com/convention-api:latest
	docker tag convention-website:local 539839626842.dkr.ecr.eu-west-1.amazonaws.com/convention-website:latest

publish: build_docker_images tag_for_ecr
	docker run -ti --mount type=bind,source=$(shell pwd),target=/home/terraform/booking --mount type=bind,source=$(shell echo ~)/.ssh/,target=/home/terraform/.ssh --mount type=bind,source=$(shell echo ~)/.aws/,target=/home/terraform/.aws  booking_terraform:local aws ecr get-login-password --region eu-west-1 --profile mfaester |docker login --username AWS --password-stdin 539839626842.dkr.ecr.eu-west-1.amazonaws.com
	docker push 539839626842.dkr.ecr.eu-west-1.amazonaws.com/identity-server:latest
	docker push 539839626842.dkr.ecr.eu-west-1.amazonaws.com/convention-api:latest
	docker push 539839626842.dkr.ecr.eu-west-1.amazonaws.com/convention-website:latest

stop-tasks:  terraform_docker
	docker run -t --mount type=bind,source=$(shell pwd),target=/home/terraform/booking --mount type=bind,source=$(shell echo ~)/.ssh/,target=/home/terraform/.ssh --mount type=bind,source=$(shell echo ~)/.aws/,target=/home/terraform/.aws  booking_terraform:local /home/terraform/bin/stop-tasks.sh

deploy: publish stop-tasks 
