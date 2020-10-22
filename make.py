import subprocess
import shutil
import os
import time
from pathlib import Path
import yaml
import datetime
import json

#########
# config
#########
pandocApp = "pandoc"
pdflatexApp = "pdflatex"
baseDir = "/miktex/work/"
originFolder = baseDir + "src/"
workFolder = baseDir + "tmp/"
distFolder = baseDir + "dist/"
archiveFolder = distFolder + "archive/"
logFolder = baseDir + "log/"
pandocLogFile = logFolder+"pandoc.log"
logFileName = "slideCrafting.log"
serverLogFileName = "access.log"
logFile = logFolder + logFileName
serverLogFile = logFolder + serverLogFileName
pptxReference = originFolder + "_templates/pptx/fhtw_reference.pptx"
beamerTemplate = originFolder + "_templates/tex/latex/beamer/" # only 1 beamer theme file in the folder supported!
configFile = baseDir + ".slidecrafting.config"
indexFilesExtension = ".meta.yml"
listFilesExtension = ".lst.yaml"
templateUpdateScript = "/miktex/work/slideCrafting/updateTemplate.sh"
viewerHtml = '/miktex/work/slideCrafting/viewer/index.html'
favicon = '/miktex/work/slideCrafting/viewer/favicon.ico'
attemptsOnError = 3

######
# log
######
logFileHandler = open(logFile, 'w')

def writeToLog(text):
    logFileHandler.write(datetime.datetime.now().strftime("%H:%M:%S") + ": " + text + "\n")
    logFileHandler.flush()

def writeToScreen(text):
    print(text)

def writeToLogAndScreen(text):
    writeToLog(text)
    writeToScreen(text)

writeToScreen("\n\n\n\n\n")
writeToLogAndScreen("______________________________________________________")
writeToLogAndScreen("")
writeToLogAndScreen("Slide Crafting (started: " +  str(datetime.datetime.now()) + ")")
writeToLogAndScreen("______________________________________________________")

###################
# read gen options
###################
def checkEnvValueSet(key, value):
    return key in os.environ and os.environ[key] == value

generationMethods = []
if(checkEnvValueSet("GEN_BEAMER", "1")):
    writeToLog("beamer-slides requested")
    generationMethods.append("beamer")

if(checkEnvValueSet("GEN_BEAMER_NOTES", "1")):
    writeToLog("beamer-slides with notes requested")
    generationMethods.append("beamerN")

if(checkEnvValueSet("GEN_HANDOUT", "1")):
    writeToLog("handout requested")
    generationMethods.append("pdf")

if(checkEnvValueSet("GEN_PPTX", "1")):
    writeToLog("powerpoint-slides requested")
    generationMethods.append("pptx")

if(checkEnvValueSet("GEN_REVEALJS", "1")):
    writeToLog("reveal.js-slides requested")
    generationMethods.append("revealjs")

if(len(generationMethods) == 0):
    writeToLog("no request... fallback slides with notes")
    generationMethods = ["beamerN"]

filters = []
if(checkEnvValueSet("FILTER_PLANTUML", "1")):
    writeToLog("embedded plantuml requested")
    filters.append("pandoc-plantuml")

########
# clean
########
# - dist folder
# - work folder

def rmdirExt(directory, removeOnlyContent, filesOnly):
    returnValue = True
    directory = Path(directory)
    for item in directory.iterdir():
        try:
            if item.is_dir():
                if not filesOnly:
                    if(not rmdirExt(item, False, False)):
                        returnValue = False
            else:
                item.unlink()
        except:
            writeToLogAndScreen(f"item can not be deleted: {str(item)}")
            returnValue = False
    if(not removeOnlyContent):
        directory.rmdir()

    return returnValue

def archiveFiles(directory, archiveFolder):
    directory = Path(directory)
    for item in directory.iterdir():
        try:
            if not item.is_dir():
                item.rename(archiveFolder + item.name)
        except:
            writeToLogAndScreen(f"item can not be moved: {str(item)}")
            try:
                shutil.copy(item.resolve(), archiveFolder + item.name)
            except:
                writeToLogAndScreen(f"item can not be copied: {str(item)}")
                try:
                    shutil.copy(item.resolve(), archiveFolder + "backup_" + item.name)
                except:
                    writeToLogAndScreen(f"item can not be copied: {str(item)}")

if os.path.isdir(distFolder):
    if not os.path.isdir(archiveFolder):
        os.mkdir(archiveFolder)
 
    archiveFiles(distFolder, archiveFolder)

    if (not rmdirExt(distFolder, True, True)):
        writeToScreen("dist folder cleanup failed: stopped execution")
        exit()
    time.sleep(0.1) 
