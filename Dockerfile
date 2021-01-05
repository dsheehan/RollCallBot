#docker run -v /home/docker/rollcallbot/app:/app mcr.microsoft.com/dotnet/runtime:5.0 dotnet /app/RollCallBot.dll
FROM mcr.microsoft.com/dotnet/runtime:5.0
WORKDIR /app

#copy dependencies first, to try and cache layer upload
COPY "src/bin/Release/net5.0/publish/[^RollCallBot]*" .

#copy app
COPY "src/bin/Release/net5.0/publish/RollCallBot*" .

ENTRYPOINT ["dotnet", "/app/RollCallBot.dll"]