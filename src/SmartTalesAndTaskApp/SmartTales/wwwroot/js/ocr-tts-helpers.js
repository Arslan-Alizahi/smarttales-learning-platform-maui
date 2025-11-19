// Helper functions for OCR and TTS functionality

// Trigger file input click
window.triggerFileInput = function (inputId) {
    const element = document.getElementById(inputId);
    if (element) {
        element.click();
    }
};

// Set up audio callbacks
window.setupAudioCallbacks = function () {
    window.audioFinishedPromiseResolve = null;
};

// Play audio from base64 data
window.playAudio = function (base64Audio) {
    // Stop any currently playing audio
    window.stopAudio();
    
    // Create audio element if it doesn't exist
    if (!window.audioPlayer) {
        window.audioPlayer = new Audio();
    }
    
    // Set audio source
    window.audioPlayer.src = "data:audio/wav;base64," + base64Audio;
    
    // Play audio
    window.audioPlayer.play();
    
    // Return a promise that resolves when audio finishes
    return new Promise((resolve) => {
        window.audioFinishedPromiseResolve = resolve;
        window.audioPlayer.onended = function() {
            if (window.audioFinishedPromiseResolve) {
                window.audioFinishedPromiseResolve();
                window.audioFinishedPromiseResolve = null;
            }
        };
    });
};

// Audio finished callback
window.audioFinishedCallback = function () {
    return new Promise((resolve) => {
        if (!window.audioPlayer || window.audioPlayer.paused) {
            resolve();
        } else {
            window.audioFinishedPromiseResolve = resolve;
        }
    });
};

// Stop audio playback
window.stopAudio = function () {
    if (window.audioPlayer) {
        window.audioPlayer.pause();
        window.audioPlayer.currentTime = 0;
        
        // Resolve any pending promise
        if (window.audioFinishedPromiseResolve) {
            window.audioFinishedPromiseResolve();
            window.audioFinishedPromiseResolve = null;
        }
    }
}; 