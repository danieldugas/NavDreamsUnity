unity version: 2021.1.25f1

# Opening the Project in Unity

- make sure to clone with --recursive
- (to be fixed) after cloning, ensure WaveEnv/Assets/AddOns/Microsoft-Rocketbox points to remote git@github.com:danieldugas/Microsoft-Rocketbox.git, branch **waveenv**
- (to be fixed) make sure blender ver >2.9 is installed or unity will complain when importing some models
- open project in unity
- open Assets/Scenes/KozeHD.unity and press play

# Creating your own Scene

TODO
- Adding objects
- Adding spawns
- NavMesh

# Adding your own Robot

Follow this guide to import your robot from ROS to unity
https://github.com/siemens/ros-sharp/wiki/User_App_ROS_TransferURDFFromROS

Add hinge joints

TODO: default inputs
actions:
  - forward vel
  - side vel
  - rotation vel
  - difficulty
  - joints

TODO: adding your own joints

TODO: default outputs
  - camera
  - goal
  - velocity
