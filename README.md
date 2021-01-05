# RollCallBot [![.NET Build Status](https://github.com/dsheehan/RollCallBot/workflows/.NET/badge.svg)](https://github.com/dsheehan/RollCallBot/actions?query=workflow%3A.NET)
A basic discord bot used to manage the roll call for an Among Us discord server.
Responds to "Roll Call" command with an embed to track yes/no/maybe responses.

Requires setting an environment variable `RollCallBotToken` with a discord bot token.

## Example docker run
``` sh
docker run -e RollCallBotToken='discord bot token goes here' docker.pkg.github.com/dsheehan/rollcallbot/rollcallbot:latest
```

## Example docker-compose.yml
```yaml
version: '3'
services:
  rollcallbot:
    image: docker.pkg.github.com/dsheehan/rollcallbot/rollcallbot:latest
    container_name: rollcallbot
    environment:
      RollCallBotToken: 'discord bot token goes here'
    restart: unless-stopped
```
