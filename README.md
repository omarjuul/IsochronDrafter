Isochron Drafter
================

A hacked-together way to draft custom Magic: the Gathering sets online.
Forked from the [original](https://github.com/thquinn/IsochronDrafter) by thquinn, adapted to draft cubes.

## What did I (omarjuul) add/change?
* [scryfall](https://scryfall.com/) image lookup, eliminating the need to host draft images yourself
* make packs "consume" cards from the cube instead of viewing it as a set (no duplicated unless there are multiple copies in the cube)

## Features
* Draft custom Magic ~~sets~~ cubes online with your friends!
* Graphical deck builder allows you to build your deck as you draft!
* That's it. [Screenshot.](http://i.imgur.com/ssYb7TB.jpg)

## Requirements
Windows and .NET 4.0, until someone wants to do a Mono port.

## What's Broken?
Well, a lot of stuff. For starters:
* Slow image downloading
* Little to no error handling when parsing set files, loading images, etc.
* Graphical glitches galore

## Coming Eventually
* More error handling
* Better image downloading

## How to Use
* In Isochron Drafter, start a server. Browse to the set file (a csv with format `<#copies>|<cardname>|<?land>`).
* If you're behind a router, [make sure](http://www.canyouseeme.org/) port 10024 is [forwarded to your computer](http://www.wikihow.com/Set-Up-Port-Forwarding-on-a-Router).
* Have everyone connect to the IP you're hosting the server on.
* Start the draft on the server.
* Enjoy.

## Notes
* Isochron Drafter was written by Tom Quinn, and is available under a [Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License](http://creativecommons.org/licenses/by-nc-sa/4.0/). It's a real hack job and not at all representative of my overall coding style or quality.
* It uses Craig Baird's [C# TCP Server](http://www.codeproject.com/Articles/488668/Csharp-TCP-Server), skatamatic's [Event-Driven TCP Client](https://www.daniweb.com/software-development/csharp/code/422291/user-friendly-asynchronous-event-driven-tcp-client), and Chris Pietschmann's [FlashWindow](http://pietschsoft.com/post/2009/01/26/CSharp-Flash-Window-in-Taskbar-via-Win32-FlashWindowEx).
