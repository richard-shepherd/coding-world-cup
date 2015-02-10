package main;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;


class Main {

	public static void main(String[] args) {
		AICalypsoPB ai = new AICalypsoPB();
		BufferedReader in = new BufferedReader(new InputStreamReader(System.in));
		while(true) {

			try {
				ai.processJsonMessage(in.readLine());

			} catch (IOException e) {
				e.printStackTrace();
			}

		}

	}

}
