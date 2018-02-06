# Geonorge.NedlastingKlient

This project provides a client software for downloading dataset's published through Geonorge's Atom Feed. The general idea is to provided a tool to synchronize dataset on a regular basis. 

It includes a desktop application for browsing and selecting files you want to download. A console application is provided to perform downloads. This console application can be scheduled to run through Scheduled tasks on windows, or cron on *nix platforms.

## How to setup development environment

Project depends on:
* .net core 2.0 SDK
* .net framework 4.7.1 Developer pack

Packages can be downloaded from here:
https://www.microsoft.com/net/download/windows

Solution builds with Visual Studio 2017. 

## Project structure

### NedlastingKlient
Common class library for parsing atom feeds and downloading files.

Compilation targets both netstandard 2.0 (.net core) and .net framework 4.7.1

### NedlastingKlient.Gui

Graphical user interface for browsing and selecting files for download

Compilation target: .net framework 4.7.1

### NedlastingKlient.Konsoll
Console application for downloading selected files

Compilation target: netstandard 2.0 (.net core)
