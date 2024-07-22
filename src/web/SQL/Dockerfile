FROM mcr.microsoft.com/mssql/server:2019-latest

ENV SA_PASSWORD=${SA_PASSWORD}
ENV ACCEPT_EULA=${ACCEPT_EULA}

RUN mkdir -p /usr/src/app

COPY init.sql /usr/src/app/

ENTRYPOINT /bin/bash -c " \
    /opt/mssql/bin/sqlservr & \
    sleep 20s && \
    /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $SA_PASSWORD -i /usr/src/app/init.sql && \
    wait"
