from fastapi import APIRouter, HTTPException
from fastapi.responses import FileResponse
from app.models.tts_input import TTSRequest
from app.services.tts_service import generate_tts

router = APIRouter()

@router.post("/synthesize")
async def synthesize_text(request: TTSRequest):
    try:
        file_path = generate_tts(request.text)
        return FileResponse(file_path, media_type="audio/wav", filename="tts_output.wav")
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Error generating TTS: {str(e)}")
