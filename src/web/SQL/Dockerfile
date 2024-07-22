FROM mcr.microsoft.com/mssql/server:2019-latest

USER root

ARG WORK_DIR=/usr/src/app

RUN mkdir -p ${WORK_DIR}

COPY init.sql ${WORK_DIR}

COPY startup.sh ${WORK_DIR}
RUN chmod +x ${WORK_DIR}/startup.sh

EXPOSE 1433


CMD ["/usr/src/app/startup.sh"]