import itertools
import os
import traceback
import math

import Util


class Pitch :
    def __init__(self, pitch) :
        self.__centreCircleRadius = float(pitch['centreCircleRadius'])
        self.__goalCentre = float(pitch['goalCentre'])
        self.__height = float(pitch['height'])
        self.__width = float(pitch['width'])
        self.__centreSpot = (float(pitch['centreSpot']['x']), float(pitch['centreSpot']['y']))
        self.__goalY = (float(pitch['goalY1']), float(pitch['goalY2']))
        self.__goalWidth = self.__goalY[1] - self.__goalY[0]
        self.__goalAreaRadius = float(pitch['goalAreaRadius'])
        

    @property
    def CentreCircleRadius(self) :
        return self.__centreCircleRadius

    @property
    def GoalCentre(self) :
        return self.__goalCentre

    @property
    def GoalWidth(self) :
        return self.__goalWidth

    @property
    def Height(self) :
        return self.__height

    @property
    def Width(self) :
        return self.__width

    @property
    def CentreSpot(self) :
        return self.__centreSpot

    @property
    def GoalY(self) :
        return self.__goalY

    @property
    def GoalAreaRadius(self) :
        return self.__goalAreaRadius


class Ball :
    
    MaxSpeed_ms = 30.0

    __friction = -10.0 # (from Ball.js)
    __tick_ms = 100.0 # time tick in ms

    def __init__(self, ballMessageDict) :
        self.__ballDict = ballMessageDict
        self.__vector = (ballMessageDict['vector']['x'], ballMessageDict['vector']['y'])
        self.__position = (ballMessageDict['position']['x'], ballMessageDict['position']['y'])
        self.__speed_ms = float(ballMessageDict['speed'])
        self.__controllingPlayerNbr = ballMessageDict['controllingPlayerNumber']
        if self.__controllingPlayerNbr == -1 :
            self.__controllingPlayerNbr = None
        self.__SetDestinationAndTimeSteps()
        self.__controllingPlayer = None


    def __str__(self) :
        return str(self.__ballDict)

    @classmethod
    def FastBall(cls) :
        '''Ball at max speed, not caring about position'''
        return cls.GetBall((0,0), (0,0))
        
    @classmethod
    def GetBall(cls, pos1, pos2) :
        '''Get Ball kicked from pos1 to pos2 at max speed'''
        dx = pos2[0] - pos2[0]
        dy = pos2[1] - pos2[1]
        h = math.sqrt(dx*dx + dy*dy)
        if dx == 0 :
            if dy == 0 :
                vector = (0, 0)
            else :
                vector = (0, dy/dy)
        elif dy == 0 :
            vector = (dx/dx, 0)
        else :
            vector = (dx/h, dy/h)
        message = {'speed' : cls.MaxSpeed_ms, 'position' : { 'x' : pos1[0], 'y' : pos1[1]}, 'vector' : { 'x' : vector[0], 'y' : vector[1]},                  'controllingPlayerNumber' : -1}
        return Ball(message)


    def SetPlayer(self, p) :
        self.__controllingPlayer = p


    @property
    def Player(self) :
        return self.__controllingPlayer # None if I do not control ball


    @property
    def PlayerNbr(self) :
        return self.__controllingPlayerNbr


    @property
    def Speed(self) :
        return self.__speed_ms


    @property
    def Position(self) :
        return self.__position


    @property
    def Vector(self) :
        return self.__vector

    @property
    def MaxDistance(self) :
        return 45

    def GetDestinationAndTimeSteps(self) :
        return self.__destAndTime


    def __SetDestinationAndTimeSteps(self) :
        ''' return (x,y), NbrOfTurns '''
        if self.__speed_ms == 0 :
            self.__destAndTime = (self.__position, 0)

        # v = u + at,  s = ut + 0.5at*t
        u = self.__speed_ms
        a = self.__friction
        t = -float(self.__speed_ms) / a
        s = u*t + 0.5*a*t*t

        # TODO handle reflection...
        endPosX = self.__position[0] + s * self.__vector[0]
        endPosY = self.__position[1] + s * self.__vector[1]
        self.__destAndTime = (endPosX, endPosY), int(1000*t / self.__tick_ms)


    def GetSpeedForDistance(self, s) :
        '''return speed required (as a percentage)'''
        # v = u + at,  s = ut + 0.5at*t
        a = self.__friction
        u = math.sqrt(-2 * s * a)

        if u >= self.MaxSpeed_ms :
            return self.MaxSpeed_ms
        return 100 * u/self.MaxSpeed_ms


    def GetNextPosition(self) :
        '''Position after the next turn'''
        if self.__speed_ms == 0 :
            return self.__position

        # s = ut + 0.5at*t
        s = self.__speed_ms * self.__tick_ms + 0.5 * self.__friction * self.__tick_ms * self.__tick_ms
        endPosX = self.__position[0] + s * self.__vector[0]
        endPosY = self.__position[1] + s * self.__vector[1]
        return (endPosX, endPosY)

    def IsMovingCloserToPosition(self, position) :
        distanceNow = Util.Distance(self.__position, position)
        return Util.Distance(self.GetNextPosition(), position) < distanceNow

    def TimeToPosition(self, pos) :
        # s = ut + 0.5at*t
        s = Util.Distance(self.__position, pos)
        return self.TimeForDistance(s)


    def TimeForDistance(self, distance) :
        if distance == 0 :
            return 0
        s = 0
        t = 0
        dt = 0.05
        speed = self.__speed_ms
        while s <= distance :
            if speed == 0 :
                return 99999
            ds = speed * dt
            speed = speed + self.__friction * dt
            if speed < 0 :
                speed = 0
            s += ds
            t += dt
        #Util.Debug('Ball.TimeForDistance[speed=%.1f, dist=%.1f] result[%.1f]' % (self.__speed_ms, distance, t))
        return t


    def GetClosestInterceptionPosition(self, startPosition, runSpeed) :
        def Debug(msg) :
            pass #Util.Debug(msg)

        '''form a 90deg triangle to intercept and calculate the angles/distances'''
        # first check interception is possible
        # Call A = startPosition, B = ball Position, C = ball destination
        A = startPosition
        B = self.__position
        C, timeSteps = self.GetDestinationAndTimeSteps()
        BC_Dir = self.Direction
        

        pos = Util.ClosestPointOnLine(B, C, A)
        if not Util.PositionValid(pos) :
            return None

        BC_distance = Util.Distance(B, C)
        Debug('BC_Distance[%.1f]' % BC_distance)
        x = BC_distance

        timeToGetToPos = Util.TimeToPosition(A, pos, runSpeed)
        Debug('timeToGetToPos[%s, %s, %.1f]' % (str(A), str(pos), timeToGetToPos))
        timeToGetToPos += Util.TimeForTurn(90) # allow some turn time
        timeToGetToPos += 0.01 # add 10ms to be ready with 1 tick to spare

        distanceForBallToPos = Util.Distance(B, pos)
        timeForBallToPos = self.TimeForDistance(distanceForBallToPos)
        if timeToGetToPos < timeForBallToPos :
            Debug('Time to get to ball pos=%.1f. Time for ball to move=%.1f. Returning pos[%s]' % \
                (timeToGetToPos, timeForBallToPos, str(pos)))
            return pos

        while x < BC_distance :
            x += 1.0
            Debug('x[%.1f]' % x)
            pos = Util.PositionFromDirAndDistance(B, BC_Dir, x)

            timeToGetToPos = Util.TimeToPosition(A, pos, runSpeed)

            timeToGetToPos += Util.TimeForTurn(90) # allow some turn time
            timeToGetToPos += 0.01 # add 10ms to be ready with 1 tick to spare
            Debug('timeToGetToPos[%.1f]' % timeToGetToPos)

            timeForBallToPos = self.TimeForDistance(x)
            Debug('timeForBallToPos[%.1f]' % timeForBallToPos)
            if timeToGetToPos < timeForBallToPos :
                Debug('1. Returning pos[%s]' % (str(pos)))
                return pos
            Debug('x[%.1f] bc_distance[%.1f]' % (x, BC_distance))

        Debug('Returning None')
        return None


    @property
    def Gradient(self) :
        if self.__vector[0] == 0.0 :
            return 99999
        return self.__vector[1] / self.__vector[0]


    @property
    def Direction(self) :
        if self.__vector[0] == 0 :
            return 180 if self.__vector[1] >= 0 else 0
        if self.__vector[1] == 0 :
            return 90 if self.__vector[0] > 0 else 270

        rad = math.atan( abs(self.__vector[0]) / abs(self.__vector[1]) )
        deg = math.degrees(rad)
        if self.__vector[0] > 0 :
            result = 180 - deg if self.__vector[1] > 0 else deg
        else :
            result = 180 + deg if self.__vector[1] > 0 else 360 - deg
        #Util.Debug('Direction[%.1f]' % (result))
        return result


