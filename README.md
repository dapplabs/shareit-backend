# shareit-backend
<p>This is the shareit`s backend/node. Anyone can run a node and upload files to it. Files are accesible from any node in the network throught it`s hash</p>

To run it, you need to have docker installed.

- Clone the repository.
- Configure the docker-compose.yml (select your own media folders and ports).
- Run $docker-compose up -d

To get this functional you need to run it over https, so you need to do some reverse-proxy stuff. I use nginx as reverse proxy.
