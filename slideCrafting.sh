#!/bin/bash
# cd /miktex/work/dist/ && python3 -m http.server 8080 2>> /miktex/work/log/access.log & 
cd /miktex/work/slideCrafting/webserver && npm start &
sleep 1 && /miktex/work/run.sh && /miktex/work/watch.sh
