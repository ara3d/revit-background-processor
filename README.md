# Revit Background Processor

Perform computations in the background without blocking the Revit UI.  

https://github.com/user-attachments/assets/4e41f514-31ef-4b3d-a33e-1a63f4e9f85b

# About 

This code contains a library for executing code in the background of Revit with 
blocking the UI. 

It uses a queue to contain work items and uses the Revit [UIApplication.Idling](https://www.revitapidocs.com/2019/e233027b-ba8c-0bd1-37b7-93a066efa5a3.htm) 
event to process items from the queue, within an alotted time slot. 

This address the issue that Revit is single-threaded. 

# Difference from Revit.Async

[Revit.Async](https://github.com/KennanChan/Revit.Async) solves the problem of assuring that code 
is executed within a valid Revit context. This is achieved to a lesser degree by the `ApiContext`
class provided here, or by executing code during an `Idling` event. 

# About the Code

This code is not a standalone plug-in, the `BackgroundProcessor.cs` and `ApiContext.cs` 
files are intended to be reused in your other projects. The `BackgroundForm.cs` Windows Form can 
be used for debugging purposes. 

The development version of this code can be found in the 
[Bowerbird repository](https://github.com/ara3d/bowerbird/blob/main/Ara3D.Bowerbird.RevitSamples/BackgroundProcessor.cs)

# Feedback and Contributions 

We warmly welcome contributions and feedback. 
We would appreciate learning about any usage of this project. 

# Acknowledgement

This work is sponsored by HOK. 
Special thanks to Greg Schleussner. 





