#!/bin/bash 
/usr/bin/mpm --update \
    && initexmf --register-root=/miktex/work/src/_templates \
    && mpm --admin --update-db \
    && initexmf --admin --update-fndb \
    && mpm --update