# WIP

Fork of SonryP's VitaMote to make the app work on recent Android versions.

The plan is to remove the deprecated components and replace them with up-to-date alternatives, as well as depending less on Java and more on C#.

Planned improvements:

- [x] Remove the deprecated `Keyboard` and `KeyboardView` classes
- [x] Fix the custom keyboard not appearing (I wonder if it's needed at all)
- [x] Send keys from the PSVita to the app
- [x] Interact with other processes (create a new Input Method Service to send those keys to other processes)
- [x] Send gamepad keys to a process based on the received PSVita keys (parse the keys in the Input Method Service)
- [ ] Reduce input lag

Impossible to improve:

- When the app is closed, the PSVita still says it is "Connected", but it is not (limitation of VitaPad). The only solution is to restart the VitaPad app on the PSVita.