#!/usr/bin/env bash
dotnet ef migrations add "$1" --output-dir Migrations