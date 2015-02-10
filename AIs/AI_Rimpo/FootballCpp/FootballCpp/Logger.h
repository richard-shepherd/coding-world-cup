#pragma once

//NOTE: Source code copied from https://cppcodetips.wordpress.com/2014/01/02/a-simple-logger-class-in-c/


#include <fstream>
#include <iostream>
#include <cstdarg>
#include <string>
using namespace std;
#define LOGGER CLogger::getLogger()
/**
 *   Singleton Logger Class.
 */
class CLogger
{
public:
    /**
     *   Logs a message
     *   @param sMessage message to be logged.
     */
    void Log(const std::string& sMessage);
    /**
     *   Variable Length Logger function
     *   @param format string for the message to be logged.
     */
    void Log( const char * format, ... );
        /**
     *   << overloaded function to Logs a message
     *   @param sMessage message to be logged.
     */
    CLogger& operator<<(const string& sMessage );
    /**
     *   Funtion to create the instance of logger class.
     *   @return singleton object of Clogger class..
     */
    static CLogger* getLogger();
private:
    /**
     *    Default constructor for the Logger class.
     */
    CLogger();
    /**
     *   copy constructor for the Logger class.
     */
    CLogger( const CLogger&){};             // copy constructor is private
    /**
     *   assignment operator for the Logger class.
     */
    CLogger& operator=(const CLogger& ){ return *this;};  // assignment operator is private
    /**
     *   Log file name.
     **/
    static const std::string m_sFileName;
    /**
     *   Singleton logger class object pointer.
     **/
    static CLogger* m_pThis;
    /**
     *   Log file stream object.
     **/
    static ofstream m_Logfile;
};
