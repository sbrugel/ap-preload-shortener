# ap-preload-shortener
This project is licensed under the [GNU General Public License v3.0](https://www.gnu.org/licenses/gpl-3.0.txt). This basically means you can redistribute this product with modification, for patent or private use, as long as you accredit me and keep the license on your derivation of the project. You may NOT distribute the code as closed-source. Please follow the aforementioned link for more information.

## Installation
Those who are familiar with GitHub, I'll assume you know what you're doing.

Those who aren't, go to the rgiht side of the page and you'll see the Releases section. Click there, find the latest version, and click on said version. Download PreloadShortener.exe

## About
Intended to be supplemental to Train Simulator 20XX (on Steam), this program simplies the names of all Armstrong Powerhouse preload/quick drive consist names. For those who have used these consists in scenario editing before, you'd probably know how long these names are, meaning it takes lots of time and patience to find the correct consist for your needs.

Below is an example of a preload name before and after modification through running this program.
![example](https://cdn.discordapp.com/attachments/735213126567329905/805883635315507201/unknown.png)

## Note
You must ensure the railworks directory is entered in exactly this format - E:\steam\steamapps\common\RailWorks - NO SLASHES AT THE END. 

## Features
- One click and the preloads will be shortened. ~~The program detects your TS20xx install location through a registry key~~, as well as all associated AP products installed. (This directory can always be changed if needed) (registry key detection temporarily removed for the timebeing due to bugs on certain systems, hopefully back soon! AP products installed are still auto-detected however!)
- Futureproofing for future AP addons. This program will still programatically shorten preload names of future AP products, ***without the need for a software update!*** As long as the AP naming convention remains the same (and no weird TOCs have been introduced in future packs), the program will still shorten names fine. In the case that new TOCs/name formatting changes are introduced, this program will receive an update, but for many packs introduced post-development no major problems have been noted of.

## Known Issues
- There is still a bit of redundancy in the preload name (e.g. C158 - C159 in the above). This can't be simplified further as they are two separate elements in the .bin file. If one is removed, the preload will break and won't be visible in the editor.
- I should also probably make adding elements to the arrays a bit more efficient.

## Closing Remarks
- Thank you to Jake Horton for the PowerShell script that this program is based off, and Ben Penhalagan for testing.
