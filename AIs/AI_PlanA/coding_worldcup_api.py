from __future__ import division
from collections import namedtuple, OrderedDict
import json
import sys
import logging
from position import Position
from vector import Vector
from js_object import JSObject
from classes import MarginFactors, DirectionType, GoalType, PitchCollection
import DMT

# noinspection PyMethodMayBeStatic
class CodingWorldCupAPI(object):
    def __init__(self, **kwargs):

        # Declare class var
        # The dimensions of the pitch...
        self.pitch = None

        # Our team number...
        self.team_number = -1

        # My team storage
        self.my_team_players_all = {}
        self.my_team_players = {}
        self.my_gk = None
        
        self.my_gk_number = -1
        self.my_team_player_numbers = []
        self.my_team_player_numbers_all = []

        # Opp team storage 
        self.opposing_team_players_all = {}
        self.opposing_team_players = {}
        self.opposing_gk = None

        self.opposing_gk_number = -1
        self.opposing_team_player_numbers = []
        self.opposing_team_player_numbers_all = []

        # The game start at the start of each turn.
        # Some of this data is split up into the 'ball' info and the 'players' info...
        self.game_state = None

        # Information about the ball, including its position, direction and speed...
        self.ball = None

        # Info about our team. Includes the score and the direction of play...
        self.team_info = None

        # Info about the opposing team. Includes the score and the direction of play...
        self.opposing_team_info = None

        # True if we are kicking off.
        # (Only valid during a KICKOFF event and request.)
        self.we_are_kicking_off = False

        self.playing_direction = None

        # The current 'game-time' in seconds from the start of the match...
        self.game_time_seconds = -1

        self.we_owned_the_ball = False

        self.MF = MarginFactors()
        self.PC = PitchCollection()


        # Uncomment this line to launch the debugger when the AI starts up.
        # you can then set breakpoints in the rest of your code.
        # System.Diagnostics.Debugger.Launch();

        # We run the game loop...
        # Logger.log("Starting game loop", Logger.LogLevel.INFO);
        # DEBUG - INFO - WARNING - ERROR - CRITICAL
        # logging.basicConfig(filename='~log.log', filemode='w', level=logging.DEBUG)

        # for test

        od = OrderedDict(sorted(kwargs.items()))
        for k, m in od.items():
            self.process_message(m)

        while True:
        # We "listen" to game messages on stdin, and process them...
            message = sys.stdin.readline()
            self.process_message(message)

    @staticmethod
    def send_reply(reply):
        str_reply = reply.to_json()
        logging.debug("Sending reply: %s " % str_reply)
        sys.stdout.writelines(str_reply + "\r")
        sys.stdout.flush()

        # print(str_reply)


    # # Returns a Position for the centre of the requested goal.
    def get_goal_centre(self, goal):
        x = 0.0
        if ((goal == GoalType.OUR_GOAL and self.playing_direction == DirectionType.LEFT)
           or (goal == GoalType.THEIR_GOAL and self.playing_direction == DirectionType.RIGHT)):
            x = self.pitch.width
        y = self.pitch.goalCentre
        return Position(x, y)

    # Default implementation for the CONFIGURE_ABLITIES request.
    # We give all players an average level of ability in all areas.
    def process_request_configure_abilities(self, data):
        # As a default, we give all players equal abilities...
        total_kicking_ability = data.totalKickingAbility
        total_running_ability = data.totalRunningAbility
        total_ball_control_ability = data.totalBallControlAbility
        total_tackling_ability = data.totalTacklingAbility

        # +1 for the goalkeeper
        number_of_players = len(self.my_team_players) + 1

        # We create the reply...
        reply = JSObject()
        reply.add("requestType", "CONFIGURE_ABILITIES")

        # We give each player an average ability in all categories...
        # This is a 'list'
        player_infos = []
        # foreach(var playerNumber in self.allTeamPlayers.Keys)
        for player_number in self.my_team_players_all:
            # This is a 'dict'
            player_info = JSObject()
            player_info.add("playerNumber", player_number)
            player_info.add("kickingAbility", total_kicking_ability / number_of_players)
            player_info.add("runningAbility", total_running_ability / number_of_players)
            player_info.add("ballControlAbility", total_ball_control_ability / number_of_players)
            player_info.add("tacklingAbility", total_tackling_ability / number_of_players)
            # A list that consists of dict object
            player_infos.append(player_info)

        reply.add("players", player_infos)

        self.send_reply(reply)

    # Default implementation for the kickoff request.
    # Returns a minimal response (which results in default positions
    # being allocated).
    def process_request_kickoff(self):
        # We create the reply...
        reply = JSObject()
        reply.add("requestType", "KICKOFF")
        reply.add("players", [])
        self.send_reply(reply)

    # Default implementation for the PLAY request.
    # We send back an empty list of actions, which means that
    # the players do nothing.
    def process_request_play(self):
        # We create the reply...
        reply = JSObject()
        reply.add("requestType", "PLAY")
        reply.add("actions", [])
        self.send_reply(reply)

    # Called after the TEAM_INFO event has been processed.
    # (If the implementation in this class has been called.)
    def on_team_info_updated(self):
        a = 1

    def on_goal(self):
        a = 1

    # Called after the GOAL event has been processed.
    # (If the implementation in this class has been called.)
    def process_message(self, message):
        # Called when we have received a new message from the game-engine.
        logging.debug("Received message:\n%s" % message)
        # We decode the JSON message, and process it depending on its message type...
        data = json.loads(message, object_hook=lambda d: namedtuple('X', d.keys())(*d.values()))
        message_type = data.messageType
        if message_type == "EVENT":
            self.process_event(data)
        elif message_type == "REQUEST":
            self.process_request(data)
        else:
            raise ValueError("Unexpected message_type %s" % message_type)

    # Called when we receive an EVENT.
    def process_event(self, data):
        event_type = data.eventType
        if event_type == "GAME_START":
            self.process_event_game_start(data)
        elif event_type == "TEAM_INFO":
            self.process_event_team_info(data)
        elif event_type == "START_OF_TURN":
            self.process_event_start_of_turn(data)
        elif event_type == "GOAL":
            self.process_event_goal(data)
        elif event_type == "KICKOFF":
            self.process_event_kickoff(data)
        elif event_type == "HALF_TIME":
            self.process_event_half_time(data)
        else:
            raise ValueError("Unexpected event_type %s" % event_type)

    # Called when we receive a REQUEST.
    def process_request(self, data):

        request_type = data.requestType
        if request_type == "CONFIGURE_ABILITIES":
            self.process_request_configure_abilities(data)
        elif request_type == "KICKOFF":
            self.process_request_kickoff()
        elif request_type == "PLAY":
            self.process_request_play()
        else:
            raise ValueError("Unexpected request_type %s" % request_type)

    # Called when we receive a GAME_START event.
    def process_event_game_start(self, data):
        self.pitch = data.pitch

    # Called when we receive a TEAM_INFO event.
    def process_event_team_info(self, data):
        self.team_number = data.teamNumber

        # We keep a collection of all the player-numbers in our team
        # as well as the information sorted by player vs. goalkeeper...
        for player in data.players:

            player_number = player.playerNumber

            if len(self.my_team_player_numbers_all) < 6:
                self.my_team_player_numbers_all.append(player_number)

            # We set up the all-players collection...
            self.my_team_players_all[player_number] = {}

            # And the player  goalkeeper split...
            if player.playerType == "P":
                # This is a player...
                self.my_team_players[player_number] = {}
                self.my_team_player_numbers.append(player_number)
            else:
                # This is the goalkeeper...
                self.my_gk_number = player_number

        # We notify that team inf has been updated...
        self.on_team_info_updated()

    # Called when we receive a START_OF_TURN event.


    def process_event_start_of_turn(self, data):
        logging.debug("---------------------- SOT ----------------------")
        # We store the whole game-state...
        self.game_state = data

        # And we split up parts of it.

        # The time...
        self.game_time_seconds = data.game.currentTimeSeconds

        # The ball...
        self.ball = data.ball

        if self.ball.controllingPlayerNumber in self.my_team_player_numbers_all:
            self.we_owned_the_ball = True
        elif self.ball.controllingPlayerNumber in self.opposing_team_player_numbers_all:
            self.we_owned_the_ball = False

        # The teams...
        if self.team_number == 1:
            # We are team 1...
            self.store_team_info(data.team1)
            self.store_opposing_team_info(data.team2)
        else:
            # We are team 2...
            self.store_team_info(data.team2)
            self.store_opposing_team_info(data.team1)

    def store_team_info(self, team_info):
        # Stores info about our team in our internal collections.
        # We store info about each player in the team in a map
        # by player-number, and we also split out the player info
        # from the goalkeeper info...
        player_idx = 0
        for player_info in team_info.players:
            static_state = player_info.staticState
            player_number = static_state.playerNumber

            # We store all the players in one collection...
            self.my_team_players_all[player_number] = player_info

            # And split by player vs. goalkeeper...
            player_type = static_state.playerType
            if player_type == "P":
                # This is a player...
                self.my_team_players[player_number] = player_info
            else:
                # This is the goalkeeper...
                self.my_gk = player_info

    def store_opposing_team_info(self, team_info):
        # Stores info about the opposing team in our internal collections.
        # We store info about each player in the opposing team
        # in a map by player-number...
        for player_info in team_info.players:
            static_state = player_info.staticState
            player_number = player_info.staticState.playerNumber

            self.opposing_team_players_all[player_number] = player_info

            if len(self.opposing_team_player_numbers_all) < 6:
                self.opposing_team_player_numbers_all.append(player_number)

            # And split by player vs. goalkeeper...
            player_type = static_state.playerType
            if player_type == "P":
                # This is a player...
                self.opposing_team_players[player_number] = player_info
                self.opposing_team_player_numbers.append(player_number)
            else:
                # This is the goalkeeper...
                self.opposing_gk = player_info
                self.opposing_gk_number = player_number

    def process_event_goal(self, data):
        # Called when a goal is scored.
        # We notify the derived class...
        self.on_goal()

    def process_event_half_time(self, data):
        # Called at half-time.
        a = 1

    def process_event_kickoff(self, data):
        # Called when we receive a KICKOFF event.
        # We store the team info (including playing direction and score)...

        if self.team_number == 1:
            # We are team 1...
            self.team_info = data.team1
            self.opposing_team_info = data.team2
        else:
            # We are team 2...
            self.team_info = data.team2
            self.opposing_team_info = data.team1

        # We find the direction we are playing...
        if self.team_info.direction == "LEFT":
            self.playing_direction = DirectionType.LEFT

        else:
            self.playing_direction = DirectionType.RIGHT

        # Are we the team kicking off?
        self.we_are_kicking_off = (data.teamKickingOff == self.team_number)

    def build_action_move(self, player_number, dest):
        if not dest.is_position_in_pitch():
            raise ValueError("Action_Move: dest (%1.2f,%1.2f) is not in ptich" %(dest.x, dest.y))
            return False
        if dest.is_position_in_goal_area() and player_number != self.my_gk_number:
            raise ValueError("Action_Move: dest (%1.2f,%1.2f) is in GA while the p is not GK" %(dest.x, dest.y))
            return False

        action = JSObject()
        action.add("playerNumber", player_number)
        action.add("action", "MOVE")
        action.add("destination", dest)
        action.add("speed", 100.0)
        return action

    def build_action_turn(self, player_number, direction):
        action = JSObject()
        action.add("action", "TURN")
        action.add("playerNumber", player_number)
        action.add("direction", direction)
        return action

    def build_action_take_possession(self, player_number):
        action = JSObject()
        action.add("action", "TAKE_POSSESSION")
        action.add("playerNumber", player_number)
        return action

    def build_action_kick(self, player_number, kick_at, speed, angle_margin=-1):
        action = JSObject()
        if angle_margin >= 0:
            player = self.my_team_players_all[player_number]
            player_pos = Position(position=player.dynamicState.position)

            kick_vector = Vector(pos_from=player_pos, pos_to=kick_at)
            angle = kick_vector.angle_from(direction=player.dynamicState.direction)
            if angle > angle_margin:
                # should turn first
                logging.debug("In BLD_ACTION_KICK, TURN FIRST as angle [%1.2f](m:%1.2f, v:%1.2f) is too wider than margin [%1.2f] " %(angle, player.dynamicState.direction, kick_vector.direction, angle_margin))
                return self.build_action_turn(player_number, kick_vector.direction)

        action.add("action", "KICK")
        action.add("playerNumber", player_number)
        action.add("destination", kick_at)
        action.add("speed", speed)
        return action

    def build_action_move_and_take(self, player_number, dest, ball_pos, take_dist):
        if not dest.is_position_in_pitch():
            raise ValueError("Action_Move: dest (%1.2f,%1.2f) is not in ptich" %(dest.x, dest.y))
            return False
        if dest.is_position_in_goal_area() and player_number != self.my_gk_number:
            raise ValueError("Action_Move: dest (%1.2f,%1.2f) is in GA while the p is not GK" %(dest.x, dest.y))
            return False
        action = JSObject()
        p_pos = Position(position=self.my_team_players_all[player_number].dynamicState.position)
        if p_pos.distance_from(ball_pos) <= take_dist:
            # raise ValueError("Move & Take ball_pos(%1.2f, %1.2f), dist[%1.2f], p_pos (%1.2f, %1.2f)"%(ball_pos.x, ball_pos.y, p_pos.distance_from(ball_pos), p_pos.x,p_pos.y))
            action.add("action", "TAKE_POSSESSION")
            action.add("playerNumber", player_number)
            return action
        else:
            final_dest = dest
            if player_number != self.my_gk_number:
                next_pos = DMT.player_will_be_at(dest, self.my_team_players_all[player_number])
                if next_pos.is_position_in_goal_area():
                    new_dest = self.get_new_pos_detouring_goal_area(player_number, dest)
                    logging.debug("!!! DETOUR fr ({},{}) to ({},{})".format(dest.x,dest.y,new_dest.x,new_dest.y))
                    if new_dest.is_position_in_goal_area():
                        final_dest = self.get_new_pos_detouring_goal_area(player_number, new_dest)
                        logging.debug("!!! 2nd DETOUR fr ({},{}) to ({},{})".format(new_dest.x,new_dest.y,final_dest.x,final_dest.y))
                    else:
                        final_dest = new_dest

            action.add("playerNumber", player_number)
            action.add("action", "MOVE")
            action.add("destination", final_dest)
            action.add("speed", 100.0)
            return action

    def get_new_pos_detouring_goal_area(self, player_number, dest):
        p = self.my_team_players[player_number]
        p_pos = Position(position=p.dynamicState.position)
        to_dest_vector = Vector(pos_from=p_pos, pos_to=dest)

        # Left side GA
        if p_pos.x <= self.pitch.centreSpot.x:
            if 90 <= to_dest_vector.direction <= 270:
                new_vector = to_dest_vector.get_rotated_vector(-self.MF.GA_DETOUR_ANGLE)
            else:
                new_vector = to_dest_vector.get_rotated_vector(self.MF.GA_DETOUR_ANGLE)
        # Right side GA
        else:
            if 90 <= to_dest_vector.direction <= 270:
                new_vector = to_dest_vector.get_rotated_vector(self.MF.GA_DETOUR_ANGLE)
            else:
                new_vector = to_dest_vector.get_rotated_vector(-self.MF.GA_DETOUR_ANGLE)

        return p_pos.get_position_plus_vector_for_player(new_vector)
# G_note
# Make shoot_x limit -> if no agnle, pass!