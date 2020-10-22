import subprocess
import shutil
import os
import time
from pathlib import Path
import yaml
import datetime
import json

topicsDict = {}
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
    if file == "tests.meta.yml.000":
        stream = open("c:\\repos\\KnowledgeBase\\" + file, 'r')
        indexFileContent = yaml.load(stream, Loader=yaml.FullLoader)
        version[file] = str(indexFileContent['version'])
        topicsDict[file] = readListFilesRecursive(indexFileContent['input-files'])
        stream.close()
        output = "   files: " + ", ".join(topicsDict[file]) 
        # print("   files: " + ", ".join(topicsDict[file]))
        if output != "   files: _tests/intro.md, _tests/section_1.md, _tests/section_2.md, _tests/section_3.md":
            raise Exception('test failed')

print('test ok')

directory = Path("C:\\temp")
for item in directory.iterdir():
    print(item.name)
