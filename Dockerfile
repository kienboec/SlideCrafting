FROM ubuntu:bionic

# Based on: https://github.com/MiKTeX/docker-miktex/blob/master/Dockerfile

RUN    apt-get update \
    && apt-get install -y --no-install-recommends \
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
            inotify-tools

# MikTex

RUN apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys D6BC243565B2087BC3F897C9277A7293F59E4889
RUN echo "deb http://miktex.org/download/ubuntu bionic universe" | tee /etc/apt/sources.list.d/miktex.list

RUN    apt-get update \
    && apt-get install -y --no-install-recommends \
           miktex

RUN    miktexsetup finish \
    && initexmf --admin --set-config-value=[MPM]AutoInstall=1 \
    && mpm --admin --update-db \
    && mpm --admin \
           --install amsfonts \
           --install biber-linux-x86_64 \
    && initexmf --admin --update-fndb

WORKDIR /miktex/work

RUN mkdir /pandoc && \
    wget https://github.com/jgm/pandoc/releases/download/2.10/pandoc-2.10-1-amd64.deb && \
    mv pandoc-2.10-1-amd64.deb /pandoc && \
    apt install /pandoc/pandoc-2.10-1-amd64.deb

RUN pip3 install wheel && \
    pip3 install pandoc-plantuml-filter pyyaml

RUN mkdir /miktex/work/log && \
    touch /miktex/work/log/pandoc.log && \
    mkdir /miktex/work/slideCrafting/

ADD * /miktex/work/slideCrafting/

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

EXPOSE 80/tcp

CMD ./slideCrafting.sh 