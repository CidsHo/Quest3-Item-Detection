# Quest3-Item-Detection
A methods to detect item via Quest3 camera.

# Requirements

- **Hardware**  
  - NVIDIA GPU (for AI processing).

- **Software**  
  - Python 3.11.7.  
  - Unity 2022.  
  - OBS.

# Implementation Principles

1. **Screen Casting**  
   Cast the Quest's video passthrough view to the PC.

2. **Virtual Camera Capture**  
   Use OBS's virtual camera to capture the screen-casted view from the Quest.

3. **AI Object Detection**  
   Utilize AI developed in Python (using YOLO v8 in this tool) to analyze the captured virtual camera feed for real-time object recognition.

4. **Network Communication**  
   Establish a network connection between the Python program on the PC and the Unity program on the Quest.

5. **Data Transfer**  
   Send the AI detection results back to the Unity program on the Quest in JSON format.

6. **Result Display**  
   Unity parses the received JSON data and displays the detection results on the Quest screen.

# Important Notes

- **Cannot Run Independently**  
  The application requires the Quest and the PC to be connected to the same network.

- **2D Screen Analysis Only**  
  The AI currently analyzes the 2D screen-casted view. It does not restore depth or 3D spatial information into the Quest.

- **Susceptible to Virtual Object Interference**  
  The performance may be affected by interference from virtual objects.
