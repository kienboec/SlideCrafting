#!/bin/bash
kill -9 $(pgrep -f make.py)
python3 /miktex/work/slideCrafting/make.py &
