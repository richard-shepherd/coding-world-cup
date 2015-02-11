from __future__ import division
import logging

# The direction we are playing...

class MarginFactors(object):

    CB_OVERLAP_TO_RIGHT_X = 75
    CB_OVERLAP_TO_LEFT_X = 25
    WB_OVERLAP_TO_RIGHT_X = 50
    WB_OVERLAP_TO_LEFT_X = 50

    CB_PASS_ANGLE_MARGIN = 5
    WB_PASS_ANGLE_MARGIN = 5
    FW_PASS_ANGLE_MARGIN = 10
    GK_PASS_ANGLE_MARGIN = 0.05

    CB_SHOOT_RANGE = 32
    FW_SHOOT_RANGE = 26
    FW_NO_TIME_SHOOT_RANGE = 33


    FW_KICK_ANGLE_MARGIN = 15

    TACKLE_DISTANCE = 1.0
    DFLT_KICK_TURNING_TIME = 0.0

    CB_DFLT_KICK_TURNING_TIME = 0.25
    WB_DFLT_KICK_TURNING_TIME = 0.2 #120 degree
    FW_DFLT_KICK_TURNING_TIME = 0.2

    SHOOT_ANGLE_LIMIT = 20
    ACC_BALL_SPEED_FACTOR = 1.05
    WHEN_TO_TAKE_POSSESSION = 1.0
    WHEN_GK_TO_TAKE_POSSESSION = 5.0

    CB_HOLD_AREA = 20
    WB_MARKING_DISTANCE = 8.0

    GA_DETOUR_ANGLE = 45

    DANGEROUS_AREA_DIST = 25

    ## Config Abilities
    AB_CB_KICK = 90
    AB_WB_KICK = 50
    AB_LFW_KICK = 95
    AB_RFW_KICK = 85
    AB_GK_KICK = 30

    AB_CB_RUN = 50
    AB_WB_RUN = 75
    AB_LFW_RUN = 90
    AB_RFW_RUN = 50
    AB_GK_RUN = 60

    AB_CB_BALL_CTRL = 60
    AB_WB_BALL_CTRL = 70
    AB_LFW_BALL_CTRL = 60
    AB_RFW_BALL_CTRL = 50
    AB_GK_BALL_CTRL = 90

    AB_CB_TACKLE = 100
    AB_WB_TACKLE = 100
    AB_LFW_TACKLE = 50
    AB_RFW_TACKLE = 50
    AB_GK_TACKLE = 00


    def get_dflt_kick_turning_time(self, role_type):
        if role_type == RoleType.CB:
            return self.CB_DFLT_KICK_TURNING_TIME
        elif role_type == RoleType.WB:
            return self.WB_DFLT_KICK_TURNING_TIME
        elif role_type == RoleType.FW:
            return self.FW_DFLT_KICK_TURNING_TIME
        elif role_type == RoleType.GK:
            raise ValueError("We don't count GK's kick turning time")
        else:
            raise ValueError("What role is this [%d] ?" % role_type)


    def can_cb_overlap(self, playing_direction, position_x):
        limit_x = self.get_cb_overlap_limit_x(playing_direction)

        if playing_direction == DirectionType.RIGHT:
            if position_x < limit_x:
                return True
            else:
                logging.debug("he can't overlap further")
                return False
        else:
            if position_x > limit_x:
                return True
            else:
                logging.debug("he can't overlap further")
                return False

    def can_wb_overlap(self, playing_direction, position_x):
        limit_x = self.get_wb_overlap_limit_x(playing_direction)

        if playing_direction == DirectionType.RIGHT:
            if position_x < limit_x:
                return True
            else:
                logging.debug("he can't overlap further")
                return False
        else:
            if position_x > limit_x:
                return True
            else:
                logging.debug("he can't overlap further")
                return False

    def get_cb_overlap_limit_x(self, direction):
        if direction == DirectionType.RIGHT:
            return self.CB_OVERLAP_TO_RIGHT_X
        else:
            return self.CB_OVERLAP_TO_LEFT_X

    def get_wb_overlap_limit_x(self, direction):
        if direction == DirectionType.RIGHT:
            return self.WB_OVERLAP_TO_RIGHT_X
        else:
            return self.WB_OVERLAP_TO_LEFT_X

    def can_fw_dribble(self, fw_pos, goal_pos):
        if goal_pos.distance_from(fw_pos) > 15.3:
            return True
        else:
            return False


        # limit_x = self.get_fw_dribble_limit_x(playing_direction)
        #
        # if playing_direction == DirectionType.RIGHT:
        #     if position_x < limit_x:
        #         return limit_x
        #     else:
        #         logging.debug("he can't dribble further")
        #         return -1
        # else:
        #     if position_x > limit_x:
        #         return limit_x
        #     else:
        #         logging.debug("he can't dribble further")
        #         return -1




class DirectionType(object):
    DONT_KNOW = 0
    LEFT = 1
    RIGHT = 2


# An enum for the two goals...
class GoalType(object):
    OUR_GOAL = 0
    THEIR_GOAL = 1


class RoleType(object):
    CB = 0
    WB = 1
    FW = 2
    GK = 3
    FW_NT = 4


class PitchCollection(object):
    pitch_spots_left = []
    pitch_spots_right = []

    def __init__(self):
        for x in range(15, 45, 3):
            for y in range(48, 2, -3):
                self.pitch_spots_left.append((x, y))

        for x in range(85, 55, -3):
            for y in range(3, 50, 3):
                self.pitch_spots_right.append((x, y))

