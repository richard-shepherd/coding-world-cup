from __future__ import division
from coding_worldcup_api import CodingWorldCupAPI
from position import Position
from vector import Vector
from js_object import JSObject
import DMT
from classes import MarginFactors, DirectionType, GoalType, PitchCollection, RoleType
import random
import logging
import math

class GohAI (CodingWorldCupAPI):

    class PositionKey(tuple):
        def __new__(cls, itr):
            seq = [int(x) for x in itr]
            return tuple.__new__(cls, seq)

    class PositionValue:
        def __init__(self, position, direction):
            self.position = position
            self.direction = direction
    # chg - for Debugging
    # def __init__(self,m1,m2,m3,m4,m5,m6,m7):

    def __init__(self, **kwargs):
        self.player_number_lwb = -1
        self.player_number_cb = -1
        self.player_number_rwb = -1
        self.player_number_lwf = -1
        self.player_number_rwf = -1
        self.fw_numbers = []
        self.wb_numbers = []
        self.dflt_positions = {}
        self.kickoff_positions = {}
        self.rebounder_info = ()

        # chg - For Debuging
        # super(GohAI, self).__init__(m1,m2,m3,m4,m5,m6,m7)
        super(GohAI, self).__init__(**kwargs)

    # Called when we receive a PLAY request. We act as discussed in the heading comments.
    def process_request_play(self):
        # Store player number finished building action
        player_actioned = []

        # We create the reply object...
        reply = JSObject()
        reply.add("requestType", "PLAY")

        actions = []
        if self.ball.controllingPlayerNumber != -1 or self.ball.speed == 0:
            self.rebounder_info = ()

        # When no one has ball, chase it
        if self.ball.controllingPlayerNumber == -1:
            min_time_to_ball = 9999
            escort_player_number = -1
            min_ep = None
            for player_number in self.my_team_player_numbers:
                # find escort point (ep)
                logging.debug("%%%% Checking Escort for p[%d]" % player_number)
                ep = DMT.where_to_move_to_get_the_free_ball(self.my_team_players_all[player_number], self.ball)
                if ep is not None:
                    # logging.debug("&&&&& Escort point(%1.2f, %1.2f)"%(ep.x, ep.y))
                    time = ep.time_from(self.my_team_players_all[player_number])
                    if time < min_time_to_ball:
                        min_time_to_ball = time
                        escort_player_number = player_number
                        min_ep = ep

            if escort_player_number >= 0:
                logging.info("#####1-1 Escort: P[%d](%1.2F,%1.2F), time_to_ball[%1.2f] dest(%1.2f, %1.2f)" % (escort_player_number, self.my_team_players_all[escort_player_number].dynamicState.position.x, self.my_team_players_all[escort_player_number].dynamicState.position.y, min_time_to_ball, min_ep.x, min_ep.y))
                actions.append(self.build_action_move_and_take(escort_player_number, min_ep, self.ball.position, self.MF.WHEN_TO_TAKE_POSSESSION))
                player_actioned.append(escort_player_number)
            else: # just find closet player and go to ball ending up area
                ball_ends_up_at = DMT.where_ball_ends_up(self.ball)
                if ball_ends_up_at.is_position_in_player_field():
                    near_player_tp = DMT.get_nearby_player(ball_ends_up_at, self.my_team_players)

                    logging.info("#####1-2 BallChasing: This p[%d] is closest to ball_end_up_dest (%1.2f, %1.2f], move!"%(near_player_tp[1].staticState.playerNumber, ball_ends_up_at.x, ball_ends_up_at.y))
                    actions.append(self.build_action_move_and_take(near_player_tp[1].staticState.playerNumber, ball_ends_up_at, self.ball.position, self.MF.WHEN_TO_TAKE_POSSESSION))
                    player_actioned.append(near_player_tp[1].staticState.playerNumber)
                else:
                    logging.debug("##### BallChasing: Ball ends up at (%1.2f, %1.2f) is in goal area, let GK handle it " % (ball_ends_up_at.x, ball_ends_up_at.y))

        # When our teammate owns the ball
        elif self.ball.controllingPlayerNumber in self.my_team_player_numbers_all:
            controlling_player = self.my_team_players_all[self.ball.controllingPlayerNumber]
            player_actioned.append(self.ball.controllingPlayerNumber)
            # player = self.my_team_players_all[self.ball.controllingPlayerNumber]
            if self.ball.controllingPlayerNumber in self.fw_numbers:
                self.get_action_fw_has_ball(actions, controlling_player)
            elif self.ball.controllingPlayerNumber == self.player_number_cb:
                self.get_action_cb_has_ball(actions, controlling_player)
            elif self.ball.controllingPlayerNumber in self.wb_numbers:
                self.get_action_wb_has_ball(actions, controlling_player)
            elif self.ball.controllingPlayerNumber == self.my_gk_number:
                self.get_action_gk_has_ball(actions)
            else:
                raise ValueError("Invalid player_number[%d]" % self.ball.controllingPlayerNumber)

            pass_receiver = self.get_pass_receiver_ready(actions)
            if pass_receiver >= 0:
                player_actioned.append(pass_receiver)
            shoot_rebounder = self.get_shoot_rebounder_ready(actions)
            if len(shoot_rebounder) >0:
                player_actioned.append(shoot_rebounder[0])
                self.rebounder_info = shoot_rebounder

        # When opp_team owns the ball
        elif self.ball.controllingPlayerNumber in self.opposing_team_player_numbers:
            opp_pos = Position(position=self.opposing_team_players[self.ball.controllingPlayerNumber].dynamicState.position)
            near_player_tp = DMT.get_nearby_player(opp_pos, self.my_team_players)

            actions.append(self.build_action_move_and_take(near_player_tp[1].staticState.playerNumber, opp_pos, self.ball.position, self.MF.WHEN_TO_TAKE_POSSESSION))
            logging.debug("#####3-1 FollowOP, OP[%d](%1.2F,%1.2F), MP[%d](%1.2F,%1.2F)" % (self.ball.controllingPlayerNumber, opp_pos.x, opp_pos.y, near_player_tp[1].staticState.playerNumber, near_player_tp[1].dynamicState.position.x, near_player_tp[1].dynamicState.position.y))
            player_actioned.append(near_player_tp[1].staticState.playerNumber)

        # else
            # Opp_GK Has the ball
            # raise ValueError("Where's the ball then ?")

        # loop remaining players to build actions
        for player_number in self.my_team_player_numbers_all:
            if player_number in player_actioned:
                continue
            if len(self.rebounder_info) > 0 and self.rebounder_info[0] == player_number:
                continue
            if player_number in self.wb_numbers:
                self.get_action_wb(actions, player_number)
            elif player_number == self.player_number_cb:
                self.get_action_cb(actions, player_number)
            elif player_number in self.fw_numbers:
                self.get_action_fw(actions, player_number)
            elif player_number == self.my_gk_number:
                self.get_action_gk(actions)
            else:
                raise ValueError("Unexpected player_number [%d]" % player_number)

        reply.add("actions", actions)
        self.send_reply(reply)

    # Called when a goal has been scored.
    def on_goal(self):
        message = "0: GOAL!!! %1.2f" % self.game_time_seconds
        logging.info(message)

    # Works out the action we want to take for this defender.
    def get_action_cb(self, actions, player_number):
        player = self.my_team_players_all[player_number]
        player_pos = Position(position=player.dynamicState.position)
        ball_pos = Position(position=self.ball.position)
        dflt_pos = self.get_dflt_position(player_number, self.playing_direction)

        dist_to_ball = player_pos.distance_from(self.ball.position)
        if dist_to_ball < self.MF.CB_HOLD_AREA and not(ball_pos.is_position_in_goal_area) \
                and self.ball.controllingPlayerNumber not in self.my_team_player_numbers_all:
            actions.append(self.build_action_move_and_take(player_number, ball_pos
                            , self.ball.position, self.MF.WHEN_TO_TAKE_POSSESSION))
            return
        else:
            if self.ball.controllingPlayerNumber in self.my_team_player_numbers_all \
                    or (self.ball.controllingPlayerNumber == -1 and self.we_owned_the_ball):
                if DMT.is_in_offense_area(self.ball.position, self.playing_direction, self.pitch.centreSpot):

                    if self.playing_direction == DirectionType.LEFT:
                        new_dflt_pos = Position(x=dflt_pos.position.x - 20, y=dflt_pos.position.y)
                    else:
                        new_dflt_pos = Position(x=dflt_pos.position.x + 20, y=dflt_pos.position.y)
                else:
                    if self.playing_direction == DirectionType.LEFT:
                        new_dflt_pos = Position(x=dflt_pos.position.x - 10, y=dflt_pos.position.y)
                    else:
                        new_dflt_pos = Position(x=dflt_pos.position.x + 10, y=dflt_pos.position.y)

                actions.append(self.build_action_move_and_take(player_number, new_dflt_pos, self.ball.position, self.MF.WHEN_TO_TAKE_POSSESSION))
                return

            elif self.ball.controllingPlayerNumber in self.opposing_team_player_numbers_all \
                    or (self.ball.controllingPlayerNumber == -1 and not self.we_owned_the_ball):

                if DMT.is_in_offense_area(self.ball.position, self.playing_direction, self.pitch.centreSpot):
                    new_dflt_pos = Position(x=dflt_pos.position.x, y=dflt_pos.position.y)
                else:
                    if DMT.is_ball_in_dangerous_area(self.ball.position, self.get_goal_centre(GoalType.OUR_GOAL)) and not(ball_pos.is_position_in_goal_area()):
                        # raise ValueError("DANGER")
                        logging.debug("dangerous!! ball_in({},{})".format(ball_pos.x, ball_pos.y))
                        actions.append(self.build_action_move_and_take(player_number, ball_pos, self.ball.position, self.MF.WHEN_TO_TAKE_POSSESSION))

                    if self.playing_direction == DirectionType.LEFT:
                        new_dflt_pos = Position(x=dflt_pos.position.x + 7, y=dflt_pos.position.y)
                    else:
                        new_dflt_pos = Position(x=dflt_pos.position.x - 7, y=dflt_pos.position.y)

                actions.append(self.build_action_move_and_take(player_number, new_dflt_pos, self.ball.position, self.MF.WHEN_TO_TAKE_POSSESSION))
                return

            else:
                actions.append(self.build_action_move_and_take(player_number, dflt_pos.position, self.ball.position, self.MF.WHEN_TO_TAKE_POSSESSION))
                return
                # raise ValueError("What cases then?")


    # Works out the action we want to take for this defender.
    def get_action_wb(self, actions, player_number):
        player = self.my_team_players_all[player_number]

        our_goal_pos = self.get_goal_centre(GoalType.OUR_GOAL)

        if player.dynamicState.hasBall:
            # The player has the ball, so we kick it to one of the forwards...
            raise ValueError("this method shouldn't be called when has ball")

        near_player1 = DMT.get_nearby_player(our_goal_pos, self.opposing_team_players)[1]
        near_player2 = DMT.get_nearby_player(our_goal_pos, self.opposing_team_players, excl_player_number=near_player1.staticState.playerNumber)[1]

        near_player = None
        if self.playing_direction == DirectionType.LEFT:
            if near_player1.dynamicState.position.y > 25:
                if player_number == self.player_number_lwb:
                    near_player = near_player1
                elif player_number == self.player_number_rwb:
                    near_player = near_player2
                else:
                    raise ValueError("who's wb then ?")
            else:
                if player_number == self.player_number_lwb:
                    near_player = near_player2
                elif player_number == self.player_number_rwb:
                    near_player = near_player1
                else:
                    raise ValueError("who's wb then ?")

        else:
            if near_player1.dynamicState.position.y > 25:
                if player_number == self.player_number_lwb:
                    near_player = near_player2
                elif player_number == self.player_number_rwb:
                    near_player = near_player1
                else:
                    raise ValueError("who's wb then ?")
            else:
                if player_number == self.player_number_lwb:
                    near_player = near_player1
                elif player_number == self.player_number_rwb:
                    near_player = near_player2
                else:
                    raise ValueError("who's wb then ?")

        near_player_pos = Position(position=near_player.dynamicState.position)

        g_to_near_p_vector = Vector(pos_from=our_goal_pos, pos_to=near_player_pos)
        dist_to_p = our_goal_pos.distance_from(near_player_pos)
        if dist_to_p < self.pitch.goalAreaRadius + self.MF.WB_MARKING_DISTANCE:
            if dist_to_p <= self.pitch.goalAreaRadius:
                mark_dist = self.pitch.goalAreaRadius + 0.2
            else:
                mark_dist = dist_to_p
        else:
            mark_dist = dist_to_p - self.MF.WB_MARKING_DISTANCE

        if mark_dist > 50:
            mark_dist = 40

        mark_vector = g_to_near_p_vector.get_scaled_vector(mark_dist)
        mark_point = our_goal_pos.get_position_plus_vector_for_player(mark_vector)

        logging.debug("WB_MARK_Test [{}] is moving to mark_point ({},{}) to mark op[{}]".format(player_number, mark_point.x, mark_point.y , near_player.staticState.playerNumber))

        actions.append(self.build_action_move_and_take(player_number, mark_point, self.ball.position, self.MF.WHEN_TO_TAKE_POSSESSION))
        logging.debug("WB_MARK [{}] is moving to mark_point ({},{}) to mark op[{}]".format(player_number, mark_point.x, mark_point.y , near_player.staticState.playerNumber))


    # Gets the current action for one of the forwards.
    def get_action_fw(self, actions, player_number):
        player = self.my_team_players_all[player_number]
        ball_position = self.ball.position

        if player.dynamicState.hasBall:
            # The player has the ball, so we kick it at the goal...
            raise ValueError("This method shouldn't be called when has ball")

        my_dflt_pos = Position(position=self.get_dflt_position(player_number, self.playing_direction).position)
        cb_pos = Position(position=self.my_team_players[self.player_number_cb].dynamicState.position)
        rfw_pos = Position(position=self.my_team_players[self.player_number_rwf].dynamicState.position)

        if self.playing_direction == DirectionType.LEFT:
            if player_number == self.player_number_rwf:
                new_dflt_pos = Position(x=cb_pos.x - 25, y=my_dflt_pos.y)
                if new_dflt_pos.is_position_in_goal_area():
                    new_dflt_pos = Position(x=18, y=my_dflt_pos.y)

            elif player_number == self.player_number_lwf:
                new_dflt_pos = Position(x=rfw_pos.x - 5, y=my_dflt_pos.y)
                if new_dflt_pos.is_position_in_goal_area():
                    new_dflt_pos = Position(x=15.5, y=my_dflt_pos.y)
            else:
                raise ValueError("What fw is it then?")
        else:
            if player_number == self.player_number_rwf:
                new_dflt_pos = Position(x=cb_pos.x + 25, y=my_dflt_pos.y)
                if new_dflt_pos.is_position_in_goal_area():
                    new_dflt_pos = Position(x=82, y=my_dflt_pos.y)

            elif player_number == self.player_number_lwf:
                new_dflt_pos = Position(x=rfw_pos.x + 5, y=my_dflt_pos.y)
                if new_dflt_pos.is_position_in_goal_area():
                    new_dflt_pos = Position(x=84.5, y=my_dflt_pos.y)
            else:
                raise ValueError("What fw is it then?")

        # The ball is in the other half, so the player moves back
        # to his dflt position...

        actions.append(self.build_action_move_and_take(player_number, new_dflt_pos, self.ball.position, self.MF.WHEN_TO_TAKE_POSSESSION))

    # # Kicks the ball from a defending player to a forward.
    def get_action_cb_has_ball(self, actions, player):
        # Ov - Sp - Fd - Sh
        player_number = player.staticState.playerNumber
        player_pos = Position(position=player.dynamicState.position)
        time_to_get_tackle = DMT.get_nearby_tackling_opponent(player_pos, self.opposing_team_players)
        shoot_at = self.get_goal_centre(GoalType.THEIR_GOAL)
        shoot_vector = Vector(pos_from=player_pos, pos_to=shoot_at)
        is_in_shooting_range = DMT.is_in_shooting_range(player_pos, self.get_goal_centre(GoalType.THEIR_GOAL), RoleType.CB)

        logging.debug("##### CBhb: Time2Tackle:[%1.2f] ---------------------" % time_to_get_tackle[0])
        if time_to_get_tackle[0] > 0:

            ####### OVERLAP #################################
            if self.MF.can_cb_overlap(self.playing_direction, player_pos.x):
                dest_pos = Position(x=self.MF.get_cb_overlap_limit_x(self.playing_direction), y=player_pos.y)
                if self.try_move_forward(actions, player, RoleType.CB, dest_pos):
                    return

            ####### KILL_PASS #################################
            kill_spot = self.try_kill_pass(player)
            if kill_spot is not None:
                actions.append(self.build_action_kick(player_number, kill_spot[0], kill_spot[1], angle_margin=self.MF.CB_PASS_ANGLE_MARGIN))
                return

            ####### FEED_PASS #################################
            feed_spot = self.feed_pass(player)
            if feed_spot is not None:
                actions.append(self.build_action_kick(player_number, feed_spot[0], feed_spot[1], angle_margin=self.MF.CB_PASS_ANGLE_MARGIN))
                return

            ####### SAFE_PASS #################################
            if self.try_safe_pass(actions, player, RoleType.CB, time_to_get_tackle[0], self.MF.CB_PASS_ANGLE_MARGIN):
                return
            ####### SHOOT #################################
            if is_in_shooting_range:
                angle_adjustable = DMT.get_angle_adjustable(time_to_get_tackle[0])
                angle_diff = shoot_vector.angle_from(direction=player.dynamicState.direction)
                logging.info("&&& CBShoot : ang_diff[%1.2f] (kick_dir[%1.2f], my_dir[%1.2f]) ang_adjable [%1.2f] in tckl_time(%1.2fs), CB_angle_mg [%1.2f]"%(angle_diff, shoot_vector.direction, player.dynamicState.direction, angle_adjustable, time_to_get_tackle[0], self.MF.CB_KICK_ANGLE_MARGIN))
                if angle_diff - angle_adjustable < self.MF.CB_KICK_ANGLE_MARGIN:
                    logging.debug("&&& CBShooooooot corectly !")
                    actions.append(self.build_action_kick(player_number, shoot_at, 100.0, angle_margin=self.MF.CB_KICK_ANGLE_MARGIN))
                    return

        # Even tho no time to turn or anything, just  try pass
        if self.try_safe_pass(actions, player, RoleType.CB, 9999, 360):
            return

        # Last choice. random kick
        if random.random() < 0.5:
            forward_player_number = self.player_number_lwf
        else:
            forward_player_number = self.player_number_rwf

        forward = self.my_team_players_all[forward_player_number]
        destination = Position(position=Position(position=forward.dynamicState.position))
        # We kick the ball towards him...
        actions.append(self.build_action_kick(player_number, destination, DMT.get_pass_speed_for_accuracy(player_pos, destination)))

        logging.info("%1.2f: [Sucks] Centre %d kicks the ball to (%1.2f, %1.2f)" % (self.game_time_seconds, player_number, destination.x, destination.y))


    # # Kicks the ball from a defending player to a forward.
    def get_action_wb_has_ball(self, actions, player):
        # Ov - Sp - Fd - Sh
        player_number = player.staticState.playerNumber
        player_pos = Position(position=player.dynamicState.position)
        time_to_get_tackle = DMT.get_nearby_tackling_opponent(player_pos, self.opposing_team_players)
        shoot_at = self.get_goal_centre(GoalType.THEIR_GOAL)

        logging.debug("##### WBhb: Time2Tackle:[%1.2f] ---------------------" % time_to_get_tackle[0])
        if time_to_get_tackle[0] > 0:

            ####### OVERLAP #################################
            if self.MF.can_wb_overlap(self.playing_direction, player_pos.x):
                dest_pos = Position(x=self.MF.get_wb_overlap_limit_x(self.playing_direction), y=player_pos.y)
                if self.try_move_forward(actions, player, RoleType.WB, dest_pos):
                    return

            ####### FEED_PASS #################################
            feed_spot = self.feed_pass(player)
            if feed_spot is not None:
                actions.append(self.build_action_kick(player_number, feed_spot[0], feed_spot[1], angle_margin=self.MF.WB_PASS_ANGLE_MARGIN))
                return

            ####### SAFE_PASS #################################
            if self.try_safe_pass(actions, player, RoleType.WB, time_to_get_tackle[0], self.MF.WB_PASS_ANGLE_MARGIN):
                return

            ####### KILL_PASS #################################
            kill_spot = self.try_kill_pass(player)
            if kill_spot is not None:
                actions.append(self.build_action_kick(player_number, kill_spot[0], kill_spot[1], angle_margin=self.MF.WB_PASS_ANGLE_MARGIN))
                return



        # # Even tho no time to turn or anything, just  try pass
        # if self.try_safe_pass(actions, player, RoleType.WB, 9999, 360):
        #     return

        # We choose a forward and find his position...
        if random.random() < 0.5:
            forward_player_number = self.player_number_lwf
        else:
            forward_player_number = self.player_number_rwf

        forward = self.my_team_players_all[forward_player_number]
        destination = Position(position=Position(position=forward.dynamicState.position))

        # We kick the ball towards him...
        actions.append(self.build_action_kick(player_number, destination, 100))

        logging.info("%1.2f: [Sucks] Wingback %d kicks the ball to (%1.2f, %1.2f)" % (self.game_time_seconds, player_number, destination.x, destination.y))

    # The forward shoots at the goal.
    def get_action_fw_has_ball(self, actions, player):
        # Drb - Sh - Sp - Fd
        player_number = player.staticState.playerNumber
        player_pos = Position(position=player.dynamicState.position)
        time_to_get_tackle = DMT.get_nearby_tackling_opponent(player_pos, self.opposing_team_players)
        shoot_at = self.get_goal_centre(GoalType.THEIR_GOAL)
        shoot_vector = Vector(pos_from=player_pos, pos_to=shoot_at)
        is_in_shooting_range = DMT.is_in_shooting_range(player_pos, shoot_at, RoleType.FW)

        logging.debug("##### FWhb: Time2Tackle:[%1.2f] ---------------------" % time_to_get_tackle[0])
        if time_to_get_tackle[0] > 0:

            ####### DRIBBLE #################################
            if self.MF.can_fw_dribble(player_pos, shoot_at):
                logging.debug("FW_dribble_trial---------------------")
                dest_pos = DMT.get_fw_dribble_dest(shoot_at, player_pos)
                if self.try_move_forward(actions, player, RoleType.FW, dest_pos):
                    return

            ####### SHOOT #################################
            if is_in_shooting_range:
                angle_adjustable = DMT.get_angle_adjustable(time_to_get_tackle[0])
                angle_diff = shoot_vector.angle_from(direction=player.dynamicState.direction)
                logging.info("&&& FWShoot : ang_diff[%1.2f] (kick_dir[%1.2f], my_dir[%1.2f])  ang_adjable [%1.2f] in tckl_time(%1.2fs), fw_angle_mg [%1.2f]"%(angle_diff, shoot_vector.direction, player.dynamicState.direction, angle_adjustable, time_to_get_tackle[0], self.MF.FW_KICK_ANGLE_MARGIN))
                if angle_diff - angle_adjustable < self.MF.FW_KICK_ANGLE_MARGIN:
                    logging.debug("&&& FWShooooooot corectly !")
                    actions.append(self.build_action_kick(player_number, shoot_at, 100.0, angle_margin=self.MF.FW_KICK_ANGLE_MARGIN))
                    return

            ####### KILL_PASS #################################
            kill_spot = self.try_kill_pass(player)
            if kill_spot is not None:
                actions.append(self.build_action_kick(player_number, kill_spot[0], kill_spot[1], angle_margin=self.MF.FW_PASS_ANGLE_MARGIN))
                return

            ####### SAFE_PASS #################################
            if self.try_safe_pass(actions, player, RoleType.FW, time_to_get_tackle[0], self.MF.FW_PASS_ANGLE_MARGIN):
                return


            ####### FEED_PASS #################################
            feed_spot = self.feed_pass(player)
            if feed_spot is not None:
                actions.append(self.build_action_kick(player_number, feed_spot[0], feed_spot[1], angle_margin=self.MF.FW_PASS_ANGLE_MARGIN))
                return

        # We kick the ball at the goal...
        if DMT.is_in_shooting_range(player_pos, shoot_at, RoleType.FW_NT):
            actions.append(self.build_action_kick(player_number, shoot_at, 100.0))
        elif self.try_safe_pass(actions, player, RoleType.CB, 9999, 360):
            return
        else:
            actions.append(self.build_action_kick(player_number, shoot_at, 100.0))



        # tbi
        logging.info("%1.2f: [Sucks] Forward %d shoots to (%1.2f, %1.2f)" % (self.game_time_seconds, player_number, shoot_at.x, shoot_at.y))

    # Gets the current action for the goalkeeper.
    def get_action_gk(self, actions):

        if self.my_gk.dynamicState.hasBall:
            # 1. The goalkeeper has the ball, so he kicks it to a defender...
            raise ValueError("This method shouldn't be called whenn has ball")

        elif self.ball_is_in_our_goal_area():

            # 2. The ball is in the goal area, so the goalkeeper tries
            #    to take possession...
            self.get_action_gk_ball_in_goal_area(actions)

        else:
            # 3. The ball is outside the goal-area, so the goalkeeper
            #    keeps between it and the goal...
            self.get_action_gk_ball_outside_goal_area(actions)

    # The goalkeeper kicks the ball to a defender.
    def get_action_gk_has_ball(self, actions):
        logging.debug("GK HAS BALL")

        feed_spot = self.feed_pass(self.my_gk)
        if feed_spot is not None:
            actions.append(self.build_action_kick(self.my_gk_number, feed_spot[0], feed_spot[1], angle_margin=self.MF.GK_PASS_ANGLE_MARGIN))
            return

        # We choose a defender and find his position...
        if random.random() < 0.5:
            defender_number = self.player_number_lwb
        else:
            defender_number = self.player_number_rwb

        defender = self.my_team_players_all[defender_number]
        defender_position = Position(position=defender.dynamicState.position)

        # We kick the ball to the defender...
        accurate_kick_speed = DMT.get_pass_speed_for_accuracy(Position(position=self.my_gk.dynamicState.position), defender_position)
        actions.append(self.build_action_kick(self.my_gk_number, defender_position, accurate_kick_speed, angle_margin=self.MF.GK_PASS_ANGLE_MARGIN))

        # TBI
        logging.info("Goalkeeper kicks the ball to (%1.2f, %1.2f)" % (defender_position.x, defender_position.y))


    # The goalkeeper tries to take possession of the ball.
    def get_action_gk_ball_in_goal_area(self, actions):
        # If we are within 5m of the ball, we try to take possession.
        # Otherwise we move towards the ball.
        goal_centre = self.get_goal_centre(GoalType.OUR_GOAL)
        ball_position = Position(position=self.ball.position)
        gk_pos = Position(position=self.my_gk.dynamicState.position)

        distance = gk_pos.distance_from(ball_position)
        if distance < self.MF.WHEN_GK_TO_TAKE_POSSESSION:
            # We are close to the ball, so we try to take possession...
            logging.debug("GK --- is trying to take possession as dist is [%1.2f]"%distance)
            actions.append(self.build_action_take_possession(self.my_gk_number))
            return
        else:
            ball_dest = DMT.where_ball_ends_up(self.ball)
            if abs(ball_dest.x - goal_centre.x) < 1: #if it's shoot
                if (self.pitch.goalY1 - 1) < ball_dest.y < (self.pitch.goalY2 + 1):
                #If it's effective shoot
                    x_dist = gk_pos.x - ball_position.x
                    if self.ball.vector.x == 0:
                        actions.append(self.build_action_move(self.my_gk_number, ball_position))
                        return
                    y_dist = (self.ball.vector.y / self.ball.vector.x) * x_dist
                    gk_dest = Position(x=gk_pos.x, y=(ball_position.y+y_dist))
                    actions.append(self.build_action_move(self.my_gk_number, gk_dest))
                    logging.debug("GK --- Eff_Sht (in GA) -- GK (%1.2f, %1.2f) is moving to (%1.2f, %1.2f) " % (self.my_gk.dynamicState.position.x, self.my_gk.dynamicState.position.y ,gk_dest.x,gk_dest.y))
                    return

        actions.append(self.build_action_move(self.my_gk_number, ball_position))

    # The goalkeeper keeps between the ball and the goal.
    def get_action_gk_ball_outside_goal_area(self, actions):
        goal_centre = self.get_goal_centre(GoalType.OUR_GOAL)
        ball_position = Position(position=self.ball.position)
        vector = Vector(pos_from=goal_centre, pos_to=ball_position)
        gk_pos = Position(position=self.my_gk.dynamicState.position)
        vector_5m = vector.get_scaled_vector(5.0)
        move_to = goal_centre.get_position_plus_vector_for_player(vector_5m)

        # logging.debug("GK --- START! (outside_goal_area) ")
        if self.ball.controllingPlayerNumber == -1:
            ball_dest = DMT.where_ball_ends_up(self.ball)
            logging.debug("GK --- ball_dest (%1.2f, %1.2f) " %(ball_dest.x, ball_dest.y))
            if self.position_is_in_our_goal_area(ball_dest):
                logging.debug("GK --- ball_pos(%1.2f, %1.2f) ball_dest(%1.2f, %1.2f) GC(%1.2f, %1.2f)" %(ball_position.x, ball_position.y, ball_dest.x, ball_dest.y, goal_centre.x, goal_centre.y))
                logging.debug("GK --- isThis Eff_sht? ball_dest.x-goal_centr:[%1.2f] pitchY1-2[%1.2f] < ball_dest_Y[%1.2f] < pitchY2+2[%1.2f]"%(abs(ball_dest.x - goal_centre.x), (self.pitch.goalY1 - 2), ball_dest.y, (self.pitch.goalY2 + 2)))
                if abs(ball_dest.x - goal_centre.x) < 1: # If it's shoot
                    if (self.pitch.goalY1 - 1) < ball_dest.y < (self.pitch.goalY2 + 1):
                    #If it's effective shoot
                        x_dist = gk_pos.x - ball_position.x
                        if self.ball.vector.x == 0:
                            actions.append(self.build_action_move(self.my_gk_number, move_to))
                            return

                        y_dist = (self.ball.vector.y / self.ball.vector.x) * x_dist
                        gk_dest = Position(x=gk_pos.x, y=(ball_position.y+y_dist))

                        actions.append(self.build_action_move_and_take(self.my_gk_number, gk_dest, ball_position, self.MF.WHEN_GK_TO_TAKE_POSSESSION))
                        logging.debug("GK --- Eff_Sht -- GK (%1.2f, %1.2f) is moving to (%1.2f, %1.2f) " % (self.my_gk.dynamicState.position.x, self.my_gk.dynamicState.position.y ,gk_dest.x,gk_dest.y))
                        return

        elif self.ball.controllingPlayerNumber in self.opposing_team_player_numbers:
            logging.debug("#GK_ANGLE_NARROWER Try")
            opp = self.opposing_team_players_all[self.ball.controllingPlayerNumber]
            opp_pos = Position(position=opp.dynamicState.position)
            our_goal_centre = self.get_goal_centre(GoalType.OUR_GOAL)
            v_to_boundary = Vector(pos_from=our_goal_centre, pos_to=opp_pos).get_scaled_vector(self.pitch.goalAreaRadius)
            cut_pnt = our_goal_centre.get_position_plus_vector_for_player(v_to_boundary)

            gk_time_to_cut_pnt = DMT.get_actual_moving_time(self.my_gk, cut_pnt)
            op_time_to_cut_pnt = DMT.get_actual_moving_time(opp, cut_pnt)
            logging.debug("#gk: gk_time2cp:{}, op_time2cp:{}".format(gk_time_to_cut_pnt, op_time_to_cut_pnt))
            if gk_time_to_cut_pnt >= op_time_to_cut_pnt:
                logging.debug("#GK_ANGLE_NARROWER Successs!!")
                actions.append(self.build_action_move_and_take(self.my_gk_number, cut_pnt, ball_position, self.MF.WHEN_GK_TO_TAKE_POSSESSION))


        logging.debug("GK Default Action")
        # We find the position 5m from the goal-centre...
        # We move to this position...
        actions.append(self.build_action_move(self.my_gk_number, move_to))


    # The player tries to get the ball.
    def get_action_own_half(self, actions, player):
        # Is the ball already controlled by someone in our own team?
        if self.ball.controllingPlayerNumber in self.my_team_players_all:
            # The ball is already controlled by this team...
            return

        # The ball is not controlled by us, so we try to take possession...
        player_number = player.staticState.playerNumber

        # If we are less than 5m from the ball, we try to take possession.
        # If we are further away, we move towards it.
        player_position = Position(position=Position(position=player.dynamicState.position))
        ball_position = Position(position=self.ball.position)
        distance = player_position.distance_from(ball_position)
        if distance < self.MF.WHEN_TO_TAKE_POSSESSION:
            # We attempt to take possession of the ball...
            actions.append(self.build_action_take_possession(player_number))
        else:
            # We move towards the ball...
            actions.append(self.build_action_move(player_number, ball_position))



    # We request semi-random abilities for our players.
    def process_request_configure_abilities(self, data):

        # +1 for the goalkeeper
        # number_of_players = len(self.my_team_players) + 1
        sum_kicking_ability = self.MF.AB_CB_KICK + self.MF.AB_WB_KICK*2 + self.MF.AB_LFW_KICK + self.MF.AB_RFW_KICK + self.MF.AB_GK_KICK
        sum_running_ability = self.MF.AB_CB_RUN + self.MF.AB_WB_RUN*2 + self.MF.AB_LFW_RUN + self.MF.AB_RFW_RUN + self.MF.AB_GK_RUN
        sum_ball_control_ability = self.MF.AB_CB_BALL_CTRL + self.MF.AB_WB_BALL_CTRL*2 + self.MF.AB_LFW_BALL_CTRL + self.MF.AB_RFW_BALL_CTRL + self.MF.AB_GK_BALL_CTRL
        sum_tackling_ability = self.MF.AB_CB_TACKLE + self.MF.AB_WB_TACKLE*2 + self.MF.AB_LFW_TACKLE + self.MF.AB_RFW_TACKLE + self.MF.AB_GK_TACKLE

        if sum_kicking_ability != data.totalKickingAbility:
            raise ValueError("sum_kick_abi is not same as total {}".format(sum_kicking_ability))
        if sum_running_ability != data.totalRunningAbility:
            raise ValueError("sum_run_abi is not same as total {}".format(sum_running_ability))
        if sum_ball_control_ability != data.totalBallControlAbility:
            raise ValueError("sum_ball_ctrl_abi is not same as total {}".format(sum_ball_control_ability))
        if sum_tackling_ability != data.totalTacklingAbility:
            raise ValueError("sum_tackle_abi is not same as total {}".format(sum_tackling_ability))


        # We create the reply...
        reply = JSObject()
        reply.add("requestType", "CONFIGURE_ABILITIES")

        # We give each player a random ability based around the average...
        # variation = 30.0

        player_infos = []
        for player_number in self.my_team_players_all:
            player_info = JSObject()
            player_info.add("playerNumber", player_number)
            if player_number == self.player_number_cb:
                player_info.add("kickingAbility", self.MF.AB_CB_KICK)
                player_info.add("runningAbility", self.MF.AB_CB_RUN)
                player_info.add("ballControlAbility", self.MF.AB_CB_BALL_CTRL)
                player_info.add("tacklingAbility", self.MF.AB_CB_TACKLE)
            elif player_number in self.wb_numbers:
                player_info.add("kickingAbility", self.MF.AB_WB_KICK)
                player_info.add("runningAbility", self.MF.AB_WB_RUN)
                player_info.add("ballControlAbility", self.MF.AB_WB_BALL_CTRL)
                player_info.add("tacklingAbility", self.MF.AB_WB_TACKLE)
            elif player_number == self.player_number_lwf:
                player_info.add("kickingAbility", self.MF.AB_LFW_KICK)
                player_info.add("runningAbility", self.MF.AB_LFW_RUN)
                player_info.add("ballControlAbility", self.MF.AB_LFW_BALL_CTRL)
                player_info.add("tacklingAbility", self.MF.AB_LFW_TACKLE)
            elif player_number == self.player_number_rwf:
                player_info.add("kickingAbility", self.MF.AB_RFW_KICK)
                player_info.add("runningAbility", self.MF.AB_RFW_RUN)
                player_info.add("ballControlAbility", self.MF.AB_RFW_BALL_CTRL)
                player_info.add("tacklingAbility", self.MF.AB_RFW_TACKLE)
            elif player_number == self.my_gk_number:
                player_info.add("kickingAbility", self.MF.AB_GK_KICK)
                player_info.add("runningAbility", self.MF.AB_GK_RUN)
                player_info.add("ballControlAbility", self.MF.AB_GK_BALL_CTRL)
                player_info.add("tacklingAbility", self.MF.AB_GK_TACKLE)
            else:
                raise ValueError("What number is this then?")

            player_infos.append(player_info)

        reply.add("players", player_infos)

        self.send_reply(reply)


    # Called when we receive the kickoff request.
    # We reply with the positions we set up (for the current playing direction)
    # set up (in onTeamInfoUpdated) above.
    def process_request_kickoff(self):
        # We create the reply...
        reply = JSObject()
        reply.add("requestType", "KICKOFF")

        players = []
        for pos_key, pos_value in self.kickoff_positions.items():
            # Get first element of pair (PositionKey cls)
            player_number = pos_key[0]
            playing_direction = pos_key[1]

            # We only want values for this kickoff's paying direction...
            if playing_direction == self.playing_direction:
                player = JSObject()
                player.add("playerNumber", player_number)
                player.add("position", pos_value.position)
                # player.add("position", pos_dict)
                player.add("direction", pos_value.direction)
                players.append(player)

        reply.add("players", players)

        self.send_reply(reply)

    # Called when team-info has been updated.
    # At this point we know the player-numbers for the players
    # in our team, so we can choose which players are playing in
    # which positions.
    def on_team_info_updated(self):
        # We assign players to the positions...
        player_numbers = []
        for key in self.my_team_players:
            player_numbers.append(key)

        self.player_number_lwb = player_numbers[0]
        self.player_number_cb = player_numbers[1]
        self.player_number_rwb = player_numbers[2]
        self.player_number_lwf = player_numbers[3]
        self.player_number_rwf = player_numbers[4]

        self.wb_numbers.append(self.player_number_lwb)
        self.wb_numbers.append(self.player_number_rwb)
        self.fw_numbers.append(self.player_number_lwf)
        self.fw_numbers.append(self.player_number_rwf)

        lwb = Position(23, 12)
        cb = Position(38, 25)
        rwb = Position(20, 37)
        lwf = Position(73, 14)
        rwf = Position(68, 38)

        lwf_ko = Position(49, 14)
        rwf_ko = Position(49, 36)

        # We set up the dflt positions...
        self.set_dflt_position(self.player_number_lwb, DirectionType.RIGHT, Position(position=lwb), 90)
        self.set_dflt_position(self.player_number_cb, DirectionType.RIGHT, Position(position=cb), 90)
        self.set_dflt_position(self.player_number_rwb, DirectionType.RIGHT, Position(position=rwb), 90)
        self.set_dflt_position(self.player_number_lwf, DirectionType.RIGHT, Position(position=lwf), 90)
        self.set_dflt_position(self.player_number_rwf, DirectionType.RIGHT, Position(position=rwf), 90)

        self.set_dflt_position(self.player_number_lwb, DirectionType.LEFT, Position(100-lwb.x, 50-lwb.y), 270)
        self.set_dflt_position(self.player_number_cb, DirectionType.LEFT, Position(100-cb.x, 50-cb.y), 270)
        self.set_dflt_position(self.player_number_rwb, DirectionType.LEFT, Position(100-rwb.x, 50-rwb.y), 270)
        self.set_dflt_position(self.player_number_lwf, DirectionType.LEFT, Position(100-lwf.x, 50-lwf.y), 270)
        self.set_dflt_position(self.player_number_rwf, DirectionType.LEFT, Position(100-rwf.x, 50-rwf.y), 270)

        # We set up the kickoff positions.
        # Note: The player nearest the centre will be automatically "promoted"
        #       to the centre by the game.
        self.set_kickoff_position(self.player_number_lwb, DirectionType.RIGHT, Position(position=lwb), 90)
        self.set_kickoff_position(self.player_number_cb, DirectionType.RIGHT, Position(x=cb.x-15, y=cb.y), 90)
        self.set_kickoff_position(self.player_number_rwb, DirectionType.RIGHT, Position(position=rwb), 90)
        self.set_kickoff_position(self.player_number_lwf, DirectionType.RIGHT, Position(position=lwf_ko), 90)
        self.set_kickoff_position(self.player_number_rwf, DirectionType.RIGHT, Position(position=rwf_ko), 90)

        self.set_kickoff_position(self.player_number_lwb, DirectionType.LEFT, Position(100-lwb.x, 50-lwb.y), 270)
        self.set_kickoff_position(self.player_number_cb, DirectionType.LEFT, Position(100-cb.x+15, 50-cb.y), 270)
        self.set_kickoff_position(self.player_number_rwb, DirectionType.LEFT, Position(100-rwb.x, 50-rwb.y), 270)
        self.set_kickoff_position(self.player_number_lwf, DirectionType.LEFT, Position(100-lwf_ko.x, 50-lwf_ko.y), 270)
        self.set_kickoff_position(self.player_number_rwf, DirectionType.LEFT, Position(100-rwf_ko.x, 50-rwf_ko.y), 270)

    def get_pass_receiver_ready(self, actions):
        action = actions[-1]
        if action.get_value("action") == "KICK":
            dest = Position(x=action.get_value("destination")["x"], y=action.get_value("destination")["y"])

            if 0 < dest.x < 100: # if it's not shoot, near player escort the pass
                near_player = DMT.get_nearby_player(dest, self.my_team_players, excl_player_number=action.get_value("playerNumber"))[1]
                # if it's pass to temmate, the receiver shd face the ball to escort
                # if it's thru pass, the player shd face the escort point (ep)
                if dest.distance_from(near_player.dynamicState.position) < 1:
                    logging.debug("Got Signal for Pass, p[%d] is facing the ball" %near_player.staticState.playerNumber)
                    ball_pos = Position(position=self.ball.position)
                    if ball_pos.is_position_in_goal_area():
                        actions.append(self.build_action_turn(near_player.staticState.playerNumber, Vector(pos_from=Position(position=near_player.dynamicState.position), pos_to=self.ball.position).direction))
                    else:
                        actions.append(self.build_action_move_and_take(near_player.staticState.playerNumber, ball_pos, ball_pos, self.MF.WHEN_TO_TAKE_POSSESSION))
                else:
                    ep = DMT.where_to_move_to_get_the_free_ball(near_player, self.ball)
                    if ep is not None:
                        logging.debug("Got Signal for Pass, p[%d] is facing EP" %near_player.staticState.playerNumber)
                        actions.append(self.build_action_move_and_take(near_player.staticState.playerNumber, ep, self.ball.position, self.MF.WHEN_TO_TAKE_POSSESSION))
                    else:
                        logging.debug("Got Signal for Pass, p[%d] is facing the ball_dest" %near_player.staticState.playerNumber)
                        actions.append(self.build_action_move_and_take(near_player.staticState.playerNumber, dest, self.ball.position, self.MF.WHEN_TO_TAKE_POSSESSION))

                return near_player.staticState.playerNumber

        return -1

    def get_shoot_rebounder_ready(self, actions):
        action = actions[-1]
        if action.get_value("action") == "KICK":
            dest = Position(x=action.get_value("destination")["x"], y=action.get_value("destination")["y"])

            shoot_at = self.get_goal_centre(GoalType.THEIR_GOAL)
            if dest.x == shoot_at.x:
                near_player = DMT.get_nearby_player(dest, self.my_team_players, excl_player_number=action.get_value("playerNumber"))[1]
                near_p_pos = Position(position=near_player.dynamicState.position)
                v_to_near_p = Vector(pos_from=dest, pos_to=near_p_pos)
                new_v = v_to_near_p.get_scaled_vector(self.pitch.goalAreaRadius + 1)
                new_dest = shoot_at.get_position_plus_vector_for_player(new_v)
                actions.append(self.build_action_move_and_take(near_player.staticState.playerNumber, new_dest, self.ball.position, self.MF.WHEN_TO_TAKE_POSSESSION))
                logging.debug("P[{}] is trying rebound!".format(near_player.staticState.playerNumber))
                return near_player.staticState.playerNumber, new_dest

        return ()

    # True if the ball is in our goal-area, false if not.
    def ball_is_in_our_goal_area(self):

        ball_position = Position(position=self.ball.position)
        goal_centre = self.get_goal_centre(GoalType.OUR_GOAL)
        return ball_position.distance_from(goal_centre) < self.pitch.goalAreaRadius

    def position_is_in_our_goal_area(self, pos):

        position = Position(position=pos)
        goal_centre = self.get_goal_centre(GoalType.OUR_GOAL)
        return position.distance_from(goal_centre) < self.pitch.goalAreaRadius

    # chk
    # Sets the dflt position for (player, direction).
    def set_dflt_position(self, player_number, playing_direction, position, direction):

        key = GohAI.PositionKey((player_number, playing_direction))
        value = GohAI.PositionValue(position, direction)
        self.dflt_positions[key] = value

    # Returns the dflt position for the player passed in, when playing
    # in the direction passed in
    def get_dflt_position(self, player_number, playing_direction):
        key = GohAI.PositionKey((player_number, playing_direction))
        return self.dflt_positions[key]

    # Sets the kickoff position for (player, direction).
    def set_kickoff_position(self, player_number, playing_direction, position, direction):

        key = GohAI.PositionKey((player_number, playing_direction))
        value = GohAI.PositionValue(position, direction)
        self.kickoff_positions[key] = value

    def get_player_role_type(self, player_num):
        if player_num in self.fw_numbers:
            return RoleType.FW
        elif player_num in self.wb_numbers:
            return RoleType.WB
        elif player_num == self.player_number_cb:
            return RoleType.CB
        elif player_num == self.my_gk_number:
            return RoleType.GK
        else:
            raise ValueError("What's the role of [%d] then ?" % player_num)

    # Returns the dflt position for (player, direction).
    def get_kickoff_position(self, player_number, playing_direction):
        key = GohAI.PositionKey((player_number, playing_direction))
        return self.kickoff_positions[key]

    @property
    def player_turning_to_kick(self):
        return self._player_turning_to_kick

    @player_turning_to_kick.setter
    def player_turning_to_kick(self, tp):
        self._player_turning_to_kick = tp

    @property
    def ball_is_passed_to(self):
        return self._ball_is_passed_to

    @ball_is_passed_to.setter
    def ball_is_passed_to(self, player_number):
        self._ball_is_passed_to = player_number



    def safe_pass_cb(self, this_player):
        logging.debug("CB -safe_pass_method_start trial ------------------------")
        my_number = this_player.staticState.playerNumber
        this_player_pos = Position(position=this_player.dynamicState.position)
        # my_pos = Position(this_player.dynamicState.position.x, this_player.dynamicState.position.y)

        free_time_dict = {}
        for teammate_number, teammate in self.my_team_players_all.items():
            if teammate_number == my_number:
                continue
            teammate_pos = Position(position=teammate.dynamicState.position)
            accurate_ball_speed = DMT.get_pass_speed_for_accuracy(this_player_pos, teammate_pos) * 0.3
            free_time = DMT.free_time_for_safe_pass(this_player, teammate, self.opposing_team_players, accurate_ball_speed)#tbc
            logging.debug("---- For player{}, accurate_ball_spd {}, free_time {}".format(teammate_number, accurate_ball_speed, free_time))
            if free_time > 0:
                if DMT.can_pass_cut_by_opp(this_player_pos, teammate_pos, accurate_ball_speed, self.opposing_team_players):
                    logging.debug("---------------- will be cut though..")
                    continue
                else:
                    free_time_dict[free_time] = teammate_number

        dict_len = len(free_time_dict)
        if dict_len == 0:
            return -1
        elif dict_len == 1:
            for key, value in free_time_dict.items():
                if value == self.my_gk_number:
                    return -1
                else:
                    return value
        else:
            for tp in sorted(free_time_dict.items(), reverse=True):
                logging.debug("Sorted by free_time : teammate[%d], time[%1.2f]" % (tp[1], tp[0]))
                if tp[1] in self.fw_numbers:
                    logging.debug("Passing to forward!")
                    return tp[1]

            for tp in sorted(free_time_dict.items(), reverse=True):
                if tp[1] == self.my_gk_number:
                    logging.debug("Let's skip GK")
                    continue

                return tp[1]

            return -1

    def safe_pass_wb(self, this_player):

        logging.debug("WB -safe_pass_method_start trial ------------------------")
        my_number = this_player.staticState.playerNumber
        this_player_pos = Position(position=this_player.dynamicState.position)
        # my_pos = Position(this_player.dynamicState.position.x, this_player.dynamicState.position.y)

        free_time_dict = {}
        for teammate_number, teammate in self.my_team_players_all.items():
            if teammate_number == my_number:
                continue
            teammate_pos = Position(position=teammate.dynamicState.position)
            accurate_ball_speed = DMT.get_pass_speed_for_accuracy(this_player_pos, teammate_pos) * 0.3
            free_time = DMT.free_time_for_safe_pass(this_player, teammate, self.opposing_team_players, accurate_ball_speed)#tbc
            logging.debug("---- For player{}, accurate_ball_spd {}, free_time {}".format(teammate_number, accurate_ball_speed, free_time))
            if free_time > 0:
                if DMT.can_pass_cut_by_opp(this_player_pos, teammate_pos, accurate_ball_speed, self.opposing_team_players):
                    logging.debug("---------------- will be cut though..")
                    continue
                else:
                    free_time_dict[free_time] = teammate_number

        dict_len = len(free_time_dict)
        if dict_len == 0:
            return -1
        elif dict_len == 1:
            for key, value in free_time_dict.items():
                if value == self.my_gk_number and DMT.is_in_offense_area(this_player.dynamicState.position, self.playing_direction, self.pitch.centreSpot):
                    logging.debug("Don't pass to GK when it's in offense area")
                    return -1
                else:
                    return value
        else:
            for tp in sorted(free_time_dict.items(), reverse=True):
                logging.debug("Sorted by free_time : teammate[%d], time[%1.2f]" % (tp[1], tp[0]))
                if tp[1] in self.fw_numbers:
                    logging.debug("Passing to forward!")
                    return tp[1]

            for tp in sorted(free_time_dict.items(), reverse=True):
                if tp[1] == self.my_gk_number:
                    logging.debug("Let's skip GK")
                    continue
                elif tp[1] == self.wb_numbers:
                    logging.debug("Let's skip WB")
                    continue
                return tp[1]

            return -1


    def safe_pass_fw(self, this_player):
        logging.debug("FW -safe_pass_method_start trial ------------------------")
        my_number = this_player.staticState.playerNumber
        this_player_pos = Position(position=this_player.dynamicState.position)
        # my_pos = Position(this_player.dynamicState.position.x, this_player.dynamicState.position.y)

        free_time_dict = {}
        for teammate_number, teammate in self.my_team_players_all.items():
            if teammate_number == my_number:
                continue
            teammate_pos = Position(position=teammate.dynamicState.position)
            accurate_ball_speed = DMT.get_pass_speed_for_accuracy(this_player_pos, teammate_pos) * 0.3
            free_time = DMT.free_time_for_safe_pass(this_player, teammate, self.opposing_team_players, accurate_ball_speed)#tbc
            logging.debug("---- For player{}, accurate_ball_spd {}, free_time {}".format(teammate_number, accurate_ball_speed, free_time))
            if free_time > 0:
                if DMT.can_pass_cut_by_opp(this_player_pos, teammate_pos, accurate_ball_speed, self.opposing_team_players):
                    logging.debug("---------------- will be cut though..")
                    continue
                else:
                    free_time_dict[free_time] = teammate_number

        dict_len = len(free_time_dict)
        if dict_len == 0:
            return -1
        elif dict_len == 1:
            for key, value in free_time_dict.items():
                if value == self.my_gk_number:
                    return -1
                else:
                    return value
        else:
            for tp in sorted(free_time_dict.items(), reverse=True):
                logging.debug("Sorted by free_time : teammate[%d], time[%1.2f]" % (tp[1], tp[0]))
                if tp[1] in self.fw_numbers:
                    logging.debug("Passing to forward!")
                    return tp[1]

            for tp in sorted(free_time_dict.items(), reverse=True):
                if tp[1] == self.my_gk_number:
                    logging.debug("Let's skip GK")
                    continue
                # Pass only when p's in offense area
                if DMT.is_in_offense_area(self.my_team_players[tp[1]].dynamicState.position, self.playing_direction, self.pitch.centreSpot):
                    return tp[1]

            return -1

    def try_move_forward(self, actions, player, role, dest_pos):
        player_pos = Position(position=player.dynamicState.position)
        player_number = player.staticState.playerNumber
        shoot_at = self.get_goal_centre(GoalType.THEIR_GOAL)
        shoot_vector = Vector(pos_from=player_pos, pos_to=shoot_at)
        turning_time_for_shoot = shoot_vector.turning_time(direction=player.dynamicState.direction)

        my_next_pos = DMT.player_will_be_at(dest_pos, player)
        logging.debug("#### MOVE_FW D: p[%d] cur_pos:(%1.2f, %1.2f), dest_pos (%1.2f, %1.2f), next_pos will be (%1.2f, %1.2f)"%(player_number, player_pos.x, player_pos.y, dest_pos.x, dest_pos.y, my_next_pos.x, my_next_pos.y))
        nearby_info = DMT.get_nearby_tackling_opponent(player_pos, self.opposing_team_players)
        nearby_info2 = DMT.get_nearby_tackling_opponent(my_next_pos, self.opposing_team_players)
        opp_next_pos = DMT.player_will_be_at(self.ball.position, nearby_info2[1])
        next_tackle_time = my_next_pos.will_be_tackled_within(opp_next_pos, nearby_info2[1].staticState.runningAbility)
        if (role == RoleType.CB or role == RoleType.FW) \
            and DMT.is_in_shooting_range(player_pos, self.get_goal_centre(GoalType.THEIR_GOAL), role):
            turning_time = turning_time_for_shoot + 0.1
        else:
            if role == RoleType.CB:
                turning_time = self.MF.CB_DFLT_KICK_TURNING_TIME
            elif role == RoleType.WB:
                turning_time = self.MF.WB_DFLT_KICK_TURNING_TIME
            elif role == RoleType.FW:
                turning_time = self.MF.FW_DFLT_KICK_TURNING_TIME
            else:
                raise ValueError("Who else is tying move forward? {}".format(role))
        logging.info("##### MOVE_FWD RSLT #########: 1st_tckl_tim: %1.2f: nxt_tckl_tm: %1.2f, turn_time [%1.2f]" % (nearby_info[0], next_tackle_time, turning_time))
        if nearby_info[0] > turning_time and next_tackle_time > turning_time:
            # destination = Position(dribble_limit_x, my_pos.y)
            actions.append(self.build_action_move(player_number, dest_pos))
            logging.debug("")
            logging.info("####: p[%d] dribble ~~~~~~~~~ to (%1.2f, %1.2f)" % (player_number, dest_pos.x, dest_pos.y))
            return True

        return False

    def try_kill_pass(self, feeder):
        feeder_num = feeder.staticState.playerNumber
        feeder_pos = Position(position=feeder.dynamicState.position)
        logging.debug(" %%% KillPass Trial Start!")
        shoot_at = self.get_goal_centre(GoalType.THEIR_GOAL)
        for baby_num in self.fw_numbers:
            if baby_num == feeder_num:
                continue
            baby = self.my_team_players[baby_num]
            baby_pos = Position(position=baby.dynamicState.position)

            goal_to_baby_vector = Vector(pos_from=shoot_at, pos_to=baby_pos)
            for x in range(1, 25, 1):
                dist = 17.5 + x * 0.5
                kp_vector = goal_to_baby_vector.get_scaled_vector(dist)
                kill_point = shoot_at.get_position_plus_vector_for_player(kp_vector)

                accurate_ball_speed = DMT.get_pass_speed_for_accuracy(feeder_pos, kill_point) * 0.3
                if DMT.can_pass_cut_by_opp(feeder_pos, kill_point, accurate_ball_speed, self.opposing_team_players):
                    continue

                nearby_tp = DMT.get_nearby_tackling_opponent(kill_point, self.opposing_team_players)
                baby_mov_time = DMT.get_actual_moving_time(baby, kill_point)
                free_time = nearby_tp[0] - baby_mov_time
                turn_time = DMT.get_kick_turning_time(baby_pos, shoot_at, baby.dynamicState.direction)

                logging.debug(" %%% KillPass ---- fw[{}] : kp:({},{}), opp:{} tkl_time:{}, bby_mov_tm:{} = free_tm:{} > turn_time:{} ?"
                              .format(baby_num, kill_point.x, kill_point.y, nearby_tp[1].staticState.playerNumber, nearby_tp[0], baby_mov_time, free_time, turn_time ))
                if free_time > turn_time:
                    logging.debug(" %%% KillPasss SUCCESSFUL! %%%")
                    return kill_point, round(accurate_ball_speed * 3.33333, 5)

        return None

    def try_safe_pass(self, actions, player, role, time_to_get_tackle, angle_margin):
        player_pos = Position(position=player.dynamicState.position)
        player_number = player.staticState.playerNumber
        if role == RoleType.CB:
            teammate_number_to_pass = self.safe_pass_cb(player)
        elif role == RoleType.WB:
            teammate_number_to_pass = self.safe_pass_wb(player)
        elif role == RoleType.FW:
            teammate_number_to_pass = self.safe_pass_fw(player)
        else:
            raise ValueError("This role {} wouldn't try SAFE PASS".format(role))

        if teammate_number_to_pass >= 0:
            logging.debug("#### safepass tyrial_trial---------------------")
            teammate = self.my_team_players_all[teammate_number_to_pass]
            time_to_turn_for_pass = Vector(pos_from=player_pos, pos_to=teammate.dynamicState.position).turning_time(direction=player.dynamicState.direction)
            logging.debug("#### SFPS Time_to_get_tackle %1.2f, time_to_turn %1.2f" %(time_to_get_tackle, time_to_turn_for_pass))
            if time_to_get_tackle > time_to_turn_for_pass:
                destination = Position(position=teammate.dynamicState.position)
                accurate_kick_speed = DMT.get_pass_speed_for_accuracy(player_pos, destination)
                actions.append(self.build_action_kick(player_number, destination, accurate_kick_speed, angle_margin=angle_margin))
                kick_vector = Vector(pos_from=player_pos, pos_to=destination)
                logging.info("#### SFPS P[%d] kicks the ball to teammate[%d](%1.2f, %1.2f) my_dir:[%1.2f], kick_vector_dir:[%1.2f], angle_dif[%1.2f]" % (player_number, teammate_number_to_pass, destination.x, destination.y, player.dynamicState.direction, kick_vector.direction, kick_vector.angle_from(direction=player.dynamicState.direction)))
                return True
            else:
                logging.info("#### SFPS Failed due to time limit")

        return False

    def feed_pass(self, feeding_player):
        feeding_player_num = feeding_player.staticState.playerNumber
        feeding_player_pos = Position(position=feeding_player.dynamicState.position)
        logging.debug(" %%% FeedPass Trial Start!")
        if self.playing_direction == DirectionType.LEFT:
            spots_list = self.PC.pitch_spots_left
        else:
            spots_list = self.PC.pitch_spots_right

        # if self.turn_count % 100 == 0:
        #     random.shuffle(spots_list)

        for pos_tp in spots_list:
            spot = Position(x=pos_tp[0], y=pos_tp[1])
            if feeding_player_pos.is_this_pos_behind(spot, self.playing_direction, how_far=15):
                continue
            accurate_ball_speed = DMT.get_pass_speed_for_accuracy(feeding_player_pos, spot) * 0.3
            if DMT.can_pass_cut_by_opp(feeding_player_pos, spot, accurate_ball_speed, self.opposing_team_players):
                continue
            receiver = DMT.get_nearby_player(spot, self.my_team_players, excl_player_number=feeding_player_num)
            opp_receiver = DMT.get_nearby_tackling_opponent(spot, self.opposing_team_players)

            free_time = opp_receiver[0] - receiver[0]
            turn_time = DMT.get_kick_turning_time(feeding_player_pos, spot, feeding_player.dynamicState.direction)

            # self.MF.get_dflt_kick_turning_time(self.get_player_role_type(receiver[1].staticState.playerNumber)):
            logging.debug(" &&&&&&& FeedPass Rcvr[%d] min time:[%1.2f] Opp[%d] min_time [%1.2f] free_time [%1.2f], turn_time [%1.2f]" %(receiver[1].staticState.playerNumber, receiver[0], opp_receiver[1].staticState.playerNumber, opp_receiver[0], free_time, turn_time))
            if free_time > turn_time:
                logging.debug("&&& FeedPass : p[%d] passes to spot(%1.2f, %1.2f) and p[%d] will be getting it" %(feeding_player_num ,spot.x, spot.y, receiver[1].staticState.playerNumber))
                return spot, round(accurate_ball_speed * 3.33333, 5)

        return None

