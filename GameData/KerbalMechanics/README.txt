Kerbal Mechanics: Part Failures
V0.5: Alpha

This software is provided "as-is" with no warranties.

Presented under the GPL v3 license.

Creation and/or publication of media (images, videos, etc.) while using this software is authorized.

Created by: Nifty255

Copyright 2014 All rights reserved.


This mod is in ALPHA. There will likely be bugs. The parts are not balanced. If you have a bug, or a suggestion, please leave your feedback in a mature manner.
Also, all parts currently included in the mod are NOT the IP of Nifty255 and will change before final release.


FEATURES:

Parts that can fail are tested based on their "reliability", which drains faster or slower, depending on the "quality".
The quality of a part also affects the cost of that part. Lower quality makes it cheaper, but also makes failure more likely.
Some parts also rely on not being shaken up too badly (excessively high g forces) to remain operational.
Each kind of failure is individually tested against the appropriate module.
As an example, an engine will have up to 4 different pieces that can fail:
- The ignition coil
- The cooling system
- The gimbal motor
- The alternator

Most failures have two options for fixing:
A standard fix, which will cost Rocket Parts, and a second, more silly option, which is free, but provides a risk of further damage.
The free option may need to be clicked more than once.

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

The thrust gauge can fail:
 - Excessively high g forces can damage the thrust gauge, causing it to display an incorrect thrust value.

A Reliability Monitor has been added:
 - The monitor also has quality and reliability, but cannot "break". Instead, reliability information becomes more inaccurate as reliability drains.
 - Through the Monitor GUI, parts with failed modules can be set to highlight red.
 - Through the Monitor GUI, a color map of the ship's status can be placed on the ship. Green for high reliability, yellow for aging, and red for very low.

Failures can be fixed by going on EVA, getting some spare Rocket Parts, right-clicking the part, and attempting the fix.
Most failure types have two repair options, one which costs Rocket Parts and is more reliable, and another, more silly option that is free, but may make the problem worse.
Be warned, though. Most proper repairs require an experienced Kerbal Engineer.

CHANGELOG:

v0.5:
- KSP 0.9 compatibility update. Fixed IPartCostModifier bug and part highlighting bug resulting from changes between 0.25 and 0.9.
- All proper repairs require a Kerbal with the Engineer skill, with varying degrees of experience. Improper repairs remain accessible by any Kerbal.
- All proper repairs can be done by any Kerbal of any experience if not in Career Mode.
- Reduced Decoupler chance of silent failure from 20% to 10%.
- Module Injector now searches all config files for injections, allowing separated injection files.
- Modified reliability drain logic to properly reflect the Kerbin 6 hour day.

v0.4.2:
- Fixed Decouplers not completing contract tests when either exploding or silently failing.
- Lowered decoupler default chance to explode from 12.5% to 6.25%.
- Lowered decoupler default chance to silently fail from 50% to 20%.
- Fixed fuel tanks with more than one resource initially leaking one resource, and occasionally reloading with a different one leaking.
- Fixed more variables not loading properly.
- Changed "reliabilityDrain" variable names to "lifeTime" to coincide with the fact that they hold the number of days a part will take to hit 0% reliability.
- Added Small Spare Parts Container to tech tree under "General Construction" and fixed the base price to 100 FUNds.
- Set RocketParts resource unit cost to 1 FUNd per unit of RocketParts, making a full Small Spare Parts Container cost 600 FUNds.

v0.4.1:
- Fixed the inability to fix the Altimeter and the Thrust Gauge.
- Fixed the Alternator fix action saying "Fix Altimeter" instead of "Fix Alternator".
- Fixed being able to perform additional actions on a spent decoupler.
- Changed Maintenance context action to read "Maintain <Module>" instead of the generic "Perform Maintenance".
- Changed the ship color map such that a part with a failed module will always appear red, regardless of the average reliability.

v0.4:
- Updated mod to KSP version 0.24.1.
- Added Funds support. Terrible quality parts (all modules 0%) cost 50% of the part's default cost. Default quality parts (all modules 75%) cost 100% of the default.
- Added thrust gauge failures. Excessively high g forces will cause the thrust gauge to display incorrect data. If multiple parts with gauge capabilities are present, all must fail to cause true failure.
- Added Reliability Monitor to all command parts. Has both quality and reliability, but no true failure. Low reliability results in inaccurate reliability data across the ship.
- Added a control panel to the Reliability Monitor that allows highlighting of parts with failed modules. Also allows the player to map the status of the ship's parts on the ship. (Green is good, red is bad.)
- Added option for quality changes in the editor to also affect parts in symetry with original part.

v0.3.1:
- Updated mod to KSP version 0.24.
- Fixed instant and automatic altimeter failures on all but one pod.
- IMPORTANT NOTE: Due to a fault in how part costs work, this version DOES NOT support funds. An urgent email to Squad support has already been sent.

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