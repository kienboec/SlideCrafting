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
            npm 
            
            # Mermaid, Pupeteer
            #gconf-service libasound2 libatk1.0-0 libc6 libcairo2 libcups2 libdbus-1-3 libexpat1 libfontconfig1 libgcc1 libgconf-2-4 libgdk-pixbuf2.0-0 libglib2.0-0 \
            #libgtk-3-0 libnspr4 libpango-1.0-0 libpangocairo-1.0-0 libstdc++6 libx11-6 libx11-xcb1 libxcb1 libxcomposite1 libxcursor1 libxdamage1 libxext6 libxfixes3 \
            #libxi6 libxrandr2 libxrender1 libxss1 libxtst6 \
            #fonts-liberation libappindicator1 libnss3 lsb-release xdg-utils 

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
    # && \ #pip3 install pandoc-mermaid-filter

#ARG PUPPETEER_PRODUCT=chrome
#RUN wget --quiet https://dl.google.com/linux/direct/google-chrome-stable_current_amd64.deb && \
#    apt install ./google-chrome-stable_current_amd64.deb --yes
# RUN npm install -g @mermaid-js/mermaid-cli mermaid-filter || exit 0
#RUN npm init -y && npm install @mermaid-js/mermaid-cli || exit 0

COPY . /miktex/work/slideCrafting/

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
#ENV MERMAID_BIN="/usr/local/bin/mmdc"

EXPOSE 8080/tcp

CMD ./slideCrafting.sh 