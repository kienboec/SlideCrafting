# SlideCrafting
*Crafting slides in docker.*

The docker container in this repo is only needed to compile markdown files (mounted into the source folder) into pdf using pandoc and latex (miktex, beamer, plantuml).

## Start

* create a folder to work in (see BASEFOLDER in code sample)
* clone this repo into a subfolder "slidecrafting"
* create a second folder to work in e.g.: "KnowledgeBase"
* (optional): create a folder structure "_templates/tex/latex/beamer/" and add a beamertheme-file with .sty extension to apply the theme automatically
* build the docker container using "docker build"
* run the docker container with mounted volumes (in and out)

Sample (windows):
```
TITLE SlideCrafting
SET BASEFOLDER=C:/repos
cd %BASEFOLDER%

docker stop slidecrafting-instance
docker rm   slidecrafting-instance

@mkdir slidecrafting\dist
docker build -t slidecrafting-container:latest -f slidecrafting\Dockerfile .
docker run --rm -ti --privileged -v %BASEFOLDER%/KnowledgeBase:/miktex/work/src:ro -v %BASEFOLDER%/slidecrafting/dist:/miktex/work/dist:rw -p 8080:80/tcp --name slidecrafting-instance slidecrafting-container
```
## Further Tooling Recommendations

* Editor: https://github.com/zettlr/zettlr
* Presenter: https://github.com/Cimbali/pympress/
* Table-Generator: https://www.tablesgenerator.com/markdown_tables#
  
## Icons
* Viewer-favicon: https://www.iconfinder.com/icons/272699/pdf_icon
* General ico: https://www.iconfinder.com/icons/2527990/analytics_business_chart_display_media_presentation_projector_icon