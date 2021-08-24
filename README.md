![SOCIALLY DISTANT](/assets/LogoText.png)
*Logo designed by [@byrahilu](https://rahilu.net/)*

Socially Distant is an open-source semi-realistic hacking game with a story based around the global COVID-19 pandemic. Investigate and discover the truth behind the rise of a deadly global pandemic, the spread of a vicious ransomware throughout the healthcare industry, and the fall of democracy.

This game is based on and is a complete demonstration of [our custom-built .NET 5 OpenGL game engine](https://github.com/thundershock-alliance/thundershock).

### Build Instructions

A graphical IDE is highly recommended over using the command-line.

First, recursively clone the repository and all of its submodules.

```bash
git clone --recurse-submodules https://github.com/thundershock-alliance/socially-distant
```

Next, ensure that the .NET 5 SDK (and an IDE, we recommend [Rider](https://www.jetbrains.com/rider/)) is installed. Upon doing so, navigate to `src` and open the solution file.

```bash
cd socially-distant/src
rider ./SociallyDistant.sln
```

The game will run as if it has been installed, but you've just built it yourself.

### Accessing Content Editor

Content Editor can be used to create custom story packs for the game. It can be accessed in both installed copies and compiled builds of Socially Distant with the `editorr` command-line argument.

```bash
SociallyDistant editor
```

A custom run task can be created to run the game with this command-line argument. See your IDE's documentation for information on how to do this.

### Developer Console

Development builds of Socially Distant have a developer console that can be used to invoke cheats and debug commands. This is compiled out of released versions of the game, however it can be very useful. To access it, simply press the Tilde key (<code>\`</code>). Type `CheatManager.Help` to see a list of available console commands.

![Dev console](/assets/console.png)

### Credit where credit is due

 - Big thank-you to the [Silk.NET](https://github.com/dotnet/Silk.NET) and [MonoGame](https://github.com/MonoGame) guys for helping give advice and code optimizations for the engine. (This game started out being written in MonoGame)
 - [@byrahilu](https://rahilu.net/): for designing the game's logo and icon.
 - Logan Lowe: for contributing other graphical assets to the game

### FAQ

Below are some questions I've either been asked or expect to sometine be asked about this game and my plans with it. These are my answers.

**Q:** What platforms will be supported?

Thundershock has so far only been tested on Windows by myself and Linux by a few other people. Native Windows support is fully tested and so you can add that to the list of platforms I'm developing for. Linux is known to work somewhat but the engine is slightly buggy in full-screen mode. However, Linux is one of my main daily drivers, so I plan to fix those bugs soon.

I don't have a Mac, I can't afford a Mac, and I'm not going to say a platform is supported unless I can personally vet the quality of my code on it. I have no way to test the game myself on macOS and so I can't vet it. I'm not officially supporting macOS. You are welcome to try the game on it yourself, since .NET **is cross-platform**, but your mileage may vary.

**Q:** How resource-intensive is the game?

Short answer: I don't really know. I develop the game on a Ryzen 7 3700x, in a Windows 11 KVM virtual machine, with an RTX 2060 GPU and 24GB of RAM passed to it. It runs reasonably. I also test the game on my laptop, with a Ryzen 7 4800HS and a 2060 Max-Q and 16GB of RAM, it runs reasonably on there as well.

I don't have any low-end hardware to test the game on, so I'll need to optimize the engine as issues prop up. Again, your mileage may vary. I would love for the game to theoretically get 1080p 60 FPS on low-end hardware, but I don't know for sure if that's the case yet.

**Q:** Will there be multiplayer?

The idea of a multiplayer hacking game with social elements is really cool to me. But online servers are expensive and difficult to maintain, even for large companies, and I'm just a college student. In the future, maybe, but that's NOT a promise.

**Q:** Do you make money off this?

No.

The base game will always be free to compile and free to download. The normal career story (with the exception of the tutorial), however, will not be free. It costs me money to develop this game, so I think that's fair. I'm not going to force you to buy the game to experience it, only do so if you like what you see and you want to support me.

**Q:** How will you handle Career Mode piracy?

The better question is, why would you pirate a simple PAK file for a game that's already mostly freely available? If you're asking that question then I'm confused why you're reading this. Just... don't come to me if things break. Old career paks are bound to break as the engine's PAK format changes or I change the way the script host and conteny loader routines work.

**Q:** Can I customize my shell?

Yeah! Being the lead dev of ShiftOS for a while, I'm  really into customizable graphical shells. I want to see what the community creates in  that regard.

 - You can change your hostname at any time in Terminal: `echo newhostname > /etc/hostname`
 - Custom Terminal palettes can be used to change the colors of text. Documentation coming soon.
 - Decorator themes may be used to customize the look of window borders, just like in ShiftOS.
 - Custom wallpapers can be imported into the game. Custom stories can provide additional wallpapers as well.
 - With the ShiftOS 1.0 preservation effort going on in parallel, I plan to add skin load routines for ShiftOS 0.0.7, 0.0.8, Next-BWM, and 1.0 skin files. Socially Distant will convert them into its own theme format.
 - I don't have plans to add an in-game Shifter equivalent, but if there's demand for a graphical theme editor, I'll  add one.
