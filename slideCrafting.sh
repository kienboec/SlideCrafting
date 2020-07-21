#!/bin/bash
cd /miktex/work/dist/ && python3 -m http.server 80 & 
/miktex/work/run.sh && /miktex/work/watch.sh
