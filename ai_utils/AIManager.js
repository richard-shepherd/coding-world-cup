/**
 * AIManager
 * ---------
 * Discovers the collection of available AIs and can launch them when needed.
 *
 * AIs live in the 'AIs' folder. Each one has its own sub-folder, with a name
 * like 'AI_[name]'. The folder contains the AI code itself and a file called
 * 'CommandLine.txt' that says how to launch the AI code.
 */
var fs = require('fs');
var UtilsLib = require('../utils');
var CWCError = UtilsLib.CWCError;
var AIWrapper = require('./AIWrapper');

/**
 * @constructor
 */
function AIManager() {
    // The collection of AIs, mapped from the name of the AI
    // to information about it...
    this._aiInfos = {};
    this._findAIs();
}

/**
 * _findAIs
 * --------
 * Finds the available AIs.
 */
AIManager.prototype._findAIs = function() {

    // Each AI is a folder in the 'AIs' folder...
    var folders = fs.readdirSync('AIs');
    folders.forEach(function(folder) {

        // Is this an AI folder?
        if(folder.substring(0, 3) !== "AI_") {
            return;
        }

        // We get the name of the AI, ie the folder name without
        // the 'AI_' prefix...
        var name = folder.substring(3);

        // We've found an AI folder, so we read the command-line from it...
        var commandLineFile = 'AIs/' + folder + '/CommandLine.txt';
        var commandLineFileExists = fs.existsSync(commandLineFile);
        if(!commandLineFileExists) {
            return;
        }

        // We parse the command-line file...
        var contents = fs.readFileSync(commandLineFile, 'utf-8');
        var lines = contents.split('\r\n');
        if(lines.length === 0) {
            return;
        }
        var commandLine = lines[0];

        // We split the line into the executable and the arguments...
        var tokens = commandLine.split(' ');
        if(tokens.length === 0) {
            return;
        }
        var executable = tokens[0];
        var args = [];
        for(var i=1; i<tokens.length; ++i) {
            var arg = tokens[i];
            if(arg !== '') {
                args.push(tokens[i]);
            }
        }

        // We find the relative and absolute paths to the folder...
        var relativeFolder = 'AIs/' + folder;
        var absoluteFolder = process.cwd() + '/' + relativeFolder;

        // We add the info to our collection...
        this._aiInfos[name] = {
            name:name,
            relativeFolder:relativeFolder,
            absoluteFolder:absoluteFolder,
            executable:executable,
            args:args
        };
    }, this);
};

/**
 * getAIWrapperFromName
 * --------------------
 * Returns an AIWrapper for the name passed in.
 */
AIManager.prototype.getAIWrapperFromName = function(name) {
    // We find the info for this AI...
    if(!(name in this._aiInfos)) {
        throw new CWCError('Could not find AI: ' + name);
    }
    var aiInfo = this._aiInfos[name];

    // And create an AIWrapper for it...
    var aiWrapper = new AIWrapper(name);
    aiWrapper.wrap(aiInfo);

    return aiWrapper;
};

// Exports...
module.exports = AIManager;



