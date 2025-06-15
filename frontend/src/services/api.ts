import axios from 'axios';
import {
  PromptTemplate,
  Category,
  Tag,
  PromptExecution,
  CreatePromptTemplateRequest,
  UpdatePromptTemplateRequest,
  ExecutePromptRequest,
  PromptSuggestionRequest,
  PromptSuggestionResponse,
} from '@/types';

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5055/api';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Prompt Templates
export const promptTemplateAPI = {
  getAll: async (): Promise<PromptTemplate[]> => {
    const response = await api.get('/prompts');
    return response.data;
  },

  getById: async (id: string): Promise<PromptTemplate> => {
    const response = await api.get(`/prompts/${id}`);
    return response.data;
  },

  create: async (data: CreatePromptTemplateRequest): Promise<PromptTemplate> => {
    const response = await api.post('/prompts', data);
    return response.data;
  },

  update: async (data: UpdatePromptTemplateRequest): Promise<PromptTemplate> => {
    const response = await api.put(`/prompts/${data.id}`, data);
    return response.data;
  },

  delete: async (id: string): Promise<void> => {
    await api.delete(`/prompts/${id}`);
  },

  execute: async (data: ExecutePromptRequest): Promise<PromptExecution> => {
    const response = await api.post('/prompts/execute', data);
    return response.data;
  },

  exportYaml: async (id: string): Promise<string> => {
    const response = await api.get(`/prompts/${id}/export-yaml`);
    return response.data;
  },
  importYaml: async (yamlContent: string): Promise<PromptTemplate> => {
    const response = await api.post('/prompts/import-yaml', { yamlContent });
    return response.data;
  },  
  getSuggestions: async (data: PromptSuggestionRequest): Promise<PromptSuggestionResponse> => {
    const response = await api.post('/prompts/suggest', data);
    return response.data;
  },
};

// Categories
export const categoryAPI = {
  getAll: async (): Promise<Category[]> => {
    const response = await api.get('/categories');
    return response.data;
  },

  create: async (data: Omit<Category, 'id' | 'createdAt'>): Promise<Category> => {
    const response = await api.post('/categories', data);
    return response.data;
  },

  update: async (id: string, data: Partial<Category>): Promise<Category> => {
    const response = await api.put(`/categories/${id}`, data);
    return response.data;
  },

  delete: async (id: string): Promise<void> => {
    await api.delete(`/categories/${id}`);
  },
};

// Tags
export const tagAPI = {
  getAll: async (): Promise<Tag[]> => {
    const response = await api.get('/tags');
    return response.data;
  },

  create: async (data: Omit<Tag, 'id'>): Promise<Tag> => {
    const response = await api.post('/tags', data);
    return response.data;
  },

  update: async (id: string, data: Partial<Tag>): Promise<Tag> => {
    const response = await api.put(`/tags/${id}`, data);
    return response.data;
  },

  delete: async (id: string): Promise<void> => {
    await api.delete(`/tags/${id}`);
  },
};

export default api;
