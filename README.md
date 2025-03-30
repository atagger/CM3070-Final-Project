# CM3070-Final-Project
 Solulu - A Physics Based Driving Game

## About Solulu
Solulu is a low-polygon, third-person view arcade racing game set in a solar punk world, where the player races a vehicle against three artificial intelligence (AI) opponents on a racetrack. The goal is to reach the finish line in first, second or third place to advance to the next course and earn car upgrade points as rewards. Upgrade points are spent between levels to increase the vehicle’s performance and handling with top speed, acceleration, tire grip and suspension. The game borrows from classic racing games like R.C. Pro-Am and Super Off-Road, but reimagines them using Unity’s physics engine and a novel approach to car customization, aesthetics and level environments.

## Requirements
1. 2GB of space for the cloned repo. 9GB of space if you are planning on opening the project in Unity.
2. Solulu was developed in Unity 2021.3.45f1, please use this version if you open the files
3. You must use LFS when cloning the repo
4. A machine with >= 64GB ram, preferably Intel i5 / i7 or AMD Ryzen 5 / 7 series processor, 1080+ NViDIA card or equivalent graphics card suitable for playing video games on platforms like Steam

## Play Testing Game Builds
 There is a PC and a webGL build in the /Game Builds directory for play testing.
 It is not recommended to use the WebGL version, but it is there in case the grader does not have a PC.
 The game is not meant to be played or tested on tablets or phones.

## Controls
WASD or Arrows to Steer + Accelerate
S or Down Arrow will reverse car if stopped
Space Bar to hand brake (an extra option to try instead of down arrow or S for braking)
ESC for menu

## Asset Locations
Manager scripts are contained in the root level<br />
Assets parent folder: /Assets<br />
Scenes: /Scenes<br />
Scripts: /Assets/Scripts<br />
Car Tuning: /Scriptable Objects<br />
Animations: /Animations<br />
Audio Files: /Audio<br />
3D Models + FBX files: /Models<br />
Shaders: /Shaders<br />
Misc Assets: /Decals /Fonts /Images /Materials /Physics Materials /Prefabs /Textures<br />

 ## Scenes
 The project contains three scenes:
 1. Home - the main menu
 2. Level 1 - all the race tracks and game logic
 3. Upgrades - the upgrade manager (after winning a race)

 Level 1 is the recommended scene to open if you are a grader

## Usage
1. Clone the repository
2. Click 'YES' to the LFS option
3. Open in Unity 2021.3.45f1
4. If there are console WARNINGS on launch about the project assets needing Blender, open the Level 1 scene and they should go away. The other option is to install Blender which is a free program and relaunch.

I verified the clone works when the project was opened on a machine without Blender, you should be able to open it also.

## Authors and Acknowledgment
Programmed by Andreas Tagger<br />
All graphics and modelling done by Andreas Tagger<br />
Level music graciously donated by Sean Kosa<br />
Announcer voice performed by Nikki Delgado

## Conclusion
I hope you enjoying playing this as much as I enjoyed making it! :)