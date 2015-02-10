import math
import sys

import Class

def DebugMode() :
    return False


def Shoot(player, destination) :
    '''Generator for player p1 to turn and shoot at destination'''
    directionThreshold = 2 # if we need to turn more than this, issue an explicit turn order
    direction = Direction(player.Position, destination)
    while abs(player.Position[2] - direction) > directionThreshold :
        yield Class.Action.Turn(player, direction)

    yield Class.Action.Kick(player, destination, 100)
    return


def MoveWithBall(player, destination) :
    '''Generator to move with the ball'''
    while not PositionEqual(player.Position, destination) :
        if not player.HasBall :
            return
        yield Class.Action.Move(player, destination)


def WaitWithBall(player, nbr) :
    '''Generator to wait with the ball, nbr timeslots'''
    dir = player.Position[2]
    for _ in range(nbr) :
        if not player.HasBall :
            return
        yield Class.Action.Turn(player, dir)


def KickAndGetBall(ai, player, destination) :
    '''Generator to kick ball to destination and move to it and get the ball.'''
    directionThreshold = 2
    targetDirection = Direction(player.Position, destination)
    Assert(player.HasBall)
    while abs(player.Position[2] - targetDirection) > directionThreshold :
        yield Class.Action.Turn(player, targetDirection)
    if not ai.InControl : 
        return
    speed = ai.Ball.GetSpeedForDistance(Distance(player.Position, destination))
    yield Class.Action.Kick(player, destination, speed)

    while not PositionEqual(player.Position, destination) :
        if not ai.InControl : 
            return
        yield Class.Action.Move(player, destination)
    yield Class.Action.Possess(player)


def Move(player, destination) :
    '''Generator to move'''
    while not PositionEqual(player.Position, destination) :
        yield Class.Action.Move(player, destination)


def PassBall(ai, p1, pos, posAdjust = None) :
    '''Generator for player p1 to pass ball to position pos'''
    if posAdjust is None :
        posAdjust = (0,0)
    directionThreshold = 2 # if we need to turn more than this, issue an explicit turn order

    source = p1.Position
    destination = (pos[0] + posAdjust[0], pos[1] + posAdjust[1])
    distance = Distance(source, destination)

    speed = ai.Ball.GetSpeedForDistance(distance*1.2)

    targetDirection = Direction(source, destination)

    while True :
        currentDirection = p1.Position[2]
        if abs(currentDirection - targetDirection) > directionThreshold :
            action = Class.Action.Turn(p1, targetDirection)
            yield action
            continue
        else :
            action = Class.Action.Kick(p1, destination, speed)
            yield action
            return

def IsNearlyVertical(direction, delta) :
    if direction < delta or abs(direction - 180) < delta or abs(direction - 360) < delta :
        return True
    return False
    

def SaveBall(ai, goalKeeper, goalY) :
    '''Generator to save ball - assumes standing on the goal line'''
    #Debug('SaveBall: goalY = %.1f' % goalY)
    
    ballDestinationAtGoalX = 0.0 if ai.DirectionRight else 100.0
    ballDestinationAtGoal = (ballDestinationAtGoalX, goalY)
    
    ballDirection = Direction(ai.Ball.Position, goalKeeper.Position)
    moveAtBall = IsNearlyVertical(ballDirection, 2.0)
        
    interceptPos = ai.Ball.GetClosestInterceptionPosition(goalKeeper.Position, goalKeeper.RunSpeed)
    targetPosition = interceptPos if interceptPos is not None else (goalKeeper.Position[0], goalY)
    
    while True :
        if ai.Ball.PlayerNbr is not None :
            return
        direction = Direction(goalKeeper.Position, ai.Ball.Position)

        s = Distance(ai.Ball.Position, goalKeeper.Position)
        if s < 2 :
            #Debug('SaveBall: Possess')
            yield Class.Action.Possess(goalKeeper)
            continue

        ballDistanceToGoal = Distance(ai.Ball.Position, ballDestinationAtGoal)
        timeToGoal_ms = 1000* ai.Ball.TimeForDistance(ballDistanceToGoal)
        turns = timeToGoal_ms / 100
        
        #Debug('SaveBall: GK_Pos[%s] Turns left[%.2f], distance_to_ball[%.2f]' % (str(goalKeeper.Position), turns, s))
        if turns <= 2.0 :
            #Debug('SaveBall: Possess')
            yield Class.Action.Possess(goalKeeper)
            #yield Class.Action.Turn(goalKeeper, direction)
            continue

        if moveAtBall :
            if s < 3 :
                #Debug('SaveBall: Possess')
                yield Class.Action.Possess(goalKeeper)
                continue
            #Debug('SaveBall: MoveAtBall')
            yield Class.Action.Move(goalKeeper, ai.Ball.Position)
        else :
            if not PositionEqual(goalKeeper.Position, targetPosition) :
                #Debug('SaveBall: Move to %s' % str(targetPosition))
                yield Class.Action.Move(goalKeeper, targetPosition)
            elif abs(direction - goalKeeper.Position[2]) > 0.1 :
                #Debug('SaveBall: Turn')
                yield Class.Action.Turn(goalKeeper, direction)
            else :
                #Debug('SaveBall: MoveAtBall')
                yield Class.Action.Move(goalKeeper, ai.Ball.Position)


