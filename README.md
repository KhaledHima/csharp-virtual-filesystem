# OS with C#

An educational operating system simulation written in C#.

This project implements a virtual disk, FAT-based file system,
and a command-line shell similar to Windows CMD.

## Features
- Virtual disk abstraction
- FAT (File Allocation Table) implementation
- Directory & file entries
- Shell commands:
  - cd, dir, copy, del, rename
  - import / export between host and virtual disk

## Architecture
- `VirtualDisk` — disk abstraction & cluster IO
- `MiniFAT` — FAT management
- `Directory / FileEntry` — file system structures
- `FileSystemShell` — CLI interface

## How to Run
1. Open `OS with C#.sln` in Visual Studio
2. Build & Run
3. Use shell commands inside the console

## Disclaimer
This project is for educational purposes only and is not a real OS.

## Author
Khaled Ibrahim