else:
    os.mkdir(distFolder)
    os.mkdir(archiveFolder)
writeToLog("dist folder cleaned..." + distFolder)

if os.path.isdir(workFolder):
    if (not rmdirExt(workFolder, False, False)):
        writeToScreen("work folder cleanup failed: stopped execution")
        exit()
    time.sleep(0.1)
writeToLog("work folder cleaned..." + workFolder)

# subprocess.call(["ln", "-s", logFile, distFolder + logFileName[0:-4] + ".html"])
# subprocess.call(["ln", "-s", serverLogFile, distFolder + serverLogFileName[0:-4] + ".html"])

##########
# viewer
##########
writeToLog("copy viewer")
subprocess.call(["cp", viewerHtml, distFolder + "index.html"])
subprocess.call(["cp", favicon, distFolder + "favicon.ico"])

###############
# pandoc-config
###############
# https://pandoc.org/MANUAL.html#exit-codes
pandocErrors = {
    "0": "OK",
    "3": "PandocFailOnWarningError",
    "4": "PandocAppError",
    "5": "PandocTemplateError",
    "6": "PandocOptionError",
    "21": "PandocUnknownReaderError",
    "22": "PandocUnknownWriterError",
    "23": "PandocUnsupportedExtensionError",
    "31": "PandocEpubSubdirectoryError",
    "43": "PandocPDFError",
    "47": "PandocPDFProgramNotFoundError",
    "61": "PandocHttpError",
    "62": "PandocShouldNeverHappenError",
    "63": "PandocSomeError",
    "64": "PandocParseError",
    "65": "PandocParsecError",
    "66": "PandocMakePDFError",
    "67": "PandocSyntaxMapError",
    "83": "PandocFilterError",
    "91": "PandocMacroLoop",
    "92": "PandocUTF8DecodingError",
    "93": "PandocIpynbDecodingError",
    "97": "PandocCouldNotFindDataFileError",
    "99": "PandocResourceNotFound"
}
###########################################
# take over data from src to work folder
###########################################
# (needed because temp files are required
# during the process and the src folder
# is mounted as a read-only volume)
subprocess.call(["cp", "-r", originFolder, workFolder])
time.sleep(0.1)
writeToLog("work folder filled with source data...")

#######################
# search index files
# (= projects)
#######################
topicsDict = {}
version = {}

def readListFilesRecursive(inputFiles):
    patchedInputFiles = []
    for fileToCheck in sorted(inputFiles):
        if fileToCheck.endswith(listFilesExtension):
            streamLst = open(workFolder + fileToCheck, 'r')
            listFileContent = yaml.load(streamLst, Loader=yaml.FullLoader)    
            streamLst.close()
            for fileFromList in readListFilesRecursive(listFileContent['input-files']):
                patchedInputFiles.append(fileFromList)
        else:
            patchedInputFiles.append(fileToCheck)

    return patchedInputFiles

for file in sorted(os.listdir(workFolder)):
    if file.endswith(indexFilesExtension):
        writeToLogAndScreen("index file found: " + file)
        stream = open(workFolder + file, 'r')
        indexFileContent = yaml.load(stream, Loader=yaml.FullLoader)
        version[file] = str(indexFileContent['version'])
        topicsDict[file] = readListFilesRecursive(indexFileContent['input-files'])
        stream.close()
        writeToLog("   files: " + ", ".join(topicsDict[file]))

writeToLog("searching for index files completed")

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
            writeToLog("theme applied: "+ themeName)
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
    writeToLog("updating template")
    executionResult = subprocess.run([templateUpdateScript], stdout=subprocess.PIPE, stderr=subprocess.PIPE, universal_newlines=True)
    writeToLog("standard out: \n" + executionResult.stdout)
    writeToLog("standard err: \n" + executionResult.stderr)

    f = open(configFile, "w")
    f.write("template: " + themeName)
    f.close()
    writeToLog("wrote config")
else:
    writeToLog("config up to date...")

pandocArgs = [
    # "--verbose",
    "--from=markdown+raw_tex+header_attributes+implicit_header_references+raw_attribute+inline_code_attributes+fancy_lists+line_blocks", 
    "--log="+pandocLogFile,
    # "--toc", # table of content
    # "--toc-depth=2",
    "--highlight-style=tango",
    # "--listings", # removes highlighting from source code samples
    "--resource-path=.", # : on linux ; on windows as delimiter
    # "--filter", "pandoc-plantuml",
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
    "-M", "date="+ datetime.datetime.today().strftime('%Y-%m-%d') +""

    # -V for variables
    #"-V", "lot", # list of tables
    #"-V", "lof"  # list of figures
]

filterArgs = []
for f in filters:
    filterArgs.append("--filter")
    filterArgs.append(f)

