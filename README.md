# SmartTales - Educational Learning Platform

A cross-platform educational app built with .NET MAUI & Blazor. Features AI-powered story generation, assignment management, grading system, OCR & text-to-speech. Supports multiple roles: Kids, Parents, Teachers & Admins. Integrates Azure OpenAI, Cognitive Services & Twilio SMS.

---

## Features

### For Students (Kids)
- Interactive AI-powered story generation
- View and submit assignments
- Track grades and progress
- Text-to-speech for accessibility
- Personal dashboard with metrics

### For Parents
- Monitor child's academic progress
- View assignments and grades
- Monthly progress tracking
- Performance analytics dashboard

### For Teachers
- Create and publish assignments
- Grade student submissions
- Provide feedback
- Track class performance

### For Administrators
- User management (CRUD operations)
- Bulk user operations
- Parent-child relationship management
- Password reset processing via SMS
- Comprehensive audit logging
- Data export (JSON/CSV)

### AI & Accessibility Features
- **AI Story Generation** - Azure OpenAI GPT-4o integration
- **Text-to-Speech** - Azure Cognitive Services
- **OCR** - Tesseract image-to-text extraction
- **Image-to-Speech** - Complete accessibility pipeline

---

## Tech Stack

### Frontend
- **.NET MAUI** - Cross-platform mobile/desktop framework
- **Blazor Components** - Web UI framework
- **Bootstrap 5.3** - CSS framework

### Backend
- **ASP.NET Core 8.0** - Web API
- **FastAPI** - Python microservice for TTS
- **SQLite** - Local database

### Integrations
- **Azure OpenAI** - GPT-4o for story generation
- **Azure Cognitive Services** - Speech synthesis
- **Tesseract** - OCR engine
- **Twilio** - SMS notifications

### Target Platforms
- Android
- iOS
- Windows 10+
- macOS

---

## Project Structure

```
src/
├── SmartTalesAndTaskApp/
│   ├── SmartTales/                    # Main MAUI Application
│   │   ├── Components/
│   │   │   ├── Pages/                 # 50+ Razor pages
│   │   │   ├── Layout/                # App layouts
│   │   │   └── Shared/                # Reusable components
│   │   ├── Service/                   # Business logic services
│   │   ├── Repository/                # Data access layer
│   │   ├── Model/                     # Data models
│   │   ├── Data/                      # Database context
│   │   └── wwwroot/                   # Static assets
│   └── SmartTalesAndTaskApp.sln
│
└── SmartTalesAndTaskApp-master/
    ├── SmartTalesAndTaskWebAPI-master/
    │   └── SmartTalesAndTaskWebAPI/   # REST API
    └── SmartTalesAndTaskFastAPI-main/
        └── app/                       # Python TTS service
```

---

## Prerequisites

- .NET 8.0 SDK
- Visual Studio 2022 with MAUI workload
- Python 3.10+ (for FastAPI service)
- Android SDK (for Android deployment)
- Xcode (for iOS/macOS deployment)

---

## Installation

### 1. Clone the repository
```bash
git clone https://github.com/yourusername/smart-tales-app.git
cd smart-tales-app
```

### 2. Setup MAUI Application
```bash
cd src/SmartTalesAndTaskApp
dotnet restore
dotnet build
```

### 3. Setup Web API
```bash
cd src/SmartTalesAndTaskApp-master/SmartTalesAndTaskWebAPI-master/SmartTalesAndTaskWebAPI
dotnet restore
dotnet run
```

### 4. Setup FastAPI Service (Optional)
```bash
cd src/SmartTalesAndTaskApp-master/SmartTalesAndTaskFastAPI-main
python -m venv venv
venv\Scripts\activate  # Windows
pip install -r requirements.txt
uvicorn app.main:app --host 0.0.0.0 --port 8000
```

---

## Configuration

### appsettings.json

Add the following configuration to `src/SmartTalesAndTaskApp/SmartTales/appsettings.json`:

```json
{
  "AzureOpenAI": {
    "Endpoint": "https://your-resource.openai.azure.com/",
    "ApiKey": "your-api-key",
    "DeploymentName": "gpt-4o",
    "ApiVersion": "2024-12-01-preview",
    "MaxTokens": 1000,
    "Temperature": 0.7
  },
  "SpeechService": {
    "Key": "your-speech-key",
    "Region": "your-region",
    "UseFallback": false
  },
  "Twilio": {
    "AccountSid": "your-account-sid",
    "AuthToken": "your-auth-token",
    "FromPhoneNumber": "+1234567890"
  }
}
```

### Environment Variables (Recommended for Production)

```bash
AZURE_OPENAI_ENDPOINT=https://your-resource.openai.azure.com/
AZURE_OPENAI_KEY=your-api-key
SPEECH_SERVICE_KEY=your-speech-key
SPEECH_SERVICE_REGION=your-region
TWILIO_ACCOUNT_SID=your-account-sid
TWILIO_AUTH_TOKEN=your-auth-token
```

---

## Running the Application

### MAUI App (Windows)
```bash
dotnet build -t:Run -f net8.0-windows10.0.19041.0
```

### MAUI App (Android)
```bash
dotnet build -t:Run -f net8.0-android
```

### Web API
```bash
cd SmartTalesAndTaskWebAPI
dotnet run
# API available at http://localhost:5000
# Swagger UI at http://localhost:5000/swagger
```

---

## Default Credentials

> **Warning:** Change these credentials in production!

### Admin Account
- **Username:** admin
- **Password:** admin123

### Sample Users
- Created automatically on first run
- Check `LocalDbService.cs` for sample data

---

## API Endpoints

### OCR Controller
- `POST /api/ocr/extract-text` - Extract text from image
- `POST /api/ocr/image-to-speech` - Convert image to speech

### TTS Controller
- `POST /api/tts/synthesize` - Text-to-speech conversion

---

## Database

The application uses SQLite for local data storage.

### Tables
- `User` - User accounts with role-based attributes
- `Assignment` - Assignment records
- `Grade` - Student grades
- `ParentChild` - Parent-child relationships
- `AdminUser` - Administrator accounts
- `AdminAuditLog` - Audit trail
- `PasswordResetRequest` - Password reset tracking

### Location
```
{LocalApplicationData}/smarttales_local_db.db3
```

---

## Key Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| Microsoft.Maui.Controls | 8.0.72 | Cross-platform UI |
| Microsoft.CognitiveServices.Speech | 1.43.0 | Text-to-Speech |
| Tesseract | 5.2.0 | OCR Engine |
| Twilio | 7.2.2 | SMS Service |
| BCrypt.Net-Next | 4.0.3 | Password Hashing |
| sqlite-net-pcl | 1.8.116 | SQLite Database |

---

## Security Considerations

- All passwords are hashed using BCrypt
- Role-based access control implemented
- Audit logging for admin actions
- Move sensitive credentials to environment variables
- Enable HTTPS in production
- Restrict CORS origins in production

---

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit changes (`git commit -m 'Add amazing feature'`)
4. Push to branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

---

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## Acknowledgments

- Azure OpenAI for AI story generation
- Microsoft Cognitive Services for speech synthesis
- Tesseract OCR for image text extraction
- Twilio for SMS services

---

## Support

For issues and feature requests, please [open an issue](https://github.com/yourusername/smart-tales-app/issues).

**Developed by Arslandevs | Powered by CodingZoo**
