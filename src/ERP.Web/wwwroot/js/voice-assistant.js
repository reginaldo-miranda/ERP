window.voiceAssistant = {
    recognition: null,
    dotNetRef: null,
    isListening: false,
    handsFreeMode: true,
    userExplicitlyStopped: false,
    resumeTimeoutId: null,

    isSupported: function () {
        return !!(window.SpeechRecognition || window.webkitSpeechRecognition);
    },

    setHandsFree: function (enabled) {
        this.handsFreeMode = !!enabled;
    },

    init: function (dotNetRef, handsFree) {
        this.dotNetRef = dotNetRef;
        this.handsFreeMode = handsFree !== undefined ? !!handsFree : true;
        const SpeechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition;
        if (!SpeechRecognition) {
            console.warn("Reconhecimento de fala (Web Speech API) não suportado neste navegador.");
            if (this.dotNetRef) {
                this.dotNetRef.invokeMethodAsync('OnSpeechError', 'Este navegador não suporta reconhecimento de voz nativo. Por favor, utilize o Google Chrome ou Microsoft Edge.');
            }
            return false;
        }

        try {
            this.recognition = new SpeechRecognition();
            this.recognition.lang = 'pt-BR';
            this.recognition.continuous = false;
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
                console.warn("Speech recognition error:", event.error);
                this.isListening = false;

                // Em modo mãos livres, se for 'no-speech' (pausa do usuário), retoma a escuta suavemente
                if (event.error === 'no-speech') {
                    if (this.handsFreeMode && !this.userExplicitlyStopped && (!window.speechSynthesis || !window.speechSynthesis.speaking)) {
                        this.scheduleAutoResume(600);
                        return;
                    }
                }

                let mensagem = event.error;
                if (event.error === 'not-allowed') {
                    mensagem = 'Permissão do microfone negada. Verifique as permissões do navegador e do sistema operacional.';
                } else if (event.error === 'no-speech') {
                    mensagem = 'Nenhum som detectado. Pode falar novamente.';
                } else if (event.error === 'network') {
                    mensagem = 'Erro de conexão com o serviço de reconhecimento de voz.';
                }

                if (this.dotNetRef) {
                    this.dotNetRef.invokeMethodAsync('OnSpeechError', mensagem);
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

    startListening: async function () {
        if (!this.recognition) return;
        this.userExplicitlyStopped = false;
        if (this.resumeTimeoutId) {
            clearTimeout(this.resumeTimeoutId);
            this.resumeTimeoutId = null;
        }

        // Se o assistente estiver falando no momento, não inicia para não capturar o próprio áudio
        if (window.speechSynthesis && window.speechSynthesis.speaking) {
            return;
        }

        if (this.isListening) return;

        try {
            if (navigator.mediaDevices && navigator.mediaDevices.getUserMedia) {
                const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
                stream.getTracks().forEach(track => track.stop());
            }
        } catch (err) {
            console.error("Permissão de microfone negada no getUserMedia:", err);
            if (this.dotNetRef) {
                this.dotNetRef.invokeMethodAsync('OnSpeechError', 'Microfone bloqueado! Autorize o microfone no navegador.');
            }
            return;
        }

        try {
            this.recognition.start();
        } catch (e) {
            console.warn("Recognition já iniciado ou erro ao iniciar:", e);
        }
    },

    stopListening: function (userInitiated = true) {
        if (userInitiated) {
            this.userExplicitlyStopped = true;
            if (this.resumeTimeoutId) {
                clearTimeout(this.resumeTimeoutId);
                this.resumeTimeoutId = null;
            }
        }

        if (!this.recognition) return;
        if (!this.isListening) return;

        try {
            this.recognition.stop();
        } catch (e) {
            console.warn("Erro ao parar recognition:", e);
        }
    },

    scheduleAutoResume: function (delayMs = 500) {
        if (!this.handsFreeMode || this.userExplicitlyStopped) return;
        if (this.resumeTimeoutId) {
            clearTimeout(this.resumeTimeoutId);
        }
        this.resumeTimeoutId = setTimeout(() => {
            if (this.handsFreeMode && !this.userExplicitlyStopped && !this.isListening) {
                this.startListening();
            }
        }, delayMs);
    },

    speak: function (text) {
        if (!window.speechSynthesis) return;

        // Pausa temporariamente a escuta para não capturar a própria fala do assistente
        this.stopListening(false);

        try {
            window.speechSynthesis.cancel();
            const utterance = new SpeechSynthesisUtterance(text);
            utterance.lang = 'pt-BR';
            utterance.rate = 1.05;
            utterance.pitch = 1.0;

            utterance.onend = () => {
                // Assim que o assistente termina de falar: reabre o microfone automaticamente!
                if (this.handsFreeMode && !this.userExplicitlyStopped) {
                    this.scheduleAutoResume(450);
                }
            };

            utterance.onerror = (e) => {
                console.warn("Erro na síntese de voz:", e);
                if (this.handsFreeMode && !this.userExplicitlyStopped) {
                    this.scheduleAutoResume(400);
                }
            };

            window.speechSynthesis.speak(utterance);
        } catch (e) {
            console.error("Erro na síntese de voz:", e);
            if (this.handsFreeMode && !this.userExplicitlyStopped) {
                this.scheduleAutoResume(400);
            }
        }
    }
};
