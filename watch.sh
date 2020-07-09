#!/bin/bash
while true; do

inotifywait -e modify,create,delete,move -r /miktex/work/src && /miktex/work/run.sh

done