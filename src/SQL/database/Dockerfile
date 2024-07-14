FROM mysql:9.0.0

ENV MYSQL_ROOT_PASSWORD=root
ENV MYSQL_DATABASE=mydatabase
ENV MYSQL_USER=myuser
ENV MYSQL_PASSWORD=mypassword

COPY ./init.sql /docker-entrypoint-initdb.d/

EXPOSE 3306
