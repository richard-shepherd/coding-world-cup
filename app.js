var Utils = require('./utils');

var Logger = Utils.Logger;
Logger.addHandler(new Utils.LogHandler_Console(Logger.LogLevel.INFO_PLUS));
Logger.addHandler(new Utils.LogHandler_File('./log/log.txt', Logger.LogLevel.WARNING));

Logger.log("@INFO", Logger.LogLevel.INFO);
Logger.log("@INFO_PLUS", Logger.LogLevel.INFO_PLUS);
Logger.log("@WARNING", Logger.LogLevel.WARNING);

Logger.log("Before indent", Logger.LogLevel.INFO_PLUS);
Logger.indent();
Logger.log("Indented 1a", Logger.LogLevel.INFO_PLUS);
Logger.log("Indented 1b", Logger.LogLevel.INFO_PLUS);
Logger.indent();
Logger.log("Indented 2a", Logger.LogLevel.WARNING);
Logger.dedent();
Logger.log("Indented 3a", Logger.LogLevel.INFO_PLUS);
Logger.dedent();
Logger.log("Indented 4a", Logger.LogLevel.INFO_PLUS);
Logger.dedent();
Logger.log("Indented 5a", Logger.LogLevel.INFO_PLUS);


