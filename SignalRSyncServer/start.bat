@echo off
start dotnet run --urls=http://*:5100 --no-build
start http://localhost:5100