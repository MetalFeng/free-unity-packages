Download and import the untiyproject file into your project on the editor by going to the top left corner of the editor:

Assets -> Import Package -> Custom Package -> .unitypackage file

Attach the AnchoredTouchMovement2D Script to any GameObject with a Rigidbody2D Component to move using touch anywhere on screen.

Use the AnchoredTouchMovement3D Script for Rigidbody3D GameObjects.

You will need Canvas and EventSystem GameObjects in the scene for it to work, as there are functionalities in place to avoid registering touches on UI Elements.

Suggested Settings on Rigidbody2D/3D Component:

- Disable Gravity, Set Gravity to 0.
- Constraint all Rotation except the ones you need, Constraint Position on the unused axis.
- Turn Interpolate property to Interpolate.
- Turn Collision Detection to either Continuous or Continuous Dynamic.
- Go to Edit > Project Settings > Time and decrease the “Fixed Time Step” for even smoother physics and movement. (Can be performance heavy for physics intensive games.)

It works for both Touch Screen and Mouse Controls.

I hope you make a lot of fun and interesting games with this Script. You are free to modify and use it any way you like for commercial and non-commercial projects.