def ReceiveBall(ai, source, p2, inControl = True) :
    '''Generator for p2 to receive the ball from source position'''
    Debug('ReceiveBall[%s -> %d]' % (str(source), p2.Number))
    directionThreshold = 5 # if we need to turn more than this, issue an explicit turn order
    destination = p2.Position
    interceptPos = None
    initialInterceptDir = None

    # before kick i.e. first iteration
    iteration = 0

    action = None
    stateInit = 0
    stateMoveToIntercept = 1
    stateAtInterceptPos = 2
    stateJustGetBall = 3

    state = stateInit
    #waitingForBallToMove = True

    while True :
        iteration += 1

        if not inControl and ai.Ball.PlayerNbr == p2.Number :
            return # intercepted.
        elif (inControl and not ai.InControl) or ai.Ball.PlayerNbr == p2.Number :
            # has the pass succeeded or failed ?
            return

        ballDestination, steps = ai.Ball.GetDestinationAndTimeSteps()

        if state == stateInit :
            if ai.Ball.Speed > 0 and iteration > 1 :
                initialBallDirection = ai.Ball.Direction
            else :
                # we don't know where the ball will go yet but turn towards the passer
                yield Class.Action.Turn(p2, Direction(destination, source))
                continue

        distanceToBall = Distance(p2.Position, ai.Ball.Position)

        if (ai.Ball.Speed == 0 and ai.Ball.PlayerNbr is None) or initialBallDirection != ai.Ball.Direction :
            state = stateJustGetBall

        if state == stateJustGetBall :
            # we have bounced off a side or some other cock up, just get the ball
            if distanceToBall < 5 :
                action = Class.Action.Possess(p2)
            else :
                action = Class.Action.Move(p2, ai.Ball.Position)
            yield action
            continue

        if state == stateInit :
            state = stateMoveToIntercept
            interceptPos = ai.Ball.GetClosestInterceptionPosition(p2.Position, p2.RunSpeed)
            Debug('Intercept pos = %s' % str(interceptPos))
            
            if interceptPos is None :
                state = stateJustGetBall
                dest, steps = ai.Ball.GetDestinationAndTimeSteps()
                while not PositionEqual(p2.Position, dest) and steps > 0 :
                    steps -= 1
                    yield Class.Action.Move(p2, dest)
                continue
            # why work out direction here?    
            #else :
            #    interceptDir = Direction(p2.Position, ai.Ball.Position)
            #    Debug('Intercept pos[%s] dir[%.1f]' % (str(interceptPos), interceptDir))
        interceptDistance = Distance(p2.Position, interceptPos)
        Debug('interceptDistance[%.1f]\n' % interceptDistance)
        
        if state == stateMoveToIntercept :
            if interceptDistance > 0.2 :
                action = Class.Action.Move(p2, interceptPos)
                yield action
                continue
            else :
                if not ai.Ball.IsMovingCloserToPosition(p2.Position) :
                    Debug('Ball not moving closer')
                    state = stateJustGetBall
                    continue

                interceptDir = Direction(p2.Position, ai.Ball.Position)
                if initialInterceptDir is None :
                    initialInterceptDir = interceptDir
                if abs(initialInterceptDir - interceptDir) > 0.5 :
                    # the ball has got pass me
                    Debug('Ball has got passed me')
                    state = stateJustGetBall
                    continue

            if abs(p2.Position[2] - interceptDir) > directionThreshold :
                # have i got time to turn?
                Debug('turning to intercept. Pos=[%s], ballPas=[%s]\n' % (p2.Position, ai.Ball.Position))
                action = Class.Action.Turn(p2, interceptDir)
                yield action
                continue
            else :
                state = stateAtInterceptPos

        if state == stateAtInterceptPos :
            Debug('Possess[%d]' % p2.Number)
            action = Class.Action.Possess(p2)

        else :
            raise Exception('State error %d' % state)

        yield action


