import json
import os

import Util
import Class


globalTimeLimitForBallInMyGoalArea = 15 # seconds

class AiBase(object) :

    def __init__(self) :
        self.__log = None
        self.__pitch = None
        self.__inControl = None # Do we have ball OR did we have it last
        self.__iHaveBall = None # Do we have the ball
        self.__ballInMyGoalAreaTurns = 0 # Nbr of turns that the ball has been in my goal area.
        self.__closestMyPlayerNbrToBall = None
        self.__closestOpponentPlayerNbrToBall = None
        self.__positions = {} # Positions of all players  { playerNbr : pos(x,y,direction) }
        self.__opponentPlayers = {} #  { playerNbr : Player obj }
        
        while True :
            line = os.sys.stdin.readline()
            message = json.loads(line)
            self.Log('\t\t' + 'RECV: ' + str(message))
            if message['messageType'] == 'EVENT' :
                self._OnEvent(message)
            elif message['messageType'] == 'REQUEST' :
                reply = self._OnRequest(message)
                self.Log('\t\t' + 'REPLY: ' + str(reply))
                os.sys.stdout.write(json.dumps(reply) + '\n')
                os.sys.stdout.flush()
            elif message['messageType'] == 'ERROR' :
                self.Log('\t\tRECV ERROR1: ' + str(message))
                raise Exception('Received error from framework: ' + str(message))


    def Log(self, msg) :
        if not self.__log :
            self.__log = open('baldrick.log', 'w')
        self.__log.write(msg + '\n')
        self.__log.flush()


    def LogErr(self, msg):
        Util.Debug(msg)


    def GetPlayer(self, nbr) :
        for p in self._Players() :
            if p.Number == nbr :
                return p
        raise Exception('player nbr not found: %d' % nbr)


    def _Players(self) :
        raise Exception('_Players not implemented (must return array of Player obj)')

    
    def IsBallInMyGoalArea(self) :
        return self.__ballInMyGoalAreaTurns > 0


    @staticmethod
    def LogFuncName(func) :
        def f(self, *args, **kwargs) :
            self.Log(func.__name__)
            return func(self, *args, **kwargs)
        return f


    def _OnEvent(self, message) :
        { 'GAME_START'    : self.__OnEventGameStart,
          'TEAM_INFO'     : self.__OnEventTeamInfo,
          'KICKOFF'       : self.__OnEventKickOff,
          'START_OF_TURN' : self.__OnEventStartOfTurn,
          'GOAL'          : self._OnEventGoal,
          'HALF_TIME'     : self._OnEventHalfTime,
        }[message['eventType']](message)


    def _OnRequest(self, message) :
        return { 'CONFIGURE_ABILITIES' : self._OnConfigureAbilities,
                 'KICKOFF'             : self._OnKickOff,
                 'PLAY'                : self._OnPlay,
               }[message['requestType']](message)
        

    
    def __OnEventGameStart(self, message) :
        Class.Pitch = Class.Pitch(message['pitch']) # creates singleton
        self._OnEventGameStart(message)

            
    def __OnEventTeamInfo(self, message) :
        """{'teamNumber': 2, 'players': [{'playerNumber': 6, 'playerType': 'P'}, {'playerNumber': 7, 'playerType': 'P'}, {'playerNumber': 8, 'playerType': 'P'}, {'playerNumber': 9, 'playerType': 'P'}, {'playerNumber': 10, 'playerType': 'P'}, {'playerNumber': 11, 'playerType': 'G'}]}"""
        self.__teamNumber = message['teamNumber']
        self.__players = []
        self.__playersNotGoalie = []
        for player in message['players'] :
            if player['playerType'] == 'G' :
                self.__goalie = player['playerNumber']
                self.__opponentGoalie = 11 if self.__goalie == 5 else 5
            else :
                self.__playersNotGoalie.append(player['playerNumber'])
            self.__players.append(player['playerNumber'])

        self.Log( 'Players: ' + str(self.__players) )
        self.Log( 'Goalie: ' + str(self.__goalie) )


    def __OnEventKickOff(self, message) :
        '''
        {'teamKickingOff': 1, 'team1': {'direction': 'RIGHT', 'score': 0, 'name': 'Baldrick'}, 
                              'team2': {'direction': 'LEFT', 'score': 0, 'name': 'BootAndShoot'}}
                               '''
        self.__directionRight = message[self.TeamTag]['direction'] == 'RIGHT'
        self.__kickingOff = message['teamKickingOff'] == self.TeamNumber
        self.__score = (0,0)
        self.Log('My direction is ' + ('R' if self.DirectionRight else 'L'))

    
    def __OnEventStartOfTurn(self, message) :
        """{"game": {"currentTimeSeconds":7.2},
            "ball": {"position": {"x":52.239,"y":19.314}, "vector": {"x":-0.915, "y":-0.403}, "speed":14, "controllingPlayerNumber" :-1},
            "team1":{"team":{"name":"","score":0,"direction":"RIGHT"}, "players":[{"staticState":{"playerNumber":0,"playerType":"P","kickingAbility":66.667,"runningAbility":66.667,"ballControlAbility":66.667,"tacklingAbility":66.667},"dynamicState":{"position":{"x":25,"y":10},"hasBall":false,"energy":100,"direction":270}},{"staticState":{"playerNumber":1,"playerType":"P","kickingAbility":66.667,"runningAbility":66.667,"ballControlAbility":66.667,"tacklingAbility":66.667},"dynamicState":{"position":{"x":25,"y":25},"hasBall":false,"energy":100,"direction":270}},{"staticState":{"playerNumber":2,"playerType":"P","kickingAbility":66.667,"runningAbility":66.667,"ballControlAbility":66.667,"tacklingAbility":66.667},"dynamicState":{"position":{"x":25,"y":40},"hasBall":false,"energy":100,"direction":270}},{"staticState":{"playerNumber":3,"playerType":"P","kickingAbility":66.667,"runningAbility":66.667,"ballControlAbility":66.667,"tacklingAbility":66.667},"dynamicState":{"position":{"x":73.068,"y":30.268},"hasBall":false,"energy":100,"direction":298.001}},{"staticState":{"playerNumber":4,"playerType":"P","kickingAbility":66.667,"runningAbility":66.667,"ballControlAbility":66.667,"tacklingAbility":66.667},"dynamicState":{"position":{"x":72.957,"y":31.666},"hasBall":false,"energy":100,"direction":301.252}},{"staticState":{"playerNumber":5,"playerType":"G","kickingAbility":66.667,"runningAbility":66.667,"ballControlAbility":66.667,"tacklingAbility":66.667},"dynamicState":{"position":{"x":4.977,"y":24.526},"hasBall":false,"energy":100,"direction":354.952}}]},
            "team2":{"team":{"name":"","score":0,"direction":"LEFT"},"players":[{"staticState":{"playerNumber":6,"playerType":"P","kickingAbility":66.667,"runningAbility":66.667,"ballControlAbility":66.667,"tacklingAbility":66.667},"dynamicState":{"position":{"x":81.198,"y":33.952},"hasBall":false,"energy":100,"direction":296.958}},{"staticState":{"playerNumber":7,"playerType":"P","kickingAbility":66.667,"runningAbility":66.667,"ballControlAbility":66.667,"tacklingAbility":66.667},"dynamicState":{"position":{"x":78.653,"y":32.417},"hasBall":false,"energy":100,"direction":296.521}},{"staticState":{"playerNumber":8,"playerType":"P","kickingAbility":66.667,"runningAbility":66.667,"ballControlAbility":66.667,"tacklingAbility":66.667},"dynamicState":{"position":{"x":76.46,"y":29.984},"hasBall":false,"energy":100,"direction":293.775}},{"staticState":{"playerNumber":9,"playerType":"P","kickingAbility":66.667,"runningAbility":66.667,"ballControlAbility":66.667,"tacklingAbility":66.667},"dynamicState":{"position":{"x":25,"y":35},"hasBall":false,"energy":100,"direction":270}},{"staticState":{"playerNumber":10,"playerType":"P","kickingAbility":66.667,"runningAbility":66.667,"ballControlAbility":66.667,"tacklingAbility":66.667},"dynamicState":{"position":{"x":25,"y":15},"hasBall":false,"energy":100,"direction":270}},{"staticState":{"playerNumber":11,"playerType":"G","kickingAbility":66.667,"runningAbility":66.667,"ballControlAbility":66.667,"tacklingAbility":66.667},"dynamicState":{"position":{"x":95.03,"y":24.454},"hasBall":false,"energy":100,"direction":5.975}}]}}"""
        self.__ball = Class.Ball(message['ball'])
        self.__iHaveBall = False
        
        x = 0 if self.DirectionRight else Class.Pitch.Width
        if Util.Distance( (x, Class.Pitch.GoalCentre), self.__ball.Position) <= Class.Pitch.GoalAreaRadius :
            self.__ballInMyGoalAreaTurns += 1
            if self.__ballInMyGoalAreaTurns > 10* globalTimeLimitForBallInMyGoalArea :
                raise Exception('Time wasting!')
        else :
            self.__ballInMyGoalAreaTurns = 0
        if self.__ball.PlayerNbr is not None :
            self.__inControl =  (self.__ball.PlayerNbr in self.MyPlayerNumbers)
            self.__iHaveBall = self.__inControl

        for team in ('team1', 'team2') :
            players = message[team]['players']
            for p in players :
                static = p['staticState']
                nbr = static['playerNumber']
                pos = (p['dynamicState']['position']['x'], p['dynamicState']['position']['y'], p['dynamicState']['direction'])
                self.__positions[nbr] = pos
                if team != self.TeamTag :
                    if nbr not in self.__opponentPlayers :
                        self.__opponentPlayers[nbr] = Player(nbr, static['ballControlAbility'], static['kickingAbility'], static['runningAbility'], static['tacklingAbility'])
                    self.__opponentPlayers[nbr].SetPosition(pos)
                     
        players = message[self.TeamTag]['players']
        for p in players :
            pos = (p['dynamicState']['position']['x'], p['dynamicState']['position']['y'], p['dynamicState']['direction'])
            pl = self.GetPlayer(p['staticState']['playerNumber'])
            pl.OnNewTurn(position = pos, hasBall = self.Ball.PlayerNbr == pl.Number)

        if self.__iHaveBall :
            pl = self.GetPlayer(self.Ball.PlayerNbr)
            self.__ball.SetPlayer(pl)

        self.__closestMyPlayerNbrToBall = self.ClosestPlayerNbrToBall(friendly = True)
        self.__closestOpponenetPlayerNbrToBall = self.ClosestPlayerNbrToBall(friendly = False)

        self.Log('Ball: %s' % str(self.__ball))
        self.Log('In Control: %s' % str(self.__inControl))


    def ClosestPlayerNbrToBall(self, friendly, excludeNumbers = None) :
        # friendly = True - out of my players, False, opponents players
        closest = (None, 10000.0) # player, distance
        if excludeNumbers is None :
            excludeNumbers = []

        for nbr, pos in self.AllPlayerPositions.iteritems() :
            if nbr in excludeNumbers :
                continue
            if friendly and nbr not in self.MyPlayerNumbers :
                continue
            elif not friendly and nbr in self.MyPlayerNumbers :
                continue
            distance = Util.Distance(pos, self.Ball.Position)
            if distance < closest[1] :
                closest = (nbr, distance)
        return closest[0]


    @property
    def ClosestMyPlayerNbrToBall(self) :
        return self.__closestMyPlayerNbrToBall


    @property
    def ClosestOpponentPlayerNbrToBall(self) :
        return self.__closestOpponentPlayerNbrToBall


    @property
    def Ball(self) :
        return self.__ball


    @property
    def KickingOff(self) :
        return self.__kickingOff


    @property
    def InControl(self) :
        return self.__inControl


    @property
    def IHaveBall(self) :
        return self.__iHaveBall


    @property
    def DirectionRight(self) :
        return self.__directionRight


    @property
    def DirectionAttack(self) :
        if self.__directionRight :
            return 90
        return 270


    @property
    def TeamNumber(self) :
        return self.__teamNumber


    @property
    def TeamTag(self) :
        return 'team' + str(self.TeamNumber)


    @property
    def Score(self) :
        return self.__score


    @property
    def AllPlayerPositions(self) :
        return self.__positions


    @property
    def OpponentPlayers(self) :
        return self.__opponentPlayers


    @property
    def MyPlayerNumbers(self) :
        return self.__players


    @property
    def MyPlayerNumbersNotG(self) :
        return self.__playersNotGoalie


    @property
    def GoalieNumber(self) :
        return self.__goalie

    @property
    def OpponentGoalieNumber(self) :
        return self.__opponentGoalie

    def ReflectXPos(self, pos) :
        # e.g. 0,5 -> 100, 5
        return (Class.Pitch.Width - pos[0], pos[1])



