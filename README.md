# idbfs-tool

Dump idbfs data from IndexedDB to a JSON file, then decode it into files. Created for debugging and working with [CommonCore](https://github.com/XCVG/commoncore/) WebGL builds but could be used to back up game saves or other things

# Usage

Drop the contents of dumpidbfs.js into the browser console, then run the dumpIdbfs function that now exists in global scope. For Unity games, `dumpIdbfs()` should work. For Godot, it might be something like `dumpIdbfs("/userfs")`. You should be prompted to download a JSON file if all goes well.

Drop that JSON file beside a built version of idbfs-decoder and call it with the filename as an argument, something like `idbfs-decoder idbfs.json`. You should end up with a folder full of the files originally from idbfs. Dumps done with the `DumpIdbfs` console command in CommonCore (available from 6.x onward) should work as well.

# License

This project is licensed under the MIT License. Refer to the LICENSE file for details.

To be absolutely clear, this is pretty much an internal tool made public and is provided with ABSOLUTELY NO WARRANTY.