def PositionValid(pos, playerIsGoalkeeper = None, playDirectionRight = None) :
    # default is don't worry about which player e.g. to validate ball position
    if playerIsGoalkeeper is not None :
        if playerIsGoalkeeper :
            circleCentre = (0, Class.Pitch.GoalCentre) if playDirectionRight else (100, Class.Pitch.GoalCentre)
            if Distance(circleCentre, pos) >= Class.Pitch.GoalAreaRadius :
                return False
        else :
            circleCentre = (0, Class.Pitch.GoalCentre) if not playDirectionRight else (100, Class.Pitch.GoalCentre)
            if Distance(circleCentre, pos) < Class.Pitch.GoalAreaRadius :
                return False

    return pos[0] <= Class.Pitch.Width and pos[0] >= 0 and pos[1] <= Class.Pitch.Height and pos[1] >= 0


def DirectionValid(direction) :
    return direction >= 0 and direction < 360


def TimeToPosition(startPosition, endPosition, speed_ms) :
    # neglect turning time for now
    s = Distance(startPosition, endPosition)
    result = TimeForDistance(s, speed_ms)
    #Debug('TimeToPosition[%s, %s, %.1f] result[%.1f]' % (str(startPosition), str(endPosition), speed, result))
    return result


def TimeForTurn(angleToTurn) :
    #600 = deg / sec
    return angleToTurn / 600.0


def TimeForDistance(distance, speed) :
    if speed == 0.0 :
        return 99999
    return distance / speed


def Debug(msg) :
    if not DebugMode() :
        return
    sys.stderr.write(msg + '\n')
    sys.stderr.flush()


def DistanceLimitForSave(ballToGoalDistance) :
    '''Maximum distance the goalkeeper can be from the ball and save it.
       Assumes goal keeper has 100 running ability'''
    timeToGoal_s = Class.Ball.FastBall().TimeForDistance(ballToGoalDistance)

    #Debug('Distance to goal = %.2f, Time to goal = %.2f.' % (ballToGoalDistance, timeToGoal_s))

    reactionTime_s = 0.1
    turnTime_s = TimeForTurn(90) # assume a bit of turning is needed.
    movementTime_s = timeToGoal_s - reactionTime_s - turnTime_s
    if movementTime_s <= 0 :
        return 0
    playerSpeed_ms_1 = 10 # todo take from player class
    x = movementTime_s * playerSpeed_ms_1
    if x > Class.Pitch.GoalWidth :
        x = Class.Pitch.GoalWidth
    return x


def MidPoint(pos1, pos2) :
    # mid point in a line between 1 and 2
    s = Distance(pos1, pos2)
    d = Direction(pos1, pos2)
    return PositionFromDirAndDistance(pos1, d, s/2)


def PositionFromDirAndDistance(startPosition, direction, distance) :
    endPos = (0,0)

    if direction == 90 :
        dx = 1
        dy = 0
    elif direction == 270 :
        dx = -1
        dy = 0
    elif direction == 0 :
        dx = 0
        dy = -1
    elif direction == 180 :
        dx = 0
        dy = 1
    elif direction < 90 :
        dirRad = math.radians(direction)
        dx = math.sin(dirRad)
        dy = -math.cos(dirRad)
    elif direction < 180 :
        dirRad = math.radians(180 - direction)
        dx = math.sin(dirRad)
        dy = math.cos(dirRad)
    elif direction < 270 :
        dirRad = math.radians(270 - direction)
        dx = -math.cos(dirRad)
        dy = math.sin(dirRad)
    elif direction < 360 :
        dirRad = math.radians(360 - direction)
        dx = -math.sin(dirRad)
        dy = -math.cos(dirRad)
    else :
        raise Exception('Invalid direction %.1f' % direction)

    endPos = [startPosition[0] + distance * dx, startPosition[1] + distance * dy]
    
    if endPos[0] > Class.Pitch.Width : endPos[0] == 0
    if endPos[0] < 0 : endPos = 0
    if endPos[1] > Class.Pitch.Height : endPos[1] = Class.Pitch.Height
    if endPos[1] < 0 : endPos[1] = 0
    return tuple(endPos)


