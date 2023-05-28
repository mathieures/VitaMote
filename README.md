# WIP

Fork of SonryP's VitaMote to make the app work on recent Android versions.

The plan is to remove the deprecated components and replace them with up-to-date alternatives, as well as depending less on Java and more on C#.

To fix:

- The `Keyboard` and `KeyboardView` classes are deprecated
- The custom keyboard does not appear in the list of input methods (and I wonder if it's needed at all)

Planned improvements:

- [x] Put some variables in arrays/lists (`b1, b2, ..., b8` for example)
- [x] Send key events from the PSVita to the app
- [ ] Interact with the running process thanks to those keys

