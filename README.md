# Now archived
Superseeded by [PZZ-ARC](https://github.com/penguino118/PZZ-ARC)
# PZZ Pasta
A modding tool for .PZZ packages in [**GioGio's Bizarre Adventure (PS2)**](https://jojowiki.com/GioGio%27s_Bizarre_Adventure), driven by [infval's scripts for compression and decompression](https://github.com/infval/pzzcompressor_jojo). <br>
**Extract .PZZ** will extract any given .pzz file to a specified folder, and **Repack .PZZ** will repack a folder's contents based on the created filelist text file from extraction. **Extract Folder** and **Repack Folder** will perform the same operations to all found .pzz files in the input folder. As .PZZ packages contain no filenames, they are indexed based on their position within the package and are given an extension based on the detected filetype.<br>
* **.AMO** - Artistoon Model
* **.AHI** - Skeleton Data
* **.TXB** - Texture Batch
* **.AAN** - Artistoon Animation
* **.SDT** - Cast Shadow Mesh Data
* **.HIT** - Player Collision
* **.HITS** - Stage Collision
* **.TXT** - Dialouge Text Script
# Settings
## **Truncate Text Files**
Truncates null bytes on extraction for dialogue scripts for 3D (Demo Type) and 2D (Book Type) cutscenes.
## **Unpack Repack Texture Batches (.txb)**
Unpacks and repacks TIM2 (.tm2) files contained in extracted .txb files.
## **Fix Color Table Sizes**
Adjusts CLUT sizes on unpacked TIM2 textures for correct detection in miscellaneous editing programs.
