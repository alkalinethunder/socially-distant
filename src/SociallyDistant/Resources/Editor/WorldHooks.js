function onCreate(saveFile, done) {
    // Use this method to set values on the newly created
    // save file.
    done();
}

function onCreateHomeDirectory(fs, device, user, home) {
    // Use this method to write dynamic data to the home directory
    // currently being created.
    
    // fs is the virtual file system for the device.
    // device is the save data information for the device.
    // user is the UNIX user information for the home directory currently being created.
    // home is the absolute path to the home directory.
}

function beforeGameStart(saveFile, gameState) {
    // Use this  method to manage how the game starts up. saveFile is the current state of the save
    // file, duh, and gameState allows you to access and perform actions on the state of the game.
    //
    // Example use: Start a tutorial mission if the save file is marked as "new".
}