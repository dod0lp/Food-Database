FROM mysql:9.0.0

ENV MYSQL_ROOT_PASSWORD=root
ENV MYSQL_DATABASE=db_food
ENV MYSQL_USER=user
ENV MYSQL_PASSWORD=pw

COPY ./init.sql /docker-entrypoint-initdb.d/

EXPOSE 3306
