import subprocess
import shutil
import os
import time
from pathlib import Path
import yaml
import datetime
import json

topicsDict = {}
exercises = {}
version = {}

###############################################################################
# Test bench
###############################################################################

def readListFilesRecursive(inputFiles):
    patchedInputFiles = []
    for fileToCheck in sorted(inputFiles):
        if fileToCheck.endswith(".lst.yaml"):
            streamLst = open("c:\\repos\\KnowledgeBase\\" + fileToCheck, 'r')
            listFileContent = yaml.load(streamLst, Loader=yaml.FullLoader)    
            streamLst.close()
            for fileFromList in readListFilesRecursive(listFileContent['input-files']):
                patchedInputFiles.append(fileFromList)
        else:
            patchedInputFiles.append(fileToCheck)

    return patchedInputFiles

for file in sorted(os.listdir("c:\\repos\\KnowledgeBase\\")):
    if file == "_tests.meta.yml.000":
        stream = open("c:\\repos\\KnowledgeBase\\" + file, 'r')
        indexFileContent = yaml.load(stream, Loader=yaml.FullLoader)
        version[file] = str(indexFileContent['version'])
        if("input-files" in indexFileContent):
            topicsDict[file] = readListFilesRecursive(indexFileContent['input-files'])
            print("input-files found")
        else:
            topicsDict[file] = []
            print("input-files not found")

        if("exercise-files" in indexFileContent):
            exercises[file] = readListFilesRecursive(indexFileContent['input-files'])
            print("exercise-files found")
        else:
            exercises[file] = []
            print("exercise-files not found")
            
        stream.close()
        output = "   files: " + ", ".join(topicsDict[file]) 
        # print("   files: " + ", ".join(topicsDict[file]))
        if output != '   files: _tests/00__intro.md, _tests/01__section.md, _tests/02__section.md, _tests/03__section.md, _tests/_title.md':
            raise Exception('test failed')

print('test ok')

#directory = Path("C:\\temp")
#for item in directory.iterdir():
#    print(item.name)
