from __future__ import division
from position import Position
from vector import Vector
from classes import MarginFactors, DirectionType, GoalType, RoleType
import math
import cmath
import logging

# Provide Decision Making Tools


def player_will_be_at(dest_pos, player):
    cur_pos = Position(position=player.dynamicState.position)
    cur_direction = player.dynamicState.direction
    speed = player.staticState.runningAbility
    has_ball = player.dynamicState.hasBall
    vector_to_dest = Vector(pos_from=cur_pos, pos_to=dest_pos)
    turning_time = vector_to_dest.turning_time(direction=cur_direction)
    # if turning time is more than one turn, he will stay at the same position
    # deg_diff = abs(cur_direction-vector_to_dest.direction) # tbd
    deg_diff = vector_to_dest.angle_from(direction=cur_direction)
    if turning_time >= 0.1:
        logging.debug("as the direction of player[%d]:[%d] dest:[%d], diff[%d] => turn_time[%1.4f], can't move at all"
            %(player.staticState.playerNumber, cur_direction, vector_to_dest.direction, deg_diff, turning_time))
        return cur_pos
    else:

        logging.debug("as the direction of player[%d]:[%d] dest:[%d], diff[%d] => turn_time[%1.4f]"
         %(player.staticState.playerNumber, cur_direction, vector_to_dest.direction, deg_diff, turning_time))

        if has_ball:
            actual_speed = speed * 0.4
        else:
            actual_speed = speed


        actual_time = 0.1 - turning_time

        moving_distance = get_player_moving_distance(actual_speed, actual_time)
        return cur_pos.get_position_plus_vector_for_player(vector_to_dest.get_scaled_vector(moving_distance))


def get_actual_moving_time(chasing_player, pos_to):

    turn_time = Vector(pos_from=chasing_player.dynamicState.position, pos_to=pos_to) \
                .turning_time(direction=chasing_player.dynamicState.direction)
    moving_time = pos_to.time_from(chasing_player)
    total_time = turn_time + moving_time
    # logging.debug("get_NbP:from p[%d] to pos(%1.2f, %1.2f), distance[%1.2f] : mov_tm:[%1.2f] + turn_tm:[%1.2f] = [%1.2f]" % (chasing_player.staticState.playerNumber, pos_to.x, pos_to.y, pos_to.distance_from(chasing_player.dynamicState.position), moving_time, turn_time, total_time))
    return total_time

# this returns distance and player_number
# This assumes that players running toward this_pos
def get_nearby_player(this_pos, players, excl_player_number=-1):
    min_time = 100
    near_player = None

    for key, player in players.items():
        if key == excl_player_number:
            continue

        total_time = get_actual_moving_time(player, this_pos)
        if min_time > total_time:
            min_time = total_time
            near_player = player

    logging.info("** Nearby p : From this pos(%1.2f, %1.2f), this player[%d][%1.2f,%1.2f][spd:%d] is [%1.2fs] away"
                      % (this_pos.x, this_pos.y, near_player.staticState.playerNumber, near_player.dynamicState.position.x, near_player.dynamicState.position.y, near_player.staticState.runningAbility, min_time))

    return min_time, near_player

# this return time, player_number
def get_nearby_tackling_opponent(my_pos, opposing_team_players):
    min_time = 100
    near_player = None
    logging.debug("DMT: Check time_from_any_tackling_opponent")

    for key, opp_player in opposing_team_players.items():
        if my_pos.distance_from(opp_player.dynamicState.position) <= MarginFactors.TACKLE_DISTANCE:
            return 0, opp_player

        turn_time = Vector(pos_from=opp_player.dynamicState.position, pos_to=my_pos) \
                    .turning_time(direction=opp_player.dynamicState.direction)
        moving_time = my_pos.will_be_tackled_within(opp_player.dynamicState.position, opp_player.staticState.runningAbility)
        total_time = turn_time + moving_time
        # logging.debug("--- to p[%d], mov_tm:[%1.2f] + turn_tm:[%1.2f] = [%1.2f]" % (opp_player.staticState.playerNumber, moving_time, turn_time, total_time))
        if min_time > total_time:
            min_time = total_time
            near_player = opp_player

    logging.info("** Nearby Tackling opp : From me(%1.2f, %1.2f), this player[%d][%1.2f,%1.2f][spd:%d] is [%1.2fs] away"
                      % (my_pos.x, my_pos.y, near_player.staticState.playerNumber, near_player.dynamicState.position.x, near_player.dynamicState.position.y, near_player.staticState.runningAbility, min_time))

    return min_time, near_player


