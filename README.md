# Stellaris Tech Tree Parser & CWTools Helpers
A Tech Tree Parser / Displayer for Stellaris

Uses https://github.com/tboby/cwtools/tree/master/CWTools to parse a directory tree of stellaris folders to get all the research options and convert them and their dependencies to a json format that can be parsed by a graphing library.  

Currently using http://visjs.org for graphing.

## Features
* Display all Technologies
* Filter graph by Tech Area
* Category highlight / selection / filtering
* Search
* Tech images
* Dependency highlight
* Rare/Starter/Dangerous/Acquisition tech highlight
* Techs from mods
* Buildings that are unlocked by techs.

Live version at http://www.draconas.co.uk/stellaristech

The live version uses the Stellaris interface icon gfx which are copyright to Paradox Interactive (with permission), these are not part of the git repository and to use them you will need your own copy of Stellaris and to run the parser.

## CWTools Helpers
The Library Project CWToolsHelpers contains a variety of helper classes for working with PDX files parsed by CWTools in C#.  A well commented example is in [ExampleUse.cs](CWToolsHelpers/ExampleUse.cs) 

## In Progress
- [ ] Unlocked buildings / ships / other stuff - either as additional nodes with clustering or a breakout status window.  Currently buildings are done and I am working on others.  
- [ ] Add "My Mod"  support to generate trees for mods - works and processes them and the main tree shows techs from some of the mods I use.

Note forking the Repo: I strongly recommend forking off a release, I am a one man band who works from several machines so the master branch is frequently broken as I use it to transfer or store code after a session.  

Many thanks to https://github.com/tboby for all his help with using CWTools and parsing the PDX files.
