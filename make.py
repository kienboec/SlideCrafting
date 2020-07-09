import subprocess
import shutil
import os
import time
from pathlib import Path
import yaml
import datetime

print("______________________________________________________")
print("")
print("Slide Crafting (started: " +  str(datetime.datetime.now()) + ")")
print("______________________________________________________")

#########
# config
#########
pandocApp = "pandoc"
pdflatexApp = "pdflatex"
baseDir = "/miktex/work/"
originFolder = baseDir + "src/"
workFolder = baseDir + "tmp/"
distFolder = baseDir + "dist/"
logFolder = baseDir + "log/"
beamerTemplate = originFolder + "_templates/tex/latex/beamer/" # only 1 beamer theme file in the folder supported!
configFile = baseDir + ".slidecrafting.config"
indexFilesExtension = ".meta.yml"
templateUpdateScript = "/miktex/work/slideCrafting/updateTemplate.sh"

########
# clean
########
# - dist folder
# - work folder

def rmdirRecursive(directory, removeOnlyContent=False):
    directory = Path(directory)
    for item in directory.iterdir():
        if item.is_dir():
            rmdirRecursive(item)
        else:
            item.unlink()
    if(not removeOnlyContent):
        directory.rmdir()

if os.path.isdir(distFolder):
    rmdirRecursive(distFolder, True)
    time.sleep(0.1) 
else:
    os.mkdir(distFolder)
print("dist folder cleaned..." + distFolder)

if os.path.isdir(workFolder):
    rmdirRecursive(workFolder)
    time.sleep(0.1)
print("work folder cleaned..." + workFolder)

###########################################
# take over data from src to work folder
###########################################
# (needed because temp files are required
# during the process and the src folder
# is mounted as a read-only volume)
subprocess.call(["cp", "-r", originFolder, workFolder])
time.sleep(0.1)
print("work folder filled with source data...")

#######################
# search index files
# (= projects)
#######################
topicsDict = {}
for file in os.listdir(workFolder):
    if file.endswith(indexFilesExtension):
        print("index file found: " + file)
        stream = open(workFolder + file, 'r')
        topicsDict[file] = yaml.load(stream, Loader=yaml.FullLoader)['input-files']
        stream.close()
        print("   files: ", topicsDict[file])
print("searching for index files completed")

###############
# build config
###############
themeArgs = []
themeName = None
if os.path.isdir(beamerTemplate):
    for file in os.listdir(beamerTemplate):
        if file.startswith("beamertheme") and file.endswith(".sty"):
            themeName = file[11:-4]
            themeArgs = ["-V", "theme:" + themeName]
            print("theme applied: "+ themeName)
            break

needToWriteConfig = False
storedConfig = {}
if not os.path.isfile(configFile):
    needToWriteConfig = True
else:
    stream = open(configFile, 'r')
    storedConfig = yaml.load(stream, Loader=yaml.FullLoader)
    stream.close()

needToWriteConfig = needToWriteConfig or storedConfig.get("template") != themeName

if needToWriteConfig:
    print("updating template")
    subprocess.call([templateUpdateScript])
    f = open(configFile, "w")
    f.write("template: " + themeName)
    f.close()
    print("wrote config")
else:
    print("config up to date...")

pandocArgs = [
    # "--verbose",
    "--from=markdown+raw_tex+header_attributes+implicit_header_references+raw_attribute+inline_code_attributes+fancy_lists+line_blocks", 
    "--log="+logFolder+"pandoc.log",
    # "--toc", # table of content
    # "--toc-depth=2",
    "--highlight-style=tango",
    # "--listings", # removes highlighting from source code samples
    "--resource-path=.", # : on linux ; on windows as delimiter
    "--filter", "pandoc-plantuml",
    "-s", # standalone
    "--self-contained",
    "--indented-code-classes=numberLines",
    "--tab-stop=2",
    "--strip-comments",
    "--number-sections",
    # "--dump-args", # dumps only and does not process the request afterwards
    "--fail-if-warnings",
    # "--strip-empty-paragraphs", -> warning deprecated
    "--strip-comments",
    # "--mathml",
    
    # -V for variables
    #"-V", "lot", # list of tables
    #"-V", "lof"  # list of figures
]

########
# build
########
os.chdir(workFolder)

for fileName in topicsDict.keys():
    pandocMetadataArg = "--metadata-file=" + workFolder + str(fileName)
    projectName = fileName[0:-9]
    
    projectArgs = {
        "pdf":      [pandocApp] + topicsDict[fileName] + [pandocMetadataArg] + pandocArgs + 
                    ["--to=pdf", "--pdf-engine=" + pdflatexApp, "-o", distFolder + projectName + "_index.pdf"],

        "beamer":   [pandocApp] + topicsDict[fileName] + [pandocMetadataArg] + pandocArgs + 
                    ["--to=beamer"] + themeArgs + ["--pdf-engine=" + pdflatexApp, 
                     "-o", distFolder + projectName + "_slides.pdf", "--slide-level=2"],

        "beamerN":  [pandocApp] + topicsDict[fileName] + [pandocMetadataArg] + pandocArgs + 
                    ["--to=beamer"] + themeArgs + ["-V","beameroption:show notes", "--pdf-engine=" + pdflatexApp, 
                     "-o", distFolder + projectName + "_slides_notes.pdf", "--slide-level=2"]
    }
    
    for mode in projectArgs.keys():
        print ("---------------------------------------------- " + str(datetime.datetime.now()))
        print(mode + ": (" + fileName + ")")
        exitCode = subprocess.call(projectArgs[mode])
        print ("exit code (https://pandoc.org/MANUAL.html#exit-codes): ", exitCode)
