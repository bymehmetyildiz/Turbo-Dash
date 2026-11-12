mergeInto(LibraryManager.library, {
    // Caps the resolution Unity renders at. Unity reads Module.devicePixelRatio when it sizes the canvas,
    // so lowering it reduces the number of pixels drawn without changing the size of the game on the page.
    SetMaxCanvasPixelRatio: function (maxRatio) {
        var ratio = window.devicePixelRatio || 1;
        Module.devicePixelRatio = Math.min(ratio, maxRatio);
    }
});
