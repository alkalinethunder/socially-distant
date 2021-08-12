// Thundershock Engine script for testing that the script system works.

var interval = 5;
var totalTime = 0;

function awake() {
    var btn = new TextBlock();
    btn.Text = "Oh my god it's Robert Loggia";
    btn.ForeColor = StyleColor.FromHtml("#1baaf7");
    Gui.AddToViewport(btn);
}

function update(deltaTime) {
    if (totalTime >= interval) {
        totalTime = 0;
    }
    
    totalTime += deltaTime;
}