# Throttle Chess Engine
<img  src="https://github.com/Dragjon/Throttle/blob/main/images/throttle.png" width="300" height="300"></img><br><br>![](https://img.shields.io/badge/Version-3.2.2-green)
![](https://img.shields.io/badge/CCRL_Elo_Estimate-2379-orange)
[![Lichess rapid rating](https://lichess-shield.vercel.app/api?username=Tokenstealer&format=rapid)](https://lichess.org/@/Tokenstealer/perf/rapid)
[![Lichess blitz rating](https://lichess-shield.vercel.app/api?username=Tokenstealer&format=blitz)](https://lichess.org/@/Tokenstealer/perf/blitz)
[![Lichess bullet rating](https://lichess-shield.vercel.app/api?username=Tokenstealer&format=bullet)](https://lichess.org/@/Tokenstealer/perf/bullet)<br><hr>
Throttle is a UCI chess engine developed using [Sebastian Lague's chess framework](https://github.com/SebLague/Chess-Challenge). <hr>

## Playing
You can either download my latest version in [releases](https://github.com/Dragjon/Throttle/releases) or play with me online at [lichess](https://lichess.org/@/TokenStealer) but note I may not always be online. If you choose to download me, also note that I am a UCI chess engine and I don't come with my own graphical interface. I would recommend you to use a GUI to test my engine if you are not familiar with the [UCI protocol](https://www.wbec-ridderkerk.nl/html/UCIProtocol.html) such as [Arena Chess GUI](http://www.playwitharena.de/) or [Banksia Chess GUI](https://banksiagui.com/).

## Lichess
Connected with lichess using this [lichess-bot tool](https://github.com/lichess-bot-devs/lichess-bot).

## Rating (Playing strength)
| Version | Estimate CCRL Elo | CCRL Blitz         | CCRL Bullet         | CEDR         |
|---------|-------------------|--------------------|---------------------|--------------|
| 3.1     | 2379.6 +/- 64.2   | -                  | -                   | -            |
| 3.0     | 2376.3 +/- 62.0   | -                  | -                   | -            |
* Estimated Elo is calculated against <a href="https://gitlab.com/mhouppin/stash-bot">Stash 14.0</a> (sorry stash!)

## Features
### Search:
- <a href="https://www.chessprogramming.org/Aspiration_Windows"> Aspiration window </a> ```v2.4``` ```16.6 +/- 9.6```
- <a href="https://www.chessprogramming.org/Negamax"> Fail-Soft Negamax Search </a>
- <a href="https://www.chessprogramming.org/Quiescence_Search"> Quiescence Search </a>
- <a href="https://www.chessprogramming.org/Principal_Variation_Search"> Principle Variation Search </a>
- Triple PVS ```v1.5``` ```21.3 +/- 11.1```

#### Pruning:
- <a href="https://www.chessprogramming.org/Transposition_Table">Actual TT pruning</a> ```v2.1``` ```136.4 +/- 31.2```
- <a href="https://en.wikipedia.org/wiki/Alpha%E2%80%93beta_pruning">A/B Pruning</a>
- <a href="https://www.chessprogramming.org/Null_Move_Pruning">Null Move Pruning</a>
- <a href="https://www.chessprogramming.org/Reverse_Futility_Pruning">Reverse Futility Pruning</a>
- <a href="https://www.chessprogramming.org/Futility_Pruning">Futility Pruning (depth*depth*margin)</a> ```v3.0``` ```15.7 +/- 9.2 vs Normal futility pruning```
- Quiescence Search Standing Pat Pruning
- <a href="https://www.chessprogramming.org/Delta_Pruning">Quiescence Search Delta Pruning</a> ```v2.5``` ```11.7 +/- 7.7```
- <a href="https://www.chessprogramming.org/Mate_Distance_Pruning">Mate Distancing Pruning</a>

#### Reductions:
- Internal Iterative Reduction
- <a href="https://www.chessprogramming.org/Late_Move_Reductions">Late move reductions with triple pvs</a>

#### Extensions:
- <a href="https://www.chessprogramming.org/Check_Extensions">Check Extensions</a>

#### Ordering:
- TT Moves
- <a href="https://www.chessprogramming.org/MVV-LVA">MVV-LVA (for good captures)</a>
- <a href="https://www.chessprogramming.org/Killer_Move">Killer moves (quiets)</a>
- <a href="https://www.chessprogramming.org/History_Heuristic">History moves (quiets)</a> ```v2.0``` ```67.9 +/- 20.9```

#### Time Management:
- Hard and Soft Time Management

### Evaluation:
- Tuned with Gedimas' [Texel Tuner](https://github.com/GediminasMasaitis/texel-tuner)
- <a href="https://minuskelvin.net/chesswiki/content/packed-eval.html">SWAR compression</a> ```v2.2``` ``` 5.7 +/- 4.6```
- Material values (PeSTO)
- Piece square tables (PeSTO)
- Tapered Eval
- Tempo
- Mobility

### UCI Functionality (adapted from gediminas' code)
#### Most basic UCI commands
- ```uci```
- ```setoption name [name] value [value]```
- ```position [startpos/FEN] [moves]```
- ```isready```
- ```go [wtime] [winc] [btime] [binc]```
#### Options
- ```Hash type spin default 201 min 1 max 1024``` - TT Hash size in MB
- ```rfpMargin type spin default 55 min 0 max 200``` - Reverse Futility Pruning margin
- ```futilityMargin type spin default 116 min 0 max 400``` - Futility Pruning margin
- ```hardBoundTimeRatio type spin default 10 min 1 max 100``` - Hard Bound time management ratio
- ```softBoundTimeRatio type spin default 40 min 1 max 300``` - Soft Bound time management ratio
#### Other nonstandard commands
- ```sevel``` - Get the static evaluation of the position

## Log of feature addition
```
Log after v2.1
---------------
v2.2 - SWAR test for compressed eval for speedup
Score of SWARtest vs Original: 5383 - 5132 - 4808  [0.508] 15323
...      SWARtest playing White: 3347 - 1892 - 2423  [0.595] 7662
...      SWARtest playing Black: 2036 - 3240 - 2385  [0.421] 7661
...      White vs Black: 6587 - 3928 - 4808  [0.587] 15323
Elo difference: 5.7 +/- 4.6, LOS: 99.3 %, DrawRatio: 31.4 %
SPRT: llr 2.95 (100.3%), lbound -2.94, ubound 2.94 - H1 was accepted

v2.3  - Check for non pv before pruning or reducing in tt
(COMPUTER BLUESCREENED HALFWAY SO THIS IS JUST PARTIAL RESULTS)
Score of NoTTCutoffsInPv vs Original: 2204 - 2080 - 1824  [0.510] 6108
...      NoTTCutoffsInPv playing White: 1359 - 788 - 908  [0.593] 3055
...      NoTTCutoffsInPv playing Black: 845 - 1292 - 916  [0.427] 3053
...      White vs Black: 2651 - 1633 - 1824  [0.583] 6108
Elo difference: 7.1 +/- 7.3, LOS: 97.1 %, DrawRatio: 29.9 %
SPRT: llr 1.64 (55.8%), lbound -2.94, ubound 2.94

v2.4 - Budget aspiration window search
Score of ASP vs Original: 1307 - 1139 - 1072  [0.524] 3518
...      ASP playing White: 810 - 433 - 516  [0.607] 1759
...      ASP playing Black: 497 - 706 - 556  [0.441] 1759
...      White vs Black: 1516 - 930 - 1072  [0.583] 3518
Elo difference: 16.6 +/- 9.6, LOS: 100.0 %, DrawRatio: 30.5 %
SPRT: llr 2.96 (100.4%), lbound -2.94, ubound 2.94 - H1 was accepted

v2.5 - Delta pruning
Score of DeltaPrune vs Original: 2000 - 1817 - 1615  [0.517] 5432
...      DeltaPrune playing White: 1252 - 668 - 796  [0.608] 2716
...      DeltaPrune playing Black: 748 - 1149 - 819  [0.426] 2716
...      White vs Black: 2401 - 1416 - 1615  [0.591] 5432
Elo difference: 11.7 +/- 7.7, LOS: 99.8 %, DrawRatio: 29.7 %
SPRT: llr 2.95 (100.2%), lbound -2.94, ubound 2.94 - H1 was accepted

v2.6 - Correct ASP window search
Score of ASP vs Original: 3391 - 3179 - 2721  [0.511] 9291
...      ASP playing White: 2084 - 1259 - 1304  [0.589] 4647
...      ASP playing Black: 1307 - 1920 - 1417  [0.434] 4644
...      White vs Black: 4004 - 2566 - 2721  [0.577] 9291
Elo difference: 7.9 +/- 5.9, LOS: 99.6 %, DrawRatio: 29.3 %
SPRT: llr 2.95 (100.3%), lbound -2.94, ubound 2.94 - H1 was accepted

v2.7 - Tuned eval + changed mobility
(BLUESCREENED)
Around 7 elo

v2.8 - Speed up evaluation - extract only at the end
Score of SpeedUp vs Original: 902 - 733 - 544  [0.539] 2179
...      SpeedUp playing White: 538 - 284 - 268  [0.617] 1090
...      SpeedUp playing Black: 364 - 449 - 276  [0.461] 1089
...      White vs Black: 987 - 648 - 544  [0.578] 2179
Elo difference: 27.0 +/- 12.6, LOS: 100.0 %, DrawRatio: 25.0 %
SPRT: llr 2.95 (100.1%), lbound -2.94, ubound 2.94 - H1 was accepted

v2.9 - RFP changed to 55;
...      RFP55 playing White: 3188 - 1958 - 1745  [0.589] 6891
...      RFP55 playing Black: 2148 - 3124 - 1617  [0.429] 6889
...      White vs Black: 6312 - 4106 - 3362  [0.580] 13780
Elo difference: 6.4 +/- 5.0, LOS: 99.4 %, DrawRatio: 24.4 %
SPRT: llr 2.95 (100.1%), lbound -2.94, ubound 2.94 - H1 was accepted

v3.0 - New futility pruning
Score of NewFP vs Original: 1646 - 1461 - 1002  [0.523] 4109
...      NewFP playing White: 953 - 603 - 499  [0.585] 2055
...      NewFP playing Black: 693 - 858 - 503  [0.460] 2054
...      White vs Black: 1811 - 1296 - 1002  [0.563] 4109
Elo difference: 15.7 +/- 9.2, LOS: 100.0 %, DrawRatio: 24.4 %
SPRT: llr 2.96 (100.5%), lbound -2.94, ubound 2.94 - H1 was accepted

v3.1 - Retuned eval with mobility
Score of ReplacedEval vs Original: 1807 - 1610 - 789  [0.523] 4206
...      ReplacedEval playing White: 1026 - 674 - 404  [0.584] 2104
...      ReplacedEval playing Black: 781 - 936 - 385  [0.463] 2102
...      White vs Black: 1962 - 1455 - 789  [0.560] 4206
Elo difference: 16.3 +/- 9.5, LOS: 100.0 %, DrawRatio: 18.8 %
SPRT: llr 2.96 (100.4%), lbound -2.94, ubound 2.94 - H1 was accepted
```

## Special thanks to (in no particular order)
- [Sebastian Lague](https://github.com/SebLague/) (For hosting the tiny chess bot tournament, although I couldn't submit on time)
- [Ciekce](https://github.com/Ciekce) (For helping with a lot of engine-related stuff)
- [cj5716](https://github.com/cj5716) (For helping with a lot of engine-related stuff)
- [arandomnoob(on discord)](https://github.com/mcthouacbb) (For helping with a lot of engine-related stuff)
- [Toanth](https://github.com/toanth/) (For hosting the 400 token challenge to really get me into chess programming)
- [Gediminas](https://github.com/GediminasMasaitis/) - for texel-tuner and stuff related to tuning
- [jw](https://github.com/jw1912) (nnue stuff and bullet)
- and many other people in sebastian lague's chess programming server / the engine programming server

## Engines I took inspiration from (in no particular order)
- [Stormphrax](https://github.com/Ciekce/Stormphrax)
- [Boychesser](https://github.com/analog-hors/Boychesser/)
- [Smol.cs / NOSPRT](https://github.com/cj5716/smol.cs)
- [200 token monstrosity](https://gist.github.com/mcthouacbb/2e87229fc971cd30762d6b481bdaac0b)
- [Atadofanoobbot](https://github.com/mcthouacbb/Chess-Challenge-400/blob/400tokens/Chess-Challenge/src/My%20Bot/AtadOfANoobBot.cs)
- [Viridithas](https://github.com/cosmobobak/viridithas/)
- [Ethereal](https://github.com/AndyGrant/Ethereal)
- [Altair](https://github.com/Alex2262/AltairChessEngine)

## Resources I found useful
- https://minuskelvin.net/chesswiki/ (Updated Chess programming wiki)
- https://www.chessprogramming.org/ (Old Wiki)
- https://github.com/cosmobobak/viridithas/blob/master/wiki.md (Small repository of search optims)
- https://discord.gg/S3MMh67q (Sebastian Lagues's chess programming serever)
- https://discord.gg/dVgkdqqt (Engine programming server)

## TODO
### Search
- [x] Aspiration window ```v2.4``` ```16.6 +/- 9.6```
- [x] Fail-Soft Negamax Search
- [x] Quiescence Search
- [x] Principle Variation Search
- [x] Triple PVS ```v1.5``` ```21.3 +/- 11.1```
- [ ] Lazy SMP multithreading
- [ ] Pondering
      
#### Pruning
- [x] Actual TT pruning ```v2.1``` ```136.4 +/- 31.2```
- [x] A/B Pruning
- [x] Null Move Pruning
- [x] Reverse Futility Pruning
- [x] Futility Pruning (depth*depth*margin) ```v3.0``` ```15.7 +/- 9.2 vs Normal futility pruning```
- [x] Quiescence Search Standing Pat Pruning
- [x] Quiescence Search Delta Pruning ```v2.5``` ```11.7 +/- 7.7```
- [x] Mate distancing pruning
- [ ] Static Exchange Evaluation Pruning (QSearch)
- [ ] Late Move Pruning
- [ ] Multicut
- [ ] Prob-cut
- [ ] History leaf pruning
- [ ] Parity pruning
- [ ] Uncertainty cutoffs
- [ ] AEL-Pruning
- [ ] Enhanced forward pruning

#### Reductions
- [x] Internal Iterative Reduction
- [x] Late move reductions with triple pvs
- [ ] Internal Iterative deepening
- [ ] Fail-high reductions
- [ ] Rank-cut
- [ ] Razoring

#### Extensions
- [x] Check Extensions
- [ ] Capture extensions
- [ ] Mate threat extensions
- [ ] One reply extension
- [ ] Passed pawn extension
- [ ] Pv extension
- [ ] Recapture extension
- [ ] Singular extensions

#### Ordering
- [x] TT Moves
- [x] MVV-LVA (for good captures)
- [x] iller moves (quiets)
- [x] History moves (quiets) ```v2.0``` ```67.9 +/- 20.9```
- [ ] History gravity
- [ ] History ageing
- [ ] History malus
- [ ] 1-ply continuation history
- [ ] 2-ply continuation history
- [ ] More extensive MVV-LVA ordering
- [ ] Capture history
- [ ] Counter-moves


#### Time Management
- [x] Hard Bound
- [x] Soft Bound 
      
### Evaluation
- [x] Piece Square Tables
- [x] Piece Weights
- [x] Tempo
- [x] Piece mobility (no king no pawn)

#### Pawn
- [ ] Pawn shield
- [ ] Pawn storm
- [ ] Strong pawn bonus
- [ ] Doubled pawn malus
- [ ] Passed pawn bonus
- [ ] Phalanx pawn bonus
- [ ] Isolated pawn malus

#### Piece
- [ ] Piece attacking enemy bonus
- [ ] Pawn defending piece bonus
- [ ] Trapped piece malus
      
##### General Major Pieces
- [ ] Major pieces on open file bonus
- [ ] Major piece on semi-open file bonus
      
##### General Minor Pieces
- [ ] Minor piece outpost bonus
- [ ] Undefended minor piece malus
      
##### Knight
- [ ] Pawn-count relative weight
- [ ] Blocking C-Pawn malus (in closed openings)
- [ ] Omit squares controlled by enemy pawns for mobility

##### Bishop
- [x] Bishop pair
- [ ] Bad bishop
- [ ] Color weakness (also considering majority of pawns on what colored square)
- [ ] Fianchettoed bishop bonus (a fianchettoed bishop shouldn't be exchanged)
- [ ] Returning bishop bonus

##### Rook
- [ ] Pawn-count relative weight
- [ ] Rook behind passed-pawns
- [ ] Rook blocked by uncastled king malus
- [ ] Rook connecting bonus

##### Queen
- [ ] Early development malus

##### King
- [x] King cornering for endgame checkmate (UNTUNED!!)
- [ ] King tropism
- [ ] King-relative PSQTs
- [ ] King Virtual mobility
- [ ] King on open file malus
- [ ] King on semi-open file malus
- [ ] King-relative to pawn chain malus
- [ ] Attacking king zone
- [ ] X-Rays/pins malus
- [ ] Castling rights
