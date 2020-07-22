#!/bin/bash
cd /miktex/work/dist/ && python3 -m http.server 8080 & 
/miktex/work/run.sh && /miktex/work/watch.sh
