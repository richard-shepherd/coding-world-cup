#include "stdafx.h"
#include "Logger.h"
//#include"Utilities.h"
const string CLogger::m_sFileName = "./my_log.log";
CLogger* CLogger:: m_pThis = NULL;
ofstream CLogger::m_Logfile;
char sMessage[4096];

CLogger::CLogger()
{
 
}
CLogger* CLogger::getLogger(){
    if(m_pThis == NULL){
        m_pThis = new CLogger();
        m_Logfile.open(m_sFileName.c_str(), ios::out | ios::app );
    }
    return m_pThis;
}
 
void CLogger::Log( const char * format, ... )
{
    
    va_list args;
    va_start (args, format);
#ifdef _WIN32	
    vsprintf_s(sMessage,format, args);
#else
	vsprintf(sMessage,format, args);
#endif	
    //m_Logfile <<"\n"<<Util::CurrentDateTime()<<":\t";
    m_Logfile << sMessage << "\n";
    va_end (args);
	m_Logfile.flush();
}
 
void CLogger::Log( const string& sMessage )
{
    //m_Logfile <<"\n"<<Util::CurrentDateTime()<<":\t";
    m_Logfile << sMessage << "\n";
	m_Logfile.flush();
}
 
CLogger& CLogger::operator<<(const string& sMessage )
{
    //m_Logfile <<"\n"<<Util::CurrentDateTime()<<":\t";
    m_Logfile << sMessage << "\n";
	m_Logfile.flush();
    return *this;
}