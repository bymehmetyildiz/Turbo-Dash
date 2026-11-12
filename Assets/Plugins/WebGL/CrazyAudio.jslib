mergeInto(LibraryManager.library, {
    // Reads the "mute audio" setting CrazyGames controls from its own UI.
    // The bundled Unity SDK (5.21) does not expose this field, so it is read from the JS SDK directly.
    CrazyIsAudioMuted: function () {
        try {
            var sdk = window.CrazyGames && window.CrazyGames.SDK;
            var settings = sdk && sdk.game && sdk.game.settings;
            return settings && settings.muteAudio ? 1 : 0;
        } catch (e) {
            return 0;
        }
    }
});