class Action :
    
    @staticmethod
    def Turn(player, direction) :
        Util.Assert(Util.DirectionValid(direction))
        return { 'playerNumber' : player.Number, 'action' : 'TURN', 'direction' : direction }

    @staticmethod
    def Kick(player, dest, spd) :
        Util.Assert(Util.PositionValid(dest))
        Util.Assert(spd > 0 and spd <= 100)
        return {'action': 'KICK', 'playerNumber' : player.Number, 'destination' :
            {'x': dest[0], 'y' : dest[1]}, 'speed' : spd }

    @staticmethod
    def Move(player, dest) :
        try :
            Util.Assert(Util.PositionValid(dest, player.IsGoalkeeper, player.PlayDirectionRight))
        except :
            # usually occurs because a pass goes into the goal circle and I request to intercept the ball
            msg = 'GK[%r]. Right[%r] Invalid position %s' % (player.IsGoalkeeper, player.PlayDirectionRight, str(dest))
            Util.Debug('Move requested to invalid position. ' + msg)
            pass
        
        return {'action' : 'MOVE', 'playerNumber' : player.Number, 'destination' :
            {'x': dest[0], 'y' : dest[1]} }

    @staticmethod
    def Possess(player) :
        return { 'action': 'TAKE_POSSESSION', 'playerNumber' : player.Number }



