from TTS.api import TTS
from tempfile import NamedTemporaryFile

# Load once when the service starts
tts_model = TTS(model_name="tts_models/en/ljspeech/neural_hmm", progress_bar=False, gpu=False)

def generate_tts(text: str) -> str:
    with NamedTemporaryFile(delete=False, suffix=".wav") as tmp:
        file_path = tmp.name
    tts_model.tts_to_file(text=text, file_path=file_path)
    return file_path