########
# build
########
os.chdir(workFolder)

outputFileNames = []
for fileName in sorted(topicsDict.keys()):
    pandocMetadataArg = "--metadata-file=" + workFolder + str(fileName)
    projectName = fileName[0:-9]
    
    projectArgs = {
        "beamer":   [pandocApp] + topicsDict[fileName] + [pandocMetadataArg] + pandocArgs + filterArgs +
                    ["--to=beamer"] + themeArgs + ["--pdf-engine=" + pdflatexApp, 
                     "-o", distFolder + projectName + "_slides.pdf", "--slide-level=3"],

        "beamerN":  [pandocApp] + topicsDict[fileName] + [pandocMetadataArg] + pandocArgs + filterArgs +
                    ["--to=beamer"] + themeArgs + ["-V","beameroption:show notes", "--pdf-engine=" + pdflatexApp, 
                     "-o", distFolder + projectName + "_slides_notes.pdf", "--slide-level=3"],

        "pdf":      [pandocApp] + topicsDict[fileName] + [pandocMetadataArg] + pandocArgs + filterArgs +
                    ["--to=pdf", "--pdf-engine=" + pdflatexApp, "-o", distFolder + projectName + "_index.pdf"],

        "pptx":     [pandocApp] + topicsDict[fileName] + [pandocMetadataArg] + pandocArgs + filterArgs +
                    ["--to=pptx", "--reference-doc=" + pptxReference, "-o", distFolder + projectName + "_slides.pptx"],
        
        # work in progress
        "revealjs":   [pandocApp] + topicsDict[fileName] + [pandocMetadataArg] + pandocArgs + filterArgs +
                    ["--to=revealjs", "-V", "revealjs-url=https://unpkg.com/reveal.js@4.0.2/", "-o", distFolder + projectName + "_slides.html"]
    }
    
    for mode in projectArgs.keys():
        if mode in generationMethods:
            exitCode = ""
            attempt = 1
            while exitCode != "0" and attempt <= attemptsOnError:
                writeToLogAndScreen ("---------------------------------------------- start    : " + str(datetime.datetime.now()))
                outputFileName = projectArgs[mode][projectArgs[mode].index('-o')+1]
                outputFileNames.append(outputFileName)
                writeToLogAndScreen(mode + ": (file:" + outputFileName + ", version: " + version[fileName] + ", attempt: " + str(attempt) + ")")
                executionResult = subprocess.run(projectArgs[mode], stdout=subprocess.PIPE, stderr=subprocess.PIPE, universal_newlines=True)
                
                if(len(executionResult.stdout) > 0):
                    first = True
                    for line in executionResult.stdout.splitlines():
                        patchedLine = line.strip()
                        if len(patchedLine) > 0:
                            if first:
                                writeToLogAndScreen("standard out:")
                                first=False
                            writeToLogAndScreen(" - " + patchedLine)
                    
                if(len(executionResult.stderr) > 0):
                    first = True
                    for line in executionResult.stderr.splitlines():
                        # i accept these warnings and remove it from the error output                
                        patchedLine = line.replace("pdflatex: security risk: running with elevated privileges","").replace("pdflatex: major issue: So far, no MiKTeX administrator has checked for updates.", "").strip()
                        
                        if len(patchedLine) > 0:
                            if first:
                                writeToLogAndScreen("standard error:")
                                first=False
                            writeToLogAndScreen(" - " + patchedLine)
                            #  -Unfortunately, the package koma-script could not be installed.
                            # consider not to subtract 1 in attempts if a package could not be installed
                    

                exitCode = str(executionResult.returncode)
                exitCodeDescription = "error"
                
                if exitCode != "0":
                    if exitCode in pandocErrors:
                        exitCodeDescription = pandocErrors[exitCode]
                    writeToLogAndScreen ("exit code: " + exitCode + " (" + exitCodeDescription + ")")
                    writeToLogAndScreen ("                                               failed   : " + str(datetime.datetime.now()))
                else:
                    writeToLogAndScreen ("                                               succeeded: " + str(datetime.datetime.now()))
                attempt += 1
            
writeToLogAndScreen("\n\nFiles created: " + ", ".join(outputFileNames))
writeToScreen("\n\n")
outputFileNameFile = open(distFolder+"files.json","w")
isFirst = True

outputFileNameFile.write("[\n")
for outputFileName in outputFileNames:
    if not isFirst:
        outputFileNameFile.write(",\n")
    outputFileNameFile.write("   \"" + outputFileName.split("/")[-1] + "\"")
    isFirst = False

outputFileNameFile.write("\n]")
outputFileNameFile.close()

logFileHandler.close()
time.sleep(0.1) 
