# PerformanceMeter
A Beat Saber mod to show a graph of your energy bar or percentage level throughout a map on the end screen.

![Image](screenshot.png)

## Requirements
* Beat Saber 1.13.2 or compatible
* BSIPA 4.1.4 or later
* Beat Saber Utils 1.7.0 or later
* BeatSaberMarkupLanguage 1.4.5 or later

## Installation
Simply drop the latest PerformanceMeter.dll plugin file into your Plugins folder, inside the main Beat Saber installation directory.

## Usage
Out of the box, PerformanceMeter displays a graph of the energy bar's status from the beginning of the level to the end, whether that's the end of the level or whenever you failed. This is shown on the level complete screen underneath the buttons. The lines are colored depending on the value of the endpoint and the color scheme of the selected mode.

You can change the type of data PerformanceMeter records in the Mod Settings. See below for more information on how to do this.

Note: PerformanceMeter only appears in Solo, Party, and Campaign game modes. It does not appear in online matches.

## Configuration
### UI
PerformanceMeter can be configured in the Mod Settings section of the options. Here you can enable/disable PerformanceMeter, as well as change the mode.

These are the modes available as of 1.1.0:

#### Energy
This mode records the level of the energy bar on every note hit or miss.

| Max % | Min % | Color  |
|-------|-------|--------|
| 100%  | 50%   | Green  |
| 49%   | 25%   | Yellow |
| 24%   | 0%    | Red    |

#### Percentage (Modified)
This mode records the percentage with all modifiers applied on every note hit or miss.

| Max % | Min % | Color  |
|-------|-------|--------|
| 100%+ | 90%   | Cyan   |
| 89%   | 80%   | White  |
| 79%   | 65%   | Green  |
| 64%   | 50%   | Yellow |
| 49%   | 35%   | Orange |
| 34%   | 0%    | Red    |

#### Percentage (Raw)
This mode records the percentage with no modifiers applied on every note hit or miss.

| Max % | Min % | Color  |
|-------|-------|--------|
| 100%  | 90%   | Cyan   |
| 89%   | 80%   | White  |
| 79%   | 65%   | Green  |
| 64%   | 50%   | Yellow |
| 49%   | 35%   | Orange |
| 34%   | 0%    | Red    |

#### Note Cut Value
This mode records the score given for each note cut.
| Max Score | Min Score | Color    |
|-----------|-----------|----------|
| 115       | 115       | White    |
| 114       | 101       | Green    |
| 100       | 90        | Yellow   |
| 89        | 80        | Orange   |
| 79        | 60        | Red      |
| 59        | 0         | Dark Red |

#### Average Cut Value
This mode records the average score of all cuts up to the current one on every note hit.

| Max Score | Min Score | Color    |
|-----------|-----------|----------|
| 115       | 115       | White    |
| 114       | 101       | Green    |
| 100       | 90        | Yellow   |
| 89        | 80        | Orange   |
| 79        | 60        | Red      |
| 59        | 0         | Dark Red |

### JSON
PerformanceMeter's configuration file is stored at `UserData\PerformanceMeter.json`. Here you can change some options regarding how PerformanceMeter looks and acts.

#### `enabled`
This toggles PerformanceMeter on and off. When set to `false`, recording is disabled and the graph will not be shown.

#### `mode`
This changes what data PerformanceMeter records in-game. These are the mappings between ID and mode statistic:

| ID | Statistic             |
|----|-----------------------|
| 0  | Energy                |
| 1  | Percentage (Modified) |
| 2  | Percentage (Raw)      |
| 3  | Note Cut Value        |
| 4  | Average Cut Value     |

More modes may be added in the future.

## License
PerformanceMeter is licensed under the MIT license.