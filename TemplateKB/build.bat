@cls
title SlideCrafting (test)
REM assuming you cloned the project into c:\repos\SlideCrafting

docker stop slidecrafting-instance
docker rm   slidecrafting-instance

cd ..\..
if not exist slidecrafting\dist\NUL mkdir slidecrafting\dist
docker build -t slidecrafting-container:latest -f slidecrafting\Dockerfile .
docker run --rm -ti --privileged -v C:/repos/slidecrafting/TemplateKB:/miktex/work/src:ro -v C:/repos/slidecrafting/dist:/miktex/work/dist:rw --name slidecrafting-instance slidecrafting-container
cd slidecrafting\TemplateKB