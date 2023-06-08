# WIP

Fork of SonryP's VitaMote to make the app work on recent Android versions.

The plan is to remove the deprecated components and replace them with up-to-date alternatives, as well as depending less on Java and more on C#.

Planned improvements:

- [x] Remove the deprecated `Keyboard` and `KeyboardView` classes
- [x] Fix the custom keyboard not appearing (I wonder if it's needed at all)
- [x] Send keys from the PSVita to the app
- [x] Interact with other processes (create a new Input Method to send those keys to other processes)
- [ ] Send gamepad keys to a process based on the received PSVita keys (by putting the HTTP client in the Input Method Service, I think)
- [ ] Reduce input lag
