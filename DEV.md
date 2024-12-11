## Requirements

- Visual Studio 2022 (with the `.NET desktop development` workload)



## Building

The projects inside the solution are configured to execute the `post_build.ps1`
script after each successful build.
The `post_build.ps1` script installs the newly builded artifacts by copying
the `Indicators++.dll` and `Objects++.dll` files into their respective folders.
Every time those files are changed you need to restart [Tiger.com (aka
TigerTrade)][1].

### Debug

```ps1
Scripts\build.ps1
```

*or*

```ps1
MSBuild
```

### Release

```ps1
Scripts\build_release.ps1
```

*or*

```ps1
MSBuild /p:Configuration=Release
```

### Auto

> :warning: [Tiger.com (aka TigerTrade)][1] will be closed the first time you
> execute the script.

```ps1
Scripts\auto_builder.ps1
```

Keep the script running in a terminal and every time you want to build just
close [Tiger.com (aka TigerTrade)][1] and the script will build the solution
in debug mode, install the newly builded plugins and open [Tiger.com (aka
TigerTrade)][1] for you.

> Ideally, hot reload would be better, but currently, it is not supported.



## Overview

The solution contains 3 projects: Indicators, Objects, Common.

- The `Indicators` project contains all the indicators and it compiles to
  `Indicators++.dll`.
- The `Objects` project contains all the objects and it compiles to
  `Objects++.dll`.
- The `Common` project contains all the code shared between indicators and
  objects and for simplicity it's directly compiled into both
  `Indicators++.dll` and `Objects++.dll`, to avoid having a separate dll.

```
TTPlugins\
├─ Common\                 Shared library for the plugins
│  ├─ Dev\                 Development utilities
│  ├─ Helpers\             Basic helper classes
│  └─ UI\                  UI utilities
├─ Indicators\             Indicator plugins
├─ Objects\                Object plugins
├─ Scripts\                Utility scripts
├─ Directory.Build.props   Global project settings
└─ TTPlugins.sln         Solution
```

> [!WARNING]
> The development utilities contain some hard coded settings like the fonts
> used when rendering text. To make sure that text is properly rendered you
> need to install the hard-coded fonts if they are missing from your system
> (`Inter`, `JetBrainsMono Nerd Font`), or you need to change those hard coded
> values according to your own preferences.



## Editors

<details>
<summary>Visual Studio 2022</summary>

### Visual Studio 2022

Extensions:
- [CSharpier](https://marketplace.visualstudio.com/items?itemName=csharpier.CSharpier)
</details>

<details>
<summary>Visual Studio Code</summary>

### Visual Studio Code

Extensions:
- [C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit)
- [CSharpier](https://marketplace.visualstudio.com/items?itemName=csharpier.csharpier-vscode)
</details>

<details>
<summary>Vim/Neovim</summary>

### Vim/Neovim

> Assuming you use [mason.nvim](https://github.com/williamboman/mason.nvim) and
> [conform.nvim](https://github.com/stevearc/conform.nvim)

Extensions:
- [OmniSharp](https://github.com/mason-org/mason-registry/blob/main/packages/omnisharp/package.yaml)
- [CSharpier](https://github.com/mason-org/mason-registry/blob/main/packages/csharpier/package.yaml)

<details>
<summary>How to configure the formatter?</summary>

```lua
{
    'stevearc/conform.nvim',
    event = 'VeryLazy',
    keys = {
        {
            '<leader><leader>',
            function()
                require('conform').format {
                    timeout_ms = 3000,
                    lsp_fallback = true
                }
            end,
            mode = '',
        },
    },
    opts = {
        notify_on_error = true,
        formatters_by_ft = {
            cs = { 'csharpier' },
        },
    },
}
```
</details>
</details>



## License

TTPlugins is released under the [AGPL-3.0 license](LICENSE.md).



[1]: https://www.tiger.com/terminal
