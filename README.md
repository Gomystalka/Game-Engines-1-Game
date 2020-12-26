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
<p>The game should be fully usable when pulled/downloaded within the editor or when built within the editor. The custom video files must be put into the <i>StreamingAssets/Videos</i> folder to be detected by the game. This is the same for the audio file, except audio files must be placed within the <i>StreamingAssets/Music</i> folder instead. The game cannot pe played when there is no audio file loaded so make sure there is one present within the specified folder!</p>

<h1>How it works</h1>
<p>Under the hood, the game is powered by a large amount of systems. The brain of the game lies within the <b>AudioManager</b> class. This is the class responsible for analyzing the audio, splitting the audio into frequency bands, and detecting beats through the use of the <i>Frequency Energy</i> or <i>Spectral Flux</i> algorithms (Changeable however Frequency Energy proved to be more reliable.) The second most important class is the <b>AudioBehaviour</b> class. This class is an abstract class which exposes all Spectrum information to any class that inherits it. The <b>CustomVisualizedTerrain</b> class makes heavy use of it for hooking the OnBeat event and changing the terrain color and terrain shader line width based on the combined frequency data of the first, second and third band. This data is then mapped into the <i>HSV</i> color space for a rainbow effect.</p>

<p>There is another important class which takes care of screen bounds collision, <b>ScreenBoundsColliderOld</b>. Old is in the name due to a previous naming mistake which I forgot to fix. This behaviour is achieved by first calculating the frustum corners of the camera and converting them into world space, then in order for all directions and rotations to function correctly, distance calculations and the Dot product is used in order to find out whether the player's distance is positive or negative (Outside the bounds). This function looks like this. It uses a combined enum for precise collision identification.</p>

```CSharp
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
<p>There is also a <b>Player</b> and <b>Camera Controller</b> class which take care of the player movement through data fetched from a custom function in my <b>Utilities</b> class called <i>GetGoodAxis</i> which returns a smoothed value just like <i>Input.GetAxis("")</i> however the smooth rate is customizable and an annoying bug which snaps the axis value if the opposite button is pressed is completely fixed. This function is simple under the hood.</p>

```CSharp
private static Vector2 _goodAxis;
    private static Vector2 _goodAxisRaw;
    public static float inputInterpolationSpeed = 1f;

    public static float GetGoodAxis(string axis)
    {
        float value = 0f;
        if (axis == "Horizontal")
        {
            value = _goodAxis.x;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                value = Mathf.MoveTowards(_goodAxis.x, -1f, inputInterpolationSpeed * Time.deltaTime);
            else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                value = Mathf.MoveTowards(_goodAxis.x, 1f, inputInterpolationSpeed * Time.deltaTime);
            else
                value = Mathf.MoveTowards(_goodAxis.x, 0f, inputInterpolationSpeed * Time.deltaTime);
            _goodAxis = new Vector2(value, _goodAxis.y);
            return value;
        }
        else if (axis == "Vertical")
        {
            value = _goodAxis.y;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                value = Mathf.MoveTowards(_goodAxis.y, 1f, inputInterpolationSpeed * Time.deltaTime);
            else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                value = Mathf.MoveTowards(_goodAxis.y, -1f, inputInterpolationSpeed * Time.deltaTime);
            else
                value = Mathf.MoveTowards(_goodAxis.y, 0f, inputInterpolationSpeed * Time.deltaTime);

            _goodAxis = new Vector2(_goodAxis.x, value);

            return value;
        }
        return value;
    }
