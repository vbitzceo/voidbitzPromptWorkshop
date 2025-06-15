# AI Suggestion System API Reference

## Overview
The AI-powered suggestion system automatically analyzes prompt content and suggests relevant categories and tags using LLM providers or keyword-based fallback.

## API Endpoint

### POST `/api/prompts/suggest`

Analyzes prompt content and returns suggested categorization.

**Request Body:**
```json
{
  "name": "React Component Generator",
  "content": "Generate a React functional component with TypeScript"
}
```

**Response:**
```json
{
  "suggestedCategoryId": "cat-web",
  "suggestedTagIds": ["tag-few-shot", "tag-zero-shot"],
  "reasoning": "Based on analysis, this appears to be a web development prompt..."
}
```

## Configuration

Configure AI providers in `appsettings.json`:

```json
{
  "AI": {
    "Provider": "openai"  // or "azure", "ollama"
  },
  "OpenAI": {
    "ApiKey": "your-api-key",
    "Model": "gpt-3.5-turbo"
  }
}
```

**Fallback Behavior**: Works without API keys using intelligent keyword matching.

## Features

- **Real-time Suggestions**: Get suggestions as you type in the prompt editor
- **Context-Aware**: Analyzes both prompt name and content
- **Technique Recognition**: Identifies prompting techniques (Chain of Thought, Few-Shot, etc.)
- **Robust Fallback**: Graceful degradation when AI services are unavailable
- **Type Safety**: Full TypeScript support throughout the stack

## Usage

The suggestion system is automatically integrated into the PromptEditor component. Users receive real-time suggestions while creating or editing prompts, improving productivity and ensuring consistent categorization across the prompt library.
