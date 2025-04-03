window.playSystemSound = function (soundType) {
    const context = new (window.AudioContext || window.webkitAudioContext)();
    const oscillator = context.createOscillator();
    const gainNode = context.createGain();

    oscillator.connect(gainNode);
    gainNode.connect(context.destination);

    // Configura il suono in base al tipo
    switch (soundType) {
        case 'asterisk':
            oscillator.frequency.setValueAtTime(800, context.currentTime);
            oscillator.type = 'sine';
            break;
        case 'beep':
            oscillator.frequency.setValueAtTime(600, context.currentTime);
            oscillator.type = 'square';
            break;
        case 'exclamation':
            oscillator.frequency.setValueAtTime(900, context.currentTime);
            oscillator.type = 'triangle';
            break;
        case 'error':
            oscillator.frequency.setValueAtTime(400, context.currentTime);
            oscillator.type = 'sawtooth';
            break;
        case 'question':
            oscillator.frequency.setValueAtTime(700, context.currentTime);
            oscillator.type = 'sine';
            break;
    }

    // Configura il volume (fade out)
    gainNode.gain.setValueAtTime(0.3, context.currentTime);
    gainNode.gain.exponentialRampToValueAtTime(0.001, context.currentTime + 0.5);

    oscillator.start(context.currentTime);
    oscillator.stop(context.currentTime + 0.5);
};