# this return time, player_number
def get_nearby_tackling_opponent_in_front(my_pos, opposing_team_players, playing_direction):
    min_time = 100
    near_player = None
    logging.debug("DMT: Check time_from_any_tackling_opponent_IN_FRONT ###")

    for key, opp_player in opposing_team_players.items():
        if playing_direction == DirectionType.LEFT:
            if my_pos.x < opp_player.dynamicState.position.x:
                continue
        else:
            if my_pos.x > opp_player.dynamicState.position.x:
                continue

        if my_pos.distance_from(opp_player.dynamicState.position) <= MarginFactors.TACKLE_DISTANCE:
            return 0, opp_player

        turn_time = Vector(pos_from=opp_player.dynamicState.position, pos_to=my_pos) \
                    .turning_time(direction=opp_player.dynamicState.direction)
        moving_time = my_pos.will_be_tackled_within(opp_player.dynamicState.position, opp_player.staticState.runningAbility)
        total_time = turn_time + moving_time
        # logging.debug("--- to p[%d], mov_tm:[%1.2f] + turn_tm:[%1.2f] = [%1.2f]" % (opp_player.staticState.playerNumber, moving_time, turn_time, total_time))
        if min_time > total_time:
            min_time = total_time
            near_player = opp_player

    logging.info("** Nearby Tackling opp_IN_FRONT : From me(%1.2f, %1.2f), this player[%d][%1.2f,%1.2f][spd:%d] is [%1.2fs] away"
                      % (my_pos.x, my_pos.y, near_player.staticState.playerNumber, near_player.dynamicState.position.x, near_player.dynamicState.position.y, near_player.staticState.runningAbility, min_time))

    return min_time, near_player


def free_time_for_safe_pass(this_player, teammate, opposing_team_players, ball_speed):
    my_pos = Position(position=this_player.dynamicState.position)
    teammate_number = teammate.staticState.playerNumber

    teammate_pos = Position(position=teammate.dynamicState.position)
    nearby_opp_tuple = get_nearby_tackling_opponent(teammate_pos, opposing_team_players)
    logging.debug("**From this teammate,nearby_taclking_opp_time:[%1.2f] fr[%d] to[%d] " % (nearby_opp_tuple[0], nearby_opp_tuple[1].staticState.playerNumber, teammate_number ))
    ball_travel_time = Position(position=teammate.dynamicState.position).ball_time_from(my_pos, ball_speed)
    # logging.debug("**Ball traveling time [%1.2f]" % ball_travel_time)

    logging.info("*** From me[%d], to teammate[%d], free_time_for_safe_pass is [%1.2f]" % (this_player.staticState.playerNumber, teammate_number, nearby_opp_tuple[0] - ball_travel_time - MarginFactors.DFLT_KICK_TURNING_TIME))
    return nearby_opp_tuple[0] - ball_travel_time - MarginFactors.DFLT_KICK_TURNING_TIME

