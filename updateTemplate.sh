#!/bin/bash 

echo update
mpm --update

echo register template
initexmf --register-root=/miktex/work/src/_templates 

echo update-db
mpm --admin --update-db 

echo update fndb
initexmf --admin --update-fndb

echo update
mpm --update
