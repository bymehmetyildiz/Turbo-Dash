mergeInto(LibraryManager.library, {
    $TurboDashSave: {
        key: 'turbo_dash.save.v1',
        memory: null,
        read: function () {
            if (this.memory) return this.memory;
            try {
                var raw = localStorage.getItem(this.key);
                var parsed = raw ? JSON.parse(raw) : {};
                this.memory = parsed && typeof parsed === 'object' && !Array.isArray(parsed) ? parsed : {};
            } catch (error) { console.warn('Turbo Dash save read failed', error); this.memory = {}; }
            return this.memory;
        },
        write: function (data) {
            this.memory = data;
            try { localStorage.setItem(this.key, JSON.stringify(data)); }
            catch (error) { console.warn('Turbo Dash save is session-only', error); }
        }
    },
    TurboDashStorageGet__deps: ['$TurboDashSave'],
    TurboDashStorageGet: function (keyPtr) {
        var data = TurboDashSave.read(), key = UTF8ToString(keyPtr);
        if (!Object.prototype.hasOwnProperty.call(data, key)) return 0;
        var value = String(data[key]), size = lengthBytesUTF8(value) + 1;
        var ptr = _malloc(size);
        stringToUTF8(value, ptr, size);
        return ptr;
    },
    TurboDashStorageSet__deps: ['$TurboDashSave'],
    TurboDashStorageSet: function (keyPtr, valuePtr) {
        var data = TurboDashSave.read();
        Object.defineProperty(data, UTF8ToString(keyPtr), { value: UTF8ToString(valuePtr), enumerable: true, writable: true, configurable: true });
        TurboDashSave.write(data);
    },
    TurboDashStorageDelete__deps: ['$TurboDashSave'],
    TurboDashStorageDelete: function (keyPtr) {
        var data = TurboDashSave.read(); delete data[UTF8ToString(keyPtr)]; TurboDashSave.write(data);
    },
    TurboDashStorageClear__deps: ['$TurboDashSave'],
    TurboDashStorageClear: function () { TurboDashSave.write({}); }
});
