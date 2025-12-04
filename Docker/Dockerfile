FROM ubuntu:latest

ARG HOME_DIR=/home/nooterdooter
ARG APPS="expect curl wget git"

RUN apt-get update && apt-get upgrade -y
RUN apt-get install ${APPS} -y

RUN <<DOTNET_INSTALL 
wget https://packages.microsoft.com/config/debian/13/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb
apt-get update
apt-get install dotnet-sdk-8.0 -y
apt-get install dotnet-sdk-9.0 -y
DOTNET_INSTALL

ENV PATH="/home/nooterdooter/.dotnet/tools:${PATH}"

RUN mkdir ${HOME_DIR}
COPY steamcmd_install.exp ${HOME_DIR}/steamcmd_install.exp
COPY steamcmd_install.sh ${HOME_DIR}/steamcmd_install.sh
RUN chmod a+x ${HOME_DIR}/steamcmd_install.sh ${HOME_DIR}/steamcmd_install.exp
RUN <<STEAM_CMD_INSTALL
    dpkg --add-architecture i386
    apt-get update
    expect ${HOME_DIR}/steamcmd_install.exp
STEAM_CMD_INSTALL
ENV PATH="/usr/games:${PATH}"
RUN groupadd nooterdooter
RUN useradd -s /bin/bash -g nooterdooter nooterdooter
RUN chgrp nooterdooter ${HOME_DIR}/ -R
RUN chown nooterdooter ${HOME_DIR}/ -R
WORKDIR ${HOME_DIR}/
USER nooterdooter
RUN dotnet tool install ilspycmd --global
ENTRYPOINT ["/bin/bash"]