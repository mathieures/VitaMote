# WIP

Fork of SonryP's VitaMote to make the app work on recent Android versions.

The plan is to remove the deprecated components and replace them with up-to-date alternatives, as well as depending less on Java and more on C#.

Planned improvements:

- [ ] Remove the deprecated `Keyboard` and `KeyboardView` classes
- [ ] Fix the custom keyboard not appearing (I wonder if it's needed at all)
- [x] Send keys from the PSVita to the app
- [ ] Interact with other processes (create a new Input Method to send those keys to other processes)