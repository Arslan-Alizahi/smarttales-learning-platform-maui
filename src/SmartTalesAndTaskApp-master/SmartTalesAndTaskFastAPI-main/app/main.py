from fastapi import FastAPI
from app.api.routes_tts import router as tts_router

app = FastAPI(
    title="TTS Microservice API",
    description="Microservice for converting text to speech using FastAPI and TTS",
    version="1.0.0"
)

app.include_router(tts_router, prefix="/api/tts", tags=["TTS"])