class Goalkeeper(Class.PlayerBase) :
    stateIdle = 0
    stateMakingSave = 1
    stateFetchingBall = 2
    stateMovingToKickOffPos = 3
    stateKickingBall = 4


    def __init__(self, ai, ballControl, kicking, running, tackling) :
        Class.PlayerBase.__init__(self, ai.GoalieNumber, ballControl, kicking, running, tackling)
        self.__state = self.stateIdle
        self.__ai = ai


    def OnHalfTime(self) :
        self.__state = self.stateIdle


    def OnPlay(self) :
        shotOnTargetYPos = self.__ai.Ball.PlayerNbr is None and self.__ai.Ball.Speed > 0 and self.__ai.IsOpponentShotOnTarget()
        
        count = 0
        while True : # TODO rewrite without the possibility of inf loop
            count += 1
            if count > 10 : raise Exception('Goalie inf loop')
            if self.HasBall :
                
                if self.__state not in (self.stateKickingBall, self.stateMovingToKickOffPos) :
                    #self.__ai.LogErr('GK: Moving to kick off pos')
                    # set a sequence of move to one position (near top of goal circle), wait, then move to bottom
                    
                    self.SetActionGen(
                        Util.KickAndGetBall(self.__ai, self,
                                            self.__ai.TranslatePos( (-Class.Pitch.Width/2 + 0.1, Class.Pitch.Height/2 - Class.Pitch.GoalAreaRadius + 1) ) ))
                    self.AppendActionGen( Util.WaitWithBall(self, 40) ) # nbr of time slots to wait
                    
                    x = Util.GoalCirclePosition(200)
                    goalKickPosition = (-x[0], x[1] - 1) # switch to my goal
                    
                    self.AppendActionGen( Util.KickAndGetBall(self.__ai, self,
                                                self.__ai.TranslatePos( goalKickPosition ) ))

                    self.__state = self.stateMovingToKickOffPos
                elif self.__state == self.stateMovingToKickOffPos :
                    if self.SetOngoingAction() :
                        return
                    self.__state = self.stateKickingBall
                    passTarget = self.__ai.FindClosestTeamMate(self)
                    if self.__ai.IsPassOk(self.Position, passTarget.Position) :
                        self.__ai.Pass(self, passTarget)
                    else :
                        self.__ai.Pass(self, passTarget, 0, -5)
                    self.__state = self.stateKickingBall
                return

            if self.__state == self.stateKickingBall or self.__state == self.stateMakingSave :
                #self.__ai.LogErr('GK: Getting action')
                if self.SetOngoingAction() :
                    return
                self.__state = self.stateIdle
                continue

            if shotOnTargetYPos :
                self.__state = self.stateMakingSave
                self.__ai.LogErr('GK: Shot on target')
                self.SetActionGen(Util.SaveBall(self.__ai, self, shotOnTargetYPos))
                return

            if self.__state == self.stateFetchingBall :
                if not self.__ai.IsBallInMyGoalArea() :
                    self.__state = self.stateIdle
                else :
                    if Util.Distance(self.Position, self.__ai.Ball.Position) < 5 :
                        self.SetOneOffAction( Class.Action.Possess(self) )
                    else :
                        self.SetOneOffAction( Class.Action.Move(self, self.__ai.Ball.Position) )
                    return

            if self.__state == self.stateIdle :
                #self.__ai.LogErr('GK: idle')
                
                if self.__ai.Ball.PlayerNbr is None and self.__ai.Ball.Speed == 0 :
                    if self.__ai.IsBallInMyGoalArea() :
                        self.__state = self.stateFetchingBall
                        #self.__ai.LogErr('GK: fetching ball')
                        continue

                targetPosition = self.KickOffPos
                targetY, targetDirection = self.__CalculateBestYPosition()
                targetPosition = (targetPosition[0], targetY)

                if not Util.PositionEqual(self.Position, targetPosition) :
                    #self.__ai.LogErr('GK: move to best goal position: %s' % str(targetPosition))
                    self.SetOneOffAction( Class.Action.Move(self, targetPosition) )
                elif not abs(self.Position[2] - targetDirection) < 0.1 :
                    self.SetOneOffAction( Class.Action.Turn(self, targetDirection) )
                return

            if self.__state == self.stateMovingToKickOffPos :
                return


    def __CalculateBestYPosition(self) :
        
        def ModifyDir(direction, dirMod) :
            direction += dirMod
            if direction >= 360 :
                direction -= 360
            elif direction < 0 :
                direction += 360
            Util.Assert(Util.DirectionValid(direction))
            return direction
        
        direction = Util.Direction(self.Position, self.__ai.Ball.Position)
            
        if Util.IsNearlyVertical(direction, 10) :
            dirModAmount = 90
            if self.__ai.Ball.Position[1] > self.Position[1] :
                dirMod = -dirModAmount if self.__ai.DirectionRight else dirModAmount
                direction = ModifyDir(direction, dirMod)
                return self.__ai.MyGoalPosition(False)[1], direction
            else :
                dirMod = dirModAmount if self.__ai.DirectionRight else -dirModAmount
                direction = ModifyDir(direction, dirMod)
                return self.__ai.MyGoalPosition(True)[1], direction
        
        topCornerDistance = Util.DistanceLimitForSave( Util.Distance(self.__ai.Ball.Position, self.__ai.MyGoalPosition(True)))
        bottomCornerDistance = Util.DistanceLimitForSave( Util.Distance(self.__ai.Ball.Position, self.__ai.MyGoalPosition(False)))
        
        s = topCornerDistance - (topCornerDistance + bottomCornerDistance - Class.Pitch.GoalWidth) / 2
        #self.__ai.LogErr('SAVE DISTANCES :   %.2f | %.2f' % (topCornerDistance, bottomCornerDistance))
        y =  Class.Pitch.GoalY[0] + s
        Util.Assert(Util.DirectionValid(direction))
        return y, direction
    