def PositionEqual(pos1, pos2) :
    if abs(pos1[0] - pos2[0]) >= 0.4 :
        return False
    if abs(pos1[1] - pos2[1]) >= 0.4 :
        return False
    return True


def ClosestPointOnLine(pos1, pos2, startPoint) :
    '''On the line defined by pos1, pos2 find the closest point to startPoint'''
    if pos1[0] == pos2[0] :
        pos2 = (pos2[0] + 0.0001, pos2[1])
    if pos1[0] < pos2[0] :
        m, c = __LineEquation(pos1, pos2)
    else :
        m, c = __LineEquation(pos2, pos1)

    x0 = startPoint[0]
    y0 = startPoint[1]

    x = (x0 + m*y0 - m*c) / (m*m + 1)
    y = m*x + c

    #Debug('ClosestPointOnLine[%s, %s, %s] result=[%s]' % (str(pos1), str(pos2), str(startPoint), str((x,y))))
    return (x,y)

def Distance(posXY1, posXY2) :
    x = abs(posXY1[0] - posXY2[0])
    y = abs(posXY1[1] - posXY2[1])
    return math.sqrt(x*x + y*y)


def Direction(posXY1, posXY2) :
    x = posXY2[0] - posXY1[0]
    y = posXY2[1] - posXY1[1]

    if x == 0 :
        return 0 if y < 0 else 180
    elif y == 0 :
        return 90 if x > 0 else 270

    rad = math.atan(float(abs(x)) / abs(y))
    deg = math.degrees(rad)
    #raise Exception('%s -> %s = %.1f' % (str(posXY1), str(posXY2), deg  ))
    if x > 0 and y < 0 : # up-right
        ans = deg
    if x < 0 and y < 0 : # up-left
        ans = 360 - deg
    if x > 0 and y > 0 : # down-right
        ans = 180 - deg
    if x < 0 and y > 0 : # down-left
        ans = 180 + deg
    return ans


def OppositeDirection(dir) :
    ans = dir + 180
    if ans >= 360 :
        ans -= 360
    return ans


def Assert(cond) :
    if DebugMode() :
        assert(cond)

    
def GoalCirclePosition(direction) :
    '''return position on the opponent goal circle line 
    (with the xaxis origin at centre) from direction centred on the goal'''
    if direction >= 270 :
        direction = direction - 270 
        theta = math.radians(direction)
        x = Class.Pitch.GoalAreaRadius * math.cos(theta)
        y = Class.Pitch.GoalAreaRadius * math.sin(theta)
        y = Class.Pitch.Height / 2 - y
        return (Class.Pitch.Width/2 - x, y)
    elif direction < 270 and direction >= 180 :
        direction = 270 - direction
        theta = math.radians(direction)
        x = Class.Pitch.GoalAreaRadius * math.cos(theta)
        y = Class.Pitch.GoalAreaRadius * math.sin(theta)
        y = Class.Pitch.Height / 2 + y
        return (Class.Pitch.Width/2 - x, y)
    raise Exception('Invalid direction %.1f' % direction)


def AngleBetween(dir1, dir2) :
    a = abs(dir1 - dir2)
    b = abs(dir1 + 360 - dir2)
    c = abs(dir2 + 360 - dir1)
    return min((a,b,c))

    
def __LineEquation(pos1, pos2) :
    '''y = mx + c'''
    dy = pos2[1] - pos1[1]
    dx = pos2[0] - pos1[0]
    if dx == 0 : dx = 0.00000001
    m =  float(dy) / dx
    c = pos1[1] - (m * pos1[0])
    return (m,c)
