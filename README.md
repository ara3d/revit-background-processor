# Revit Background Processor

Perform computations in the background without blocking the Revit UI.  

https://github.com/user-attachments/assets/4e41f514-31ef-4b3d-a33e-1a63f4e9f85b

# About 

This code contains a library for executing code in the background of Revit with 
blocking the UI. 

It uses a queue to contain work items and uses the Revit [UIApplication.Idling](https://www.revitapidocs.com/2019/e233027b-ba8c-0bd1-37b7-93a066efa5a3.htm) 
event to process items from the queue, within an alotted time slot. 

This addresses the issue that Revit is single-threaded. 

# Next Steps 

There challenges remain: 
1. the frequency of idle events is inconsistent
2. do not happen if Revit is a background state. 
3. When "idle" message is too frequent the CPU resources may get hogged    

Jeremy Tammik describes the issue here, and suggests using an External event: 
https://thebuildingcoder.typepad.com/blog/2013/12/replacing-an-idling-event-handler-by-an-external-event.html

This could be added as an option to the current library: by creating an external event and a thread that triggers work periodically.  

However two outstanding problems with that approach are:

1. Revit does not actually call the external event Execute method until a cursor movement or some other system event wakes it up.
2. External events won't fire if Revit doesn't have focus.
   
The beginnings of a solution are decribed here: https://adndevblog.typepad.com/aec/2013/07/tricks-to-force-trigger-idling-event.html, but note 
that the code there does not actually work.

Some options to explore are the following, which execute either on a separate thread or process,:

1. explicitly moving the mouse a tiny bit
2. posting the windows message for a mouse move event to the main Revit window

The final solution may involve chained idle events, along with an external event that checks for changes less frequently  

More empirical data needs to be gathered.  

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
Thank you to Jeremey Tammik for valuable feedback. 
Special thanks to Greg Schleusner for starting and organizing the project.  





