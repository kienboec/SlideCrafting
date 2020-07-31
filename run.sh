#!/bin/bash
kill -9 $(pgrep -f make.py) > /dev/null 2> /dev/null
python3 /miktex/work/slideCrafting/make.py &
