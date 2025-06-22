# SEUnp4ker
Giving the ability to tear apart Star Engine games!

People seemingly love to pick Star Engine games, like Star Citizen, apart! To find all the little details and changes and possibly more. So... Why not make this easy for the masses?
## Usage
### Command-Line Arguments
```
-h   or --help:      Print out the manual.
-i   or -input:      The input file path.
-o   or -output:     The output directory path.
-f   or --filter:    Allows you to filter in the files you want.
-d   or --details:   Enabled detailed logging including errors.
-ow  or --overwrite: Overwrites files that already exist.
-y   or --yes:       Don't ask for input, just continue.
```

## Contributions
Everyone is welcome to make contributions! Just leave a PR as normal.

SEUnp4ker is developed on macOS (always latest or dev beta's for major updates) using IntelliJ Rider. But VSCodium (recommended over VSCode) can also work with this.
### Building
#### All Platforms
You will require the latest version of the .NET 9 SDK for your platform.
#### Windows
1. Install [Chocolatey](https://chocolatey.org)
2. Install the dependencies through chocolatey by running [zstd/setup_deps.cmd](https://github.com/Stryxus/SEUnp4ker/blob/main/zstd/setup_deps.cmd).
3. Run [zstd/build_windows_x64.cmd](https://github.com/Stryxus/SEUnp4ker/blob/main/zstd/build_windows_x64.cmd) or [zstd/build_windows_arm64.cmd](https://github.com/Stryxus/SEUnp4ker/blob/main/zstd/build_windows_arm64.cmd) to build the zstd library.
4. Build SEUnp4ker!
#### Linux
1. Install the dependencies by running [zstd/setup_deps.cmd](https://github.com/Stryxus/SEUnp4ker/blob/main/zstd/setup_deps.cmd).
2. Run [zstd/build_linux_x64.cmd](https://github.com/Stryxus/SEUnp4ker/blob/main/zstd/build_linux_x64.cmd) or [zstd/build_linux_arm64.cmd](https://github.com/Stryxus/SEUnp4ker/blob/main/zstd/build_linux_arm64.cmd) to build the zstd library.
3. Build SEUnp4ker!
#### macOS
1. Install [Homebrew](https://brew.sh)
2. Install the dependencies through Homebrew by running [zstd/setup_deps.cmd](https://github.com/Stryxus/SEUnp4ker/blob/main/zstd/setup_deps.cmd).
3. Run [zstd/build_macOS_arm64.cmd](https://github.com/Stryxus/SEUnp4ker/blob/main/zstd/build_macOS_arm64.cmd) to build the zstd library.
4. Build SEUnp4ker!
## Goals (Currently)
- Add in a completely re-written [unforger](https://github.com/Stryxus/unp4k/tree/feature/rewrite/libs/unforge) system.
  - The ability to convert the CryXML (may have changed since) to JSON.
- Include and re-write the relevant pieces of [SharpZipLib](https://github.com/Stryxus/SharpZipLib) into SEUnp4ker itself.
- Re-write the entire app in the latest [Rust lang](https://www.rust-lang.org/).
  - Add an API and therefore make the unp4ker, unforger and other libraries available on [crates.io](https://crates.io/).
  - WebAssembly 64-bit compatibility (Will be added to https://stryxus.xyz/ with a GUI).
  - Ability to extract through memory streaming instead of writing to storage.

## FAQ
### Q: Why do we need this?
Simple. Many like to take games apart and in some cases, put them back together to see how they work, get all the little details out of them and much more. Star Citizen leaks often post differences down to file names and technical details when the game updates.
