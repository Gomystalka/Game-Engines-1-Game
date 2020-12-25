<h1>Game Engines 1 Assignment</h1>
<h2>Name: <b>Tomasz Galka</b></h2>
<h2>Student Number: <b>C18740411</b></h2>
<h2>Class Group: <b>DT508</b></h2>

<h1>CONTENT WARNING</h1>
<h2>This project contains a large amount of flashing lights which may trigger seizures in the vulnerable. Viewer discretion is advised!

<p>Unity Version: <b>Unity 2019.4.1f1</b>
<p>This project is being made for educational purposes only.</p>
<p>Any credits will be made within the game and on this repository</p>
<p><b>ALL</b> assets apart from <b>NAudio.dll, Audio.xml and MilkyWay Skybox</b> were created by me.</p>
<p>NAudio had to be used as Unity does not support .MP3 file streaming due to licensing issues.</p>

<h1>Description of the project</h1>
<p>Due to time constraints and many issues some previous features had to be scrapped. This project is a game/music visualizer made in the <b>Unity 2019.4.1f1</b> game Engine (Education Edition). The main purpose of this game is for music visualization and possible VJing due to one of the features. The project contains a procedurally-generated map using Perlin Noise and a custom terrain. This terrain is generated with correct UVs which allows one of the best features to work; the ability to display any video file on the terrain which gives it VJ potential. Both the video and audio files can be changed by the user which gives the game a lot of customizability.
The game's main purpose is once again, for visualization, however there are shooting mechanics implemented in order to make the game slightly more fun. The control scheme of the game was inspired by <b>Star Fox 64</b> created by <i>Nintendo</i>.</p>

<h1>Instructions for use</h1>
<p>The game should be fully usable when pulled/downloaded within the editor or when built within the editor. The custom video files must be put into the <i>StreamingAssets/Videos</i> folder to be detected by the game. This is the same for the audio file, except audio files must be placed within the <i>StreamingAssets/Music</i> folder instead. The game cannot pe played when there is no audio file loaded so make sure there is one present within the specified folder!

<h1>How it works</h1>
Under the hood, the game is powered by a large amount of systems. The brain of the game lies within the <b>AudioManager</b> class. This is the class responsible for 
analyzing the audio, splitting the audio into frequency bands, and detecting beats through the use of the <i>Frequency Energy</i> or <i>Spectral Flux</i> algorithms (Changeable however Frequency Energy proved to be more reliable.) The second most important class is the <b>AudioBehaviour</b> class. This class is an abstract class which exposes all Spectrum information to any class that inherits it. The <b>CustomVisualizedTerrain</b> class makes heavy use of it for hooking the OnBeat event and changing the terrain color and terrain shader line width based on the combined frequency data of the first, second and third band. This data is then mapped into the <i>HSV</i> color space for a rainbow effect. 

There is another important class which takes care of screen bounds collision, <b>ScreenBoundsColliderOld</b>. Old is in the name due to a previous naming mistake which I forgot to fix. This behaviour is achieved by first calculating the frustum corners of the camera and converting them into world space, then in order for all directions and rotations to function correctly, distance calculations and the Dot product is used in order to find out whether the player's distance is positive or negative (Outside the bounds). This function looks like this. It uses a combined enum for precise collision identification.
```
public CollisionLocation Constrain3DObject(Transform objectTransform, bool collisionInfoOnly = false) {
        yMidPoint = new Vector3(FrustumCorners[3].x, objectTransform.position.y, FrustumCorners[3].z);
        xMidPoint = FrustumCorners[3] - (FrustumCorners[3] - FrustumCorners[1]).normalized * Vector3.Distance(objectTransform.position, yMidPoint);

        float hDist = Vector3.Distance(yMidPoint, objectTransform.position);
        float vDist = Vector3.Distance(xMidPoint, objectTransform.position);
        float hDirDot = Vector3.Dot((yMidPoint - objectTransform.position).normalized, objectTransform.right);
        float vDirDot = Vector3.Dot((xMidPoint - objectTransform.position).normalized, objectTransform.up);
        hDist *= hDirDot < 0f ? -1f : 1f;
        vDist *= vDirDot < 0f ? -1f : 1f;
        CollisionLocation location = CollisionLocation.None;

        if (hDist <= 0f)
        {
            if(!collisionInfoOnly)
                objectTransform.position = new Vector3(yMidPoint.x, objectTransform.position.y, yMidPoint.z);
            location ^= CollisionLocation.Right;
        }

        if (hDist >= HorizontalDistance)
        {
            if(!collisionInfoOnly)
                objectTransform.position = new Vector3(FrustumCorners[0].x, objectTransform.position.y, FrustumCorners[0].z);
            location ^= CollisionLocation.Left;
        }

        if (vDist <= 0f)
        {
            if (!collisionInfoOnly)
                objectTransform.position = new Vector3(objectTransform.position.x, xMidPoint.y, objectTransform.position.z);
            location ^= CollisionLocation.Top;
        }

        if (vDist >= VerticalDistance)
        {
            if (!collisionInfoOnly)
                objectTransform.position = new Vector3(objectTransform.position.x, FrustumCorners[0].y, objectTransform.position.z);
            location ^= CollisionLocation.Bottom;
        }

        return location;
    }
```
<h1>Project Feature Overview</h1>
<ul>
 <li>Procedural Generation</li>
 <li>Audio Visualization</li>
 <
 <li>Custom Editor Inspector for the <b>AudioManager</b> class</li>
 <li>Toggleable Settings UI</li>
</ul>
<h1>Required Asset Overview</h1>
<ol>
 <li>Player 3D Asset (Possibly a spaceship of some kind)</li>
 <li>Weapon asset</li>
 <li>Possibly an enemy asset although enemies are planned to have procedurally-generated visuals</li>
</ol>
<h1>Current Development Milestones</h1>
<ul>
 <li>Project was created.</li>
 <li>AudioBehaviour class created (Abstract class exposing certain methods and fields to aid visualization.)</li>
</ul>
<h1>Gameplay Demo</h1>
<i>To be populated by a video showcasing the gameplay...</i>
<h1>Controls</h1>
<ul>
 N/A
</ul>
<h1>Credits</h1>
<ul>
 N/A
</ul>
<h1>Screenshots</h1>
<ul>
 N/A
</ul>
 
 <h2>Well, I guess <a href="https://www.urbandictionary.com/define.php?term=Ganbaruby"><b>GanbaRuby</b></a> to me.</h2>
<img src="https://media1.tenor.com/images/61bcbafc85870fdb1db95ac298f9b8f8/tenor.gif?itemid=7273202" width=50% height=50%/>

<h2>This is <b><u>NOT</u></b> a commercial project. It is completely free and always will be.</h2>

<h1>Previous Proposal</h1>
<p>This'll be a bullet-hell 3D game for which the enemies and certain parts of the map will be either randomly generated, generated based off spectrum data from an audio file of choice or preset but will react to music. The surrounding landscape will also change based on the audio file provided. The music will be freely interchangable by the player/user and most of the map will be generated as the music plays. There will be a length restriction placed on the imported audio file as to avoid both too long and too short clips.This assignment will be developed through the use of the <b>Unity 2019.4.1f1</b> Game Engine.</p>
<p>The overall difficulty of the map will be determined by the audio file provided. UI will consist of a generic menu with an option to select an audio file. Most if not all of the landscape/world will be procedurally-generated/generated relative to the spectrum data as the game plays. There will be no pre-processing of any kind therefore the game will not support pre-made maps.</p>
