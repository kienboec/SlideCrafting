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

RUN    apt-get update \
    && apt-get install -y --no-install-recommends \
           miktex

RUN    miktexsetup finish
RUN    initexmf --admin --set-config-value=[MPM]AutoInstall=1
RUN    mpm --admin --update-db 
RUN    mpm --admin --install amsfonts --install biber-linux-x86_64 
RUN    initexmf --admin --update-fndb

# Pandoc
WORKDIR /miktex/work

RUN mkdir /pandoc && \
    wget https://github.com/jgm/pandoc/releases/download/2.10/pandoc-2.10-1-amd64.deb && \
    mv pandoc-2.10-1-amd64.deb /pandoc && \
    apt install /pandoc/pandoc-2.10-1-amd64.deb

# install .net5
RUN wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb && \
	dpkg -i packages-microsoft-prod.deb && \
	apt-get update; \
	apt-get install -y dotnet-sdk-5.0 dotnet-runtime-5.0

RUN mkdir /miktex/work/log && \
    touch /miktex/work/log/access.log && \
    touch /miktex/work/log/pandoc.log && \
    mkdir /miktex/work/slideCrafting/

RUN pip3 install wheel && \
    pip3 install pandoc-plantuml-filter pyyaml 

# set environment variables
ENV MIKTEX_USERCONFIG=/miktex/.miktex/texmfs/config
ENV MIKTEX_USERDATA=/miktex/.miktex/texmfs/data
ENV MIKTEX_USERINSTALL=/miktex/.miktex/texmfs/install
ENV PLANTUML_BIN="java -jar /miktex/work/slideCrafting/dependencies/plantuml.1.2020.14.jar"

RUN    mpm --update
RUN    initexmf --register-root=/miktex/work/src/_templates || true
RUN    mpm --admin --update-db 
RUN    initexmf --admin --update-fndb
RUN    mpm --update 

# Copy files from local directory
COPY . /miktex/work/slideCrafting/

# Patch the installed filter with my additional code
RUN rm /usr/local/lib/python3.8/dist-packages/pandoc_plantuml_filter.py && \
	cp /miktex/work/slideCrafting/dependencies/pandoc_plantuml_filter.py /usr/local/lib/python3.8/dist-packages/ && \
	rm /usr/local/lib/python3.8/dist-packages/__pycache__/pandoc_plantuml_filter.cpython-38.pyc

# expose webserver port
EXPOSE 8080/tcp

# run slidecrafting
RUN echo cd slideCrafting && echo dotnet build && cd /miktex/work/slideCrafting/SlideCrafting/bin/Debug/net5.0
CMD cd /miktex/work/slideCrafting/SlideCrafting/bin/Debug/net5.0 && dotnet SlideCrafting.dll --environment "Production"
