FROM mcr.microsoft.com/mssql/server:2019-latest

USER root
ARG WORK_DIR=/usr/src/app
RUN mkdir -p ${WORK_DIR}

CMD echo ${WORK_DIR}
COPY init.sql ${WORK_DIR}

EXPOSE 1433

COPY startup.sh ${WORK_DIR}
RUN chmod +x ${WORK_DIR}/startup.sh
CMD ["sh", "-c", "/usr/src/app/startup.sh"]