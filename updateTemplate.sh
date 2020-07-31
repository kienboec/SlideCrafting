#!/bin/bash 
echo update
/usr/bin/mpm --update
echo register template
initexmf --register-root=/miktex/work/src/_templates \
echo update-db
mpm --admin --update-db \
echo update fndb
initexmf --admin --update-fndb