class Player(Class.PlayerBase) :
    pass


class Baldrick(AiBase) :
    
    def _Name(self) :
        return 'Baldrick'

    def __init(self) :
        # initialisation after the informational events
        self.__OnPlayGenerator = None
        self.__markingInitialised = False

        def YieldNumber() :
            for n in self.MyPlayerNumbersNotG :
                yield n
        nbr = YieldNumber()
        
        self.__Goalie   = Goalkeeper(self,   ballControl = 100, kicking = 100,  running = 100, tackling = 100)
        
        self.__Forward1 = Player(nbr.next(), ballControl = 100,  kicking = 100, running = 100, tackling = 100)
        self.__Forward2 = Player(nbr.next(), ballControl = 100,  kicking = 100, running = 100, tackling = 100)
        self.__Centre   = Player(nbr.next(), ballControl = 100,  kicking = 100, running = 100,  tackling = 100)

        self.__Back1    = Player(nbr.next(), ballControl = 0,  kicking = 0,  running = 0,  tackling = 0)
        self.__Back2    = Player(nbr.next(), ballControl = 0,   kicking = 0,   running = 0,   tackling = 0)
        
        totals = { 'ball' : 0, 'kick' : 0, 'run' : 0, 'tackle' : 0 }
        for p in self._Players() :
            totals['ball'] += p.GetStats()['ballControlAbility']
            totals['kick'] += p.GetStats()['kickingAbility']
            totals['run'] += p.GetStats()['runningAbility']
            totals['tackle'] += p.GetStats()['tacklingAbility']
        for k, v in totals.items() :
            if v > 400 :
                raise Exception('Stats error on %s' % k)
        

    def _Players(self) :
        return [ self.__Goalie, self.__Forward1, self.__Forward2, self.__Centre, self.__Back1, self.__Back2 ]


    @AiBase.LogFuncName
    def _OnConfigureAbilities(self, message) :
        """message e.g. = {'totalBallControlAbility': 400, 'totalKickingAbility': 400, 'totalRunningAbility': 400, 'totalTacklingAbility': 400}"""    
        """response e.g. = {"requestType":"CONFIGURE_ABILITIES","players":[{"playerNumber":6,"kickingAbility":66.6667,"runningAbility":66.6667,"ballControlAbility":66.6667,"tacklingAbility":66.6667},{"playerNumber":7,"kickingAbility":66.6667,"runningAbility":66.6667,"ballControlAbility":66.6667,"tacklingAbility":66.6667},{"playerNumber":8,"kickingAbility":66.6667,"runningAbility":66.6667,"ballControlAbility":66.6667,"tacklingAbility":66.6667},{"playerNumber":9,"kickingAbility":66.6667,"runningAbility":66.6667,"ballControlAbility":66.6667,"tacklingAbility":66.6667},{"playerNumber":10,"kickingAbility":66.6667,"runningAbility":66.6667,"ballControlAbility":66.6667,"tacklingAbility":66.6667},{"playerNumber":11,"kickingAbility":66.6667,"runningAbility":66.6667,"ballControlAbility":66.6667,"tacklingAbility":66.6667}]}"""
        self.__init()
        response = {'requestType' : 'CONFIGURE_ABILITIES', 'players' : []}
        for p in self._Players() :
            playerDict = { 'playerNumber' : p.Number }
            playerDict.update(p.GetStats())
            response['players'].append(playerDict)
        return response


    @AiBase.LogFuncName
    def _OnKickOff(self, message) :
        """no other message info"""
        #return {"requestType":"KICKOFF","players":
        #        [{"playerNumber":6,"position":{"x":75,"y":40},"direction":270},{"playerNumber":7,"position":{"x":75,"y":25},"direction":270},{"playerNumber":8,"position":{"x":75,"y":10},"direction":270},{"playerNumber":9,"position":{"x":51,"y":36},"direction":270},{"playerNumber":10,"position":{"x":51,"y":14},"direction":270}]}
        for p in self._Players() :
            if self.DirectionRight :
                p.SetPlayDirectionRight()
            else :
                p.SetPlayDirectionLeft()

        self.__SetPositions()

        playerResults = []
        direction = 90 if self.DirectionRight else 270

        for p in self._Players() :
            pos = p.KickOffPos
            res = {'playerNumber' : p.Number, 'position' : {'x' : pos[0], 'y' : pos[1] }, 'direction' : direction }
            playerResults.append(res)
        return { 'requestType' : 'KICKOFF', 'players' : playerResults }


    @AiBase.LogFuncName
    def _OnPlay(self, message) :
        actions = []
        try :
            self.__Goalie.OnPlay()
            if self.__Goalie.HasBall :
                self.MoveToGoalKickPositions()

            self.Strategy_2PlayerAttack()
            
            #self.Strategy_Defend()
            self.Strategy_MarkingDefend()
            actions = self.__GetActions()
        except :
            self.Log(Class.LastException.GetInfo())
            if Util.DebugMode() :
                raise
        return { 'requestType' : 'PLAY', 'actions' : actions }
    

    def InitialiseMarking(self) :
        playersInKickerOrder = [p for p in self.OpponentPlayers.values() if not p.IsGoalkeeper]
        playersInKickerOrder.sort( key = lambda pl : pl.Kicking, reverse = True )
        self.__Centre.SetPlayerToMark(playersInKickerOrder[0])
        if playersInKickerOrder[1].Kicking >= 95 :
            self.__Forward1.SetPlayerToMark(playersInKickerOrder[1])
        self.__markingInitialised = True

            
    def Strategy_MarkingDefend(self) :
        if not self.__markingInitialised :
            self.InitialiseMarking()
        if self.InControl :
            return

        exclude = [ p.Number for p in self._Players() if not p.IsPlaying or p.PlayerToMark is not None ]
        exclude.append(self.__Goalie.Number)
        closest = self.ClosestPlayerNbrToBall(True, exclude)

        for p in self._Players() :
            if not p.IsPlaying or p.Number == self.__Goalie.Number :
                continue # goalie already handled
            if Util.Distance(p.Position, self.Ball.Position) < 5 :
                Util.Debug('%d Grab' % p.Number)
                p.SetOneOffAction( Class.Action.Possess(p) )
                continue
            #interceptPos = self.Ball.GetClosestInterceptionPosition(p.Position, p.RunSpeed)
            #if interceptPos is not None :
            #    Util.Debug('%d Intercept' % p.Number)
            #    p.SetActionGen( Util.ReceiveBall(self, self.Ball.Position, p, False ) )
            #    continue
            
            if p.PlayerToMark is not None :
                if not Util.PositionEqual(p.Position, p.PlayerToMark.Position) :
                    Util.Debug('%d Mark' % p.Number)
                    p.SetOneOffAction( Class.Action.Move(p, p.PlayerToMark.Position) )
            else :        
                if p.Number == closest :
                    Util.Debug('%d Move' % p.Number)
                    p.SetOneOffAction( Class.Action.Move(p, self.Ball.Position) )
                else :
                    Util.Debug('%d Return' % p.Number)
                    p.SetOneOffAction( Class.Action.Move(p, p.KickOffPos) )


    def Strategy_Defend(self) :
        if self.InControl :
            return
        exclude = [ p.Number for p in self._Players() if not p.IsPlaying ]
        exclude.append(self.__Goalie.Number)
        closest = self.ClosestPlayerNbrToBall(True, exclude)

        for p in self._Players() :
            if not p.IsPlaying or p.Number == self.__Goalie.Number :
                continue # goalie already handled

            interceptPos = self.Ball.GetClosestInterceptionPosition(p.Position, p.RunSpeed)
            if interceptPos is not None :
                p.SetActionGen( Util.ReceiveBall(self, self.Ball.Position, p, False ) )
                continue
            if p.Number == closest :
                if Util.Distance(p.Position, self.Ball.Position) < 5 :
                    p.SetOneOffAction( Class.Action.Possess(p) )
                else :
                    p.SetOneOffAction( Class.Action.Move(p, self.Ball.Position) )
            else :
                p.SetOneOffAction( Class.Action.Move(p, p.KickOffPos) )


    def MyGoalPosition(self, topCorner) :
        yPos = Class.Pitch.GoalY[0] + 0.01 if topCorner else Class.Pitch.GoalY[1] - 0.01
        return self.TranslatePos((-50, yPos))
        

    def OpponentGoalPosition(self, topCorner) :
        yPos = Class.Pitch.GoalY[0] + 0.01 if topCorner else Class.Pitch.GoalY[1] - 0.01
        return self.TranslatePos((50, yPos))
    

    def FurthestGoalPosFromGoalie(self) :
        top = False if self.OpponentPlayers[self.OpponentGoalieNumber].Position[1] < 25 else True
        return self.OpponentGoalPosition(top)


    def Strategy_AllPlayerAttack(self) :
        if self.__Goalie.HasBall or not self.InControl :
            return

        exclude = [ p.Number for p in self._Players() if not p.IsPlaying ]
        exclude.append(self.__Goalie.Number)

        playersWithNoActions = [p for p in self._Players() if p.Number not in exclude and not p.SetOngoingAction()]
        
        for player in playersWithNoActions :
            if player.HasBall :
                if Util.PositionEqual(player.Position, player.ShotPos) :
                    player.Shoot(self.FurthestGoalPosFromGoalie())
                else :
                    closestNbr = self.ClosestPlayerNbrToBall(True, exclude + [player.Number])
                    self.Pass(player, self.GetPlayer(closestNbr))

        for player in playersWithNoActions :
            if player.SetOngoingAction() :
                # check if still has no action (possible pass just started)
                continue
            player.SetOneOffAction( Class.Action.Move(player, player.ShotPos) )

    
    def Strategy_2PlayerAttack(self) :
        if not self.IHaveBall or self.__Goalie.HasBall :
            return
        
        player = self.Ball.Player
        Util.Debug('player %d has ball' % player.Number)
        if player.SetOngoingAction() :
            Util.Debug('Attack, ongoing action')
            return

        goalPosition = self.FurthestGoalPosFromGoalie()
        distanceToShootingPosition = 7
        distanceForPassing = 22

        # the aim is to pass the ball between these two players until in a shooting position
        attackers = (self.__Forward1.Number, self.__Forward2.Number)
        goalCentre = self.TranslatePos((50, Class.Pitch.GoalCentre))
        atMyShotPosition = Util.PositionEqual(player.Position, player.ShotPos)
        atAnyShotPosition = atMyShotPosition or Util.Distance(player.Position, goalCentre) < Class.Pitch.GoalAreaRadius + 1
        
        if not atAnyShotPosition and self.SafeToRunAtGoal(player) :
            player.SetActionGen( Util.KickAndGetBall(self, player, player.ShotPos ) )
            return

        if atAnyShotPosition :
            player.Shoot(goalPosition)
            return

        closestEnemyNbr, closestEnemyDistance = self.FindClosestOpponent(player)
        kickItNow = closestEnemyDistance < 2
        
        if player.Number not in attackers :
            Util.Debug('Ball player not an attacker')
            closestPlayer = self.FindClosestTeamMate(player, attackers)
            
            # if within range for a pass to closest player or enemy is very close
            if (Util.Distance(player.Position, closestPlayer.Position) < distanceForPassing) or closestEnemyDistance < 2 :
                passTo = None
                if self.IsPassOk(player.Position, closestPlayer.Position) :
                    passTo = closestPlayer
                else :
                    if kickItNow :
                        for p in (self.__Forward1, self.__Forward2, self.__Centre) :
                            if p.Number not in (closestPlayer.Number, player.Number) and self.IsPassOk(player.Position, p.Position) :
                                passTo = p
                                break
                    else :
                        if self.IsPassOk(player.Position, self.__Goalie.Position) :
                            passTo = self.__Goalie
                        else :
                            pass
                    
                xAdjust = 0
                if passTo is None :
                    # TODO: maybe think of a different action but for now try to pass to closest anyway
                    passTo = closestPlayer
                    xAdjust = self.GetSafePass(player, closestPlayer)
                    
                if kickItNow :
                    Util.Debug('1. Quick Pass. pl=%d' % player.Number)
                    self.PassQuick(player, passTo)
                else :
                    if Util.Distance(player.Position, passTo.Position) < 2 :
                        self.MoveBothApart(player, passTo)
                    else :
                        Util.Debug('1. Pass')
                        self.Pass(player, passTo, xAdjust)
            else :
                Util.Debug('1. MoveBothCloser')
                self.MoveBothCloser(player, closestPlayer)
            return

        
        partner = self.GetPlayer([n for n in attackers if n != player.Number][0])
        
        # if within range for a pass to partner or enemy is very close
        if (Util.Distance(player.Position, partner.Position) < distanceForPassing) or kickItNow :
            Util.Debug('trying to pass.')
            
            xAdjust = self.GetSafePass(player, partner)
            if xAdjust is not None :
                # pass and move forward
                if kickItNow :
                    Util.Debug('2. Quick Pass')
                    self.PassQuick(player, partner, xAdjust)
                    player.AppendActionGen( Util.Move(player, player.ShotPos) )
                else :
                    if Util.Distance(player.Position, partner.Position) < 2 :
                        self.MoveBothApart(player, partner)
                    else :
                        Util.Debug('2. Pass')
                        self.Pass(player, partner, xAdjust)
                        player.AppendActionGen( Util.Move(player, player.ShotPos) )
            else :
                Util.Debug('Pass is not safe - moving (p1,p2) = (%d,%d)' % (player.Number, partner.Number))
                dir = Util.Direction(player.Position, player.ShotPos)
                moveToPos = Util.PositionFromDirAndDistance(player.Position, dir, 5)
                player.SetActionGen( Util.KickAndGetBall(self, player, moveToPos ) )
            return

        else :
            Util.Debug('2. MoveBothCloser')
            self.MoveBothCloser(player, partner)


    def SafeToRunAtGoal(self, player) :
        '''Any opposing player between me and shotPos? (ignoring run speeds)'''
        distance = Util.Distance(player.Position, player.ShotPos)
        for nbr, pl in self.OpponentPlayers.iteritems() :
            if pl.IsGoalkeeper :
                continue
            if Util.Distance(pl.Position, player.ShotPos) < distance :
                return False
        return True


    def GetSafePass(self, playerFrom, playerTo) :
        '''return the first safe pass or None'''
        pos1, pos2 = playerFrom.Position, playerTo.Position
        
        if playerTo.IsGoalkeeper :
            xAdjustList = [0]
        else :
            xAdjustList = [3.5,3,2,0,-3]
            for x in (5,6,7,8,9,10,11,12,13,14,15) :
                if Util.Distance(pos2, self.OpponentGoalPosition(True)) > x + 5 :
                    xAdjustList.append(x)

        for xAdjust in xAdjustList :
            x = xAdjust if self.DirectionRight else -xAdjust
            if self.IsPassOk(pos1, (pos2[0] + x, pos2[1])) :
                return xAdjust
          

    def IsPassOk(self, position1, position2, enemyDistanceThreshold = 2) :
        '''If the pass is 100% accurate would it be safe and valid?'''
        if not Util.PositionValid(position2, False, self.DirectionRight) :
            Util.Debug('Positions not valid - %s -> %s' % (position1, position2))
            return False
        dir = Util.Direction(position1, position2)
        oppositeDir = Util.OppositeDirection(dir)
        
        x = position1
        count = 0
        while not Util.PositionEqual(x, position2) and abs(dir - oppositeDir) > 0.0001 :
            count += 1
            if count == 100 :
                Util.Debug('infinite loop bug!!')
                return False # infinite loop bug workaround (until I can fix properly!!!) 
            x = Util.PositionFromDirAndDistance(x, dir, 0.5)
            dir = Util.Direction(x, position2)
            for nbr, pl in self.OpponentPlayers.iteritems() :
                pos = pl.Position
                if Util.Distance(x, pos) < enemyDistanceThreshold :
                    # close but is he looking the right way?
                    dir = Util.Direction(pos, x)
                    if Util.AngleBetween(dir, pos[2]) < 120 :
                        return False
        return True


    def MoveBothCloser(self, p1, p2) :
        dir = Util.Direction(p1.Position, p2.Position)
        p1.SetOneOffAction( Class.Action.Move(p1, Util.PositionFromDirAndDistance(p1.Position, dir, 2)) )
        dir = Util.OppositeDirection(dir)
        p2.SetOneOffAction( Class.Action.Move(p2, Util.PositionFromDirAndDistance(p2.Position, dir, 2)) )


    def MoveBothApart(self, p1, p2) :
        dir = Util.Direction(p1.Position, p2.Position)
        p2.SetOneOffAction( Class.Action.Move(p2, Util.PositionFromDirAndDistance(p2.Position, dir, 1)) )
        dir = Util.OppositeDirection(dir)
        p1.SetOneOffAction( Class.Action.Move(p1, Util.PositionFromDirAndDistance(p1.Position, dir, 1)) )


    def Pass(self, passFromPlayer, passToPlayer, xAdjust = 0, yAdjust = 0) :
        Util.Assert(passFromPlayer.Number != passToPlayer.Number)
        if xAdjust is None :
            xAdjust = 0
        if not self.DirectionRight :
            xAdjust *= -1
        passFromPlayer.SetActionGen( Util.PassBall(self, passFromPlayer, passToPlayer.Position, (xAdjust, yAdjust)) )
        passToPlayer.SetActionGen( Util.ReceiveBall(self, passFromPlayer.Position, passToPlayer ) )


    def PassQuick(self, passFromPlayer, passToPlayer, xAdjust = 0, yAdjust = 0) :
        Util.Assert(passFromPlayer.Number != passToPlayer.Number)
        # pass by kicking immediately - no time to turn.
        if not self.DirectionRight :
            xAdjust *= -1
        destination = (passToPlayer.Position[0] + xAdjust, passToPlayer.Position[1] + yAdjust)
        distance = Util.Distance(passFromPlayer.Position, destination)
        speed = self.Ball.GetSpeedForDistance(distance*1.2)
        def OneOffKickGen(pl, dest, sp) :
            yield Class.Action.Kick(pl, dest, sp)
        passFromPlayer.SetActionGen( OneOffKickGen(passFromPlayer, destination, speed) )
        passToPlayer.SetActionGen( Util.ReceiveBall(self, passFromPlayer.Position, passToPlayer ) )
        

    def __GetActions(self) :
        actions = []
        for p in self._Players() :
            a = p.GetAction()
            if a is not None :
                actions.append(a)
        return actions


    def __SetPositions(self) :
        # Positions set with the x axis origin at the centre and +ve is towards opponent goal
        self.__Goalie.SetKickOffPos( self.TranslatePos((-49.9, 23.9)) )

        self.__Back1.SetKickOffPos( self.TranslatePos((0, 0)) )
        self.__Back2.SetKickOffPos( self.TranslatePos((0, 50)) )

        if self.KickingOff :
            self.__Centre.SetKickOffPos( self.TranslatePos((0, 25)) )      # he will kick off
            self.__Forward1.SetKickOffPos( self.TranslatePos((-5, 25)) )
            self.__Forward2.SetKickOffPos( self.TranslatePos((-5, 40)) )

        else :
            self.__Centre.SetKickOffPos( self.TranslatePos((-11, 25)) )
            self.__Forward1.SetKickOffPos( self.TranslatePos((-1, 15)) )
            self.__Forward2.SetKickOffPos( self.TranslatePos((-1, 35)) )

        self.__Centre.SetGoalKickPos( self.TranslatePos((-25, 40)) )
        self.__Forward1.SetGoalKickPos( self.TranslatePos((2, 23)) )
        self.__Forward2.SetGoalKickPos( self.TranslatePos((-8, 37)) )
        
        
        pt = Util.GoalCirclePosition(300)
        self.__Forward1.SetShotPos(self.TranslatePos((pt[0] - 0.5, pt[1])))

        pt = Util.GoalCirclePosition(240)
        self.__Forward2.SetShotPos(self.TranslatePos((pt[0] - 0.5, pt[1])))

        pt = Util.GoalCirclePosition(270)
        self.__Centre.SetShotPos(self.TranslatePos((pt[0] - 0.5, pt[1])))
        

    def MoveToGoalKickPositions(self) :
        for p in (self.__Centre, self.__Forward1, self.__Forward2) :
            if not Util.PositionEqual( p.Position, p.GoalKickPos ) :
                p.SetOneOffAction(Class.Action.Move(p, p.GoalKickPos) )


    def TranslatePos(self, pos) :
        '''Return true x,y position. The x axis origin at the centre and +ve is towards opponent goal'''
        ret = (pos[0] + 50, pos[1]) if self.DirectionRight else (50 - pos[0], pos[1])
        Util.Assert(Util.PositionValid(ret))
        return ret


    def IsOpponentShotOnTarget(self) :
        '''Return False or Y coord of would-be goal'''
        ballDestination, _ = self.Ball.GetDestinationAndTimeSteps()
        #self.LogErr('ball dest[%s]' % str(ballDestination))
        
        xLimit = 0.01 if self.DirectionRight else 99.99
        if (self.DirectionRight and ballDestination[0] < xLimit) or \
            (not self.DirectionRight and ballDestination[0] > xLimit) :
        
            # c = y0 - mx0 
            # y1 = mx1 + (y0 - mx0)
            m = self.Ball.Gradient
            c = self.Ball.Position[1] - m * self.Ball.Position[0]
            y1 = m * xLimit + c
            #self.LogErr('Shot on target? y1[%.2f] m[%.2f] c[%.2f]' % (y1,m,c))
            if y1 >= Class.Pitch.GoalY[0] - 5 and y1 <= Class.Pitch.GoalY[1] + 5 :
                #self.LogErr('Shot on target: y1[%.2f]' % y1)
                return y1
        return False


    def IsBallInOurHalf(self) :
        return self.IsPosInOurHalf(self.Ball.Position)


    def IsPosInOurHalf(self, position) :
        if self.DirectionRight :
            return position[0] < Class.Pitch.Width / 2
        return position[0] > Class.Pitch.Width / 2


    def FindClosestTeamMate(self, player, playerNumbers = None) :
        '''return closest team-mate out of playerNumbers (default all) to player'''
        closest = (None, 10000.0) # player, distance
        for p in self._Players() :
            if playerNumbers and p.Number not in playerNumbers :
                continue

            if p.Number == player.Number or not p.IsPlaying :
                continue
            distance = Util.Distance(player.Position, p.Position)
            if distance < closest[1] :
                closest = (p, distance)
        return closest[0]


    def FindClosestOpponent(self, player) :
        '''return closest opponent to player'''
        closest = (None, 10000.0) # player, distance
        for nbr, pl in self.OpponentPlayers.iteritems() :
            pos = pl.Position
            distance = Util.Distance(player.Position, pos)
            if distance < closest[1] :
                closest = (nbr, distance)
        return closest


    @AiBase.LogFuncName
    def _OnEventGameStart(self, message) :
        """message just has pitch info handled by base class"""
        pass


    @AiBase.LogFuncName
    def _OnEventGoal(self, message) :
        """{"eventType":"GOAL","team1":{"name":"","score":2,"direction":"LEFT"},"team2":{"name":"","score":7,"direction":"RIGHT"},"messageType":"EVENT"}"""
        #raise Exception('GOAL')
    

    @AiBase.LogFuncName
    def _OnEventHalfTime(self, message) :
        self.__Goalie.OnHalfTime()


ai = Baldrick()