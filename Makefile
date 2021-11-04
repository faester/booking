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
