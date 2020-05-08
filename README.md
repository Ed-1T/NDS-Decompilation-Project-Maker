# NDS-Decompilation-Project-Maker
A tool to create an XML decompilation project from a Nintendo DS ROM

# Usage
- Select an input ROM
- Select an output path (a new folder is preferred as, depending on the ROM and the settings, a lot of files could be created)
- Select a symbols.x file (optional, each symbol must be defined like this: "[symbol name] = 0x[hex address];")
- Click **Generate**

# Settings
- **Auto name sections**: Tries to name ARM9 sections based on the start address
- **Define symbols as functions**: Defines symbols from the symbols file as both symbols and functions
- **Fill bss sections**: By default the bss sections don't have any files linked to them, enabling this option will create them
- **Default value**: The value that fills the bss section (only available if **Fill bss sections** is enabled)
