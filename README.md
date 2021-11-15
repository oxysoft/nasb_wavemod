# Wavemod

Wavemod is a mod for Nickelodeon All Star Brawl! adding a collection of new input mechanics, aiming to increase the speed of the game even further.
Some of these are simple design or preferencial tweaks that arguably make no difference in a match, while others clearly give an advantage
with sufficient mastery. The goal is to improve on certain aspects of the movement and controls design I think are problematic, increasing the sense of player/character connection.

## Disclaimers

- Currently in development, features and buttons are hardcoded and not configurable.
- Does not appear to work in online, needs debugging.
- Not yet acknowledged nor approved by Ludosity, though I don't think anyone will complain unless it becomes a real problem that needs to be addressed. Have fun, go crazy, I think it's all gonna turn out good in the end.

## Wavedash notation

- PWD
- AWD

TODO

# Features

## Ground mods

TODO

## Aerial mods

TODO


## Ledge mods

TODO

## Sidewave buttons

TODO

## Buffered inputs

The visual telegraphing in NASB isn't as clear as in Melee, making it harder to get a sense of what is happening and reacting with precision when things happen at a really high speed. To work around this, this mod implements more aggressive buffering in certain scenarios to remove some frustrating timings. All buffered inputs have been carefully selected to make the game's movement smooth like butter.

- All three attack inputs during the following states will come out on the first frame possible: wavedash/airdash.
- Sidewave is buffered as much as possible

# Roadmap

## Real Joystick

Reading the real joystick inputs for use in our postprocessor will allow us to detect taps in a far more accurate manner. Currently, fastfalling is EXTREMELY sensitive because it's purely a threshold whereas it should be based on joystick velocity.

## PWD/AWD angle fix

In NASB, wavedash distance is a binary operation, you just choose between PWD or AWD. In Melee, it's an infinite spectrum of angles, and the value of each angle changes for every frame depending on how far you are from the ground.

No matter how you look at it, joystick users are handicapped against D-pad or keyboard users who can selectively pick between the two states. Therefore we buff the joystick as much as possible.

When within short distance of the ground, we modify downward airdash angles to be much easier to hit on the horizontal axis. With sidewave buttons, this is always a PWD.

## Wavedash extension

Because wavedashes can come out instantly in one frame, we can extend an airborn airdash input by a number of frames if we want. This confers no advantage and gives the player some artificial satisfaction by seemingly giving the impression you wavedashed on a super late frame. The frames should be kept low (1-3 max) to avoid creating problems with sticky inputs.

## Artificial momentum

By pressing the horizontal directions in alternating pulses, we can possibly implement _artificial momentum_ in aerial states. For example, you could carry your momentum forward when jumping like in melee. You could wavedash, buffer a jump input, and keep a little bit of that horizontal momentum during your jump. A lot of possibilities come to mind!

- Pressing any directional input would mix them with the velocity pulses.
- Pressing the opposite horizontal direction would quickly take away from the artificial momentum, as expected.

We can also implement it for ground states to some extent, but it might feel odd as you'd see the run animations.
