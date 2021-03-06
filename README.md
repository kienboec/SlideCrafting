# SlideCrafting
*Crafting slides in docker.*

The docker container in this repo is only needed to compile markdown files 
(mounted into the source folder) into pdf using pandoc and latex (miktex, beamer, plantuml).

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
docker run --rm -ti --privileged -v %BASEFOLDER%/KnowledgeBase:/miktex/work/src:ro -v %BASEFOLDER%/slidecrafting/dist:/miktex/work/dist:rw -p 8080:8080/tcp --name slidecrafting-instance slidecrafting-container
```

### Integrated Viewer
An integrated viewer is exposed on port 8080.

Based on the files.json file generated in the dist folder it allows you to select a pdf to show.

Shortcuts:
* ctrl + alt + L: shows the generation log
* ctrl + alt + G: reloads the background data (same as goto button)
* ctrl + alt + R: activates refreshing
* Arrow Keys for navigation

## Further Tooling Recommendations

* Editor: https://github.com/zettlr/zettlr
* VSCode: https://code.visualstudio.com/
* Presenter: https://github.com/Cimbali/pympress/
* Table-Generator: https://www.tablesgenerator.com/markdown_tables#
* Edotor (dot): https://edotor.net/
* Graphviz: http://graphviz.it/#/
* Plantuml: https://www.planttext.com/ or https://liveuml.com/
* Mermaid-js: https://mermaid-js.github.io/mermaid-live-editor
* textract (npm): to extract text from pptx
* https://github.com/jupe/puml2code

## Icons
* Viewer-favicon: https://www.iconfinder.com/icons/272699/pdf_icon
* General ico: https://www.iconfinder.com/icons/2527990/analytics_business_chart_display_media_presentation_projector_icon

## Best Practice

### Make Listings smaller
in Beamer add header-includes
```
  - \usepackage{fvextra}
  - \DefineVerbatimEnvironment{Highlighting}{Verbatim}{breaklines,commandchars=\\\{\},fontsize=\tiny}
```

