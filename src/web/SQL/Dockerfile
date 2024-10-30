FROM mcr.microsoft.com/mssql/server:2019-latest

USER root
ARG WORK_DIR=/usr/src/app
ENV WORK_DIR=${WORK_DIR}
RUN mkdir -p ${WORK_DIR}

COPY startup.sh ${WORK_DIR}
COPY init.sql ${WORK_DIR}
COPY ./init-scripts/ ${WORK_DIR}/init-scripts/

EXPOSE ${DATABASE_PORT}

RUN chmod +x ${WORK_DIR}/startup.sh
CMD sh -c "${WORK_DIR}/startup.sh"