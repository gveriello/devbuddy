window.cryptoInterop = {
    async deriveKey(passwordBase64, saltBase64, iterations, keyLength) {
        const password = new Uint8Array(atob(passwordBase64).split('').map(c => c.charCodeAt(0)));
        const salt = new Uint8Array(atob(saltBase64).split('').map(c => c.charCodeAt(0)));

        const importedKey = await crypto.subtle.importKey(
            'raw',
            password,
            'PBKDF2',
            false,
            ['deriveBits']
        );

        const derivedBits = await crypto.subtle.deriveBits(
            {
                name: 'PBKDF2',
                salt: salt,
                iterations: iterations,
                hash: 'SHA-256'
            },
            importedKey,
            keyLength * 8
        );

        return btoa(String.fromCharCode(...new Uint8Array(derivedBits)));
    },

    getRandomBytes(length) {
        const array = new Uint8Array(length);
        crypto.getRandomValues(array);
        return array;
    },

    async encrypt(keyBase64, ivBase64, dataBase64) {
        const key = new Uint8Array(atob(keyBase64).split('').map(c => c.charCodeAt(0)));
        const iv = new Uint8Array(atob(ivBase64).split('').map(c => c.charCodeAt(0)));
        const data = new Uint8Array(atob(dataBase64).split('').map(c => c.charCodeAt(0)));

        const importedKey = await crypto.subtle.importKey(
            'raw',
            key,
            { name: 'AES-CBC', length: 256 },
            false,
            ['encrypt']
        );

        const encrypted = await crypto.subtle.encrypt(
            { name: 'AES-CBC', iv: iv },
            importedKey,
            data
        );

        return btoa(String.fromCharCode(...new Uint8Array(encrypted)));
    },

    async decrypt(keyBase64, ivBase64, dataBase64) {
        const key = new Uint8Array(atob(keyBase64).split('').map(c => c.charCodeAt(0)));
        const iv = new Uint8Array(atob(ivBase64).split('').map(c => c.charCodeAt(0)));
        const data = new Uint8Array(atob(dataBase64).split('').map(c => c.charCodeAt(0)));

        const importedKey = await crypto.subtle.importKey(
            'raw',
            key,
            { name: 'AES-CBC', length: 256 },
            false,
            ['decrypt']
        );

        const decrypted = await crypto.subtle.decrypt(
            { name: 'AES-CBC', iv: iv },
            importedKey,
            data
        );

        return btoa(String.fromCharCode(...new Uint8Array(decrypted)));
    }
};