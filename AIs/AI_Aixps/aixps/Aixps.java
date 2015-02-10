package aixps;

import aixps.Player;
import aixps.GameState;
import aixps.NetCom;
import aixps.AI;

import java.io.IOException;
import java.io.StringReader;
import java.io.InputStreamReader;

import java.util.Scanner;

class Aixps
{

	GameState	xGameState = null;
	AI 			xAI = null;

	public Aixps()
	{
		xGameState = new GameState();
	}



	static public void main(String[] args)
	 {
	 	Aixps xCoreProg = new Aixps();
	 	NetCom xCom = new NetCom();
	 	GameState xGameState = new GameState();
	 	xCom.setGameState(xGameState);
		xCoreProg.xAI = new AI();
		xCoreProg.xAI.setGameState(xGameState);
		xCoreProg.xAI.setup();
		xCom.setAI(xCoreProg.xAI);

	 	for(;;)
		{
			InputStreamReader reader = null;
			int iNbMsg = 0;
	        try 
	        {
	          System.err.println("Starting AIXPS Program");	
	          reader = new InputStreamReader(System.in);
	          Scanner scan = new Scanner(reader);

	          while (scan.hasNextLine())
	          {
	          		String sMessage = null;
	          		sMessage = scan.nextLine();
	          		iNbMsg++;

		          	xCom.readMessage(sMessage);
//System.err.println("Evaluate AI");	
		          	xCoreProg.xAI.evaluateSituation();
//System.err.println("Update Cycle");			          	
					xGameState.updateCycle(1);
//System.err.println("Respond");						
		          	xCom.respond();
//System.err.println("End loop");			          	

	          }

	        }
	        catch (Exception e)
	        {
	          System.err.println("error, exiting: " + e);
	          System.exit(0);
	      	}
 		}

	 }
	  
};