```

<p>The KeyCode values however are currently hardcoded but it would be difficult to specify custom values.</p>

<p>The visualization on the terrain is done through the use of custom mesh generation within the <b>CustomTerrain</b> class. The terrain first generates a plane with a set amount of vertices determined by the <i>Width</i> and <i>Height</i> parameters. The terrain is then divided up into Chunks which store the indices and X and Y positions of each vertex within said chunk. There is also a function within the Chunk object which remaps a X and Y value into the specified chunk's X and Y value, then clamps it. Through the use of this, a seamless terrain generation could be achieved by teleporting the terrain's start point to the player's forward once the player reaches a certain point on the terrain. When this point is reached, the last three chunks are copied and pasted into the first three chunks and the chunks after the first three are regenerated based on Perlin Noise data. The visualization of the video on the terrain was achieved through the use of the <b>Video Player</b> Unity component and a custom shader (<b>ScalableStandard</b>) which scaled the texture as for some reason Unity renders the video on the terrain at a very big scale by default. The terrain wireframe was also achieved through a modified built-in Unity shader (<b>WireframeShaderEx</b>).</p>
        
<p>The enemy spawns are determined by the beat of the audio file specified. A beat detection algorithm is used in order to detect beats within a song, and when one is detected, an enemy is spawned. In order to avoid too many enemies spawning, a time between spawns value was added. The shape of the enemy is determined by the data from the Spectrum at the time of the beat that spawns said enemy. In order to not cull backfaces for when the enemy faces become too distorted, the Standard Unity shader (<b>NoCullingStandardShader</b>) was modified to not Cull anything.</p>

<p>The lighting effects were added thorugh Unity's Post Processing package. Effects involved were mainly Bloom and Color Grading.</p>

<p>Finally, the loading of the files was achieved through the use of the <b>UnityWebRequest</b> class which loads a file from a URL which supports both HTTP and File protocols. The loading of the video file is natively supported by the Video Player component.</p>

<h1>References</h1>
<ul>
    <li><b>Star Fox 64</b> by <i>Nintento</i></li>
    <li><b>Minim for Processing</b> by <i>ddf/Compartmental</i></li>
    <li><b>Unity Docs</b></li>
    <li><b>Stackoverflow</b></li>
</ul>
        
<h1>What I am most proud of in the assignment</h1>
<p>I am very proud of how the custom terrain generation and visualization turned out. Despite my limited knowledge of ShaderLab, HLSL and mesh manipulation, I managed to create a fairly optimized and fully working terrain generation system which can be altered in many ways and supports many materials when the appropriate shader is used.</p>

<h1>Project Feature Overview</h1>
<ul>
 <li>Procedural Generation</li>
 <li>Audio Visualization</li>
 <li>Custom Editor Inspector for the <b>AudioManager</b> class</li>
 <li>Interchangable Video and Audio files</li>
 <li>Support for MP3 files thanks to NAudio!</li>
</ul>
<h1>Gameplay Demo</h1>
<a href="https://www.youtube.com/watch?v=oLE2dOjXtI4"><img src="http://img.youtube.com/vi/oLE2dOjXtI4/0.jpg" title="Game Engines 1 Assignment Submission - Space Game Thing"/></a>
<h1>Controls</h1>
<ul>
        <li><b>WSAD</b>: Move</li>
        <li><b>Left Mouse Button</b>: Fire Lasers</li>
        <li><b>Right Mouse Button</b>: Fire Rockets when you have a target</li>
</ul>

<h1>Credits</h1>
<ul>
 <li> Adam Bielecki (theadambielecki@gmail.com) - Amazing Skybox</li>
 <li> Mark Heath - NAudio - I love you.</li>
</ul>
<h2>This is <b><u>NOT</u></b> a commercial project. It is completely free and always will be.</h2>

<h1>Previous Proposal</h1>
<p>This'll be a bullet-hell 3D game for which the enemies and certain parts of the map will be either randomly generated, generated based off spectrum data from an audio file of choice or preset but will react to music. The surrounding landscape will also change based on the audio file provided. The music will be freely interchangable by the player/user and most of the map will be generated as the music plays. There will be a length restriction placed on the imported audio file as to avoid both too long and too short clips.This assignment will be developed through the use of the <b>Unity 2019.4.1f1</b> Game Engine.</p>
<p>The overall difficulty of the map will be determined by the audio file provided. UI will consist of a generic menu with an option to select an audio file. Most if not all of the landscape/world will be procedurally-generated/generated relative to the spectrum data as the game plays. There will be no pre-processing of any kind therefore the game will not support pre-made maps.</p>
