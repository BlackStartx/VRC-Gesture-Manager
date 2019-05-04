# VRC Gesture Manager

A simple script that will help you testing you animation before uploading your avatar to VRChat.  
Just download and import in your project the unitypackage that you can download here:  
https://vrcmods.com/user/BlackStartx (WIP)

In your Asset folder you will find one named "GestureManager", there you will find the prefab you need to drop in the scene.

![alt text](https://cdn.discordapp.com/attachments/561337898864082996/574264846250541059/Project.png)

Drop the prefab in to your scene and whenever you will enter in PlayMode the script will aim for the first active avatar that has an VRC_AvatarDescriptor whit at least one controller override.

![alt text](https://cdn.discordapp.com/attachments/561337898864082996/574267570690195456/OnPlay.PNG)

Your avatar should now enter into the "Animation pose" (Like the one on the image above).  
Now you can select the gesture to play directly in Unity, each gesture has the name of the animation file its releated.  
