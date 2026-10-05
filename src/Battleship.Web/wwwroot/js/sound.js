// Game sounds, generated with the Web Audio API, so there are no audio files.
// Called from C# through SoundService.

const MasterVolume = 0.35;

let context;
let master;

export function play(name) {
    const ctx = audioContext();
    sounds[name]?.(ctx, ctx.currentTime);
}

// Browsers only allow audio after the player has clicked something,
// so the context is created on the first sound instead of at page load
function audioContext() {
    if (!context) {
        context = new AudioContext();
        master = context.createGain();
        master.gain.value = MasterVolume;
        master.connect(context.destination);
    }

    if (context.state === "suspended")
        context.resume();

    return context;
}

// A tone that slides from one pitch to another while fading out
function tone(ctx, start, { duration, type = "sine", from, to = from, volume }) {
    const oscillator = ctx.createOscillator();
    oscillator.type = type;
    oscillator.frequency.setValueAtTime(from, start);
    oscillator.frequency.exponentialRampToValueAtTime(to, start + duration);

    const gain = ctx.createGain();
    gain.gain.setValueAtTime(0.0001, start);
    gain.gain.exponentialRampToValueAtTime(volume, start + 0.01);
    gain.gain.exponentialRampToValueAtTime(0.0001, start + duration);

    oscillator.connect(gain).connect(master);
    oscillator.start(start);
    oscillator.stop(start + duration);
}

// Random noise through a filter that slides between two frequencies: splashes and rumbles
function noise(ctx, start, { duration, filter = "lowpass", from, to, volume }) {
    const buffer = ctx.createBuffer(1, Math.ceil(ctx.sampleRate * duration), ctx.sampleRate);
    const samples = buffer.getChannelData(0);
    for (let i = 0; i < samples.length; i++)
        samples[i] = Math.random() * 2 - 1;

    const source = ctx.createBufferSource();
    source.buffer = buffer;

    const biquad = ctx.createBiquadFilter();
    biquad.type = filter;
    biquad.frequency.setValueAtTime(from, start);
    biquad.frequency.exponentialRampToValueAtTime(to, start + duration);

    const gain = ctx.createGain();
    gain.gain.setValueAtTime(volume, start);
    gain.gain.exponentialRampToValueAtTime(0.0001, start + duration);

    source.connect(biquad).connect(gain).connect(master);
    source.start(start);
    source.stop(start + duration);
}

// Names match the GameSound enum in C#
const sounds = {
    place: (ctx, t) => tone(ctx, t, { duration: 0.12, type: "triangle", from: 220, to: 110, volume: 0.5 }),

    invalid: (ctx, t) => tone(ctx, t, { duration: 0.15, type: "square", from: 150, to: 120, volume: 0.12 }),

    miss: (ctx, t) => noise(ctx, t, { duration: 0.5, filter: "bandpass", from: 1800, to: 400, volume: 0.6 }),

    hit: (ctx, t) => {
        tone(ctx, t, { duration: 0.35, from: 120, to: 40, volume: 0.9 });
        noise(ctx, t, { duration: 0.6, from: 3000, to: 200, volume: 0.8 });
    },

    sunk: (ctx, t) => {
        tone(ctx, t, { duration: 0.8, from: 90, to: 30, volume: 1 });
        noise(ctx, t, { duration: 1.2, from: 2500, to: 80, volume: 1 });
        noise(ctx, t + 0.15, { duration: 0.9, from: 1500, to: 60, volume: 0.7 });
    },

    // C major going up
    victory: (ctx, t) => [523.25, 659.25, 783.99, 1046.5].forEach((frequency, i) =>
        tone(ctx, t + i * 0.12, { duration: 0.4, type: "triangle", from: frequency, volume: 0.4 })),

    // Going down, slower
    defeat: (ctx, t) => [392.0, 349.23, 311.13, 261.63].forEach((frequency, i) =>
        tone(ctx, t + i * 0.18, { duration: 0.5, type: "triangle", from: frequency, volume: 0.35 })),
};