# when ball is coming to incorrect destination, calculate best dest the player move to get the ball
def where_to_move_to_get_the_free_ball(player, ball):

    player_pos = Position(position=player.dynamicState.position)
    ball_pos = Position(x=ball.position.x, y=ball.position.y)

    if ball.speed <= 1:
        if ball_pos.is_position_in_player_field():
            return ball_pos
        else:
            return None

    ball_vector = Vector(x=ball.vector.x, y=ball.vector.y)
    b_to_p_vector = Vector(pos_from=ball_pos, pos_to=player_pos)

    k = ball_pos.distance_from(player_pos)
    vp = player.staticState.runningAbility / 10
    vb = ball.speed

    angle = ball_vector.angle_from(vector=b_to_p_vector)
    cos = math.cos(math.radians(angle))
    if vb == 0:
        return None
    a = 1 - (vp**2 / vb**2)
    # logging.debug("GCHK player speed {}, a={}, vb={}".format(vp, a, vb))
    b = -2 * k * cos
    c = k**2
    d = (b**2) - (4*a*c)
    # logging.debug("[GMATH] d[%1.2f], a[%1.2f] b[%1.2f] c[%1.2f]" % (d, a, b, c))
    if d < 0:
        return None

    if a == 0:
        if cos == 0:
            return None
        sol1 = k / (2*cos)
        sol2 = -1
        logging.debug("Where_to_get GCHK 1st_order- sol1 = {}".format(sol1))
    else:
        sol1 = (-b-math.sqrt(d))/(2*a)
        sol2 = (-b+math.sqrt(d))/(2*a)
    # logging.debug("[GMATH_Suc] sol1[%1.2f] sol2[%1.2f]" % (sol1, sol2))
    if sol1 > 0 or sol2 > 0:
        if sol1 > 0 and sol2 > 0:

            dest_ball_vector1 = ball_vector.get_scaled_vector(sol1)
            dest1 = ball_pos.get_position_plus_vector_for_player(dest_ball_vector1)

            dest_ball_vector2 = ball_vector.get_scaled_vector(sol2)
            dest2 = ball_pos.get_position_plus_vector_for_player(dest_ball_vector2)

            is_dest1_valid = dest1.is_position_in_player_field()
            is_dest2_valid = dest2.is_position_in_player_field()

            if is_dest1_valid and is_dest2_valid:
                if sol1 < sol2:
                    return dest1
                else:
                    return dest2
            elif is_dest1_valid:
                return dest1
            elif is_dest2_valid:
                return dest2
            else:
                return None

        else: # if one of sol is positive,
            sol = max(sol1, sol2)
            dest_ball_vector = ball_vector.get_scaled_vector(sol)
            dest = ball_pos.get_position_plus_vector_for_player(dest_ball_vector)
            if dest.is_position_in_player_field():
                return dest
            else:
                return None
    else:
        return None


def where_ball_ends_up(ball):
    distance_moving = ball.speed**2 * 1/20
    ball_vector_to_dest = Vector(x=ball.vector.x, y=ball.vector.y).get_scaled_vector(distance_moving)

    # logging.debug("where ball_ends_up : dist_mov[%1.2f], ball_vec_to_dest(%1.2f, %1.2f), dest_pos(%1.2f, %1.2f)" %(distance_moving, ball_vector_to_dest.x, ball_vector_to_dest.y, Position(position=ball.position).get_position_plus_vector_for_ball(ball_vector_to_dest).x, Position(position=ball.position).get_position_plus_vector_for_ball(ball_vector_to_dest).y))
    return Position(position=ball.position).get_position_plus_vector_for_ball(ball_vector_to_dest)


def get_player_moving_distance(speed, time):
    if time <= 0:
        return 0
    else:
        return round((speed / 10) * time, 6)


def is_in_offense_area(position, playing_direction, centre_spot):
    if playing_direction == DirectionType.LEFT:
        if position.x < centre_spot.x:
            logging.debug("yes it's({}{}) is offense area :dir{}, centre_spot{}".format(position.x, position.y, playing_direction, centre_spot))
            return True
        else:
            logging.debug("No it's({}{}) not off area :dir{}, centre_spot{}".format(position.x, position.y, playing_direction, centre_spot))
            return False
    elif playing_direction == DirectionType.RIGHT:
        if position.x > centre_spot.x:
            logging.debug("yes it's({}{}) off area :dir{}, centre_spot{}".format(position.x, position.y, playing_direction, centre_spot))
            return True
        else:
            logging.debug("No it's({}{}) not off area :dir{}, centre_spot{}".format(position.x, position.y, playing_direction, centre_spot))
            return False
    else:
        raise ValueError("What's the playing direction then?")


def is_in_shooting_range(pos, goal_centre, role):
    dist = pos.distance_from(goal_centre)
    if role == RoleType.FW:
        return dist < MarginFactors.FW_SHOOT_RANGE
    elif role == RoleType.CB:
        return dist < MarginFactors.CB_SHOOT_RANGE
    elif role == RoleType.FW_NT:
        return dist < MarginFactors.FW_NO_TIME_SHOOT_RANGE
    else:
        raise ValueError("Who else is shooting ?")


def is_in_2nd_shooting_area(position, playing_direction, centre_spot):
    if playing_direction == DirectionType.LEFT:
        if (15 <= position.x < centre_spot.x - 25) and (10 <= position.y <= 40):
            return True
        else:
            return False
    elif playing_direction == DirectionType.RIGHT:
        if (85 >= position.x > centre_spot.x + 25) and (10 <= position.y <= 40):
            return True
        else:
            return False
    else:
        raise ValueError("What's the playing direction then?")


