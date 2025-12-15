FROM ubuntu:latest

ARG GROUP_USER=nooterdooter
ARG GUI_UID=1006
ARG HOME_DIR=/home/${GROUP_USER}
ARG APPS="expect curl wget git nano"

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

ENV PATH="${HOME_DIR}/.dotnet/tools:${PATH}"

RUN mkdir ${HOME_DIR}
COPY steamcmd_install.exp ${HOME_DIR}/steamcmd_install.exp
COPY steamcmd_install.sh ${HOME_DIR}/steamcmd_install.sh
COPY startup.sh ${HOME_DIR}/startup.sh
COPY repo_install.steam_script ${HOME_DIR}/repo_install.steam_script
RUN chmod a+x ${HOME_DIR}/steamcmd_install.sh ${HOME_DIR}/steamcmd_install.exp ${HOME_DIR}/startup.sh

RUN <<STEAM_CMD_INSTALL
    dpkg --add-architecture i386
    apt-get update
    expect ${HOME_DIR}/steamcmd_install.exp
STEAM_CMD_INSTALL

RUN mkdir -p -m 0700 /root/.ssh && ssh-keyscan -t rsa github.com >> /root/.ssh/known_hosts
RUN --mount=type=ssh git clone git@github.com:NolanEastburn/REPO_Cerveza_Cristal.git ${HOME_DIR}/REPO_Cerveza_Cristal

ENV PATH="/usr/games:${PATH}"

RUN groupadd -g ${GUI_UID} ${GROUP_USER}
RUN useradd -s /bin/bash -g ${GROUP_USER} -u ${GUI_UID} ${GROUP_USER}
RUN chgrp ${GROUP_USER} ${HOME_DIR}/ -R
RUN chown ${GROUP_USER} ${HOME_DIR}/ -R
WORKDIR ${HOME_DIR}/
USER ${GROUP_USER}
RUN dotnet tool install ilspycmd --global
ENTRYPOINT ["/home/nooterdooter/startup.sh"]