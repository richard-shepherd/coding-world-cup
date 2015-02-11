from __future__ import division
import math
from classes import MarginFactors, DirectionType
import logging


class Position:
    # Holds an (x, y) position, with some helper functions.

    class Pitch(object):
        WIDTH = 100
        HEIGHT = 50
        GOAL_CENTRE = 25
        GOALY1 = 21
        GOALY2 = 29
        CENTRE_SPOT_X = 50
        CENTRE_SPOT_Y = 25
        CENTRE_CIRCLE_RADIUS = 10
        GOAL_AREA_RADIUS = 15

    def __init__(self, x=0.0, y=0.0, position=None):

        if position is None:
            self.x = x
            self.y = y
        else:
            self.x = position.x
            self.y = position.y

    # Returns the distance between two positions.
    def distance_from(self, other):
        diff_x = self.x - other.x
        diff_y = self.y - other.y
        diff_x_squared = diff_x * diff_x
        diff_y_squared = diff_y * diff_y
        distance = math.sqrt(diff_x_squared + diff_y_squared)
        return distance

    def time_from(self, player):
        distance = self.distance_from(player.dynamicState.position)
        player_speed = player.staticState.runningAbility
        if player_speed <= 0:
            return 999
        return round(distance / (player_speed / 10), 4)

    def ball_time_from(self, position, ball_speed):

        if ball_speed == 0:
            return 999
        # Distance = V0*t - 5t**2
        distance = self.distance_from(position)
        simple_answer = round(distance / ball_speed, 4)

        a = 5
        b = -ball_speed
        c = distance
        d = (b**2) - (4*a*c)

        if d < 0:
            return simple_answer

        sol1 = (-b-math.sqrt(d))/(2*a)
        sol2 = (-b+math.sqrt(d))/(2*a)

        # return simple_answer
        if sol1 < 0 or sol2 < 0:
            raise ValueError("We have negative time value")

        if abs(simple_answer - sol1) < abs(simple_answer - sol2):
            return sol1
        else:
            return sol2

    def will_be_tackled_within(self, player_pos, player_running_ability):
        distance = self.distance_from(player_pos) - MarginFactors.TACKLE_DISTANCE

        if distance <= 0:
            return 0
        if player_running_ability <= 0:
            return 0

        return round(distance / (player_running_ability / 10), 4)

    # Returns a new position from this position plus the vector passed in.
    # def get_position_plus_vector(self, vector):
    #     position = Position(self.x + vector.x, self.y + vector.y)
    #     return position

    def get_position_plus_vector_for_ball(self, vector):
        position = Position(self.x + vector.x, self.y + vector.y)
        if position.is_position_in_pitch():
            return position
        else:
            a = vector.x
            b = vector.y
            x_sum = position.x
            y_sum = position.y
            x_red = 0
            y_red = 0

            if a == 0 or b == 0:
                return position

            if x_sum < 0:
                x_red = x_sum
            elif x_sum > Position.Pitch.WIDTH:
                x_red = x_sum - Position.Pitch.WIDTH

            if y_sum < 0:
                y_red = y_sum
            elif y_sum > Position.Pitch.HEIGHT:
                y_red = y_sum - Position.Pitch.HEIGHT

            if x_red == 0: #y is out_of_range
                x_red = (a/b) * y_red
                new_pos = Position(x_sum - x_red, y_sum - y_red)
                new_bounced_pos = Position(x_sum, y_sum - y_red*2)
            elif y_red == 0: #x is out_of_range
                y_red = (b/a) * x_red
                new_pos = Position(x_sum - x_red, y_sum - y_red)
                new_bounced_pos = Position(x_sum - x_red*2, y_sum)
            else: # Both are out_of_range
                if abs(x_red) > abs(y_red):
                    y_red2 = (b/a) * x_red
                    new_pos = Position(x_sum - x_red, y_sum - y_red2)

                else:
                    x_red2 = (a/b) * y_red
                    new_pos = Position(x_sum - x_red2, y_sum - y_red)
                # logging.debug("OOR Bounce Check!: 3coo !!!!")
                new_bounced_pos = Position(x_sum - x_red*2, y_sum - y_red*2)

            if new_pos.is_position_in_goal():
                # logging.debug("OOR Check & MOD : from pos(%1.2f, %1.2f), new_pos(%1.2f, %1.2f) a:%1.2f, b:%1.2f, x_red:%1.2f, y_red:%1.2f" % (position.x, position.y, new_pos.x, new_pos.y, vector.x, vector.y , x_red, y_red))
                return new_pos
            else: # it's bounced, get bounced_pos !
                # logging.debug("OOR Bounce Check!: from pos(%1.2f, %1.2f), new_bounced_pos(%1.2f, %1.2f) a:%1.2f, b:%1.2f, x_red:%1.2f, y_red:%1.2f" % (position.x, position.y, new_bounced_pos.x, new_bounced_pos.y, vector.x, vector.y , x_red, y_red))
                return new_bounced_pos

    def get_position_plus_vector_for_player(self, vector):
        position = Position(self.x + vector.x, self.y + vector.y)
        # if not (0 <= position.x <= 100) or not (0 <= position.y <= 50):
        #     raise ValueError("player dest position is not in pitch (%1.2f ,%1.2f)" % (position.x, position.y))
        return position

    # ToString
    def to_string(self):
        return "x:%1.4f, y:%1.4f" % (self.x, self.y)

    def is_position_in_goal(self):
        if (self.x == 0 or self.x == Position.Pitch.WIDTH) and (Position.Pitch.GOALY1 <= self.y <= Position.Pitch.GOALY2):
            # raise ValueError("No, (%1.2f, %1.2f)s is in Goal  " %(self.x, self.y))
            return True
        else:
            return False

    def is_position_in_goal_area(self):
        # position = Position(position=self)
        goal_centre1 = Position(x=0, y=Position.Pitch.GOAL_CENTRE)
        goal_centre2 = Position(x=Position.Pitch.WIDTH, y=Position.Pitch.GOAL_CENTRE)
        # logging.debug("FromP(%1.2f, %1.2f), gc1 dist[%1.2f], gc2 dist[%1.2f], RADIUS [%d]"%(self.x, self.y, self.distance_from(goal_centre1), self.distance_from(goal_centre2), Position.Pitch.GOAL_CENTRE))
        if self.distance_from(goal_centre1) < Position.Pitch.GOAL_AREA_RADIUS \
            or self.distance_from(goal_centre2) < Position.Pitch.GOAL_AREA_RADIUS:
            return True
        else:
            return False
        # return position.distance_from(goal_centre) < self.Position.Pitch.goalAreaRadius


    def is_position_in_pitch(self):
        if (0 <= self.x <= Position.Pitch.WIDTH) and (0 <= self.y <= Position.Pitch.HEIGHT):
            return True
        else:
            return False

    def is_position_in_player_field(self):
        if self.is_position_in_pitch() and not self.is_position_in_goal_area():
            return True
        else:
            return False
            # raise ValueError("No, (%1.2f, %1.2f)s Not in player_field PFPF" %(self.x, self.y))

    def is_x_in_pitch(self):
        if 0 <= self.x <= Position.Pitch.WIDTH:
            return True
        else:
            return False

    def is_y_in_pitch(self):
        if 0 <= self.y <= Position.Pitch.HEIGHT:
            return True
        else:
            return False

    def is_this_pos_behind(self, other_pos, playing_direction, how_far=0):
        if playing_direction == DirectionType.LEFT:
            if self.x + how_far < other_pos.x:
                return True
            else:
                return False
        elif playing_direction == DirectionType.RIGHT:
            if self.x > other_pos.x + how_far:
                return True
            else:
                return False
        else:
            raise ValueError("What direction then ?")