def get_opponent_shoot_angle(ball, our_playing_direction):
    shoot_vector = Vector(x=ball.vector.x, y=ball.vector.y)
    if our_playing_direction == DirectionType.LEFT:
        return abs(shoot_vector.direction - 90)
    elif our_playing_direction == DirectionType.RIGHT:
        return abs(shoot_vector.direction - 270)
    else:
        raise ValueError("What's the playing direction then?")

def get_angle_adjustable(time):
    return 600 * time

def can_pass_cut_by_opp(pos_from, pos_to, ball_speed, opp_players):
    kick_vector = Vector(pos_from=pos_from, pos_to=pos_to)
    kick_distance = pos_from.distance_from(pos_to)

    for p_num, opp in opp_players.items():
        k = pos_from.distance_from(opp.dynamicState.position)
        to_opp_vector = Vector(pos_from=pos_from, pos_to=opp.dynamicState.position)
        angle = kick_vector.angle_from(vector=to_opp_vector)

        if k >= kick_distance or k < 1 or angle >= 90:
            logging.debug("PASS_CUT EXCLUDED this opp[{}], ang[{}, k[{}]".format(p_num, angle, k))
            continue

        sin = math.sin(math.radians(angle))
        h = k * sin

        cos = math.cos(math.cos(angle))
        z = k * cos

        z_vector = kick_vector.get_scaled_vector(z)
        cut_point = pos_from.get_position_plus_vector_for_ball(z_vector)
        ball_time_to_cut_point = cut_point.ball_time_from(pos_from, ball_speed)

        turn_time = Vector(pos_from=opp.dynamicState.position, pos_to=cut_point) \
                .turning_time(direction=opp.dynamicState.direction)
        opp_moving_dist = get_player_moving_distance(opp.staticState.runningAbility, ball_time_to_cut_point - turn_time)

        logging.debug("@@@ PassCut: fr opp[%d] h is [%1.2f] op_turn_time[%1.2f](fr:%1.2f to:%1.2f), op_mv_dist[%1.2f] cp(%1.2f, %1.2f), ball_time_to_cp[%1.2f])" %(p_num, h, turn_time, Vector(pos_from=opp.dynamicState.position, pos_to=cut_point).direction, opp.dynamicState.direction, opp_moving_dist, cut_point.x, cut_point.y, ball_time_to_cut_point))
        if h - opp_moving_dist > MarginFactors.TACKLE_DISTANCE:
            continue
        else:
            logging.debug("@@@ PassCut TRUE: fr opp[%d] h is [%1.2f] op_turn_time[%1.2f](fr:%1.2f to:%1.2f), op_mv_dist[%1.2f] cp(%1.2f, %1.2f), ball_time_to_cp[%1.2f])" %(p_num, h, turn_time, Vector(pos_from=opp.dynamicState.position, pos_to=cut_point).direction, opp.dynamicState.direction, opp_moving_dist, cut_point.x, cut_point.y, ball_time_to_cut_point))
            # raise ValueError("This will be cut by opp[%d] h[%1.2f] angle[%1.2f], sin[%1.2f]"%(opp.staticState.playerNumber, h, angle, sin))
            return True

    return False


