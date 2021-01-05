#docker run -v /home/docker/rollcallbot/app:/app mcr.microsoft.com/dotnet/runtime:5.0 dotnet /app/RollCallBot.dll
FROM mcr.microsoft.com/dotnet/runtime:5.0
WORKDIR /app
COPY app .
ENTRYPOINT ["dotnet", "/app/RollCallBot.dll"]
