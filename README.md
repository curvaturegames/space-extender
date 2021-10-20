# space-extender
Space Extender: A Unity Plugin to Aid Level Design for VR Experiences with Natural Locomotion

### Logging redirection info

Documentation for SpaceExtenderLoggingManager
Status 11.10.2021

SpaceExtenderLoggingManager is a tool to log specific data from the Rotation Redirector.

**What is getting logged?**
The tool is saving three different pieces of information at this moment. The Name of the redirector (name of gameobject the redirector script is on), the time it took to redirect and the total of rotations in degree performed in real life. 

**When is it getting logged?**
The information is getting collected during the redirection and saved after the redirection is completed. (This could also be changed to periodic logging if needed.)

**Where is it getting logged?**
The collected information is saved in an CSV file in the game data folder on the target device. (cf. https://docs.unity3d.com/ScriptReference/Application-dataPath.html) 
The File will be called “SpaceExtenderLogging” + the current Date and Time. 

**How to activate logging?**
The Script “SpaceExtenderLoggingManager” contains a boolean called LoggingEnabled. The tool only logs the information if it is set to true. 
Simply add an Empty to your scene and add the “SpaceExtenderLoggingManager” script to it. After this, the Rotation Redirector will send its Information to the Logging Manager and a file with the information is getting created. 

**Change what the tool is logging?**
To change the information that is getting collected and then saved in a file, there are multiple steps necessary. 
To Change the data that is supposed to be logged, check the three functions “StartLogging”, “UpdateLogging” and “EndLogging” in “RotationRedirector”. The first one is to collect data at the beginning of the redirection. 
The second is to collect data during each step of redirection, and the last one is for after the redirection is completed. 
Add other variables to log your own data. 
IMPORTANT: To also save those values in the CSV file you need to change the “LogData” function in “SpaceExtenderLoggingManager” and the call of the function in “RotationRedirector” in “EndLogging”.