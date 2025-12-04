FROM ubuntu:latest

ARG HOME_DIR=/home/nooterdooter

RUN apt-get update && apt-get upgrade -y
RUN apt-get install expect -y
RUN apt-get install curl -y
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
ENTRYPOINT ["/bin/bash"]