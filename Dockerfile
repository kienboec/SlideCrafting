FROM ubuntu:focal

# Based on: https://github.com/MiKTeX/docker-miktex/blob/master/Dockerfile
ARG DEBIAN_FRONTEND=noninteractive
ENV TZ=Europe/Vienna
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone

RUN    apt-get update \
    && apt-get install -y --no-install-recommends \
            apt-utils \
            apt-transport-https \
            ca-certificates \
            dirmngr \
            ghostscript \
            gnupg \
            gosu \
            make \
            perl \
            \
            python3 \
            python3-pip \
            python3-setuptools \
            wget \
            openjdk-8-jdk \
            inotify-tools \
            nodejs \
            npm \
            vim \
			graphviz \
			librsvg2-bin

# MikTex
RUN apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys D6BC243565B2087BC3F897C9277A7293F59E4889
RUN echo "deb http://miktex.org/download/ubuntu focal universe" | tee /etc/apt/sources.list.d/miktex.list

RUN apt-key adv --refresh-keys
RUN apt-key adv --keyserver hkps://keyserver.ubuntu.com --refresh-keys

RUN    apt-get update \
    && apt-get install -y --no-install-recommends \
           miktex

RUN    miktexsetup finish
RUN    initexmf --admin --set-config-value=[MPM]AutoInstall=1
RUN    mpm --admin --update-db 
RUN    mpm --admin --install amsfonts --install biber-linux-x86_64 
RUN    initexmf --admin --update-fndb

WORKDIR /miktex/work

RUN mkdir /pandoc && \
    wget https://github.com/jgm/pandoc/releases/download/2.10/pandoc-2.10-1-amd64.deb && \
    mv pandoc-2.10-1-amd64.deb /pandoc && \
    apt install /pandoc/pandoc-2.10-1-amd64.deb

RUN mkdir /miktex/work/log && \
    touch /miktex/work/log/access.log && \
    touch /miktex/work/log/pandoc.log && \
    mkdir /miktex/work/slideCrafting/

RUN pip3 install wheel && \
    pip3 install pandoc-plantuml-filter pyyaml 

COPY . /miktex/work/slideCrafting/

# Patch the installed filter with my additional code
RUN rm /usr/local/lib/python3.8/dist-packages/pandoc_plantuml_filter.py && \
	cp /miktex/work/slideCrafting/dependencies/pandoc_plantuml_filter.py /usr/local/lib/python3.8/dist-packages/ && \
	rm /usr/local/lib/python3.8/dist-packages/__pycache__/pandoc_plantuml_filter.cpython-38.pyc

RUN cd /miktex/work/slideCrafting/webserver && npm install

RUN ln -s /miktex/work/slideCrafting/run.sh /miktex/work/run.sh && \
    ln -s /miktex/work/slideCrafting/watch.sh /miktex/work/watch.sh && \
    ln -s /miktex/work/slideCrafting/slideCrafting.sh /miktex/work/slideCrafting.sh && \
    chmod 0705 /miktex/work/run.sh && \
    chmod 0705 /miktex/work/slideCrafting.sh && \
    chmod 0705 /miktex/work/slideCrafting/updateTemplate.sh && \
    chmod 0705 /miktex/work/watch.sh

ENV MIKTEX_USERCONFIG=/miktex/.miktex/texmfs/config
ENV MIKTEX_USERDATA=/miktex/.miktex/texmfs/data
ENV MIKTEX_USERINSTALL=/miktex/.miktex/texmfs/install
ENV PLANTUML_BIN="java -jar /miktex/work/slideCrafting/dependencies/plantuml.1.2020.14.jar"

# RUN echo \# TEST > /tmp/test.md && \
#     pandoc /tmp/test.md \
#         --from=markdown+raw_tex+header_attributes+implicit_header_references+raw_attribute+inline_code_attributes+fancy_lists+line_blocks \
#         --highlight-style=tango --to=beamer -V 'beameroption:show notes' \
#         --pdf-engine=pdflatex -o /tmp/test_slides_notes.pdf && \
#     rm /tmp/test.md && \
#     rm /tmp/test_slides_notes.pdf

EXPOSE 8080/tcp

CMD ./slideCrafting.sh 
