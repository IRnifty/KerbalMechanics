Kerbal Mechanics: Part Failures
V0.3: Alpha

This software is provided "as-is" with no warranties.

Redistribution and/or modification of anything provided is STRICTLY PROHIBITED.

Creation and/or publication of media (images, videos, etc.) while using this software is authorized.

Created by: Nifty255

Copyright 2014 All rights reserved.


This mod is in ALPHA. There will likely be bugs. The parts are not balanced. If you have a bug, or a suggestion, please leave your feedback in a mature manner. Also, all parts currently included in the mod are NOT the IP of Nifty255 and will change before final release.


FEATURES:

Parts that can fail are tested based on their "reliability", which drains faster or slower, depending on the "quality".
Some parts also rely on not being shaken up too badly (excessively high g forces) to remain operational.
Each kind of failure is individually tested against the appropriate module.
As an example, an engine will have up to 4 different pieces that can fail:
- The ignition coil
- The cooling system
- The gimbal motor
- The alternator

Most failures have two options for fixing. A standard fix, which will cost Rocket Parts, and a second, more silly option, which is free, but provides a risk of further damage. The free option may need to be clicked more than once.

Engines can fail:
  - Ignition failures while throttling up from 0, preventing the engine from firing.
  - Cooling failures while the engine is running, causing greatly increased heat production.
  - Gimbal freezes while the engine is running, reducing maneuverability of the craft.
  - Alternators can fail due to engine burns or excessively high g forces.

Decouplers can fail:
  - While decoupling in any way, the decoupler may either decouple, do nothing, or explode.

Lights can fail:
  - Once a light has begun to fail, it will begin flickering for a few seconds, and then burn out.

Tanks can leak:
  - Over time, tank walls can weaken, leading to leaks. The size of a leak is random, and the leak is quickest the fuller the tank is, simulating pressure.

The altimeter can fail:
  - Excessively high g forces can damage the altimeter, causing it to display incorrect height data.

Failures can be fixed by going on EVA, getting some spare Rocket Parts, right-clicking the part, and attempting the fix. Most failure types have two repair options, one which costs Rocket Parts and is more reliable, and another, more silly option that is free, but may make the problem worse.

CHANGELOG:

v0.3:
- Modified reliability drain algorithm to use a curve, such that increases in quality past a certain point gives less and less decrease in reliability drain. The same about quality decrease is also true.
- Reliability drain now goes by Kerbin days instead of tiny percentages per 10 seconds, but the drains are still calculated every 10 seconds.
- Most modules now drain a small initial amount while idle, and an increased amount if the part is considered in operation.
- Added preventative maintenance. This allows the player to increase the reliability of old parts before failure occurs or after repairs at a cheap Rocket Parts cost.
- Added fuel tank leaks. Leaks become more likely the longer a craft remains. Once a leak is started, the amount leaked is initially large and leaks less as time goes on, simulating pressure.
- Added engine alternator failures. The alternator may fail as its reliability drains, or at a vastly increased likelyhood under excessively high g forces.
- Added altimeter failures. Excessively high g forces will cause the altimeter to display incorrect data. If multiple parts with altimeter capabilities are present, all must fail to cause true failure.

v0.2.3:
- Fixed bug that allowed use of lights while broken via the Lights button at the top of the flight screen.
- Reduced reliability drain of lights.

v0.2.2:
- Fixed bug that prevented proper loading of quality and reliability data.
- Fixed bugs in ModuleReliabilityIgnitor and ModuleReliabilityCooling that prevented proper fixing.

v0.2:
- The entire system has been rewritten, such that failure modules no longer inherit the module they cause to fail.
- Parts can have multiple failure capabilities.
- The mod no longer replaces stock parts, but instead injects its modules once the game starts.
- The engine's failure module has been split into three: Gimbal, Ignition, and Cooling
- The information on all failure modules on a given part can be viewed by right clicking the part and selecting "Reliability Stats"
- The quality of a part can be modified in the editor through the same window.
- A part with a broken module will now glow red until fixed.

v0.1:
- INITIAL RELEASE