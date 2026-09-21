FROM mcr.microsoft.com/mssql/server:2025-latest

USER root
ARG WORK_DIR=/usr/src/app
ARG DATABASE_PORT=1433
ENV WORK_DIR=${WORK_DIR}
ENV DATABASE_PORT=${DATABASE_PORT}
RUN mkdir -p ${WORK_DIR}

COPY startup.sh ${WORK_DIR}
COPY init.sql ${WORK_DIR}
COPY ./init-scripts/ ${WORK_DIR}/init-scripts/

RUN sed -i 's/\r$//' ${WORK_DIR}/startup.sh \
    && chmod +x ${WORK_DIR}/startup.sh \
    && chown -R mssql:mssql ${WORK_DIR}

USER mssql
CMD ["/bin/bash", "/usr/src/app/startup.sh"]