#!/bin/bash
cd /miktex/work/dist/ && python3 -m http.server 8080 2>> /miktex/work/log/access.log & 
sleep 1 && /miktex/work/run.sh && /miktex/work/watch.sh
