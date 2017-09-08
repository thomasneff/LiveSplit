Yeah I know, the code is ugly, it started as a very small thing and became more complex step by step... It should definitely be rewritten, but I don't feel like it's worth it ¯\\\_(ツ)\_/¯

## LiveSplit.GameTimeCounter
This is a modification / adaptation of the LiveSplit.Counter component, which stores counter values *per split*, using the GameTime column/data in your splits. Therefore this is only applicable for games running in RealTime. You can use this for e.g. counting random encounters, deaths, ... *per split*.

LiveSplit.GameTimeCounter.dll:
Includes the main "Counter per Split" functionality. This has the same configuration as the "LiveSplit.Counter" component.
The counter is stored inside the GameTime of your splits. Therefore, if you run a game where GameTime is used, you
unfortunately can not use this component. You can also edit your counter values by editing in your splits editor, as you
know if for your Real Time splits. The counter stores its values in the "Seconds" of the GameTime.

Setup/Configuration:

To compare the counter values (for example: random encounters per split) you just need to add a column to your splits
which uses GameTime as the timing method. 

## Packages / Requirements

- If you plan on building this solution yourself, you may need to add references to the following dlls (the included versions may be outdated), all of which are distributed with LiveSplit: [http://livesplit.org/](http://livesplit.org/ "Livesplit Home").
	+ LiveSplit.Core.dll
	+ UpdateManger.dll
	+ WinformsColor.dll