class PlayerBase(object) :
    __maxSpeedms = 10.0 # m/s

    def __init__(self, number, ballControl, kicking, running, tackling) :
        self.__number = number
        self.__ballControl = ballControl
        self.__kicking = kicking
        self.__running = running
        self.__tackling = tackling
        self.__kickoffPos = None
        self.__goalKickPos = None
        self.__shotPos = None
        self.__pos = None
        self.__actionGenerator = None
        self.__hasBall = False
        self.__playDirectionRight = False
        self.__action = None
        self.__playerToMark = None

    def SetPlayerToMark(self, player) :
        self.__playerToMark = player

    def SetPosition(self, pos) :
        self.__pos = pos
        
    def SetKickOffPos(self, kickOffPos) :
        self.__kickoffPos = kickOffPos

    def SetGoalKickPos(self, goalKickPos) :
        self.__goalKickPos = goalKickPos

    def SetShotPos(self, shotPos) :
        self.__shotPos = shotPos

    def SetPlayDirectionRight(self) :
        self.__playDirectionRight = True

    def SetPlayDirectionLeft(self) :
        self.__playDirectionRight = False

    @property
    def Kicking(self) :
        return self.__kicking

    def GetStats(self) :
        return { 'ballControlAbility': self.__ballControl,
                 'kickingAbility': self.__kicking,
                 'runningAbility': self.__running,
                 'tacklingAbility': self.__tackling }

    @property
    def IsPlaying(self) :
        return self.__running > 0

    @property
    def GoalKickPos(self) :
        return self.__goalKickPos

    @property
    def ShotPos(self) :
        return self.__shotPos
    
    @property
    def KickOffPos(self) :
        return self.__kickoffPos

    def OnNewTurn(self, position, hasBall) :
        self.__action = None
        self.__hasBall = hasBall
        self.__pos = position
    
    @property
    def PlayDirectionRight(self) :
        return self.__playDirectionRight

    @property
    def IsGoalkeeper(self) :
        return self.__number in (5, 11)

    @property
    def HasBall(self) :
        return self.__hasBall

    @property
    def Number(self) :
        return self.__number

    @property
    def PlayerToMark(self) :
        return self.__playerToMark

    @property
    def RunSpeed(self) :
        return self.__maxSpeedms * self.__running / 100

    def TimeForDistance(self, distance) :
        return distance / self.RunSpeed

    @property
    def Position(self) :
        '''return tuple of x, y, direction'''
        return self.__pos

    def SetActionGen(self, actionGenerator) :
        self.__action = None
        self.__actionGenerator = actionGenerator
        self.SetOngoingAction()

    def AppendActionGen(self, actionGenerator) :
        if not self.__actionGenerator :
            self.SetActionGen(actionGenerator)
        else :
            self.__actionGenerator = itertools.chain(self.__actionGenerator, actionGenerator)

    def PeekAction(self) :
        # for debugging
        return self.__action

    def GetAction(self) :
        if self.__action is None :
            self.SetOngoingAction()
        
        ret = self.__action
        self.__action = None
        return ret

    def SetOneOffAction(self, action) :
        self.__actionGenerator = None
        self.__action = action

    def SetOngoingAction(self) :
        if self.__actionGenerator is None :
            self.__action = None
        else :
            if self.__action is not None :
                return True # already done this turn

            try :
                self.__action = self.__actionGenerator.next()
                return True
            except StopIteration :
                self.__actionGenerator = None
                self.__action = None
        return False

    def Shoot(self, position) :
        self.SetActionGen(Util.Shoot(self, position))

    def MoveWithBall(self, position) :
        self.SetActionGen( Util.MoveWithBall(self, position) )

    def Move(self, position) :
        self.SetActionGen( Util.Move(self, position) )


class LastException :

    @staticmethod
    def GetType() :
        return os.sys.exc_info()[0]

    @staticmethod
    def GetObject() :
        return os.sys.exc_info()[1]

    @staticmethod
    def GetReason() :
        return str(os.sys.exc_info()[1])

    @staticmethod
    def GetStack() :
        return traceback.extract_tb(os.sys.exc_info()[2])

    @classmethod
    def GetInfo(cls, withStack = True) :
        exceptionInfo = 'Exception %s (reason : %s)' % (cls.GetType(), cls.GetReason())
        if withStack :
            for stackLevelData in cls.GetStack() :
                exceptionInfo += '\nIn script %s, line %i, function %s\n    %s' % tuple(stackLevelData)
        return exceptionInfo
