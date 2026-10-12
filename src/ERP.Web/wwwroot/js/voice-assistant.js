window.voiceAssistant = {
    recognition: null,
    dotNetRef: null,
    isListening: false,

    isSupported: function () {
        return !!(window.SpeechRecognition || window.webkitSpeechRecognition);
    },

    init: function (dotNetRef) {
        this.dotNetRef = dotNetRef;
        const SpeechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition;
        if (!SpeechRecognition) {
            console.warn("Reconhecimento de fala (Web Speech API) não suportado neste navegador.");
            return false;
        }

        try {
            this.recognition = new SpeechRecognition();
            this.recognition.lang = 'pt-BR';
            this.recognition.continuous = false; // Captura frase completa
            this.recognition.interimResults = true;

            this.recognition.onstart = () => {
                this.isListening = true;
                if (this.dotNetRef) {
                    this.dotNetRef.invokeMethodAsync('OnSpeechStateChanged', true);
                }
            };

            this.recognition.onend = () => {
                this.isListening = false;
                if (this.dotNetRef) {
                    this.dotNetRef.invokeMethodAsync('OnSpeechStateChanged', false);
                }
            };

            this.recognition.onerror = (event) => {
                console.error("Speech recognition error:", event.error);
                this.isListening = false;
                if (this.dotNetRef) {
                    this.dotNetRef.invokeMethodAsync('OnSpeechError', event.error);
                }
            };

            this.recognition.onresult = (event) => {
                let interimTranscript = '';
                let finalTranscript = '';

                for (let i = event.resultIndex; i < event.results.length; ++i) {
                    if (event.results[i].isFinal) {
                        finalTranscript += event.results[i][0].transcript;
                    } else {
                        interimTranscript += event.results[i][0].transcript;
                    }
                }

                if (finalTranscript && this.dotNetRef) {
                    this.dotNetRef.invokeMethodAsync('OnFinalTranscript', finalTranscript.trim());
                } else if (interimTranscript && this.dotNetRef) {
                    this.dotNetRef.invokeMethodAsync('OnInterimTranscript', interimTranscript.trim());
                }
            };

            return true;
        } catch (e) {
            console.error("Erro ao inicializar reconhecimento de fala:", e);
            return false;
        }
    },

    startListening: function () {
        if (!this.recognition) return;
        if (this.isListening) return;

        try {
            this.recognition.start();
        } catch (e) {
            console.warn("Recognition já iniciado ou erro ao iniciar:", e);
        }
    },

    stopListening: function () {
        if (!this.recognition) return;
        if (!this.isListening) return;

        try {
            this.recognition.stop();
        } catch (e) {
            console.warn("Erro ao parar recognition:", e);
        }
    },

    speak: function (text) {
        if (!window.speechSynthesis) return;
        try {
            window.speechSynthesis.cancel();
            const utterance = new SpeechSynthesisUtterance(text);
            utterance.lang = 'pt-BR';
            utterance.rate = 1.05;
            utterance.pitch = 1.0;
            window.speechSynthesis.speak(utterance);
        } catch (e) {
            console.error("Erro na síntese de voz:", e);
        }
    }
};
