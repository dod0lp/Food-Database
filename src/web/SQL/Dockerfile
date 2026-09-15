FROM mcr.microsoft.com/mssql/server:2022-latest

USER root
ARG WORK_DIR=/usr/src/app
ENV WORK_DIR=${WORK_DIR}
RUN mkdir -p ${WORK_DIR}

COPY startup.sh ${WORK_DIR}
COPY init.sql ${WORK_DIR}
COPY ./init-scripts/ ${WORK_DIR}/init-scripts/

EXPOSE ${DATABASE_PORT}

RUN sed -i 's/\r$//' ${WORK_DIR}/startup.sh && chmod +x ${WORK_DIR}/startup.sh
CMD ["/bin/bash", "/usr/src/app/startup.sh"]