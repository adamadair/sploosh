![Sploosh Logo](assets/Sploosh_Logo_256.png)

**SPLOOSH**: _Simple Process Launcher Optimized for Organized System Handling_

A hobbyist POSIX-style shell written in C#, because writing your own shell is a rite of passage—and C# deserves a chance to party at `/bin`.

---

## 🚀 Planned Features

- 🖥️ **Command Execution** – Run external programs and built-in commands
- 🧵 **Pipelines & Redirection** – Supports `|`, `>`, `<`, and friends
- 🔁 **Control Structures** – `if`, `for`, `while`, and logical operators (`&&`, `||`)
- 🔤 **Variable & Command Expansion** – `$VAR`, `$(cmd)`, `$((math))`, etc.
- 🐚 **Script Execution** – Run shell scripts with shebang support
- 👥 **Background Job Simulation** – `jobs`, `fg`, `bg` (simulated)
- 🎯 **Tab Completion** – For files and commands (in progress)
- 📦 **Built-in Commands** – `cd`, `exit`, `echo`, and more
- 🧠 **Custom Input Loop** – Replaces `Console.ReadLine()` for full control
- ⚠️ **Signal Handling** – Intercepts Ctrl+C and exit events (limited by .NET)

---
# POSIX Shell Development To-Do List

## Core Features
- [x] Implement command parsing and execution
- [x] Support shell built-ins (`cd`, `exit`, `echo`, etc.)
- [ ] Handle background execution with `&`
- [X] Implement pipelines using `|`
- [X] Implement I/O redirection (`>`, `<`, `>>`, `2>`, `2>>`)

## Control Structures
- [ ] `if` / `else` / `elif` support
- [ ] `for` loops
- [ ] `while` / `until` loops
- [ ] `case` statements
- [ ] Logical operators (`&&`, `||`, `!`)

## Expansions
- [ ] Variable expansion (`$VAR`, `${VAR}`)
- [ ] Command substitution (`` `cmd` `` and `$(cmd)`)
- [ ] Arithmetic expansion (`$((...))`)
- [ ] Filename expansion/globbing (`*`, `?`, `[...]`)
- [ ] Quoting rules (`"`, `'`, `\`)

## Job Control & Signals
- [ ] Implement background processes (`&`)
- [ ] `fg`, `bg`, and `jobs` support
- [ ] Signal handling (e.g., `SIGINT`, `SIGCHLD`)

## Environment Management
- [ ] Variable assignment and export (`VAR=value`, `export VAR`)
- [ ] Accessing and inheriting environment variables

## Script Execution
- [ ] Read and execute commands from a script file
- [ ] Support `#!` (shebang) handling

## Optional Enhancements
- [X] Command line history
- [X] Tab completion
- [X] Command line editing (e.g., readline-like behavior)

---
## 📄 Requirements

- [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet)
- Linux (recommended) or Windows Subsystem for Linux (WSL)  
  _(Signal handling and process control are best on real POSIX systems)_

---

## 🛠️ Build & Run

```bash
git clone https://github.com/adamadair/sploosh.git
cd sploosh
dotnet build
dotnet run
```

# 🤓 Why?

Because writing a shell teaches you everything:

* Parsing
* Processes
* Signals
* I/O
* Devotional suffering
* And most importantly, how to handle user input gracefully

*And because C# can absolutely pull it off.*

# 🧼 Disclaimer

This project is for educational and recreational purposes. It’s not meant to replace your daily driver shell unless you are a masochist or giving a very cool demo.

# 📜 License

MIT License. Splash away.

# 🙏 Acknowledgements
Inspired by:

* [POSIX Shell Spec](https://pubs.opengroup.org/onlinepubs/9699919799/utilities/V3_chap02.html)

* Other hobby shells like osh, nushell, and the mighty bash