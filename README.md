# Wavemod

Wavemod is a mod for Nickelodeon All Star Brawl! adding a collection of new input mechanics, aiming to increase the speed of the game even further.
Some of these are simple design or preferencial tweaks that arguably make no difference in a match, while others clearly give an advantage
with sufficient mastery. The goal is to improve on certain aspects of the movement and controls design I think are problematic, increasing the sense of player/character connection.

## Disclaimers

- Currently in development, features and buttons are hardcoded and not configurable.
- Does not appear to work in online, needs debugging.
- Not yet acknowledged nor approved by Ludosity, though I don't think anyone will complain unless it becomes a real problem that needs to be addressed. Have fun, go crazy, I think it's all gonna turn out good in the end.

## Wavedash notation

- PWD: refers to a perfect wavedash, holding left/right with the max distance possible.
- AWD: refers to a shorter wavedash in diagonal, with down held.

# Features

## Ground mods

- Configurable auto-strafe (refer to STRAFE LOGIC)
- Crouch Slide: left/right while crouching to PWD.
- Run slide: down while running to PWD.
- Buffered attacks: inputing a strong attack at any time during a wavedash will buffer the input and fire it as soon as landlag is over. TODO
- Instant turnarounds FIX: the first turnaround is not instant, only consecutive dash dancing. This feels like crap, so this feature buffers inputs on the next 2 frames to do an instant dash dance. TODO
- Platform tap drop: tap the joystick down to fall from a platform. TODO

## Aerial mods

- Configurable auto-strafe (refer to STRAFE LOGIC)
- Hold fall: hold down while falling to airdash down (delayed for ledge drops)
- Tap fall: tap down to airdash down in midair (buffered)
- C-stick airdash: airdash with cstick (off by default, feel free to try it)


## Ledge mods

- DOWN to let go of ledge

## Sidewave buttons

This mod adds 2 new buttons, I call them SIDEWAVE LEFT and SIDEWAVE RIGHT. (usually placed on triggers)
They re-imagine wavedashing as a free sidestepping mechanic and decouple airdash from the joystick.

Contextual inputs:

- Grounded: PWD left/right. DOWN to AWD, UP to dash diagonally up.
- Airborn: AWD left/right, or PWD if still within PWD frame window. UP to dash diagonally up. L/R to dash left/right.
- Ledge: PWD getup onto stage.


## Buffered inputs

The visual telegraphing in NASB isn't as clear as in Melee, making it harder to get a sense of what is happening and reacting with precision when things happen at a really high speed. To work around this, this mod implements more aggressive buffering in certain scenarios to remove some frustrating timings. All buffered inputs have been carefully selected to make the game's movement smooth like butter.

- All three attack inputs during the following states will come out on the first frame possible: wavedash/airdash.
- Sidewave is buffered as much as possible

# Roadmap

## Real Joystick

Reading the real joystick inputs for use in our postprocessor will allow us to detect taps in a far more accurate manner. Currently, fastfalling is EXTREMELY sensitive because it's purely a threshold whereas it should be based on joystick velocity.

## Automatic PWD/AWD affinity (joystick)

In NASB, wavedash distance is a binary operation, you just choose between PWD or AWD. In Melee, it's an infinite spectrum of angles, and the value of each angle changes for every frame depending on how far you are from the ground. No matter how you look at it, joystick users are handicapped against D-pad or keyboard users who can explicitly choose between the two with no room for error.

Thus, we can manipulate the boundary so the PWD zone grows bigger the closer you are to the ground, since it's more likely to be what you want. Players who prefer to stick with classic SSBM style wavedashing should enjoy this feature.

## Waveland coyote frames

Because wavedashes can come out instantly in one frame, we can continue to detect waveland inputs even after landing. It's make waveland a little forgiving, and gives player an illusion of satisfaction from getting the waveland on a super late frame despite actually overshooting it. The frames should be kept low (1-3 max) to avoid creating problems with sticky inputs.

## Artificial Momentum

By pressing the horizontal directions in alternating pulses, it might be possibe to invent _artificial momentum_. In melee, you could maintain your momentum in mid-air with the joystick untouched. This creates short moments where the joystick is decoupled from movement, raising the skill ceiling of the game. With this, it would be possible to wavedash, buffer a jump input, and keep a little bit of that horizontal momentum during your jump. A lot of possibilities come to mind! 

- Pressing any directional input would mix them with the velocity pulses.
- Pressing any attack should completely override the pulses, to ensure the correct direction is used.  
- Pressing the opposite horizontal direction would quickly take away from the artificial momentum.
- Many values to configure.

We can also implement it for ground states to some extent, but it might feel odd as you'd see the run animations.
