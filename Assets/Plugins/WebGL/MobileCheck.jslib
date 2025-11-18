mergeInto(LibraryManager.library, {
    IsMobileBrowser: function () {
        // Simple user-agent check
        const isMobile = /Android|iPhone|iPad|iPod|Windows Phone|webOS|BlackBerry|Opera Mini|IEMobile/i
            .test(navigator.userAgent);

        return isMobile ? 1 : 0; // Return int because WebGL can't return bool
    }
});
