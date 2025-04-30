@echo off
start dotnet run --urls=http://*:5101 --no-build
start dotnet run --urls=http://*:5102 --no-build
start dotnet run --urls=http://*:5103 --no-build
start dotnet run --urls=http://*:5104 --no-build
start dotnet run --urls=http://*:5105 --no-build

start http://localhost:5101
start http://localhost:5102
start http://localhost:5103
start http://localhost:5104
start http://localhost:5105