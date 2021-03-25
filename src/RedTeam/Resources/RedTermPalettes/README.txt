REDTERM COLOR PALETTES

Much like ShiftOS, RED TEAM OS allows you to customize the look and feel of your graphical environment and terminal. The terminal's appearance is customized using color palettes. Redterm palettes are simple JSON files that the game uses for the various elements of the terminal.

You can create your own redterm palette using any text editor by basing it off of either of the three that come with the game (default.json, light.json or highContrast.json). You CANNOT edit these files, the game will overwrite your changes. They're just here as examples. You can also download other players' redterm palettes from the Alkaline Thunder community forum:

https://community.mvanoverbeek.me/

===================================================================================

Example redterm palette:

{
  "name": "Default",
  "description": "Default dark color scheme of redterm",
  "author": "Michael VanOverbeek",
  "cursor": {
    "bg": "#202020",
    "fg": "white"
  },
  "completions": {
    "bg": "#222222",
    "border": "#242424",
    "text": "#999",
    "textHighlight": "#fff",
    "highlight": "#1baaf7"
  },
  "colors": {
    "black": "#202020",
    "darkBlue": "#000080",
    "darkGreen": "#008000",
    "darkCyan": "#008080",
    "darkRed": "#800000",
    "darkMagenta": "#800080",
    "darkYellow": "#808000",
    "gray": "#999",
    "darkGray": "#808080",
    "blue": "#0000ff",
    "green": "#00ff00",
    "cyan": "#00ffff",
    "red": "#ff0000",
    "magenta": "#ff00ff",
    "yellow": "#ffff00",
    "white": "#ffffff"
  }
}

===================================================================================

Anatomy:

    - name: The display name of the palette as shown in "palette list"
    - description: The description of the palette inside of "palette list"
    - author: The name of the palette's creator.
    - cursor:
        - bg: The color of the cursor block.
        - fg: The color of text underneath the cursor.
    - completions:
        - bg: Background of the tab completions menu
        - text: Text color of the completions menu.
        - border: Border color of the completions menu (not used yet)
        - textHighlight: Text color of the highlighted completion.
        - highlight: Background color of the highlighted completion.
    - colors: Contains mappings from System.ConsoleColor to custom colors. "black" is the background and "gray" is the foreground, just like the normal C# console.
                See https://docs.microsoft.com/en-us/dotnet/api/system.consolecolor?view=net-5.0 for a list of available colors. Color keys must be in camelCase.