def get_fw_dribble_dest(goal_pos, fw_pos):
    to_goal_vector = Vector(pos_from=fw_pos, pos_to=goal_pos)

    if goal_pos.x == 0: # Attack LEFT
        if fw_pos.y < 25:
            shoot_angle = to_goal_vector.direction - 180
            if shoot_angle < MarginFactors.SHOOT_ANGLE_LIMIT:
                dest_pos = Position(x=15*math.sin(math.radians(MarginFactors.SHOOT_ANGLE_LIMIT)) + 0.1, y=25 - (15*math.cos(math.radians(MarginFactors.SHOOT_ANGLE_LIMIT))) - 0.1)
                logging.debug("FW DRB Dest_Angle[%1.2f] ***** fr(%1.2f, %1.2f), to(%1.2f, %1.2f)"%(shoot_angle, fw_pos.x, fw_pos.y, dest_pos.x, dest_pos.y))
                return dest_pos

        else:
            shoot_angle = 360 - to_goal_vector.direction
            if shoot_angle < MarginFactors.SHOOT_ANGLE_LIMIT:
                dest_pos = Position(x=15*math.sin(math.radians(MarginFactors.SHOOT_ANGLE_LIMIT)) + 0.1, y=25 + (15*math.cos(math.radians(MarginFactors.SHOOT_ANGLE_LIMIT))) + 0.1)
                logging.debug("FW DRB Dest_Angle[%1.2f] ***** fr(%1.2f, %1.2f), to(%1.2f, %1.2f)"%(shoot_angle, fw_pos.x, fw_pos.y, dest_pos.x, dest_pos.y))
                return dest_pos

    elif goal_pos.x == 100: # Attack RIGHT
        if fw_pos.y < 25:
            shoot_angle = 180 - to_goal_vector.direction
            if shoot_angle < MarginFactors.SHOOT_ANGLE_LIMIT:
                dest_pos = Position(x=100 - (15*math.sin(math.radians(MarginFactors.SHOOT_ANGLE_LIMIT))) - 0.1, y=25 - (15*math.cos(math.radians(MarginFactors.SHOOT_ANGLE_LIMIT))) - 0.1)
                logging.debug("FW DRB Dest_Angle[%1.2f] ***** fr(%1.2f, %1.2f), to(%1.2f, %1.2f)"%(shoot_angle, fw_pos.x, fw_pos.y, dest_pos.x, dest_pos.y))
                return dest_pos
        else:
            shoot_angle = to_goal_vector.direction
            if shoot_angle < MarginFactors.SHOOT_ANGLE_LIMIT:
                dest_pos = Position(x=100 - (15*math.sin(math.radians(MarginFactors.SHOOT_ANGLE_LIMIT))) - 0.1, y=25 + (15*math.cos(math.radians(MarginFactors.SHOOT_ANGLE_LIMIT))) + 0.1)
                logging.debug("FW DRB Dest_Angle[%1.2f] ***** fr(%1.2f, %1.2f), to(%1.2f, %1.2f)"%(shoot_angle, fw_pos.x, fw_pos.y, dest_pos.x, dest_pos.y))
                return dest_pos

    else:
        raise ValueError("Invalid goal_pos (%1.2f, %1.2f)"%(goal_pos.x, goal_pos.y))

    dest_pos = goal_pos.get_position_plus_vector_for_player(to_goal_vector.get_opposite_vector().get_scaled_vector(15.15))

    # logging.debug("FW DRB Dest_Normal[%1.2f] ***** fr(%1.2f, %1.2f), to(%1.2f, %1.2f)"%(shoot_angle, fw_pos.x, fw_pos.y, dest_pos.x, dest_pos.y))
    return dest_pos


def get_pass_speed_in_time(pos_from, pos_to, time_limit):
    dist = pos_from.distance_from(pos_to)
    if time_limit <= 0:
        return 100
    ball_speed = (5*time_limit**2 + dist) / time_limit
    kick_speed = round(ball_speed * 3.33333, 5)
    if kick_speed > 100:
        return 100
    elif kick_speed <= 0:
        raise ValueError("Can kick speed negative? {}".format(kick_speed))
    else:
        return kick_speed


def get_pass_speed_for_accuracy(pos_from, pos_to):
    dist = pos_from.distance_from(pos_to)
    ball_speed = math.sqrt(20*dist)
    # raise ValueError("dist:%1.2f, b_spd:%1.2f, kick_spd:%1.2f"%(dist, ball_speed)
    kick_speed = round(ball_speed * 3.33333 * MarginFactors.ACC_BALL_SPEED_FACTOR, 5)
    logging.debug("Pass_speed_for_accy: dist:%1.2f, b_spd:%1.2f, kick_spd:%1.2f"%(dist, ball_speed, kick_speed))
    if kick_speed > 100:
        return 100
    elif kick_speed <= 0:
        return 100
        # raise ValueError("Can kick speed negative? {}".format(kick_speed))
    else:
        return kick_speed

def get_kick_turning_time(pos_from, pos_to, p_direction):
    kick_vector = Vector(pos_from=pos_from, pos_to=pos_to)
    return kick_vector.turning_time(direction=p_direction)


def is_ball_in_dangerous_area(ball_pos, our_goal_spot):
        if Position(position=ball_pos).distance_from(our_goal_spot) < MarginFactors.DANGEROUS_AREA_DIST:
            return True
        else:
            return False