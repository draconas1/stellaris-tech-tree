# Stellaris Tech Tree Parser
A Tech Tree Parser / Displayer for Stellaris

Uses https://github.com/tboby/cwtools/tree/master/CWTools to parse a directory tree of stellaris folders to get all the research options and convert them and their dependencies to a json format that can be parsed by a graphing library.  

Currently using http://visjs.org for graphing.

Inital version has:
* Display all Technologies
* Filter graph by Tech Area
* Search
* Tech images
* Dependency highlight
* Rare/Starter/Dangerous/Acquisition tech highlight

Live version at http://www.draconas.co.uk/stellarisTech

The live version uses the Stellaris interface icon gfx which are copyright to Paradox Interactive, these are not part of the git repository and to use them you will need your own copy of Stellaris and to run the parser.

Note forking the Repo: I strongly recommend forking off a release, I am a one man band who works from several machines so the master branch is frequently broken as I use it to transfer or store code after a session.  

Many thanks to https://github.com/tboby for all his help with using CWTools and parsing the PDS files.
