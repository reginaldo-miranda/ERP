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
                console.error("Speech recognition error:", event.error);
                this.isListening = false;
                let mensagem = event.error;
                if (event.error === 'not-allowed') {
                    mensagem = 'Permissão do microfone negada. Verifique as permissões do navegador e do macOS (Ajustes do Sistema > Privacidade e Segurança > Microfone).';
                } else if (event.error === 'no-speech') {
                    mensagem = 'Nenhum som detectado. Tente falar novamente.';
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
        if (!this.recognition) {
            if (this.dotNetRef) {
                this.dotNetRef.invokeMethodAsync('OnSpeechError', 'Reconhecimento de fala não disponível. Utilize o Google Chrome.');
            }
            return;
        }

        if (this.isListening) return;

        // Solicita permissão explícita de microfone ao navegador/sistema operacional
        try {
            if (navigator.mediaDevices && navigator.mediaDevices.getUserMedia) {
                const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
                stream.getTracks().forEach(track => track.stop());
            }
        } catch (err) {
            console.error("Permissão de microfone negada no getUserMedia:", err);
            if (this.dotNetRef) {
                this.dotNetRef.invokeMethodAsync('OnSpeechError', 'Microfone bloqueado! Autorize o microfone no navegador ou em Ajustes do Sistema do macOS.');
            }
            return;
        }

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
